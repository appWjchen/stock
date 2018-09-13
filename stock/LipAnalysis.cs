using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stock
{
    /* 
     * 資料結構 LipHipData 是用來記錄歷史資料的極值(最高、最低)的結構， 
     */
    public class LipHipData
    {
        public Boolean type;        // Lip, Hip 資料的型態，true 表示 Hip，false 表示 Lip
        public DateTime date;       // 最高或最低點的日期
        public Double value;        // 最高或最低點的值
        public Int32 index;         // 最高或最低價發生時的 index
    }
    /*
     * 資料結構 WaveData 是用來記錄波段上漲及下跌幅度的
     */
    public class WaveData
    {
        public Boolean type;                // 上漲或下跌，true 表示上漲，false 表示下跌
        public Double diffPercent;          // 上漲或下跌的百分比，百分比是和波段的起始價格比較
        public Int32 diffDays;              // 波段的總日數
        public DateTime startDate;
        public DateTime endDate;
        public Double startPrice;
        public Double endPrice;
    }
    /*
     * 資料結構 WaveStatisticInformation 用來表示波段漲跌幅度統計資訊用。
     */
    public class WaveStatisticInformation
    {
        Double maxUpDiffPercent;
        Double minUpDiffPercent;
        Int32 maxUpDiffDate;
        Int32 minUpDiffDate;
        Double totalUpDiffPercent;
        Double averageUpDiffPercent;
        Int32 totalUpDiffDate;
        Int32 averageUpDiffDate;
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
     * 主要函式列表：
     *      1. findLipHipData ：
     *          用來找尋 historyDataArray 的最高點及最低點的列表
     *      2. 
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
            /*
             * 這裡的 type 和 LipHipData 中的 type 有點不一樣，
             *  type=-1     表示目前找到的是最低點，要開始找最高點
             *  typr=0      表示目前正在找第一點，還沒找到最高或最低點
             *  type=1      表示目前找到的是最高點，要開始找最低點
             */
            int type = 0;
            while (index >= 0)
            {
                /*
                 * 把目前的最高價和先前找到的最高價比較，若時間差值在 3 年內，
                 * 則認定其為同一波段的最高點，以目前最高價取代先前的最高價。
                 * 最低價也是如此。
                 * indexDiff 個月以內表示 index(月線歷史資料) 相差 indexDiff個月以內為準。
                 * 此分析方法將來可能用在短線(日線歷史資料)上，故以 indexDiff 
                 * 伐表波段時間差之容許值。
                 */
                HistoryData historyData = historyDataArray[index];
                Double currentValue = historyData.c;
                Double currentHigh = historyData.h;
                Double currentLow = historyData.l;
                DateTime currentDate = dateStringToDateTime(historyData.t);
                /* (1) 首先要找到第一個最高或最低點 */
                if (type == 0)
                {
                    /* 找第一點 */
                    if (prevHipData != null)
                    {
                        /* 處理最高價程式 */
                        if ((prevHipData.index - index) > indexDiff)
                        {
                            /* 找到第一高點，將其放到 lipHipDataList 列表中 */
                            LipHipData hipData = new LipHipData();
                            hipData.date = prevHipData.date;
                            hipData.value = prevHipData.value;
                            hipData.type = prevHipData.type;
                            hipData.index = prevHipData.index;
                            lipHipDataList.Add(hipData);
                            type = 1;
                            /* 將最低點設為最低高點當天，準備往後繼續尋找 */
                            prevLipData.date = prevHipData.date;
                            prevLipData.value = prevHipData.value;
                            prevLipData.type = false;
                            prevLipData.index = prevHipData.index;
                            index = prevHipData.index;
                            continue;
                        }
                        if (currentHigh > prevHipData.value)
                        {
                            /* 找到更高點 */
                            prevHipData.date = currentDate;
                            prevHipData.value = currentHigh;
                            prevHipData.type = true;
                            prevHipData.index = index;
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
                        if ((prevLipData.index - index) > indexDiff)
                        {
                            /* 找到第一低點，將其放到 lipHipDataList 列表中 */
                            LipHipData lipData = new LipHipData();
                            lipData.date = prevLipData.date;
                            lipData.value = prevLipData.value;
                            lipData.type = prevLipData.type;
                            lipData.index = prevLipData.index;
                            lipHipDataList.Add(lipData);
                            type = -1;
                            /* 將最高點設為最低點當天，準備往後繼續尋找 */
                            prevHipData.date = prevLipData.date;
                            prevHipData.value = prevLipData.value;
                            prevHipData.type = true;
                            prevHipData.index = prevLipData.index;
                            index = prevLipData.index;
                            continue;
                        }
                        if (currentLow < prevLipData.value)
                        {
                            /* 找到更低點 */
                            prevLipData.date = currentDate;
                            prevLipData.value = currentLow;
                            prevLipData.type = false;
                            prevLipData.index = index;
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
                }
                else
                {
                    /* 不是第一點，則根據 type 依序找高低點 */
                    if (type == 1)
                    {
                        /* 前一極點是最高點, 往後找最低點 */
                        if ((prevHipData.index - index) > indexDiff)
                        {
                            /* 找到最低點，將其放到 lipHipDataList 列表中 */
                            LipHipData lipData = new LipHipData();
                            lipData.date = prevLipData.date;
                            lipData.value = prevLipData.value;
                            lipData.type = prevLipData.type;
                            lipData.index = prevLipData.index;
                            lipHipDataList.Add(lipData);
                            type = -1;
                            /* 將最高點設為最低點當天，準備往後繼續尋找 */
                            prevHipData.date = prevLipData.date;
                            prevHipData.value = prevLipData.value;
                            prevHipData.type = true;
                            prevHipData.index = prevLipData.index;
                            index = prevLipData.index;
                            index--;
                            continue;
                        }
                        if (currentLow < prevLipData.value)
                        {
                            /* 找到更低點 */
                            prevLipData.date = currentDate;
                            prevLipData.value = currentLow;
                            prevLipData.type = false;
                            prevLipData.index = index;
                        }
                    }
                    else
                    {
                        /* 前一極點是最低點點, 往後找最高點 */
                        if ((prevLipData.index - index) > indexDiff)
                        {
                            /* 找到第一高點，將其放到 lipHipDataList 列表中 */
                            LipHipData hipData = new LipHipData();
                            hipData.date = prevHipData.date;
                            hipData.value = prevHipData.value;
                            hipData.type = prevHipData.type;
                            hipData.index = prevHipData.index;
                            lipHipDataList.Add(hipData);
                            type = 1;
                            /* 將最低點設為最低高點當天，準備往後繼續尋找 */
                            prevLipData.date = prevHipData.date;
                            prevLipData.value = prevHipData.value;
                            prevLipData.type = false;
                            prevLipData.index = prevHipData.index;
                            index = prevHipData.index;
                            index--;
                            continue;
                        }
                        if (currentHigh > prevHipData.value)
                        {
                            /* 找到更高點 */
                            prevHipData.date = currentDate;
                            prevHipData.value = currentHigh;
                            prevHipData.type = true;
                            prevHipData.index = index;
                        }
                    }
                }
                index--;
            }


            if ((type == 1) && (!isDataInLipHipList(prevLipData)))
            {
                LipHipData lipData = new LipHipData();
                lipData.date = prevLipData.date;
                lipData.value = prevLipData.value;
                lipData.type = prevLipData.type;
                lipData.index = prevLipData.index;
                lipHipDataList.Add(lipData);
            }
            if ((type == -1) && (!isDataInLipHipList(prevHipData)))
            {
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
        /*
         * 函式 findAllLipHipDataList 用來計算大盤及所有
         */
        public void findAllLipHipDataList()
        {
            /* 
            // bug 試用程式片段
            stockDatabase.companies[77].lipHipDataList = findLipHipData(
                stockDatabase.companies[77].getRealHistoryDataArray("m"),
                36
                );
            */
            /* 大盤波段用 48 個月做波段搜尋最大期限 */
            stockDatabase.lipHipDataList = findLipHipData(
                stockDatabase.getMonthHistoryData(),
                48
                );
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                Company company = stockDatabase.companies[i];
                new WarningWriter().showMessage("正在搜尋" + company.name + "(" + company.id + ")的波段極值，index=" + i);
                new AppDoEvents().DoEvents();
                /* 各股波段用 36 個月做波段搜尋最大期限 */
                company.lipHipDataList = findLipHipData(
                    company.getRealHistoryDataArray("m"),
                    36
                    );
            }
            new WarningWriter().showMessage("搜尋所有公司波段極值完畢");
            new AppDoEvents().DoEvents();
        }
        /*
         * 函式 findWaveDataList 用來找尋波段上漲或下跌的幅度。
         */
        public List<WaveData> findWaveDataList(List<LipHipData> lipHipDataList)
        {
            List<WaveData> waveDataList = new List<WaveData>();
            for (var i = 0; i < (lipHipDataList.Count() - 1); i++)
            {
                /* 
                 * lipHipDataList.Count()-1 是因為最後一個波段是到當日為止，
                 * 還不能算是一個完整的波段，不能用來計算波段漲幅。
                 */
                var oneLipHipData = lipHipDataList[i];
                var nextLipHipData = lipHipDataList[i + 1];
                Double valueDiff = Math.Abs(oneLipHipData.value - nextLipHipData.value);
                Double daysDiff = Math.Abs((nextLipHipData.date - oneLipHipData.date).TotalDays);
                Int32 daysOffInteger = Convert.ToInt32(daysDiff);
                WaveData waveData = new WaveData();
                waveData.diffDays = daysOffInteger;
                waveData.diffPercent = valueDiff * 100.0 / oneLipHipData.value;
                waveData.startDate = oneLipHipData.date;
                waveData.endDate = nextLipHipData.date;
                waveData.startPrice = oneLipHipData.value;
                waveData.endPrice = nextLipHipData.value;
                if (oneLipHipData.type)
                {
                    /* 高點 */
                    waveData.type = false;      // 下跌
                }
                else
                {
                    /* 低點 */
                    waveData.type = true;       // 上漲
                }
                waveDataList.Add(waveData);
            }
            return waveDataList;
        }
        /*
         * 函式 findAllWaveDataList 用來找出大盤及所有股票的漲幅資訊。
         */
        public void findAllWaveDataList()
        {
            if (stockDatabase.lipHipDataList == null)
            {
                findAllLipHipDataList();
            }
            stockDatabase.waveDataList = findWaveDataList(stockDatabase.lipHipDataList);
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                Company company = stockDatabase.companies[i];
                new WarningWriter().showMessage("正在搜尋" + company.name + "(" + company.id + ")的波段漲幅，index=" + i);
                new AppDoEvents().DoEvents();
                company.waveDataList = findWaveDataList(company.lipHipDataList);
            }
        }
        /*
         *
         */
        public List<WaveStatisticInformation> findWaveStatisticInformation(List<WaveData> waveDataList)
        {
            List<WaveStatisticInformation> waveStatisticInformationList = new List<WaveStatisticInformation>();

            return waveStatisticInformationList;
        }
        /*
         *
         */
        public void findAllWaveStatisticInformation()
        {
            if (stockDatabase.waveDataList == null)
            {
                findAllWaveDataList();
            }
            stockDatabase.waveStatisticInformationList = findWaveStatisticInformation(stockDatabase.waveDataList);
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                Company company = stockDatabase.companies[i];
                new WarningWriter().showMessage("正在搜尋" + company.name + "(" + company.id + ")的波段漲幅統計資料，index=" + i);
                new AppDoEvents().DoEvents();
                company.waveStatisticInformationList = findWaveStatisticInformation(company.waveDataList);
            }
        }
    }
}
