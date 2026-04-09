using ProtoBuf;

namespace Server.Data
{
    [ProtoContract]
    public class GameEventsData
    {
        [ProtoMember(1)]
        public int GameId;

        [ProtoMember(2)]
        public string Data;
    }
}
