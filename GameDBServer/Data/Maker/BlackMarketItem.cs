using ProtoBuf;
using Server.Data;
using System;

[ProtoContract]
    public class BlackMarketItem
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public int Category { get; set; }

        [ProtoMember(4)]
        public int Genre { get; set; }

        [ProtoMember(5)]
        public int DetailType { get; set; }

        [ProtoMember(6)]
        public int ParticularType { get; set; }

        [ProtoMember(7)]
        public int Series { get; set; }

        [ProtoMember(8)]
        public int Level { get; set; }

        [ProtoMember(9)]
        public DateTime CreateDate { get; set; }

        [ProtoMember(10)]
        public DateTime UpdateDate { get; set; }

        [ProtoMember(11)]
        public DateTime EndDate { get; set; }

        [ProtoMember(12)]
        public int PriceList { get; set; }

        [ProtoMember(13)]
        public int PriceBuy { get; set; }

        [ProtoMember(14)]
        public int rid_owner { get; set; }

        [ProtoMember(15)]
        public int rid_bid { get; set; }

        [ProtoMember(16 /*0x10*/)]
        public int MoneyType { get; set; }

        [ProtoMember(17)]
        public int GoodID { get; set; }

        [ProtoMember(18)]
        public int GCount { get; set; }

        [ProtoMember(19)]
        public string rname_bit { get; set; }

        [ProtoMember(20)]
        public string rname_owner { get; set; }

        [ProtoMember(21)]
        public string Props { get; set; }

        [ProtoMember(22)]
        public string OtherPramer { get; set; }

        [ProtoMember(23)]
        public int Forge_level { get; set; }
    }
