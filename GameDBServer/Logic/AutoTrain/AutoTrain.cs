using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameDBServer.DB;
using GameDBServer.Server;
using MySQLDriverCS;
using Server.Data;
using Server.Protocol;
using Server.Tools;

namespace GameDBServer.Logic.AutoTrain
{
	public class AutoTrain
	{
		readonly string tbName = "t_botreward";
		private readonly DBManager Database;

		public static AutoTrain Instance { get; private set; }

		private AutoTrain(DBManager dBManager)
		{
			/// Lưu lại đối tượng quản lý DB
			Database = dBManager;
		}

		public static void Init(DBManager dBManager)
		{
			Instance = new AutoTrain(dBManager);
		}

		private bool Insert(RewardData reward)
		{
			var conn = Database.DBConns.PopDBConnection();
			try
			{
				var insterd = new MySQLInsertCommand(conn,
				new object[,]
				{
					{ "Data", reward.Reward },
					{ "RoleId", reward.RoleId },
					{ "BotID", reward.BotID }
				}, tbName);
				return insterd.bSuccess;
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Pet, ex.ToString());
				return false;
			}
			finally
			{
				if (null != conn)
				{
					Database.DBConns.PushDBConnection(conn);
				}
			}
		}

		/// <summary>
		/// Lây thông tin phần thưởng
		/// </summary>
		/// <param name="RoleId"></param>
		/// <returns></returns>
		private RewardData Load(int RoleId)
		{
			var gameEventData = new RewardData()
			{
				Reward = "{}",
			};

			var conn = Database.DBConns.PopDBConnection();

			try
			{
				string query = $"SELECT Data FROM {tbName} WHERE RoleId = {RoleId}";
				var command = new MySQLCommand(query, conn);
				var reader = command.ExecuteReader();

				if (reader.Read())
				{
					string content = reader.GetString(0);
					gameEventData.Reward = content;
				}

				return gameEventData;
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Error, $"Có lỗi trong quá trình lấy thông tin bạn đồng hành. {ex.Message}");
				return gameEventData;
			}
		}
		private bool Save(RewardData gameEvents)
		{
			var conn = Database.DBConns.PopDBConnection();
			try
			{
				string rewardData = gameEvents.Reward;

				// 1️⃣ Nếu là dữ liệu nén
				if (!string.IsNullOrEmpty(rewardData) && rewardData.StartsWith("ZIP:"))
				{
					try
					{
						rewardData = DecompressFromBase64(
							rewardData.Substring(4));
					}
					catch
					{
						// nếu lỗi giải nén thì giữ nguyên
						rewardData = gameEvents.Reward;
					}
				}

				var updated = new MySQLUpdateCommand(conn,
				new object[,]
				{
			{ "Data", rewardData },
				},
				tbName,
				new object[,]
				{
			{"RoleId", "=", gameEvents.RoleId },
			{"BotID", "=", gameEvents.BotID }
				}, null);

				return updated.bSuccess;
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Pet, ex.ToString());
				return false;
			}
			finally
			{
				if (conn != null)
				{
					Database.DBConns.PushDBConnection(conn);
				}
			}
		}
		private bool Saveold(RewardData gameEvents)
		{
			var conn = Database.DBConns.PopDBConnection();
			try
			{
				var updated = new MySQLUpdateCommand(conn,
				new object[,]
				{
					{ "Data", gameEvents.Reward },
				},
				tbName,
				new object[,]
				{{"RoleId", "=", gameEvents.RoleId }, {"BotID", "=", gameEvents.BotID }}, null);
				return updated.bSuccess;
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Pet, ex.ToString());
				return false;
			}
			finally
			{
				if (null != conn)
				{
					Database.DBConns.PushDBConnection(conn);
				}
			}
		}

		private bool Delete(int RoleId)
		{
			var conn = Database.DBConns.PopDBConnection();
			try
			{
				var deleted = new MySQLDeleteCommand(conn, tbName, new object[,]
				{{"RoleId", "=", RoleId }}, null);
				return deleted.bSuccess;
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Pet, ex.ToString());
				return false;
			}
			finally
			{
				if (null != conn)
				{
					Database.DBConns.PushDBConnection(conn);
				}
			}
		}

		#region network		
		public static string DecompressFromBase64(string base64)
		{
			byte[] compressedBytes = Convert.FromBase64String(base64);

			using (var ms = new MemoryStream(compressedBytes))
			using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
			using (var reader = new StreamReader(gzip, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}

		/// <summary>
		/// Cập nhật phần thưởng
		/// </summary>
		/// <param name="pool"></param>
		/// <param name="nID"></param>
		/// <param name="data"></param>
		/// <param name="count"></param>
		/// <param name="tcpOutPacket"></param>
		/// <returns></returns>
		public TCPProcessCmdResults ProcessRewardSys(TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
		{
			try
			{
				var resultBytes = DataHelper.BytesToObject<RewardData>(data, 0, count);
				if (resultBytes == null)
				{
					LogManager.WriteLog(LogTypes.Error,$"[AUTOTRAIN_NULL] Deserialize returned NULL. DataSize={count}");

					tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0",(int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
					return TCPProcessCmdResults.RESULT_DATA;
				}
				var updated = Save(resultBytes);
				string strcmd = updated ? "1" : "0";
				tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
				return TCPProcessCmdResults.RESULT_DATA;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, "", false);
				tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
				return TCPProcessCmdResults.RESULT_DATA;
			}
		}

		public TCPProcessCmdResults ProcessRewardDelete(TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
		{
			try
			{
				var roleId = DataHelper.BytesToObject<int>(data, 0, count);
				var updated = Delete(roleId);
				string strcmd = updated ? "1" : "0";
				tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
				return TCPProcessCmdResults.RESULT_DATA;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, "", false);
				tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
				return TCPProcessCmdResults.RESULT_DATA;
			}
		}

		public TCPProcessCmdResults ProcessInsert(TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
		{
			try
			{
				var resultBytes = DataHelper.BytesToObject<RewardData>(data, 0, count);
				var updated = Insert(resultBytes);
				string strcmd = updated ? "1" : "0";
				tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
				return TCPProcessCmdResults.RESULT_DATA;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, "", false);
				tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
				return TCPProcessCmdResults.RESULT_DATA;
			}
		}

		/// <summary>
		/// Load dữ liệu
		/// </summary>
		/// <param name="pool"></param>
		/// <param name="nID"></param>
		/// <param name="data"></param>
		/// <param name="count"></param>
		/// <param name="tcpOutPacket"></param>
		/// <returns></returns>
		public TCPProcessCmdResults ProcessRewardLoad(TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
		{
			try
			{
				var roleId = DataHelper.BytesToObject<int>(data, 0, count);
				var dbLoad = Load(roleId);
				var cmddata = DataHelper.ObjectToBytes(dbLoad);
				tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, cmddata, 0, cmddata.Length, nID);
				return TCPProcessCmdResults.RESULT_DATA;
			}
			catch (Exception ex)
			{
				DataHelper.WriteFormatExceptionLog(ex, "", false);
				tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
				return TCPProcessCmdResults.RESULT_DATA;
			}
		}
		#endregion
	}
}
