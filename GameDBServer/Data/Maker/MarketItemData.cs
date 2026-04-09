using ProtoBuf;
using Server.Data;

[ProtoContract]
public class MarketItemData
{
    // ===== SELLER =====
    [ProtoMember(1)]
    public int SellerRoleID;

    [ProtoMember(2)]
    public string SellerName;

    [ProtoMember(3)]
    public string StallName;

    // ===== GOODS =====
    [ProtoMember(4)]
    public int GoodsDBID;

    [ProtoMember(5)]
    public int GoodsID;

    [ProtoMember(6)]
    public GoodsData Goods;

    // ===== AUCTION =====

    [ProtoMember(7)]
    public int StartPrice;        // Giá đấu ban đầu

    [ProtoMember(8)]
    public int BuyoutPrice;       // Giá mua ngay (0 = không có)

    [ProtoMember(9)]
    public int CurrentPrice;      // Giá cao nhất hiện tại

    [ProtoMember(10)]
    public int HighestBidder;     // RoleID người đang đấu cao nhất

    // ===== TIME =====
    [ProtoMember(11)]
    public long StartTime;        // Thời điểm đăng bán

    [ProtoMember(12)]
    public int DurationHour;      // 24 / 48 / 72

    // ===== COMPAT (GIỮ LẠI ĐỂ CLIENT CŨ KHÔNG TOANG) =====
    [ProtoMember(13)]
    public int Price;             // Map = CurrentPrice hoặc BuyoutPrice
    [ProtoMember(14)]
    public int LeftSeconds; // 🔥 THÊM CÁI NÀY
}
