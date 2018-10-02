using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stock
{
    /* 
     * 資料結構 LipHipData 是用來記錄歷史資料的極值(最高、最低)的結構， 
     */
    class LipHipData
    {
        public Boolean type;        // Lip, Hip 資料的型態，true 表示 Hip，false 表示 Lip
        public DateTime date;       // 最高或最低點的日期
        public Double value;        // 最高或最低點的值
        public Int32 index;         // 最高或最低價發生時的 index
    }
    /*
     * 資料結構 WaveData 是用來記錄波段上漲及下跌幅度的
     */
    class WaveData
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
    class WaveStatisticInformation
    {
        public Double maxUpDiffPercent;
        public Double minUpDiffPercent;
        public Int32 maxUpDiffDate;
        public Int32 minUpDiffDate;
        public Double totalUpDiffPercent;
        public Double averageUpDiffPercent;
        public Int32 totalUpDiffDate;
        public Double averageUpDiffDate;
        public Int32 totalUpCount;
        public Double maxDownDiffPercent;
        public Double minDownDiffPercent;
        public Int32 maxDownDiffDate;
        public Int32 minDownDiffDate;
        public Double totalDownDiffPercent;
        public Double averageDownDiffPercent;
        public Int32 totalDownDiffDate;
        public Double averageDownDiffDate;
        public Int32 totalDownCount;
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
                if ((oneLipHipData.index == lipHipData.index) /*&& (oneLipHipData.type == lipHipData.type)*/)
                {
                    inList = true;
                    break;
                }
            }
            return inList;
        }
        /*
         * 函式 findLipHipDataStage1 是函式 findLipHipData 的第一階段，此階段將
         * historyDataArray 計算出 N 天平均值。
         * 傳回值型態也是 HistoryData 的陣列，元素的 c 成員是N天平均值。
         */
        public HistoryData[] findLipHipDataStage1(HistoryData[] historyDataArray, int N)
        {
            /* 拷貝一份 historayDataArray */
            HistoryData[] historyDataArrayTemp = new HistoryData[historyDataArray.Length];
            for (var i = 0; i < historyDataArray.Length; i++)
            {
                historyDataArrayTemp[i] = new HistoryData();
                historyDataArrayTemp[i].c = historyDataArray[i].c;
                historyDataArrayTemp[i].d = historyDataArray[i].d;
                historyDataArrayTemp[i].f = historyDataArray[i].f;
                historyDataArrayTemp[i].h = historyDataArray[i].h;
                historyDataArrayTemp[i].i = historyDataArray[i].i;
                historyDataArrayTemp[i].l = historyDataArray[i].l;
                historyDataArrayTemp[i].m = historyDataArray[i].m;
                historyDataArrayTemp[i].n = historyDataArray[i].n;
                historyDataArrayTemp[i].o = historyDataArray[i].o;
                historyDataArrayTemp[i].p = historyDataArray[i].p;
                historyDataArrayTemp[i].r = historyDataArray[i].r;
                historyDataArrayTemp[i].s = historyDataArray[i].s;
                historyDataArrayTemp[i].t = historyDataArray[i].t;
                historyDataArrayTemp[i].u = historyDataArray[i].u;
                historyDataArrayTemp[i].v = historyDataArray[i].v;
            }
            List<HistoryData> historyDataList = new List<HistoryData>();
            for (var i = 0; i < historyDataArrayTemp.Length; i++)
            {
                if (i < (N - 1))
                {
                    /* 前 N 日無法算 N 日平均，只能算 i 天平均(i<N) */
                    Double sum = 0;
                    for (var k = 0; k <= i; k++)
                    {
                        sum = sum + historyDataArray[k].c;
                    }
                    Double average = sum / (i + 1);
                    historyDataArrayTemp[i].c = average;
                    historyDataList.Add(historyDataArrayTemp[i]);
                }
                else
                {
                    Double sum = 0;
                    for (var k = (i - N + 1); k <= i; k++)
                    {
                        sum = sum + historyDataArray[k].c;
                    }
                    Double average = sum / N;
                    historyDataArrayTemp[i].c = average;
                    historyDataList.Add(historyDataArrayTemp[i]);
                }
            }
            return historyDataList.ToArray();
        }
        /*
         * 函式 findLipHipDataStage2 由傳入的 N 天平均值資料中，找出轉折點，
         * 所謂轉折點是指斜率由正轉負，或由負轉正的點。
         * 傳回這些點所形成的波段資料，List<LipHipData>。
         */
        public List<LipHipData> findLipHipDataStage2(HistoryData[] averageHistoryDataArray, int N)
        {
            List<LipHipData> lipHipDataList = new List<LipHipData>();
            /* 
             * 計算所有資料點的斜率，斜率為目前資料點和前一資料的差值，
             * 差值為正，表上升，斜率為正。
             * 差值為負，表下降，斜率為負。
             */
            Double[] slopeArray = new Double[averageHistoryDataArray.Length];
            slopeArray[0] = 0;
            for (var i = 1; i < averageHistoryDataArray.Length; i++)
            {
                Double slope = averageHistoryDataArray[i].c -
                    averageHistoryDataArray[i - 1].c;
                slopeArray[i] = slope;
            }
            /*
             * 由前住後比較斜率值，若有正負號變化，則將 index 放到
             * indexList 中。
             */
            Boolean prevSlopeIsPositive = false;
            if (slopeArray[1] > 0)
            {
                prevSlopeIsPositive = true;
            }
            for (var i = 2; i < slopeArray.Length; i++)
            {
                Boolean slopeIsPositive = false;
                if (slopeArray[i] > 0)
                {
                    slopeIsPositive = true;
                }
                if (slopeIsPositive == prevSlopeIsPositive)
                {
                    continue;
                }
                else
                {
                    /* 斜率符號轉換了 */
                    // indexList.Add(i);
                    LipHipData lipHipData = new LipHipData();
                    lipHipData.type = prevSlopeIsPositive;
                    lipHipData.index = i;
                    lipHipDataList.Add(lipHipData);
                    prevSlopeIsPositive = slopeIsPositive;
                }
            }
            LipHipData[] lipHipDataArray = lipHipDataList.ToArray();
            lipHipDataList = new List<LipHipData>();
            for (var i = 0; i < (lipHipDataArray.Length - 1); i++)
            {
                if ((lipHipDataArray[i + 1].index - lipHipDataArray[i].index) <= N)
                {
                    /* 二個 index 值差 N 以內,表示斜率變化太接近，要跳過不正確的 index。
                     */
                    i++;
                }
                else
                {
                    lipHipDataList.Add(lipHipDataArray[i]);
                }
            }
            /* 最後一點和前一點斜率差 N 以上，要再加入到 indexList 中(要先檢查 lipHipDataArray 中至少二點才可以) */
            if ((lipHipDataArray.Length >= 2) &&
                ((lipHipDataArray[lipHipDataArray.Length - 1].index - lipHipDataArray[lipHipDataArray.Length - 2].index) > N))
            {
                lipHipDataList.Add(lipHipDataArray[lipHipDataArray.Length - 1]);
            }
            return lipHipDataList;
        }
        /*
         * 函式 findLipHipDataStage3 用來找尋波段的極值點。
         * 原理是利用 N 日平均值的轉折點，假設極值是出現在轉折點往前
         * N 日內。
         * 因為假設可能不正確(沒有經過證明)，因此往前找尋 2*N 日內的
         * 極值，當做是波段的橿值。
         * 傳回找到的極值位置之波段資料 ，型態是  List<LipHipData>。
         */
        public List<LipHipData> findLipHipDataStage3(HistoryData[] historyDataArray, List<LipHipData> lipHipDataListChangePoint, int N)
        {
            List<LipHipData> lipHipDataList = new List<LipHipData>();
            for (var i = 0; i < lipHipDataListChangePoint.Count(); i++)
            {
                LipHipData lipHipDataChangePoint = lipHipDataListChangePoint[i];
                if (lipHipDataChangePoint.type)
                {
                    // 極大值搜尋
                    int indexMax = lipHipDataChangePoint.index;
                    Double valueMax = historyDataArray[indexMax].h;
                    for (var k = lipHipDataChangePoint.index; k > (lipHipDataChangePoint.index - 2 * N); k--)
                    {
                        if ((k >= 0) && (historyDataArray[k].h > valueMax))
                        {
                            valueMax = historyDataArray[k].h;
                            indexMax = k;
                        }
                    }
                    LipHipData lipHipData = new LipHipData();
                    lipHipData.type = lipHipDataChangePoint.type;
                    lipHipData.value = valueMax;
                    lipHipData.date = dateStringToDateTime(historyDataArray[indexMax].t);
                    lipHipData.index = indexMax;
                    lipHipDataList.Add(lipHipData);
                }
                else
                {
                    // 極小值搜尋
                    int indexMin = lipHipDataChangePoint.index;
                    Double valueMin = historyDataArray[indexMin].l;
                    for (var k = lipHipDataChangePoint.index; k > (lipHipDataChangePoint.index - 2 * N); k--)
                    {
                        if ((k >= 0) && (historyDataArray[k].l < valueMin))
                        {
                            valueMin = historyDataArray[k].l;
                            indexMin = k;
                        }
                    }
                    LipHipData lipHipData = new LipHipData();
                    lipHipData.type = lipHipDataChangePoint.type;
                    lipHipData.value = valueMin;
                    lipHipData.date = dateStringToDateTime(historyDataArray[indexMin].t);
                    lipHipData.index = indexMin;
                    lipHipDataList.Add(lipHipData);
                }
            }
            return lipHipDataList;
        }
        /*
         *  函式 findLipHipData 用來找尋 historyDataArray 的最高點及最低點的列表，
         *  傳入 historyDataArray 為歷史資料陣列，尋回一個包含最高點及最低點資料
         *  的列表，型態是 List<LipHipData>
         *  此函式是第2次嘗試的方法，主要是先找月線的5日平均線之轉折點，找到後往前
         *  5筆資料找出 HIP 或 LIP ，以便得到波段資料。
         */
        public List<LipHipData> findLipHipData(HistoryData[] historyDataArray, int indexDiff)
        {
            String msg = "";
            /* 計算 5 天平均值 */
            int N = 5;
            HistoryData[] averageHistoryDataArray = findLipHipDataStage1(historyDataArray, N);
            
            for (var i = 0; i < averageHistoryDataArray.Length; i++)
            {
                msg = msg + i +
                    "\t" + averageHistoryDataArray[i].c.ToString("f2") +
                    "\t" + averageHistoryDataArray[i].t +
                    "\r\n";
            }
            msg = msg + "\r\n";
            
            /* 找尋轉折點 */
            List<LipHipData> lipHipDataListChangePoint = findLipHipDataStage2(averageHistoryDataArray, N);
            
            for (var i = 0; i < lipHipDataListChangePoint.Count(); i++)
            {
                LipHipData lipHipData = lipHipDataListChangePoint[i];
                msg = msg + i +
                    "\t" + lipHipData.index +
                    "\t" + lipHipData.type +
                    "\r\n";
            }
            msg = msg + "\r\n";
            
            /* 利用轉折點找尋極值 */
            List<LipHipData> lipHipDataList = findLipHipDataStage3(historyDataArray, lipHipDataListChangePoint, N);
            for (var i = 0; i < lipHipDataList.Count(); i++)
            {
                LipHipData lipHipData = lipHipDataList[i];
                msg = msg + i +
                    "\t" + lipHipData.date +
                    "\t" + lipHipData.index +
                    "\t" + lipHipData.type +
                    "\t" + lipHipData.value +
                    "\r\n";
            }
            msg = msg + "\r\n";
            /*
            String msg = "";
            for (var i = 0; i < lipHipDataList.Count(); i++)
            {
                msg = msg + lipHipDataList[i].index + 
                    "\t" + lipHipDataList[i].type +
                    "\t" + lipHipDataList[i].value +
                    "\t" + lipHipDataList[i].date.ToString("yyy/MM/dd") +
                    "\r\n";
            }
            new MessageWriter().showMessage(msg);
            */
            new MessageWriter().showMessage(msg);
            return lipHipDataList;
        }
        /*
         *  函式 findLipHipData_0 用來找尋 historyDataArray 的最高點及最低點的列表，
         *  傳入 historyDataArray 為歷史資料陣列，尋回一個包含最高點及最低點資料
         *  的列表，型態是 List<LipHipData>
         *  此函式是第一次嘗試找尋極值的方法，經過實際資料的分析後，發現有些特別情形
         *  會找到不太正確的波段資料，因此放棄使用，重新再試另一種找尋波段的方法。
         */
        public List<LipHipData> findLipHipData_0(HistoryData[] historyDataArray, int indexDiff)
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
            else if ((type == -1) && (!isDataInLipHipList(prevHipData)))
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
         * 函式 findAllLipHipDataList 用來計算大盤及所有股票波段資訊。
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
        public WaveStatisticInformation findWaveStatisticInformation(List<WaveData> waveDataList)
        {
            WaveStatisticInformation waveStatisticInformation = new WaveStatisticInformation();
            /* 初始化波段統計資料 */
            waveStatisticInformation.maxUpDiffDate = 0;
            waveStatisticInformation.minUpDiffDate = Int32.MaxValue;
            waveStatisticInformation.maxUpDiffPercent = 0;
            waveStatisticInformation.minUpDiffPercent = Double.MaxValue;
            waveStatisticInformation.totalUpDiffDate = 0;
            waveStatisticInformation.totalUpDiffPercent = 0;
            waveStatisticInformation.totalUpCount = 0;
            waveStatisticInformation.maxDownDiffDate = 0;
            waveStatisticInformation.minDownDiffDate = Int32.MaxValue;
            waveStatisticInformation.maxDownDiffPercent = 0;
            waveStatisticInformation.minDownDiffPercent = Double.MaxValue;
            waveStatisticInformation.totalDownDiffDate = 0;
            waveStatisticInformation.totalDownDiffPercent = 0;
            waveStatisticInformation.totalDownCount = 0;

            for (var i = 0; i < waveDataList.Count(); i++)
            {
                var oneWaveData = waveDataList[i];
                if (oneWaveData.type)
                {
                    /* 上漲 */
                    waveStatisticInformation.totalUpCount++;
                    if (oneWaveData.diffDays > waveStatisticInformation.maxUpDiffDate)
                    {
                        waveStatisticInformation.maxUpDiffDate = oneWaveData.diffDays;
                    }
                    if (oneWaveData.diffDays < waveStatisticInformation.minUpDiffDate)
                    {
                        waveStatisticInformation.minUpDiffDate = oneWaveData.diffDays;
                    }
                    if (oneWaveData.diffPercent > waveStatisticInformation.maxUpDiffPercent)
                    {
                        waveStatisticInformation.maxUpDiffPercent = oneWaveData.diffPercent;
                    }
                    if (oneWaveData.diffPercent < waveStatisticInformation.minUpDiffPercent)
                    {
                        waveStatisticInformation.minUpDiffPercent = oneWaveData.diffPercent;
                    }
                    waveStatisticInformation.totalUpDiffDate = waveStatisticInformation.totalUpDiffDate +
                        oneWaveData.diffDays;
                    waveStatisticInformation.totalUpDiffPercent = waveStatisticInformation.totalUpDiffPercent +
                        oneWaveData.diffPercent;
                }
                else
                {
                    /* 下跌 */
                    waveStatisticInformation.totalDownCount++;
                    if (oneWaveData.diffDays > waveStatisticInformation.maxDownDiffDate)
                    {
                        waveStatisticInformation.maxDownDiffDate = oneWaveData.diffDays;
                    }
                    if (oneWaveData.diffDays < waveStatisticInformation.minDownDiffDate)
                    {
                        waveStatisticInformation.minDownDiffDate = oneWaveData.diffDays;
                    }
                    if (oneWaveData.diffPercent > waveStatisticInformation.maxDownDiffPercent)
                    {
                        waveStatisticInformation.maxDownDiffPercent = oneWaveData.diffPercent;
                    }
                    if (oneWaveData.diffPercent < waveStatisticInformation.minDownDiffPercent)
                    {
                        waveStatisticInformation.minDownDiffPercent = oneWaveData.diffPercent;
                    }
                    waveStatisticInformation.totalDownDiffDate = waveStatisticInformation.totalDownDiffDate +
                        oneWaveData.diffDays;
                    waveStatisticInformation.totalDownDiffPercent = waveStatisticInformation.totalDownDiffPercent +
                        oneWaveData.diffPercent;
                }
            }
            if (waveStatisticInformation.totalUpCount != 0)
            {
                waveStatisticInformation.averageUpDiffDate =
                    waveStatisticInformation.totalUpDiffDate / waveStatisticInformation.totalUpCount;
                waveStatisticInformation.averageUpDiffPercent =
                    waveStatisticInformation.totalUpDiffPercent / waveStatisticInformation.totalUpCount;
            }
            if (waveStatisticInformation.totalDownCount != 0)
            {
                waveStatisticInformation.averageDownDiffDate =
                    waveStatisticInformation.totalDownDiffDate / waveStatisticInformation.totalDownCount;
                waveStatisticInformation.averageDownDiffPercent =
                    waveStatisticInformation.totalDownDiffPercent / waveStatisticInformation.totalDownCount;
            }
            return waveStatisticInformation;
        }
        /*
         * 函式 findAllWaveStatisticInformation 用來計算大盤及各股票的漲跌幅統計資料。
         */
        public void findAllWaveStatisticInformation()
        {
            if (stockDatabase.waveDataList == null)
            {
                findAllWaveDataList();
            }
            /* 計算大盤的波段漲跌幅的統計資訊 */
            stockDatabase.waveStatisticInformation = findWaveStatisticInformation(stockDatabase.waveDataList);
            /* 計算各個股的波段漲跌幅的統計資訊 */
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                Company company = stockDatabase.companies[i];
                new WarningWriter().showMessage("正在搜尋" + company.name + "(" + company.id + ")的波段漲幅統計資料，index=" + i);
                new AppDoEvents().DoEvents();
                company.waveStatisticInformation = findWaveStatisticInformation(company.waveDataList);
            }
            /* 將個股的波段漲跌幅的統計資訊再加以統計 */
            stockDatabase.waveStatisticInformationAllCompany = new WaveStatisticInformation();
            stockDatabase.waveStatisticInformationAllCompany.maxUpDiffDate = 0;
            stockDatabase.waveStatisticInformationAllCompany.minUpDiffDate = Int32.MaxValue;
            stockDatabase.waveStatisticInformationAllCompany.maxUpDiffPercent = 0;
            stockDatabase.waveStatisticInformationAllCompany.minUpDiffPercent = Double.MaxValue;
            stockDatabase.waveStatisticInformationAllCompany.totalUpDiffDate = 0;
            stockDatabase.waveStatisticInformationAllCompany.totalUpDiffPercent = 0;
            stockDatabase.waveStatisticInformationAllCompany.totalUpCount = 0;
            stockDatabase.waveStatisticInformationAllCompany.maxDownDiffDate = 0;
            stockDatabase.waveStatisticInformationAllCompany.minDownDiffDate = Int32.MaxValue;
            stockDatabase.waveStatisticInformationAllCompany.maxDownDiffPercent = 0;
            stockDatabase.waveStatisticInformationAllCompany.minDownDiffPercent = Double.MaxValue;
            stockDatabase.waveStatisticInformationAllCompany.totalDownDiffDate = 0;
            stockDatabase.waveStatisticInformationAllCompany.totalDownDiffPercent = 0;
            stockDatabase.waveStatisticInformationAllCompany.totalDownCount = 0;
            /* ↓ 除錯用資訊 */
            Company maxUpDiffDate = null;
            Company minUpDiffDate = null;
            Company maxDownDiffDate = null;
            Company minDownDiffDate = null;
            Company maxUpDiffPercent = null;
            Company minUpDiffPercent = null;
            Company maxDownDiffPercent = null;
            Company minDownDiffPercent = null;
            /* ↑ 除錯用資訊 */
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                Company company = stockDatabase.companies[i];
                if (company.waveStatisticInformation.totalUpCount > 0)
                {
                    if (company.waveStatisticInformation.maxUpDiffDate > stockDatabase.waveStatisticInformationAllCompany.maxUpDiffDate)
                    {
                        stockDatabase.waveStatisticInformationAllCompany.maxUpDiffDate = company.waveStatisticInformation.maxUpDiffDate;
                        maxUpDiffDate = company;
                    }
                    if (company.waveStatisticInformation.minUpDiffDate < stockDatabase.waveStatisticInformationAllCompany.minUpDiffDate)
                    {
                        stockDatabase.waveStatisticInformationAllCompany.minUpDiffDate = company.waveStatisticInformation.minUpDiffDate;
                        minUpDiffDate = company;
                    }
                    if (company.waveStatisticInformation.maxUpDiffPercent > stockDatabase.waveStatisticInformationAllCompany.maxUpDiffPercent)
                    {
                        stockDatabase.waveStatisticInformationAllCompany.maxUpDiffPercent = company.waveStatisticInformation.maxUpDiffPercent;
                        maxUpDiffPercent = company;
                    }
                    if (company.waveStatisticInformation.minUpDiffPercent < stockDatabase.waveStatisticInformationAllCompany.minUpDiffPercent)
                    {
                        stockDatabase.waveStatisticInformationAllCompany.minUpDiffPercent = company.waveStatisticInformation.minUpDiffPercent;
                        minUpDiffPercent = company;
                    }
                    stockDatabase.waveStatisticInformationAllCompany.totalUpDiffDate =
                        stockDatabase.waveStatisticInformationAllCompany.totalUpDiffDate +
                        company.waveStatisticInformation.totalUpDiffDate;
                    stockDatabase.waveStatisticInformationAllCompany.totalUpDiffPercent =
                        stockDatabase.waveStatisticInformationAllCompany.totalUpDiffPercent +
                        company.waveStatisticInformation.totalUpDiffPercent;
                    stockDatabase.waveStatisticInformationAllCompany.totalUpCount =
                        stockDatabase.waveStatisticInformationAllCompany.totalUpCount +
                        company.waveStatisticInformation.totalUpCount;

                }
                if (company.waveStatisticInformation.totalDownCount > 0)
                {
                    if (company.waveStatisticInformation.maxDownDiffDate > stockDatabase.waveStatisticInformationAllCompany.maxDownDiffDate)
                    {
                        stockDatabase.waveStatisticInformationAllCompany.maxDownDiffDate = company.waveStatisticInformation.maxDownDiffDate;
                        maxDownDiffDate = company;
                    }
                    if (company.waveStatisticInformation.minDownDiffDate < stockDatabase.waveStatisticInformationAllCompany.minDownDiffDate)
                    {
                        stockDatabase.waveStatisticInformationAllCompany.minDownDiffDate = company.waveStatisticInformation.minDownDiffDate;
                        minDownDiffDate = company;
                    }
                    if (company.waveStatisticInformation.maxDownDiffPercent > stockDatabase.waveStatisticInformationAllCompany.maxDownDiffPercent)
                    {
                        stockDatabase.waveStatisticInformationAllCompany.maxDownDiffPercent = company.waveStatisticInformation.maxDownDiffPercent;
                        maxDownDiffPercent = company;
                    }
                    if (company.waveStatisticInformation.minDownDiffPercent < stockDatabase.waveStatisticInformationAllCompany.minDownDiffPercent)
                    {
                        stockDatabase.waveStatisticInformationAllCompany.minDownDiffPercent = company.waveStatisticInformation.minDownDiffPercent;
                        minDownDiffPercent = company;
                    }
                    stockDatabase.waveStatisticInformationAllCompany.totalDownDiffDate =
                        stockDatabase.waveStatisticInformationAllCompany.totalDownDiffDate +
                        company.waveStatisticInformation.totalDownDiffDate;
                    stockDatabase.waveStatisticInformationAllCompany.totalDownDiffPercent =
                        stockDatabase.waveStatisticInformationAllCompany.totalDownDiffPercent +
                        company.waveStatisticInformation.totalDownDiffPercent;
                    stockDatabase.waveStatisticInformationAllCompany.totalDownCount =
                        stockDatabase.waveStatisticInformationAllCompany.totalDownCount +
                        company.waveStatisticInformation.totalDownCount;
                }
            }
            stockDatabase.waveStatisticInformationAllCompany.averageUpDiffDate =
                stockDatabase.waveStatisticInformationAllCompany.totalUpDiffDate /
                stockDatabase.waveStatisticInformationAllCompany.totalUpCount;
            stockDatabase.waveStatisticInformationAllCompany.averageUpDiffPercent =
                stockDatabase.waveStatisticInformationAllCompany.totalUpDiffPercent /
                stockDatabase.waveStatisticInformationAllCompany.totalUpCount;
            stockDatabase.waveStatisticInformationAllCompany.averageDownDiffDate =
                stockDatabase.waveStatisticInformationAllCompany.totalDownDiffDate /
                stockDatabase.waveStatisticInformationAllCompany.totalDownCount;
            stockDatabase.waveStatisticInformationAllCompany.averageDownDiffPercent =
                stockDatabase.waveStatisticInformationAllCompany.totalDownDiffPercent /
                stockDatabase.waveStatisticInformationAllCompany.totalDownCount;
            /* ↓ 除錯用資訊 */
            String msg = "";
            msg = msg + "最長上漲時間公司是：" + maxUpDiffDate.name + "(" + maxUpDiffDate.id + ")\r\n";
            msg = msg + "最短上漲時間公司是：" + minUpDiffDate.name + "(" + minUpDiffDate.id + ")\r\n";
            msg = msg + "最大上漲幅度公司是：" + maxUpDiffPercent.name + "(" + maxUpDiffPercent.id + ")\r\n";
            msg = msg + "最小上漲幅度公司是：" + minUpDiffPercent.name + "(" + minUpDiffPercent.id + ")\r\n";
            msg = msg + "最長下跌時間公司是：" + maxDownDiffDate.name + "(" + maxDownDiffDate.id + ")\r\n";
            msg = msg + "最短下跌時間公司是：" + minDownDiffDate.name + "(" + minDownDiffDate.id + ")\r\n";
            msg = msg + "最大下跌幅度公司是：" + maxDownDiffPercent.name + "(" + maxDownDiffPercent.id + ")\r\n";
            msg = msg + "最小下跌幅度公司是：" + minDownDiffPercent.name + "(" + minDownDiffPercent.id + ")\r\n";
            new WarningWriter().showMessage(msg);
            /* ↑ 除錯用資訊 */
        }
    }
}
