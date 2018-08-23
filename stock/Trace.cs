using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stock
{
    class TraceCompany
    {
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
        private Double startPrice;          // 被追踪股票加入追踪當日的股價

        public TraceCompany(Company company, String type)
        {
            this.id = company.id;
            this.name = company.name;
            this.date = DateTime.Now;
            this.count = 1;
            this.startScore = 0;
            this.score = 0;
            HistoryData[] dayHistoryData = company.getRealHistoryDataArray("d");
            this.startPrice = dayHistoryData[dayHistoryData.Length - 1].c;
            this.type = type;
        }
    }
    class Trace
    {
        public StockDatabase stockDatabase;
        private List<TraceCompany> traceCompanyList = null;

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
            traceCompanyList.Add(new TraceCompany(company, type));
        }
    }
}
