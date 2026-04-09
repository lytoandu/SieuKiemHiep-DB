using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Thông tin bạn bè
    /// </summary>
    [ProtoContract]
    public class OccupationData
    {
        /// <summary>
        /// Id phái
        /// </summary>
        [ProtoMember(1)]
        public int Id { get; set; }

        /// <summary>
        /// 1: phái chính, 0: phái phụ
        /// </summary>
        [ProtoMember(2)]
        public int PrimaryFlag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(3)]
        public int SubId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(4)]
        public string MainQuickBarKeys { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(5)]
        public string OtherQuickBarKeys { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(6)]
        public string AutoSettings { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(7)]
        public int Strength { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(8)]
        public int Intelligence { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(9)]
        public int Dexterity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(10)]
        public int Constitution { get; set; }
    }
}
