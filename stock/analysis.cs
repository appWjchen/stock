using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
 * 2018/03/05
 * 原電腦 JavaScript 版有動能分析，在此 C# 版中移除動能分析，
 * 因為動能分析似乎影響不大。
 * 將來若有需要，再加入即可。
 */
namespace stock
{
    class AccumulateData
    {
        public String lastDate;
        public int accumulateScore;
        public Double lowestPrice;
        public Company company;
        public String id;
        public String name;
        public Double lowestPriceToday;
        public Double volumeToday;
        public Double volumeYesterday;
        public int rankToday;
        public int lastRank;
        public Double sVolumeIncrese1;      // 今天法人買超量
        public Double sVolumeIncrese2;      // 昨天法人買超量
        public Double diff;                 // 脫出排名天數 
        public Boolean pass;
    }
    /*
        模組 Analysis 用來對大盤及各股的動能及技術分析指標做評比，並從各股
        評比中挑選出排名前 20 名的股票，由使用者查看挑選出股票的訐比分數及
        原因。
    */
    class analysis
    {
        StockDatabase stockDatabase;
        /*
          以下是每種分析的結果，每種股票有一個 scoreArray 陣列，
          當分析出一種結果時，會將結果 push 到該陣列中，例如當分
          析出大盤是多頭排列時，
            scoreArray.push(TOCK_TREND);
          查詢每個股票的這個陣列，將可以知道該股票的總訐分及原因。
            1 ~ 500 保留給大盤評比用
            501 ~ 各股評比用
          日線的評估值通常最低，其次是週線，月線評估值最高。
        */
        const int STOCK_TREND_UP = 1;                     // 大盤趨勢，多頭排列
        const int STOCK_DAY_TURN_UP = 2;                  // 大盤趨勢，表示日線由空轉多
        const int STOCK_WEEK_TURN_UP = 3;                 // 大盤趨勢，表示週線由空轉多
        const int STOCK_MONTH_TURN_UP = 4;                // 大盤趨勢，表示月線由空轉多
        const int STOCK_DAY_K20 = 5;                      // 大盤趨勢，表示日線 K 值低於 20
        const int STOCK_WEEK_K20 = 6;                     // 大盤趨勢，表示週線 K 值低於 20
        const int STOCK_MONTH_K20 = 7;                    // 大盤趨勢，表示月線 K 值低於 20
        const int STOCK_DAY_K15 = 8;                      // 大盤趨勢，表示日線 K 值低於 15
        const int STOCK_WEEK_K15 = 9;                     // 大盤趨勢，表示週線 K 值低於 15
        const int STOCK_MONTH_K15 = 10;                   // 大盤趨勢，表示月線 K 值低於 15
        const int STOCK_DAY_K10 = 11;                     // 大盤趨勢，表示日線 K 值低於 10
        const int STOCK_WEEK_K10 = 12;                    // 大盤趨勢，表示週線 K 值低於 10
        const int STOCK_MONTH_K10 = 13;                   // 大盤趨勢，表示月線 K 值低於 10
        const int STOCK_DAY_K5 = 14;                      // 大盤趨勢，表示日線 K 值低於 5
        const int STOCK_WEEK_K5 = 15;                     // 大盤趨勢，表示週線 K 值低於 5
        const int STOCK_MONTH_K5 = 16;                    // 大盤趨勢，表示月線 K 值低於 5
        const int STOCK_DAY_THROUGH_K20 = 17;             // 大盤趨勢，表示日線 K 值由低於 20 穿越過，變成高於 20
        const int STOCK_WEEK_THROUGH_K20 = 18;            // 大盤趨勢，表示週線 K 值由低於 20 穿越過，變成高於 20
        const int STOCK_MONTH_THROUGH_K20 = 19;           // 大盤趨勢，表示月線 K 值由低於 20 穿越過，變成高於 20
        const int STOCK_DAY_THROUGH_K15 = 20;             // 大盤趨勢，表示日線 K 值由低於 15 穿越過，變成高於 15
        const int STOCK_WEEK_THROUGH_K15 = 21;            // 大盤趨勢，表示週線 K 值由低於 15 穿越過，變成高於 15
        const int STOCK_MONTH_THROUGH_K15 = 22;           // 大盤趨勢，表示月線 K 值由低於 15 穿越過，變成高於 15
        const int STOCK_DAY_THROUGH_K10 = 23;             // 大盤趨勢，表示日線 K 值由低於 10 穿越過，變成高於 10
        const int STOCK_WEEK_THROUGH_K10 = 24;            // 大盤趨勢，表示週線 K 值由低於 10 穿越過，變成高於 10
        const int STOCK_MONTH_THROUGH_K10 = 25;           // 大盤趨勢，表示月線 K 值由低於 10 穿越過，變成高於 10
        const int STOCK_DAY_THROUGH_K5 = 26;              // 大盤趨勢，表示日線 K 值由低於 5 穿越過，變成高於 5
        const int STOCK_WEEK_THROUGH_K5 = 27;             // 大盤趨勢，表示週線 K 值由低於 5 穿越過，變成高於 5
        const int STOCK_MONTH_THROUGH_K5 = 28;            // 大盤趨勢，表示月線 K 值由低於 5 穿越過，變成高於 5
        const int STOCK_DAY_K_THROUGH_D = 29;             // 大盤趨勢，表示日線 K 值由低於 D 值穿越，變成高於 D 值
        const int STOCK_WEEK_K_THROUGH_D = 30;            // 大盤趨勢，表示週線 K 值由低於 D 值穿越，變成高於 D 值
        const int STOCK_MONTH_K_THROUGH_D = 31;           // 大盤趨勢，表示月線 K 值由低於 D 值穿越，變成高於 D 值
        const int STOCK_DAY_MACD = 32;                    // 大盤趨勢，表示日線 MACD 值穿越信號線(9期MACD平均線)
        const int STOCK_WEEK_MACD = 33;                   // 大盤趨勢，表示週線 MACD 值穿越信號線(9期MACD平均線)
        const int STOCK_MONTH_MACD = 34;                  // 大盤趨勢，表示月線 MACD 值穿越信號線(9期MACD平均線)
        const int STOCK_DAY_RSI5_30 = 35;                 // 大盤趨勢，表示日線 RSI5 值小於 30
        const int STOCK_DAY_RSI5_20 = 36;                 // 大盤趨勢，表示日線 RSI5 值小於 20
        const int STOCK_DAY_RSI5_10 = 37;                 // 大盤趨勢，表示日線 RSI5 值小於 10
        const int STOCK_WEEK_RSI5_30 = 38;                // 大盤趨勢，表示週線 RSI5 值小於 30
        const int STOCK_WEEK_RSI5_20 = 39;                // 大盤趨勢，表示週線 RSI5 值小於 20
        const int STOCK_WEEK_RSI5_10 = 40;                // 大盤趨勢，表示週線 RSI5 值小於 10
        const int STOCK_MONTH_RSI5_30 = 41;               // 大盤趨勢，表示月線 RSI5 值小於 30
        const int STOCK_MONTH_RSI5_20 = 42;               // 大盤趨勢，表示月線 RSI5 值小於 20
        const int STOCK_MONTH_RSI5_10 = 43;               // 大盤趨勢，表示月線 RSI5 值小於 10
        const int STOCK_DAY_RSI5_THROUGH_RSI10 = 44;      // 大盤趨勢，表示日線 RSI5 由下穿越 RSI10
        const int STOCK_WEEK_RSI5_THROUGH_RSI10 = 45;     // 大盤趨勢，表示週線 RSI5 由下穿越 RSI10
        const int STOCK_MONTH_RSI5_THROUGH_RSI10 = 46;    // 大盤趨勢，表示月線 RSI5 由下穿越 RSI10
        const int STOCK_DAY_ABI10MA = 47;                 // 大盤趨勢，表示日線 ABI10MA 大於 40%
        const int STOCK_WEEK_ABI10MA = 48;                // 大盤趨勢，表示週線 ABI10MA 大於 40%
        const int STOCK_MONTH_ABI10MA = 49;               // 大盤趨勢，表示月線 ABI10MA 大於 40%
        const int STOCK_DAY_ADL20MA = 50;                 // 大盤趨勢，表示日線 ADL20MA 斜率為正
        const int STOCK_WEEK_ADL20MA = 51;                // 大盤趨勢，表示週線 ADL20MA 斜率為正
        const int STOCK_MONTH_ADL20MA = 52;               // 大盤趨勢，表示月線 ADL20MA 斜率為正

