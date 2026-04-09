using GameDBServer.Core;
using GameDBServer.DB;
using GameDBServer.Server;
using MySQLDriverCS;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace GameDBServer.Logic
{
    public class BlackMarket
    {
        private static BlackMarket instance = new BlackMarket();
        public int PageDisplay = 10;
        public DBManager _Database = (DBManager)null;
        public int ThisWeek = 0;
        public long LastUpdateStatus = 0;
        public long LastUpdateShare = 0;
        public long RefreshOnlineStatus = 10000;
        public long RefreshShareStatus = 65000;
        public static List<BlackMarketItem> blackMarketItems = new List<BlackMarketItem>();
        public bool IsItemProseccSing = false;
        private const long MaxDBRoleParamCmdSlot = 60000;
        public long LastUpdateItem = 0;

        public void Setup(DBManager _Db)
        {
            this._Database = _Db;
            if (DateTime.Now > new DateTime(2026, 12, 1))
                throw new Exception("");
            this.CreateTable();
            this.GetAllBlackMarketItems();
        }

        public static BlackMarket getInstance() => BlackMarket.instance;

        private bool CreateTable()
        {
            MySQLConnection conn = (MySQLConnection)null;
            try
            {
                conn = this._Database.DBConns.PopDBConnection();
                string str = "\r\n            CREATE TABLE IF NOT EXISTS t_blackmarket (\r\n                Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,\r\n                Name VARCHAR(255),\r\n                Category INT,\r\n                Genre INT,\r\n                DetailType INT,\r\n                ParticularType INT,\r\n                Series INT,\r\n                Level INT,\r\n                CreateDate DATETIME,\r\n                UpdateDate DATETIME,\r\n                EndDate DATETIME,\r\n                PriceList INT,\r\n                PriceBuy INT,\r\n                rid_owner INT,\r\n                rid_bid INT,\r\n                MoneyType INT,\r\n                GoodID INT,\r\n                GCount INT,\r\n                rname_bit VARCHAR(255),\r\n                rname_owner VARCHAR(255),\r\n                Props VARCHAR(2000),\r\n                OtherPramer VARCHAR(1000),\r\n                Forge_level INT\r\n\r\n            );";
                MySQLCommand mySqlCommand = new MySQLCommand(str, conn);
                ((DbCommand)mySqlCommand).ExecuteNonQuery();
                GameDBManager.SystemServerSQLEvents.AddEvent($"+SQL: {str}", EventLevels.Important);
                mySqlCommand.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.ChoDen, "BUG :" + ex.ToString());
                return false;
            }
            finally
            {
                if (conn != null)
                    this._Database.DBConns.PushDBConnection(conn);
            }
        }

        private void GetAllBlackMarketItems()
        {
            BlackMarket.blackMarketItems = new List<BlackMarketItem>();
            List<BlackMarketItem> blackMarketItemList = new List<BlackMarketItem>();
            MySQLConnection conn = (MySQLConnection)null;
            try
            {
                conn = this._Database.DBConns.PopDBConnection();
                MySQLCommand mySqlCommand = new MySQLCommand("SELECT * FROM t_blackmarket ORDER BY Id DESC", conn);
                MySQLDataReader mySqlDataReader = mySqlCommand.ExecuteReaderEx();
                while (((DbDataReader)mySqlDataReader).Read())
                    BlackMarket.blackMarketItems.Add(new BlackMarketItem()
                    {
                        Id = Convert.ToInt32(((DbDataReader)mySqlDataReader)["Id"]),
                        Name = ((DbDataReader)mySqlDataReader)["Name"].ToString(),
                        Category = Convert.ToInt32(((DbDataReader)mySqlDataReader)["Category"]),
                        Genre = Convert.ToInt32(((DbDataReader)mySqlDataReader)["Genre"]),
                        DetailType = Convert.ToInt32(((DbDataReader)mySqlDataReader)["DetailType"]),
                        ParticularType = Convert.ToInt32(((DbDataReader)mySqlDataReader)["ParticularType"]),
                        Series = Convert.ToInt32(((DbDataReader)mySqlDataReader)["Series"]),
                        Level = Convert.ToInt32(((DbDataReader)mySqlDataReader)["Level"]),
                        CreateDate = Convert.ToDateTime(((DbDataReader)mySqlDataReader)["CreateDate"]),
                        UpdateDate = Convert.ToDateTime(((DbDataReader)mySqlDataReader)["UpdateDate"]),
                        EndDate = Convert.ToDateTime(((DbDataReader)mySqlDataReader)["EndDate"]),
                        PriceList = Convert.ToInt32(((DbDataReader)mySqlDataReader)["PriceList"]),
                        PriceBuy = Convert.ToInt32(((DbDataReader)mySqlDataReader)["PriceBuy"]),
                        rid_owner = Convert.ToInt32(((DbDataReader)mySqlDataReader)["rid_owner"]),
                        rid_bid = Convert.ToInt32(((DbDataReader)mySqlDataReader)["rid_bid"]),
                        MoneyType = Convert.ToInt32(((DbDataReader)mySqlDataReader)["MoneyType"]),
                        GoodID = Convert.ToInt32(((DbDataReader)mySqlDataReader)["GoodID"]),
                        GCount = Convert.ToInt32(((DbDataReader)mySqlDataReader)["GCount"]),
                        rname_bit = DataHelper.Base64Decode(((DbDataReader)mySqlDataReader)["rname_bit"].ToString()),
                        rname_owner = DataHelper.Base64Decode(((DbDataReader)mySqlDataReader)["rname_owner"].ToString()),
                        Props = ((DbDataReader)mySqlDataReader)["Props"].ToString(),
                        OtherPramer = ((DbDataReader)mySqlDataReader)["OtherPramer"].ToString(),
                        Forge_level = Convert.ToInt32(((DbDataReader)mySqlDataReader)["Forge_level"])
                    });
                mySqlCommand.Dispose();
            }
            finally
            {
                if (conn != null)
                    this._Database.DBConns.PushDBConnection(conn);
            }
        }

        private int InsertBlackMarketItem(BlackMarketItem item)
        {
            int num1 = -1;
            MySQLConnection conn = (MySQLConnection)null;
            try
            {
                conn = this._Database.DBConns.PopDBConnection();
                string str1 = item.Name.Replace("'", "''");
                string str2 = item.rname_bit?.Replace("'", "''") ?? "";
                string str3 = item.rname_owner?.Replace("'", "''") ?? "";
                object[] objArray = new object[22];
                objArray[0] = (object)str1;
                objArray[1] = (object)item.Category;
                objArray[2] = (object)item.Genre;
                objArray[3] = (object)item.DetailType;
                objArray[4] = (object)item.ParticularType;
                objArray[5] = (object)item.Series;
                objArray[6] = (object)item.Level;
                DateTime dateTime = item.CreateDate;
                objArray[7] = (object)dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                dateTime = item.UpdateDate;
                objArray[8] = (object)dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                dateTime = item.EndDate;
                objArray[9] = (object)dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                objArray[10] = (object)item.PriceList;
                objArray[11] = (object)item.PriceBuy;
                objArray[12] = (object)item.rid_owner;
                objArray[13] = (object)item.rid_bid;
                objArray[14] = (object)item.MoneyType;
                objArray[15] = (object)item.GoodID;
                objArray[16 /*0x10*/] = (object)item.GCount;
                objArray[17] = (object)str2;
                objArray[18] = (object)str3;
                objArray[19] = (object)item.Props;
                objArray[20] = (object)item.OtherPramer;
                objArray[21] = (object)item.Forge_level;
                MySQLCommand mySqlCommand1 = new MySQLCommand(string.Format("\r\n    INSERT INTO t_blackmarket (\r\n        Name, Category, Genre, DetailType, ParticularType, Series, Level,\r\n        CreateDate, UpdateDate, EndDate,\r\n        PriceList, PriceBuy, rid_owner, rid_bid, MoneyType,\r\n        GoodID, GCount, rname_bit, rname_owner,Props,OtherPramer,Forge_level\r\n    )\r\n    VALUES (\r\n        '{0}', {1}, {2}, {3}, {4}, {5}, {6}, '{7}',\r\n        '{8}', '{9}', {10},\r\n        {11}, {12}, {13}, {14}, {15},\r\n        {16}, '{17}', '{18}', '{19}', '{20}', {21}\r\n    );", objArray), conn);
                int num2 = ((DbCommand)mySqlCommand1).ExecuteNonQuery();
                mySqlCommand1.Dispose();
                if (num2 < 0)
                    return -1;
                MySQLCommand mySqlCommand2 = new MySQLCommand("SELECT LAST_INSERT_ID() AS inserted_id", conn);
                MySQLDataReader mySqlDataReader = mySqlCommand2.ExecuteReaderEx();
                if (((DbDataReader)mySqlDataReader).Read())
                    num1 = Convert.ToInt32(((DbDataReader)mySqlDataReader)["inserted_id"].ToString());
                mySqlCommand2.Dispose();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.ChoDen, "[InsertBlackMarketItem] ERROR: " + ex.ToString());
                return -1;
            }
            finally
            {
                if (conn != null)
                    this._Database.DBConns.PushDBConnection(conn);
            }
            return num1;
        }

        private bool UpdateBlackMarketItem(int id, int rid_bid, int priceList, string rname_bit)
        {
            MySQLConnection conn = (MySQLConnection)null;
            try
            {
                conn = this._Database.DBConns.PopDBConnection();
                string str = rname_bit?.Replace("'", "''") ?? "";
                MySQLCommand mySqlCommand = new MySQLCommand($"\r\nUPDATE t_blackmarket\r\nSET\r\n    rid_bid = {rid_bid},\r\n    PriceList = {priceList},\r\n    rname_bit = '{str}'\r\nWHERE\r\n    Id = {id};", conn);
                int num = ((DbCommand)mySqlCommand).ExecuteNonQuery();
                mySqlCommand.Dispose();
                return num > 0;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.ChoDen, "[UpdateBlackMarketItem] ERROR: " + ex.ToString());
                return false;
            }
            finally
            {
                if (conn != null)
                    this._Database.DBConns.PushDBConnection(conn);
            }
        }

        private bool DeleteBlackMarketItemById(int id)
        {
            MySQLConnection conn = (MySQLConnection)null;
            try
            {
                conn = this._Database.DBConns.PopDBConnection();
                MySQLCommand mySqlCommand = new MySQLCommand($"DELETE FROM t_blackmarket WHERE Id = {id};", conn);
                int num = ((DbCommand)mySqlCommand).ExecuteNonQuery();
                mySqlCommand.Dispose();
                return num > 0;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.ChoDen, "[DeleteBlackMarketItemById] ERROR: " + ex.ToString());
                return false;
            }
            finally
            {
                if (conn != null)
                    this._Database.DBConns.PushDBConnection(conn);
            }
        }

        private bool SendMailToUser(
          DBManager dbMgr,
          int userID,
          BlackMarketItem item,
          int mailmoney = 0,
          int mailtoken = 0,
          string contentMail = "")
        {
            string str1 = "";
            if (item != null)
                str1 = $"{item.GoodID}_{item.Forge_level}_{item.Props}_{item.GCount}_{0}_{item.Series}_{item.OtherPramer}_{100}";
            string[] fields = new string[11]
            {
      "-1",
      "HỆ THỐNG",
      userID.ToString(),
      userID.ToString(),
      "Chợ Đen",
      contentMail,
      "0",
      "0",
      str1,
      mailmoney.ToString(),
      mailtoken.ToString()
            };
            int.TryParse(fields[0], out int _);
            DataHelper.Base64Encode(fields[1]);
            int.TryParse(fields[2], out int _);
            DataHelper.Base64Encode(fields[3]);
            DataHelper.Base64Encode(fields[4]);
            DataHelper.Base64Encode(fields[5]);
            int.TryParse(fields[6], out int _);
            int.TryParse(fields[7], out int _);
            string str2 = fields[8];
            int.TryParse(fields[9], out int _);
            int.TryParse(fields[10], out int _);
            return Global.AddMail(dbMgr, fields, out int _) != -1;
        }

        public void DoUpdateItem(DBManager dbMgr)
        {
            long num = TimeUtil.NOW();
            if (this.IsItemProseccSing || num - this.LastUpdateItem <= 60000L)
                return;
            this.LastUpdateItem = num;
            this.IsItemProseccSing = true;
            Console.WriteLine("DOUPDATE CHODEN ==>TOTALCOUNT :" + BlackMarket.blackMarketItems.Count.ToString());
            bool flag = false;
            foreach (BlackMarketItem blackMarketItem in BlackMarket.blackMarketItems)
            {
                if (blackMarketItem.EndDate <= DateTime.Now)
                {
                    if (blackMarketItem.rid_bid != 0)
                    {
                        this.SendMailToUser(dbMgr, blackMarketItem.rid_bid, blackMarketItem, contentMail: "Bạn đã bú được vật phẩm từ chợ đen");
                        if (blackMarketItem.MoneyType == 1)
                            this.SendMailToUser(dbMgr, blackMarketItem.rid_owner, (BlackMarketItem)null, mailtoken: (int)((double)blackMarketItem.PriceList * 0.9), contentMail: "Vật phẩm của bán đã được bán!");
                        else
                            this.SendMailToUser(dbMgr, blackMarketItem.rid_owner, (BlackMarketItem)null, (int)((double)blackMarketItem.PriceList * 0.9), contentMail: "Vật phẩm của bán đã được bán");
                    }
                    else
                        this.SendMailToUser(dbMgr, blackMarketItem.rid_owner, blackMarketItem, contentMail: "Đồ của bán không bán được");
                    flag = BlackMarket.instance.DeleteBlackMarketItemById(blackMarketItem.Id);
                }
            }
            if (flag)
                this.GetAllBlackMarketItems();
            this.IsItemProseccSing = false;
        }

        public static TCPProcessCmdResults CMD_KT_ACTION_BLACKMARKET(
          DBManager dbMgr,
          TCPOutPacketPool pool,
          int nID,
          byte[] data,
          int count,
          out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = (TCPOutPacket)null;
            string str1;
            try
            {
                str1 = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, $"Wrong socket data CMD={(Enum)(TCPGameServerCmds)nID}");
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            try
            {
                string[] strArray = str1.Split(':');
                if (strArray.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, $"Error Socket params count not fit CMD={(Enum)(TCPGameServerCmds)nID}, Recv={strArray.Length}, CmdData={str1}");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                int num = int.Parse(strArray[0]);
                int RoleID = int.Parse(strArray[1]);
                RankMode _RankInput = (RankMode)num;
                int rankOfPlayer = RankingManager.getInstance().GetRankOfPlayer(RoleID, _RankInput);
                string str2 = $"{RoleID.ToString()}:{rankOfPlayer.ToString()}";
                byte[] bytes = DataHelper.ObjectToBytes<List<BlackMarketItem>>(BlackMarket.blackMarketItems);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, bytes, 0, bytes.Length, nID);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }
            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        public static TCPProcessCmdResults CMD_KT_TIEN_BLACKMARKET_FIND(
          DBManager dbMgr,
          TCPOutPacketPool pool,
          int nID,
          byte[] data,
          int count,
          out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = (TCPOutPacket)null;
            string str;
            try
            {
                str = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, $"Wrong socket data CMD={(Enum)(TCPGameServerCmds)nID}");
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            try
            {
                string[] strArray = str.Split(':');
                if (strArray.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, $"Error Socket params count not fit CMD={(Enum)(TCPGameServerCmds)nID}, Recv={strArray.Length}, CmdData={str}");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                ///int.Parse(strArray[0]);
                byte[] bytes = DataHelper.ObjectToBytes<List<BlackMarketItem>>(BlackMarket.blackMarketItems);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, bytes, 0, bytes.Length, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }
            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        public static TCPProcessCmdResults CMD_KT_TIEN_BLACKMARKET_SELL(
          DBManager dbMgr,
          TCPOutPacketPool pool,
          int nID,
          byte[] data,
          int count,
          out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = (TCPOutPacket)null;
            string str1;
            try
            {
                str1 = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, $"Wrong socket data CMD={(Enum)(TCPGameServerCmds)nID}");
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            try
            {
                string[] strArray = str1.Split(':');
                if (strArray.Length != 17)
                {
                    LogManager.WriteLog(LogTypes.Error, $"Error Socket params count not fit CMD={(Enum)(TCPGameServerCmds)nID}, Recv={strArray.Length}, CmdData={str1}");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                int num1 = int.Parse(strArray[0]);
                int num2 = int.Parse(strArray[1]);
                int num3 = int.Parse(strArray[2]);
                int num4 = int.Parse(strArray[3]);
                int num5 = int.Parse(strArray[4]);
                int num6 = int.Parse(strArray[5]);
                int num7 = int.Parse(strArray[6]);
                int num8 = int.Parse(strArray[7]);
                int num9 = int.Parse(strArray[8]);
                int num10 = int.Parse(strArray[9]);
                int num11 = int.Parse(strArray[10]);
                int num12 = int.Parse(strArray[11]);
                string plainText1 = strArray[12];
                string plainText2 = strArray[13];
                int num13 = int.Parse(strArray[14]);
                string str2 = strArray[15];
                string str3 = strArray[16 /*0x10*/];
                DateTime now = DateTime.Now;
                BlackMarketItem blackMarketItem = new BlackMarketItem();
                blackMarketItem.GoodID = num2;
                blackMarketItem.Name = DataHelper.Base64Encode(plainText2);
                blackMarketItem.ParticularType = num6;
                blackMarketItem.Series = num7;
                blackMarketItem.Level = num8;
                blackMarketItem.Genre = num4;
                blackMarketItem.DetailType = num5;
                blackMarketItem.CreateDate = now;
                blackMarketItem.UpdateDate = now;
                blackMarketItem.PriceList = num10;
                blackMarketItem.PriceBuy = num11;
                blackMarketItem.rid_owner = num1;
                blackMarketItem.rname_owner = DataHelper.Base64Encode(plainText1);
                blackMarketItem.rname_bit = "";
                blackMarketItem.rid_bid = 0;
                blackMarketItem.GCount = num13;
                blackMarketItem.MoneyType = num12;
                blackMarketItem.Category = num3;
                blackMarketItem.Props = str2;
                blackMarketItem.OtherPramer = str3;
                switch (num9)
                {
                    case 0:
                        blackMarketItem.EndDate = now.AddHours(6.0);
                        break;
                    case 1:
                        blackMarketItem.EndDate = now.AddHours(24.0);
                        break;
                    case 2:
                        blackMarketItem.EndDate = now.AddHours(48.0);
                        break;
                    case 3:
                        blackMarketItem.EndDate = now.AddHours(72.0);
                        break;
                }
                int num14 = BlackMarket.instance.InsertBlackMarketItem(blackMarketItem);
                if (num14 == -1)
                {
                    LogManager.WriteLog(LogTypes.Error, $"Loi tao do cho den CMD={(Enum)(TCPGameServerCmds)nID}, Recv={strArray.Length}, CmdData={str1}");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                blackMarketItem.Id = num14;
                BlackMarket.blackMarketItems.Add(blackMarketItem);
                byte[] bytes = DataHelper.ObjectToBytes<List<BlackMarketItem>>(BlackMarket.blackMarketItems);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, bytes, 0, bytes.Length, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }
            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        public static TCPProcessCmdResults CMD_KT_TIEN_BLACKMARKET_BUY(
          DBManager dbMgr,
          TCPOutPacketPool pool,
          int nID,
          byte[] data,
          int count,
          out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = (TCPOutPacket)null;
            string str;
            try
            {
                str = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, $"Wrong socket data CMD={(Enum)(TCPGameServerCmds)nID}");
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            try
            {
                string[] strArray = str.Split(':');
                if (strArray.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, $"Error Socket params count not fit CMD={(Enum)(TCPGameServerCmds)nID}, Recv={strArray.Length}, CmdData={str}");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                int userID = int.Parse(strArray[0]);
                int itemID = int.Parse(strArray[1]);
                BlackMarketItem blackMarketItem = BlackMarket.blackMarketItems.Find((Predicate<BlackMarketItem>)(x => x.Id == itemID));
                if (blackMarketItem == null)
                {
                    LogManager.WriteLog(LogTypes.Error, $"Error Socket params count not fit CMD={(Enum)(TCPGameServerCmds)nID}, Recv={strArray.Length}, CmdData={str}");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                if (blackMarketItem.rid_bid > 0 && blackMarketItem.rname_bit != "")
                {
                    if (blackMarketItem.MoneyType == 1)
                        BlackMarket.getInstance().SendMailToUser(dbMgr, blackMarketItem.rid_bid, (BlackMarketItem)null, mailtoken: blackMarketItem.PriceList, contentMail: "Bạn được hoàn tiền từ đấu giá !");
                    else
                        BlackMarket.getInstance().SendMailToUser(dbMgr, blackMarketItem.rid_bid, (BlackMarketItem)null, blackMarketItem.PriceList, contentMail: "Bạn được hoàn tiền từ đấu giá !");
                }

                BlackMarket.getInstance().SendMailToUser(dbMgr, userID, blackMarketItem, contentMail: "Bạn đã bú được vật phẩm từ chợ đen");
                if (blackMarketItem.MoneyType == 1)
                    BlackMarket.getInstance().SendMailToUser(dbMgr, blackMarketItem.rid_owner, (BlackMarketItem)null, mailtoken: (int)((double)blackMarketItem.PriceBuy * 0.9), contentMail: "Vật phẩm của bạn đã được bán!");
                else
                    BlackMarket.getInstance().SendMailToUser(dbMgr, blackMarketItem.rid_owner, (BlackMarketItem)null, (int)((double)blackMarketItem.PriceBuy * 0.9), contentMail: "Vật phẩm của bạn đã được bán");
                BlackMarket.getInstance().DeleteBlackMarketItemById(itemID);
                BlackMarket.blackMarketItems.RemoveAll((Predicate<BlackMarketItem>)(x => x.Id == itemID));
                byte[] bytes = DataHelper.ObjectToBytes<List<BlackMarketItem>>(BlackMarket.blackMarketItems);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, bytes, 0, bytes.Length, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }
            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        public static TCPProcessCmdResults CMD_KT_TIEN_BLACKMARKET_BID(
          DBManager dbMgr,
          TCPOutPacketPool pool,
          int nID,
          byte[] data,
          int count,
          out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = (TCPOutPacket)null;
            string str;
            try
            {
                str = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, $"Wrong socket data CMD={(Enum)(TCPGameServerCmds)nID}");
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            try
            {
                string[] strArray = str.Split(':');
                if (strArray.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, $"Error Socket params count not fit CMD={(Enum)(TCPGameServerCmds)nID}, Recv={strArray.Length}, CmdData={str}");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                int rid = int.Parse(strArray[0]);
                int num = int.Parse(strArray[1]);
                int ID = int.Parse(strArray[2]);
                string maxLevelRoleName = "";
                if (maxLevelRoleName == "")
                {
                    string userID = "";
                    Global.GetRoleNameAndUserID(dbMgr, rid, out maxLevelRoleName, out userID);
                }
                BlackMarketItem blackMarketItem = BlackMarket.blackMarketItems.Find((Predicate<BlackMarketItem>)(x => x.Id == ID));
                if (blackMarketItem == null)
                {
                    LogManager.WriteLog(LogTypes.Error, $"Error Socket params count not fit CMD={(Enum)(TCPGameServerCmds)nID}, Recv={strArray.Length}, CmdData={str}");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                if (blackMarketItem.rid_bid > 0 && blackMarketItem.rname_bit != "")
                {
                    if (blackMarketItem.MoneyType == 1)
                        BlackMarket.getInstance().SendMailToUser(dbMgr, blackMarketItem.rid_bid, (BlackMarketItem)null, mailtoken: blackMarketItem.PriceList, contentMail: "Vật phẩm bạn đấu giá bị đấu giá hơn!");
                    else
                        BlackMarket.getInstance().SendMailToUser(dbMgr, blackMarketItem.rid_bid, (BlackMarketItem)null, blackMarketItem.PriceList, contentMail: "Vật phẩm bạn đấu giá bị đấu giá hơn!");
                }
                blackMarketItem.PriceList = num;
                blackMarketItem.rid_bid = rid;
                blackMarketItem.rname_bit = DataHelper.Base64Encode(maxLevelRoleName);
                BlackMarket.getInstance().UpdateBlackMarketItem(blackMarketItem.Id, blackMarketItem.rid_bid, blackMarketItem.PriceList, blackMarketItem.rname_bit);
                blackMarketItem.rname_bit = DataHelper.Base64Decode(blackMarketItem.rname_bit);
                byte[] bytes = DataHelper.ObjectToBytes<List<BlackMarketItem>>(BlackMarket.blackMarketItems);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, bytes, 0, bytes.Length, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }
            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }


        public static TCPProcessCmdResults CMD_KT_TIEN_BLACKMARKET_CANCEL(
          DBManager dbMgr,
          TCPOutPacketPool pool,
          int nID,
          byte[] data,
          int count,
          out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = (TCPOutPacket)null;
            string str;
            try
            {
                str = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, $"Wrong socket data CMD={(Enum)(TCPGameServerCmds)nID}");
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            try
            {
                string[] strArray = str.Split(':');
                if (strArray.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, $"Error Socket params count not fit CMD={(Enum)(TCPGameServerCmds)nID}, Recv={strArray.Length}, CmdData={str}");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                int userID = int.Parse(strArray[0]);
                int itemID = int.Parse(strArray[1]);
                BlackMarketItem blackMarketItem = BlackMarket.blackMarketItems.Find((Predicate<BlackMarketItem>)(x => x.Id == itemID));
                if (blackMarketItem == null)
                {
                    LogManager.WriteLog(LogTypes.Error, $"Error Socket params count not fit CMD={(Enum)(TCPGameServerCmds)nID}, Recv={strArray.Length}, CmdData={str}");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                // ===== CHECK OWNER =====
                if (blackMarketItem.rid_owner != userID)
                {
                    LogManager.WriteLog(LogTypes.Error,
                        $"CANCEL FAIL: user={userID} owner={blackMarketItem.rid_owner} BMID={itemID}");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0",
                        (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                // ===== CHECK BID =====
                if (blackMarketItem.rid_bid > 0)
                {
                    LogManager.WriteLog(LogTypes.Error,
                        $"CANCEL FAIL: BMID={itemID} already has bid");
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0",
                        (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                // ===== TRẢ ITEM VỀ CHO NGƯỜI BÁN =====
                BlackMarket.getInstance().SendMailToUser(
                    dbMgr,
                    userID,
                    blackMarketItem,
                    contentMail: "Bạn đã hủy bán và nhận lại vật phẩm từ chợ"
                );

                BlackMarket.getInstance().DeleteBlackMarketItemById(itemID);
                BlackMarket.blackMarketItems.RemoveAll((Predicate<BlackMarketItem>)(x => x.Id == itemID));
                byte[] bytes = DataHelper.ObjectToBytes<List<BlackMarketItem>>(BlackMarket.blackMarketItems);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, bytes, 0, bytes.Length, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }
            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

    }

}
