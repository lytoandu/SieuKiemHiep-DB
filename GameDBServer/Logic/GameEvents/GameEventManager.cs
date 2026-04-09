using GameDBServer.DB;
using GameDBServer.Server;
using MySQLDriverCS;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDBServer.Logic.GameEvents
{
	public class GameEventManager
	{
		private const string V = "DataString";
		readonly string tbName = "t_gameevents";

		private readonly DBManager Database;

		public static GameEventManager Instance { get; private set; }

		private GameEventManager(DBManager dBManager)
		{
			/// Lưu lại đối tượng quản lý DB
			this.Database = dBManager;
		}

		public static void Init(DBManager dBManager)
		{
			Instance = new GameEventManager(dBManager);
		}

		private bool Save(GameEventsData gameEvents)
		{
			var conn = Database.DBConns.PopDBConnection();
			try
			{
				var updated = new MySQLUpdateCommand(conn,
				new object[,]
				{
					{ "Data", gameEvents.Data },
				},
				tbName,
				new object[,]
				{{"GameId", "=", gameEvents.GameId }}, null);
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

		private GameEventsData Load(int GameId)
		{
			var gameEventData = new GameEventsData()
			{
				GameId = GameId,
				Data = "{}",
			};

			var conn = Database.DBConns.PopDBConnection();
			try
			{
				string query = $"SELECT Data FROM {tbName} WHERE GameId = {GameId}";
				var command = new MySQLCommand(query, conn);
				var reader = command.ExecuteReader();

				if (reader.Read())
				{
					// Lấy dữ liệu từ cột Content
					string content = reader.GetString(0);
					gameEventData.Data = content;
				}

				return gameEventData;
			}
			catch (Exception ex)
			{
				LogManager.WriteLog(LogTypes.Pet, $"Có lỗi trong quá trình lấy thông tin bạn đồng hành. {ex.Message}");
				return gameEventData;
			}
		}


		#region network
		public TCPProcessCmdResults ProcessGameeventSys(TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
		{
			try
			{
				var resultBytes = DataHelper.BytesToObject<GameEventsData>(data, 0, count);
				var updated = Save(resultBytes);
				string strcmd = updated ? "1" : "0";
				byte[] bytes = DataHelper.ObjectToBytes(strcmd);
				tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, bytes, 0, bytes.Length, nID);
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
		public TCPProcessCmdResults ProcessGameeventLoad(TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
		{
			try
			{
				var gameId = DataHelper.BytesToObject<int>(data, 0, count);
				var dbLoad = Load(gameId);
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
