using ProtoBuf;

namespace Server.Data
{
    [ProtoContract]
    public class RewardData
    {
        [ProtoMember(1)]
        public int BotID { get; set; }

        [ProtoMember(2)]
        public int RoleId { get; set; }

        [ProtoMember(3)]
        public string Reward { get; set; }
    }
}
