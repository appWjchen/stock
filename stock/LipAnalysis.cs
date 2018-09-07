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
        public Int32 index;         // 最高或最低價發生時的 index
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
        public List<LipHipData> lipHipDataList;

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
         * 
         */
        public Boolean isDataInLipHipList(LipHipData lipHipData)
        {
            Boolean inList = false;
            for (var i = 0; i < lipHipDataList.Count(); i++)
            {
                LipHipData oneLipHipData = lipHipDataList[i];
                if ((oneLipHipData.index == lipHipData.index) && (oneLipHipData.type == lipHipData.type))
                {
                    inList = true;
                    break;
                }
            }
            return inList;
        }
        /*
         *  函式 findLipHipData 用來找尋 historyDataArray 的最高點及最低點的列表，
         *  傳入 historyDataArray 為歷史資料陣列，尋回一個包含最高點及最低點資料
         *  的列表，型態是 List<LipHipData>
         */
        public List<LipHipData> findLipHipData(HistoryData[] historyDataArray, int indexDiff)
        {
            lipHipDataList = new List<LipHipData>();
            LipHipData prevLipData = null;
            LipHipData prevHipData = null;
            Int32 index = historyDataArray.Length - 1;
            while (index >= 0)
            {
                /*
                 * 把目前的最高價和先前找到的最高價比較，若時間差值在 1.5 年內，
                 * 則認定其為同一波段的最高點，以目前最高價取代先前的最高價。
                 * 最低價也是如此。
                 * 1.5 年以內表示 index(月線歷史資料) 相差 18 (不含)以內為準。
                 * 此分析方法將來可能用在短線(日線歷史資料)上，故以 indexDiff 
                 * 伐表波段時間差之容許值。
                 */
                HistoryData historyData = historyDataArray[index];
                Double currentValue = historyData.c;
                DateTime currentDate = dateStringToDateTime(historyData.t);
                if (prevHipData != null)
                {
                    /* 處理最高價程式 */
                    if (currentValue > prevHipData.value)
                    {
                        /* 找到更高價 */
                        if ((prevHipData.index - index) < indexDiff)
                        {
                            /* 此高點和前次記錄高點為同一波段，更新前次最高價為目前最高價 */
                            prevHipData.date = currentDate;
                            prevHipData.value = currentValue;
                            prevHipData.type = true;
                            prevHipData.index = index;
                        }
                        else
                        {
                            /* 此高點和前次記錄高點為不同波段，將前波高點放到 lipHipDataList 列表中 */
                            LipHipData hipData = new LipHipData();
                            hipData.date = prevHipData.date;
                            hipData.value = prevHipData.value;
                            hipData.type = prevHipData.type;
                            hipData.index = prevHipData.index;
                            lipHipDataList.Add(hipData);
                            /* 更新前波高點為目前高點 */
                            prevHipData.date = currentDate;
                            prevHipData.value = currentValue;
                            prevHipData.type = true;
                            prevHipData.index = index;
                        }
                    }
                }
                else
                {
                    /* 沒有最高價物件，產生之 */
                    prevHipData = new LipHipData();
                    prevHipData.date = currentDate;
                    prevHipData.type = true;
                    prevHipData.value = currentValue;
                    prevHipData.index = index;
                }
                if (prevLipData != null)
                {
                    /* 處理最低價程式 */
                    if (currentValue < prevLipData.value)
                    {
                        /* 找到更低價 */
                        if ((prevLipData.index - index) < indexDiff)
                        {
                            /* 此低點和前次記錄低點為同一波段，更新前次最低價為目前最低價 */
                            prevLipData.date = currentDate;
                            prevLipData.value = currentValue;
                            prevLipData.type = false;
                            prevLipData.index = index;
                        }
                        else
                        {
                            /* 此低點和前次記錄低點為不同波段，將前波低點放到 lipHipDataList 列表中 */
                            LipHipData lipData = new LipHipData();
                            lipData.date = prevLipData.date;
                            lipData.value = prevLipData.value;
                            lipData.type = prevLipData.type;
                            lipData.index = prevLipData.index;
                            lipHipDataList.Add(lipData);
                            /* 更新前波低點為目前低點 */
                            prevLipData.date = currentDate;
                            prevLipData.value = currentValue;
                            prevLipData.type = true;
                            prevLipData.index = index;
                        }
                    }
                }
                else
                {
                    /* 沒有最低價物件，產生之 */
                    prevLipData = new LipHipData();
                    prevLipData.date = currentDate;
                    prevLipData.type = false;
                    prevLipData.value = currentValue;
                    prevLipData.index = index;
                }
                index--;
            }
            /* 檢查前波最高最低點是否在列表中 */
            if (!isDataInLipHipList(prevLipData))
            {
                /* 前波最低點不在列表中，加入列表 */
                LipHipData lipData = new LipHipData();
                lipData.date = prevLipData.date;
                lipData.value = prevLipData.value;
                lipData.type = prevLipData.type;
                lipData.index = prevLipData.index;
                lipHipDataList.Add(lipData);
            }
            if (!isDataInLipHipList(prevHipData))
            {
                /* 前波最高點不在列表中，加入列表 */
                LipHipData hipData = new LipHipData();
                hipData.date = prevHipData.date;
                hipData.value = prevHipData.value;
                hipData.type = prevHipData.type;
                hipData.index = prevHipData.index;
                lipHipDataList.Add(hipData);
            }
            lipHipDataList.Sort(
                delegate(LipHipData x, LipHipData y)
                {
                    if (x.index > y.index)
                    {
                        return 1;
                    }
                    else if (x.index < y.index)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                );
            return lipHipDataList;
        }
    }
}