        const int COMPANY_TREND_UP = 501;                 // 各股趨勢，多頭排列
        const int COMPANY_DAY_TURN_UP = 502;              // 各股趨勢，表示日線由空轉多
        const int COMPANY_WEEK_TURN_UP = 503;             // 各股趨勢，表示週線由空轉多
        const int COMPANY_MONTH_TURN_UP = 504;            // 各股趨勢，表示月線由空轉多
        const int COMPANY_DAY_K20 = 505;                  // 各股趨勢，表示日線 K 值低於 20
        const int COMPANY_WEEK_K20 = 506;                 // 各股趨勢，表示週線 K 值低於 20
        const int COMPANY_MONTH_K20 = 507;                // 各股趨勢，表示月線 K 值低於 20
        const int COMPANY_DAY_K15 = 508;                  // 各股趨勢，表示日線 K 值低於 15
        const int COMPANY_WEEK_K15 = 509;                 // 各股趨勢，表示週線 K 值低於 15
        const int COMPANY_MONTH_K15 = 510;                // 各股趨勢，表示月線 K 值低於 15
        const int COMPANY_DAY_K10 = 511;                  // 各股趨勢，表示日線 K 值低於 10
        const int COMPANY_WEEK_K10 = 512;                 // 各股趨勢，表示週線 K 值低於 10
        const int COMPANY_MONTH_K10 = 513;                // 各股趨勢，表示月線 K 值低於 10
        const int COMPANY_DAY_K5 = 514;                   // 各股趨勢，表示日線 K 值低於 5
        const int COMPANY_WEEK_K5 = 515;                  // 各股趨勢，表示週線 K 值低於 5
        const int COMPANY_MONTH_K5 = 516;                 // 各股趨勢，表示月線 K 值低於 5
        const int COMPANY_DAY_THROUGH_K20 = 517;          // 各股趨勢，表示日線 K 值由低於 20 穿越過，變成高於 20
        const int COMPANY_WEEK_THROUGH_K20 = 518;         // 各股趨勢，表示週線 K 值由低於 20 穿越過，變成高於 20
        const int COMPANY_MONTH_THROUGH_K20 = 519;        // 各股趨勢，表示月線 K 值由低於 20 穿越過，變成高於 20
        const int COMPANY_DAY_THROUGH_K15 = 520;          // 各股趨勢，表示日線 K 值由低於 15 穿越過，變成高於 15
        const int COMPANY_WEEK_THROUGH_K15 = 521;         // 各股趨勢，表示週線 K 值由低於 15 穿越過，變成高於 15
        const int COMPANY_MONTH_THROUGH_K15 = 522;        // 各股趨勢，表示月線 K 值由低於 15 穿越過，變成高於 15
        const int COMPANY_DAY_THROUGH_K10 = 523;          // 各股趨勢，表示日線 K 值由低於 10 穿越過，變成高於 10
        const int COMPANY_WEEK_THROUGH_K10 = 524;         // 各股趨勢，表示週線 K 值由低於 10 穿越過，變成高於 10
        const int COMPANY_MONTH_THROUGH_K10 = 525;        // 各股趨勢，表示月線 K 值由低於 10 穿越過，變成高於 10
        const int COMPANY_DAY_THROUGH_K5 = 526;           // 各股趨勢，表示日線 K 值由低於 5 穿越過，變成高於 5
        const int COMPANY_WEEK_THROUGH_K5 = 527;          // 各股趨勢，表示週線 K 值由低於 5 穿越過，變成高於 5
        const int COMPANY_MONTH_THROUGH_K5 = 528;         // 各股趨勢，表示月線 K 值由低於 5 穿越過，變成高於 5
        const int COMPANY_DAY_K_THROUGH_D = 529;          // 各股趨勢，表示日線 K 值由低於 D 值穿越，變成高於 D 值
        const int COMPANY_WEEK_K_THROUGH_D = 530;         // 各股趨勢，表示週線 K 值由低於 D 值穿越，變成高於 D 值
        const int COMPANY_MONTH_K_THROUGH_D = 531;        // 各股趨勢，表示月線 K 值由低於 D 值穿越，變成高於 D 值
        const int COMPANY_CAPITAL_1_10 = 532;             // 各股趨勢，表示資本額排名是 1 到 10 名
        const int COMPANY_CAPITAL_11_20 = 533;            // 各股趨勢，表示資本額排名是 11 到 20 名
        const int COMPANY_CAPITAL_21_30 = 534;            // 各股趨勢，表示資本額排名是 21 到 30 名
        const int COMPANY_CAPITAL_31_40 = 534;            // 各股趨勢，表示資本額排名是 31 到 40 名
        const int COMPANY_CAPITAL_41_50 = 535;            // 各股趨勢，表示資本額排名是 41 到 50 名
        const int COMPANY_DAY_MACD = 536;                 // 各股趨勢，表示日線 MACD 值穿越信號線(9期MACD平均線)
        const int COMPANY_WEEK_MACD = 537;                // 各股趨勢，表示週線 MACD 值穿越信號線(9期MACD平均線)
        const int COMPANY_MONTH_MACD = 538;               // 各股趨勢，表示月線 MACD 值穿越信號線(9期MACD平均線)
        const int COMPANY_DAY_RSI5_30 = 539;              // 各股趨勢，表示日線 RSI5 值小於 30
        const int COMPANY_DAY_RSI5_20 = 540;              // 各股趨勢，表示日線 RSI5 值小於 20
        const int COMPANY_DAY_RSI5_10 = 541;              // 各股趨勢，表示日線 RSI5 值小於 10
        const int COMPANY_WEEK_RSI5_30 = 542;             // 各股趨勢，表示週線 RSI5 值小於 30
        const int COMPANY_WEEK_RSI5_20 = 543;             // 各股趨勢，表示週線 RSI5 值小於 20
        const int COMPANY_WEEK_RSI5_10 = 544;             // 各股趨勢，表示週線 RSI5 值小於 10
        const int COMPANY_MONTH_RSI5_30 = 545;            // 各股趨勢，表示月線 RSI5 值小於 30
        const int COMPANY_MONTH_RSI5_20 = 546;            // 各股趨勢，表示月線 RSI5 值小於 20
        const int COMPANY_MONTH_RSI5_10 = 547;            // 各股趨勢，表示月線 RSI5 值小於 10
        const int COMPANY_DAY_RSI5_THROUGH_RSI10 = 548;   // 各股趨勢，表示日線 RSI5 由下穿越 RSI10
        const int COMPANY_WEEK_RSI5_THROUGH_RSI10 = 549;  // 各股趨勢，表示週線 RSI5 由下穿越 RSI10
        const int COMPANY_MONTH_RSI5_THROUGH_RSI10 = 550; // 各股趨勢，表示月線 RSI5 由下穿越 RSI10
        /*
          scoreValueArray 陣列中的值是代表
            某個技術指標到達某個條件時，所給出的分數值
          剛開始此數值是在程式碼中直接給定評分值，將來可以用歷史記錄
          分析最適當的評分值，也就是找尋讓股票獲利最大化的評分值陣列。
          <<某個技術指標到達某個條件>>代表的是一股勢力，也就是一群使用
          這種方式來購買股票的人，所集合起來的能量，所有的能量集合起來
          就是整個股票上漲的能量。
          shortTermScoreValueArray 是短線估分數陣列
          longTermScoreValueArray 是長線估分數陣列
        */
        int[] shortTermScoreValueArray = new int[1000];
        int[] longTermScoreValueArray = new int[1000];
        int[] scoreValueArray = null;
        string[] scoreTextArray = new string[1000];
        /* analysis 建構式 */
        public analysis(StockDatabase stockDatabaseParam)
        {
            stockDatabase = stockDatabaseParam;
            for (var i = 0; i < 1000; i++)
            {
                shortTermScoreValueArray[i] = -1;
            }
            shortTermScoreValueArray[STOCK_TREND_UP] = 8;                     // 大盤趨勢，多頭排列
            shortTermScoreValueArray[STOCK_DAY_TURN_UP] = 8;                  // 大盤趨勢，表示日線由空轉多
            shortTermScoreValueArray[STOCK_WEEK_TURN_UP] = 2;                 // 大盤趨勢，表示週線由空轉多
            shortTermScoreValueArray[STOCK_MONTH_TURN_UP] = 2;                // 大盤趨勢，表示月線由空轉多
            shortTermScoreValueArray[STOCK_DAY_K20] = 16;                     // 大盤趨勢，表示日線 K 值低於 20
            shortTermScoreValueArray[STOCK_WEEK_K20] = 2;                     // 大盤趨勢，表示週線 K 值低於 20
            shortTermScoreValueArray[STOCK_MONTH_K20] = 2;                    // 大盤趨勢，表示月線 K 值低於 20
            shortTermScoreValueArray[STOCK_DAY_K15] = 16;                     // 大盤趨勢，表示日線 K 值低於 15
            shortTermScoreValueArray[STOCK_WEEK_K15] = 2;                     // 大盤趨勢，表示週線 K 值低於 15
            shortTermScoreValueArray[STOCK_MONTH_K15] = 2;                    // 大盤趨勢，表示月線 K 值低於 15
            shortTermScoreValueArray[STOCK_DAY_K10] = 16;                     // 大盤趨勢，表示日線 K 值低於 10
            shortTermScoreValueArray[STOCK_WEEK_K10] = 2;                     // 大盤趨勢，表示週線 K 值低於 10
            shortTermScoreValueArray[STOCK_MONTH_K10] = 2;                    // 大盤趨勢，表示月線 K 值低於 10
            shortTermScoreValueArray[STOCK_DAY_K5] = 32;                      // 大盤趨勢，表示日線 K 值低於 5
            shortTermScoreValueArray[STOCK_WEEK_K5] = 4;                      // 大盤趨勢，表示週線 K 值低於 5
            shortTermScoreValueArray[STOCK_MONTH_K5] = 4;                     // 大盤趨勢，表示月線 K 值低於 5
            shortTermScoreValueArray[STOCK_DAY_THROUGH_K20] = 16;             // 大盤趨勢，表示日線 K 值由低於 20 穿越過，變成高於 20
            shortTermScoreValueArray[STOCK_WEEK_THROUGH_K20] = 2;             // 大盤趨勢，表示週線 K 值由低於 20 穿越過，變成高於 20
            shortTermScoreValueArray[STOCK_MONTH_THROUGH_K20] = 2;            // 大盤趨勢，表示月線 K 值由低於 20 穿越過，變成高於 20
            shortTermScoreValueArray[STOCK_DAY_THROUGH_K15] = 16;             // 大盤趨勢，表示日線 K 值由低於 15 穿越過，變成高於 15
            shortTermScoreValueArray[STOCK_WEEK_THROUGH_K15] = 2;             // 大盤趨勢，表示週線 K 值由低於 15 穿越過，變成高於 15
            shortTermScoreValueArray[STOCK_MONTH_THROUGH_K15] = 2;            // 大盤趨勢，表示月線 K 值由低於 15 穿越過，變成高於 15
            shortTermScoreValueArray[STOCK_DAY_THROUGH_K10] = 16;             // 大盤趨勢，表示日線 K 值由低於 10 穿越過，變成高於 10
            shortTermScoreValueArray[STOCK_WEEK_THROUGH_K10] = 2;             // 大盤趨勢，表示週線 K 值由低於 10 穿越過，變成高於 10
            shortTermScoreValueArray[STOCK_MONTH_THROUGH_K10] = 2;            // 大盤趨勢，表示月線 K 值由低於 10 穿越過，變成高於 10
            shortTermScoreValueArray[STOCK_DAY_THROUGH_K5] = 32;              // 大盤趨勢，表示日線 K 值由低於 5 穿越過，變成高於 5
            shortTermScoreValueArray[STOCK_WEEK_THROUGH_K5] = 4;              // 大盤趨勢，表示週線 K 值由低於 5 穿越過，變成高於 5
            shortTermScoreValueArray[STOCK_MONTH_THROUGH_K5] = 4;             // 大盤趨勢，表示月線 K 值由低於 5 穿越過，變成高於 5
            shortTermScoreValueArray[STOCK_DAY_K_THROUGH_D] = 128;            // 大盤趨勢，表示日線 K 值由低於 D 值穿越，變成高於 D 值
            shortTermScoreValueArray[STOCK_WEEK_K_THROUGH_D] = 16;            // 大盤趨勢，表示週線 K 值由低於 D 值穿越，變成高於 D 值
            shortTermScoreValueArray[STOCK_MONTH_K_THROUGH_D] = 16;           // 大盤趨勢，表示月線 K 值由低於 D 值穿越，變成高於 D 值
            shortTermScoreValueArray[STOCK_DAY_MACD] = 8;                     // 大盤趨勢，表示日線 MACD 值穿越信號線(9期MACD平均線)
            shortTermScoreValueArray[STOCK_WEEK_MACD] = 2;                    // 大盤趨勢，表示週線 MACD 值穿越信號線(9期MACD平均線)
            shortTermScoreValueArray[STOCK_MONTH_MACD] = 2;                   // 大盤趨勢，表示月線 MACD 值穿越信號線(9期MACD平均線)
            shortTermScoreValueArray[STOCK_DAY_RSI5_30] = 8;                  // 大盤趨勢，表示日線 RSI5 值小於 30
            shortTermScoreValueArray[STOCK_DAY_RSI5_20] = 16;                 // 大盤趨勢，表示日線 RSI5 值小於 20
            shortTermScoreValueArray[STOCK_DAY_RSI5_10] = 32;                 // 大盤趨勢，表示日線 RSI5 值小於 10
            shortTermScoreValueArray[STOCK_WEEK_RSI5_30] = 4;                 // 大盤趨勢，表示週線 RSI5 值小於 30
            shortTermScoreValueArray[STOCK_WEEK_RSI5_20] = 4;                 // 大盤趨勢，表示週線 RSI5 值小於 20
            shortTermScoreValueArray[STOCK_WEEK_RSI5_10] = 4;                 // 大盤趨勢，表示週線 RSI5 值小於 10
            shortTermScoreValueArray[STOCK_MONTH_RSI5_30] = 4;                // 大盤趨勢，表示月線 RSI5 值小於 30
            shortTermScoreValueArray[STOCK_MONTH_RSI5_20] = 4;                // 大盤趨勢，表示月線 RSI5 值小於 20
            shortTermScoreValueArray[STOCK_MONTH_RSI5_10] = 4;                // 大盤趨勢，表示月線 RSI5 值小於 10
            shortTermScoreValueArray[STOCK_DAY_RSI5_THROUGH_RSI10] = 16;      // 大盤趨勢，表示日線 RSI5 由下穿越 RSI10
            shortTermScoreValueArray[STOCK_WEEK_RSI5_THROUGH_RSI10] = 4;      // 大盤趨勢，表示週線 RSI5 由下穿越 RSI10
            shortTermScoreValueArray[STOCK_MONTH_RSI5_THROUGH_RSI10] = 4;     // 大盤趨勢，表示月線 RSI5 由下穿越 RSI10
            shortTermScoreValueArray[STOCK_DAY_ABI10MA] = 16;                 // 大盤趨勢，表示日線 ABI10MA 大於 40%
            shortTermScoreValueArray[STOCK_WEEK_ABI10MA] = 4;                 // 大盤趨勢，表示週線 ABI10MA 大於 40%
            shortTermScoreValueArray[STOCK_MONTH_ABI10MA] = 4;                // 大盤趨勢，表示月線 ABI10MA 大於 40%
            shortTermScoreValueArray[STOCK_DAY_ADL20MA] = 16;                 // 大盤趨勢，表示日線 ADL20MA 斜率為正
            shortTermScoreValueArray[STOCK_WEEK_ADL20MA] = 4;                 // 大盤趨勢，表示週線 ADL20MA 斜率為正
            shortTermScoreValueArray[STOCK_MONTH_ADL20MA] = 4;                // 大盤趨勢，表示月線 ADL20MA 斜率為正

            shortTermScoreValueArray[COMPANY_TREND_UP] = 8;                   // 各股趨勢，多頭排列
            shortTermScoreValueArray[COMPANY_DAY_TURN_UP] = 8;                // 各股趨勢，表示日線由空轉多
            shortTermScoreValueArray[COMPANY_WEEK_TURN_UP] = 2;               // 各股趨勢，表示週線由空轉多
            shortTermScoreValueArray[COMPANY_MONTH_TURN_UP] = 2;              // 各股趨勢，表示月線由空轉多
            shortTermScoreValueArray[COMPANY_DAY_K20] = 16;                   // 各股趨勢，表示日線 K 值低於 20
            shortTermScoreValueArray[COMPANY_WEEK_K20] = 2;                   // 各股趨勢，表示週線 K 值低於 20
            shortTermScoreValueArray[COMPANY_MONTH_K20] = 2;                  // 各股趨勢，表示月線 K 值低於 20
            shortTermScoreValueArray[COMPANY_DAY_K15] = 16;                   // 各股趨勢，表示日線 K 值低於 15
            shortTermScoreValueArray[COMPANY_WEEK_K15] = 2;                   // 各股趨勢，表示週線 K 值低於 15
            shortTermScoreValueArray[COMPANY_MONTH_K15] = 2;                  // 各股趨勢，表示月線 K 值低於 15
            shortTermScoreValueArray[COMPANY_DAY_K10] = 16;                   // 各股趨勢，表示日線 K 值低於 10
            shortTermScoreValueArray[COMPANY_WEEK_K10] = 2;                   // 各股趨勢，表示週線 K 值低於 10
            shortTermScoreValueArray[COMPANY_MONTH_K10] = 2;                  // 各股趨勢，表示月線 K 值低於 10
            shortTermScoreValueArray[COMPANY_DAY_K5] = 32;                    // 各股趨勢，表示日線 K 值低於 5
            shortTermScoreValueArray[COMPANY_WEEK_K5] = 4;                    // 各股趨勢，表示週線 K 值低於 5
            shortTermScoreValueArray[COMPANY_MONTH_K5] = 4;                   // 各股趨勢，表示月線 K 值低於 5
            shortTermScoreValueArray[COMPANY_DAY_THROUGH_K20] = 16;           // 各股趨勢，表示日線 K 值由低於 20 穿越過，變成高於 20
            shortTermScoreValueArray[COMPANY_WEEK_THROUGH_K20] = 2;           // 各股趨勢，表示週線 K 值由低於 20 穿越過，變成高於 20
            shortTermScoreValueArray[COMPANY_MONTH_THROUGH_K20] = 2;          // 各股趨勢，表示月線 K 值由低於 20 穿越過，變成高於 20
            shortTermScoreValueArray[COMPANY_DAY_THROUGH_K15] = 16;           // 各股趨勢，表示日線 K 值由低於 15 穿越過，變成高於 15
            shortTermScoreValueArray[COMPANY_WEEK_THROUGH_K15] = 2;           // 各股趨勢，表示週線 K 值由低於 15 穿越過，變成高於 15
            shortTermScoreValueArray[COMPANY_MONTH_THROUGH_K15] = 2;          // 各股趨勢，表示月線 K 值由低於 15 穿越過，變成高於 15
            shortTermScoreValueArray[COMPANY_DAY_THROUGH_K10] = 16;           // 各股趨勢，表示日線 K 值由低於 10 穿越過，變成高於 10
            shortTermScoreValueArray[COMPANY_WEEK_THROUGH_K10] = 2;           // 各股趨勢，表示週線 K 值由低於 10 穿越過，變成高於 10
            shortTermScoreValueArray[COMPANY_MONTH_THROUGH_K10] = 2;          // 各股趨勢，表示月線 K 值由低於 10 穿越過，變成高於 10
            shortTermScoreValueArray[COMPANY_DAY_THROUGH_K5] = 32;            // 各股趨勢，表示日線 K 值由低於 5 穿越過，變成高於 5
            shortTermScoreValueArray[COMPANY_WEEK_THROUGH_K5] = 4;            // 各股趨勢，表示週線 K 值由低於 5 穿越過，變成高於 5
            shortTermScoreValueArray[COMPANY_MONTH_THROUGH_K5] = 4;           // 各股趨勢，表示月線 K 值由低於 5 穿越過，變成高於 5
            shortTermScoreValueArray[COMPANY_DAY_K_THROUGH_D] = 128;          // 各股趨勢，表示日線 K 值由低於 D 值穿越，變成高於 D 值
            shortTermScoreValueArray[COMPANY_WEEK_K_THROUGH_D] = 16;          // 各股趨勢，表示週線 K 值由低於 D 值穿越，變成高於 D 值
            shortTermScoreValueArray[COMPANY_MONTH_K_THROUGH_D] = 16;         // 各股趨勢，表示月線 K 值由低於 D 值穿越，變成高於 D 值
            shortTermScoreValueArray[COMPANY_CAPITAL_1_10] = 8;               // 各股趨勢，表示資本額排名是 1 到 10 名
            shortTermScoreValueArray[COMPANY_CAPITAL_11_20] = 8;              // 各股趨勢，表示資本額排名是 11 到 20 名
            shortTermScoreValueArray[COMPANY_CAPITAL_21_30] = 4;              // 各股趨勢，表示資本額排名是 21 到 30 名
            shortTermScoreValueArray[COMPANY_CAPITAL_31_40] = 4;              // 各股趨勢，表示資本額排名是 31 到 40 名
            shortTermScoreValueArray[COMPANY_CAPITAL_41_50] = 2;              // 各股趨勢，表示資本額排名是 41 到 50 名
            shortTermScoreValueArray[COMPANY_DAY_MACD] = 8;                   // 各股趨勢，表示日線 MACD 值穿越信號線(9期MACD平均線)
            shortTermScoreValueArray[COMPANY_WEEK_MACD] = 2;                  // 各股趨勢，表示週線 MACD 值穿越信號線(9期MACD平均線)
            shortTermScoreValueArray[COMPANY_MONTH_MACD] = 2;                 // 各股趨勢，表示月線 MACD 值穿越信號線(9期MACD平均線)
            shortTermScoreValueArray[COMPANY_DAY_RSI5_30] = 8;                // 各股趨勢，表示日線 RSI5 值小於 30
            shortTermScoreValueArray[COMPANY_DAY_RSI5_20] = 16;               // 各股趨勢，表示日線 RSI5 值小於 20
            shortTermScoreValueArray[COMPANY_DAY_RSI5_10] = 32;               // 各股趨勢，表示日線 RSI5 值小於 10
            shortTermScoreValueArray[COMPANY_WEEK_RSI5_30] = 4;               // 各股趨勢，表示週線 RSI5 值小於 30
            shortTermScoreValueArray[COMPANY_WEEK_RSI5_20] = 4;               // 各股趨勢，表示週線 RSI5 值小於 20
            shortTermScoreValueArray[COMPANY_WEEK_RSI5_10] = 4;               // 各股趨勢，表示週線 RSI5 值小於 10
            shortTermScoreValueArray[COMPANY_MONTH_RSI5_30] = 4;              // 各股趨勢，表示月線 RSI5 值小於 30
            shortTermScoreValueArray[COMPANY_MONTH_RSI5_20] = 4;              // 各股趨勢，表示月線 RSI5 值小於 20
            shortTermScoreValueArray[COMPANY_MONTH_RSI5_10] = 4;              // 各股趨勢，表示月線 RSI5 值小於 10
            shortTermScoreValueArray[COMPANY_DAY_RSI5_THROUGH_RSI10] = 16;    // 各股趨勢，表示日線 RSI5 由下穿越 RSI10
            shortTermScoreValueArray[COMPANY_WEEK_RSI5_THROUGH_RSI10] = 4;    // 各股趨勢，表示週線 RSI5 由下穿越 RSI10
            shortTermScoreValueArray[COMPANY_MONTH_RSI5_THROUGH_RSI10] = 4;   // 各股趨勢，表示月線 RSI5 由下穿越 RSI10
            for (var i = 0; i < 1000; i++)
            {
                longTermScoreValueArray[i] = -1;
            }
            longTermScoreValueArray[STOCK_TREND_UP] = 16;                    // 大盤趨勢，多頭排列
            longTermScoreValueArray[STOCK_DAY_TURN_UP] = 16;                 // 大盤趨勢，表示日線由空轉多
            longTermScoreValueArray[STOCK_WEEK_TURN_UP] = 32;                // 大盤趨勢，表示週線由空轉多
            longTermScoreValueArray[STOCK_MONTH_TURN_UP] = 64;               // 大盤趨勢，表示月線由空轉多
            longTermScoreValueArray[STOCK_DAY_K20] = 2;                      // 大盤趨勢，表示日線 K 值低於 20
            longTermScoreValueArray[STOCK_WEEK_K20] = 4;                     // 大盤趨勢，表示週線 K 值低於 20
            longTermScoreValueArray[STOCK_MONTH_K20] = 8;                    // 大盤趨勢，表示月線 K 值低於 20
            longTermScoreValueArray[STOCK_DAY_K15] = 4;                      // 大盤趨勢，表示日線 K 值低於 15
            longTermScoreValueArray[STOCK_WEEK_K15] = 8;                     // 大盤趨勢，表示週線 K 值低於 15
            longTermScoreValueArray[STOCK_MONTH_K15] = 16;                   // 大盤趨勢，表示月線 K 值低於 15
            longTermScoreValueArray[STOCK_DAY_K10] = 8;                      // 大盤趨勢，表示日線 K 值低於 10
            longTermScoreValueArray[STOCK_WEEK_K10] = 16;                    // 大盤趨勢，表示週線 K 值低於 10
            longTermScoreValueArray[STOCK_MONTH_K10] = 32;                   // 大盤趨勢，表示月線 K 值低於 10
            longTermScoreValueArray[STOCK_DAY_K5] = 16;                      // 大盤趨勢，表示日線 K 值低於 5
            longTermScoreValueArray[STOCK_WEEK_K5] = 32;                     // 大盤趨勢，表示週線 K 值低於 5
            longTermScoreValueArray[STOCK_MONTH_K5] = 64;                    // 大盤趨勢，表示月線 K 值低於 5
            longTermScoreValueArray[STOCK_DAY_THROUGH_K20] = 1;              // 大盤趨勢，表示日線 K 值由低於 20 穿越過，變成高於 20
            longTermScoreValueArray[STOCK_WEEK_THROUGH_K20] = 2;             // 大盤趨勢，表示週線 K 值由低於 20 穿越過，變成高於 20
            longTermScoreValueArray[STOCK_MONTH_THROUGH_K20] = 4;            // 大盤趨勢，表示月線 K 值由低於 20 穿越過，變成高於 20
            longTermScoreValueArray[STOCK_DAY_THROUGH_K15] = 2;              // 大盤趨勢，表示日線 K 值由低於 15 穿越過，變成高於 15
            longTermScoreValueArray[STOCK_WEEK_THROUGH_K15] = 4;             // 大盤趨勢，表示週線 K 值由低於 15 穿越過，變成高於 15
            longTermScoreValueArray[STOCK_MONTH_THROUGH_K15] = 8;            // 大盤趨勢，表示月線 K 值由低於 15 穿越過，變成高於 15
            longTermScoreValueArray[STOCK_DAY_THROUGH_K10] = 4;              // 大盤趨勢，表示日線 K 值由低於 10 穿越過，變成高於 10
            longTermScoreValueArray[STOCK_WEEK_THROUGH_K10] = 8;             // 大盤趨勢，表示週線 K 值由低於 10 穿越過，變成高於 10
            longTermScoreValueArray[STOCK_MONTH_THROUGH_K10] = 16;           // 大盤趨勢，表示月線 K 值由低於 10 穿越過，變成高於 10
            longTermScoreValueArray[STOCK_DAY_THROUGH_K5] = 8;               // 大盤趨勢，表示日線 K 值由低於 5 穿越過，變成高於 5
            longTermScoreValueArray[STOCK_WEEK_THROUGH_K5] = 16;             // 大盤趨勢，表示週線 K 值由低於 5 穿越過，變成高於 5
            longTermScoreValueArray[STOCK_MONTH_THROUGH_K5] = 32;            // 大盤趨勢，表示月線 K 值由低於 5 穿越過，變成高於 5
            longTermScoreValueArray[STOCK_DAY_K_THROUGH_D] = 2;              // 大盤趨勢，表示日線 K 值由低於 D 值穿越，變成高於 D 值
            longTermScoreValueArray[STOCK_WEEK_K_THROUGH_D] = 8;             // 大盤趨勢，表示週線 K 值由低於 D 值穿越，變成高於 D 值
            longTermScoreValueArray[STOCK_MONTH_K_THROUGH_D] = 32;           // 大盤趨勢，表示月線 K 值由低於 D 值穿越，變成高於 D 值
            longTermScoreValueArray[STOCK_DAY_MACD] = 4;                     // 大盤趨勢，表示日線 MACD 值穿越信號線(9期MACD平均線)
            longTermScoreValueArray[STOCK_WEEK_MACD] = 8;                    // 大盤趨勢，表示週線 MACD 值穿越信號線(9期MACD平均線)
            longTermScoreValueArray[STOCK_MONTH_MACD] = 16;                  // 大盤趨勢，表示月線 MACD 值穿越信號線(9期MACD平均線)
            longTermScoreValueArray[STOCK_DAY_RSI5_30] = 4;                  // 大盤趨勢，表示日線 RSI5 值小於 30
            longTermScoreValueArray[STOCK_DAY_RSI5_20] = 4;                  // 大盤趨勢，表示日線 RSI5 值小於 20
            longTermScoreValueArray[STOCK_DAY_RSI5_10] = 4;                  // 大盤趨勢，表示日線 RSI5 值小於 10
            longTermScoreValueArray[STOCK_WEEK_RSI5_30] = 4;                 // 大盤趨勢，表示週線 RSI5 值小於 30
            longTermScoreValueArray[STOCK_WEEK_RSI5_20] = 8;                 // 大盤趨勢，表示週線 RSI5 值小於 20
            longTermScoreValueArray[STOCK_WEEK_RSI5_10] = 16;                // 大盤趨勢，表示週線 RSI5 值小於 10
            longTermScoreValueArray[STOCK_MONTH_RSI5_30] = 8;                // 大盤趨勢，表示月線 RSI5 值小於 30
            longTermScoreValueArray[STOCK_MONTH_RSI5_20] = 16;               // 大盤趨勢，表示月線 RSI5 值小於 20
            longTermScoreValueArray[STOCK_MONTH_RSI5_10] = 32;               // 大盤趨勢，表示月線 RSI5 值小於 10
            longTermScoreValueArray[STOCK_DAY_RSI5_THROUGH_RSI10] = 4;       // 大盤趨勢，表示日線 RSI5 由下穿越 RSI10
            longTermScoreValueArray[STOCK_WEEK_RSI5_THROUGH_RSI10] = 8;      // 大盤趨勢，表示週線 RSI5 由下穿越 RSI10
            longTermScoreValueArray[STOCK_MONTH_RSI5_THROUGH_RSI10] = 16;    // 大盤趨勢，表示月線 RSI5 由下穿越 RSI10
            longTermScoreValueArray[STOCK_DAY_ABI10MA] = 4;                  // 大盤趨勢，表示日線 ABI10MA 大於 40%
            longTermScoreValueArray[STOCK_WEEK_ABI10MA] = 8;                 // 大盤趨勢，表示週線 ABI10MA 大於 40%
            longTermScoreValueArray[STOCK_MONTH_ABI10MA] = 16;               // 大盤趨勢，表示月線 ABI10MA 大於 40%
            longTermScoreValueArray[STOCK_DAY_ADL20MA] = 16;                 // 大盤趨勢，表示日線 ADL20MA 斜率為正
            longTermScoreValueArray[STOCK_WEEK_ADL20MA] = 4;                 // 大盤趨勢，表示週線 ADL20MA 斜率為正
            longTermScoreValueArray[STOCK_MONTH_ADL20MA] = 4;                // 大盤趨勢，表示月線 ADL20MA 斜率為正

            longTermScoreValueArray[COMPANY_TREND_UP] = 16;                  // 各股趨勢，多頭排列
            longTermScoreValueArray[COMPANY_DAY_TURN_UP] = 16;               // 各股趨勢，表示日線由空轉多
            longTermScoreValueArray[COMPANY_WEEK_TURN_UP] = 16;              // 各股趨勢，表示週線由空轉多
            longTermScoreValueArray[COMPANY_MONTH_TURN_UP] = 16;             // 各股趨勢，表示月線由空轉多
            longTermScoreValueArray[COMPANY_DAY_K20] = 2;                    // 各股趨勢，表示日線 K 值低於 20
            longTermScoreValueArray[COMPANY_WEEK_K20] = 4;                   // 各股趨勢，表示週線 K 值低於 20
            longTermScoreValueArray[COMPANY_MONTH_K20] = 8;                  // 各股趨勢，表示月線 K 值低於 20
            longTermScoreValueArray[COMPANY_DAY_K15] = 4;                    // 各股趨勢，表示日線 K 值低於 15
            longTermScoreValueArray[COMPANY_WEEK_K15] = 8;                   // 各股趨勢，表示週線 K 值低於 15
            longTermScoreValueArray[COMPANY_MONTH_K15] = 16;                 // 各股趨勢，表示月線 K 值低於 15
            longTermScoreValueArray[COMPANY_DAY_K10] = 8;                    // 各股趨勢，表示日線 K 值低於 10
            longTermScoreValueArray[COMPANY_WEEK_K10] = 16;                  // 各股趨勢，表示週線 K 值低於 10
            longTermScoreValueArray[COMPANY_MONTH_K10] = 32;                 // 各股趨勢，表示月線 K 值低於 10
            longTermScoreValueArray[COMPANY_DAY_K5] = 16;                    // 各股趨勢，表示日線 K 值低於 5
            longTermScoreValueArray[COMPANY_WEEK_K5] = 32;                   // 各股趨勢，表示週線 K 值低於 5
            longTermScoreValueArray[COMPANY_MONTH_K5] = 64;                  // 各股趨勢，表示月線 K 值低於 5
            longTermScoreValueArray[COMPANY_DAY_THROUGH_K20] = 1;            // 各股趨勢，表示日線 K 值由低於 20 穿越過，變成高於 20
            longTermScoreValueArray[COMPANY_WEEK_THROUGH_K20] = 2;           // 各股趨勢，表示週線 K 值由低於 20 穿越過，變成高於 20
            longTermScoreValueArray[COMPANY_MONTH_THROUGH_K20] = 4;          // 各股趨勢，表示月線 K 值由低於 20 穿越過，變成高於 20
            longTermScoreValueArray[COMPANY_DAY_THROUGH_K15] = 2;            // 各股趨勢，表示日線 K 值由低於 15 穿越過，變成高於 15
            longTermScoreValueArray[COMPANY_WEEK_THROUGH_K15] = 4;           // 各股趨勢，表示週線 K 值由低於 15 穿越過，變成高於 15
            longTermScoreValueArray[COMPANY_MONTH_THROUGH_K15] = 8;          // 各股趨勢，表示月線 K 值由低於 15 穿越過，變成高於 15
            longTermScoreValueArray[COMPANY_DAY_THROUGH_K10] = 4;            // 各股趨勢，表示日線 K 值由低於 10 穿越過，變成高於 10
            longTermScoreValueArray[COMPANY_WEEK_THROUGH_K10] = 8;           // 各股趨勢，表示週線 K 值由低於 10 穿越過，變成高於 10
            longTermScoreValueArray[COMPANY_MONTH_THROUGH_K10] = 16;         // 各股趨勢，表示月線 K 值由低於 10 穿越過，變成高於 10
            longTermScoreValueArray[COMPANY_DAY_THROUGH_K5] = 8;             // 各股趨勢，表示日線 K 值由低於 5 穿越過，變成高於 5
            longTermScoreValueArray[COMPANY_WEEK_THROUGH_K5] = 16;           // 各股趨勢，表示週線 K 值由低於 5 穿越過，變成高於 5
            longTermScoreValueArray[COMPANY_MONTH_THROUGH_K5] = 32;          // 各股趨勢，表示月線 K 值由低於 5 穿越過，變成高於 5
            longTermScoreValueArray[COMPANY_DAY_K_THROUGH_D] = 2;            // 各股趨勢，表示日線 K 值由低於 D 值穿越，變成高於 D 值
            longTermScoreValueArray[COMPANY_WEEK_K_THROUGH_D] = 8;           // 各股趨勢，表示週線 K 值由低於 D 值穿越，變成高於 D 值
            longTermScoreValueArray[COMPANY_MONTH_K_THROUGH_D] = 32;         // 各股趨勢，表示月線 K 值由低於 D 值穿越，變成高於 D 值
            longTermScoreValueArray[COMPANY_CAPITAL_1_10] = 32;              // 各股趨勢，表示資本額排名是 1 到 10 名
            longTermScoreValueArray[COMPANY_CAPITAL_11_20] = 16;             // 各股趨勢，表示資本額排名是 11 到 20 名
            longTermScoreValueArray[COMPANY_CAPITAL_21_30] = 8;              // 各股趨勢，表示資本額排名是 21 到 30 名
            longTermScoreValueArray[COMPANY_CAPITAL_31_40] = 4;              // 各股趨勢，表示資本額排名是 31 到 40 名
            longTermScoreValueArray[COMPANY_CAPITAL_41_50] = 2;              // 各股趨勢，表示資本額排名是 41 到 50 名
            longTermScoreValueArray[COMPANY_DAY_MACD] = 4;                   // 各股趨勢，表示日線 MACD 值穿越信號線(9期MACD平均線)
            longTermScoreValueArray[COMPANY_WEEK_MACD] = 8;                  // 各股趨勢，表示週線 MACD 值穿越信號線(9期MACD平均線)
            longTermScoreValueArray[COMPANY_MONTH_MACD] = 16;                // 各股趨勢，表示月線 MACD 值穿越信號線(9期MACD平均線)
            longTermScoreValueArray[COMPANY_DAY_RSI5_30] = 4;                // 各股趨勢，表示日線 RSI5 值小於 30
            longTermScoreValueArray[COMPANY_DAY_RSI5_20] = 4;                // 各股趨勢，表示日線 RSI5 值小於 20
            longTermScoreValueArray[COMPANY_DAY_RSI5_10] = 4;                // 各股趨勢，表示日線 RSI5 值小於 10
            longTermScoreValueArray[COMPANY_WEEK_RSI5_30] = 4;               // 各股趨勢，表示週線 RSI5 值小於 30
            longTermScoreValueArray[COMPANY_WEEK_RSI5_20] = 8;               // 各股趨勢，表示週線 RSI5 值小於 20
            longTermScoreValueArray[COMPANY_WEEK_RSI5_10] = 16;              // 各股趨勢，表示週線 RSI5 值小於 10
            longTermScoreValueArray[COMPANY_MONTH_RSI5_30] = 8;              // 各股趨勢，表示月線 RSI5 值小於 30
            longTermScoreValueArray[COMPANY_MONTH_RSI5_20] = 16;             // 各股趨勢，表示月線 RSI5 值小於 20
            longTermScoreValueArray[COMPANY_MONTH_RSI5_10] = 32;             // 各股趨勢，表示月線 RSI5 值小於 10
            longTermScoreValueArray[COMPANY_DAY_RSI5_THROUGH_RSI10] = 4;     // 各股趨勢，表示日線 RSI5 由下穿越 RSI10
            longTermScoreValueArray[COMPANY_WEEK_RSI5_THROUGH_RSI10] = 8;    // 各股趨勢，表示週線 RSI5 由下穿越 RSI10
            longTermScoreValueArray[COMPANY_MONTH_RSI5_THROUGH_RSI10] = 16;  // 各股趨勢，表示月線 RSI5 由下穿越 RSI10
            /*
              scoreTextArray 陣列中的值是代表
                某個技術指標到達某個條件時，所給出分數的原因(字串解釋)
              剛開始此數值是在程式碼中直接給定評分值，將來可以用歷史記錄
              分析最適當的評分值，也就是找尋讓股票獲利最大化的評分值陣列。
              <<某個技術指標到達某個條件>>代表的是一股勢力，也就是一群使用
              這種方式來購買股票的人，所集合起來的能量，所有的能量集合起來
              就是整個股票上漲的能量。
            */
            scoreTextArray[STOCK_TREND_UP] = "大盤，多頭排列";
            scoreTextArray[STOCK_DAY_TURN_UP] = "大盤，日線由空轉多";
            scoreTextArray[STOCK_WEEK_TURN_UP] = "大盤，週線由空轉多";
            scoreTextArray[STOCK_MONTH_TURN_UP] = "大盤，月線由空轉多";
            scoreTextArray[STOCK_DAY_K20] = "大盤，日線 K 值低於 20";
            scoreTextArray[STOCK_WEEK_K20] = "大盤，週線 K 值低於 20";
            scoreTextArray[STOCK_MONTH_K20] = "大盤，月線 K 值低於 20";
            scoreTextArray[STOCK_DAY_K15] = "大盤，日線 K 值低於 15";
            scoreTextArray[STOCK_WEEK_K15] = "大盤，週線 K 值低於 15";
            scoreTextArray[STOCK_MONTH_K15] = "大盤，月線 K 值低於 15";
            scoreTextArray[STOCK_DAY_K10] = "大盤，日線 K 值低於 10";
            scoreTextArray[STOCK_WEEK_K10] = "大盤，週線 K 值低於 10";
            scoreTextArray[STOCK_MONTH_K10] = "大盤，月線 K 值低於 10";
            scoreTextArray[STOCK_DAY_K5] = "大盤，日線 K 值低於 5";
            scoreTextArray[STOCK_WEEK_K5] = "大盤，週線 K 值低於 5";
            scoreTextArray[STOCK_MONTH_K5] = "大盤，月線 K 值低於 5";
            scoreTextArray[STOCK_DAY_THROUGH_K20] = "大盤，日線 K 值由低於 20 穿越過，變成高於 20";
            scoreTextArray[STOCK_WEEK_THROUGH_K20] = "大盤，週線 K 值由低於 20 穿越過，變成高於 20";
            scoreTextArray[STOCK_MONTH_THROUGH_K20] = "大盤，月線 K 值由低於 20 穿越過，變成高於 20";
            scoreTextArray[STOCK_DAY_THROUGH_K15] = "大盤，日線 K 值由低於 15 穿越過，變成高於 15";
            scoreTextArray[STOCK_WEEK_THROUGH_K15] = "大盤，週線 K 值由低於 15 穿越過，變成高於 15";
            scoreTextArray[STOCK_MONTH_THROUGH_K15] = "大盤，月線 K 值由低於 15 穿越過，變成高於 15";
            scoreTextArray[STOCK_DAY_THROUGH_K10] = "大盤，日線 K 值由低於 10 穿越過，變成高於 10";
            scoreTextArray[STOCK_WEEK_THROUGH_K10] = "大盤，週線 K 值由低於 10 穿越過，變成高於 10";
            scoreTextArray[STOCK_MONTH_THROUGH_K10] = "大盤，月線 K 值由低於 10 穿越過，變成高於 10";
            scoreTextArray[STOCK_DAY_THROUGH_K5] = "大盤，日線 K 值由低於 5 穿越過，變成高於 5";
            scoreTextArray[STOCK_WEEK_THROUGH_K5] = "大盤，週線 K 值由低於 5 穿越過，變成高於 5";
            scoreTextArray[STOCK_MONTH_THROUGH_K5] = "大盤，月線 K 值由低於 5 穿越過，變成高於 5";
            scoreTextArray[STOCK_DAY_K_THROUGH_D] = "大盤，日線 K 值由低於 D 值穿越，變成高於 D 值";
            scoreTextArray[STOCK_WEEK_K_THROUGH_D] = "大盤，週線 K 值由低於 D 值穿越，變成高於 D 值";
            scoreTextArray[STOCK_MONTH_K_THROUGH_D] = "大盤，月線 K 值由低於 D 值穿越，變成高於 D 值";
            scoreTextArray[STOCK_DAY_MACD] = "大盤，日線 MACD 值穿越信號線(9期MACD平均線)";
            scoreTextArray[STOCK_WEEK_MACD] = "大盤，週線 MACD 值穿越信號線(9期MACD平均線)";
            scoreTextArray[STOCK_MONTH_MACD] = "大盤，月線 MACD 值穿越信號線(9期MACD平均線)";
            scoreTextArray[STOCK_DAY_RSI5_30] = "大盤，日線 RSI5 值小於 30";
            scoreTextArray[STOCK_DAY_RSI5_20] = "大盤，日線 RSI5 值小於 20";
            scoreTextArray[STOCK_DAY_RSI5_10] = "大盤，日線 RSI5 值小於 10";
            scoreTextArray[STOCK_WEEK_RSI5_30] = "大盤，週線 RSI5 值小於 30";
            scoreTextArray[STOCK_WEEK_RSI5_20] = "大盤，週線 RSI5 值小於 20";
            scoreTextArray[STOCK_WEEK_RSI5_10] = "大盤，週線 RSI5 值小於 10";
            scoreTextArray[STOCK_MONTH_RSI5_30] = "大盤，月線 RSI5 值小於 30";
            scoreTextArray[STOCK_MONTH_RSI5_20] = "大盤，月線 RSI5 值小於 20";
            scoreTextArray[STOCK_MONTH_RSI5_10] = "大盤，月線 RSI5 值小於 10";
            scoreTextArray[STOCK_DAY_RSI5_THROUGH_RSI10] = "大盤，日線 RSI5 由下穿越 RSI10";
            scoreTextArray[STOCK_WEEK_RSI5_THROUGH_RSI10] = "大盤，週線 RSI5 由下穿越 RSI10";
            scoreTextArray[STOCK_MONTH_RSI5_THROUGH_RSI10] = "大盤，月線 RSI5 由下穿越 RSI10";
            scoreTextArray[STOCK_DAY_ABI10MA] = "大盤，日線 ABI10MA 大於 40%";
            scoreTextArray[STOCK_WEEK_ABI10MA] = "大盤，週線 ABI10MA 大於 40%";
            scoreTextArray[STOCK_MONTH_ABI10MA] = "大盤，月線 ABI10MA 大於 40%";
            scoreTextArray[STOCK_DAY_ADL20MA] = "大盤，日線 ADL20MA 斜率為正";
            scoreTextArray[STOCK_WEEK_ADL20MA] = "大盤，週線 ADL20MA 斜率為正";
            scoreTextArray[STOCK_MONTH_ADL20MA] = "大盤，月線 ADL20MA 斜率為正";

            scoreTextArray[COMPANY_TREND_UP] = "此股，多頭排列";
            scoreTextArray[COMPANY_DAY_TURN_UP] = "此股，日線由空轉多";
            scoreTextArray[COMPANY_WEEK_TURN_UP] = "此股，週線由空轉多";
            scoreTextArray[COMPANY_MONTH_TURN_UP] = "此股，月線由空轉多";
            scoreTextArray[COMPANY_DAY_K20] = "此股，日線 K 值低於 20";
            scoreTextArray[COMPANY_WEEK_K20] = "此股，週線 K 值低於 20";
            scoreTextArray[COMPANY_MONTH_K20] = "此股，月線 K 值低於 20";
            scoreTextArray[COMPANY_DAY_K15] = "此股，日線 K 值低於 15";
            scoreTextArray[COMPANY_WEEK_K15] = "此股，週線 K 值低於 15";
            scoreTextArray[COMPANY_MONTH_K15] = "此股，月線 K 值低於 15";
            scoreTextArray[COMPANY_DAY_K10] = "此股，日線 K 值低於 10";
            scoreTextArray[COMPANY_WEEK_K10] = "此股，週線 K 值低於 10";
            scoreTextArray[COMPANY_MONTH_K10] = "此股，月線 K 值低於 10";
            scoreTextArray[COMPANY_DAY_K5] = "此股，日線 K 值低於 5";
            scoreTextArray[COMPANY_WEEK_K5] = "此股，週線 K 值低於 5";
            scoreTextArray[COMPANY_MONTH_K5] = "此股，月線 K 值低於 5";
            scoreTextArray[COMPANY_DAY_THROUGH_K20] = "此股，日線 K 值由低於 20 穿越過，變成高於 20";
            scoreTextArray[COMPANY_WEEK_THROUGH_K20] = "此股，週線 K 值由低於 20 穿越過，變成高於 20";
            scoreTextArray[COMPANY_MONTH_THROUGH_K20] = "此股，月線 K 值由低於 20 穿越過，變成高於 20";
            scoreTextArray[COMPANY_DAY_THROUGH_K15] = "此股，日線 K 值由低於 15 穿越過，變成高於 15";
            scoreTextArray[COMPANY_WEEK_THROUGH_K15] = "此股，週線 K 值由低於 15 穿越過，變成高於 15";
            scoreTextArray[COMPANY_MONTH_THROUGH_K15] = "此股，月線 K 值由低於 15 穿越過，變成高於 15";
            scoreTextArray[COMPANY_DAY_THROUGH_K10] = "此股，日線 K 值由低於 10 穿越過，變成高於 10";
            scoreTextArray[COMPANY_WEEK_THROUGH_K10] = "此股，週線 K 值由低於 10 穿越過，變成高於 10";
            scoreTextArray[COMPANY_MONTH_THROUGH_K10] = "此股，月線 K 值由低於 10 穿越過，變成高於 10";
            scoreTextArray[COMPANY_DAY_THROUGH_K5] = "此股，日線 K 值由低於 5 穿越過，變成高於 5";
            scoreTextArray[COMPANY_WEEK_THROUGH_K5] = "此股，週線 K 值由低於 5 穿越過，變成高於 5";
            scoreTextArray[COMPANY_MONTH_THROUGH_K5] = "此股，月線 K 值由低於 5 穿越過，變成高於 5";
            scoreTextArray[COMPANY_DAY_K_THROUGH_D] = "此股，日線 K 值由低於 D 值穿越，變成高於 D 值";
            scoreTextArray[COMPANY_WEEK_K_THROUGH_D] = "此股，週線 K 值由低於 D 值穿越，變成高於 D 值";
            scoreTextArray[COMPANY_MONTH_K_THROUGH_D] = "此股，月線 K 值由低於 D 值穿越，變成高於 D 值";
            scoreTextArray[COMPANY_CAPITAL_1_10] = "此股，資本額排名是 1 到 10 名";
            scoreTextArray[COMPANY_CAPITAL_11_20] = "此股，資本額排名是 11 到 20 名";
            scoreTextArray[COMPANY_CAPITAL_21_30] = "此股，資本額排名是 21 到 30 名";
            scoreTextArray[COMPANY_CAPITAL_31_40] = "此股，資本額排名是 31 到 40 名";
            scoreTextArray[COMPANY_CAPITAL_41_50] = "此股，資本額排名是 41 到 50 名";
            scoreTextArray[COMPANY_DAY_MACD] = "此股，日線 MACD 值穿越信號線(9期MACD平均線)";
            scoreTextArray[COMPANY_WEEK_MACD] = "此股，週線 MACD 值穿越信號線(9期MACD平均線)";
            scoreTextArray[COMPANY_MONTH_MACD] = "此股，月線 MACD 值穿越信號線(9期MACD平均線)";
            scoreTextArray[COMPANY_DAY_RSI5_30] = "此股，日線 RSI5 值小於 30";
            scoreTextArray[COMPANY_DAY_RSI5_20] = "此股，日線 RSI5 值小於 20";
            scoreTextArray[COMPANY_DAY_RSI5_10] = "此股，日線 RSI5 值小於 10";
            scoreTextArray[COMPANY_WEEK_RSI5_30] = "此股，週線 RSI5 值小於 30";
            scoreTextArray[COMPANY_WEEK_RSI5_20] = "此股，週線 RSI5 值小於 20";
            scoreTextArray[COMPANY_WEEK_RSI5_10] = "此股，週線 RSI5 值小於 10";
            scoreTextArray[COMPANY_MONTH_RSI5_30] = "此股，月線 RSI5 值小於 30";
            scoreTextArray[COMPANY_MONTH_RSI5_20] = "此股，月線 RSI5 值小於 20";
            scoreTextArray[COMPANY_MONTH_RSI5_10] = "此股，月線 RSI5 值小於 10";
            scoreTextArray[COMPANY_DAY_RSI5_THROUGH_RSI10] = "此股，日線 RSI5 由下穿越 RSI10";
            scoreTextArray[COMPANY_WEEK_RSI5_THROUGH_RSI10] = "此股，週線 RSI5 由下穿越 RSI10";
            scoreTextArray[COMPANY_MONTH_RSI5_THROUGH_RSI10] = "此股，月線 RSI5 由下穿越 RSI10";
            /*
             * 以下程式初始化大盤及各公司評分列表
             * 評分列表中將記錄該公司所得到的項目之 Index，
             * 例如若大盤日線 K 值低於 20，則在列表中加入一整數 STOCK_DAY_K20
             * 大盤的所有評分加總可以由此列表中的 Index，由
             * scoreValueArray[Index] 取得評分來加總。
             */
            stockDatabase.scoreIndexList = new List<int>();
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                stockDatabase.companies[i].scoreIndexList = new List<int>();
            }
        }
        /* 
          函式 stockKDJAnalysis 根據大盤的 KD 技術指標做評估，
          並將結果 push 到 stockDatabase.scoreArray 陣列中。
        */
        public void stockKDJAnalysis(StockIndicator[] dayIndicatorArray,
            StockIndicator[] weekIndicatorArray,
            StockIndicator[] monthIndicatorArray)
        {
            var length = dayIndicatorArray.Length;
            /* 日線 KD 值評估 */
            if (dayIndicatorArray[length - 1].K < 20)
            {
                stockDatabase.scoreIndexList.Add(STOCK_DAY_K20);
            }
            if (dayIndicatorArray[length - 1].K < 15)
            {
                stockDatabase.scoreIndexList.Add(STOCK_DAY_K15);
            }
            if (dayIndicatorArray[length - 1].K < 10)
            {
                stockDatabase.scoreIndexList.Add(STOCK_DAY_K10);
            }
            if (dayIndicatorArray[length - 1].K < 5)
            {
                stockDatabase.scoreIndexList.Add(STOCK_DAY_K5);
            }
            if (length > 2)
            {
                if (dayIndicatorArray[length - 1].K <= 15)
                {
                    if ((dayIndicatorArray[length - 2].K <= dayIndicatorArray[length - 2].D) &&
                      (dayIndicatorArray[length - 1].K >= dayIndicatorArray[length - 1].D))
                    {
                        stockDatabase.scoreIndexList.Add(STOCK_DAY_K_THROUGH_D);
                    }
                }
                if ((dayIndicatorArray[length - 2].K <= 20) && (dayIndicatorArray[length - 1].K >= 20))
                {
                    stockDatabase.scoreIndexList.Add(STOCK_DAY_THROUGH_K20);
                }
                if ((dayIndicatorArray[length - 2].K <= 15) && (dayIndicatorArray[length - 1].K >= 15))
                {
                    stockDatabase.scoreIndexList.Add(STOCK_DAY_THROUGH_K15);
                }
                if ((dayIndicatorArray[length - 2].K <= 10) && (dayIndicatorArray[length - 1].K >= 10))
                {
                    stockDatabase.scoreIndexList.Add(STOCK_DAY_THROUGH_K10);
                }
                if ((dayIndicatorArray[length - 2].K <= 5) && (dayIndicatorArray[length - 1].K >= 5))
                {
                    stockDatabase.scoreIndexList.Add(STOCK_DAY_THROUGH_K5);
                }
                /* 週線 KD 值評估 */
                length = weekIndicatorArray.Length;
                if (weekIndicatorArray[length - 1].K < 20)
                {
                    stockDatabase.scoreIndexList.Add(STOCK_WEEK_K20);
                }
                if (weekIndicatorArray[length - 1].K < 15)
                {
                    stockDatabase.scoreIndexList.Add(STOCK_WEEK_K15);
                }
                if (weekIndicatorArray[length - 1].K < 10)
                {
                    stockDatabase.scoreIndexList.Add(STOCK_WEEK_K10);
                }
                if (weekIndicatorArray[length - 1].K < 5)
                {
                    stockDatabase.scoreIndexList.Add(STOCK_WEEK_K5);
                }
                if (length > 2)
                {
                    if (weekIndicatorArray[length - 1].K <= 15)
                    {
                        if ((weekIndicatorArray[length - 2].K <= weekIndicatorArray[length - 2].D) &&
                          (weekIndicatorArray[length - 1].K >= weekIndicatorArray[length - 1].D))
                        {
                            stockDatabase.scoreIndexList.Add(STOCK_WEEK_K_THROUGH_D);
                        }
                    }
                    if ((weekIndicatorArray[length - 2].K <= 20) && (weekIndicatorArray[length - 1].K >= 20))
                    {
                        stockDatabase.scoreIndexList.Add(STOCK_WEEK_THROUGH_K20);
                    }
                    if ((weekIndicatorArray[length - 2].K <= 15) && (weekIndicatorArray[length - 1].K >= 15))
                    {
                        stockDatabase.scoreIndexList.Add(STOCK_WEEK_THROUGH_K15);
                    }
                    if ((weekIndicatorArray[length - 2].K <= 10) && (weekIndicatorArray[length - 1].K >= 10))
                    {
                        stockDatabase.scoreIndexList.Add(STOCK_WEEK_THROUGH_K10);
                    }
                    if ((weekIndicatorArray[length - 2].K <= 5) && (weekIndicatorArray[length - 1].K >= 5))
                    {
                        stockDatabase.scoreIndexList.Add(STOCK_WEEK_THROUGH_K5);
                    }
                }
                /* 月線 KD 值評估 */
                length = monthIndicatorArray.Length;
                if (monthIndicatorArray[length - 1].K < 20)
                {
                    stockDatabase.scoreIndexList.Add(STOCK_MONTH_K20);
                }
                if (monthIndicatorArray[length - 1].K < 15)
                {
                    stockDatabase.scoreIndexList.Add(STOCK_MONTH_K15);
                }
                if (monthIndicatorArray[length - 1].K < 10)
                {
                    stockDatabase.scoreIndexList.Add(STOCK_MONTH_K10);
                }
                if (monthIndicatorArray[length - 1].K < 5)
                {
                    stockDatabase.scoreIndexList.Add(STOCK_MONTH_K5);
                }
                if (length > 2)
                {
                    if (monthIndicatorArray[length - 1].K <= 15)
                    {
                        if ((monthIndicatorArray[length - 2].K <= monthIndicatorArray[length - 2].D) &&
                          (monthIndicatorArray[length - 1].K >= monthIndicatorArray[length - 1].D))
                        {
                            stockDatabase.scoreIndexList.Add(STOCK_MONTH_K_THROUGH_D);
                        }
                    }
                    if ((monthIndicatorArray[length - 2].K <= 20) && (monthIndicatorArray[length - 1].K >= 20))
                    {
                        stockDatabase.scoreIndexList.Add(STOCK_MONTH_THROUGH_K20);
                    }
                    if ((monthIndicatorArray[length - 2].K <= 15) && (monthIndicatorArray[length - 1].K >= 15))
                    {
                        stockDatabase.scoreIndexList.Add(STOCK_MONTH_THROUGH_K15);
                    }
                    if ((monthIndicatorArray[length - 2].K <= 10) && (monthIndicatorArray[length - 1].K >= 10))
                    {
                        stockDatabase.scoreIndexList.Add(STOCK_MONTH_THROUGH_K10);
                    }
                    if ((monthIndicatorArray[length - 2].K <= 5) && (monthIndicatorArray[length - 1].K >= 5))
                    {
                        stockDatabase.scoreIndexList.Add(STOCK_MONTH_THROUGH_K5);
                    }
                }
            }
        }
        /* 函式 MA 用來計算 N 天的股價平均值 
         *      傳入參數 :
         *          historyDataArray : 要計算平均值的股價陣列
         *          N : N 天股價平均值
         *          index : 計算股價陣列中第 index 筆(天)資料的平均值
         */
        public Double MA(HistoryData[] historyDataArray, int N, int index)
        {
            if (index < N)
            {
                return 0.0;
            }
            var ma = 0.0;
            for (var i = index; i > index - N; i--)
            {
                ma = ma + historyDataArray[i].c;
            }
            ma = ma / N;
            return ma;
        }
        /* 
         * 函式 stockTrendAnalysis 根據大盤的 N 天平均技術指標，找出是否為多頭排列 
         */
        public void stockTrendAnalysis()
        {
            var historyDataArray = stockDatabase.getRealOldHistoryData("d");
            var MA5 = MA(historyDataArray, 5, historyDataArray.Length - 1);
            var MA20 = MA(historyDataArray, 20, historyDataArray.Length - 1);
            var MA60 = MA(historyDataArray, 60, historyDataArray.Length - 1);
            if ((MA5 > MA20) && (MA20 > MA60))
            {
                stockDatabase.scoreIndexList.Add(STOCK_TREND_UP);
            }
            var prev1MA5 = MA(historyDataArray, 5, historyDataArray.Length - 2);
            var prev2MA5 = MA(historyDataArray, 5, historyDataArray.Length - 3);
            var prev3MA5 = MA(historyDataArray, 5, historyDataArray.Length - 4);
            var prev4MA5 = MA(historyDataArray, 5, historyDataArray.Length - 5);
            if ((prev4MA5 > prev3MA5) && (prev3MA5 > prev2MA5) && (prev2MA5 > prev1MA5) && (prev1MA5 < MA5))
            {
                stockDatabase.scoreIndexList.Add(STOCK_DAY_TURN_UP);
            }
            historyDataArray = stockDatabase.getRealOldHistoryData("w");
            prev1MA5 = MA(historyDataArray, 5, historyDataArray.Length - 2);
            prev2MA5 = MA(historyDataArray, 5, historyDataArray.Length - 3);
            prev3MA5 = MA(historyDataArray, 5, historyDataArray.Length - 4);
            prev4MA5 = MA(historyDataArray, 5, historyDataArray.Length - 5);
            if ((prev4MA5 > prev3MA5) && (prev3MA5 > prev2MA5) && (prev2MA5 > prev1MA5) && (prev1MA5 < MA5))
            {
                stockDatabase.scoreIndexList.Add(STOCK_WEEK_TURN_UP);
            }
            historyDataArray = stockDatabase.getRealOldHistoryData("m");
            prev1MA5 = MA(historyDataArray, 5, historyDataArray.Length - 2);
            prev2MA5 = MA(historyDataArray, 5, historyDataArray.Length - 3);
            prev3MA5 = MA(historyDataArray, 5, historyDataArray.Length - 4);
            prev4MA5 = MA(historyDataArray, 5, historyDataArray.Length - 5);
            if ((prev4MA5 > prev3MA5) && (prev3MA5 > prev2MA5) && (prev2MA5 > prev1MA5) && (prev1MA5 < MA5))
            {
                stockDatabase.scoreIndexList.Add(STOCK_MONTH_TURN_UP);
            }
        }
        /* 
         * 函式 stockMACDAnalysys 用來分析大盤 MACD 線是否由下向上穿越信號線 
         */
        public void stockMACDAnalysys(StockIndicator[] dayIndicatorArray,
            StockIndicator[] weekIndicatorArray,
            StockIndicator[] monthIndicatorArray)
        {
            var length = dayIndicatorArray.Length;
            if (length > 2)
            {
                if ((dayIndicatorArray[length - 2].MACD <= dayIndicatorArray[length - 2].MACD9MA) &&
                  (dayIndicatorArray[length - 1].MACD > dayIndicatorArray[length - 1].MACD9MA))
                {
                    stockDatabase.scoreIndexList.Add(STOCK_DAY_MACD);
                }
            }
            length = weekIndicatorArray.Length;
            if (length > 2)
            {
                if ((weekIndicatorArray[length - 2].MACD <= weekIndicatorArray[length - 2].MACD9MA) &&
                  (weekIndicatorArray[length - 1].MACD > weekIndicatorArray[length - 1].MACD9MA))
                {
                    stockDatabase.scoreIndexList.Add(STOCK_WEEK_MACD);
                }
            }
            length = monthIndicatorArray.Length;
            if (length > 2)
            {
                if ((monthIndicatorArray[length - 2].MACD <= monthIndicatorArray[length - 2].MACD9MA) &&
                  (monthIndicatorArray[length - 1].MACD > monthIndicatorArray[length - 1].MACD9MA))
                {
                    stockDatabase.scoreIndexList.Add(STOCK_MONTH_MACD);
                }
            }
        }
        /* 
         * 函式 stockRSIAnalysis 用來分析大盤 RSI 技術指標 
         */
        public void stockRSIAnalysis(StockIndicator[] dayIndicatorArray,
            StockIndicator[] weekIndicatorArray,
            StockIndicator[] monthIndicatorArray)
        {
            var length = dayIndicatorArray.Length;
            if (dayIndicatorArray[length - 1].RSI5 < 30)
            {
                stockDatabase.scoreIndexList.Add(STOCK_DAY_RSI5_30);
            }
            if (dayIndicatorArray[length - 1].RSI5 < 20)
            {
                stockDatabase.scoreIndexList.Add(STOCK_DAY_RSI5_20);
            }
            if (dayIndicatorArray[length - 1].RSI5 < 10)
            {
                stockDatabase.scoreIndexList.Add(STOCK_DAY_RSI5_10);
            }
            if (length > 2)
            {
                if ((dayIndicatorArray[length - 2].RSI5 <= dayIndicatorArray[length - 2].RSI10) &&
                  (dayIndicatorArray[length - 1].RSI5 > dayIndicatorArray[length - 1].RSI10))
                {
                    stockDatabase.scoreIndexList.Add(STOCK_DAY_RSI5_THROUGH_RSI10);
                }
            }
            length = weekIndicatorArray.Length;
            if (weekIndicatorArray[length - 1].RSI5 < 30)
            {
                stockDatabase.scoreIndexList.Add(STOCK_WEEK_RSI5_30);
            }
            if (weekIndicatorArray[length - 1].RSI5 < 20)
            {
                stockDatabase.scoreIndexList.Add(STOCK_WEEK_RSI5_20);
            }
            if (weekIndicatorArray[length - 1].RSI5 < 10)
            {
                stockDatabase.scoreIndexList.Add(STOCK_WEEK_RSI5_10);
            }
            if (length > 2)
            {
                if ((weekIndicatorArray[length - 2].RSI5 <= weekIndicatorArray[length - 2].RSI10) &&
                  (weekIndicatorArray[length - 1].RSI5 > weekIndicatorArray[length - 1].RSI10))
                {
                    stockDatabase.scoreIndexList.Add(STOCK_WEEK_RSI5_THROUGH_RSI10);
                }
            }
            length = monthIndicatorArray.Length;
            if (monthIndicatorArray[length - 1].RSI5 < 30)
            {
                stockDatabase.scoreIndexList.Add(STOCK_MONTH_RSI5_30);
            }
            if (monthIndicatorArray[length - 1].RSI5 < 20)
            {
                stockDatabase.scoreIndexList.Add(STOCK_MONTH_RSI5_20);
            }
            if (monthIndicatorArray[length - 1].RSI5 < 10)
            {
                stockDatabase.scoreIndexList.Add(STOCK_MONTH_RSI5_10);
            }
            if (length > 2)
            {
                if ((monthIndicatorArray[length - 2].RSI5 <= monthIndicatorArray[length - 2].RSI10) &&
                  (monthIndicatorArray[length - 1].RSI5 > monthIndicatorArray[length - 1].RSI10))
                {
                    stockDatabase.scoreIndexList.Add(STOCK_MONTH_RSI5_THROUGH_RSI10);
                }
            }
        }
        /* 
         * 函式 stockAnalysis 根據大盤的技術指標，找出評估分數 
         * 參數 pCount 是計算時要使用幾期資料來計算，預設是 80 期
         */
        public void stockAnalysis(int pCount)
        {
            if (pCount < 80)
            {
                pCount = 80;
            }
            Indicator indicator;
            indicator = new Indicator(stockDatabase.getRealOldHistoryData("d"));
            var dayIndicatorArray = indicator.getStockIndicatorArray(pCount);
            indicator = new Indicator(stockDatabase.getRealOldHistoryData("w"));
            var weekIndicatorArray = indicator.getStockIndicatorArray(pCount);
            indicator = new Indicator(stockDatabase.getRealOldHistoryData("m"));
            var monthIndicatorArray = indicator.getStockIndicatorArray(pCount);
            stockKDJAnalysis(dayIndicatorArray, weekIndicatorArray, monthIndicatorArray);
            stockTrendAnalysis();
            stockMACDAnalysys(dayIndicatorArray, weekIndicatorArray, monthIndicatorArray);
            stockRSIAnalysis(dayIndicatorArray, weekIndicatorArray, monthIndicatorArray);
        }
        /* 
         * 函式 companyKDJAnalysis 根據 company 的 KD 技術指標做評估，
         * 並將結果 push 到 company.scoreArray 陣列中。
         */
        public void companyKDJAnalysis(Company company,
            StockIndicator[] dayIndicatorArray,
            StockIndicator[] weekIndicatorArray,
            StockIndicator[] monthIndicatorArray
            )
        {
            var length = dayIndicatorArray.Length;
            /* 日線 KD 值評估 */
            if (dayIndicatorArray[length - 1].K < 20)
            {
                company.scoreIndexList.Add(COMPANY_DAY_K20);
            }
            if (dayIndicatorArray[length - 1].K < 15)
            {
                company.scoreIndexList.Add(COMPANY_DAY_K15);
            }
            if (dayIndicatorArray[length - 1].K < 10)
            {
                company.scoreIndexList.Add(COMPANY_DAY_K10);
            }
            if (dayIndicatorArray[length - 1].K < 5)
            {
                company.scoreIndexList.Add(COMPANY_DAY_K5);
            }
            if (length > 2)
            {
                if (dayIndicatorArray[length - 1].K <= 15)
                {
                    if ((dayIndicatorArray[length - 2].K <= dayIndicatorArray[length - 2].D) &&
                      (dayIndicatorArray[length - 1].K >= dayIndicatorArray[length - 1].D))
                    {
                        company.scoreIndexList.Add(COMPANY_DAY_K_THROUGH_D);
                    }
                }
                if ((dayIndicatorArray[length - 2].K <= 20) && (dayIndicatorArray[length - 1].K >= 20))
                {
                    company.scoreIndexList.Add(COMPANY_DAY_THROUGH_K20);
                }
                if ((dayIndicatorArray[length - 2].K <= 15) && (dayIndicatorArray[length - 1].K >= 15))
                {
                    company.scoreIndexList.Add(COMPANY_DAY_THROUGH_K15);
                }
                if ((dayIndicatorArray[length - 2].K <= 10) && (dayIndicatorArray[length - 1].K >= 10))
                {
                    company.scoreIndexList.Add(COMPANY_DAY_THROUGH_K10);
                }
                if ((dayIndicatorArray[length - 2].K <= 5) && (dayIndicatorArray[length - 1].K >= 5))
                {
                    company.scoreIndexList.Add(COMPANY_DAY_THROUGH_K5);
                }
            }
            /* 週線 KD 值評估 */
            length = weekIndicatorArray.Length;
            if (weekIndicatorArray[length - 1].K < 20)
            {
                company.scoreIndexList.Add(COMPANY_WEEK_K20);
            }
            if (weekIndicatorArray[length - 1].K < 15)
            {
                company.scoreIndexList.Add(COMPANY_WEEK_K15);
            }
            if (weekIndicatorArray[length - 1].K < 10)
            {
                company.scoreIndexList.Add(COMPANY_WEEK_K10);
            }
            if (weekIndicatorArray[length - 1].K < 5)
            {
                company.scoreIndexList.Add(COMPANY_WEEK_K5);
            }
            if (length > 2)
            {
                if (weekIndicatorArray[length - 1].K <= 15)
                {
                    if ((weekIndicatorArray[length - 2].K <= weekIndicatorArray[length - 2].D) &&
                      (weekIndicatorArray[length - 1].K >= weekIndicatorArray[length - 1].D))
                    {
                        company.scoreIndexList.Add(COMPANY_WEEK_K_THROUGH_D);
                    }
                }
                if ((weekIndicatorArray[length - 2].K <= 20) && (weekIndicatorArray[length - 1].K >= 20))
                {
                    company.scoreIndexList.Add(COMPANY_WEEK_THROUGH_K20);
                }
                if ((weekIndicatorArray[length - 2].K <= 15) && (weekIndicatorArray[length - 1].K >= 15))
                {
                    company.scoreIndexList.Add(COMPANY_WEEK_THROUGH_K15);
                }
                if ((weekIndicatorArray[length - 2].K <= 10) && (weekIndicatorArray[length - 1].K >= 10))
                {
                    company.scoreIndexList.Add(COMPANY_WEEK_THROUGH_K10);
                }
                if ((weekIndicatorArray[length - 2].K <= 5) && (weekIndicatorArray[length - 1].K >= 5))
                {
                    company.scoreIndexList.Add(COMPANY_WEEK_THROUGH_K5);
                }
            }
            /* 月線 KD 值評估 */
            length = monthIndicatorArray.Length;
            if (monthIndicatorArray[length - 1].K < 20)
            {
                company.scoreIndexList.Add(COMPANY_MONTH_K20);
            }
            if (monthIndicatorArray[length - 1].K < 15)
            {
                company.scoreIndexList.Add(COMPANY_MONTH_K15);
            }
            if (monthIndicatorArray[length - 1].K < 10)
            {
                company.scoreIndexList.Add(COMPANY_MONTH_K10);
            }
            if (monthIndicatorArray[length - 1].K < 5)
            {
                company.scoreIndexList.Add(COMPANY_MONTH_K5);
            }
            if (length > 2)
            {
                if (monthIndicatorArray[length - 1].K <= 15)
                {
                    if ((monthIndicatorArray[length - 2].K <= monthIndicatorArray[length - 2].D) &&
                      (monthIndicatorArray[length - 1].K >= monthIndicatorArray[length - 1].D))
                    {
                        company.scoreIndexList.Add(COMPANY_MONTH_K_THROUGH_D);
                    }
                }
                if ((monthIndicatorArray[length - 2].K <= 20) && (monthIndicatorArray[length - 1].K >= 20))
                {
                    company.scoreIndexList.Add(COMPANY_MONTH_THROUGH_K20);
                }
                if ((monthIndicatorArray[length - 2].K <= 15) && (monthIndicatorArray[length - 1].K >= 15))
                {
                    company.scoreIndexList.Add(COMPANY_MONTH_THROUGH_K15);
                }
                if ((monthIndicatorArray[length - 2].K <= 10) && (monthIndicatorArray[length - 1].K >= 10))
                {
                    company.scoreIndexList.Add(COMPANY_MONTH_THROUGH_K10);
                }
                if ((monthIndicatorArray[length - 2].K <= 5) && (monthIndicatorArray[length - 1].K >= 5))
                {
                    company.scoreIndexList.Add(COMPANY_MONTH_THROUGH_K5);
                }
            }
        }
        /* 
         * 函式 companyTrendAnalysis 根據 company 公司的 N 天平均技術指標，找出是否為多頭排列 
         */
        public void companyTrendAnalysis(Company company)
        {
            var historyDataArray = company.getRealHistoryDataArray("d");
            var MA5 = MA(historyDataArray, 5, historyDataArray.Length - 1);
            var MA20 = MA(historyDataArray, 20, historyDataArray.Length - 1);
            var MA60 = MA(historyDataArray, 60, historyDataArray.Length - 1);
            if ((MA5 > MA20) && (MA20 > MA60))
            {
                company.scoreIndexList.Add(COMPANY_TREND_UP);
            }
            var prev1MA5 = MA(historyDataArray, 5, historyDataArray.Length - 2);
            var prev2MA5 = MA(historyDataArray, 5, historyDataArray.Length - 3);
            var prev3MA5 = MA(historyDataArray, 5, historyDataArray.Length - 4);
            var prev4MA5 = MA(historyDataArray, 5, historyDataArray.Length - 5);
            if ((prev4MA5 > prev3MA5) && (prev3MA5 > prev2MA5) && (prev2MA5 > prev1MA5) && (prev1MA5 < MA5))
            {
                company.scoreIndexList.Add(COMPANY_DAY_TURN_UP);
            }
            historyDataArray = company.getRealHistoryDataArray("w");
            prev1MA5 = MA(historyDataArray, 5, historyDataArray.Length - 2);
            prev2MA5 = MA(historyDataArray, 5, historyDataArray.Length - 3);
            prev3MA5 = MA(historyDataArray, 5, historyDataArray.Length - 4);
            prev4MA5 = MA(historyDataArray, 5, historyDataArray.Length - 5);
            if ((prev4MA5 > prev3MA5) && (prev3MA5 > prev2MA5) && (prev2MA5 > prev1MA5) && (prev1MA5 < MA5))
            {
                company.scoreIndexList.Add(COMPANY_WEEK_TURN_UP);
            }
            historyDataArray = company.getRealHistoryDataArray("m");
            prev1MA5 = MA(historyDataArray, 5, historyDataArray.Length - 2);
            prev2MA5 = MA(historyDataArray, 5, historyDataArray.Length - 3);
            prev3MA5 = MA(historyDataArray, 5, historyDataArray.Length - 4);
            prev4MA5 = MA(historyDataArray, 5, historyDataArray.Length - 5);
            if ((prev4MA5 > prev3MA5) && (prev3MA5 > prev2MA5) && (prev2MA5 > prev1MA5) && (prev1MA5 < MA5))
            {
                company.scoreIndexList.Add(COMPANY_MONTH_TURN_UP);
            }
        }
        /* 
         * 函式 companyMACDAnalysys 用來分析各股 company 公司 MACD 線是否由下向上穿越信號線 
         */
        public void companyMACDAnalysys(Company company,
            StockIndicator[] dayIndicatorArray,
            StockIndicator[] weekIndicatorArray,
            StockIndicator[] monthIndicatorArray)
        {
            var length = dayIndicatorArray.Length;
            if (length > 2)
            {
                if ((dayIndicatorArray[length - 2].MACD <= dayIndicatorArray[length - 2].MACD9MA) &&
                  (dayIndicatorArray[length - 1].MACD > dayIndicatorArray[length - 1].MACD9MA))
                {
                    company.scoreIndexList.Add(COMPANY_DAY_MACD);
                }
            }
            length = weekIndicatorArray.Length;
            if (length > 2)
            {
                if ((weekIndicatorArray[length - 2].MACD <= weekIndicatorArray[length - 2].MACD9MA) &&
                  (weekIndicatorArray[length - 1].MACD > weekIndicatorArray[length - 1].MACD9MA))
                {
                    company.scoreIndexList.Add(COMPANY_WEEK_MACD);
                }
            }
            length = monthIndicatorArray.Length;
            if (length > 2)
            {
                if ((monthIndicatorArray[length - 2].MACD <= monthIndicatorArray[length - 2].MACD9MA) &&
                  (monthIndicatorArray[length - 1].MACD > monthIndicatorArray[length - 1].MACD9MA))
                {
                    company.scoreIndexList.Add(COMPANY_MONTH_MACD);
                }
            }
        }
        /* 
         * 函式 companyRSIAnalysis 用來分析各股 company 公司 RSI 技術指標 
         */
        public void companyRSIAnalysis(Company company,
            StockIndicator[] dayIndicatorArray,
            StockIndicator[] weekIndicatorArray,
            StockIndicator[] monthIndicatorArray)
        {
            var length = dayIndicatorArray.Length;
            if (dayIndicatorArray[length - 1].RSI5 < 30)
            {
                company.scoreIndexList.Add(COMPANY_DAY_RSI5_30);
            }
            if (dayIndicatorArray[length - 1].RSI5 < 20)
            {
                company.scoreIndexList.Add(COMPANY_DAY_RSI5_20);
            }
            if (dayIndicatorArray[length - 1].RSI5 < 10)
            {
                company.scoreIndexList.Add(COMPANY_DAY_RSI5_10);
            }
            if (length > 2)
            {
                if ((dayIndicatorArray[length - 2].RSI5 <= dayIndicatorArray[length - 2].RSI10) &&
                  (dayIndicatorArray[length - 1].RSI5 > dayIndicatorArray[length - 1].RSI10))
                {
                    company.scoreIndexList.Add(COMPANY_DAY_RSI5_THROUGH_RSI10);
                }
            }
            length = weekIndicatorArray.Length;
            if (weekIndicatorArray[length - 1].RSI5 < 30)
            {
                company.scoreIndexList.Add(COMPANY_WEEK_RSI5_30);
            }
            if (weekIndicatorArray[length - 1].RSI5 < 20)
            {
                company.scoreIndexList.Add(COMPANY_WEEK_RSI5_20);
            }
            if (weekIndicatorArray[length - 1].RSI5 < 10)
            {
                company.scoreIndexList.Add(COMPANY_WEEK_RSI5_10);
            }
            if (length > 2)
            {
                if ((weekIndicatorArray[length - 2].RSI5 <= weekIndicatorArray[length - 2].RSI10) &&
                  (weekIndicatorArray[length - 1].RSI5 > weekIndicatorArray[length - 1].RSI10))
                {
                    company.scoreIndexList.Add(COMPANY_WEEK_RSI5_THROUGH_RSI10);
                }
            }
            length = monthIndicatorArray.Length;
            if (monthIndicatorArray[length - 1].RSI5 < 30)
            {
                company.scoreIndexList.Add(COMPANY_MONTH_RSI5_30);
            }
            if (monthIndicatorArray[length - 1].RSI5 < 20)
            {
                company.scoreIndexList.Add(COMPANY_MONTH_RSI5_20);
            }
            if (monthIndicatorArray[length - 1].RSI5 < 10)
            {
                company.scoreIndexList.Add(COMPANY_MONTH_RSI5_10);
            }
            if (length > 2)
            {
                if ((monthIndicatorArray[length - 2].RSI5 <= monthIndicatorArray[length - 2].RSI10) &&
                  (monthIndicatorArray[length - 1].RSI5 > monthIndicatorArray[length - 1].RSI10))
                {
                    company.scoreIndexList.Add(COMPANY_MONTH_RSI5_THROUGH_RSI10);
                }
            }
        }
        /* 
         * 函式 companyAnalysis 根據 company 的各項技術指標，找出評估分數 
         * 參數 pCount 是計算時要使用幾期資料來計算，預設是 80 期
         */
        public void companyAnalysis(Company company, int pCount)
        {
            Indicator indicator;
            indicator = new Indicator(company.getRealHistoryDataArray("d"));
            var dayIndicatorArray = indicator.getStockIndicatorArray(pCount);
            indicator = new Indicator(company.getRealHistoryDataArray("w"));
            var weekIndicatorArray = indicator.getStockIndicatorArray(pCount);
            indicator = new Indicator(company.getRealHistoryDataArray("m"));
            var monthIndicatorArray = indicator.getStockIndicatorArray(pCount);
            companyKDJAnalysis(company, dayIndicatorArray, weekIndicatorArray, monthIndicatorArray);
            companyTrendAnalysis(company);
            companyMACDAnalysys(company, dayIndicatorArray, weekIndicatorArray, monthIndicatorArray);
            companyRSIAnalysis(company, dayIndicatorArray, weekIndicatorArray, monthIndicatorArray);
        }
        /* 
         * 函式 allCompanyCapitalAnalysis 將所有公司的資本額加以排序，
         * 並根據排名給予評分。
         */
        public void allCompanyCapitalAnalysis()
        {
            stockDatabase.sortCompanyByCapital();
            for (var i = 0; i < 50; i++)
            {
                if (i < 10)
                {
                    stockDatabase.companies[i].scoreIndexList.Add(COMPANY_CAPITAL_1_10);
                }
                else if (i < 20)
                {
                    stockDatabase.companies[i].scoreIndexList.Add(COMPANY_CAPITAL_11_20);
                }
                else if (i < 30)
                {
                    stockDatabase.companies[i].scoreIndexList.Add(COMPANY_CAPITAL_21_30);
                }
                else if (i < 40)
                {
                    stockDatabase.companies[i].scoreIndexList.Add(COMPANY_CAPITAL_31_40);
                }
                else
                {
                    stockDatabase.companies[i].scoreIndexList.Add(COMPANY_CAPITAL_41_50);
                }
            }
        }
        /* 
         * 函式 sortCompany 根據各公司的各項技術指標評估分數的總分，加以排序 
         */
        public class Comparer : IComparer<Company>
        {
            public int Compare(Company company1, Company company2)
            {
                if (company2.score > company1.score)
                {
                    return 1;
                }
                else if (company2.score < company1.score)
                {
                    return -1;
                }
                else
                    return 0;
            }
        }
        public void sortCompany()
        {
            /* 將各公司的評估分數按高低排列出來 */
            IComparer<Company> comparer = new Comparer();
            Array.Sort<Company>(
                stockDatabase.companies,
                comparer
            );
        }
        /* 
         * 函式 allCompanyAnalysis 根據所有公司的技術指標呼叫 companyAnalysis 函式，找出評估分數 
         */
        public void allCompanyAnalysis(int pCount)
        {
            new MessageWriter().showMessage("正在進行分析的工作\n");
            allCompanyCapitalAnalysis();
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                Company company = stockDatabase.companies[i];
                company.checkDatabase();
                if (company.passCheckDatabase)
                {
                    companyAnalysis(company, pCount);
                    company.scoreIndexArray = company.scoreIndexList.ToArray();
                    company.scoreIndexList.Clear();
                    company.scoreIndexList = null;
                    evaluateCompanyScore(company);
                    company.scoreIndexArray = null;
                    new WarningWriter().showMessage("index = " + i.ToString());
                    new AppDoEvents().DoEvents();
                }
                // System.Windows.Forms.Application.DoEvents();
            }
        }
        /* 
         * 函式 evaluateTotalScore 根據 scoreValueArray 陣列中的分數，計算出所有評分的總和 
         */
        int totalScore = 0;           // 所有評分的總和
        int totalCompanyScore = 0;    // 不含大盤評分的總和
        public void evaluateTotalScore()
        {
            totalScore = 0;
            for (var i = 0; i < 1000; i++)
            {
                if (scoreValueArray[i] != -1)
                {
                    totalScore = totalScore + scoreValueArray[i];
                }
            }
        }
        /* 
         * 函式 evaluateCompanyTotalScore 根據 scoreValueArray 陣列中的分數，計算出不含大盤評分的總和 
         */
        public void evaluateCompanyTotalScore()
        {
            totalCompanyScore = 0;
            for (var i = 500; i < 1000; i++)
            {
                if (scoreValueArray[i] != -1)
                {
                    totalCompanyScore = totalCompanyScore + scoreValueArray[i];
                }
            }
        }
        /* 
         * 函式 evaluateCompanyScore 根據 company 公司的各項技術指標評估分數，計算出總分 
         */
        public void evaluateCompanyScore(Company company)
        {
            company.score = 0;
            for (var i = 0; i < stockDatabase.scoreIndexList.Count(); i++)
            {
                var score = scoreValueArray[stockDatabase.scoreIndexList[i]];
                company.score = company.score + score;
            }
            for (var i = 0; i < company.scoreIndexArray.Count(); i++)
            {
                /* 月線記錄小於 24 個月的新公司，不計分 */
                if (company.getRealHistoryDataArray("m").Length > 24)
                {
                    var score = scoreValueArray[company.scoreIndexArray[i]];
                    company.score = company.score + score;
                }
                else
                {
                    company.score = 0;
                }
                var printText = "";
                for (var k = 0; k < company.scoreIndexArray.Count(); k++)
                {
                    printText = printText + "\t\t" + scoreTextArray[company.scoreIndexArray[k]] + "，分數 "
                      + scoreValueArray[company.scoreIndexArray[k]] + " 分\n";
                }
                company.printText = printText;
            }
        }
        /* 
         * 函式 isSeasonEPSPass 用以判斷 company 的前二年(前 8 季)的每季 EPS 是否都為正值，
         * 若不是都正值，評分為 0，將其由排名中移除。
         */
        public void isSeasonEPSPass(Company company)
        {
            company.getSeasonEPS();
            var seasonEPS = company.seasonEPS;
            var length = 8;
            if (seasonEPS.Length < length)
            {
                length = seasonEPS.Length;
            }
            var pass = true;
            for (var i = seasonEPS.Length - 1; i >= seasonEPS.Length - length; i--)
            {
                if (seasonEPS[i] < 0)
                {
                    pass = false;
                }
            }
            if (!pass)
            {
                company.score = 0;
                // console.mylog(company.id + " season EPS does not PASS");
            }
        }
        /* 
         * 函式 isYearEPSPass 用以判斷 company 的前五年的每年 EPS 是否都為正值，
         * 若不是都正值，評分為 0，將其由排名中移除。
         */
        public void isYearEPSPass(Company company)
        {
            company.getYearEPS();
            var yearEPS = company.yearEPS;
            var length = 5;
            if (yearEPS.Length < length)
            {
                length = yearEPS.Length;
            }
            var pass = true;
            for (var i = yearEPS.Length - 1; i >= yearEPS.Length - length; i--)
            {
                if (yearEPS[i] < 0)
                {
                    pass = false;
                }
            }
            if (!pass)
            {
                company.score = 0;
                // console.mylog(company.id + " yaer EPS does not PASS");
            }
        }
        /* 
         * 函式 isDividendPass 用以判斷 company 的前五年的每年股利是否都不為 0 ，
         * 若不是都都不為 0 ，評分為 0，將其由排名中移除。
         */
        public void isDividendPass(Company company)
        {
            company.getDividend();
            var dividend = company.dividend;
            var length = 5;
            if (dividend.Length < length)
            {
                length = dividend.Length;
            }
            var pass = true;
            for (var i = dividend.Length - 1; i >= dividend.Length - length; i--)
            {
                if (dividend[i] == 0)
                {
                    pass = false;
                }
            }
            if (!pass)
            {
                company.score = 0;
                // console.mylog(company.id + " dividend does not PASS");
            }
        }
        /* 
         * 函式 printTop20ScoreCompany 根據排序後的各公司，按總分高低，印出前 20 名 
         */
        public String printTop20ScoreCompany()
        {
            var count = 0;
            String printText = "所有評分總合為 " + totalScore + " 分。\r\n";
            printText = printText + "不含大盤評分總合為 " + totalCompanyScore + " 分)。\r\n";
            printText = printText + "評估分數排名：";
            /* 不印出公司 id 中有英文字的公司，例如 2002A 中鋼特 */
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                var company = stockDatabase.companies[i];
                var id = company.id;
                var name = company.name;
                var category = company.category;
                var score = company.score;
                if (id.Length == 4)
                {
                    if (count == 20)
                    {
                        break;
                    }
                    var techAnalysisAddress = "https://tw.stock.yahoo.com/q/ta?s=" + id;
                    var trendAddress = "https://tw.stock.yahoo.com/q/bc?s=" + id;
                    printText = printText + "\r\n排名 " + (count + 1) + "\r\n";
                    printText = printText + "\t公司代號： " + id + "\r\n";
                    printText = printText + "\t公司名稱： " + name + "(" + category + ")\r\n";
                    printText = printText + "\t評估分數： " + score + " 分\r\n";
                    printText = printText + "\t技術分析： " + techAnalysisAddress + "\r\n";
                    printText = printText + "\t走勢圖 ： " + trendAddress + "\r\n";
                    for (var k = 0; k < stockDatabase.scoreIndexList.Count(); k++)
                    {
                        printText = printText + "\t\t" + scoreTextArray[stockDatabase.scoreIndexList[k]] + "，分數 "
                          + scoreValueArray[stockDatabase.scoreIndexList[k]] + " 分\r\n";
                    }
                    printText = printText + company.printText;
                    count++;
                }
            }
            return printText;
        }
        /* 
         * 函式 evaluateAllCompanyScore 根據所有公司的各項技術指標評估分數，計算出總分 
         */
        public String evaluateAllCompanyScore()
        {
            new MessageWriter().showMessage("正在進行篩選的工作\n");
            evaluateTotalScore();
            evaluateCompanyTotalScore();
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                var company = stockDatabase.companies[i];
                // evaluateCompanyScore(company);
                isSeasonEPSPass(company);
                isYearEPSPass(company);
                isDividendPass(company);
                new WarningWriter().showMessage("index = " + i.ToString());
                new AppDoEvents().DoEvents();
                // System.Windows.Forms.Application.DoEvents();
            }
            sortCompany();
            var printText = printTop20ScoreCompany();
            return printText;
        }
        /*
          方法 doAnalysis 用來計算大盤及各股的評估陣列。
          此方法中，分為二大部份，
            01. stockAnalysis : 根據大盤的技術指標，找出評估分數。
            02. companyAnalysis : 根據各股的技術指標，找出評估分數。
          參數 pCount 是要用幾期資料來計算
          評分完畢後，會將各公司按照評分高低排序，stockDatabase.companies 陣列
        */
        public String doAnalysis(int pCount)
        {
            stockAnalysis(pCount);
            allCompanyAnalysis(pCount);
            var printText = evaluateAllCompanyScore();
            return printText;
        }
        /* 
         * 函式 saveAnalysisScore 用來儲存最高評分的時間、公司、分數及超過
         *      總評分一半的公司數。(暫時)
         *      總評分一半的公司數愈多表示股市愈悲觀。
         *      記錄的資料：
         *          日期
         *          最高分公司 id
         *          最高分公司 name
         *          最高分公司 score
         *          超過總評分一半分數的公司數
         */
        public void saveAnalysisScore(String filename)
        {
            FileHelper fileHelper = new FileHelper();
            var topCompany = stockDatabase.companies[0];
            var companyCount = 0;
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                var company = stockDatabase.companies[i];
                var score = company.score;
                if (score > (totalCompanyScore / 2))
                {
                    companyCount++;
                }
            }
            var today = DateTime.Now;
            var year = today.Year;
            var month = today.Month;
            var day = today.Day;
            List<String> dataArray = new List<String>();
            if (fileHelper.Exists("twStock/" + filename))
            {
                var oldSaveString = fileHelper.ReadText("twStock/" + filename);
                var oldSaveStringSplit = oldSaveString.Split(new string[] { "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < oldSaveStringSplit.Length; i++)
                {
                    dataArray.Add(oldSaveStringSplit[i] + "\n");
                }
            }
            dataArray.Add(
                year + "/" + month + "/" + day + " " +
                topCompany.id + " " +
                topCompany.name + " " +
                topCompany.score + " " +
                companyCount +
                "\n");
            var saveString = "";
            for (var i = 0; i < dataArray.Count(); i++)
            {
                saveString = saveString + dataArray[i];
            }
            fileHelper.WriteText("twStock/" + filename, saveString);
        }
        /*
         * 函式 getSavedAnalysisScore 用來取得評分記錄存檔中的資訊
         */
        // String analysisType;
        public String getSavedAnalysisScore(int count)
        {
            String filename;
            /*
            if (this.analysisType == "ShortTerm")
            {
                filename = "shortTermScore.dat";
            }
            else if (this.analysisType == "ShortTerm1")
            {
                filename = "shortTerm1Score.dat";
            }
            else if (this.analysisType == "LongTerm")
            {
                filename = "longTermScore.dat";
            }
            else
            {
                return "";
            }
             * */
            filename = "shortTerm1Score.dat";
            filename = "twStock/" + filename;
            FileHelper fileHelper = new FileHelper();
            var returnString = "評分的歷史記錄：\r\n";
            var saveString = fileHelper.ReadText(filename);
            var saveStringSplit = saveString.Split(new string[] { "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
            var start = 0;
            if (saveStringSplit.Length > count)
            {
                start = saveStringSplit.Length - count;
            }
            var end = start + count;
            if (end > saveStringSplit.Length)
            {
                end = saveStringSplit.Length;
            }
            for (var i = start; i < end; i++)
            {
                var oneData = saveStringSplit[i];
                var oneDataSplit = oneData.Split(new string[] { " " },
                    StringSplitOptions.RemoveEmptyEntries);
                returnString = returnString +
                  "日期：" + oneDataSplit[0] + "\t" +
                  "公司代號：" + oneDataSplit[1] + "\t" +
                  "公司名稱：" + oneDataSplit[2] + "\t" +
                  "評分值：" + oneDataSplit[3] + "\t" +
                  "總評分一半分數以上的公司數：" + oneDataSplit[4] +
                  "\r\n";
            }

            return returnString;
        }
        /* 
         * 函式 doShortTermAnalysis 呼叫 doAnalysis 進行短線分析及評分，
         * 此函式已癈止不用，不含篩選的程式，詳見函式 doShort1ScoreAnalysis 的說明。
         */
        public String doShortTermAnalysis(int pCount)
        {
            // analysisType = "ShortTerm";
            scoreValueArray = shortTermScoreValueArray;
            var printText = doAnalysis(pCount);
            saveAnalysisScore("shortTermScore.dat");
            return printText;
        }
        /* 
         * 函式 doLongTermAnalysis 呼叫 doAnalysis 進行長線分析及評分，
         * 此函式已癈止不用，不含篩選的程式，詳見函式 doShort1ScoreAnalysis 的說明。
         */
        public String doLongTermAnalysis(int pCount)
        {
            // analysisType = "LongTerm";
            scoreValueArray = longTermScoreValueArray;
            var printText = doAnalysis(pCount);
            saveAnalysisScore("longTermScore.dat");
            return printText;
        }
        /* 
         * 函式 saveEveryDayScore 用來將每次評分的結果存於 everyDayScore.dat 檔案中 
         */
        public void saveEveryDayScore()
        {
            var topCompany = stockDatabase.companies[0];
            var companyCount = 0;
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                var company = stockDatabase.companies[i];
                var score = company.score;
                if (score >= 20)
                {
                    companyCount++;
                }
            }
            var today = DateTime.Now;
            var year = today.Year;
            var month = today.Month;
            var day = today.Day;
            // var dataArray = [];
            var count = 0;
            var printText = "\n<END>\n";
            printText = printText + "日期：" + year + "/" + month + "/" + day + "\n";
            printText = printText + "所有評分總合為 " + totalScore + " 分。\n";
            printText = printText + "不含大盤評分總合為 " + totalCompanyScore + " 分)。\n";
            printText = printText + "評分大於 20 以上的公司數：" + companyCount + "\n";
            printText = printText + "評估分數排名：\n";
            /* 不印出公司 id 中有英文字的公司，例如 2002A 中鋼特 */
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                var company = stockDatabase.companies[i];
                var id = company.id;
                var name = company.name;
                var category = company.category;
                var score = company.score;
                if (id.Length == 4)
                {
                    if (count == 20)
                    {
                        break;
                    }
                    printText = printText + "\t排名 " + (count + 1) + " ";
                    printText = printText + id + " ";
                    printText = printText + name + "(" + category + ") ";
                    printText = printText + score + " 分\n";
                    count++;
                }
            }
            printText = printText + "\n<END>\n";
            FileHelper fileHelper = new FileHelper();
            if (fileHelper.Exists("twStock/everyDayScore.dat"))
            {
                String oldText = fileHelper.ReadText("twStock/everyDayScore.dat");
                fileHelper.WriteText("twStock/everyDayScore.dat", oldText + printText);
            }
            else
            {
                fileHelper.WriteText("twStock/everyDayScore.dat", printText);
            }
        }
        /*
         * 函式 loadAccumulateScore 用來將 companyId 代號的公司之累積資料由檔
         * 案中讀出。
         * 讀出的資料為 accumulateData 物件，包括
         *      {
         *          lastDate : 最後累積評分的日期
         *          accumulateScore : 累積的評分
         *          lowestPrice : 到目前最低股價
         *      }
         * 傳回 null 表示沒有累積資料檔案。
         */
        public AccumulateData loadAccumulateScore(String companyId)
        {
            FileHelper fileHelper = new FileHelper();
            AccumulateData accumulateData;
            String lastDate;
            int accumulateScore;
            Double lowestPrice;
            var filename = "company/" + companyId + "/scoreAccumulation.dat";
            if (fileHelper.Exists(filename))
            {
                var saveString = fileHelper.ReadText(filename);
                var saveStringSplit = saveString.Split(new string[] { "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
                lastDate = saveStringSplit[0];
                accumulateScore = Convert.ToInt32(saveStringSplit[1]);
                lowestPrice = Convert.ToDouble(saveStringSplit[2]);
                accumulateData = new AccumulateData();
                accumulateData.lastDate = lastDate;
                accumulateData.accumulateScore = accumulateScore;
                accumulateData.lowestPrice = lowestPrice;
                return accumulateData;
            }
            else
            {
                return null;
            }
        }
        /*
         * 函式 saveAccumulateScore 用來將 companyId 代號的公司累積資料存入其
         * 累積資料檔案中。
         */
        public void saveAccumulateScore(String companyId, AccumulateData accumulateData)
        {
            var lastDate = accumulateData.lastDate;
            int accumulateScore;
            Double lowestPrice;
            var filename = "company/" + companyId + "/scoreAccumulation.dat";
            var loadAccumulateData = loadAccumulateScore(companyId);
            var loadLastDate = "1900/01/01";

            if (loadAccumulateData == null)
            {
                accumulateScore = accumulateData.accumulateScore;
                lowestPrice = accumulateData.lowestPrice;
            }
            else
            {
                loadLastDate = loadAccumulateData.lastDate;
                if (loadLastDate != lastDate)
                {
                    accumulateScore = loadAccumulateData.accumulateScore + accumulateData.accumulateScore;
                    if (accumulateData.lowestPrice < loadAccumulateData.lowestPrice)
                    {
                        lowestPrice = accumulateData.lowestPrice;
                    }
                    else
                    {
                        lowestPrice = loadAccumulateData.lowestPrice;
                    }
                }
                else
                {
                    accumulateScore = loadAccumulateData.accumulateScore;
                    lowestPrice = loadAccumulateData.lowestPrice;
                }
            }
            FileHelper fileHelper = new FileHelper();
            fileHelper.WriteText(filename, lastDate + "\n" + accumulateScore + "\n" + lowestPrice + "\n");
        }
        /*
         * 函式 timeToString 將轉入的 DateTime time 物件轉換為字串型態。
         * 字串型態為 yyyy/mm/dd 的格式。
         */
        public String timeToString(DateTime time)
        {
            var year = time.Year;
            var month = time.Month;
            var date = time.Day;
            var timeString = year + "/";
            if (month >= 10)
            {
                timeString = timeString + month + "/";
            }
            else
            {
                timeString = timeString + "0" + month + "/";
            }
            if (date >= 10)
            {
                timeString = timeString + date;
            }
            else
            {
                timeString = timeString + "0" + date;
            }
            return timeString;
        }
        /*
         * 函式 saveAllAccumulateScore 用來將每日評分前 20 的資訊記錄到累積資料檔
         * 案中。
         * 每日評分前 20 名，如果
         *      該股票資料庫路徑中沒有 scoreAccumulation.dat 檔案，則創建
         *      該檔，寫入預設初始資訊。
         * 讀出累積資訊，更新累積資訊檔案。
         */
        public void saveAllAccumulateScore()
        {
            int count = 0;
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                var company = stockDatabase.companies[i];
                var id = company.id;
                var score = company.score;
                var dayHistoryData = company.getRealHistoryDataArray("d");
                if (id.Length == 4)
                {
                    if (count == 20)
                    {
                        break;
                    }
                    var now = DateTime.Now;
                    AccumulateData accumulateData = new AccumulateData();
                    accumulateData.lastDate = timeToString(now);
                    accumulateData.accumulateScore = score;
                    accumulateData.lowestPrice = dayHistoryData[dayHistoryData.Length - 1].l;
                    saveAccumulateScore(id, accumulateData);
                    count++;
                }
            }
        }
        /*
         * 函式 checkConditionA 利用 Date.DateDiff 函式檢查
         * 累積排名的最後記錄時間與今天的時間差，檢查該時間差
         * 是否超過 10 天，若超過 10 天傳回 true，否則傳回 false。
         */
        public Boolean checkConditionA(AccumulateData loadAccumulateData, DateTime now)
        {
            int year = Convert.ToInt32(loadAccumulateData.lastDate.Substring(0, 4));
            int month = Convert.ToInt32(loadAccumulateData.lastDate.Substring(5, 2));
            int day = Convert.ToInt32(loadAccumulateData.lastDate.Substring(8, 2));
            var lastDate = new DateTime(year, month, day);
            var diff = (DateTime.Now - lastDate).TotalDays;
            if (diff >= 10)
            {
                return true;
            }
            return false;
        }
        /*
         * 函式 checkConditionB 檢查累積記錄中的股價最低值是否已上漲超過
         * 10% ，如果是超過 10% 傳回 true，否則傳回 false。
         */
        public Boolean checkConditionB(AccumulateData loadAccumulateData, Double lowestPriceToday)
        {
            var priceDiff = lowestPriceToday - loadAccumulateData.lowestPrice;
            var ratio = priceDiff / loadAccumulateData.lowestPrice;
            if (ratio > 0.1)
            {
                return true;
            }
            return false;
        }
        /*
         * 原 JavaScript 程式中，candidateArrayReset 等是陣列型態，在此
         * 則為 List 型態，變數名字不更改是為了和 JavaScript 程式做對照。
         */
        List<AccumulateData> candidateArrayReset = new List<AccumulateData>();
        List<AccumulateData> candidateArrayA = new List<AccumulateData>();
        List<AccumulateData> candidateArrayD = new List<AccumulateData>();
        List<AccumulateData> candidateArrayE = new List<AccumulateData>();
        List<AccumulateData> candidateArrayF = new List<AccumulateData>();
        /*
         * 函式 resetAccumulateScore 用來將
         *      (a) 脫出累積排行超過 10 天，或
         *      (b) 上漲超過最低股價 10%
         * 股票的累積記錄清除，清除資訊會輸出至訊息視窗中。
         */
        public String resetAccumulateScore()
        {
            var returnText = "重置累積排名記錄結果：\r\n";
            var resetCount = 0;
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                var company = stockDatabase.companies[i];
                if (!company.passCheckDatabase)
                {
                    continue;
                }
                var id = company.id;
                var name = company.name;
                var dayHistoryData = company.getRealHistoryDataArray("d");
                var lowestPriceToday = dayHistoryData[dayHistoryData.Length - 1].l;
                FileHelper fileHelper = new FileHelper();
                if (id.Length == 4)
                {
                    var now = DateTime.Now;
                    var loadAccumulateData = loadAccumulateScore(id);

                    if (loadAccumulateData == null)
                    {
                        // 沒有累積排名記錄
                    }
                    else
                    {
                        // 有累積排名記錄，檢查記錄是否符合 (a) 或 (b) 條件
                        if (checkConditionA(loadAccumulateData, now))
                        {
                            // 符合條件 (a)，重置記錄
                            var filename = "company/" + id + "/scoreAccumulation.dat";
                            fileHelper.Delete(filename);
                            resetCount++;
                            AccumulateData candidate = new AccumulateData();
                            candidate.company = company;
                            candidate.id = id;
                            candidate.name = name;
                            candidateArrayReset.Add(candidate);
                            returnText = returnText + "\t" + name + "公司(" + id + ")因脫出排名 10 天以上，故重置其累積記錄。\r\n";
                        }
                        else if (checkConditionB(loadAccumulateData, lowestPriceToday))
                        {
                            // 符合條件 (b)，重置記錄
                            var filename = "company/" + id + "/scoreAccumulation.dat";
                            fileHelper.Delete(filename);
                            resetCount++;
                            AccumulateData candidate = new AccumulateData();
                            candidate.company = company;
                            candidate.id = id;
                            candidate.name = name;
                            candidateArrayReset.Add(candidate);
                            returnText = returnText + "\t" + name + "公司(" + id + ")因上漲超過前波最低股價 10% 以上，故重置其累積記錄。\r\n";
                        }
                    }
                }
            }
            if (resetCount == 0)
            {
                returnText = returnText + "\t沒有重置發生。\r\n";
            }
            returnText = returnText + "\r\n";
            return returnText;
        }
        List<AccumulateData> accumulateDataArray = null;
        /*
         * 函式 printAndSaveInfomationA 用來印出並存檔下列資訊
         * (a) 累積評分 X 脫出日數
         * 按排名依序列出所有股票(脫出 2 日以上)
         */
        public String printAndSaveInfomationA()
        {
            var returnText = "排名脫出：\r\n";
            var findCount = 0;
            var now = DateTime.Now;
            List<AccumulateData> filterData = new List<AccumulateData>();
            for (var i = 0; i < accumulateDataArray.Count(); i++)
            {
                var accumulateData = accumulateDataArray[i];
                int year = Convert.ToInt32(accumulateData.lastDate.Substring(0, 4));
                int month = Convert.ToInt32(accumulateData.lastDate.Substring(5, 2));
                int day = Convert.ToInt32(accumulateData.lastDate.Substring(8, 2));
                DateTime lastDate = new DateTime(year, month, day);
                Double diff = (now - lastDate).TotalDays;
                if (diff >= 3)
                {
                    findCount++;
                    accumulateData.diff = diff;
                    filterData.Add(accumulateData);
                }
            }
            filterData.Sort(
                delegate(AccumulateData accumulateDataA, AccumulateData accumulateDataB)
                {
                    if (accumulateDataA.accumulateScore > accumulateDataB.accumulateScore)
                    {
                        return -1;
                    }
                    else if (accumulateDataA.accumulateScore < accumulateDataB.accumulateScore)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            );
            for (var i = 0; i < filterData.Count(); i++)
            {
                AccumulateData accumulateData = filterData[i];
                candidateArrayA.Add(accumulateData);
                /*
                 * JavaAcript 程式中的 diff 應該有錯，這裡的才正確。
                 */
                returnText = returnText + "\t" + accumulateData.name + "(" + accumulateData.id +
                    ") 累積評分 = " + accumulateData.accumulateScore +
                    " 脫出排名 " + Convert.ToInt32(accumulateData.diff) + " 天\r\n";
            }
            if (findCount == 0)
            {
                returnText = returnText + "\t沒有股票脫出排名 2 天以上\r\n";
            }
            returnText = returnText + "\r\n";
            return returnText;
        }
        /*
         * 函式 printAndSaveInfomationB 用來印出並存檔下列資訊
         *      (b) 未脫出時，排名下降達 10 名以上股票(脫出趨勢強)
         */
        public String printAndSaveInfomationB()
        {
            var returnText = "排名下降趨勢強：\r\n";
            var findCount = 0;
            for (var i = 0; i < accumulateDataArray.Count(); i++)
            {
                var accumulateData = accumulateDataArray[i];
                if ((accumulateData.rankToday != -1) && (accumulateData.lastRank != -1))
                {
                    var diff = accumulateData.rankToday - accumulateData.lastRank;
                    if (diff >= 10)
                    {
                        findCount++;
                        returnText = returnText + "\t" + accumulateData.name +
                            "(" + accumulateData.id + ") 排名下降達 10 名以上(脫出趨勢強)\r\n";
                    }
                }
            }
            if (findCount == 0)
            {
                returnText = returnText + "\t沒有股票排名下降達 10 名以上\r\n";
            }
            returnText = returnText + "\r\n";
            return returnText;
        }
        /*
         * 函式 printAndSaveInfomationC 用來印出並存檔下列資訊
         *      (c) 未脫出時，排名下降達 5 名以上股票(脫出趨勢弱)
         */
        public String printAndSaveInfomationC()
        {
            var returnText = "排名下降趨勢弱：\r\n";
            var findCount = 0;
            for (var i = 0; i < accumulateDataArray.Count(); i++)
            {
                var accumulateData = accumulateDataArray[i];
                if ((accumulateData.rankToday != -1) && (accumulateData.lastRank != -1))
                {
                    var diff = accumulateData.rankToday - accumulateData.lastRank;
                    if ((diff >= 5) && (diff < 10))
                    {
                        findCount++;
                        returnText = returnText + "\t" + accumulateData.name +
                            "(" + accumulateData.id + ") 排名下降達 5 名以上(脫出趨勢弱)\r\n";
                    }
                }
            }
            if (findCount == 0)
            {
                returnText = returnText + "\t沒有股票下降達 5 名以上股票\r\n";
            }
            returnText = returnText + "\r\n";
            return returnText;
        }
        /*
         * 函式 printAndSaveInfomationD 用來印出並存檔下列資訊
         *      (d) 不論是否脫出，法人投信外資(合計)大買股票
         */
        public String printAndSaveInfomationD()
        {
            var returnText = "法人買超：\r\n";
            var findCount = 0;
            for (var i = 0; i < accumulateDataArray.Count(); i++)
            {
                var accumulateData = accumulateDataArray[i];
                /*
                  法人大買判斷原則：
                    (a) 今天及昨天連二天買超
                    (b) 今天買超量大於昨天超量 
                */
                if ((accumulateData.sVolumeIncrese2 > 0) &&
                  (accumulateData.sVolumeIncrese2 > 0) &&
                  (accumulateData.sVolumeIncrese1 > accumulateData.sVolumeIncrese2))
                {
                    candidateArrayD.Add(accumulateData);
                    returnText = returnText + "\t" + accumulateData.name +
                        "(" + accumulateData.id + ") 法人投信外資(合計)大買股票\r\n";
                }
            }
            if (findCount == 0)
            {
                returnText = returnText + "\t沒有股票法人投信外資(合計)大買股票\r\n";
            }
            returnText = returnText + "\r\n";
            return returnText;
        }
        /*
         * 函式 printAndSaveInfomationE 用來印出並存檔下列資訊
         *      (e) 不論是否脫出，成交量突然放大 5 倍以上股票(脫出趨勢強)
         */
        public String printAndSaveInfomationE()
        {
            var returnText = "成交量放大(5倍以上)：\r\n";
            var findCount = 0;
            for (var i = 0; i < accumulateDataArray.Count(); i++)
            {
                var accumulateData = accumulateDataArray[i];
                var incRate = accumulateData.volumeToday / accumulateData.volumeYesterday;
                if (incRate >= 5)
                {
                    candidateArrayE.Add(accumulateData);
                    returnText = returnText + "\t" + accumulateData.name +
                        "(" + accumulateData.id + ") 成交量突然放大 5 倍以上\r\n";
                }
            }
            if (findCount == 0)
            {
                returnText = returnText + "\t沒有股票成交量突然放大 5 倍以上股票\r\n";
            }
            returnText = returnText + "\r\n";
            return returnText;
        }
        /*
         * 函式 printAndSaveInfomationF 用來印出並存檔下列資訊
         *      (f) 不論是否脫出，成交量突然放大 2 倍以上股票(脫出趨勢弱)
         */
        public String printAndSaveInfomationF()
        {
            var returnText = "成交量放大(2倍以上)：\r\n";
            var findCount = 0;
            for (var i = 0; i < accumulateDataArray.Count(); i++)
            {
                var accumulateData = accumulateDataArray[i];
                var incRate = accumulateData.volumeToday / accumulateData.volumeYesterday;
                if ((incRate >= 2) && (incRate < 5))
                {
                    candidateArrayE.Add(accumulateData);
                    returnText = returnText + "\t" + accumulateData.name +
                        "(" + accumulateData.id + ") 成交量突然放大 2 倍以上\r\n";
                }
            }
            if (findCount == 0)
            {
                returnText = returnText + "\t沒有股票成交量突然放大 2 倍以上股票\r\n";
            }
            returnText = returnText + "\r\n";
            return returnText;
        }
        class LastRank
        {
            public String id;
            public int rank;
        }
        List<LastRank> lastRankArray = new List<LastRank>();
        /*
         * 函式 findRankFromLastRankArray 由 lastRankArray 中，
         * 找出 id 是傳入參數 id 的該筆記錄之 rank。
         *  傳回 -1 表示沒有找到相符合 id 的記錄。
         */
        public int findRankFromLastRankArray(String id)
        {
            for (var i = 0; i < lastRankArray.Count(); i++)
            {
                if (id == lastRankArray[i].id)
                {
                    return lastRankArray[i].rank;
                }
            }
            return -1;
        }
        /*
         * 函式 findRankLastDay 用來找出有累積資訊的股票前一天的排名，
         * 以便後續可分析曾上榜股票排名的變動 (b)(c)。
         */
        public String findRankLastDay()
        {
            FileHelper fileHelper = new FileHelper();
            // 載入評分排行記錄檔案
            var filename = "twStock/everyDayScore.dat";
            var saveString = fileHelper.ReadText(filename);
            var saveStringSplit = saveString.Split(new string[] { "\n<END>\n" },
                    StringSplitOptions.RemoveEmptyEntries);
            String rankLastDay;
            if (saveStringSplit.Length < 1)
            {
                rankLastDay = saveStringSplit[0];
            }
            else
            {
                rankLastDay = saveStringSplit[saveStringSplit.Length - 1];
            }
            var rankLastDaySplit = rankLastDay.Split(new string[] { "\n" },
                        StringSplitOptions.RemoveEmptyEntries);
            for (var j = 5; j < rankLastDaySplit.Length; j++)
            {
                var rankData = rankLastDaySplit[j];
                var rankDataSplit = rankData.Split(new string[] { " " },
                          StringSplitOptions.RemoveEmptyEntries);
                LastRank lastRank = new LastRank();
                lastRank.id = rankDataSplit[2];
                lastRank.rank = j - 5 + 1;
                lastRankArray.Add(lastRank);
            }
            var returnText = "排名變化：\r\n";
            for (var i = 0; i < accumulateDataArray.Count(); i++)
            {
                var id = accumulateDataArray[i].id;
                var name = accumulateDataArray[i].name;
                var lastRank = findRankFromLastRankArray(id);
                accumulateDataArray[i].lastRank = lastRank;
                returnText = returnText + "" + name + " " + id + " " + accumulateDataArray[i].lastRank +
                    " " + accumulateDataArray[i].rankToday + "\r\n";
            }
            returnText = returnText + "\t";
            return returnText;
        }
        /*
         * 函式 findAccumulateDataArray 用來找出全部含有累積資訊的股票，
         * 以利後續分析其資訊是否有上漲趨勢。
         */
        public void findAccumulateDataArray()
        {
            accumulateDataArray = new List<AccumulateData>();
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                var company = stockDatabase.companies[i];
                var id = company.id;
                var name = company.name;
                var dayHistoryData = company.getRealHistoryDataArray("d");
                if (dayHistoryData.Length < 3) continue;
                var lowestPriceToday = dayHistoryData[dayHistoryData.Length - 1].l;
                var volumeToday = dayHistoryData[dayHistoryData.Length - 1].v;
                var volumeYesterday = dayHistoryData[dayHistoryData.Length - 2].v;
                var sVolumeToday = dayHistoryData[dayHistoryData.Length - 1].s;
                var sVolumeYestoday = dayHistoryData[dayHistoryData.Length - 2].s;
                var sVolumeTheDayBeforeYesterday = dayHistoryData[dayHistoryData.Length - 3].s;
                var sVolumeIncrese1 = sVolumeToday - sVolumeYestoday;
                var sVolumeIncrese2 = sVolumeYestoday - sVolumeTheDayBeforeYesterday;
                var rankToday = i + 1;
                if (i > 20)
                {
                    rankToday = -1;
                }
                if (id.Length == 4)
                {
                    var loadAccumulateData = loadAccumulateScore(id);
                    if (loadAccumulateData == null)
                    {
                        // 沒有累積排名記錄
                    }
                    else
                    {
                        // 有累積排名記錄
                        var accumulateData = new AccumulateData();
                        accumulateData.company = company;
                        accumulateData.id = id;
                        accumulateData.name = name;
                        accumulateData.lowestPriceToday = lowestPriceToday;
                        accumulateData.volumeToday = volumeToday;
                        accumulateData.volumeYesterday = volumeYesterday;
                        accumulateData.rankToday = rankToday;
                        accumulateData.lastDate = loadAccumulateData.lastDate;
                        accumulateData.accumulateScore = loadAccumulateData.accumulateScore;
                        accumulateData.lowestPrice = loadAccumulateData.lowestPrice;
                        accumulateData.sVolumeIncrese1 = sVolumeIncrese1;
                        accumulateData.sVolumeIncrese2 = sVolumeIncrese2;
                        accumulateDataArray.Add(accumulateData);
                    }
                }
            }
        }
        /*
         * 函式 printAndSaveInfomation 用來印出並存檔下列上漲趨勢資訊
         *      (a) 累積評分 X 脫出日數
         *          按排名依序列出所有股票(脫出 2 日以上)
         *      (b) 未脫出時，排名下降達 10 名以上股票(脫出趨勢強)
         *      (c) 未脫出時，排名下降達 5 名以上股票(脫出趨勢弱)
         *      (d) 不論是否脫出，法人投信外資(合計)大買股票
         *      (e) 不論是否脫出，成交量突然放大 5 倍以上股票(脫出趨勢強)
         *      (f) 不論是否脫出，成交量突然放大 2 倍以上股票(脫出趨勢弱)
         */
        public String printAndSaveInfomation()
        {
            findAccumulateDataArray();
            var testText = findRankLastDay();
            var returnText = "累積記錄資訊：\r\n";
            returnText = returnText + printAndSaveInfomationA();
            returnText = returnText + printAndSaveInfomationB();
            returnText = returnText + printAndSaveInfomationC();
            returnText = returnText + printAndSaveInfomationD();
            returnText = returnText + printAndSaveInfomationE();
            returnText = returnText + printAndSaveInfomationF();
            return returnText;
        }
        List<AccumulateData> allCandidateArray = new List<AccumulateData>();
        /*
         * 函式 isInCandidateArray 檢查 id 公司是否已經在 allCandidateArray
         * 陣列中。
         */
        public Boolean isInCandidateArray(String id)
        {
            for (var i = 0; i < allCandidateArray.Count(); i++)
            {
                if (allCandidateArray[i].company.id == id)
                {
                    return true;
                }
            }
            return false;
        }
        /*
         * 函式 createAllCandidateArray 用來從其它 candidate 陣列中，
         * 取出記錄加入到 allCandidateArray 列表中。
         */
        public void createAllCandidateArray()
        {
            for (var i = 0; i < candidateArrayReset.Count(); i++)
            {
                if (isInCandidateArray(candidateArrayReset[i].id) == false)
                {
                    AccumulateData candidate = new AccumulateData();
                    candidate.company = candidateArrayReset[i].company;
                    candidate.id = candidateArrayReset[i].id;
                    candidate.name = candidateArrayReset[i].name;
                    allCandidateArray.Add(candidate);
                }
            }
            for (var i = 0; i < candidateArrayA.Count(); i++)
            {
                if (isInCandidateArray(candidateArrayA[i].id) == false)
                {
                    AccumulateData candidate = new AccumulateData();
                    candidate.company = candidateArrayA[i].company;
                    candidate.id = candidateArrayA[i].id;
                    candidate.name = candidateArrayA[i].name;
                    allCandidateArray.Add(candidate);
                }
            }
            for (var i = 0; i < candidateArrayD.Count(); i++)
            {
                if (isInCandidateArray(candidateArrayD[i].id) == false)
                {
                    AccumulateData candidate = new AccumulateData();
                    candidate.company = candidateArrayD[i].company;
                    candidate.id = candidateArrayD[i].id;
                    candidate.name = candidateArrayD[i].name;
                    allCandidateArray.Add(candidate);
                }
            }
            for (var i = 0; i < candidateArrayE.Count(); i++)
            {
                if (isInCandidateArray(candidateArrayE[i].id) == false)
                {
                    AccumulateData candidate = new AccumulateData();
                    candidate.company = candidateArrayE[i].company;
                    candidate.id = candidateArrayE[i].id;
                    candidate.name = candidateArrayE[i].name;
                    allCandidateArray.Add(candidate);

                }
            }
            for (var i = 0; i < candidateArrayF.Count(); i++)
            {
                if (isInCandidateArray(candidateArrayF[i].id) == false)
                {
                    AccumulateData candidate = new AccumulateData();
                    candidate.company = candidateArrayF[i].company;
                    candidate.id = candidateArrayF[i].id;
                    candidate.name = candidateArrayF[i].name;
                    allCandidateArray.Add(candidate);

                }
            }
            for (var i = 0; i < allCandidateArray.Count(); i++)
            {
                allCandidateArray[i].pass = true;
            }
        }
        List<AccumulateData> allCandidateArrayA = new List<AccumulateData>();
        /*
         * 函式 checkACompany 用來檢查 company 公司是否符合
         *      (a) 過去 5 年中，最高點股價至今下跌至少 50% 以上
         *          5 年資料以大約 200*5 筆資料來看，假設一年約 200 筆。
         *          不足 1000 筆資料的股票，找尋全部現有資料。
         */
        public Boolean checkACompany(Company company, int index)
        {
            var returnValue = false;
            var historyDataArray = company.getRealHistoryDataArray("d");
            var priceToday = historyDataArray[index].c;
            var highestPrice = historyDataArray[index].h;
            var count = 0;
            for (var i = index - 1; i > 0; i--)
            {
                var oneHistoryData = historyDataArray[i];
                if (highestPrice < oneHistoryData.h)
                {
                    highestPrice = oneHistoryData.h;
                }
                count++;
                if (count > 1000)
                {
                    break;
                }
            }
            var rate = (highestPrice - priceToday) / highestPrice;
            if (rate >= 0.5)
            {
                returnValue = true;
            }
            return returnValue;
        }
        /*
         * 函式 filterCandidateA 用來檢查所有關注的股票是否可以通過條件
         *      (a) 過去 5 年中，最高點股價至今下跌至少 50% 以上
         */
        public List<AccumulateData> filterCandidateA()
        {
            allCandidateArrayA = new List<AccumulateData>();
            for (var i = 0; i < allCandidateArray.Count(); i++)
            {
                var company = allCandidateArray[i].company;
                if (!company.passCheckDatabase)
                {
                    continue;
                }
                var pass = checkACompany(company,
                    company.getRealHistoryDataArray("d").Length - 1);
                if (pass)
                {
                    AccumulateData candidate = new AccumulateData();
                    candidate.company = company;
                    candidate.id = allCandidateArray[i].id;
                    candidate.name = allCandidateArray[i].name;
                    candidate.pass = true;
                    allCandidateArrayA.Add(candidate);
                    company.candidateMatchCount++;
                    company.candidateMatchString = company.candidateMatchString + "(A)";
                }
                if (allCandidateArray[i].pass)
                {
                    allCandidateArray[i].pass = pass;
                }
            }
            return allCandidateArrayA;
        }
        List<AccumulateData> allCandidateArrayB = new List<AccumulateData>();
        /*
         * 函式 checkBCompany 用來檢查 company 公司是否符合
         *      (b) 股利推估有 5% 以上
         *      股利推估方法是利用最近 4 季 EPS 去計算
         */
        public Boolean checkBCompany(Company company, int index)
        {
            var returnValue = false;
            var historyDataArray = company.getRealHistoryDataArray("d");
            var priceToday = historyDataArray[index].c;
            company.getSeasonEPS();
            var seasonEPS = company.seasonEPS;
            var totalSeasons = seasonEPS.Length;
            // console.log(company.id + " " + totalSeasons);
            if (totalSeasons > 4)
            {
                var totalEPS =
                  seasonEPS[totalSeasons - 1] +
                  seasonEPS[totalSeasons - 2] +
                  seasonEPS[totalSeasons - 3] +
                  seasonEPS[totalSeasons - 4];
                if ((totalEPS / priceToday) > 0.05)
                {
                    returnValue = true;
                }
            }
            return returnValue;
        }
        /*
         * 函式 filterCandidateB 用來檢查所有關注的股票是否可以通過條件
         *      (b) 股利推估有 5% 以上
         */
        public List<AccumulateData> filterCandidateB()
        {
            allCandidateArrayB = new List<AccumulateData>();
            for (var i = 0; i < allCandidateArray.Count(); i++)
            {
                var company = allCandidateArray[i].company;
                if (!company.passCheckDatabase)
                {
                    continue;
                }
                var pass = checkBCompany(company,
                    company.getRealHistoryDataArray("d").Length - 1);
                if (pass)
                {
                    AccumulateData candidate = new AccumulateData();
                    candidate.company = company;
                    candidate.id = allCandidateArray[i].id;
                    candidate.name = allCandidateArray[i].name;
                    candidate.pass = true;
                    allCandidateArrayB.Add(candidate);
                    company.candidateMatchCount++;
                    company.candidateMatchString = company.candidateMatchString + "(B)";
                }
                if (allCandidateArray[i].pass)
                {
                    allCandidateArray[i].pass = pass;
                }
            }
            return allCandidateArrayB;
        }
        List<AccumulateData> allCandidateArrayC = new List<AccumulateData>();
        /*
         * 函式 checkCCompany 用來檢查 company 公司是否符合
         *      (c) 爆量上漲(單日上漲超過 3% ，成交量超過前 100 天平均 3 倍，最少要 500 張)
         */
        public Boolean checkCCompany(Company company, int indexParam)
        {
            var returnValue = false;
            if (indexParam == 0)
            {
                return returnValue;
            }
            var historyDataArray = company.getRealHistoryDataArray("d");
            var priceToday = historyDataArray[indexParam].c;
            var priceYesterday = historyDataArray[indexParam - 1].c;
            /* 以下計算 100 天平均交易量 */
            var volume100Average = 0.0;
            var count = 0;
            var index = indexParam;
            for (int i = 0; i < 100; i++)
            {
                if (index >= 0)
                {
                    volume100Average = volume100Average + historyDataArray[index].v;
                    index = index - 1;
                    count = count + 1;
                }
                else
                {
                    break;
                }

            }
            volume100Average = volume100Average / count;
            /* 以上計算 100 天平均交易量 */
            var volumeToday = historyDataArray[indexParam].v;
            if (volumeToday >= 500)
            {
                // var volumeYesterday = historyDataArray[historyDataArray.Length - 2].v;
                var priceRate = (priceToday - priceYesterday) / priceYesterday;
                var volumeRate = (volumeToday - volume100Average) / volume100Average;
                if ((priceRate > 0.03) && (volumeRate > 3))
                {
                    returnValue = true;
                }
            }
            return returnValue;
        }
        /*
         * 函式 filterCandidateC 用來檢查所有關注的股票是否可以通過條件
         *      (C) 爆量上漲(單日上漲超過 3% ，成交量超過前 100 天平均 3 倍)
         *      (E) 一個月內主力發動二次攻擊。
         */
        public List<AccumulateData> filterCandidateC()
        {
            allCandidateArrayC = new List<AccumulateData>();
            for (var i = 0; i < allCandidateArray.Count(); i++)
            {
                var company = allCandidateArray[i].company;
                if (!company.passCheckDatabase)
                {
                    continue;
                }
                var pass = checkCCompany(company,
                    company.getRealHistoryDataArray("d").Length - 1);
                if (pass)
                {
                    AccumulateData candidate = new AccumulateData();
                    candidate.company = company;
                    candidate.id = allCandidateArray[i].id;
                    candidate.name = allCandidateArray[i].name;
                    candidate.pass = true;
                    allCandidateArrayC.Add(candidate);
                    company.candidateMatchCount++;
                    company.candidateMatchString = company.candidateMatchString + "(C)";
                }
                if (allCandidateArray[i].pass)
                {
                    allCandidateArray[i].pass = pass;
                }
            }
            return allCandidateArrayC;
        }
        List<AccumulateData> allCandidateArrayD = new List<AccumulateData>();
        /*
         * 函式 checkDCompany 用來檢查 company 公司是否符合
         *      (d) 法人買超 2 日，且當日買超量大於成交量 10% (2 天都超過 50 張)
         */
        public Boolean checkDCompany(Company company, int index)
        {
            Double compareRate = 0.1;
            var returnValue = false;
            if (index == 0)
            {
                return returnValue;
            }
            var historyDataArray = company.getRealHistoryDataArray("d");
            var sVolumeToday = historyDataArray[index].s;
            var sVolumeYestoday = historyDataArray[index - 1].s;
            var sVolumeDayBeforeYestoday = historyDataArray[index - 2].s;
            var volumeToday = historyDataArray[index].v;
            var sDiff1 = sVolumeToday - sVolumeYestoday;
            var sDiff2 = sVolumeYestoday - sVolumeDayBeforeYestoday;
            var rate = sDiff1 / volumeToday;
            if ((sDiff1 >= 50) && (sDiff2 >= 50) && (rate > compareRate))
            {
                returnValue = true;
            }
            return returnValue;
        }
        /*
         * 函式 filterCandidateD 用來檢查所有關注的股票是否可以通過條件
         *      (D) 法人買超 2 日
         */
        public List<AccumulateData> filterCandidateD()
        {
            allCandidateArrayD = new List<AccumulateData>();
            for (var i = 0; i < allCandidateArray.Count(); i++)
            {
                var company = allCandidateArray[i].company;
                if (!company.passCheckDatabase)
                {
                    continue;
                }
                var pass = checkDCompany(company,
                    company.getRealHistoryDataArray("d").Length - 1);
                if (pass)
                {
                    AccumulateData candidate = new AccumulateData();
                    candidate.company = company;
                    candidate.id = allCandidateArray[i].id;
                    candidate.name = allCandidateArray[i].name;
                    candidate.pass = true;
                    allCandidateArrayD.Add(candidate);
                    company.candidateMatchCount++;
                    company.candidateMatchString = company.candidateMatchString + "(D)";
                }
                if (allCandidateArray[i].pass)
                {
                    allCandidateArray[i].pass = pass;
                }
            }
            return allCandidateArrayD;
        }
        /*
         * 函式 printCandidate 用來印出所有當天應讓關注的股票。
         * 傳入參數名稱由原本的 JavaScript 程式移植而來，不變更原來名稱，
         * 是為了和原本的 JavaScript 程式對照，在此參數型態是 List，
         * 而不是 Array 請小心。
         */
        public String printCandidate(List<AccumulateData> candidateArray)
        {
            var returnText = "";
            var count = 0;
            for (var i = 0; i < candidateArray.Count(); i++)
            {
                var name = candidateArray[i].name;
                var id = candidateArray[i].id;
                if (candidateArray[i].pass == true)
                {
                    count++;
                    returnText = returnText + "\t" + name + "(" + id + ")" + "\r\n";
                }
            }
            if (count == 0)
            {
                returnText = returnText + "\t沒有可以關注的股票\r\n";
            }
            else
            {
                returnText = returnText + "\r\n";
            }
            return returnText;
        }
        /*
         * 函式 printCandidateMatchCountString 用來印出滿足各條件的股票
         */
        public String printCandidateMatchCountString(int matchCount)
        {
            String matchString = "";
            for (int i = 0; i < stockDatabase.companies.Length; i++)
            {
                Company company = stockDatabase.companies[i];
                if (company.candidateMatchCount == matchCount)
                {
                    matchString = matchString + company.name + "(" + company.id + ")" +
                        "滿足 " + company.candidateMatchString + " " +
                        matchCount + " 個條件。\r\n";
                    if (matchCount >= 3)
                    {
                        matchString = matchString + company.getInformatioon(stockDatabase) + "\r\n";
                    }
                }
            }
            return matchString + "\r\n";
        }
        /*
         * 函式 filterCandidateE 用來篩選條件 E
         * 一個月內主力發動二次攻擊。
         * 攻擊的定義是低檔出現條件 C 或條件 D
         * 所謂低檔是指在過去 300 個交易日中(股票的最高價 maxClose ，股票最低價 minClose)
         * 股價比 minClose + 40% (maxClose-minClose) 還要低
         * 也就是股價位於底部往上 40% 以內
         * 此條件會針對所有股票進行篩選，而非只有受關注的股票。
         */
        public void filterCandidateE(int afterDay)
        {
            FileHelper fileHelper = new FileHelper();
            new MessageWriter().showMessage("二次攻擊篩選中，請稍待。\r\n");
            new AppDoEvents().DoEvents();
            for (int i = 0; i < stockDatabase.companies.Length; i++)
            {
                new WarningWriter().showMessage(i + "\r\n");
                new AppDoEvents().DoEvents();
                Company company = stockDatabase.companies[i];
                if (!company.passCheckDatabase)
                {
                    company.matchE = false;
                    company.matchF = false;
                    continue;
                }
                HistoryData[] dayHistoryDataArray = company.getRealHistoryDataArray("d");
                company.matchE = false;
                company.matchF = false;
                int dataLength = dayHistoryDataArray.Length;
                Double middleRate = 0.4;
                for (int k = dataLength - 1; k < dataLength; k++)
                {
                    if (checkCCompany(company, k))
                    {
                        int Year = Convert.ToInt32(dayHistoryDataArray[k].t.Substring(0, 4));
                        int Month = Convert.ToInt32(dayHistoryDataArray[k].t.Substring(5, 2));
                        int Day = Convert.ToInt32(dayHistoryDataArray[k].t.Substring(8, 2));
                        var newDate = new DateTime(Year, Month, Day);
                        /*
                         *  通過條件 C 測試的公司，在其資料庫中產生一個 CandidateCDsave.dat
                         *  的檔案，其中記錄下通過條件的日期及條件C。
                         *  並且檢查該公司是否已有此一檔案，若有此一檔案，再檢查檔案中日期和今日的
                         *  時間差是否在一個月內，若滿足以上條件，表示該公司於一個月內發動了二次攻
                         *  擊，此為條件 E。
                         */
                        var filename = "company/" + company.id + "/CandidateCDsave.dat";
                        if (fileHelper.Exists(filename))
                        {
                            var saveData = fileHelper.ReadText(filename);
                            var saveDataSplit = saveData.Split(new string[] { "\n" },
                                StringSplitOptions.RemoveEmptyEntries);
                            var oldDate = saveDataSplit[0];
                            var oldType = saveDataSplit[1];
                            int oldYear = Convert.ToInt32(oldDate.Substring(0, 4));
                            int oldMonth = Convert.ToInt32(oldDate.Substring(5, 2));
                            int oldDay = Convert.ToInt32(oldDate.Substring(8, 2));
                            var lastDate = new DateTime(oldYear, oldMonth, oldDay);
                            var diff = (newDate - lastDate).TotalDays;
                            if ((diff > 0) && (diff < 30) /*&& (oldYear > 2017)*/)
                            {
                                /* 以下為滿足二次攻擊條作的情況。
                                 * 因為二次攻擊有可能是連續三次攻擊的後二次，
                                 * 為了要抓到正確的前二次攻擊，在滿足二次攻擊條件時，
                                 * 先檢查最近是否有過二次攻擊。
                                 * 因此，在滿足二次攻擊時，產生一個 twiceAttack.dat
                                 * 檔案，裡面記綠最近的二次攻擊時間。
                                 */
                                var filenameAttack = "company/" + company.id + "/twiceAttack.dat";
                                if (fileHelper.Exists(filenameAttack))
                                {
                                    /*
                                     * 如果二次攻擊的時間檔案存在，讀出該時間，和目前時間
                                     * 做比較，如果時間在 2 個月之內，則算是連續攻擊。
                                     * 否則是新的二次攻擊，產生新的 twiceAttack.dat 
                                     * 攻擊時間檔。
                                     */
                                    var twiceAttackData = fileHelper.ReadText(filenameAttack);
                                    int prevYear = Convert.ToInt32(twiceAttackData.Substring(0, 4));
                                    int prevMonth = Convert.ToInt32(twiceAttackData.Substring(5, 2));
                                    int prevDay = Convert.ToInt32(twiceAttackData.Substring(8, 2));
                                    var prevDate = new DateTime(prevYear, prevMonth, prevDay);
                                    var prevDiff = (newDate - prevDate).TotalDays;
                                    /*
                                     * 在此要判斷二次攻擊當天股價是處於低檔或高檔。
                                     */
                                    Double highestPrice = 0;
                                    Double lowestPrice = Double.MaxValue;
                                    for (var d = k; d >= (k - afterDay); d--)
                                    {
                                        if (d >= 0)
                                        {
                                            if (dayHistoryDataArray[d].h > highestPrice)
                                            {
                                                highestPrice = dayHistoryDataArray[d].h;
                                            }
                                            if (dayHistoryDataArray[d].l < lowestPrice)
                                            {
                                                lowestPrice = dayHistoryDataArray[d].l;
                                            }
                                        }
                                    }
                                    Double middlePrice = lowestPrice + middleRate * (highestPrice - lowestPrice);

                                    if (prevDiff < 60)
                                    {
                                        /* 連續攻擊發生在 2 個月內，不做任何處理 */
                                    }
                                    else
                                    {
                                        /*
                                         * 連續攻擊發生在 2 個月以上，
                                         * 產生新的 twiceAttack.dat 攻擊時間檔。 
                                         */
                                        if ((dayHistoryDataArray[k].c < middlePrice))
                                        {
                                            /* 連續攻擊發生時為低檔 */
                                            company.matchE = true;
                                            company.getSeasonEPS();
                                            Double totalSeasonEPS = 0;
                                            int totalSeasonCount = 0;
                                            for (int s = company.seasonEPS.Length - 1; s >= 0; s--)
                                            {
                                                totalSeasonEPS = totalSeasonEPS + company.seasonEPS[s];
                                                totalSeasonCount = totalSeasonCount + 1;
                                                if (totalSeasonCount == 4)
                                                {
                                                    break;
                                                }
                                            }
                                            company.getDividend();
                                            Double dividend = company.dividend[company.dividend.Length - 1];
                                            CompanyInformation[] companyInformationArray = company.getMarginInformation();
                                            CompanyInformation companyInformation = companyInformationArray[companyInformationArray.Length - 1];
                                            Double shareRate = dayHistoryDataArray[k].c / companyInformation.bookValuePerShare;
                                            if ((dividend > 0) && (totalSeasonEPS > 0) && (shareRate < 1))
                                            {
                                                company.matchF = true;
                                            }
                                            else
                                            {
                                                company.matchF = false;
                                            }
                                        }
                                        fileHelper.WriteText(filenameAttack,
                                            timeToString(newDate) + "\n"
                                            );
                                    }
                                }
                                else
                                {
                                    /*
                                     * 如果二次攻擊時間檔不存在，表示是第一次發生，
                                     * 產生新的 twiceAttack.dat 攻擊時間檔。 
                                     */
                                    fileHelper.WriteText(filenameAttack,
                                        timeToString(newDate) + "\n"
                                        );
                                }
                            }
                        }
                        fileHelper.WriteText(filename,
                             timeToString(newDate) + "\n" +
                             "C" + "\n"
                            );
                    }
                    if (checkDCompany(company, k))
                    {
                        int Year = Convert.ToInt32(dayHistoryDataArray[k].t.Substring(0, 4));
                        int Month = Convert.ToInt32(dayHistoryDataArray[k].t.Substring(5, 2));
                        int Day = Convert.ToInt32(dayHistoryDataArray[k].t.Substring(8, 2));
                        var newDate = new DateTime(Year, Month, Day);
                        /*
                         *  通過條件 D 測試的公司，在其資料庫中產生一個 CandidateCDsave.dat
                         *  的檔案，其中記錄下通過條件的日期及條件D。
                         *  並且檢查該公司是否已有此一檔案，若有此一檔案，再檢查檔案中日期和今日的
                         *  時間差是否在一個月內，若滿足以上條件，表示該公司於一個月內發動了二次攻
                         *  擊，此為條件 E。
                         */
                        var filename = "company/" + company.id + "/CandidateCDsave.dat";
                        if (fileHelper.Exists(filename))
                        {
                            var saveData = fileHelper.ReadText(filename);
                            var saveDataSplit = saveData.Split(new string[] { "\n" },
                                StringSplitOptions.RemoveEmptyEntries);
                            var oldDate = saveDataSplit[0];
                            var oldType = saveDataSplit[1];
                            int oldYear = Convert.ToInt32(oldDate.Substring(0, 4));
                            int oldMonth = Convert.ToInt32(oldDate.Substring(5, 2));
                            int oldDay = Convert.ToInt32(oldDate.Substring(8, 2));
                            var lastDate = new DateTime(oldYear, oldMonth, oldDay);
                            var diff = (newDate - lastDate).TotalDays;
                            if ((diff > 0) && (diff < 30) /*&& (oldYear > 2017)*/)
                            {
                                /* 以下為滿足二次攻擊條作的情況。
                                 * 因為二次攻擊有可能是連續三次攻擊的後二次，
                                 * 為了要抓到正確的前二次攻擊，在滿足二次攻擊條件時，
                                 * 先檢查最近是否有過二次攻擊。
                                 * 因此，在滿足二次攻擊時，產生一個 twiceAttack.dat
                                 * 檔案，裡面記綠最近的二次攻擊時間。
                                 */
                                var filenameAttack = "company/" + company.id + "/twiceAttack.dat";
                                if (fileHelper.Exists(filenameAttack))
                                {
                                    /*
                                     * 如果二次攻擊的時間檔案存在，讀出該時間，和目前時間
                                     * 做比較，如果時間在 2 個月之內，則算是連續攻擊。
                                     * 否則是新的二次攻擊，產生新的 twiceAttack.dat 
                                     * 攻擊時間檔。
                                     */
                                    var twiceAttackData = fileHelper.ReadText(filenameAttack);
                                    int prevYear = Convert.ToInt32(twiceAttackData.Substring(0, 4));
                                    int prevMonth = Convert.ToInt32(twiceAttackData.Substring(5, 2));
                                    int prevDay = Convert.ToInt32(twiceAttackData.Substring(8, 2));
                                    var prevDate = new DateTime(prevYear, prevMonth, prevDay);
                                    var prevDiff = (newDate - prevDate).TotalDays;
                                    /*
                                     * 在此要判斷二次攻擊當天股價是處於低檔或高檔。
                                     */
                                    Double highestPrice = 0;
                                    Double lowestPrice = Double.MaxValue;
                                    for (var d = k; d >= (k - afterDay); d--)
                                    {
                                        if (d >= 0)
                                        {
                                            if (dayHistoryDataArray[d].h > highestPrice)
                                            {
                                                highestPrice = dayHistoryDataArray[d].h;
                                            }
                                            if (dayHistoryDataArray[d].l < lowestPrice)
                                            {
                                                lowestPrice = dayHistoryDataArray[d].l;
                                            }
                                        }
                                    }
                                    Double middlePrice = lowestPrice + middleRate * (highestPrice - lowestPrice);

                                    if (prevDiff < 60)
                                    {
                                        /* 連續攻擊發生在 2 個月內，不做任何處理 */

                                    }
                                    else
                                    {
                                        /*
                                         * 連續攻擊發生在 2 個月以上，
                                         * 產生新的 twiceAttack.dat 攻擊時間檔。 
                                         */
                                        if ((dayHistoryDataArray[k].c < middlePrice))
                                        {
                                            /* 連續攻擊發生時為低檔 */
                                            company.matchE = true;
                                            company.getSeasonEPS();
                                            Double totalSeasonEPS = 0;
                                            int totalSeasonCount = 0;
                                            for (int s = company.seasonEPS.Length - 1; s >= 0; s--)
                                            {
                                                totalSeasonEPS = totalSeasonEPS + company.seasonEPS[s];
                                                totalSeasonCount = totalSeasonCount + 1;
                                                if (totalSeasonCount == 4)
                                                {
                                                    break;
                                                }
                                            }
                                            company.getDividend();
                                            Double dividend = company.dividend[company.dividend.Length - 1];
                                            CompanyInformation[] companyInformationArray = company.getMarginInformation();
                                            CompanyInformation companyInformation = companyInformationArray[companyInformationArray.Length - 1];
                                            Double shareRate = dayHistoryDataArray[k].c / companyInformation.bookValuePerShare;
                                            if ((dividend > 0) && (totalSeasonEPS > 0) && (shareRate < 1))
                                            {
                                                company.matchF = true;
                                            }
                                            else
                                            {
                                                company.matchF = false;
                                            }
                                        }
                                        fileHelper.WriteText(filenameAttack,
                                            timeToString(newDate) + "\n"
                                            );
                                    }
                                }
                                else
                                {
                                    /*
                                     * 如果二次攻擊時間檔不存在，表示是第一次發生，
                                     * 產生新的 twiceAttack.dat 攻擊時間檔。 
                                     */
                                    fileHelper.WriteText(filenameAttack,
                                        timeToString(newDate) + "\n"
                                        );
                                }
                            }
                        }
                        fileHelper.WriteText(filename,
                            timeToString(newDate) + "\n" +
                            "D" + "\n"
                            );
                    }
                }
            }
        }
        /*
         * strongFilter 函式用來把通過連續二次主力攻撀篩選的股票，再
         * 強力篩選一次，篩選的條件是：
         *      (1) 每年股利無負值(含零值)
         *      (2) 每季 EPS 無負值(含零值)
         *      (3) 每年 EPS 無負值(含零值)
         *      (4) 日k、週k、月k值都小於20
         *      (5) 法人十日內買超 0.2% 以上
         *      (6) 低檔(比前高少70%)以上
         */
        private void strongFilter()
        {
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                Company company = stockDatabase.companies[i];
                if (company.matchF)
                {
                    company.checkMatchG();
                }
            }
        }
        /*
         * 函式 filterCandidateArray 從各個 candidateArrayX 中，取出
         * 股票資料，過濾出符合下列條件的股票：
         *      (A) 過去 5 年中，最高點股價至今下跌至少 50% 以上
         *      (B) 股利推估有 5% 以上
         *      (C) 爆量上漲
         *      (D) 法人買超 2 日，且當日買超量大於成交量 10%
         *      (E) 一個月內主力發動二次攻擊。
         *      
         *  20180818
         *      目前已不使用 (A)-(E) 過濾的資訊，改用純粹二次攻擊的
         *      分析，一次主力攻擊是表示出現 (C) (D) 其中之一的情形，
         *      二次攻擊是指一個月內出現二次主力攻擊的意思。
         *      連續二次攻擊是指二月內出現二次的二次主力攻擊的意思。
         */
        public String filterCandidateArray()
        {
            /* 
             * 進行分析及篩選前先檢查資料庫的完整性 
             * 若資料庫有缺，則不做該公司的分析及篩選。
             */
            allCandidateArray = new List<AccumulateData>();
            createAllCandidateArray();
            var printAll = printCandidate(allCandidateArray);
            var allCandidateArrayA = filterCandidateA();
            var printCandidateA = printCandidate(allCandidateArrayA);
            var allCandidateArrayB = filterCandidateB();
            var printCandidateB = printCandidate(allCandidateArrayB);
            var allCandidateArrayC = filterCandidateC();
            var printCandidateC = printCandidate(allCandidateArrayC);
            var printABC = printCandidate(allCandidateArray);
            var allCandidateArrayD = filterCandidateD();
            var printCandidateD = printCandidate(allCandidateArrayD);
            var printABCD = printCandidate(allCandidateArray);
            /*
             * 二次攻擊篩選
             * (A) 各公司的 matchE 表示一個月內多頭主力發動連續二次攻擊(無過濾)。
             * (B) 各公司的 matchF 一個月內多頭主力發動連續二次攻擊(有過濾)。
             *      過濾條件：營收沒有負值、有發股利，目前股價必須小於每股淨值。
             * 在顯示給使用者看時，分別變成滿足 (A) 及 (B) 條件。
             */
            filterCandidateE(300);
            strongFilter();

            var printMatch4 = printCandidateMatchCountString(4);
            var printMatch3 = printCandidateMatchCountString(3);
            var printMatch2 = printCandidateMatchCountString(2);
            var printMatch1 = printCandidateMatchCountString(1);

            /*
             * 印出二次攻擊結果 
             */
            var returnText = "請關注：\r\n\r\n";
            /*
            returnText = returnText +
              "\t(A) 過去 5 年中，最高點股價至今下跌至少 50% 以上\r\n" +
              "\t(B) 股利推估有 5% 以上\r\n" +
              "\t(C) 爆量上漲(單日上漲超過 3% ，成交量超過前 100 天平均 3 倍)\r\n" +
              "\t(D) 法人買超 2 日，且當日買超量大於成交量 10%\r\n" +
              "\t(E) 一個月內多頭主力發動連續二次攻擊(無過濾)。\r\n" +
              "\t(F) 一個月內多頭主力發動連續二次攻擊(有過濾)。\r\n" +
              "\t\t過濾條件：營收沒有負值、有發股利，目前股價必須小於每股淨值。\r\n\r\n";
             */
            returnText = returnText +
                "分析及篩選主要是以搜尋主力攻擊為主，亦即籌碼作用為主，\r\n" +
                "所謂主力攻擊是指下列二項事件發生時：\r\n" +
                "\t(1) 爆量上漲 (單日上漲超過 3% ，成交量超過前 100 天平均 3 倍，最少要 500 張)\r\n" +
                "\t(2) 法人買超 2 日，且當日買超量大於成交量 10% (2天都要超過 50 張)\r\n\r\n" +
                "\t    連續二次攻擊是指二月內出現二次的二次主力攻擊的意思。\r\n\r\n" +
                "篩選的條件是如下列：\r\n" +
                "\t(A) 一個月內多頭主力發動連續二次主力攻擊(無過濾)。\r\n" +
                "\t(B) 一個月內多頭主力發動連續二次主力攻擊(有過濾)。\r\n" +
                "\t\t過濾條件：營收沒有負值、有發股利，目前股價必須小於每股淨值。\r\n\r\n";
            int attackCount = 0;
            var printMatchE = "滿足條件 (A) 的股票：\r\n";
            for (int i = 0; i < stockDatabase.companies.Length; i++)
            {
                Company company = stockDatabase.companies[i];
                if (company.matchE)
                {
                    printMatchE = printMatchE +
                        company.name + "(" + company.id + ")" + "\r\n";
                    attackCount++;
                }
            }
            if (attackCount == 0)
            {
                printMatchE = printMatchE +
                    "\t沒有滿足 (A) 的股票。\r\n";
            }
            printMatchE = printMatchE +
                    "\r\n";
            // returnText = returnText + printMatchE;
            attackCount = 0;
            var printMatchF = "滿足條件 (B) 的股票：\r\n";
            for (int i = 0; i < stockDatabase.companies.Length; i++)
            {
                Company company = stockDatabase.companies[i];
                if (company.matchF)
                {

                    printMatchF = printMatchF +
                        company.getInformatioon(stockDatabase) + "\r\n";

                    attackCount++;
                }
            }
            if (attackCount == 0)
            {
                printMatchF = printMatchF +
                    "\t沒有滿足 (B) 的股票。\r\n";
            }
            else
            {
                printMatchF = printMatchF + stockDatabase.getInformation();
            }
            printMatchF = printMatchF +
                    "\r\n";
            returnText = returnText + printMatchF + printMatchE + "\r\n";
            /*
            returnText = returnText + printMatchF + printMatchE + printMatch4 + printMatch3 +
                printMatch2 + printMatch1;

            returnText = returnText + "滿足條件 (A)(B)(C)(D) 的股票：\r\n" +
              printABCD + "\r\n";
            returnText = returnText + "滿足條件 (A)(B)(C) 的股票：\r\n" +
              printABC + "\r\n";

            returnText = returnText + "所有可關注的股票：\r\n" +
              printAll;

            returnText = returnText + "滿足條件 (A) 的股票：\r\n" +
              printCandidateA + "\r\n";
            returnText = returnText + "滿足條件 (B) 的股票：\r\n" +
              printCandidateB + "\r\n";
            returnText = returnText + "滿足條件 (C) 的股票：\r\n" +
              printCandidateC + "\r\n";
            returnText = returnText + "滿足條件 (D) 的股票：\r\n" +
              printCandidateD + "\r\n";
            */
            return returnText;
        }
        /* 
         * 函式 findCandidate 用來在所有股票中找尋可能的飇股，做法如下：
         * 1.  每天評分的結果，將前 20 名股票，累積記綠到各股資料庫中的 
         *      scoreAccumulation.dat 檔案中。
         *      記錄的資訊包括：
         *          (a).  最後累積評分的日期
         *          (b).  累積的評分 = 前次累積評分 + 今天的評分
         *          (c).  到目前最低股價
         *      此項工作是在 saveAllAccumulateScore 函式中進行
         * 2.  當發生下列情形時，重設累積分數為 0，各股預設初始資訊也是如此
         *          (a).  最後累積評分的日期到今天超過 10 天以上，也就是
         *                說該股已脫出前 20 名評分 10 天以上。
         *          (b).  上漲超過 10% 以上(和目前最低股價相比)
         *      此項工作是在 resetAccumulateScore 函式中進行
         * 3.  每天評分完畢後，印出並存檔
         *          (a) 累積評分 X 脫出日數
         *              按排名依序列出所有股票(脫出 2 日以上)
         *          (b) 未脫出時，排名下降達 10 名以上股票(脫出趨勢強)
         *          (c) 未脫出時，排名下降達 5 名以上股票(脫出趨勢弱)
         *          (d) 不論是否脫出，法人投信外資(合計)大買股票
         *          (e) 不論是否脫出，成交量突然放大 5 倍以上股票(脫出趨勢強)
         *          (f) 不論是否脫出，成交量突然放大 2 倍以上股票(脫出趨勢弱)
         */
        public class FindCandidateClass
        {
            public String returnTextReset;
            public String returnTextPrintAndSave;
            public String returnTextFilterCandidate;
        }
        public FindCandidateClass findCandidate()
        {
            new MessageWriter().showMessage("開始進行需要關注股票的挑選工作，請耐心等候。");
            new AppDoEvents().DoEvents();
            // System.Windows.Forms.Application.DoEvents();
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                stockDatabase.companies[i].checkDatabase();
            }
            var returnTextReset = "";
            saveAllAccumulateScore();
            returnTextReset = returnTextReset + resetAccumulateScore() + "　\r\n\r\n\r\n";
            var returnTextPrintAndSave = "\r\n";
            returnTextPrintAndSave = returnTextPrintAndSave + printAndSaveInfomation() + "　\r\n\r\n\r\n";
            var returnTextFilterCandidate = "\r\n";
            returnTextFilterCandidate = returnTextFilterCandidate + filterCandidateArray() + "　\r\n\r\n\r\n";
            var filename = "twStock/everyDayAnalysisAndFilter.html";
            var nowTime = DateTime.Now;
            FileHelper fileHelper = new FileHelper();
            if (fileHelper.Exists(filename))
            {
                String originText = fileHelper.ReadText(filename);
                fileHelper.WriteText(filename, originText + "" + nowTime + "\r\n" +
                    returnTextFilterCandidate + returnTextReset + returnTextPrintAndSave + "\n\n\n");
            }
            else
            {
                fileHelper.WriteText(filename, nowTime + "\r\n" + returnTextFilterCandidate + returnTextReset + returnTextPrintAndSave + "\n\n\n");
            }
            FindCandidateClass findCandidateObject = new FindCandidateClass();
            findCandidateObject.returnTextReset = returnTextReset;
            findCandidateObject.returnTextPrintAndSave = returnTextPrintAndSave;
            findCandidateObject.returnTextFilterCandidate = returnTextFilterCandidate;
            return findCandidateObject;
        }
        /* 
         * 函式 doShort1ScoreAnalysis 呼叫 doAnalysis 進行短線分析及評分，
         * 由於加入了篩選的程式，不同的分析及評分方法(長、短線)影響差距不明顯，
         * 所以基本上都用此分析及評分函式即可。 
         * 此分析方法的特色是每種技術分析結果加 1 分，也就是認定每項技術分析的
         * 勢力影響都一樣，各佔比 1 分。
         * 初看此評分方法不合理，實質上，沒有可能找出各項評分的佔比，即使是用
         * 最佳化的方法也一樣，因為人心隨時都會因環境而改變，任何方法找出的佔比
         * 也只可能適用過去的結果，而非適用於未來。
         */
        public String doShort1ScoreAnalysis(int pCount)
        {
            // analysisType = "ShortTerm1";
            scoreValueArray = shortTermScoreValueArray;
            int[] shortTempScoreValueArray = new int[1000];
            for (var i = 0; i < scoreValueArray.Length; i++)
            {
                if (scoreValueArray[i] > 0)
                {
                    shortTempScoreValueArray[i] = 1;
                }
                else
                {
                    shortTempScoreValueArray[i] = -1;
                }
            }
            scoreValueArray = shortTempScoreValueArray;
            var printText = doAnalysis(pCount);
            saveAnalysisScore("shortTerm1Score.dat");
            saveEveryDayScore();
            /* 
            FindCandidateClass returnObjectFindCandidate = findCandidate();
            printText = returnObjectFindCandidate.returnTextFilterCandidate +
                returnObjectFindCandidate.returnTextReset +
                returnObjectFindCandidate.returnTextPrintAndSave + printText;
             * */
            return printText;
        }
        public String doFindCandidate()
        {
            var printText = "";
            FindCandidateClass returnObjectFindCandidate = findCandidate();
            printText = returnObjectFindCandidate.returnTextFilterCandidate +
                returnObjectFindCandidate.returnTextReset +
                returnObjectFindCandidate.returnTextPrintAndSave + printText;
            return printText;
        }
        /*
         * 函式 doAttackAnalysis 用來測試所有公司股票發生二次攻擊後，股價會上升的機率。
         * 參數 afterDay 是二次攻擊發生後， 找尋 afterDay 中股價最大值，和攻擊當日收盤價
         * 做比較，如果有大於 10%，則上漲計數値 advCount 加 1，否則 decCount 加 1，
         * 最後用 advCount 及 decCount 計算上漲機率。
         * 
         */
        public void doAttackAnalysis(int beforeDay, int afterDay, Boolean noMessage)
        {
            FileHelper fileHelper = new FileHelper();
            /*
             * advCount：二次攻擊後，afterDay 天中收盤價最大值，有上升 10% 時加 1。
             * decCount：二次攻擊後，afterDay 天中收盤價最大值，沒有上升 10% 時加 1。
             */
            int advCount = 0;
            int decCount = 0;
            /*
             * advRate : 用來計算二次攻擊後，上漲幅度的比例之平均值。
             * decRate : 用來計算二次攻擊後，下跌幅度的比例之平均值。
             * advRateCount : 二次攻擊後，實質上漲的次數。
             * decRateCount : 二次攻擊後，實質下跌的次數。
             */
            Double advRate = 0;
            Double decRate = 0;
            int advRateCount = 0;
            int decRateCount = 0;
            for (int i = 0; i < stockDatabase.companies.Length; i++)
            {
                new WarningWriter().showMessage(i + "\r\n");
                new AppDoEvents().DoEvents();
                Company company = stockDatabase.companies[i];
                if (fileHelper.Exists("company/" + company.id + "/CandidateCDsave.dat"))
                {
                    fileHelper.Delete("company/" + company.id + "/CandidateCDsave.dat");
                }
                if (fileHelper.Exists("company/" + company.id + "/twiceAttack.dat"))
                {
                    fileHelper.Delete("company/" + company.id + "/twiceAttack.dat");
                }
                HistoryData[] dayHistoryDataArray = company.getRealHistoryDataArray("d");
                int dataLength = dayHistoryDataArray.Length;
                Double middleRate = 0.4;
                // int testYear = 2011;
                for (int k = 2; k < dataLength - 1; k++)
                {
                    if (checkCCompany(company, k))
                    {
                        /* 
                            new MessageWriter().appendMessage(
                            company.name + "(" + company.id + ") " +
                            dayHistoryDataArray[k].t + " 發動 C 攻擊一次\r\n",
                            true
                            );
                         */
                        int Year = Convert.ToInt32(dayHistoryDataArray[k].t.Substring(0, 4));
                        int Month = Convert.ToInt32(dayHistoryDataArray[k].t.Substring(5, 2));
                        int Day = Convert.ToInt32(dayHistoryDataArray[k].t.Substring(8, 2));
                        var newDate = new DateTime(Year, Month, Day);
                        /*
                         *  通過條件 C 測試的公司，在其資料庫中產生一個 CandidateCDsave.dat
                         *  的檔案，其中記錄下通過條件的日期及條件C。
                         *  並且檢查該公司是否已有此一檔案，若有此一檔案，再檢查檔案中日期和今日的
                         *  時間差是否在一個月內，若滿足以上條件，表示該公司於一個月內發動了二次攻
                         *  擊，此為條件 E。
                         */
                        var filename = "company/" + company.id + "/CandidateCDsave.dat";
                        if (fileHelper.Exists(filename))
                        {
                            var saveData = fileHelper.ReadText(filename);
                            var saveDataSplit = saveData.Split(new string[] { "\n" },
                                StringSplitOptions.RemoveEmptyEntries);
                            var oldDate = saveDataSplit[0];
                            var oldType = saveDataSplit[1];
                            int oldYear = Convert.ToInt32(oldDate.Substring(0, 4));
                            int oldMonth = Convert.ToInt32(oldDate.Substring(5, 2));
                            int oldDay = Convert.ToInt32(oldDate.Substring(8, 2));
                            var lastDate = new DateTime(oldYear, oldMonth, oldDay);
                            var diff = (newDate - lastDate).TotalDays;
                            if ((diff > 0) && (diff < 30) /*&& (oldYear > 2017)*/)
                            {
                                /* 以下為滿足二次攻擊條作的情況。
                                 * 因為二次攻擊有可能是連續三次攻擊的後二次，
                                 * 為了要抓到正確的前二次攻擊，在滿足二次攻擊條件時，
                                 * 先檢查最近是否有過二次攻擊。
                                 * 因此，在滿足二次攻擊時，產生一個 twiceAttack.dat
                                 * 檔案，裡面記綠最近的二次攻擊時間。
                                new MessageWriter().appendMessage(
                                    company.name + "(" + company.id + ") " +
                                    timeToString(lastDate) + "/" + oldType + " " +
                                    timeToString(newDate) + "/C" + " 發動二次攻擊\r\n",
                                    true
                                    );
                                 */
                                var filenameAttack = "company/" + company.id + "/twiceAttack.dat";
                                if (fileHelper.Exists(filenameAttack))
                                {
                                    /*
                                     * 如果二次攻擊的時間檔案存在，讀出該時間，和目前時間
                                     * 做比較，如果時間在 2 個月之內，則算是連續攻擊。
                                     * 否則是新的二次攻擊，產生新的 twiceAttack.dat 
                                     * 攻擊時間檔。
                                     */
                                    var twiceAttackData = fileHelper.ReadText(filenameAttack);
                                    int prevYear = Convert.ToInt32(twiceAttackData.Substring(0, 4));
                                    int prevMonth = Convert.ToInt32(twiceAttackData.Substring(5, 2));
                                    int prevDay = Convert.ToInt32(twiceAttackData.Substring(8, 2));
                                    var prevDate = new DateTime(prevYear, prevMonth, prevDay);
                                    var prevDiff = (newDate - prevDate).TotalDays;
                                    /*
                                     * 在此要判斷二次攻擊當天股價是處於低檔或高檔。
                                     */
                                    Double highestPrice = 0;
                                    Double lowestPrice = Double.MaxValue;
                                    for (var d = k; d >= (k - beforeDay); d--)
                                    {
                                        if (d >= 0)
                                        {
                                            if (dayHistoryDataArray[d].h > highestPrice)
                                            {
                                                highestPrice = dayHistoryDataArray[d].h;
                                            }
                                            if (dayHistoryDataArray[d].l < lowestPrice)
                                            {
                                                lowestPrice = dayHistoryDataArray[d].l;
                                            }
                                        }
                                    }
                                    Double middlePrice = lowestPrice + middleRate * (highestPrice - lowestPrice);

                                    if (prevDiff < 60)
                                    {
                                        /* 連續攻擊發生在 2 個月內，不做任何處理 */
                                    }
                                    else
                                    {
                                        if ((dayHistoryDataArray[k].c < middlePrice)/* && (Year == testYear)*/)
                                        {
                                            /* 連續攻擊發生時為低檔 */
                                            /*
                                             * 連續攻擊發生在 2 個月以上，
                                             * 產生新的 twiceAttack.dat 攻擊時間檔。 
                                             */
                                            /*
                                             * 檢查是否距最後一筆資料超過 afterDay 個交易日，
                                             * 成立時，檢查 afterDay 天中收盤價是否有上漲。
                                             */
                                            if (k < (dataLength - afterDay))
                                            {
                                                Double attackDayClose = dayHistoryDataArray[k].c;
                                                Double maxClose = 0;
                                                for (var d = k + 30; d < (k + afterDay); d++)
                                                {
                                                    if (dayHistoryDataArray[d].c > maxClose)
                                                    {
                                                        maxClose = dayHistoryDataArray[d].c;
                                                    }
                                                }
                                                if (maxClose > (attackDayClose * 1.1))
                                                {
                                                    /*
                                                     * 10% = 1.1 倍
                                                     */
                                                    advCount++;
                                                    advRateCount++;
                                                    advRate = advRate +
                                                        (maxClose - attackDayClose) / attackDayClose;
                                                }
                                                else
                                                {
                                                    decCount++;
                                                    if (maxClose > attackDayClose)
                                                    {
                                                        advRateCount++;
                                                        advRate = advRate +
                                                            (maxClose - attackDayClose) / attackDayClose;
                                                    }
                                                    else
                                                    {
                                                        decRateCount++;
                                                        decRate = decRate +
                                                            (maxClose - attackDayClose) / attackDayClose;
                                                    }
                                                }
                                            }
                                        }
                                        fileHelper.WriteText(filenameAttack,
                                            timeToString(newDate) + "\n"
                                            );
                                    }
                                }
                                else
                                {
                                    /*
                                     * 如果二次攻擊時間檔不存在，表示是第一次發生，
                                     * 產生新的 twiceAttack.dat 攻擊時間檔。 
                                     */
                                    fileHelper.WriteText(filenameAttack,
                                        timeToString(newDate) + "\n"
                                        );
                                }
                            }
                        }
                        fileHelper.WriteText(filename,
                             timeToString(newDate) + "\n" +
                             "C" + "\n"
                            );
                    }
                    if (checkDCompany(company, k))
                    {
                        /*
                        new MessageWriter().appendMessage(
                            company.name + "(" + company.id + ") " +
                            dayHistoryDataArray[k].t + " 發動 D 攻擊一次\r\n",
                            true
                            );
                        */
                        int Year = Convert.ToInt32(dayHistoryDataArray[k].t.Substring(0, 4));
                        int Month = Convert.ToInt32(dayHistoryDataArray[k].t.Substring(5, 2));
                        int Day = Convert.ToInt32(dayHistoryDataArray[k].t.Substring(8, 2));
                        var newDate = new DateTime(Year, Month, Day);
                        /*
                         *  通過條件 D 測試的公司，在其資料庫中產生一個 CandidateCDsave.dat
                         *  的檔案，其中記錄下通過條件的日期及條件D。
                         *  並且檢查該公司是否已有此一檔案，若有此一檔案，再檢查檔案中日期和今日的
                         *  時間差是否在一個月內，若滿足以上條件，表示該公司於一個月內發動了二次攻
                         *  擊，此為條件 E。
                         */
                        var filename = "company/" + company.id + "/CandidateCDsave.dat";
                        if (fileHelper.Exists(filename))
                        {
                            var saveData = fileHelper.ReadText(filename);
                            var saveDataSplit = saveData.Split(new string[] { "\n" },
                                StringSplitOptions.RemoveEmptyEntries);
                            var oldDate = saveDataSplit[0];
                            var oldType = saveDataSplit[1];
                            int oldYear = Convert.ToInt32(oldDate.Substring(0, 4));
                            int oldMonth = Convert.ToInt32(oldDate.Substring(5, 2));
                            int oldDay = Convert.ToInt32(oldDate.Substring(8, 2));
                            var lastDate = new DateTime(oldYear, oldMonth, oldDay);
                            var diff = (newDate - lastDate).TotalDays;
                            if ((diff > 0) && (diff < 30) /*&& (oldYear > 2017)*/)
                            {
                                /* 以下為滿足二次攻擊條作的情況。
                                 * 因為二次攻擊有可能是連續三次攻擊的後二次，
                                 * 為了要抓到正確的前二次攻擊，在滿足二次攻擊條件時，
                                 * 先檢查最近是否有過二次攻擊。
                                 * 因此，在滿足二次攻擊時，產生一個 twiceAttack.dat
                                 * 檔案，裡面記綠最近的二次攻擊時間。
                                new MessageWriter().appendMessage(
                                    company.name + "(" + company.id + ") " +
                                    timeToString(lastDate) + "/" + oldType + " " +
                                    timeToString(newDate) + "/C" + " 發動二次攻擊\r\n",
                                    true
                                    );
                                 */
                                var filenameAttack = "company/" + company.id + "/twiceAttack.dat";
                                if (fileHelper.Exists(filenameAttack))
                                {
                                    /*
                                     * 如果二次攻擊的時間檔案存在，讀出該時間，和目前時間
                                     * 做比較，如果時間在 2 個月之內，則算是連續攻擊。
                                     * 否則是新的二次攻擊，產生新的 twiceAttack.dat 
                                     * 攻擊時間檔。
                                     */
                                    var twiceAttackData = fileHelper.ReadText(filenameAttack);
                                    int prevYear = Convert.ToInt32(twiceAttackData.Substring(0, 4));
                                    int prevMonth = Convert.ToInt32(twiceAttackData.Substring(5, 2));
                                    int prevDay = Convert.ToInt32(twiceAttackData.Substring(8, 2));
                                    var prevDate = new DateTime(prevYear, prevMonth, prevDay);
                                    var prevDiff = (newDate - prevDate).TotalDays;
                                    /*
                                     * 在此要判斷二次攻擊當天股價是處於低檔或高檔。
                                     */
                                    Double highestPrice = 0;
                                    Double lowestPrice = Double.MaxValue;
                                    for (var d = k; d >= (k - beforeDay); d--)
                                    {
                                        if (d >= 0)
                                        {
                                            if (dayHistoryDataArray[d].h > highestPrice)
                                            {
                                                highestPrice = dayHistoryDataArray[d].h;
                                            }
                                            if (dayHistoryDataArray[d].l < lowestPrice)
                                            {
                                                lowestPrice = dayHistoryDataArray[d].l;
                                            }
                                        }
                                    }
                                    Double middlePrice = lowestPrice + middleRate * (highestPrice - lowestPrice);

                                    if (prevDiff < 60)
                                    {
                                        /* 連續攻擊發生在 2 個月內，不做任何處理 */

                                    }
                                    else
                                    {
                                        if ((dayHistoryDataArray[k].c < middlePrice)/* && (Year == testYear)*/)
                                        {
                                            /* 連續攻擊發生時為低檔 */
                                            /*
                                             * 連續攻擊發生在 2 個月以上，
                                             * 產生新的 twiceAttack.dat 攻擊時間檔。 
                                             */
                                            /*
                                             * 檢查是否距最後一筆資料超過 afterDay 個交易日，
                                             * 成立時，檢查 afterDay 天中收盤價是否有上漲。
                                             */
                                            if (k < (dataLength - afterDay))
                                            {
                                                Double attackDayClose = dayHistoryDataArray[k].c;
                                                Double maxClose = 0;
                                                for (var d = k + 30; d < (k + afterDay); d++)
                                                {
                                                    if (dayHistoryDataArray[d].c > maxClose)
                                                    {
                                                        maxClose = dayHistoryDataArray[d].c;
                                                    }
                                                }
                                                if (maxClose > (attackDayClose * 1.1))
                                                {
                                                    /*
                                                     * 10% = 1.1 倍
                                                     */
                                                    advCount++;
                                                    advRateCount++;
                                                    advRate = advRate +
                                                        (maxClose - attackDayClose) / attackDayClose;
                                                }
                                                else
                                                {
                                                    decCount++;
                                                    if (maxClose > attackDayClose)
                                                    {
                                                        advRateCount++;
                                                        advRate = advRate +
                                                            (maxClose - attackDayClose) / attackDayClose;
                                                    }
                                                    else
                                                    {
                                                        decRateCount++;
                                                        decRate = decRate +
                                                            (maxClose - attackDayClose) / attackDayClose;
                                                    }
                                                }
                                            }
                                        }
                                        fileHelper.WriteText(filenameAttack,
                                            timeToString(newDate) + "\n"
                                            );
                                    }
                                }
                                else
                                {
                                    /*
                                     * 如果二次攻擊時間檔不存在，表示是第一次發生，
                                     * 產生新的 twiceAttack.dat 攻擊時間檔。 
                                     */
                                    fileHelper.WriteText(filenameAttack,
                                        timeToString(newDate) + "\n"
                                        );
                                }
                            }
                        }
                        fileHelper.WriteText(filename,
                            timeToString(newDate) + "\n" +
                            "D" + "\n"
                            );
                    }
                }
            }
            if (!noMessage)
            {
                new MessageWriter().appendMessage(
                    "advCount = " + advCount + "\r\n" +
                    "decCount = " + decCount + "\r\n" +
                    "advRateCount = " + advRateCount + "\r\n" +
                    "decRateCount = " + decRateCount + "\r\n" +
                    "Average of advRate = " + (advRate / advRateCount) + "\r\n" +
                    "Average of decRate = " + (decRate / decRateCount)
                    , true
                    );
            }
        }
    }
}
