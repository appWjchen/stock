/*
  StockDatabase 類別：
    方法成員：
      01. createDatabase(callback)
          此方法將下載台股加權指數的每日、每週及每月歷史資料，並存入下列檔案中：
            database\twStock\day.dat
            database\twStock\week.dat
            database\twStock\month.dat
          歷史資料都是以附加的方式添加到原檔案的後面，資料不重覆。
          歷史資料庫中資料格式為每一行代表一日、一週或一月的一筆資料，格式如下：
            時間 開盤價 最高價 最低價 收盤價 成交量(百萬)
            t   o      h     l     c     v
      02. createAllCompaniesDatabase(callback)
          此方法用以建立所有公司的各別資料庫檔案，建立完成後呼叫 callback 函式
      03. createAllCompanyHistoryDatabase(callback)
          此方法用以建立所有公司的各別歷史資料庫檔案，建立完成後呼叫 callback 函式
              database\company\各公司ID\day.dat
              database\company\各公司ID\week.dat
              database\company\各公司ID\month.dat
      04. createAllCompanyInformationDatabase(callback)
          此方法用以建立所有公司的各別基本資訊資料庫檔案，建立完成後呼叫 callback 函式
              database\company\各公司ID\baseInformation.dat
              database\company\各公司ID\marginInformation.dat
              database\company\各公司ID\seasonEPS.dat
              database\company\各公司ID\yearEPS.dat
      05. createAllCompanyEarningDatabase(callback)
          此方法用以建立所有公司的各別各月獲利能力資料庫檔案，建立完成後呼叫 callback 函式
              database\company\各公司ID\earning.dat
      06. createAllCompanyDividendDatabase(callback)
          此方法用以建立所有公司的各別每年股利資料庫檔案，建立完成後呼叫 callback 函式
              database\company\各公司ID\dividend.dat
      07. getDayHistoryData()
          取得台股大盤的每日歷史資料陣列
      08. getWeekHistoryData()
          取得台股大盤的每週歷史資料陣列
      09. getMonthHistoryData()
          取得台股大盤的每月歷史資料陣列
      10. getCompany(id)
          取得公司代號是 id 的公司資料物件
      11. sortCompanyByCapital()
          將各公司按資本額做排名
      12. getInformation()
          取得大盤的簡單描述資訊
      13. public KDJ[] kValue(HistoryData[] historyData80)
            此方法用來將傳入的 historyData80 計算其 KDJ 值
      14. public HistoryData[] getHistoryData80(HistoryData[] historyData)
            getHistoryData80 由傳入的歷史資料陣列中取最後 80 筆資料傳回。
            如果不足 80 筆則全數傳回。
  資料成員：
      01. companies
          所有公司資料(Company類別)的陣列
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stock
{
    public delegate void CreateStockDatabaseCallback();
    public delegate void CreateAllCompaniesDatabaseCallback();

    class HistoryData
    {
        /*
		    由元大公司下載的歷史資訊，有下列幾點注意事項：
			    01. 資料最多 1400 天/週/月
				02. 下載資料用空格分開，會得到 23 種資料
				03. 每種資料可用逗號分開，得到 1400 筆資料
				04. 23 種資料，已知下列幾種：
						第 00 筆  日期            t
						第 01 筆  開盤價          o
						第 02 筆  最高價          h
						第 03 筆  最低價          l
						第 04 筆  收盤價          c
						第 05 筆  成交量(張)      v
						第 06 筆  融資餘額(張)    m
						第 07 筆  融巻餘額(張)    n
						第 08 筆  外資持股(張)    f
						第 09 筆  投信持股(張)    p
						第 10 筆  自營商持股(張)  i
						第 14 筆  法人合計(張)    s
						第 16 筆  資巻相抵(張)    d
						第 17 筆  融資使用率      u
						第 18 筆  巻資比          r
		*/
        public String t;    // 第 00 筆  日期            t
        public Double o;    // 第 01 筆  開盤價          o
        public Double h;    // 第 02 筆  最高價          h
        public Double l;    // 第 03 筆  最低價          l
        public Double c;    // 第 04 筆  收盤價          c
        public Double v;    // 第 05 筆  成交量(張)      v
        public Double m;    // 第 06 筆  融資餘額(張)    m
        public Double n;    // 第 07 筆  融巻餘額(張)    n
        public Double f;    // 第 08 筆  外資持股(張)    f
        public Double p;    // 第 09 筆  投信持股(張)    p
        public Double i;    // 第 10 筆  自營商持股(張)  i
        public Double s;    // 第 14 筆  法人合計(張)    s
        public Double d;    // 第 16 筆  資巻相抵(張)    d
        public Double u;    // 第 17 筆  融資使用率      u
        public Double r;    // 第 18 筆  巻資比          r
        public bool longDataFormat;
        public HistoryData()
        {
            longDataFormat = false;
            t = "";
            o = 0;
            h = 0;
            l = 0;
            c = 0;
            v = 0;
            m = 0;
            n = 0;
            f = 0;
            p = 0;
            i = 0;
            s = 0;
            d = 0;
            u = 0;
            r = 0;
        }
    }

    class StockDatabase
    {
        public StockCategories stockCategories;
        public CompanyCategories companyCategories;
        public Company[] companies;
        List<Company> companiesList;
        public HistoryData[] dayHistoryData;
        public HistoryData[] weekHistoryData;
        public HistoryData[] monthHistoryData;
        public HistoryData[] dayRealHistoryData;
        public HistoryData[] weeRealkHistoryData;
        public HistoryData[] monthRealHistoryData;
        List<HistoryData> dayHistoryDataList;
        List<HistoryData> weekHistoryDataList;
        List<HistoryData> monthHistoryDataList;
        HttpHelper httpHelper = new HttpHelper();
        FileHelper fileHelper = new FileHelper();
        CreateStockDatabaseCallback createStockDatabaseCallback;
        public String stockDatabaseString;
        public List<int> scoreIndexList;
        public Trace stockTrace;
        public List<LipHipData> lipHipDataList;
        public List<WaveData> waveDataList;
        public WaveStatisticInformation waveStatisticInformationList;

        /*
         * StockDatabase 建構式
         */
        public StockDatabase()
        {
            stockCategories = null;
            companyCategories = null;
            companies = null;
            companiesList = new List<Company>();
            dayHistoryDataList = new List<HistoryData>();
            weekHistoryDataList = new List<HistoryData>();
            monthHistoryDataList = new List<HistoryData>();
            scoreIndexList = new List<int>();
            if (!fileHelper.DirectoryExists(FileHelper.databasePath))
            {
                fileHelper.CreateDirectory(FileHelper.databasePath);
            }
            if (!fileHelper.DirectoryExists(FileHelper.databasePath + "twStock"))
            {
                fileHelper.CreateDirectory(FileHelper.databasePath + "twStock");
            }
            if (!fileHelper.DirectoryExists(FileHelper.databasePath + "company"))
            {
                fileHelper.CreateDirectory(FileHelper.databasePath + "company");
            }
            dayRealHistoryData = null;
            weeRealkHistoryData = null;
            monthRealHistoryData = null;
            lipHipDataList = null;
            waveDataList = null;
            waveStatisticInformationList = null;
        }
        /*
         * 函式 createAllCompaniesDirectiory 用來為各公司建立資料夾。
         */
        /*
            private void createAllCompaniesDirectiory()
            {
                for (int i = 0; i < companies.Length; i++)
                {
                    String companyId = companies[i].id;
                    String companyDirectory = fileHelper.databasePath + "company/" + companyId;
                    if (!fileHelper.DirectoryExists(companyDirectory))
                    {
                        fileHelper.CreateDirectory(companyDirectory);
                    }
                }
            }
         */
        /*
         * 函式 createAllCompanyDatabase 用來建立各公司的
         * Company 物件。
         */
        private void createAllCompanyDatabase()
        {
            for (int i = 0; i < companyCategories.totalCompanies; i++)
            {
                Company company = new Company(
                  companyCategories.companyCategories[i].id,
                  companyCategories.companyCategories[i].name,
                  companyCategories.companyCategories[i].category
                );
                companiesList.Add(company);
            }
            companies = companiesList.ToArray();
        }
        /*
            函式 createDatabase(callback)
            用來下載台股加權指數的每日、每週及每月歷史資料，並存入下列檔案中：
                database\twStock\day.dat
                database\twStock\week.dat
                database\twStock\month.dat
            歷史資料都是以附加的方式添加到原檔案的後面，資料不重覆。
            歷史資料庫中資料格式為每一行代表一日、一週或一月的一筆資料，格式如下：
            時間 開盤價 最高價 最低價 收盤價 成交量(百萬)
                t   o      h     l     c     v
        */
        public void createDatabase(CreateStockDatabaseCallback callback)
        {
            createStockDatabaseCallback = callback;
            stockCategories = new StockCategories();
            stockCategories.createDatabase(
                () =>   // stockCategories.createDatabase 回呼函式
                {
                    companyCategories = new CompanyCategories();
                    companyCategories.createDatabase(
                        stockCategories,
                        () =>   // companyCategories.createDatabase 回呼函式
                        {
                            createAllCompanyDatabase();
                            createDayHistoryDatabase();
                        }
                    );
                }
            );
        }
        /*
         * 函式 httpSourceToHistoryData 將下載的大盤歷史資料字串，例如
         *      null({"mkt":"10 ","id":"#001","perd":"d","type":"ta",
         *      "mem":{"id":"#001","name":"加權指數","129":11071.57,"130":11150.85,"131":11095.85,
         *      ...
         *      "508":12204177,"125":11150.85,"509":9492575,"126":11123.58},"ta":[
         *      ...,
         *      ,{"t":20180119,"o":11123.58,"h":11150.85,"l":11095.85,"c":11150.85,"v":133881}]});
         * 分析出 HistoryData 資料。
         */
        private HistoryData[] httpSourceToHistoryData(String httpSourceIn)
        {
            var httpSource = httpSourceIn;
            List<HistoryData> tempHistoryDataList = new List<HistoryData>();
            int taStartIndex = httpSource.IndexOf("\"ta\"");
            httpSource = httpSource.Substring(taStartIndex + 5);
            taStartIndex = httpSource.IndexOf("\"ta\"");
            int taEndIndex = httpSource.LastIndexOf("]");
            httpSource = httpSource.Substring(taStartIndex + 7, taEndIndex - taStartIndex - 8);
            string[] stringSeparators = new string[] { "},{" };
            string[] historyStringArray = httpSource.Split(stringSeparators,
                StringSplitOptions.RemoveEmptyEntries);
            // httpSource = "";
            for (int i = 0; i < historyStringArray.Length; i++)
            {
                var tempString = historyStringArray[i].Substring(0, historyStringArray[i].Length);
                historyStringArray[i] = tempString;
                stringSeparators = new string[] { "," };
                String[] oneHistoryDataStringArray = historyStringArray[i].Split(stringSeparators,
                    StringSplitOptions.RemoveEmptyEntries);
                HistoryData historyData = new HistoryData();
                for (int j = 0; j < oneHistoryDataStringArray.Length; j++)
                {
                    var oneHistoryDataStrin = oneHistoryDataStringArray[j];
                    String[] oneDataArray = oneHistoryDataStrin.Split(':');
                    switch (oneDataArray[0])
                    {
                        case "\"t\"":
                            // "時間"
                            String Year = oneDataArray[1].Substring(0, 4);
                            String Month = oneDataArray[1].Substring(4, 2);
                            String Day = oneDataArray[1].Substring(6, 2);
                            historyData.t = Year + "/" + Month + "/" + Day;
                            break;
                        case "\"o\"":
                            // "開盤價"
                            historyData.o = Convert.ToDouble(oneDataArray[1]);
                            break;
                        case "\"h\"":
                            // "最高價"
                            historyData.h = Convert.ToDouble(oneDataArray[1]);
                            break;
                        case "\"l\"":
                            // "最低價"
                            historyData.l = Convert.ToDouble(oneDataArray[1]);
                            break;
                        case "\"c\"":
                            // "收盤價"
                            historyData.c = Convert.ToDouble(oneDataArray[1]);
                            break;
                        case "\"v\"":
                            // "成交量"
                            historyData.v = Convert.ToDouble(oneDataArray[1]);
                            break;
                    }
                }
                tempHistoryDataList.Add(historyData);
            }
            HistoryData[] historyDataArray = tempHistoryDataList.ToArray();
            return historyDataArray;
        }
        /*
         * 函式 parseOneHistoryDataString 負責將檔案中讀出的一條歷史資料
         * 轉換成 HistoryData 結構。
         */
        private HistoryData parseOneHistoryDataString(String oneHistoryDataString)
        {
            string[] stringSeparators = new string[] { " " };
            string[] oneHistoryDataStringArray = oneHistoryDataString.Split(stringSeparators,
                StringSplitOptions.RemoveEmptyEntries);
            HistoryData oneHistoryData = new HistoryData();
            oneHistoryData.t = oneHistoryDataStringArray[0];
            oneHistoryData.o = Convert.ToDouble(oneHistoryDataStringArray[1]);
            oneHistoryData.h = Convert.ToDouble(oneHistoryDataStringArray[2]);
            oneHistoryData.l = Convert.ToDouble(oneHistoryDataStringArray[3]);
            oneHistoryData.c = Convert.ToDouble(oneHistoryDataStringArray[4]);
            oneHistoryData.v = Convert.ToDouble(oneHistoryDataStringArray[5]);
            return oneHistoryData;
        }
        /*
         * 函式 getOldHistoryData 用來從歷史資料檔案中取出舊的歷史資料。歷史
         * 資料檔案中的格式如下：
         *      2018/01/16 10955.81 10986.11 10920.5 10986.11 131018
         *      2018/01/17 10976.13 11030.22 10942.95 11004.8 147299
         *      2018/01/18 11048.06 11122.82 11048.06 11071.57 149351
         *      2018/01/19 11123.58 11150.85 11095.85 11150.85 133881 
         */
        private HistoryData[] getOldHistoryData(String filename)
        {
            List<HistoryData> tempHistoryDataList = new List<HistoryData>();
            if (fileHelper.Exists(filename))
            {
                String oldHistoryDataString = fileHelper.ReadText(filename);
                string[] stringSeparators = new string[] { "\n" };
                string[] oldHistoryDataStringArray = oldHistoryDataString.Split(stringSeparators,
                    StringSplitOptions.RemoveEmptyEntries);
                /* oldHistoryDataStringArray.Length - 2 的原因是
                 * 每週資料及每月資料在每天下載時會以當天日期為準，
                 * 例如當天是 2018/01/22 則下載的資料日期即為
                 * "2018/01/22" 寫入到檔案中，就會造成不是當月最後
                 * 一天的日期，只要每次讀出舊資料時，捨去最後二筆，
                 * 然後用網路更新的資料取代即可。
                 */
                for (int i = 0; i < oldHistoryDataStringArray.Length - 2; i++)
                {
                    String oneOldHistoryDataString = oldHistoryDataStringArray[i];
                    HistoryData oneOldHistoryData = parseOneHistoryDataString(oneOldHistoryDataString);
                    tempHistoryDataList.Add(oneOldHistoryData);
                }
            }
            HistoryData[] historyDataArray = tempHistoryDataList.ToArray();
            return historyDataArray;
        }
        /*
         * 函式 getRealOldHistoryData 用來從歷史資料檔案中取出舊的歷史資料。歷史
         * 資料檔案中的格式如下：
         *      2018/01/16 10955.81 10986.11 10920.5 10986.11 131018
         *      2018/01/17 10976.13 11030.22 10942.95 11004.8 147299
         *      2018/01/18 11048.06 11122.82 11048.06 11071.57 149351
         *      2018/01/19 11123.58 11150.85 11095.85 11150.85 133881 
         */
        public HistoryData[] getRealOldHistoryData(String historyType)
        {
            String filename = null;
            if (historyType == "d")
            {
                filename = "twStock/day.dat";
                if (dayRealHistoryData != null)
                {
                    return dayRealHistoryData;
                }
            }
            else if (historyType == "w")
            {
                filename = "twStock/week.dat";
                if (weeRealkHistoryData != null)
                {
                    return weeRealkHistoryData;
                }
            }
            else if (historyType == "m")
            {
                filename = "twStock/month.dat";
                if (monthRealHistoryData != null)
                {
                    return monthRealHistoryData;
                }
            }

            List<HistoryData> tempHistoryDataList = new List<HistoryData>();
            if (fileHelper.Exists(filename))
            {
                String oldHistoryDataString = fileHelper.ReadText(filename);
                string[] stringSeparators = new string[] { "\n" };
                string[] oldHistoryDataStringArray = oldHistoryDataString.Split(stringSeparators,
                    StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < oldHistoryDataStringArray.Length; i++)
                {
                    String oneOldHistoryDataString = oldHistoryDataStringArray[i];
                    HistoryData oneOldHistoryData = parseOneHistoryDataString(oneOldHistoryDataString);
                    tempHistoryDataList.Add(oneOldHistoryData);
                }
            }
            HistoryData[] historyDataArray = tempHistoryDataList.ToArray();
            if (historyType == "d")
            {
                dayRealHistoryData = historyDataArray;
            }
            else if (historyType == "w")
            {
                weeRealkHistoryData = historyDataArray;
            }
            else if (historyType == "m")
            {
                monthRealHistoryData = historyDataArray;
            }
            return historyDataArray;
        }
        /*
         * 函式 combineHistoryData 將檔案讀出的舊歷史資料和網路上下載的新
         * 歷史資料結合成一個陣列，並傳回該陣列。
         */
        private HistoryData[] combineHistoryData(
            HistoryData[] newHistoryDataArray, HistoryData[] oldHistoryDataArray)
        {
            List<HistoryData> tempHistoryDataList = new List<HistoryData>();
            if (oldHistoryDataArray.Length == 0)
            {   // 沒有舊資料，將新資料放入暫時串列中
                for (int i = 0; i < newHistoryDataArray.Length; i++)
                {
                    tempHistoryDataList.Add(newHistoryDataArray[i]);
                }
            }
            else
            {   // 有舊資料，先將舊資料放入暫時串列中，再放新資料
                for (int i = 0; i < oldHistoryDataArray.Length; i++)
                {   // 將舊資料放入暫時串列中
                    tempHistoryDataList.Add(oldHistoryDataArray[i]);
                }
                String lastTime = oldHistoryDataArray[oldHistoryDataArray.Length - 1].t;
                for (int i = 0; i < newHistoryDataArray.Length; i++)
                {   // 將新資料日期和資料的最後日期 (lastTime) 比較
                    if (newHistoryDataArray[i].t == lastTime)
                    {   // 新資料的時間和舊資料的最後時間相同，更新該時間的資料
                        tempHistoryDataList[tempHistoryDataList.Count() - 1] =
                            newHistoryDataArray[i];
                    }
                    else if (String.Compare(newHistoryDataArray[i].t, lastTime) > 0)
                    {   // 新資料的時間大於舊資料的最後時間，新增資料到暫時串列中
                        tempHistoryDataList.Add(newHistoryDataArray[i]);
                    }
                }
            }
            HistoryData[] historyDataArray = tempHistoryDataList.ToArray();
            return historyDataArray;
        }
        /*
         * 函式 writeHistoryDataFile 將歷史資料 historyData 
         * 寫入到 filename 檔案中。
         */
        private void writeHistoryDataFile(String filename,
            HistoryData[] historyData)
        {
            String historyDataString = "";
            for (int i = 0; i < historyData.Length; i++)
            {
                historyDataString = historyDataString +
                  historyData[i].t + " " +
                  historyData[i].o + " " +
                  historyData[i].h + " " +
                  historyData[i].l + " " +
                  historyData[i].c + " " +
                  historyData[i].v + " " +
                  "\n";
            }
            fileHelper.WriteText(filename, historyDataString);
        }
        /*
         * 函式 timeToString 用來將 C# 的 DateTime 日期轉換為
         * yyyy/mm/dd 的格式。
         */
        private String timeToString(DateTime time)
        {
            int year = time.Year;
            int month = time.Month;
            int date = time.Day;
            String timeString = "";
            timeString = timeString + year.ToString() + "/";
            if (month >= 10)
            {
                timeString = timeString + month.ToString() + "/";
            }
            else
            {
                timeString = timeString + "0" + month.ToString() + "/";
            }
            if (date >= 10)
            {
                timeString = timeString + date.ToString();
            }
            else
            {
                timeString = timeString + "0" + date.ToString();
            }
            return timeString;
        }
        /*
         * 函式 checkWeekHistoryData 
         * 用來檢查每週歷史資料的時間是否為星期一，如果不是則修正為星期一。
           由於從網路上拿到的每週歷史資料，資料的時間有時會不對，
           例如應該每筆資料時間都應該是星期一，但因為某個星期一
           沒有開盤，結果該週時間就變成上個星期五或星期二，所以
           要對每週歷史資料做檢查。
           檢查的重點是，oldHistoryDataObject 陣列中每一筆記錄的時
           間都應該只相差一週，若有相隔不是 7 的倍數，就在 console
           中顯示訊息提示使用者，並跳出程式。
           使用者可以手動開啟 week.dat 更改其內容。
           在此 Date.DateDiff 似乎會算錯，多 1 天，可能是因為
           最後一個參數給 0 所造成，但是因為不了解該參數真正
           的用法，所以把 diff - 1 當成是二週相差的週數。
           找到的錯誤如果用手更正，不太可能，因為有 9xx 個檔案
           要去更正，可以在下面迴圈中，先查出有什麼錯誤，再用手
           動重寫程式去更正時間，存檔後，舊的資料時間就不會再
           出錯，即可把手動寫的更正程式去除。
         */
        private void checkWeekHistoryData()
        {
            /*
                以下迴圈檢查每週歷史資料的時間是否為星期一，如果不是
                則修正為星期一。
            */
            for (int i = 0; i < weekHistoryData.Length; i++)
            {
                DateTime time = Convert.ToDateTime(weekHistoryData[i].t);
                if (time.DayOfWeek == 0)
                {
                    new MessageWriter().showMessage("checkWeekHistoryData Error, sunday?!");
                    new CloseApplication().closeApplication();
                }
                else if ((int)time.DayOfWeek != 5)
                {
                    int diff = 5 - (int)time.DayOfWeek;
                    time = time.AddDays(diff);
                    String timeString = timeToString(time);
                    weekHistoryData[i].t = timeString;
                }
            }
            /*
                以下迴圈檢查每週歷史資料的時間是否間隔為7天，如果不是
                則顯示警告訊息。
            */
            for (int i = 0; i < weekHistoryData.Length; i++)
            {

            }
        }
        /*
         * 函式 createMonthHistoryDatabase 用來建立大盤的每月指數歷史資料。
         */
        private void createMonthHistoryDatabase()
        {
            httpHelper.getHttpSource(
                "https://tw.quote.finance.yahoo.net/quote/q?type=ta&perd=m&mkt=10%20&sym=%23001",
                "utf-8",
                (String httpSource) =>  // getHttpSource 回呼函式
                {
                    HistoryData[] newHistoryDataArray = httpSourceToHistoryData(httpSource);
                    HistoryData[] oldHistoryDataArray = getOldHistoryData("twStock/month.dat");
                    HistoryData[] combinedHistoryDataArray =
                        combineHistoryData(newHistoryDataArray, oldHistoryDataArray);
                    monthHistoryData = combinedHistoryDataArray;
                    writeHistoryDataFile("twStock/month.dat", monthHistoryData);
                    HistoryData[] historyDataArray = monthHistoryData;
                    stockDatabaseString = "";
                    for (int i = 0; i < historyDataArray.Length; i++)
                    {
                        stockDatabaseString = stockDatabaseString + "時間 ： " + historyDataArray[i].t + "\r\n";
                        stockDatabaseString = stockDatabaseString + "開盤價 ： " + historyDataArray[i].o + "\r\n";
                        stockDatabaseString = stockDatabaseString + "收盤價 ： " + historyDataArray[i].c + "\r\n";
                        stockDatabaseString = stockDatabaseString + "最高價 ： " + historyDataArray[i].h + "\r\n";
                        stockDatabaseString = stockDatabaseString + "最低價 ： " + historyDataArray[i].l + "\r\n";
                        stockDatabaseString = stockDatabaseString + "成交量 ： " + historyDataArray[i].v + "\r\n";
                    }
                    createStockDatabaseCallback();
                }
            );
        }
        /*
         * 函式 createWeekHistoryDatabase 用來建立大盤的每日週指數歷史資料。
         */
        private void createWeekHistoryDatabase()
        {
            httpHelper.getHttpSource(
                "https://tw.quote.finance.yahoo.net/quote/q?type=ta&perd=w&mkt=10%20&sym=%23001",
                "utf-8",
                (String httpSource) =>  // getHttpSource 回呼函式
                {
                    HistoryData[] newHistoryDataArray = httpSourceToHistoryData(httpSource);
                    weekHistoryData = newHistoryDataArray;
                    checkWeekHistoryData();
                    HistoryData[] oldHistoryDataArray = getOldHistoryData("twStock/week.dat");
                    HistoryData[] combinedHistoryDataArray =
                        combineHistoryData(newHistoryDataArray, oldHistoryDataArray);
                    weekHistoryData = combinedHistoryDataArray;
                    writeHistoryDataFile("twStock/week.dat", weekHistoryData);
                    HistoryData[] historyDataArray = weekHistoryData;
                    stockDatabaseString = "";
                    for (int i = 0; i < historyDataArray.Length; i++)
                    {
                        stockDatabaseString = stockDatabaseString + "時間 ： " + historyDataArray[i].t + "\r\n";
                        stockDatabaseString = stockDatabaseString + "開盤價 ： " + historyDataArray[i].o + "\r\n";
                        stockDatabaseString = stockDatabaseString + "收盤價 ： " + historyDataArray[i].c + "\r\n";
                        stockDatabaseString = stockDatabaseString + "最高價 ： " + historyDataArray[i].h + "\r\n";
                        stockDatabaseString = stockDatabaseString + "最低價 ： " + historyDataArray[i].l + "\r\n";
                        stockDatabaseString = stockDatabaseString + "成交量 ： " + historyDataArray[i].v + "\r\n";
                    }
                    createMonthHistoryDatabase();
                }
            );
        }
        /*
         * 函式 createDayHistoryDatabase 用來建立大盤的每日指數歷史資料。
         */
        private void createDayHistoryDatabase()
        {
            httpHelper.getHttpSource(
                "https://tw.quote.finance.yahoo.net/quote/q?type=ta&perd=d&mkt=10%20&sym=%23001",
                "utf-8",
                (String httpSource) =>  // getHttpSource 回呼函式
                {
                    HistoryData[] newHistoryDataArray = httpSourceToHistoryData(httpSource);
                    HistoryData[] oldHistoryDataArray = getOldHistoryData("twStock/day.dat");
                    HistoryData[] combinedHistoryDataArray =
                        combineHistoryData(newHistoryDataArray, oldHistoryDataArray);
                    dayHistoryData = combinedHistoryDataArray;
                    writeHistoryDataFile("twStock/day.dat", dayHistoryData);
                    HistoryData[] historyDataArray = dayHistoryData;
                    stockDatabaseString = "";
                    for (int i = 0; i < historyDataArray.Length; i++)
                    {
                        stockDatabaseString = stockDatabaseString + "時間 ： " + historyDataArray[i].t + "\r\n";
                        stockDatabaseString = stockDatabaseString + "開盤價 ： " + historyDataArray[i].o + "\r\n";
                        stockDatabaseString = stockDatabaseString + "收盤價 ： " + historyDataArray[i].c + "\r\n";
                        stockDatabaseString = stockDatabaseString + "最高價 ： " + historyDataArray[i].h + "\r\n";
                        stockDatabaseString = stockDatabaseString + "最低價 ： " + historyDataArray[i].l + "\r\n";
                        stockDatabaseString = stockDatabaseString + "成交量 ： " + historyDataArray[i].v + "\r\n";
                    }

                    createWeekHistoryDatabase();
                }
            );
        }
        /* 以下為建立各種資料庫相關程式 */
        CreateAllCompaniesDatabaseCallback createAllCompaniesDatabaseCallback;
        bool createAllDatabase = false;
        /*
         *  函式 createCompanyDevidendDatabase 用來建立各公司每年股利資料庫
         */
        private void createCompanyDevidendDatabase(int index)
        {
            int totalCompanies = companies.Length;
            // totalCompanies = 79;
            if (index < totalCompanies)
            {
                new MessageWriter().showMessage("index = " + index.ToString());
                Company company = companies[index];
                company.createYearDividendDatabase(
                    () =>
                    {
                        index++;
                        createCompanyDevidendDatabase(index);
                    }
                 );
            }
            else
            {
                createAllCompaniesDatabaseCallback();
            }
        }
        /*
         *  函式 createCompanyMonthEarningDatabase 用來建立各公司每月營收資料庫
         */
        private void createCompanyMonthEarningDatabase(int index)
        {
            int totalCompanies = companies.Length;
            // totalCompanies = 79;
            if (index < totalCompanies)
            {
                new MessageWriter().showMessage("index = " + index.ToString());
                Company company = companies[index];
                company.createMonthEarningDatabase(
                    () =>
                    {
                        index++;
                        createCompanyMonthEarningDatabase(index);
                    }
                 );
            }
            else
            {
                if (createAllDatabase)
                {
                    createCompanyDevidendDatabase(0);
                }
                else
                {
                    createAllCompaniesDatabaseCallback();
                }
            }
        }
        /*
         *  函式 createCompanyInfomationDatabase 用來建立各公司基本資料庫
         */
        private void createCompanyInfomationDatabase(int index)
        {
            int totalCompanies = companies.Length;
            // totalCompanies = 79;
            if (index < totalCompanies)
            {
                new MessageWriter().showMessage("index = " + index.ToString());
                Company company = companies[index];
                company.createInfomationDatabase(
                    () =>
                    {
                        index++;
                        createCompanyInfomationDatabase(index);
                    }
                 );
            }
            else
            {
                if (createAllDatabase)
                {
                    createCompanyMonthEarningDatabase(0);
                }
                else
                {
                    createAllCompaniesDatabaseCallback();
                }
            }
        }
        /*
         *  函式 createCompanyHistoryDatabase 用來建立各公司歷史資料
         */
        private void createCompanyHistoryDatabase(int index)
        {
            int totalCompanies = companies.Length;
            // totalCompanies = 79;
            if (index < totalCompanies)
            {
                new MessageWriter().showMessage(
                    "index = " + index.ToString());
                Company company = companies[index];
                company.createHistoryDatabase(
                    () =>
                    {
                        index++;
                        createCompanyHistoryDatabase(index);
                    }
                 );
            }
            else
            {
                if (createAllDatabase)
                {
                    createCompanyInfomationDatabase(0);
                }
                else
                {
                    createAllCompaniesDatabaseCallback();
                }
            }
        }
        /*
         * createAllCompaniesDatabase 方法用以建立所有公司的各別資料庫檔案，建立完成後呼叫 callback 函式。
         */
        public void createAllCompaniesDatabase(int startIndex, CreateAllCompaniesDatabaseCallback callback)
        {
            createAllCompaniesDatabaseCallback = callback;
            createAllDatabase = true;
            createCompanyHistoryDatabase(startIndex);
        }
        /*
         *  函式 createAllCompanyHistoryDatabase 用來啟始各公司歷史資料庫的建立。
         */
        public void createAllCompanyHistoryDatabase(int startIndex, CreateAllCompaniesDatabaseCallback callback)
        {
            createAllCompaniesDatabaseCallback = callback;
            createAllDatabase = false;
            createCompanyHistoryDatabase(startIndex);
        }
        /*
         *  函式 createAllCompanyHistoryDatabase 用來啟始各公司歷史資料庫的建立。
         */
        public void createAllCompanyInformationDatabase(int startIndex, CreateAllCompaniesDatabaseCallback callback)
        {
            createAllCompaniesDatabaseCallback = callback;
            createAllDatabase = false;
            createCompanyInfomationDatabase(startIndex);
        }
        /*
         *  函式 createAllCompanyMonthEarningDatabase 用來啟始各公司歷史資料庫的建立。
         */
        public void createAllCompanyMonthEarningDatabase(int startIndex, CreateAllCompaniesDatabaseCallback callback)
        {
            createAllCompaniesDatabaseCallback = callback;
            createAllDatabase = false;
            createCompanyMonthEarningDatabase(startIndex);
        }
        /*
         *  函式 createAllCompanyDevidendDatabase 用來啟始各公司歷史資料庫的建立。
         */
        public void createAllCompanyDevidendDatabase(int startIndex, CreateAllCompaniesDatabaseCallback callback)
        {
            createAllCompaniesDatabaseCallback = callback;
            createAllDatabase = false;
            createCompanyDevidendDatabase(startIndex);
        }
        /*
         *  函式 getDayHistoryData 用來取得台股大盤的每日歷史資料陣列
         */
        public HistoryData[] getDayHistoryData()
        {
            dayHistoryData = getRealOldHistoryData("d");
            return dayHistoryData;
        }
        /*
         *  函式 getWeekHistoryData 用來取得台股大盤的每日週歷史資料陣列
         */
        public HistoryData[] getWeekHistoryData()
        {
            weekHistoryData = getRealOldHistoryData("w");
            return weekHistoryData;
        }
        /*
         *  函式 getMonthHistoryData 用來取得台股大盤的每月歷史資料陣列
         */
        public HistoryData[] getMonthHistoryData()
        {
            monthHistoryData = getRealOldHistoryData("m");
            return monthHistoryData;
        }
        /*
         * 函式 getCompany 用來取得公司代號是 id 的公司資料物件
         */
        public Company getCompany(String idParam)
        {
            int index = Array.FindIndex<Company>(
                companies,
                (Company oneCompany) =>
                {
                    if (oneCompany.id == idParam)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            );
            if (index >= 0)
            {
                return companies[index];
            }
            else
            {
                return null;
            }
        }
        /*
         * 函式 sortCompanyByCapital 用來將各公司按資本額做排名，
         * 實際上的資本額是和股價有關，資料庫中的資本額是股票總張數。
         */
        public class Comparer : IComparer<Company>
        {
            public int Compare(Company company1, Company company2)
            {
                if (company2.capital > company1.capital)
                {
                    return 1;
                }
                else if (company2.capital < company1.capital)
                {
                    return -1;
                }
                else
                    return 0;
            }
        }
        public void sortCompanyByCapital()
        {
            for (var i = 0; i < companies.Length; i++)
            {
                companies[i].getCapital();
            }
            IComparer<Company> comparer = new Comparer();
            Array.Sort<Company>(
                companies,
                comparer
            );
        }
        public HistoryData[] getHistoryData80(HistoryData[] historyData)
        {
            List<HistoryData> historyData80List = new List<HistoryData>();
            int historyDataLength = historyData.Length;
            for (int i = 0; i < 79; i++)
            {
                var index = historyDataLength - 79 + i;
                if (index >= 0)
                {
                    historyData80List.Add(historyData[index]);
                }
                else
                {
                    continue;
                }
            }
            HistoryData[] historyData80 = historyData80List.ToArray();
            return historyData80;
        }
        public KDJ[] kValue(HistoryData[] historyData80)
        {
            Indicator indicator = new Indicator(historyData80);
            Double[] rsvArray = indicator.calcRsvArray(9);
            KDJ[] kdjArray = indicator.calcKDJArray(rsvArray);
            return kdjArray;
        }
        private String getTrendUpOrDown(HistoryData[] historyData)
        {
            String returnText = "";
            if (historyData.Length > 2)
            {
                /*
                 * firstTrend 表示最後二天的趨勢
                 * 若其值為 true 表示趨劫向上， false 表示趨勢向下(含持平)。 
                 * totalDay 由最後二天向前搜尋，查看與前相同的天數。
                 */
                Boolean firstTrend;
                int totalDay = 1;
                int lastIndex = historyData.Length - 1;
                if (historyData[lastIndex].c < historyData[lastIndex - 1].c)
                {
                    firstTrend = false;
                }
                else
                {
                    firstTrend = true;
                }
                for (int i = lastIndex - 2; i >= 0; i--)
                {
                    Boolean trend;
                    if (historyData[i + 1].c < historyData[i].c)
                    {
                        trend = false;
                    }
                    else
                    {
                        trend = true;
                    }
                    if (trend == firstTrend)
                    {
                        totalDay++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (firstTrend)
                {
                    returnText = returnText +
                        "\t指數趨勢向上 " + totalDay + " 天\r\n"
                        ;
                }
                else
                {
                    returnText = returnText +
                        "\t指數趨勢向下(含持平) " + totalDay + " 天\r\n"
                        ;
                }
            }
            return returnText;
        }
        private String getKValueTrendUpOrDown(KDJ[] KDJArray)
        {
            String returnText = "";
            if (KDJArray.Length > 2)
            {
                /*
                 * firstTrend 表示最後二天的趨勢
                 * 若其值為 true 表示趨劫向上， false 表示趨勢向下(含持平)。 
                 * totalDay 由最後二天向前搜尋，查看與前相同的天數。
                 */
                Boolean firstTrend;
                int totalDay = 1;
                int lastIndex = KDJArray.Length - 1;
                if (KDJArray[lastIndex].K < KDJArray[lastIndex - 1].K)
                {
                    firstTrend = false;
                }
                else
                {
                    firstTrend = true;
                }
                for (int i = lastIndex - 2; i >= 0; i--)
                {
                    Boolean trend;
                    if (KDJArray[i + 1].K < KDJArray[i].K)
                    {
                        trend = false;
                    }
                    else
                    {
                        trend = true;
                    }
                    if (trend == firstTrend)
                    {
                        totalDay++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (firstTrend)
                {
                    returnText = returnText +
                        "K 值趨勢向上 " + totalDay + " 筆"
                        ;
                }
                else
                {
                    returnText = returnText +
                        "K 值趨勢向下(含持平) " + totalDay + " 筆"
                        ;
                }
            }
            return returnText;
        }
        private Double highestIndex = 0;
        private Double lowestIndex = 0;
        private String getPrevHighAndLow()
        {
            HistoryData[] monthHistoryData = getMonthHistoryData();
            Double highestPrice = 0;
            Double lowestPrice = Double.MaxValue;
            int lastIndex = monthHistoryData.Length - 1;
            for (int i = lastIndex; i >= 0; i--)
            {
                HistoryData historyData=monthHistoryData[i];
                String time = historyData.t;
                int Year = Convert.ToInt32(time.Substring(0, 4));
                int Month = Convert.ToInt32(time.Substring(5, 2));
                int Day = Convert.ToInt32(time.Substring(8, 2));
                var dataDate = new DateTime(Year, Month, Day);
                var diff = (DateTime.Now - dataDate).TotalDays;
                if (diff > (6 * 365))
                {   /* 比較 6 年中的歷史資料 */
                    break;
                }
                if (historyData.h > highestPrice)
                {
                    highestPrice = historyData.h;
                }
                if (historyData.l < lowestPrice)
                {
                    lowestPrice = historyData.l;
                }
            }
            highestIndex = highestPrice;
            lowestIndex = lowestPrice;
            return "\t六年內指數最高是 " + highestPrice + " ，最低是 " + lowestPrice + "\r\n";
        }
        private DateTime timeInformation;
        private String information = null;
        public String getInformation()
        {
            if (timeInformation != null)
            {
                var diff = DateTime.Now - timeInformation;
                var diffDays = diff.TotalDays;
                if ((diffDays < 1)&&(information!=null))
                {
                    return information;
                }

            }
            String returnText = "";
            HistoryData[] historyData80 = getHistoryData80(getDayHistoryData());
            HistoryData[] dayHistoryData = historyData80;
            if (historyData80.Length == 0)
            {
                return "資料庫有問題，沒有大盤指數資料!?\r\n";
            }
            KDJ[] KDJArray;
            KDJArray = kValue(historyData80);
            String dayKValueTrend = "\t日 " + getKValueTrendUpOrDown(KDJArray) + "\r\n";
            Double kValueDay = KDJArray[KDJArray.Length - 1].K;
            historyData80 = getHistoryData80(getWeekHistoryData());
            KDJArray = kValue(historyData80);
            String weekKValueTrend = "\t週 " + getKValueTrendUpOrDown(KDJArray) + "\r\n";
            Double kValueWeek = KDJArray[KDJArray.Length - 1].K;
            historyData80 = getHistoryData80(getMonthHistoryData());
            KDJArray = kValue(historyData80);
            String monthKValueTrend = "\t月 " + getKValueTrendUpOrDown(KDJArray) + "\r\n";
            Double kValueMonth = KDJArray[KDJArray.Length - 1].K;
            returnText = returnText + "大盤：\r\n\t日k (" + kValueDay.ToString("f2") +
                ") ，週k (" + kValueWeek.ToString("f2") + ") ，月k (" + kValueMonth.ToString("f2") + ") \r\n";
            returnText = returnText + getTrendUpOrDown(dayHistoryData);
            returnText = returnText + dayKValueTrend;
            returnText = returnText + weekKValueTrend;
            returnText = returnText + monthKValueTrend;
            returnText = returnText + getPrevHighAndLow();
            Double currentIndex = historyData80[historyData80.Length - 1].c;
            Double indexDiff = highestIndex - lowestIndex;
            returnText = returnText + "\t目前加權指數： " + currentIndex +
                " (比前高少 " + ((highestIndex - currentIndex)*100 / indexDiff).ToString("f2") +"%)"+
                " (比前低多 " + ((currentIndex-lowestIndex) * 100 / indexDiff).ToString("f2") + "%)" +
                "\r\n";
            returnText = returnText + "\r\n";
            information = returnText;
            timeInformation = DateTime.Now;
            return information;
        }
    }
}
