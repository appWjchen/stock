using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

/*
 * Trace 類別是用來追踪股票的狀況，當分析及篩選出某些值得購買的股票時，會自動
 * 將該股票加入到此追踪系統中，使用者也可以手動將股票加入到追踪系統中。
 * 每個加入追踪的股票會有一個 type 類型的資料成員，它代表了它是為何會被加入到
 * 追踪系統中，例如 type="N" 表示該股票是使用者手動加入追踪的股票，如此使用者
 * 可以立即了解該股票被追踪的原因。
 *   type 可以有下列值：
 *      (1) "B"：双重二次攻擊法(有篩選)篩選出的股票
 *      (2) "C"：双重二次攻擊法(超強過濾)篩選出的股票
 *      () "N"：使用者手動加入的股票
 * 函式成員：
 * 1.   addCompany：
 *      將 company 以 type 類型加入到追踪列表中
 * 2.   findTraceCompany：
 *      用來在追踪列表中找尋股票代號是 id 的股票是否已經加入到追踪列表中，如
 *      果在追踪列表中有找到該公司，則傳回該公司的物件，資料型態是 TraceCompany
 *      ，否則傳回 null。
 */

namespace stock
{
    class TraceCompany
    {
        public StockDatabase stockDatabase;
        public Company company;
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
        //  N 手動加入的股票
        public Double startPrice;           // 被追踪股票加入追踪當日的股價
        public Double kValueDay;            // 被追踪股票加入追踪當日的日 K 值
        public Double kValueWeek;           // 被追踪股票加入追踪當日的週 K 值
        public Double kValueMonth;          // 被追踪股票加入追踪當日的月 K 值
        public Double kValueTWStockDay;     // 被追踪股票加入追踪當日的大盤日 K 值
        public Double kValueTWStockWeek;    // 被追踪股票加入追踪當日的大盤週 K 值
        public Double kValueTWStockMonth;   // 被追踪股票加入追踪當日的大盤月 K 值
        public Boolean hasBought;           // 被追踪股票是否已被購入
        public Int32 maxScore;              // 被追踪股票最高分數
        public DateTime maxScoreDate;       // 被追踪股票最高分數發生日期
        public Double maxPrice;             // 被追踪股票的最高價格
        public DateTime maxPriceDate;       // 被追踪股票最高價格發生的日期

