using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stock
{
    class TraceCompany
    {
        public StockDatabase stockDatabase;
        public String id;                   // 被追踪股票的代號
        public String name;                 // 被追踪股票的名稱
        public DateTime date;               // 被追踪股票加入追踪的日期
        public Int32 startScore;            // 被追踪股票加入追踪當日的分數
        public Int32 score;                 // 被追踪股票今天的分數
        public Int32 count;                 // 被追踪股票被要求加入追踪的次數，
        // 短時間內被加入追踪多次，必定上漲機率極高
        public Double upPercent;            // 被追踪股票從加入那天到今天的上漲幅度
        public String type;                 // 被追踪股票的類型，
        //  B 双重二次攻擊法(有篩選)
        //  C 双重二次攻擊法(超強過濾)
        public Double startPrice;           // 被追踪股票加入追踪當日的股價
        public Double kValueDay;            // 被追踪股票加入追踪當日的日 K 值
        public Double kValueWeek;           // 被追踪股票加入追踪當日的週 K 值
        public Double kValueMonth;          // 被追踪股票加入追踪當日的月 K 值
        public Double kValueTWStockDay;     // 被追踪股票加入追踪當日的大盤日 K 值
        public Double kValueTWStockWeek;    // 被追踪股票加入追踪當日的大盤週 K 值
        public Double kValueTWStockMonth;   // 被追踪股票加入追踪當日的大盤月 K 值


        public TraceCompany(Company company, String type, StockDatabase stockDatabase)
        {
            this.stockDatabase = stockDatabase;
            this.id = company.id;
            this.name = company.name;
            this.date = DateTime.Now;
            this.count = 1;
            this.startScore = 0;
            this.score = 0;

            /* 取得股票追踪當日 K 值 */
            HistoryData[] dayHistoryData = company.getRealHistoryDataArray("d");
            KDJ[] kdjValueArray = company.kValue(company.getHistoryData80(dayHistoryData));
            kValueDay = kdjValueArray[kdjValueArray.Length - 1].K;
            HistoryData[] weekHistoryData = company.getRealHistoryDataArray("w");
            kdjValueArray = company.kValue(company.getHistoryData80(weekHistoryData));
            kValueWeek = kdjValueArray[kdjValueArray.Length - 1].K;
            HistoryData[] monthHistoryData = company.getRealHistoryDataArray("m");
            kdjValueArray = company.kValue(company.getHistoryData80(monthHistoryData));
            kValueMonth = kdjValueArray[kdjValueArray.Length - 1].K;

            /* 取得股票追踪當日大盤 K 值 */
            HistoryData[] dayHistoryDataTWStock = stockDatabase.getDayHistoryData();
            kdjValueArray = stockDatabase.kValue(stockDatabase.getHistoryData80(dayHistoryDataTWStock));
            kValueTWStockDay = kdjValueArray[kdjValueArray.Length - 1].K;
            HistoryData[] weekHistoryDataTWStock = company.getRealHistoryDataArray("w");
            kdjValueArray = stockDatabase.kValue(stockDatabase.getHistoryData80(weekHistoryDataTWStock));
            kValueTWStockWeek = kdjValueArray[kdjValueArray.Length - 1].K;
            HistoryData[] monthHistoryDataTWStock = company.getRealHistoryDataArray("m");
            kdjValueArray = stockDatabase.kValue(stockDatabase.getHistoryData80(monthHistoryDataTWStock));
            kValueTWStockMonth = kdjValueArray[kdjValueArray.Length - 1].K;

            this.startPrice = dayHistoryData[dayHistoryData.Length - 1].c;
            this.type = type;
        }
    }
    class Trace
    {
        public StockDatabase stockDatabase;
        private List<TraceCompany> traceCompanyList = null;
        public Int32 successCount;      // 記錄 30 天內上漲超過 10% 的股票個數
        public Int32 failCount;         // 記錄 30 天內上漲不超過 10% 的股票個數

        /*
         * Trace 建構式
         */
        public Trace(StockDatabase stockDatabase)
        {
            this.stockDatabase = stockDatabase;
            traceCompanyList = new List<TraceCompany>();
        }
        /*
         * 函式 addCompany 用來將 company 以類型 type 加入到追踪系統中
         */
        public void addCompany(Company company, String type)
        {
            traceCompanyList.Add(new TraceCompany(company, type, stockDatabase));
        }
    }
}
