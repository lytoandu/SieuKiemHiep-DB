using GameDBServer.Data;
using Server.Data;
using System.Collections.Generic;

namespace GameDBServer.Logic.Market
{
    public static class MarketManager
    {
        public static List<MarketItemData> LoadAllMarketItems()
        {
            List<MarketItemData> result = new List<MarketItemData>();

            foreach (var pair in StallManager.StallManager.getInstance().TotalStallData)
            {
                StallData stall = pair.Value;

                if (stall == null || stall.Start != 1)
                    continue;

                if (stall.GoodsPriceDict == null || stall.GoodsPriceDict.Count == 0)
                    continue;

                if (stall.GoodsStartTimeDict == null)
                    stall.GoodsStartTimeDict = new Dictionary<int, int>();

                if (stall.GoodsBuyoutPriceDict == null)
                    stall.GoodsBuyoutPriceDict = new Dictionary<int, int>();

                if (stall.GoodsDurationHourDict == null)
                    stall.GoodsDurationHourDict = new Dictionary<int, int>();

                // 🔥 DUYỆT THEO KEY MARKET, KHÔNG PHỤ THUỘC GoodsList
                foreach (var kv in stall.GoodsPriceDict)
                {
                    int goodsDbID = kv.Key;
                    int startPrice = kv.Value;

                    stall.GoodsStartTimeDict.TryGetValue(goodsDbID, out int startTime);
                    stall.GoodsBuyoutPriceDict.TryGetValue(goodsDbID, out int buyoutPrice);
                    stall.GoodsDurationHourDict.TryGetValue(goodsDbID, out int durationHour);

                    if (startTime <= 0 || durationHour <= 0)
                        continue;

                    // tìm GoodsData tương ứng
                    GoodsData gd = stall.GoodsList?.Find(x => x.Id == goodsDbID);
                    if (gd == null)
                        continue;

                    MarketItemData item = new MarketItemData()
                    {
                        SellerRoleID = stall.RoleID,
                        SellerName = stall.RoleName,
                        StallName = stall.StallName,

                        GoodsDBID = goodsDbID,
                        GoodsID = gd.GoodsID,
                        Goods = gd,

                        StartPrice = startPrice,
                        BuyoutPrice = buyoutPrice,
                        CurrentPrice = startPrice,
                        HighestBidder = 0,

                        StartTime = startTime,
                        DurationHour = durationHour,

                        Price = buyoutPrice > 0 ? buyoutPrice : startPrice
                    };

                    result.Add(item);
                }
            }

            return result;
        }

    }
}
