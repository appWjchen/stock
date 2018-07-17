using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stock
{
    class MACD
    {
        public Double ema12;
        public Double ema26;
        public Double diff9;
        public Double macd;
    }
    class KDJ
    {
        public Double K;
        public Double D;
        public Double J;
    }
    class StockIndicator
    {
        public String time;
        public Double CEMA7;
        public Double CEMA15;
        public Double MACD;
        public Double MACD9MA;
        public Double yahooDiff9;
        public Double yahooMACD;
        public Double RSV9;
        public Double K;
        public Double D;
        public Double J;
        public Double RSI5;
        public Double RSI10;
    }
    class Indicator
    {
        public HistoryData[] historyDataArray;
        /*
         * Indicator 建構式
         */
        public Indicator(HistoryData[] historyDataArrayParam)
        {
            historyDataArray = historyDataArrayParam;
        }
        /* 
         * 方法 calcCemaArray 用來計算指數移動平均線(Calculation, Exponential Moving Average) 
         *      CEMA[i] = CEMA[i-1] * (1-percent) + historyData[i].c * percent
         * 傳入參數：
         *      startIndex : 上式中，開始計算的 i 值
         *      startCEMA : 上式中，CEMA[i-1] 的值
         *      percent : 上式中，percent 的值
         */
        public Double[] calcCemaArray(int startIndex, Double startCEMA, Double percent)
        {
            if ((startIndex < 0) || (startIndex >= historyDataArray.Length))
            {
                startIndex = 0;
            }
            List<Double> CEMAList = new List<Double>();
            Double CEMA = startCEMA;
            /* 將 startIndex 之前的 CEMA 值設為 0 */
            for (int i = 0; i < (startIndex - 1); i++)
            {   // CEMA[0] ~ CEMA[startIndex - 2] <= 0
                CEMAList.Add(0);
            }
            if (startIndex > 0)
            {   // 如果 startIndex 是 0，則 CEMA 是 0
                CEMAList.Add(CEMA);     // CEMA[startIndex - 1]
            }
            else
            {
                CEMA = 0;
            }
            for (int i = startIndex; i < historyDataArray.Length; i++)
            {
                CEMA = CEMA * (1 - percent) + historyDataArray[i].c * percent;
                CEMAList.Add(CEMA);
            }
            return CEMAList.ToArray();
        }
        /* 
         * 函式 calcMacdArray 用來計算平滑異同移動平均線(Moving Average Convergence/Divergence) 
         */
        public Double[] calcMacdArray(Double[] cema15Array, Double[] cema7dot5Array)
        {
            List<Double> MACDList = new List<Double>();
            for (int i = 0; i < cema15Array.Length; i++)
            {
                Double MACD = cema15Array[i] - cema7dot5Array[i];
                MACDList.Add(MACD);
            }
            return MACDList.ToArray();
        }
        /* 
         * 函式 calcMacd9MAArray 用來計算 MACD 9 天移動平均 
         */
        public Double[] calcMacd9MAArray(Double[] macdArray, int maCount)
        {
            int count = 0;
            List<Double> MACD9List = new List<Double>();
            for (var i = 0; i < macdArray.Length; i++)
            {
                MACD9List.Add(0);
            }
            Double[] MACD9Array = MACD9List.ToArray();
            for (int i = MACD9Array.Length - 2; i >= (maCount - 1); i--)
            {
                Double MA = 0;
                while (count < maCount)
                {
                    if ((i - count) >= 0)
                    {
                        if (macdArray[i - count] != 0)
                        {
                            MA = MA + MACD9Array[i - count];
                            count++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (count == maCount)
                {
                    MACD9Array[i] = MA / maCount;
                }
                else
                {
                    MACD9Array[i] = 0;
                }
                count = 0;
            }
            return MACD9Array;
        }
        /* 
            函式 calcYahooMACD 用來計算 Yahoo 平滑異同移動平均線(Moving Average Convergence/Divergence) 
                Yahoo 網站的 MACD 的算法如下：
                    價格 p(i)=[H(i)+L(i)+2*C(i)]/4
                    n 天 EMA E(i) = (n-1)/(1+n)*E(i-1) + 2/(1+n)*(P(i)-E(i-1))
                        首日的 ema 值為前 n 天的 p 值平均
                    Diff(i) = 26 天 EMA - 12 天 EMA
                    9 天 MACD(i) = MACD(i-1) + 2/(1+n)*(Diff(i)-MACD(i-1)); 
        */
        public MACD[] calcYahooMACD()
        {
            /* 計算價格陣列 p(i) */
            List<Double> pList = new List<Double>();
            for (int i = 0; i < historyDataArray.Length; i++)
            {
                Double pValue = (historyDataArray[i].h + historyDataArray[i].l + 2.0 * historyDataArray[i].c) / 4.0;
                pList.Add(pValue);
            }
            /* 計算 EMA12 及 EMA26 陣列 */
            Double ema12Value;
            Double[] p = pList.ToArray();
            List<Double> ema12List = new List<Double>();
            List<Double> ema26List = new List<Double>();
            for (int i = 0; i < historyDataArray.Length; i++)
            {
                if (i < 12)
                {
                    ema12List.Add(0);
                }
                else if (i == 12)
                {
                    ema12Value = 0;
                    for (int k = 0; k < 12; k++)
                    {
                        ema12Value = ema12Value + p[k];
                    }
                    ema12Value = ema12Value / 12;
                    ema12List.Add(ema12Value);
                }
                else
                {
                    ema12Value = 10.0 / 12.0 * ema12List[i - 1] + 2.0 / 12.0 * p[i];
                    ema12List.Add(ema12Value);
                }
                Double ema26Value;
                if (i < 26)
                {
                    ema26List.Add(0);
                }
                else if (i == 26)
                {
                    ema26Value = 0;
                    for (int k = 0; k < 26; k++)
                    {
                        ema26Value = ema26Value + p[k];
                    }
                    ema26Value = ema26Value / 26;
                    ema26List.Add(ema26Value);
                }
                else
                {
                    ema26Value = 24.0 / 26.0 * ema26List[i - 1] + 2.0 / 26.0 * p[i];
                    ema26List.Add(ema26Value);
                }
            }
            Double[] ema12 = ema12List.ToArray();
            Double[] ema26 = ema26List.ToArray();
            /* 計算 diff9 陣列 */
            List<Double> diff9List = new List<Double>();
            for (int i = 0; i < historyDataArray.Length; i++)
            {
                Double diff9Value;
                if (i < 26)
                {
                    diff9Value = 0;
                }
                else
                {
                    diff9Value = ema12[i] - ema26[i];
                }
                diff9List.Add(diff9Value);
            }
            Double[] diff9 = diff9List.ToArray();
            /* 計算 macd 陣列 */
            List<Double> macdList = new List<Double>();
            macdList.Add(0);
            for (var i = 1; i < historyDataArray.Length; i++)
            {
                Double macdValue;
                if (i < 26)
                {
                    macdValue = 0;
                }
                else
                {
                    macdValue = macdList[i - 1] + 2.0 / 10.0 * (diff9[i] - macdList[i - 1]);
                }
                macdList.Add(macdValue);
            }
            Double[] macd = macdList.ToArray();
            List<MACD> diffmacdList = new List<MACD>();
            for (int i = 0; i < historyDataArray.Length; i++)
            {
                MACD diff9macd = new MACD();
                diff9macd.ema12 = ema12[i];
                diff9macd.ema26 = ema26[i];
                diff9macd.diff9 = diff9[i];
                diff9macd.macd = macd[i];
                diffmacdList.Add(diff9macd);
            }
            MACD[] macdArray = diffmacdList.ToArray();
            return macdArray;
        }
        /*
         * 函式 calcRsvArray 用來記算N 期 KD 隨機震盪指標(KD Stochastic Oscillator)的 RSV 值 
         */
        public Double[] calcRsvArray(int N)
        {
            List<Double> rsvList = new List<Double>();
            for (int i = 0; i < historyDataArray.Length; i++)
            {
                if (i < N)
                {
                    rsvList.Add(0);
                }
                else
                {
                    var min = Double.MaxValue;
                    var max = Double.MinValue;
                    for (var k = 0; k < N; k++)
                    {
                        if (historyDataArray[i - k].h > max)
                        {
                            max = historyDataArray[i - k].h;
                        }
                        if (historyDataArray[i - k].l < min)
                        {
                            min = historyDataArray[i - k].l;
                        }
                        if (historyDataArray[i - k].c > max)
                        {
                            max = historyDataArray[i - k].c;
                        }
                        if (historyDataArray[i - k].c < min)
                        {
                            min = historyDataArray[i - k].c;
                        }
                    }
                    /* 計算 KD 隨機震盪指標的 RSV 值並堆入陣列中 */
                    var rsvValue = (historyDataArray[i].c - min) / (max - min) * 100;
                    rsvList.Add(rsvValue);
                }
            }
            return rsvList.ToArray();
        }
        /*
         * 函式 calcKDJArray 用來計算 KD 隨機震盪指標(KD Stochastic Oscillator)的 KD,J 值
         */
        public KDJ[] calcKDJArray(Double[] rsvArray)
        {
            List<KDJ> kdjList = new List<KDJ>();
            Double K = 0;
            Double D = 0;
            Double J = 0;
            for (var i = 0; i < rsvArray.Length; i++)
            {
                K = 2.0 / 3.0 * K + 1.0 / 3.0 * rsvArray[i];
                D = 2.0 / 3.0 * D + 1.0 / 3.0 * K;
                J = 3.0 * K - 2.0 * D;
                KDJ kdj = new KDJ();
                kdj.K = K;
                kdj.D = D;
                kdj.J = J;
                kdjList.Add(kdj);
            }
            return kdjList.ToArray();
        }
        /* 函式 calcRsiArray 用來計算 N 期相對強弱指標(Relative Strength Index) 
            RSI：相對強弱指標 
            A 值：過去 N 日內上漲點數(價格)總和/ N 
            B 值：過去 N 日內下跌點數(價格)總和/ N 
            RS： A/B 
            C 值：100/(RS+1) 
            N 日 RSI= 100-C 
        */
        public Double[] calcRsiArray(int Nparam)
        {
            List<Double> rsiList = new List<Double>();
            Double N = Nparam;
            Double prevUpSome = 0.0;
            Double prevDownSome = 0.0;
            for (var i = 0; i < historyDataArray.Length; i++)
            {
                if (i < N)
                {
                    rsiList.Add(0);
                }
                else if (i == N)
                {
                    Double upSum = 0.0;
                    Double downSum = 0.0;
                    for (int k = 0; k < N; k++)
                    {
                        if (historyDataArray[i - k].c > historyDataArray[i - k - 1].c)
                        {
                            /* 上漲 */
                            upSum = upSum + (historyDataArray[i - k].c - historyDataArray[i - k - 1].c);
                        }
                        else if (historyDataArray[i - k].c < historyDataArray[i - k - 1].c)
                        {
                            /* 下跌 */
                            downSum = downSum + (historyDataArray[i - k - 1].c - historyDataArray[i - k].c);
                        }
                    }
                    prevUpSome = upSum / N;
                    prevDownSome = downSum / N;
                    Double RS = prevUpSome / prevDownSome;
                    Double C = 100.0 / (RS + 1.0);
                    rsiList.Add(100.0 - C);
                }
                else
                {
                    if (historyDataArray[i].c > historyDataArray[i - 1].c)
                    {
                        /* 上漲 */
                        prevUpSome = prevUpSome + 1.0 / N * (historyDataArray[i].c -
                          historyDataArray[i - 1].c - prevUpSome);
                        prevDownSome = prevDownSome + 1.0 / N * (0.0 - prevDownSome);
                    }
                    else if (historyDataArray[i].c < historyDataArray[i - 1].c)
                    {
                        /* 下跌 */
                        prevUpSome = prevUpSome + 1.0 / N * (0.0 - prevUpSome);
                        prevDownSome = prevDownSome + 1.0 / N * (historyDataArray[i - 1].c -
                          historyDataArray[i].c - prevDownSome);
                    }
                    Double RS = prevUpSome / prevDownSome;
                    Double C = 100.0 / (RS + 1.0);
                    rsiList.Add(100.0 - C);
                }
            }
            return rsiList.ToArray();
        }
        /*
         * 函式 getStockIndicatorArray 用來取得大盤及公司的技術分析資料
         *      傳入參數 pCount 決定要由前取出幾筆歷史資料來計算各種技術分析資料
         *      Yahoo 股市一般是取 80 筆，而非由最前一筆開始計算
         *      二者計算出的結果會有差距
         */
        public StockIndicator[] getStockIndicatorArray(int pCount)
        {
            List<StockIndicator> stockIndicatorList = new List<StockIndicator>();
            /*
                2017/06/06 JavaScript 版註解
                以下程式只保留最後 pCount 個歷史資料來做指標計算
            */
            if (historyDataArray.Length > pCount)
            {
                List<HistoryData> tempHistoryDataList = new List<HistoryData>();
                for (var i = 0; i < pCount; i++)
                {
                    tempHistoryDataList.Add(historyDataArray[historyDataArray.Length - (pCount - i)]);
                }
                historyDataArray = tempHistoryDataList.ToArray();
            }
            /*
                以下程式片段根據 historyDataArray 初始化 historyMomentumArray
                動能資料陣列中的值為 0。
                historyMomentumArray 及 historyDataArray 二個陣列的大小完全
                一樣，時間一對一對應皆相同。
            */
            for (var i = 0; i < historyDataArray.Length; i++)
            {
                StockIndicator stockIndicator = new StockIndicator();
                stockIndicator.time = historyDataArray[i].t;
                stockIndicatorList.Add(stockIndicator);
            }
            StockIndicator[] stockIndicatorArray = stockIndicatorList.ToArray();
            var cema15Array = calcCemaArray(0, 0, 0.15);
            var cema7dot5Array = calcCemaArray(0, 0, 0.075);
            var macdArray = calcMacdArray(cema15Array, cema7dot5Array);
            var macd9MAArray = calcMacd9MAArray(macdArray, 9);
            var yahooMacdArray = calcYahooMACD();
            var rsv9Array = calcRsvArray(9);
            var kdArray = calcKDJArray(rsv9Array);
            var rsi5Array = calcRsiArray(5);
            var rsi10Array = calcRsiArray(10);
            for (int i = 0; i < stockIndicatorArray.Length; i++)
            {
                stockIndicatorArray[i].CEMA7 = cema7dot5Array[i];
                stockIndicatorArray[i].CEMA15 = cema15Array[i];
                stockIndicatorArray[i].MACD = macdArray[i];
                stockIndicatorArray[i].MACD9MA = macd9MAArray[i];
                stockIndicatorArray[i].yahooDiff9 = yahooMacdArray[i].diff9;
                stockIndicatorArray[i].yahooMACD = yahooMacdArray[i].macd;
                stockIndicatorArray[i].RSV9 = rsv9Array[i];
                stockIndicatorArray[i].K = kdArray[i].K;
                stockIndicatorArray[i].D = kdArray[i].D;
                stockIndicatorArray[i].J = kdArray[i].J;
                stockIndicatorArray[i].RSI5 = rsi5Array[i];
                stockIndicatorArray[i].RSI10 = rsi10Array[i];
            }
            return stockIndicatorArray;
        }
    }
}
