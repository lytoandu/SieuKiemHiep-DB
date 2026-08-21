using System;
using MySQLDriverCS;
using Server.Tools;
using System.Threading;
using System.Collections.Concurrent;

namespace GameDBServer.DB
{
    /// <summary>
    /// Quản lý kết nối DB
    /// </summary>
    public class DBConnections
    {
        /// <summary>
        /// Tên DB
        /// </summary>
        public static string dbNames = "";

        /// <summary>
        /// Đối tượng Semaphore
        /// </summary>
        private Semaphore SemaphoreClients = null;

        /// <summary>
        /// Danh sách kết nối đang chờ
        /// </summary>
        private ConcurrentQueue<MySQLConnection> DBConns = new ConcurrentQueue<MySQLConnection>();

        /// <summary>
        /// MUTEX dùng trong LOCK
        /// </summary>
        private object Mutex = new object();

        /// <summary>
        /// Chuỗi kết nói
        /// </summary>
        private string ConnectionString;

        /// <summary>
        /// Tổng số kết nối
        /// </summary>
        private int CurrentCount;

        /// <summary>
        /// Kết nối tối đa
        /// </summary>
        private int MaxCount;

        /// <summary>
        /// Thời gian chờ tối đa (ms) khi lấy 1 connection từ pool, tránh treo vô thời hạn
        /// khi pool cạn kiệt (VD: nhiều query chậm/deadlock giữ hết connection cùng lúc).
        /// </summary>
        private const int PopConnectionTimeoutMs = 15000;

        /// <summary>
        /// Timeout (giây) cho câu lệnh health-check "select 1" khi Pop connection, tránh chính
        /// câu health-check bị treo nếu connection/DB đang có vấn đề.
        /// </summary>
        private const int HealthCheckCommandTimeoutSec = 5;

        /// <summary>
        /// Tạo kết nối mới tới Database
        /// </summary>
        /// <param name="connStr"></param>
        public void BuidConnections(MySQLConnectionString connStr, int maxCount)
        {
            //lock (this.Mutex)
            {
                ConnectionString = connStr.AsString;
                // ConnectionString += ConnectionString + ";Character Set=UTF-8";
                /// Đọc số connection từ AppConfig.xml (maxConns), nhưng đặt SÀN tối thiểu 100 để tránh
                /// vô tình thu nhỏ pool nếu config để giá trị quá thấp (VD maxConns=10 cũ). Nhờ vậy sau
                /// này chỉ cần sửa maxConns trong AppConfig.xml là đổi được pool, không cần build lại.
                MaxCount = Math.Max(maxCount, 100);
                SemaphoreClients = new Semaphore(0, MaxCount);

                for (int i = 0; i < MaxCount; i++)
                {
                    MySQLConnection dbConn = CreateAConnection();
                    if (null == dbConn)
                    {
                        throw new Exception(string.Format("Connect to MySQL faild"));
                    }
                }
            }
        }

        /// <summary>
        /// Tạo kết nối
        /// </summary>
        /// <returns></returns>
        private MySQLConnection CreateAConnection()
        {
            try
            {
                MySQLConnection dbConn = null;
                dbConn = new MySQLConnection(ConnectionString);
                dbConn.Open();
                /*
                if (!string.IsNullOrEmpty(dbNames))
                {
                    using (MySQLCommand cmd = new MySQLCommand(string.Format("SET names '{0}'", dbNames), dbConn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                */

                //lock (this.Mutex)
                {
                    DBConns.Enqueue(dbConn);
                    CurrentCount++;
                    this.SemaphoreClients.Release();
                }

                return dbConn;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, string.Format("Create database connection exception: \r\n{0}", ex.ToString()));
            }

            return null;
        }

        /// <summary>
        /// Duy trì kết nối
        /// </summary>
        /// <returns></returns>
        public bool SupplyConnections()
        {
            bool result = false;
            //lock (this.Mutex)
            {
                if (CurrentCount < MaxCount)
                {
                    CreateAConnection();
                }
            }

            return result;
        }

        /// <summary>
        /// Trả về tổng số kết nối đến Database
        /// </summary>
        /// <returns></returns>
        public int GetDBConnsCount()
        {
            //lock (this.Mutex)
            {
                return this.DBConns.Count;
            }
        }

        /// <summary>
        /// Lấy kết nối đến Database trong hàng đợi để thực thi
        /// </summary>
        /// <returns></returns>
        public MySQLConnection PopDBConnection()
        {
            MySQLConnection conn = null;
            bool lost = false;

            do
            {
                string cmdText = @"select 1";
                lost = true;

                /// Chờ có giới hạn thời gian thay vì vô thời hạn: nếu pool cạn kiệt (VD nhiều query
                /// chậm/deadlock giữ hết connection cùng lúc), tránh việc thread gọi bị treo mãi mãi
                /// kéo theo nghẽn dây chuyền toàn server (bao gồm cả tầng nhận/gửi socket).
                if (!SemaphoreClients.WaitOne(PopConnectionTimeoutMs))
                {
                    LogManager.WriteLog(LogTypes.Exception,
                        string.Format("PopDBConnection timeout sau {0}ms: connection pool đang cạn kiệt (CurrentCount={1}, MaxCount={2})",
                            PopConnectionTimeoutMs, CurrentCount, MaxCount));
                    return null;
                }

                /// Toác
                if (!this.DBConns.TryDequeue(out conn))
                {
                    return null;
                }

                try
                {
                    using (MySQLCommand cmd = new MySQLCommand(cmdText, conn))
                    {
                        cmd.CommandTimeout = HealthCheckCommandTimeoutSec;
                        try
                        {
                            cmd.ExecuteNonQuery();
                            lost = false;
                        }
                        catch (System.Exception ex)
                        {
                            LogManager.WriteLog(LogTypes.Exception, string.Format("Exception occurred when executing Database Query: {0}\r\n{1}", cmdText, ex.ToString()));
                        }
                    }
                }
                catch(Exception ex)
                {
                    LogManager.WriteException(ex.ToString());
                }
                finally
                {
                    if (lost)
                    {
                        //lock (this.Mutex)
                        {
                            CurrentCount--;
                        }
                    }
                }
            }
            while (lost);

            return conn;
        }

        /// <summary>
        /// Thực thi kết nối
        /// </summary>
        /// <param name="conn"></param>
        public void PushDBConnection(MySQLConnection conn)
        {
            if (null != conn)
            {
                //lock (this.Mutex)
                {
                    this.DBConns.Enqueue(conn);
                }

                SemaphoreClients.Release();
            }
        }
    }
}