        /* 
         * TraceCompany 建構式 
         */
        public TraceCompany(Company company, String type, StockDatabase stockDatabase, Int32 startSCore)
        {
            passScoreTestExplain = "";
            this.stockDatabase = stockDatabase;
            this.company = company;
            this.id = company.id;
            this.name = company.name;
            this.date = DateTime.Now;
            this.hasBought = false;
            this.maxScore = 0;
            this.maxScoreDate = DateTime.Now;
            this.maxPrice = 0;
            this.maxPriceDate = DateTime.Now;

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

            this.count = 1;
            this.startPrice = dayHistoryData[dayHistoryData.Length - 1].c;
            this.type = type;
            if (startSCore == -1)
            {
                this.startScore = evaluateScore();
            }
            else
            {
                this.startScore = startSCore;
            }
            this.score = startScore;
            this.upPercent = 0;
        }
        /*
         * 函式 evaluateUpPercent 用來計算被追踪股票從加入追踪當天到今天所上漲的幅
         * 度。
         */
        public void evaluateUpPercent()
        {
            HistoryData[] dayHistoryData = this.company.getRealHistoryDataArray("d");
            Double todayPrice = dayHistoryData[dayHistoryData.Length - 1].c;
            this.upPercent = (todayPrice - this.startPrice) / this.startPrice;
            this.passScoreTestExplain = this.passScoreTestExplain +
                "\t\t加入追踪當日股價為： " + this.startPrice + " ，今天股價為： " + todayPrice + " ";
            if (upPercent >= 0)
            {
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "，上漲 " + (this.upPercent * 100).ToString("f2") + "%\r\n";
            }
            else
            {
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "，下跌 " + (-this.upPercent * 100).ToString("f2") + "%\r\n";
            }
            company.getPrevHighANdLowIndex();
            this.passScoreTestExplain = this.passScoreTestExplain +
                "\t\t今天往前六年最高股價為： " + company.highestIndex + " ，最低股價為： " + company.lowestIndex + "\r\n";
            Double priceDiff = company.highestIndex - company.lowestIndex;
            Double rate = 100 - 100 * (todayPrice - company.lowestIndex) / priceDiff;
            this.passScoreTestExplain = this.passScoreTestExplain +
                "\t\t今日股價較前高低： " + rate.ToString("f2") + "%\r\n";
        }
        /*
         * 以下為函式 evaluateScore 用來追踪通過測試的項目。
         */
        public Boolean passDividend;
        public Boolean passSeansonEPS;
        public Boolean passYearEps;
        public Boolean passKValueDay5;
        public Boolean passKValueDay10;
        public Boolean passKValueDay15;
        public Boolean passKValueDay20;
        public Boolean passKValueWeek5;
        public Boolean passKValueWeek10;
        public Boolean passKValueWeek15;
        public Boolean passKValueWeek20;
        public Boolean passKValueMonth5;
        public Boolean passKValueMonth10;
        public Boolean passKValueMonth15;
        public Boolean passKValueMonth20;
        public Boolean passKValueStockDay5;
        public Boolean passKValueStockDay10;
        public Boolean passKValueStockDay15;
        public Boolean passKValueStockDay20;
        public Boolean passKValueStockWeek5;
        public Boolean passKValueStockWeek10;
        public Boolean passKValueStockWeek15;
        public Boolean passKValueStockWeek20;
        public Boolean passKValueStockMonth5;
        public Boolean passKValueStockMonth10;
        public Boolean passKValueStockMonth15;
        public Boolean passKValueStockMonth20;
        public Int32 sValueDiffScore;
        public Int32 sValue10DayDiffSCore;
        public Int32 priceRateScore;
        /*
         * passScoreTestExplain 用來記錄分數測試的結果之說明字串。
         */
        public String passScoreTestExplain = "";
        /*
         * 函式 evaluateScore 用來計算被追踪股票今天的分數。
         */
        public Int32 evaluateScore()
        {
            Int32 score = 0;
            this.passDividend = false;
            this.passSeansonEPS = false;
            this.passYearEps = false;
            this.passKValueDay5 = false;
            this.passKValueDay10 = false;
            this.passKValueDay15 = false;
            this.passKValueDay20 = false;
            this.passKValueWeek5 = false;
            this.passKValueWeek10 = false;
            this.passKValueWeek15 = false;
            this.passKValueWeek20 = false;
            this.passKValueMonth5 = false;
            this.passKValueMonth10 = false;
            this.passKValueMonth15 = false;
            this.passKValueMonth20 = false;
            this.passKValueStockDay5 = false;
            this.passKValueStockDay10 = false;
            this.passKValueStockDay15 = false;
            this.passKValueStockDay20 = false;
            this.passKValueStockWeek5 = false;
            this.passKValueStockWeek10 = false;
            this.passKValueStockWeek15 = false;
            this.passKValueStockWeek20 = false;
            this.passKValueStockMonth5 = false;
            this.passKValueStockMonth10 = false;
            this.passKValueStockMonth15 = false;
            this.passKValueStockMonth20 = false;
            this.passScoreTestExplain = this.passScoreTestExplain +
                "\t得分意義：\r\n";
            /* 每年股利無負值加 1 分 */
            company.getDividend();
            Boolean passDividend = true;
            for (var i = 0; i < company.dividend.Length; i++)
            {
                if (company.dividend[i] <= 0)
                {
                    passDividend = false;
                }
            }
            if (passDividend)
            {
                score++;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t每年股利無負值加 1 分\r\n";
            }
            this.passDividend = passDividend;
            /* 每季 EPS 無負值加 1 分 */
            company.getSeasonEPS();
            Boolean passSeansonEPS = true;
            for (var i = 0; i < company.seasonEPS.Length; i++)
            {
                if (company.seasonEPS[i] <= 0)
                {
                    passSeansonEPS = false;
                }
            }
            if (passSeansonEPS)
            {
                score++;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t每季 EPS 無負值加 1 分\r\n";
            }
            this.passSeansonEPS = passSeansonEPS;
            /* 每年 EPS 無負值加 1 分 */
            Boolean passYearEps = true;
            company.getYearEPS();
            for (var i = 0; i < company.yearEPS.Length; i++)
            {
                if (company.yearEPS[i] <= 0)
                {
                    passYearEps = false;
                }
            }
            if (passYearEps)
            {
                score++;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t每年 EPS 無負值加 1 分\r\n";
            }
            this.passYearEps = passYearEps;
            /* 個股 K<20 +1 K<15 +2 K<10 +3 K<5 +4 */
            var historyData80 = company.getHistoryData80(company.getRealHistoryDataArray("d"));
            var dayHistoryData80 = historyData80;
            Double todayPrice = dayHistoryData80[dayHistoryData80.Length - 1].c;
            KDJ[] kValueDay = null;
            KDJ[] kValueWeek = null;
            KDJ[] kValueMonth = null;
            if (historyData80.Length == 0)
            {
                new MessageWriter().appendMessage(name + "(" + id + ") 無法取得每日歷史資料!?\r\n", true);
            }
            else
            {
                kValueDay = company.kValue(historyData80);
            }
            historyData80 = company.getHistoryData80(company.getRealHistoryDataArray("w"));
            if (historyData80.Length == 0)
            {
                new MessageWriter().appendMessage(name + "(" + id + ") 無法取得每周歷史資料!?\r\n", true);
            }
            else
            {
                kValueWeek = company.kValue(historyData80);
            }
            historyData80 = company.getHistoryData80(company.getRealHistoryDataArray("m"));
            if (historyData80.Length == 0)
            {
                new MessageWriter().appendMessage(name + "(" + id + ") 無法取得每月歷史資料!?\r\n", true);
            }
            else
            {
                kValueMonth = company.kValue(historyData80);
            }
            Double kValueDayToday = 100;
            Double kValueWeekToday = 100;
            Double kValueMonthToday = 100;
            if ((kValueDay != null) && (kValueDay.Length > 0))
            {
                kValueDayToday = kValueDay[kValueDay.Length - 1].K;
            }
            if ((kValueWeek != null) && (kValueWeek.Length > 0))
            {
                kValueWeekToday = kValueWeek[kValueWeek.Length - 1].K;
            }
            if ((kValueMonth != null) && (kValueMonth.Length > 0))
            {
                kValueMonthToday = kValueMonth[kValueMonth.Length - 1].K;
            }
            if (kValueDayToday < 5)
            {
                score += 4;
                this.passKValueDay5 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t個股日 K 值小於 5 加 4 分\r\n";
            }
            else if (kValueDayToday < 10)
            {
                score += 3;
                this.passKValueDay10 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t個股日 K 值小於 10 加 3 分\r\n";
            }
            else if (kValueDayToday < 15)
            {
                score += 2;
                this.passKValueDay15 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t個股日 K 值小於 15 加 2 分\r\n";
            }
            else if (kValueDayToday < 20)
            {
                score += 1;
                this.passKValueDay20 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t個股日 K 值小於 20 加 1 分\r\n";
            }
            if (kValueWeekToday < 5)
            {
                score += 4;
                this.passKValueWeek5 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t個股週 K 值小於 5 加 4 分\r\n";
            }
            else if (kValueWeekToday < 10)
            {
                score += 3;
                this.passKValueWeek10 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t個股週 K 值小於 10 加 3 分\r\n";
            }
            else if (kValueWeekToday < 15)
            {
                score += 2;
                this.passKValueWeek15 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t個股週 K 值小於 15 加 2 分\r\n";
            }
            else if (kValueWeekToday < 20)
            {
                score += 1;
                this.passKValueWeek20 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t個股週 K 值小於 20 加 1 分\r\n";
            }
            if (kValueMonthToday < 5)
            {
                score += 4;
                this.passKValueMonth5 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t個股月 K 值小於 5 加 4 分\r\n";
            }
            else if (kValueMonthToday < 10)
            {
                score += 3;
                this.passKValueMonth10 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t個股月 K 值小於 10 加 3 分\r\n";
            }
            else if (kValueMonthToday < 15)
            {
                score += 2;
                this.passKValueMonth15 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t個股月 K 值小於 15 加 2 分\r\n";
            }
            else if (kValueMonthToday < 20)
            {
                score += 1;
                this.passKValueMonth20 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t個股月 K 值小於 20 加 1 分\r\n";
            }
            /* 大盤 K<20 +1 K<15 +2 K<10 +3 K<5 +4 */
            historyData80 = stockDatabase.getHistoryData80(stockDatabase.getDayHistoryData());
            dayHistoryData80 = historyData80;
            kValueDay = null;
            kValueWeek = null;
            kValueMonth = null;
            if (historyData80.Length == 0)
            {
                new MessageWriter().appendMessage(name + "(" + id + ") 無法取得大盤每日歷史資料!?\r\n", true);
            }
            else
            {
                kValueDay = stockDatabase.kValue(historyData80);
            }
            historyData80 = stockDatabase.getHistoryData80(stockDatabase.getWeekHistoryData());
            if (historyData80.Length == 0)
            {
                new MessageWriter().appendMessage(name + "(" + id + ") 無法取得大盤每周歷史資料!?\r\n", true);
            }
            else
            {
                kValueWeek = stockDatabase.kValue(historyData80);
            }
            historyData80 = stockDatabase.getHistoryData80(stockDatabase.getMonthHistoryData());
            if (historyData80.Length == 0)
            {
                new MessageWriter().appendMessage(name + "(" + id + ") 無法取得大盤每月歷史資料!?\r\n", true);
            }
            else
            {
                kValueMonth = stockDatabase.kValue(historyData80);
            }
            kValueDayToday = 100;
            kValueWeekToday = 100;
            kValueMonthToday = 100;
            if ((kValueDay != null) && (kValueDay.Length > 0))
            {
                kValueDayToday = kValueDay[kValueDay.Length - 1].K;
            }
            if ((kValueWeek != null) && (kValueWeek.Length > 0))
            {
                kValueWeekToday = kValueWeek[kValueWeek.Length - 1].K;
            }
            if ((kValueMonth != null) && (kValueMonth.Length > 0))
            {
                kValueMonthToday = kValueMonth[kValueMonth.Length - 1].K;
            }
            if (kValueDayToday < 5)
            {
                score += 4;
                this.passKValueStockDay5 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t大盤日 K 值小於 5 加 4 分\r\n";
            }
            else if (kValueDayToday < 10)
            {
                score += 3;
                this.passKValueStockDay10 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t大盤日 K 值小於 10 加 3 分\r\n";
            }
            else if (kValueDayToday < 15)
            {
                score += 2;
                this.passKValueStockDay15 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t大盤日 K 值小於 15 加 3 分\r\n";
            }
            else if (kValueDayToday < 20)
            {
                score += 1;
                this.passKValueStockDay20 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t大盤日 K 值小於 20 加 1 分\r\n";
            }
            if (kValueWeekToday < 5)
            {
                score += 4;
                this.passKValueStockWeek5 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t大盤週 K 值小於 5 加 4 分\r\n";
            }
            else if (kValueWeekToday < 10)
            {
                score += 3;
                this.passKValueStockWeek10 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t大盤週 K 值小於 10 加 3 分\r\n";
            }
            else if (kValueWeekToday < 15)
            {
                score += 2;
                this.passKValueStockWeek15 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t大盤週 K 值小於 15 加 2 分\r\n";
            }
            else if (kValueWeekToday < 20)
            {
                score += 1;
                this.passKValueStockWeek20 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t大盤週 K 值小於 20 加 1 分\r\n";
            }
            if (kValueMonthToday < 5)
            {
                score += 4;
                this.passKValueStockMonth5 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t大盤月 K 值小於 5 加 4 分\r\n";
            }
            else if (kValueMonthToday < 10)
            {
                score += 3;
                this.passKValueStockMonth10 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t大盤月 K 值小於 10 加 3 分\r\n";
            }
            else if (kValueMonthToday < 15)
            {
                score += 2;
                this.passKValueStockMonth15 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t大盤月 K 值小於 15 加 2 分\r\n";
            }
            else if (kValueMonthToday < 20)
            {
                score += 1;
                this.passKValueStockMonth20 = true;
                this.passScoreTestExplain = this.passScoreTestExplain +
                    "\t\t大盤月 K 值小於 20 加 1 分\r\n";
            }
            /* 往前 10 天，法人買超 1 天加 1 分 */
            HistoryData[] historydata = company.getRealHistoryDataArray("d");
            int count = 0;
            int sValueDiffScore = 0;
            for (var i = (historydata.Length - 1); i >= 0; i--)
            {
                if (count >= 10)
                {
                    break;
                }
                if ((i >= 0) && ((i - 1) >= 0))
                {
                    Double sValueDiff = historydata[i].s - historydata[i - 1].s;
                    if (sValueDiff > 0)
                    {
                        sValueDiffScore++;
                    }
                }
                count++;
            }
            score += sValueDiffScore;
            this.passScoreTestExplain = this.passScoreTestExplain +
                "\t\t往前 10 天，法人買超 1 天加 1 分，共加 " + sValueDiffScore + " 分\r\n";
            this.sValueDiffScore = sValueDiffScore;
            /* 法人 10 天總買超每增加 0.1% 加 1 分 */
            int sValue10DayDiffSCore = 0;
            if (historydata.Length >= 10)
            {
                Double sValue1 = historydata[historydata.Length - 10].s;
                Double sValue2 = historydata[historydata.Length - 1].s;
                Double sIncPercent = 100 * (sValue2 - sValue1) / sValue1;
                if (sIncPercent > 0)
                {
                    sValue10DayDiffSCore = (int)(sIncPercent / 0.1);
                }
            }
            score += sValue10DayDiffSCore;
            this.passScoreTestExplain = this.passScoreTestExplain +
                "\t\t法人 10 天總買超每增加 0.1% 加 1 分，共加 " + sValue10DayDiffSCore + " 分\r\n";
            this.sValue10DayDiffSCore = sValue10DayDiffSCore;
            /* 比前高(50%)每少 1% 加 1 分 */
            company.getPrevHighANdLowIndex();
            Double priceDiff = company.highestIndex - company.lowestIndex;
            Double rate = 100 * (todayPrice - company.lowestIndex) / priceDiff;
            Int32 priceRateScore = (int)(50 - rate);
            if (priceRateScore > 0)
            {
                score += priceRateScore;
            }
            else
            {
                priceRateScore = 0;
            }
            this.priceRateScore = priceRateScore;
            this.passScoreTestExplain = this.passScoreTestExplain +
                "\t\t比前高(50%)每少 1% 加 1 分，共加 " + priceRateScore + " 分\r\n";
            return score;
        }
    }
    class Trace
    {
        public String traceFilename = "trace.dat";
        public StockDatabase stockDatabase;
        public ListView listView;
        public Label labelOutMessage;
        private List<TraceCompany> traceCompanyList = null;

