using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stock
{
    class LipHipData
    {
        public Boolean type;        // Lip, Hip 資料的型態，true 表示 Hip，false 表示 Lip
        public DateTime date;       // 最高或最低點的日期
        public Double value;        // 最高或最低點的值
    }
    /*
     * LipAnalysis 類別用來做絕對低點(Absolute Lowest Index Point)分析用，
     * 此分析法會試著找出股價的每波最高(x 日股價 maxIndex)及最低點(y 日股價minIndex)
     * ，並利用此資訊找出股價最大差值(下跌、上漲幅度)、y-x 的最大值、最小值及平均，
     * x-y 的最大值、最小值及平均。
     * 最後目標是找到股價位於最低檔並開始反轉的時機，分析用此方法找到的結果上漲及
     * 下跌的機率為何？
     * 若此方法可行則把找到的股票加入追踪系統。
     * 預期此方法應該適用長線的股價波動，應該很準，所以主要是以月線資料為分析以據，
     * 完成整個分析方法後，試試看對短線(日線)的分析結果為何。
     */
    class LipAnalysis
    {
        public StockDatabase stockDatabase;

        /* 建構式 */
        public LipAnalysis(StockDatabase stockDatabase)
        {
            this.stockDatabase = stockDatabase;
        }
        /*
         * 函式 dateStringToDateTime 可將傳入的日期字串(格式為 yyyy/MM/dd)轉換
         * 成 DateTime 物件的型式傳回。
         */
        public static DateTime dateStringToDateTime(String date)
        {
            int Year = Convert.ToInt32(date.Substring(0, 4));
            int Month = Convert.ToInt32(date.Substring(5, 2));
            int Day = Convert.ToInt32(date.Substring(8, 2));
            var newDate = new DateTime(Year, Month, Day);
            return newDate;
        }
        /*
         * 函式 dateTimeToDateString 將傳入的日期物件轉換成日期字串傳回。
         */
        public static String dateTimeToDateString(DateTime date)
        {
            return date.ToString("yyyy/MM/dd");
        }
        /*
         *  函式 findLipHipData 用來找尋 historyDataArray 的最高點及最低點的列表，
         *  傳入 historyDataArray 為歷史資料陣列，尋回一個包含最高點及最低點資料
         *  的列表，型態是 List<LipHipData>
         */
        public List<LipHipData> findLipHipData(HistoryData[] historyDataArray)
        {
            List<LipHipData> lipHipDataList = new List<LipHipData>();
            LipHipData prevLipData = null;
            LipHipData prevHipData = null;
            Int32 index = historyDataArray.Length - 1;
            while (index >= 0)
            {
                HistoryData historyData = historyDataArray[index];

            }
            return lipHipDataList;
        }
    }
}