        /*
         * Trace 建構式
         */
        public Trace(StockDatabase stockDatabase, ListView listView, Label labelOutMessage)
        {
            this.stockDatabase = stockDatabase;
            traceCompanyList = new List<TraceCompany>();
            this.listView = listView;
            this.labelOutMessage = labelOutMessage;
            this.labelOutMessage.ForeColor = Color.Purple;
            load();
        }
        /*
         * 函式 addCompany 用來將 company 以類型 type 加入到追踪系統中
         */
        public void addCompany(Company company, String type)
        {
            TraceCompany foundTraceCompany = findTraceCompany(company.id);
            if (foundTraceCompany == null)
            {
                TraceCompany traceCompany = new TraceCompany(company, type, stockDatabase, -1);
                traceCompany.evaluateUpPercent();
                traceCompanyList.Add(traceCompany);
                listView.Items.Add(traceCompany.id, traceCompany.id, 0);
                listView.Items[traceCompany.id].SubItems.Add(traceCompany.name);
                listView.Items[traceCompany.id].SubItems.Add(traceCompany.date.ToString("yyyy/MM/dd"));
                listView.Items[traceCompany.id].SubItems.Add(traceCompany.startScore.ToString());
                listView.Items[traceCompany.id].SubItems.Add(traceCompany.score.ToString());
                listView.Items[traceCompany.id].SubItems.Add(traceCompany.count.ToString());
                listView.Items[traceCompany.id].SubItems.Add((traceCompany.upPercent * 100).ToString("f2"));
                listView.Items[traceCompany.id].SubItems.Add(traceCompany.type);
                listView.Items[traceCompany.id].SubItems.Add(traceCompany.maxScore.ToString("f0"));
                listView.Items[traceCompany.id].SubItems.Add(traceCompany.maxScoreDate.ToString("yyyy/MM/dd"));
                listView.Items[traceCompany.id].SubItems.Add(traceCompany.maxPrice.ToString("f2"));
                listView.Items[traceCompany.id].SubItems.Add(traceCompany.maxPriceDate.ToString("yyyy/MM/dd"));
            }
            else
            {
                foundTraceCompany.count++;
                foundTraceCompany.type = foundTraceCompany.type + type;
                listView.Items[foundTraceCompany.id].SubItems[5].Text = foundTraceCompany.count.ToString();
                listView.Items[foundTraceCompany.id].SubItems[7].Text = foundTraceCompany.type;
            }
        }
        /*
         * remove 函式從追踪股票列表中移除名稱是 id 的股票。
         */
        public void remove(String id)
        {
            TraceCompany foundTraceCompany = findTraceCompany(id);
            traceCompanyList.Remove(foundTraceCompany);
            listView.Items[foundTraceCompany.id].Remove();
        }
        /*
         * 函式 findTraceCompany 用來在追踪列表中找尋股票代號是 id 的股票是否已
         * 經加入到追踪列表中，如果在追踪列表中有找到該公司，則傳回該公司的物件
         * ，資料型態是 TraceCompany ，否則傳回 null。
         */
        public TraceCompany findTraceCompany(String id)
        {
            for (var i = 0; i < traceCompanyList.Count(); i++)
            {
                TraceCompany traceCompany = traceCompanyList[i];
                if (traceCompany.id == id)
                {
                    return traceCompany;
                }
            }
            return null;
        }
        /*
         * 函式 save 用來將現有追踪股票存檔。
         * 檔案儲存於 database 目錄下的 trace.dat 檔案。
         */
        public void save()
        {
            String saveText = "";
            for (var i = 0; i < traceCompanyList.Count(); i++)
            {
                TraceCompany traceCompany = traceCompanyList[i];
                saveText = saveText +
                    traceCompany.id + " " +
                    traceCompany.name + " " +
                    traceCompany.date.ToString("yyyy/MM/dd") + " " +
                    traceCompany.startScore.ToString() + " " +
                    traceCompany.count.ToString() + " " +
                    traceCompany.type + " " +
                    traceCompany.startPrice.ToString("f2") + " " +
                    traceCompany.kValueDay.ToString("f2") + " " +
                    traceCompany.kValueWeek.ToString("f2") + " " +
                    traceCompany.kValueMonth.ToString("f2") + " " +
                    traceCompany.kValueTWStockDay.ToString("f2") + " " +
                    traceCompany.kValueTWStockWeek.ToString("f2") + " " +
                    traceCompany.kValueTWStockMonth.ToString("f2") + " " +
                    traceCompany.hasBought + " " +
                    traceCompany.maxScore.ToString() + " " +
                    traceCompany.maxScoreDate.ToString("yyyy/MM/dd") + " " +
                    traceCompany.maxPrice.ToString() + " " +
                    traceCompany.maxPriceDate.ToString("yyyy/MM/dd") + " " +
                    "\r\n";
            }
            new FileHelper().WriteText(traceFilename, saveText);
        }
        /*
         * 函式 load 用來從追踪存檔中讀出所有被追踪的股票。
         */
        public void load()
        {
            FileHelper fileHelper = new FileHelper();
            if (fileHelper.Exists(traceFilename))
            {
                var saveData = fileHelper.ReadText(traceFilename);
                var saveDataSplit = saveData.Split(new string[] { "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
                int upCount = 0;
                int successfulCount = 0;
                int failCount = 0;
                for (var i = 0; i < saveDataSplit.Length; i++)
                {
                    String oneTraceData = saveDataSplit[i];
                    var oneTraceDataSplit = oneTraceData.Split(new string[] { " " },
                    StringSplitOptions.RemoveEmptyEntries);
                    if (oneTraceDataSplit.Length >= 13)
                    {
                        String id = oneTraceDataSplit[0];
                        String name = oneTraceDataSplit[1];
                        int Year = Convert.ToInt32(oneTraceDataSplit[2].Substring(0, 4));
                        int Month = Convert.ToInt32(oneTraceDataSplit[2].Substring(5, 2));
                        int Day = Convert.ToInt32(oneTraceDataSplit[2].Substring(8, 2));
                        DateTime date = new DateTime(Year, Month, Day);
                        Int32 startScore = Convert.ToInt32(oneTraceDataSplit[3]);
                        Int32 count = Convert.ToInt32(oneTraceDataSplit[4]);
                        String type = oneTraceDataSplit[5];
                        Double startPrice = Convert.ToDouble(oneTraceDataSplit[6]);
                        Double kValueDay = Convert.ToDouble(oneTraceDataSplit[7]);
                        Double kValueWeek = Convert.ToDouble(oneTraceDataSplit[8]);
                        Double kValueMonth = Convert.ToDouble(oneTraceDataSplit[9]);
                        Double kValueTWStockDay = Convert.ToDouble(oneTraceDataSplit[10]);
                        Double kValueTWStockWeek = Convert.ToDouble(oneTraceDataSplit[11]);
                        Double kValueTWStockMonth = Convert.ToDouble(oneTraceDataSplit[12]);
                        Boolean hasBought = Convert.ToBoolean(oneTraceDataSplit[13]);
                                                
                        Int32 maxScore = Convert.ToInt32(oneTraceDataSplit[14]);
                        Year = Convert.ToInt32(oneTraceDataSplit[15].Substring(0, 4));
                        Month = Convert.ToInt32(oneTraceDataSplit[15].Substring(5, 2));
                        Day = Convert.ToInt32(oneTraceDataSplit[15].Substring(8, 2));
                        DateTime maxScoreDate = new DateTime(Year, Month, Day);
                        Double maxPrice = Convert.ToDouble(oneTraceDataSplit[16]);
                        Year = Convert.ToInt32(oneTraceDataSplit[17].Substring(0, 4));
                        Month = Convert.ToInt32(oneTraceDataSplit[17].Substring(5, 2));
                        Day = Convert.ToInt32(oneTraceDataSplit[17].Substring(8, 2));
                        DateTime maxPriceDate = new DateTime(Year, Month, Day);
                        
                        Company company = stockDatabase.getCompany(id);
                        if (company != null)
                        {
                            TraceCompany traceCompany = new TraceCompany(company, type, stockDatabase, startScore);
                            traceCompany.id = id;
                            traceCompany.name = name;
                            traceCompany.date = date;
                            traceCompany.startScore = startScore;
                            traceCompany.count = count;
                            traceCompany.type = type;
                            traceCompany.startPrice = startPrice;
                            traceCompany.kValueDay = kValueDay;
                            traceCompany.kValueWeek = kValueWeek;
                            traceCompany.kValueMonth = kValueMonth;
                            traceCompany.kValueTWStockDay = kValueTWStockDay;
                            traceCompany.kValueTWStockWeek = kValueTWStockWeek;
                            traceCompany.kValueTWStockMonth = kValueTWStockMonth;
                            traceCompany.evaluateUpPercent();
                            traceCompany.score = traceCompany.evaluateScore();
                            traceCompanyList.Add(traceCompany);
                            traceCompany.hasBought = hasBought;
                            traceCompany.maxScore = maxScore;
                            traceCompany.maxScoreDate = maxScoreDate;
                            traceCompany.maxPrice = maxPrice;
                            traceCompany.maxPriceDate = maxPriceDate;

                            if (traceCompany.score > maxScore)
                            {
                                traceCompany.maxScore = traceCompany.score;
                                traceCompany.maxScoreDate = DateTime.Now;
                            }
                            HistoryData[] dayHistoryData = traceCompany.company.getRealHistoryDataArray("d");
                            Double todayPrice = dayHistoryData[dayHistoryData.Length - 1].c;
                            if (todayPrice > traceCompany.maxPrice)
                            {
                                traceCompany.maxPrice = todayPrice;
                                traceCompany.maxPriceDate = DateTime.Now;
                            }
                            if (todayPrice > traceCompany.startPrice)
                            {
                                /* 計算目前追踪股票是否上漲，並統計總數 */
                                upCount++;
                            }
                            /*
                            if (traceCompany.startScore > traceCompany.score)
                            {
                                traceCompany.maxScore = traceCompany.startScore;
                                traceCompany.maxScoreDate = traceCompany.date;
                            }
                            else
                            {
                                traceCompany.maxScore = traceCompany.score;
                                traceCompany.maxScoreDate = DateTime.Now;
                            }
                            HistoryData[] dayHistoryData = traceCompany.company.getRealHistoryDataArray("d");
                            Double todayPrice = dayHistoryData[dayHistoryData.Length - 1].c;
                            if (traceCompany.startPrice > todayPrice)
                            {
                                traceCompany.maxPrice = traceCompany.startPrice;
                                traceCompany.maxPriceDate = traceCompany.date;
                            }
                            else
                            {
                                traceCompany.maxPrice = todayPrice;
                                traceCompany.maxPriceDate = DateTime.Now;
                            }
                            */
                            Double traceDays = (DateTime.Now - traceCompany.date).TotalDays;
                            if (todayPrice < 60)
                            {
                                /* 追踪超過 2 個月股票，上漲機率統計 */
                                if (((traceCompany.maxPrice - traceCompany.startPrice) / traceCompany.startPrice) > 0.1)
                                {
                                    successfulCount++;
                                }
                            }
                            if (traceDays > 60)
                            {
                                /* 追踪超過 2 個月股票，下跌機率統計 */
                                if (((traceCompany.maxPrice - traceCompany.startPrice) / traceCompany.startPrice) < 0.1)
                                {
                                    failCount++;
                                }
                            }
                            listView.Items.Add(traceCompany.id, traceCompany.id, 0);
                            listView.Items[traceCompany.id].SubItems.Add(traceCompany.name);
                            listView.Items[traceCompany.id].SubItems.Add(traceCompany.date.ToString("yyyy/MM/dd"));
                            listView.Items[traceCompany.id].SubItems.Add(traceCompany.startScore.ToString());
                            listView.Items[traceCompany.id].SubItems.Add(traceCompany.score.ToString());
                            listView.Items[traceCompany.id].SubItems.Add(traceCompany.count.ToString());
                            listView.Items[traceCompany.id].SubItems.Add((traceCompany.upPercent * 100).ToString("f2"));
                            listView.Items[traceCompany.id].SubItems.Add(traceCompany.type);
                            
                            listView.Items[traceCompany.id].SubItems.Add(traceCompany.maxScore.ToString("f0"));
                            listView.Items[traceCompany.id].SubItems.Add(traceCompany.maxScoreDate.ToString("yyyy/MM/dd"));
                            listView.Items[traceCompany.id].SubItems.Add(traceCompany.maxPrice.ToString("f2"));
                            listView.Items[traceCompany.id].SubItems.Add(traceCompany.maxPriceDate.ToString("yyyy/MM/dd"));
                            
                            if (hasBought)
                            {
                                listView.Items[traceCompany.id].UseItemStyleForSubItems = false;
                                for (var k = 0; k < listView.Items[traceCompany.id].SubItems.Count; k++)
                                {
                                    listView.Items[traceCompany.id].SubItems[k].ForeColor = Color.DarkBlue;
                                }
                            }
                            else
                            {
                                listView.Items[traceCompany.id].UseItemStyleForSubItems = false;
                                for (var k = 0; k < listView.Items[traceCompany.id].SubItems.Count; k++)
                                {
                                    listView.Items[traceCompany.id].SubItems[k].ForeColor = Color.OrangeRed;
                                }
                            }
                            if (traceCompany.upPercent > 0)
                            {
                                listView.Items[traceCompany.id].SubItems[6].ForeColor = Color.Red;
                            }
                            else if (traceCompany.upPercent <= 0)
                            {
                                listView.Items[traceCompany.id].SubItems[6].ForeColor = Color.Green;
                            }
                            else
                            {
                                listView.Items[traceCompany.id].SubItems[6].ForeColor = Color.Black;
                            }
                            listView.Items[traceCompany.id].SubItems[6].Font = new Font(listView.Items[traceCompany.id].SubItems[6].Font,
                                listView.Items[traceCompany.id].SubItems[6].Font.Style | FontStyle.Bold);
                        }
                    }
                }
                this.labelOutMessage.Text = "追踪上漲 " + upCount + " 家，下跌 " + (saveDataSplit.Length - upCount) + " 家\r\n";
                this.labelOutMessage.Text = this.labelOutMessage.Text + "追踪二個月成功 " + successfulCount
                    + " 家，失敗 " + failCount + " 家";
            }
        }
    }
}
