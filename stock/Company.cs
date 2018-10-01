/*
  Company 類別：
    建構式：
      new Company(id, name, category)
      id 是公司的代號
      name 是公司的名稱
      category 是公司所屬類別
    方法成員：
      01. createHistoryDatabase(id, name, category, callback)
          根據傳入的 id 為該公司建立歷史資料庫，包括
            每日歷史資料庫       database\company\id\day.dat
            每週歷史資料庫       database\company\id\week.dat
            每月歷史資料庫       database\company\id\month.dat
      02. checkDatabase(id, callback)
          根據傳入的 id 檢查該公司的各種資料庫檔案是否存在。
      03. createInfomationDatabase(id, name, category, callback)
          根據傳入的 id 為該公司建立基本資訊資料庫，包括
            基本資訊資料庫       database\company\id\baseInformation.dat
            每季獲利資料庫       database\company\id\marginInformation.dat
            每季EPS資料庫        database\company\id\seasonEPS.dat
            每年EPS資料庫        database\company\id\yearEPS.dat
      04. createMonthEarningDatabase(id, name, category, callback)
          根據傳入的 id 為該公司建立每月獲利資訊資料庫，包括
            每月獲利資料庫       database\company\id\earning.dat
      05. createYearDividendDatabase(id, name, category, callback)
          根據傳入的 id 為該公司建立歷年股利資料庫，包括
              歷年股利資料庫       database\company\id\dividend.dat 
          該資料庫中每一行為一年的股利記錄，內容為：
              年度 現金股利 盈餘配股 公積配股 股票股利 合計
      06. getHistoryDataArray(historyType)
          此方法用來取得此公司的歷史資料陣列，historyType 為 "d" 表示
          是每日歷史資料，"w" 表示是每週歷史資料，"m" 表示是每月歷史資料。
      07. getCapital()
          此方法用來取得公司的資本額，資本額將從各公司的 baseInformation.dat
          檔案中讀出，並設定給 company.capital 資料成員。
          請注意，此方法沒有傳回值。
      08. getSeasonEPS()
          此方法用來取得公司的每季EPS，每季EPS將從各公司的 seasonEPS.dat
          檔案中讀出，並設定給 company.seasonEPS 資料成員。
          請注意，此方法沒有傳回值。
      09. getYearEPS()
          此方法用來取得公司的每年EPS，每年EPS將從各公司的 yearEPS.dat
          檔案中讀出，並設定給 company.yearEPS 資料成員。
          請注意，此方法沒有傳回值。
      10. getDividend()
          此方法用來取得公司的每年股利，每年股利將從各公司的 dividend.dat
          檔案中讀出，並設定給 company.dividend 資料成員。
          請注意，此方法沒有傳回值。
      11. getMarginInformation()
          此方法用來取得公司的每年獲利能力的資料，
      12. getInformation()
          此方法用來取得股票的簡單描述資訊。
      13. checkDatabase()
          此方法用來檢查資料庫的完整性，包含歷史資料庫、基本資料庫、營收資料庫、股利資料庫。
          若檢查失敗，則標示為不做分析及篩選。
      14. getEarning()
          此方法用來取得過去幾年每月營收的資訊，以陣列形式傳回，每個元素是 EarningInformation 型態的值，
          代表某一年的每月營收值。EarningInformation 中成員解釋如下：
            year            哪一年度
            earningString   檔案中讀出的各月營收值(字串)，其中數值格式為 xxx,xxx,xxx 含有逗號
            earning         各月營收值(Double)，由上面字串轉換而來
            increasePercentCompareToLastYear
                            和前一年比較的增減百分比
      15. getMarketMaker()
          此方法用來取得過去 10 天主力的量能，以陣列形式傳回，每個元素是 MarketMakerInformation 型態的值。
          MarketMakerInformation 中成員解釋如下：
            date            日期
            fValue;         外資持股(張)
            pValue;         投信持股(張)
            iValue;         自營商持股(張)
            value           主力籌碼大小
            increase        和前一交易日比較的增減值(買賣超值)
            increasePercent 和前一交易日比較的增減百分比
       16.  public KDJ[] kValue(HistoryData[] historyData80)
            此方法用來將傳入的 historyData80 計算其 KDJ 值
       17.  public HistoryData[] getHistoryData80(HistoryData[] historyData)
            getHistoryData80 由傳入的歷史資料陣列中取最後 80 筆資料傳回。
            如果不足 80 筆則全數傳回。
       18.  findWaveDataList 
            用來找尋波段上漲或下跌的幅度。
        
  注意事項：
    2017/03/28 此類別中的方法，一開始設計時，要傳入 id, name, category
               等參數，後來變更 Company 建構式以接受這些參數，因此
               此類別的方法應該不再需要傳入這些參數，但是去掉這些參數
               會造成其它程式很多地方要修正，所以暫時不改變這些方法的
               參數。
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stock
{
    public delegate void CreateHistoryDatabaseCallback();
    public delegate void CreateInformationDatabaseCallback();
    public delegate void CreateMonthEarningDatabaseCallback();
    public delegate void CreateYearDividendDatabaseCallback();
    class MarketMakerInformation
    {
        public DateTime date;
        public Double fValue;  // 外資持股(張)
        public Double pValue;  // 投信持股(張)
        public Double iValue;  // 自營商持股(張)
        public Double value;
        public Double increase;
        public Double increasePercent;
    }
    class EarningInformation
    {
        public int year;
        public String[] earningString;
        public Double[] earning;
        public Double[] increasePercentCompareToLastYear;
    }
    class CompanyInformation
    {
        public String presidentName;
        public String ceoName;
        public Double capital;
        public String companyURL;
        public String marginYear;
        public Double grossMargin;
        public Double operatingProfitMargin;
        public Double earningBeforeTaxMargin;
        public Double ROA;
        public Double ROE;
        public String s1Name;
        public Double s1EPS;
        public String s2Name;
        public Double s2EPS;
        public String s3Name;
        public Double s3EPS;
        public String s4Name;
        public Double s4EPS;
        public String y1Name;
        public Double y1EPS;
        public String y2Name;
        public Double y2EPS;
        public String y3Name;
        public Double y3EPS;
        public String y4Name;
        public Double y4EPS;
        public Double bookValuePerShare;
    }
    class Company
    {
        public String id;
        public String name;
        public string category;
        public HistoryData[] dayHistoryData;
        public HistoryData[] weekHistoryData;
        public HistoryData[] monthHistoryData;
        public Double capital;
        public Double[] seasonEPS;
        public Double[] yearEPS;
        public Double[] dividend;
        List<HistoryData> dayHistoryDataList;
        List<HistoryData> weekHistoryDataList;
        List<HistoryData> monthHistoryDataList;
        public HistoryData[] dayRealHistoryData;
        public HistoryData[] weeRealkHistoryData;
        public HistoryData[] monthRealHistoryData;
        public List<int> scoreIndexList;
        public int[] scoreIndexArray = null;
        HttpHelper httpHelper = new HttpHelper();
        FileHelper fileHelper = new FileHelper();
        int tickCount = 600;
        // int tickCount = 1400;
        public int score;
        public String printText;
        public String candidateMatchString;
        public int candidateMatchCount;
        public Boolean matchE;
        public Boolean matchF;
        public Boolean matchG;
        public Boolean passCheckDatabase;
        public List<LipHipData> lipHipDataList;
        public List<WaveData> waveDataList;
        public WaveStatisticInformation waveStatisticInformation;

        /*
         * Company 建構式
         */
        public Company(String idParam, String nameParam, String categoryParam)
        {
            id = idParam;
            name = nameParam;
            category = categoryParam;
            if (category.Substring(category.Length - 1, 1) == "\r")
            {
                category = category.Substring(0, category.Length - 1);
            }
            dayHistoryDataList = new List<HistoryData>();
            weekHistoryDataList = new List<HistoryData>();
            monthHistoryDataList = new List<HistoryData>();
            scoreIndexList = new List<int>();
            String companyDirectory = FileHelper.databasePath + "company/" + id;
            if (!fileHelper.DirectoryExists(companyDirectory))
            {
                fileHelper.CreateDirectory(companyDirectory);
            }
            dayRealHistoryData = null;
            weeRealkHistoryData = null;
            monthRealHistoryData = null;
            candidateMatchCount = 0;
            candidateMatchString = "";
            passCheckDatabase = true;
            lipHipDataList = null;
            waveDataList = null;
            waveStatisticInformation = null;
        }
        CreateHistoryDatabaseCallback createHistoryDatabaseCallback;
        /*
         * 函式 getRealOldHistoryData 由歷史資料檔案中讀出。
         */
        public HistoryData[] getRealOldHistoryData(String filename)
        {
            List<HistoryData> tempHistoryDataList
                = new List<HistoryData>(); ;
            if (fileHelper.Exists(filename))
            {
                String oldHistoryDataString = fileHelper.ReadText(filename);
                string[] stringSeparators = new string[] { "\n" };
                string[] oldHistoryDataStringArray = oldHistoryDataString.Split(stringSeparators,
                    StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < oldHistoryDataStringArray.Length; i++)
                {
                    String oneOldHistoryDataString = oldHistoryDataStringArray[i];
                    String[] oneOldHistoryDataStringArray =
                        oneOldHistoryDataString.Split(new string[] { " " },
                        StringSplitOptions.RemoveEmptyEntries);
                    bool longDataFormat = false;
                    HistoryData oneHistoryData = new HistoryData();
                    if (oneOldHistoryDataStringArray.Length >= 15)
                    {
                        oneHistoryData.t = oneOldHistoryDataStringArray[0];
                        oneHistoryData.o = Convert.ToDouble(oneOldHistoryDataStringArray[1]);
                        oneHistoryData.h = Convert.ToDouble(oneOldHistoryDataStringArray[2]);
                        oneHistoryData.l = Convert.ToDouble(oneOldHistoryDataStringArray[3]);
                        oneHistoryData.c = Convert.ToDouble(oneOldHistoryDataStringArray[4]);
                        oneHistoryData.v = Convert.ToDouble(oneOldHistoryDataStringArray[5]);
                        oneHistoryData.m = Convert.ToDouble(oneOldHistoryDataStringArray[6]);
                        oneHistoryData.n = Convert.ToDouble(oneOldHistoryDataStringArray[7]);
                        oneHistoryData.f = Convert.ToDouble(oneOldHistoryDataStringArray[8]);
                        oneHistoryData.p = Convert.ToDouble(oneOldHistoryDataStringArray[9]);
                        oneHistoryData.i = Convert.ToDouble(oneOldHistoryDataStringArray[10]);
                        oneHistoryData.s = Convert.ToDouble(oneOldHistoryDataStringArray[11]);
                        oneHistoryData.d = Convert.ToDouble(oneOldHistoryDataStringArray[12]);
                        oneHistoryData.u = Convert.ToDouble(oneOldHistoryDataStringArray[13]);
                        oneHistoryData.r = Convert.ToDouble(oneOldHistoryDataStringArray[14]);
                        longDataFormat = true;
                    }
                    else
                    {
                        oneHistoryData.t = oneOldHistoryDataStringArray[0];
                        oneHistoryData.o = Convert.ToDouble(oneOldHistoryDataStringArray[1]);
                        oneHistoryData.h = Convert.ToDouble(oneOldHistoryDataStringArray[2]);
                        oneHistoryData.l = Convert.ToDouble(oneOldHistoryDataStringArray[3]);
                        oneHistoryData.c = Convert.ToDouble(oneOldHistoryDataStringArray[4]);
                        oneHistoryData.v = Convert.ToDouble(oneOldHistoryDataStringArray[5]);
                        longDataFormat = false;
                    }
                    oneHistoryData.longDataFormat = longDataFormat;
                    tempHistoryDataList.Add(oneHistoryData);
                }
            }
            return tempHistoryDataList.ToArray();
        }
        /*
         * 函式 getOldHistoryData 由歷史資料檔案中讀出。
         */
        private HistoryData[] getOldHistoryData(String filename)
        {
            List<HistoryData> tempHistoryDataList
                = new List<HistoryData>(); ;
            if (fileHelper.Exists(filename))
            {
                String oldHistoryDataString = fileHelper.ReadText(filename);
                string[] stringSeparators = new string[] { "\n" };
                string[] oldHistoryDataStringArray = oldHistoryDataString.Split(stringSeparators,
                    StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < oldHistoryDataStringArray.Length - 5; i++)
                // for (int i = 0; i < oldHistoryDataStringArray.Length -1390 ; i++)
                {
                    String oneOldHistoryDataString = oldHistoryDataStringArray[i];
                    String[] oneOldHistoryDataStringArray =
                        oneOldHistoryDataString.Split(new string[] { " " },
                        StringSplitOptions.RemoveEmptyEntries);
                    // new MessageWriter().showMessage(oneOldHistoryDataStringArray.Length.ToString());
                    bool longDataFormat = false;
                    HistoryData oneHistoryData = new HistoryData();
                    if (oneOldHistoryDataStringArray.Length >= 15)
                    {
                        oneHistoryData.t = oneOldHistoryDataStringArray[0];
                        oneHistoryData.o = Convert.ToDouble(oneOldHistoryDataStringArray[1]);
                        oneHistoryData.h = Convert.ToDouble(oneOldHistoryDataStringArray[2]);
                        oneHistoryData.l = Convert.ToDouble(oneOldHistoryDataStringArray[3]);
                        oneHistoryData.c = Convert.ToDouble(oneOldHistoryDataStringArray[4]);
                        oneHistoryData.v = Convert.ToDouble(oneOldHistoryDataStringArray[5]);
                        oneHistoryData.m = Convert.ToDouble(oneOldHistoryDataStringArray[6]);
                        oneHistoryData.n = Convert.ToDouble(oneOldHistoryDataStringArray[7]);
                        oneHistoryData.f = Convert.ToDouble(oneOldHistoryDataStringArray[8]);
                        oneHistoryData.p = Convert.ToDouble(oneOldHistoryDataStringArray[9]);
                        oneHistoryData.i = Convert.ToDouble(oneOldHistoryDataStringArray[10]);
                        oneHistoryData.s = Convert.ToDouble(oneOldHistoryDataStringArray[11]);
                        oneHistoryData.d = Convert.ToDouble(oneOldHistoryDataStringArray[12]);
                        oneHistoryData.u = Convert.ToDouble(oneOldHistoryDataStringArray[13]);
                        oneHistoryData.r = Convert.ToDouble(oneOldHistoryDataStringArray[14]);
                        longDataFormat = true;
                    }
                    else
                    {
                        oneHistoryData.t = oneOldHistoryDataStringArray[0];
                        oneHistoryData.o = Convert.ToDouble(oneOldHistoryDataStringArray[1]);
                        oneHistoryData.h = Convert.ToDouble(oneOldHistoryDataStringArray[2]);
                        oneHistoryData.l = Convert.ToDouble(oneOldHistoryDataStringArray[3]);
                        oneHistoryData.c = Convert.ToDouble(oneOldHistoryDataStringArray[4]);
                        oneHistoryData.v = Convert.ToDouble(oneOldHistoryDataStringArray[5]);
                        longDataFormat = false;
                    }
                    oneHistoryData.longDataFormat = longDataFormat;
                    tempHistoryDataList.Add(oneHistoryData);
                }
            }
            return tempHistoryDataList.ToArray();
        }
        /*
         * 函式 parseHistoryDataString 用來將網路下載的歷史資料轉換成
         * HistoryData 類別的陣列。
         */
        private HistoryData[] parseHistoryDataString(String historyString)
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
            List<HistoryData> tempHistoryDataList
                = new List<HistoryData>();
            String[] historyStringSplit =
                historyString.Split(new string[] { " " },
                StringSplitOptions.RemoveEmptyEntries);
            List<String[]> dataArrayList = new List<String[]>();
            List<int> historyDataLength = new List<int>();
            for (int i = 0; i < historyStringSplit.Length; i++)
            {
                String oneDataString = historyStringSplit[i];
                String[] oneDataStringSplit =
                    oneDataString.Split(new string[] { "," },
                    StringSplitOptions.RemoveEmptyEntries);
                historyDataLength.Add(oneDataStringSplit.Length);
                dataArrayList.Add(oneDataStringSplit);
            }
            String[][] dataArray = dataArrayList.ToArray();
            /* 建立 historyDataList */
            for (int i = 0; i < historyDataLength[0]; i++)
            {
                HistoryData oneDataObject = new HistoryData();
                /*
                 * 如果下載網頁資料用 ' ' 空白分割後，超過 19 
                 * 個字串以上，則表示每個歷史記錄都可能是長格式
                 * ，但是最後一筆(當天資枓)有可能外資等籌碼資料
                 * 還未得到，可能會是短格式。
                 */
                if (historyStringSplit.Length > 19)
                {
                    oneDataObject.longDataFormat = true;
                }
                else
                {
                    oneDataObject.longDataFormat = false;
                }
                tempHistoryDataList.Add(oneDataObject);
            }

            for (int i = 0; i < tempHistoryDataList.Count(); i++)
            {
                HistoryData oneDataObject = tempHistoryDataList[i];
                oneDataObject.t = dataArray[0][i];
                oneDataObject.o = Convert.ToDouble(dataArray[1][i]);
                oneDataObject.h = Convert.ToDouble(dataArray[2][i]);
                oneDataObject.l = Convert.ToDouble(dataArray[3][i]);
                oneDataObject.c = Convert.ToDouble(dataArray[4][i]);
                oneDataObject.v = Convert.ToDouble(dataArray[5][i]);
                if ((historyDataLength.Count() > 6) && (i < historyDataLength[6]))
                {   // 第 6 種資料有 tickCount 個，才能由 dataArray 中取得
                    oneDataObject.m = Convert.ToDouble(dataArray[6][i]);
                }
                else
                {
                    oneDataObject.longDataFormat = false;
                }
                if ((historyDataLength.Count() > 7) && (i < historyDataLength[7]))
                {   // 第 7 種資料有 tickCount 個，才能由 dataArray 中取得
                    oneDataObject.n = Convert.ToDouble(dataArray[7][i]);
                }
                else
                {
                    oneDataObject.longDataFormat = false;
                }
                if ((historyDataLength.Count() > 8) && (i < historyDataLength[8]))
                {   // 第 8 種資料有 tickCount 個，才能由 dataArray 中取得
                    oneDataObject.f = Convert.ToDouble(dataArray[8][i]);
                }
                else
                {
                    oneDataObject.longDataFormat = false;
                }
                if ((historyDataLength.Count() > 9) && (i < historyDataLength[9]))
                {   // 第 9 種資料有 tickCount 個，才能由 dataArray 中取得
                    oneDataObject.p = Convert.ToDouble(dataArray[9][i]);
                }
                else
                {
                    oneDataObject.longDataFormat = false;
                }
                if ((historyDataLength.Count() > 10) && (i < historyDataLength[10]))
                {   // 第 10 種資料有 tickCount 個，才能由 dataArray 中取得
                    oneDataObject.i = Convert.ToDouble(dataArray[10][i]);
                }
                else
                {
                    oneDataObject.longDataFormat = false;
                }
                if ((historyDataLength.Count() > 14) && (i < historyDataLength[14]))
                {   // 第 14 種資料有 tickCount 個，才能由 dataArray 中取得
                    oneDataObject.s = Convert.ToDouble(dataArray[14][i]);
                }
                else
                {
                    oneDataObject.longDataFormat = false;
                }
                if ((historyDataLength.Count() > 16) && (i < historyDataLength[16]))
                {   // 第 16 種資料有 tickCount 個，才能由 dataArray 中取得
                    oneDataObject.d = Convert.ToDouble(dataArray[16][i]);
                }
                else
                {
                    oneDataObject.longDataFormat = false;
                }
                if ((historyDataLength.Count() > 17) && (i < historyDataLength[17]))
                {   // 第 17 種資料有 tickCount 個，才能由 dataArray 中取得
                    oneDataObject.u = Convert.ToDouble(dataArray[17][i]);
                }
                else
                {
                    oneDataObject.longDataFormat = false;
                }
                if ((historyDataLength.Count() > 18) && (i < historyDataLength[18]))
                {   // 第 18 種資料有 tickCount 個，才能由 dataArray 中取得
                    oneDataObject.r = Convert.ToDouble(dataArray[18][i]);
                }
                else
                {
                    oneDataObject.longDataFormat = false;
                }
            }
            return tempHistoryDataList.ToArray();
        }
        /*
         *  函式 combineOldAndNewHistoryData 將新舊歷史資料加以合併。
         */
        private HistoryData[] combineOldAndNewHistoryData(
            HistoryData[] oldHistoryData,
            HistoryData[] newHistoryData
        )
        {
            List<HistoryData> tempHistoryDataList
                = new List<HistoryData>();
            if (oldHistoryData.Length == 0)
            {   // 沒有舊資料，全部用新資料
                for (int i = 0; i < newHistoryData.Length; i++)
                {
                    HistoryData oneHistoryData = newHistoryData[i];
                    tempHistoryDataList.Add(oneHistoryData);
                }
            }
            else
            {
                String lastTime =
                    oldHistoryData[oldHistoryData.Length - 1].t;
                for (int i = 0; i < oldHistoryData.Length; i++)
                {   // 將舊資料放入暫時串列中
                    tempHistoryDataList.Add(oldHistoryData[i]);
                }
                for (int i = 0; i < newHistoryData.Length; i++)
                {
                    HistoryData oneHistoryData = newHistoryData[i];
                    if (oneHistoryData.t == lastTime)
                    {   // 新資料的時間和舊資料的最後時間相同，更新該時間的資料
                        tempHistoryDataList[tempHistoryDataList.Count() - 1] =
                            newHistoryData[i];
                    }
                    else if (String.Compare(newHistoryData[i].t, lastTime) > 0)
                    {   // 新資料的時間大於舊資料的最後時間，新增資料到暫時串列中
                        tempHistoryDataList.Add(newHistoryData[i]);
                    }
                }

            }
            return tempHistoryDataList.ToArray();
        }
        /*
         * 函式 checkHistoryDataFormat 用來檢查檔案中歷史資料格式和
         * 網路下載歷史資料格式是否相同，若有不同，則於警告視窗中提出。
         */
        private void checkHistoryDataFormat(HistoryData[] oldData, HistoryData[] newData)
        {
            if ((oldData.Length != 0) && (newData.Length != 0))
            {
                if (oldData[0].longDataFormat != newData[0].longDataFormat)
                {
                    for (int i = 0; i < newData.Length; i++)
                    {
                        for (int j = 0; j < oldData.Length; j++)
                        {
                            if (newData[i].t == oldData[j].t)
                            {   // 用新資料取代舊資料
                                oldData[j].o = newData[i].o;
                                oldData[j].h = newData[i].h;
                                oldData[j].l = newData[i].l;
                                oldData[j].c = newData[i].c;
                                oldData[j].v = newData[i].v;
                                oldData[j].m = newData[i].m;
                                oldData[j].n = newData[i].n;
                                oldData[j].f = newData[i].f;
                                oldData[j].p = newData[i].p;
                                oldData[j].i = newData[i].i;
                                oldData[j].s = newData[i].s;
                                oldData[j].d = newData[i].d;
                                oldData[j].u = newData[i].u;
                                oldData[j].r = newData[i].r;
                                break;
                            }
                        }
                    }
                    /*
                    dayHistoryData =
                        combineOldAndNewHistoryData(oldData, newData);
                     */
                    new WarningWriter().showMessage(
                        name + "(" + id + ") 資料庫格式與網頁下載資料格式不符\n"
                    );
                }
            }
        }
        /*
         * 函式 writeHistoryDataFile 將歷史資料 historyData 
         * 寫入到 filename 檔案中。
         */
        private void writeHistoryDataFile(String filename,
            HistoryData[] historyData, bool longFormat)
        {
            String historyDataString = "";
            for (int i = 0; i < historyData.Length; i++)
            {
                if (longFormat)
                {
                    historyDataString = historyDataString +
                        historyData[i].t + " " +
                        historyData[i].o + " " +
                        historyData[i].h + " " +
                        historyData[i].l + " " +
                        historyData[i].c + " " +
                        historyData[i].v + " " +
                        historyData[i].m + " " +
                        historyData[i].n + " " +
                        historyData[i].f + " " +
                        historyData[i].p + " " +
                        historyData[i].i + " " +
                        historyData[i].s + " " +
                        historyData[i].d + " " +
                        historyData[i].u + " " +
                        historyData[i].r + " " +
                        "\n";
                }
                else
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
         * 
         */
        private void createMonthHistoryDatabase()
        {
            new WarningWriter().showMessage(
                "開始更新 " + name + "(" + id + ")" + " 公司的每月歷史資料。\r\n"
                );
            // tickCount = 20;
            httpHelper.getHttpSource(
                "http://jdata.yuanta.com.tw//Z/ZC/ZCW/CZKC1.djbcd?a=" +
                id + "&b=M&c=" + tickCount,
                "utf-8",
                (String httpSource) =>  // getHttpSource 回呼函式
                {
                    try
                    {
                        HistoryData[] oldDayHistoryDataArray =
                            getOldHistoryData("company/" + id + "/month.dat");
                        HistoryData[] newDayHistoryDataArray =
                            parseHistoryDataString(httpSource);
                        monthHistoryData =
                            combineOldAndNewHistoryData(oldDayHistoryDataArray, newDayHistoryDataArray);
                        writeHistoryDataFile("company/" + id + "/month.dat", monthHistoryData, false);
                        monthRealHistoryData = monthHistoryData;
                    }
                    catch (Exception)
                    {
                    }
                    createHistoryDatabaseCallback();
                }
            );
        }
        /*
         * 函式 createWeekHistoryDatabase 用來建立每週歷史資料庫
         */
        private void createWeekHistoryDatabase()
        {
            new WarningWriter().showMessage(
                "開始更新 " + name + "(" + id + ")" + " 公司的每週歷史資料。\r\n"
                );
            // tickCount = 20;
            httpHelper.getHttpSource(
                "http://jdata.yuanta.com.tw//Z/ZC/ZCW/CZKC1.djbcd?a=" +
                id + "&b=W&c=" + tickCount,
                "utf-8",
                (String httpSource) =>  // getHttpSource 回呼函式
                {
                    try
                    {
                        HistoryData[] oldDayHistoryDataArray =
                            getOldHistoryData("company/" + id + "/week.dat");
                        HistoryData[] newDayHistoryDataArray =
                            parseHistoryDataString(httpSource);
                        weekHistoryData = newDayHistoryDataArray;
                        checkWeekHistoryData();
                        weekHistoryData =
                            combineOldAndNewHistoryData(oldDayHistoryDataArray, newDayHistoryDataArray);
                        writeHistoryDataFile("company/" + id + "/week.dat", weekHistoryData, false);
                        weeRealkHistoryData = weekHistoryData;
                    }
                    catch (Exception)
                    {
                    }
                    createMonthHistoryDatabase();
                }
            );
        }
        /*
         * 函式 createDayHistoryDatabase 用來建立每日歷史資料庫
         */
        private void createDayHistoryDatabase()
        {
            new WarningWriter().showMessage(
                "開始更新 " + name + "(" + id + ")" + " 公司的每日歷史資料。\r\n"
                );
            // tickCount = 20;
            httpHelper.getHttpSource(
                "http://jdata.yuanta.com.tw//Z/ZC/ZCW/CZKC1.djbcd?a=" +
                id + "&b=D&c=" + tickCount,
                "utf-8",
                (String httpSource) =>  // getHttpSource 回呼函式
                {
                    try
                    {
                        HistoryData[] oldDayHistoryDataArray =
                            getOldHistoryData("company/" + id + "/day.dat");
                        HistoryData[] newDayHistoryDataArray =
                            parseHistoryDataString(httpSource);
                        checkHistoryDataFormat(oldDayHistoryDataArray, newDayHistoryDataArray);
                        dayHistoryData =
                            combineOldAndNewHistoryData(oldDayHistoryDataArray, newDayHistoryDataArray);
                        writeHistoryDataFile("company/" + id + "/day.dat", dayHistoryData, true);
                        dayRealHistoryData = dayHistoryData;
                    }
                    catch (Exception)
                    {
                    }
                    createWeekHistoryDatabase();
                }
            );
        }
        /*
            函式 createHistoryDatabase 
            用來為該公司建立歷史資料庫，包括
                每日歷史資料庫       database\company\id\day.dat
                每週歷史資料庫       database\company\id\week.dat
                每月歷史資料庫       database\company\id\month.dat
         */
        public void createHistoryDatabase(CreateHistoryDatabaseCallback callback)
        {
            createHistoryDatabaseCallback = callback;
            createDayHistoryDatabase();
        }
        /*
         *  函式 parseTdStringsFromInfomationHttpSource 用來將下載的公司基本資料網頁剖析為
         *  <td> 字串的陣列，以利後面從此字串陣列中取出公司基本資料。
         */
        private String[] parseTdStringsFromInfomationHttpSource(String httpSource)
        {
            int startIndex = httpSource.IndexOf("<!---------------------content start-------------------->");
            int endIndex = httpSource.LastIndexOf("<!---------------------content end-------------------->");
            if ((startIndex == -1) || (endIndex == -1)) return null;
            httpSource = httpSource.Substring(startIndex, endIndex - startIndex);
            startIndex = httpSource.IndexOf("</center>");
            httpSource = httpSource.Substring(startIndex + 9);
            List<String> tdStrings = new List<String>();
            startIndex = httpSource.IndexOf("<td");
            endIndex = httpSource.IndexOf("</td>");
            while ((startIndex != -1) && (endIndex != -1))
            {
                String tempTdString = httpSource.Substring(startIndex, endIndex - startIndex);
                httpSource = httpSource.Substring(endIndex + 6);
                startIndex = tempTdString.IndexOf("\">") + 2;
                tempTdString = tempTdString.Substring(startIndex);
                tdStrings.Add(tempTdString);
                startIndex = httpSource.IndexOf("<td");
                endIndex = httpSource.IndexOf("</td>");
            }
            return tdStrings.ToArray();
        }
        /*
         *  函式 parseCompanyURL 由公司基本資料陣列中的公司網址字串中取出網址，原字串中是含有 
         *  <a>網址</a> 的字串，此函式會取出僅含網址的字串。
         */
        private String parseCompanyURL(String urlString)
        {
            int startIndex = urlString.IndexOf("\'>");
            int endIndex = urlString.LastIndexOf("</a>");
            String companyURL = urlString.Substring(startIndex + 2, endIndex - startIndex - 2);
            return companyURL;
        }
        /*
         *  函式 tdStringsToInformation 將把公司基本資料的 <td> 字串陣列中，
         *  一一取出必要的資訊，以形成 CompanyInformation 物件。
         */
        private CompanyInformation tdStringsToInformation(String[] tdStrings)
        {
            CompanyInformation companyInformation = new CompanyInformation();
            companyInformation.presidentName = tdStrings[16];
            companyInformation.ceoName = tdStrings[20];
            try
            {
                companyInformation.capital = Convert.ToDouble(tdStrings[26].Substring(0, tdStrings[26].Length - 1));
            }
            catch (Exception)
            {
                companyInformation.capital = 0;
            }
            companyInformation.companyURL = parseCompanyURL(tdStrings[34]);
            companyInformation.marginYear = tdStrings[37].Substring(9, tdStrings[37].Length - 10);
            try
            {
                companyInformation.grossMargin = Convert.ToDouble(tdStrings[41].Substring(0, tdStrings[41].Length - 1));
            }
            catch (Exception)
            {
                companyInformation.grossMargin = 0;
            }
            try
            {
                companyInformation.operatingProfitMargin = Convert.ToDouble(tdStrings[47].Substring(0, tdStrings[47].Length - 1));
            }
            catch (Exception)
            {
                companyInformation.operatingProfitMargin = 0;
            }
            try
            {
                companyInformation.earningBeforeTaxMargin = Convert.ToDouble(tdStrings[53].Substring(0, tdStrings[53].Length - 1));
            }
            catch (Exception)
            {
                companyInformation.earningBeforeTaxMargin = 0;
            }
            try
            {
                companyInformation.ROA = Convert.ToDouble(tdStrings[59].Substring(0, tdStrings[59].Length - 1));
            }
            catch (Exception)
            {
                companyInformation.ROA = 0;
            }
            try
            {
                companyInformation.ROE = Convert.ToDouble(tdStrings[65].Substring(0, tdStrings[65].Length - 1));
            }
            catch (Exception)
            {
                companyInformation.ROE = 0;
            }
            companyInformation.s1Name = tdStrings[42];
            if (tdStrings[43] != "-")
            {
                companyInformation.s1EPS = Convert.ToDouble(tdStrings[43].Substring(0, tdStrings[43].Length - 1));
            }
            companyInformation.s2Name = tdStrings[48];
            if (tdStrings[49] != "-")
            {
                companyInformation.s2EPS = Convert.ToDouble(tdStrings[49].Substring(0, tdStrings[49].Length - 1));
            }
            companyInformation.s3Name = tdStrings[54];
            if (tdStrings[55] != "-")
            {
                companyInformation.s3EPS = Convert.ToDouble(tdStrings[55].Substring(0, tdStrings[55].Length - 1));
            }
            companyInformation.s4Name = tdStrings[60];
            if (tdStrings[61] != "-")
            {
                companyInformation.s4EPS = Convert.ToDouble(tdStrings[61].Substring(0, tdStrings[61].Length - 1));
            }
            companyInformation.y1Name = tdStrings[44];
            if (tdStrings[45] != "-")
            {
                companyInformation.y1EPS = Convert.ToDouble(tdStrings[45].Substring(0, tdStrings[45].Length - 1));
            }
            companyInformation.y2Name = tdStrings[50];
            if (tdStrings[51] != "-")
            {
                companyInformation.y2EPS = Convert.ToDouble(tdStrings[51].Substring(0, tdStrings[51].Length - 1));
            }
            companyInformation.y3Name = tdStrings[56];
            if (tdStrings[57] != "-")
            {
                companyInformation.y3EPS = Convert.ToDouble(tdStrings[57].Substring(0, tdStrings[57].Length - 1));
            }
            companyInformation.y4Name = tdStrings[62];
            if (tdStrings[63] != "-")
            {
                companyInformation.y4EPS = Convert.ToDouble(tdStrings[63].Substring(0, tdStrings[63].Length - 1));
            }
            var bookValuePerShare = tdStrings[66].Substring(8, tdStrings[66].Length - 9);
            if ((bookValuePerShare == "") || (bookValuePerShare == "-"))
            {
                companyInformation.bookValuePerShare = 0;
            }
            else
            {
                companyInformation.bookValuePerShare = Convert.ToDouble(bookValuePerShare);
            }
            return companyInformation;
        }
        /*
         *  函式 saveBaseInformation 用來將 presidentName, ceoName, capital, companyURL
         *  存入 baseInformation.dat 檔案中。
         *  			董事長     presidentName
         *  			總經理     ceoName
         *  			股本       capital
         *  			公司網址   companyURL
         */
        private void saveBaseInformation(CompanyInformation companyInformation)
        {
            String companyBaseInformation =
                companyInformation.presidentName + "#" +
                companyInformation.ceoName + "#" +
                companyInformation.capital + "#" +
                companyInformation.companyURL + "\n";
            fileHelper.WriteText("company/" + id + "/baseInformation.dat", companyBaseInformation);
        }
        /*
            函式 saveMarginInformation 將儲存公司每季的獲利能力，
            會先比較現有資料庫中是否含有該筆記錄，資料包括：
                每季名稱         maginYear   例如"105年第1季"
                營業毛利率       grossMargin
                營業利益率       operatingProfitMargin
                稅前淨利率       earningBeforeTaxMargin
                資產報酬率       ROA
                股東權益報酬率   ROE
                每股淨值         bookValuePerShare
        */
        private void saveMarginInformation(CompanyInformation companyInformation)
        {
            /* 一開始沒有資料庫時，假設最後獲利時間是 100年第1季，如此新增資料一定會加入到庫料庫中 */
            String lastMarginYear = "100年第1季";
            String oldMarginInformationString = "";
            if (fileHelper.Exists("company/" + id + "/marginInformation.dat"))
            {
                oldMarginInformationString = fileHelper.ReadText(
                    "company/" + id + "/marginInformation.dat");
                String[] oldMarginInformationStringSplit =
                    oldMarginInformationString.Split(new string[] { "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
                var lastMarginInformationStringSplit =
                  oldMarginInformationStringSplit[oldMarginInformationStringSplit.Length - 1].
                  Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                /* 從最後獲利資料中，取出最後一季的時間 */
                lastMarginYear = lastMarginInformationStringSplit[0];
            }
            if (String.Compare(companyInformation.marginYear, lastMarginYear) > 0)
            {
                /* 如果新增的每季獲利時間比資料庫中最後一筆獲利的時間還新，才要附加到資料庫中 */
                String marginInformation = oldMarginInformationString +
                  "\n" + companyInformation.marginYear + " " +
                  companyInformation.grossMargin + "% " +
                  companyInformation.operatingProfitMargin + "% " +
                  companyInformation.earningBeforeTaxMargin + "% " +
                  companyInformation.ROA + "% " +
                  companyInformation.ROE + "% " +
                  companyInformation.bookValuePerShare + "元";
                fileHelper.WriteText("company/" + id + "/marginInformation.dat", marginInformation);
            }
        }
        /*
         * 函式 saveSeasonEPS 用來將每季 EPS 存檔。
         */
        private void saveSeasonEPS(String seasonName, Double seasonEPS)
        {
            String lastSeasonName = "100年第1季";
            String oldSeasonEPSString = "";
            if (fileHelper.Exists("company/" + id + "/seasonEPS.dat"))
            {
                oldSeasonEPSString = fileHelper.ReadText(
                    "company/" + id + "/seasonEPS.dat");
                String[] oldSeasonEPSStringSplit = oldSeasonEPSString.Split(new string[] { "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
                var lastSeasonEPSStringSplit =
                  oldSeasonEPSStringSplit[oldSeasonEPSStringSplit.Length - 1].Split(new string[] { " " },
                    StringSplitOptions.RemoveEmptyEntries);
                /* 從最後獲利資料中，取出最後一季的時間 */
                lastSeasonName = lastSeasonEPSStringSplit[0];
            }
            if (String.Compare(seasonName, lastSeasonName) > 0)
            {
                /* 如果新增的每季 EPS 時間比資料庫中最後一筆 EPS 的時間還新，才要附加到資料庫中 */
                var seasonEPSString = oldSeasonEPSString +
                  "\n" + seasonName + " " +
                  seasonEPS + "元";
                fileHelper.WriteText("company/" + id + "/seasonEPS.dat", seasonEPSString);
            }
        }
        /*
         * 函式 saveYearEPS 用來將每年 EPS 存檔。
         */
        private void saveYearEPS(String yearNameParam, Double yearEPS)
        {
            int lastYearName = 70;
            int yearName =
                Convert.ToInt32(yearNameParam.Substring(0, yearNameParam.Length - 1));
            String oldYearEPSString = "";
            if (fileHelper.Exists("company/" + id + "/yearEPS.dat"))
            {
                oldYearEPSString = fileHelper.ReadText(
                    "company/" + id + "/yearEPS.dat");
                var oldYearEPSStringSplit = oldYearEPSString.Split(new string[] { "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
                var lastYearEPSStringSplit =
                  oldYearEPSStringSplit[oldYearEPSStringSplit.Length - 1].Split(new string[] { " " },
                    StringSplitOptions.RemoveEmptyEntries);
                /* 從最後獲利資料中，取出最後一年的時間 */
                lastYearName = Convert.ToInt32(lastYearEPSStringSplit[0]);
            }
            if (yearName > lastYearName)
            {
                /* 如果新增的每季年 EPS 時間比資料庫中最後一年 EPS 的時間還新，才要附加到資料庫中 */
                var yearEPSString = oldYearEPSString +
                  "\n" + yearName + " " +
                  yearEPS + "元";
                fileHelper.WriteText("company/" + id + "/yearEPS.dat", yearEPSString);
            }
        }
        /*
         * 函式 saveCompanyInformation 用來將公司基本資料存檔。
         */
        private void saveCompanyInformation(CompanyInformation companyInformation)
        {
            saveBaseInformation(companyInformation);
            saveMarginInformation(companyInformation);
            if (companyInformation.s4Name != null)
            {
                saveSeasonEPS(companyInformation.s4Name, companyInformation.s4EPS);
            }
            if (companyInformation.s3Name != null)
            {
                saveSeasonEPS(companyInformation.s3Name, companyInformation.s3EPS);
            }
            if (companyInformation.s2Name != null)
            {
                saveSeasonEPS(companyInformation.s2Name, companyInformation.s2EPS);
            }
            if (companyInformation.s1Name != null)
            {
                saveSeasonEPS(companyInformation.s1Name, companyInformation.s1EPS);
            }
            if (companyInformation.y4Name != null)
            {
                saveYearEPS(companyInformation.y4Name, companyInformation.y4EPS);
            }
            if (companyInformation.y3Name != null)
            {
                saveYearEPS(companyInformation.y3Name, companyInformation.y3EPS);
            }
            if (companyInformation.y2Name != null)
            {
                saveYearEPS(companyInformation.y2Name, companyInformation.y2EPS);
            }
            if (companyInformation.y1Name != null)
            {
                saveYearEPS(companyInformation.y1Name, companyInformation.y1EPS);
            }
        }
        /*
            根據傳入的 id 為該公司建立基本資訊資料庫，包括
                基本資訊資料庫       database\company\id\baseInformation.dat
                每季獲利資料庫       database\company\id\marginInformation.dat
                每季EPS資料庫        database\company\id\seasonEPS.dat
                每年EPS資料庫        database\company\id\yearEPS.dat
         */
        String tempId;
        CreateInformationDatabaseCallback createInformationDatabaseCallback;
        public void createInfomationDatabase(CreateInformationDatabaseCallback callback)
        {
            createInformationDatabaseCallback = callback;
            new WarningWriter().showMessage(
                "開始更新 " + id + " 公司的基本資料。\r\n"
                );
            if (id.Length > 4)
            {
                tempId = id.Substring(0, 4);
            }
            else
            {
                tempId = id;
            }
            httpHelper.getHttpSource(
                "https://tw.stock.yahoo.com/d/s/company_" + tempId + ".html",
                "big5",
                (httpSource) =>
                {
                    String[] tdStrings =
                        parseTdStringsFromInfomationHttpSource(httpSource);
                    CompanyInformation companyInformation = null;
                    if (tdStrings == null)
                    {
                        /* 取得資料失敗，不寫出資料庫 */
                    }
                    else
                    {
                        companyInformation = tdStringsToInformation(tdStrings);
                        saveCompanyInformation(companyInformation);
                    }
                    
                    String printString = "";
                    for (int i = 0; i < tdStrings.Length; i++)
                    {
                        printString = printString + i.ToString() + "\t" + tdStrings[i] + "\r\n";
                    }
                    // String printString = companyInformation.y1Name;
                    // new MessageWriter().showMessage(printString);
                    
                    createInformationDatabaseCallback();
                }
            );
        }
        /* 以下程式用以處理各公司每月營收資料庫 */
        /*
         *  函式 parseTdStringsFromMonthEarningHttpSource 用來將下載的公司每月營收資料網頁剖析為
         *  <td> 字串的陣列，以利後面從此字串陣列中取出公司每月營收資料。
         */
        private String[] parseTdStringsFromMonthEarningHttpSource(String httpSource)
        {
            int startIndex = httpSource.IndexOf("<!---------------------content start-------------------->");
            int endIndex = httpSource.LastIndexOf("<!---------------------content end-------------------->");
            if ((startIndex == -1) || (endIndex == -1))
                return null;
            httpSource = httpSource.Substring(startIndex, endIndex - startIndex);
            for (int i = 0; i < 3; i++)
            {
                startIndex = httpSource.IndexOf("</table>");
                httpSource = httpSource.Substring(startIndex + 9);
            }
            for (int i = 0; i < 7; i++)
            {
                endIndex = httpSource.LastIndexOf("<table");
                httpSource = httpSource.Substring(0, endIndex);
            }
            List<String> tdStrings = new List<String>();
            startIndex = httpSource.IndexOf("<td");
            endIndex = httpSource.IndexOf("</td>");
            while ((startIndex != -1) && (endIndex != -1))
            {
                String tempTdString = httpSource.Substring(startIndex, endIndex - startIndex);
                httpSource = httpSource.Substring(endIndex + 6);
                startIndex = tempTdString.IndexOf("\">") + 2;
                tempTdString = tempTdString.Substring(startIndex);
                tdStrings.Add(tempTdString);
                startIndex = httpSource.IndexOf("<td");
                endIndex = httpSource.IndexOf("</td>");
            }
            return tdStrings.ToArray();
        }
        /*
         *  函式 getMonthEarningFromTdStrings 用來將下載的 tdStrings 字串陣列
         *  取出一個年度的每月營收字串。
         *  每月營收的字串中有逗號在其中，例如 6,575,590 ，目前暫不處理直接存檔，
         *  以向後相容。
         *  某些新上市的公司，甚至有 - 在營收中。
         */
        private String getMonthEarningFromTdStrings(int yearIndex, int startIndex, String[] tdStrings)
        {
            String yearString = tdStrings[yearIndex];
            yearString = yearString.Substring(3, 3);
            String earningString = yearString + " ";
            for (int i = startIndex; i < startIndex + 12 * 9; i = i + 9)
            {
                earningString = earningString + tdStrings[i] + " ";
            }
            return earningString;
        }
        /*
         * 函式 saveEarningDatabase 用來將每月營收字串存入資料庫中。
         */
        private void saveEarningDatabase(String earningString)
        {
            int lastTime = 70;
            String earningDatabaseString = "";
            String[] earningDatabaseStringSplit;
            List<String> earningDatabaseStringList = new List<string>();
            if (fileHelper.Exists("company/" + id + "/earning.dat"))
            {
                earningDatabaseString = fileHelper.ReadText(
                    "company/" + id + "/earning.dat");
                earningDatabaseStringSplit = earningDatabaseString.Split(new string[] { "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < earningDatabaseStringSplit.Length; i++)
                {
                    earningDatabaseStringList.Add(earningDatabaseStringSplit[i]);
                }
                var lastEarning = earningDatabaseStringSplit[earningDatabaseStringSplit.Length - 1];
                var lastEarningSplit = lastEarning.Split(new string[] { " " },
                    StringSplitOptions.RemoveEmptyEntries);
                lastTime = Convert.ToInt32(lastEarningSplit[0]);
            }
            var earningStringSplit = earningString.Split(new string[] { " " },
                    StringSplitOptions.RemoveEmptyEntries);
            var earningTime = Convert.ToInt32(earningStringSplit[0]);
            if (earningTime == lastTime)
            {
                earningDatabaseStringList[earningDatabaseStringList.Count() - 1] = earningString;
            }
            else if (earningTime > lastTime)
            {
                earningDatabaseStringList.Add(earningString);
            }
            String saveString = "";
            for (int i = 0; i < earningDatabaseStringList.Count(); i++)
            {
                saveString = saveString + earningDatabaseStringList[i] + "\n";
            }
            fileHelper.WriteText("company/" + id + "/earning.dat", saveString);
        }
        CreateMonthEarningDatabaseCallback createMonthEarningDatabaseCallback;
        /*
            為該公司建立月營收資料庫，包括
                每月營收資料庫     database\company\id\earning.dat
         */
        public void createMonthEarningDatabase(CreateMonthEarningDatabaseCallback callback)
        {
            createMonthEarningDatabaseCallback = callback;
            new WarningWriter().showMessage(
                "開始更新 " + id + " 公司的每月營收資料。\r\n"
            );
            if (id.Length > 4)
            {
                tempId = id.Substring(0, 4);
            }
            else
            {
                tempId = id;
            }
            httpHelper.getHttpSource("https://tw.stock.yahoo.com/d/s/earning_" +
                tempId + ".html",
                "big5",
                (httpSource) =>
                {
                    String[] tdStrings = parseTdStringsFromMonthEarningHttpSource(httpSource);
                    if (tdStrings != null)
                    {
                        String previousEaningString = getMonthEarningFromTdStrings(3, 15, tdStrings);
                        String currentEarningString = getMonthEarningFromTdStrings(4, 18, tdStrings);
                        saveEarningDatabase(previousEaningString);
                        saveEarningDatabase(currentEarningString);
                    }
                    createMonthEarningDatabaseCallback();
                    /*
                    String printString = "";
                    for (int i = 0; i < tdStrings.Count(); i++)
                    {
                        printString = printString + i.ToString() + "\t" + tdStrings[i] + "\r\n";
                    }

                    String printString = previousEaningString + "\r\n" + currentEarningString;
                    new MessageWriter().showMessage(printString);
                    */
                }
            );
        }
        /* 以下程式用來處理每年股利資料 */
        /*
         *  函式 parseTdStringsFromYearDividendHttpSource 用來將下載的公司每月營收資料網頁剖析為
         *  <td> 字串的陣列，以利後面從此字串陣列中取出公司每月營收資料。
         */
        private String[] parseTdStringsFromYearDividendHttpSource(String httpSource)
        {
            int startIndex = httpSource.IndexOf("<!---------------------content start-------------------->");
            int endIndex = httpSource.LastIndexOf("<!---------------------content end-------------------->");
            if ((startIndex == -1) || (endIndex == -1)) return null;
            httpSource = httpSource.Substring(startIndex, endIndex - startIndex);
            for (int i = 0; i < 2; i++)
            {
                startIndex = httpSource.IndexOf("</table>");
                httpSource = httpSource.Substring(startIndex + 9);
            }
            for (int i = 0; i < 2; i++)
            {
                endIndex = httpSource.LastIndexOf("</table>");
                httpSource = httpSource.Substring(0, endIndex);
            }
            List<String> tdStrings = new List<String>();
            startIndex = httpSource.IndexOf("<td");
            endIndex = httpSource.IndexOf("</td>");
            while ((startIndex != -1) && (endIndex != -1))
            {
                String tempTdString = httpSource.Substring(startIndex, endIndex - startIndex);
                httpSource = httpSource.Substring(endIndex + 6);
                startIndex = tempTdString.IndexOf("\">") + 2;
                tempTdString = tempTdString.Substring(startIndex);
                tdStrings.Add(tempTdString);
                startIndex = httpSource.IndexOf("<td");
                endIndex = httpSource.IndexOf("</td>");
            }
            return tdStrings.ToArray();
        }
        /*
         *  函式 tdStringToDividendStrings 用來將下載的字串轉換成每年股利字串。
         */
        private String[] tdStringToDividendStrings(String[] tdStrings)
        {
            List<String> dividendStringList = new List<String>();
            for (int i = 6; i < tdStrings.Length; i = i + 6)
            {
                // String dividendString = tdStrings[i];
                String dividendString = "";
                for (int k = 0; k < 6; k++)
                {
                    dividendString = dividendString + " " + tdStrings[i + k];
                }
                dividendStringList.Add(dividendString);
            }
            return dividendStringList.ToArray();
        }
        /*
         *  函式 saveYearDividenDatabase 用來將每年股利資料存檔。
         */
        private void saveYearDividenDatabase(String[] dividendStrings)
        {
            var lastTime = 70;
            String dividendDatabaseString = "";
            if (fileHelper.Exists("company/" + id + "/dividend.dat"))
            {
                dividendDatabaseString = fileHelper.ReadText("company/" + id + "/dividend.dat");
                var dividendDatabaseStringSplit = dividendDatabaseString.Split(new string[] { "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
                if (dividendDatabaseStringSplit.Length >= 1)
                {
                    var lastDividend = dividendDatabaseStringSplit[dividendDatabaseStringSplit.Length - 1];
                    var lastDividendSplit = lastDividend.Split(new string[] { " " },
                        StringSplitOptions.RemoveEmptyEntries);
                    lastTime = Convert.ToInt32(lastDividendSplit[0]);
                }
            }
            for (int i = dividendStrings.Length - 1; i >= 0; i--)
            {
                String oneDividendString = dividendStrings[i];
                var oneDividendStringSplit = oneDividendString.Split(new string[] { " " },
                    StringSplitOptions.RemoveEmptyEntries);
                int oneTime = Convert.ToInt32(oneDividendStringSplit[0]);
                if (oneTime > lastTime)
                {
                    dividendDatabaseString = dividendDatabaseString + oneDividendString + "\n";
                }
            }
            fileHelper.WriteText("company/" + id + "/dividend.dat", dividendDatabaseString);
        }
        /*
            為公司建立歷年股利資料庫，包括
                歷年股利資料庫       database\company\id\dividend.dat 
            該資料庫中每一行為一年的股利記錄，內容為：
                年度 現金股利 盈餘配股 公積配股 股票股利 合計 
         */
        CreateYearDividendDatabaseCallback createYearDividendDatabaseCallback;
        public void createYearDividendDatabase(CreateYearDividendDatabaseCallback callback)
        {
            createYearDividendDatabaseCallback = callback;
            new WarningWriter().showMessage(
                "開始更新 " + id + " 公司的每年股利資料。\r\n"
            );
            if (id.Length > 4)
            {
                tempId = id.Substring(0, 4);
            }
            else
            {
                tempId = id;
            }
            httpHelper.getHttpSource("https://tw.stock.yahoo.com/d/s/dividend_" +
                tempId + ".html",
                "big5",
                (httpSource) =>
                {
                    String[] tdStrings = parseTdStringsFromYearDividendHttpSource(httpSource);
                    if (tdStrings != null)
                    {
                        int yearCount = tdStrings.Length / 6 - 1;
                        String[] dividendStrings = tdStringToDividendStrings(tdStrings);
                        saveYearDividenDatabase(dividendStrings);
                    }
                    createYearDividendDatabaseCallback();
                    /*
                    var printString = "";
                    for (int i = 0; i < dividendStrings.Count(); i++)
                    {
                        printString = printString + i.ToString() + "\t" + dividendStrings[i] + "\r\n";
                    }
                    new MessageWriter().showMessage(printString);*/
                }
            );
        }
        /*
         *  此方法用來取得此公司的歷史資料陣列，historyType 為 "d" 表示
         *  是每日歷史資料，"w" 表示是每週歷史資料，"m" 表示是每月歷史資料。
         */
        public HistoryData[] getRealHistoryDataArray(String historyType)
        {
            if (historyType == "d")
            {
                if (dayRealHistoryData == null)
                {
                    dayRealHistoryData = getRealOldHistoryData("company/" + id + "/day.dat");
                }
                return dayRealHistoryData;
            }
            else if (historyType == "w")
            {
                if (weeRealkHistoryData == null)
                {
                    weeRealkHistoryData = getRealOldHistoryData("company/" + id + "/week.dat");
                }
                return weeRealkHistoryData;
            }
            else if (historyType == "m")
            {
                if (monthRealHistoryData == null)
                {
                    monthRealHistoryData = getRealOldHistoryData("company/" + id + "/month.dat");
                }
                return monthRealHistoryData;
            }
            return new List<HistoryData>().ToArray();
        }
        /*
         *  此方法用來取得此公司的歷史資料陣列，historyType 為 "d" 表示
         *  是每日歷史資料，"w" 表示是每週歷史資料，"m" 表示是每月歷史資料。
         */
        public HistoryData[] getHistoryDataArray(String historyType)
        {
            if (historyType == "d")
            {
                dayHistoryData = getOldHistoryData("company/" + id + "/day.dat");
                return dayHistoryData;
            }
            else if (historyType == "w")
            {
                weekHistoryData = getOldHistoryData("company/" + id + "/week.dat");
                return weekHistoryData;
            }
            else if (historyType == "m")
            {
                monthHistoryData = getOldHistoryData("company/" + id + "/month.dat");
                return monthHistoryData;
            }
            return new List<HistoryData>().ToArray();
        }
        /*
         * 此方法用來取得公司的資本額，資本額將從各公司的 baseInformation.dat
         * 檔案中讀出，並設定給 company.capital 資料成員。
         * 請注意，此方法沒有傳回值。
         */
        public void getCapital()
        {
            if (id.Length > 4)
            {
                tempId = id.Substring(0, 4);
            }
            else
            {
                tempId = id;
            }
            var baseInformation = fileHelper.ReadText("company/" + id + "/baseInformation.dat");
            var baseInformationSplit = baseInformation.Split(new string[] { "#" },
                    StringSplitOptions.RemoveEmptyEntries);
            capital = Convert.ToDouble(baseInformationSplit[2]);
        }
        /*
         *  此方法用來取得公司的每季EPS，每季EPS將從各公司的 seasonEPS.dat
         *  檔案中讀出，並設定給 company.seasonEPS 資料成員。
         *  請注意，此方法沒有傳回值。
         */
        public void getSeasonEPS()
        {
            if (id.Length > 4)
            {
                tempId = id.Substring(0, 4);
            }
            else
            {
                tempId = id;
            }
            var seasonEPSString = fileHelper.ReadText("company/" + id + "/seasonEPS.dat");
            var seasonEPSSplit = seasonEPSString.Split(new string[] { "\n" },
                              StringSplitOptions.RemoveEmptyEntries);
            List<Double> seasonEPSList = new List<double>();
            for (var i = 1; i < seasonEPSSplit.Length; i++)
            {
                var oneSeasonEPS = seasonEPSSplit[i];
                if (oneSeasonEPS == "\r")
                {
                    continue;
                }
                var oneSeasonEPSSplit = oneSeasonEPS.Split(new string[] { " " },
                                StringSplitOptions.RemoveEmptyEntries);
                Double oneSeasonEPSValue = 0;
                try
                {
                    String seasonValue;
                    /* 若最後一個字元是 \r ，則要去掉最後二個字元，即 "元\r" */
                    if (oneSeasonEPSSplit[1].Substring(oneSeasonEPSSplit[1].Length - 1, 1) == "\r")
                    {
                        seasonValue = oneSeasonEPSSplit[1].Substring(0, oneSeasonEPSSplit[1].Length - 2);
                    }
                    else
                    {
                        seasonValue = oneSeasonEPSSplit[1].Substring(0, oneSeasonEPSSplit[1].Length - 1);
                    }
                    oneSeasonEPSValue = Convert.ToDouble(seasonValue);
                }
                catch (Exception error)
                {
                    new WarningWriter().showMessage(
                        error.Message + "\r\n" + id + " getSeasonEPS 錯誤，有 NaN 資料!"
                        );
                    oneSeasonEPSValue = 0;
                }
                seasonEPSList.Add(oneSeasonEPSValue);
            }
            seasonEPS = seasonEPSList.ToArray();
        }
        /*
         * 此方法用來取得公司的每年EPS，每年EPS將從各公司的 yearEPS.dat
         * 檔案中讀出，並設定給 company.yearEPS 資料成員。
         * 請注意，此方法沒有傳回值。
         */
        public void getYearEPS()
        {
            if (id.Length > 4)
            {
                tempId = id.Substring(0, 4);
            }
            else
            {
                tempId = id;
            }
            var yearEPSString = fileHelper.ReadText("company/" + id + "/yearEPS.dat");
            var yearEPSSplit = yearEPSString.Split(new string[] { "\n" },
                              StringSplitOptions.RemoveEmptyEntries);
            List<Double> yearEPSList = new List<double>();
            for (var i = 1; i < yearEPSSplit.Length; i++)
            {
                var oneyearEPS = yearEPSSplit[i];
                if (oneyearEPS == "\r")
                {
                    continue;
                }
                var oneyearEPSSplit = oneyearEPS.Split(new string[] { " " },
                                StringSplitOptions.RemoveEmptyEntries);
                Double oneyearEPSValue = 0;
                try
                {
                    String yearValue;
                    /* 若最後一個字元是 \r ，則要去掉最後二個字元，即 "元\r" */
                    if (oneyearEPSSplit[1].Substring(oneyearEPSSplit[1].Length - 1, 1) == "\r")
                    {
                        yearValue = oneyearEPSSplit[1].Substring(0, oneyearEPSSplit[1].Length - 2);
                    }
                    else
                    {
                        yearValue = oneyearEPSSplit[1].Substring(0, oneyearEPSSplit[1].Length - 1);
                    }
                    oneyearEPSValue = Convert.ToDouble(yearValue);
                }
                catch (Exception error)
                {
                    new WarningWriter().showMessage(
                        error.Message + "\r\n" + id + " getyearEPS 錯誤，有 NaN 資料!");
                    oneyearEPSValue = 0;
                }
                yearEPSList.Add(oneyearEPSValue);
            }
            yearEPS = yearEPSList.ToArray();
        }
        /*
         * getDividend()
         * 此方法用來取得公司的每年股利，每年股利將從各公司的 dividend.dat
         * 檔案中讀出，並設定給 company.dividend 資料成員。
         * 請注意，此方法沒有傳回值。
         */
        public void getDividend()
        {
            if (id.Length > 4)
            {
                tempId = id.Substring(0, 4);
            }
            else
            {
                tempId = id;
            }
            var dividendString = fileHelper.ReadText("company/" + id + "/dividend.dat");
            var dividendSplit = dividendString.Split(new string[] { "\n" },
                              StringSplitOptions.RemoveEmptyEntries);
            List<Double> dividendList = new List<double>();
            for (var i = 0; i < dividendSplit.Length; i++)
            {
                var onedividend = dividendSplit[i];
                var onedividendSplit = onedividend.Split(new string[] { " " },
                                StringSplitOptions.RemoveEmptyEntries);
                Double onedividendValue = 0;
                try
                {
                    onedividendValue = Convert.ToDouble(onedividendSplit[onedividendSplit.Length - 1]);
                }
                catch (Exception error)
                {
                    new WarningWriter().showMessage(
                        error.Message + "\r\n" + id + " getdividend 錯誤，有 NaN 資料!");
                    onedividendValue = 0;
                }
                dividendList.Add(onedividendValue);
            }
            dividend = dividendList.ToArray();
        }
        /*
         * getMarginInformation()
            函式 getMarginInformation 用來取得公司的獲利能力資料，
                資料包括：
                每季名稱         maginYear   例如"105年第1季"
                營業毛利率       grossMargin
                營業利益率       operatingProfitMargin
                稅前淨利率       earningBeforeTaxMargin
                資產報酬率       ROA
                股東權益報酬率   ROE
                每股淨值         bookValuePerShare
         */
        public CompanyInformation[] getMarginInformation()
        {
            var filename = "company/" + id + "/marginInformation.dat";
            var oldMarginInformationString = new FileHelper().ReadText(filename);
            String[] oldMarginInformationStringSplit =
                oldMarginInformationString.Split(new string[] { "\n" },
                StringSplitOptions.RemoveEmptyEntries);
            List<CompanyInformation> companyInformationList = new List<CompanyInformation>();
            for (int i = 0; i < oldMarginInformationStringSplit.Length; i++)
            {
                var lastMarginInformationStringSplit =
                    oldMarginInformationStringSplit[i].
                    Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (lastMarginInformationStringSplit.Length < 7)
                {
                    continue;
                }
                CompanyInformation companyInformation = new CompanyInformation();
                companyInformation.marginYear = lastMarginInformationStringSplit[0];
                companyInformation.grossMargin = Convert.ToDouble(
                    lastMarginInformationStringSplit[1].Substring(0, lastMarginInformationStringSplit[1].Length - 1)
                    );
                companyInformation.operatingProfitMargin = Convert.ToDouble(
                    lastMarginInformationStringSplit[2].Substring(0, lastMarginInformationStringSplit[2].Length - 1)
                    );
                companyInformation.earningBeforeTaxMargin = Convert.ToDouble(
                    lastMarginInformationStringSplit[3].Substring(0, lastMarginInformationStringSplit[3].Length - 1)
                    );
                companyInformation.ROA = Convert.ToDouble(
                    lastMarginInformationStringSplit[4].Substring(0, lastMarginInformationStringSplit[4].Length - 1)
                    );
                companyInformation.ROE = Convert.ToDouble(
                    lastMarginInformationStringSplit[5].Substring(0, lastMarginInformationStringSplit[5].Length - 1)
                    );
                try
                {
                    companyInformation.bookValuePerShare = Convert.ToDouble(
                        lastMarginInformationStringSplit[6].Substring(0, lastMarginInformationStringSplit[6].Length - 1)
                        );
                }
                catch (Exception)
                {
                    companyInformation.bookValuePerShare = -1;
                }
                companyInformationList.Add(companyInformation);
            }

            return companyInformationList.ToArray();
        }
        /*
         * getHistoryData80 由傳入的歷史資料陣列中取最後 80 筆資料傳回。
         * 如果不足 80 筆則全數傳回。
         */
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
        /*
         * kValue 計算傳入歷史資料 historyData80 的 KDJ 值陣列。
         */
        public KDJ[] kValue(HistoryData[] historyData80)
        {
            Indicator indicator = new Indicator(historyData80);
            Double[] rsvArray = indicator.calcRsvArray(9);
            KDJ[] kdjArray = indicator.calcKDJArray(rsvArray);
            return kdjArray;
        }
        /*
         * earningStringToDoubleEarning 函式用來將每月營收字串轉換成數值 Double 型態。
         */
        private Double[] earningStringToDoubleEarning(String[] earningString)
        {
            List<Double> earningList = new List<Double>();
            for (var i = 0; i < earningString.Length; i++)
            {
                String oneEarning = earningString[i];
                List<String> tempStringList = new List<string>();
                for (var k = 0; k < oneEarning.Length; k++)
                {
                    if (oneEarning.Substring(k, 1) != ",")
                    {
                        tempStringList.Add(oneEarning.Substring(k, 1));
                    }
                }
                String[] tempStringArray = tempStringList.ToArray();
                var doubleEaringString = String.Concat(tempStringArray);
                Double earning;
                try
                {
                    earning = Convert.ToDouble(doubleEaringString);
                }
                catch (Exception)
                {
                    earning = 0;
                }
                earningList.Add(earning);
            }
            return earningList.ToArray();
        }
        /*
         * getEarning 傳回歷年的每月營收資訊。
         */
        public EarningInformation[] getEarning()
        {
            String earningDatabaseString = "";
            String[] earningDatabaseStringSplit;
            List<String> earningDatabaseStringList = new List<string>();
            List<EarningInformation> earningInformationList = new List<EarningInformation>();
            if (fileHelper.Exists("company/" + id + "/earning.dat"))
            {
                earningDatabaseString = fileHelper.ReadText(
                    "company/" + id + "/earning.dat");
                earningDatabaseStringSplit = earningDatabaseString.Split(new string[] { "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < earningDatabaseStringSplit.Length; i++)
                {
                    earningDatabaseStringList.Add(earningDatabaseStringSplit[i]);
                }
                for (int i = 0; i < earningDatabaseStringSplit.Length; i++)
                {
                    var oneEarning = earningDatabaseStringSplit[i];
                    var oneEarningSplit = oneEarning.Split(new string[] { " " },
                        StringSplitOptions.RemoveEmptyEntries);
                    var earingYear = Convert.ToInt32(oneEarningSplit[0]);
                    EarningInformation earingInformation = new EarningInformation();
                    earingInformation.year = earingYear;
                    List<String> earningList = new List<String>();
                    for (int k = 1; k < oneEarningSplit.Length; k++)
                    {
                        String oneMonthEarning = oneEarningSplit[k];
                        if (oneMonthEarning != "\r")
                        {
                            earningList.Add(oneMonthEarning);
                        }
                    }
                    earingInformation.earningString = earningList.ToArray();
                    earingInformation.earning = earningStringToDoubleEarning(earingInformation.earningString);
                    earingInformation.increasePercentCompareToLastYear = new Double[earingInformation.earning.Length];
                    earningInformationList.Add(earingInformation);
                }
            }
            EarningInformation[] earningInformationArray = earningInformationList.ToArray();
            for (int i = 1; i < earningInformationArray.Length; i++)
            {
                EarningInformation thisYearEarningInformation = earningInformationArray[i];
                EarningInformation lastYearEarningInformation = earningInformationArray[i - 1];
                for (var k = 0; k < thisYearEarningInformation.earning.Length; k++)
                {
                    if (thisYearEarningInformation.earning[k] > 0)
                    {
                        thisYearEarningInformation.increasePercentCompareToLastYear[k] =
                            (thisYearEarningInformation.earning[k] - lastYearEarningInformation.earning[k]) /
                            lastYearEarningInformation.earning[k] * 100.0;
                    }
                    else
                    {
                        thisYearEarningInformation.increasePercentCompareToLastYear[k] = 0.0;
                    }
                }
            }
            return earningInformationArray;
        }
        /*
         * getMarketMaker()
         * 此方法用來取得過去 10 天主力的量能，以陣列形式傳回，每個元素是 MarketMakerInformation 型態的值。
         * MarketMakerInformation 中成員解釋如下：
         *  date            日期
         *  fValue;         外資持股(張)
         *  pValue;         投信持股(張)
         *  iValue;         自營商持股(張)
         *  value           主力籌碼大小
         *  increase        和前一交易日比較的增減值(買賣超值)
         *  increasePercent 和前一交易日比較的增減百分比
         */
        public MarketMakerInformation[] getMarketMaker(HistoryData[] historyData80)
        {
            List<MarketMakerInformation> marketMakerInformationList = new List<MarketMakerInformation>();
            int endIndex = historyData80.Length - 1;
            int startIndex = endIndex - 10;
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            MarketMakerInformation previousInfo = null;
            for (int i = startIndex; i <= endIndex; i++)
            {
                HistoryData oneHistoryData = historyData80[i];
                MarketMakerInformation marketMakerInfo = new MarketMakerInformation();
                int Year = Convert.ToInt32(oneHistoryData.t.Substring(0, 4));
                int Month = Convert.ToInt32(oneHistoryData.t.Substring(5, 2));
                int Day = Convert.ToInt32(oneHistoryData.t.Substring(8, 2));
                marketMakerInfo.date = new DateTime(Year, Month, Day);
                marketMakerInfo.fValue = oneHistoryData.f;
                marketMakerInfo.pValue = oneHistoryData.p;
                marketMakerInfo.iValue = oneHistoryData.i;
                marketMakerInfo.value = oneHistoryData.s;
                if (i == startIndex)
                {
                    marketMakerInfo.increase = 0;
                    marketMakerInfo.increasePercent = 0;
                }
                else
                {
                    marketMakerInfo.increase = marketMakerInfo.value - previousInfo.value;
                    try
                    {
                        marketMakerInfo.increasePercent = marketMakerInfo.increase * 100 / previousInfo.value;
                    }
                    catch (Exception)
                    {
                        marketMakerInfo.increasePercent = 0;
                    }
                }
                previousInfo = marketMakerInfo;
                marketMakerInformationList.Add(marketMakerInfo);
            }
            return marketMakerInformationList.ToArray();
        }
        /*
         * getPrevHighAndLow 函式用來取得股價在六年最高及最低價。
         */
        public  Double highestIndex = 0;
        public Double lowestIndex = 0;
        public void getPrevHighANdLowIndex()
        {
            HistoryData[] monthHistoryData = getRealHistoryDataArray("m");
            Double highestPrice = 0;
            Double lowestPrice = Double.MaxValue;
            int lastIndex = monthHistoryData.Length - 1;
            for (int i = lastIndex; i >= 0; i--)
            {
                HistoryData historyData = monthHistoryData[i];
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
        }
        private String getPrevHighAndLow()
        {
            getPrevHighANdLowIndex();
            HistoryData[] dayHistoryData = getRealHistoryDataArray("d");
            Double todayPrice = dayHistoryData[dayHistoryData.Length - 1].c;
            Double indexDiff = highestIndex - lowestIndex;
            String returnText = "\t六年內股價最高是 " + highestIndex + " ，最低是 " + lowestIndex + "\r\n";
            returnText = returnText + "\t目前股價： " + todayPrice +
                " (比前高少 " + ((highestIndex - todayPrice) * 100 / indexDiff).ToString("f2") + "%)" +
                " (比前低多 " + ((todayPrice - lowestIndex) * 100 / indexDiff).ToString("f2") + "%)" +
                "\r\n";
            return returnText;
        }
        /*
         * getInformatioon() 用來取得股票的簡單描述資訊
         */
        private DateTime timeInformation;
        private String information = null;
        public String getInformatioon(StockDatabase stockDatabase)
        {
            if (timeInformation != null)
            {
                var diff = DateTime.Now - timeInformation;
                var diffDays = diff.TotalDays;
                if ((diffDays < 1) && (information != null))
                {
                    return information;
                }

            }
            String returnText = "";
            returnText = returnText + name + "(" + id + ") 類別：" + this.category + "\r\n";
            var historyData80 = getHistoryData80(getRealHistoryDataArray("d"));
            var dayHistoryData80 = getHistoryData80(getRealHistoryDataArray("d"));
            Double todayPrice = 0;
            KDJ[] kValueDay = null;
            KDJ[] kValueWeek = null;
            KDJ[] kValueMonth = null;
            if (historyData80.Length == 0)
            {
                returnText = returnText + name + "(" + id + ") 無法取得每日歷史資料!?\r\n";
            }
            else
            {
                todayPrice = historyData80[historyData80.Length - 1].c;
                kValueDay = kValue(historyData80);
            }
            historyData80 = getHistoryData80(getRealHistoryDataArray("w"));
            if (historyData80.Length == 0)
            {
                returnText = returnText + name + "(" + id + ") 無法取得每周歷史資料!?\r\n";
            }
            else
            {
                kValueWeek = kValue(historyData80);
            }
            historyData80 = getHistoryData80(getRealHistoryDataArray("m"));
            if (historyData80.Length == 0)
            {
                returnText = returnText + name + "(" + id + ") 無法取得每月歷史資料!\r\n";
            }
            else
            {
                kValueMonth = kValue(historyData80);
            }
            returnText = returnText + "個股：\r\n";
            if ((kValueDay != null) && (kValueDay.Length > 0))
            {
                returnText = returnText + "\t日k： " + kValueDay[kValueDay.Length - 1].K.ToString("f2") + "\r\n";
            }
            if ((kValueWeek != null) && (kValueWeek.Length > 0))
            {
                returnText = returnText + "\t週k： " + kValueWeek[kValueWeek.Length - 1].K.ToString("f2") + "\r\n";
            }
            if ((kValueMonth != null) && (kValueMonth.Length > 0))
            {
                returnText = returnText + "\t月k： " + kValueMonth[kValueMonth.Length - 1].K.ToString("f2") + "\r\n";
            }
            getYearEPS();
            Double oneYearEPS = yearEPS[yearEPS.Length - 1];
            returnText = returnText + getPrevHighAndLow() + "\r\n";
            returnText = returnText + "\t上一年 EPS：" + oneYearEPS +
                " 元，利率約： " + (oneYearEPS * 100.0 / todayPrice).ToString("f2") + "%\r\n";
            if (yearEPS.Length > 0)
            {
                returnText = returnText + "\t過去幾年 EPS (由遠到近)：\r\n";
                for (int j = 0; j < yearEPS.Length; j++)
                {
                    returnText = returnText + "\t\t" + yearEPS[j].ToString("f2") + "\r\n";
                }
            }
            getSeasonEPS();
            Double fourSeasonEPS = 0;
            int seasonSPECount = 0;
            for (var d = seasonEPS.Length - 1; d >= 0; d--)
            {
                fourSeasonEPS = fourSeasonEPS + seasonEPS[d];
                seasonSPECount++;
                if (seasonSPECount == 4) break;
            }
            returnText = returnText + "\t前四季 EPS：" + fourSeasonEPS +
                " 元，利率約： " + (fourSeasonEPS * 100.0 / todayPrice).ToString("f2") + "%\r\n";
            if (seasonEPS.Length > 0)
            {
                returnText = returnText + "\t過去幾季 EPS (由遠到近)：\r\n";
                for (int j = 0; j < seasonEPS.Length; j++)
                {
                    returnText = returnText + "\t\t" + seasonEPS[j].ToString("f2") + "\r\n";
                }
            }
            getDividend();
            Double oneDividend = dividend[dividend.Length - 1];
            returnText = returnText + "\t上次股利：" + oneDividend +
                " 元，殖利率約： " + (oneDividend * 100.0 / todayPrice).ToString("f2") + "%\r\n";
            if (dividend.Length > 0)
            {
                returnText = returnText + "\t過去幾年股利(由遠到近)：\r\n";
                for (int j = 0; j < dividend.Length; j++)
                {
                    returnText = returnText + "\t\t" + dividend[j].ToString("f2") + "\r\n";
                }
            }
            returnText = returnText + "\t目前股價：" + todayPrice + " 元\r\n";
            CompanyInformation[] companyInformationArray = getMarginInformation();
            CompanyInformation companyInformation = companyInformationArray[companyInformationArray.Length - 1];
            returnText = returnText + "\t每股淨值：" + companyInformation.bookValuePerShare + " 元\r\n" +
                "\t目前股價/每股淨值：" +
                (todayPrice / companyInformation.bookValuePerShare).ToString("f2") + " 倍\r\n";
            returnText = returnText + "\t歷史 ROA 及 ROE：\r\n";
            for (int j = 0; j < companyInformationArray.Length; j++)
            {
                returnText = returnText + "\t\t" + companyInformationArray[j].ROA +
                    "\t" + companyInformationArray[j].ROE + "\r\n";
            }
            EarningInformation[] earningInformation = getEarning();
            if (earningInformation.Length > 0)
            {
                EarningInformation lastEaringInformation = earningInformation[earningInformation.Length - 1];
                returnText = returnText + "\t" + lastEaringInformation.year + " 年度每月營收：\r\n";
                returnText = returnText + "\t\t月份\t營收(元)\t\t與去年比較營收增長(%)\r\n";
                for (var i = 0; i < lastEaringInformation.earningString.Length; i++)
                {
                    if (i >= 12) break;
                    if (lastEaringInformation.earningString[i] != "-")
                    {
                        returnText = returnText + "\t\t" + (i + 1) + "月\t"
                            + lastEaringInformation.earningString[i] +
                            "\t\t" + lastEaringInformation.increasePercentCompareToLastYear[i].ToString("f2") +
                            "\r\n";
                    }
                }
            }
            MarketMakerInformation[] marketMakerInfomationArray = getMarketMaker(dayHistoryData80);
            returnText = returnText + "\t" + "近十日法人籌碼狀況(單位:張)：\r\n";
            returnText = returnText + "\t\t日期\t外資\t投信\t自營商\t法人總持股\t總持股增減\t增減百分比\r\n";
            MarketMakerInformation oneMarketMakerInfomation = null;
            for (int i = 0; i < marketMakerInfomationArray.Length; i++)
            {
                oneMarketMakerInfomation = marketMakerInfomationArray[i];
                returnText = returnText + "\t\t" + oneMarketMakerInfomation.date.Month +
                    "/" + oneMarketMakerInfomation.date.Day +
                    "\t" + oneMarketMakerInfomation.fValue.ToString("f0") +
                    "\t" + oneMarketMakerInfomation.pValue.ToString("f0") +
                    "\t" + oneMarketMakerInfomation.iValue.ToString("f0") +
                    "\t" + oneMarketMakerInfomation.value.ToString("f0") +
                    "\t\t" + oneMarketMakerInfomation.increase.ToString("f0") +
                    "\t\t" + oneMarketMakerInfomation.increasePercent.ToString("f2") + "%" +
                    "\r\n";
            }
            if (oneMarketMakerInfomation != null)
            {
                Double increase = oneMarketMakerInfomation.value - marketMakerInfomationArray[0].value;
                Double increasePercent;
                try
                {
                    increasePercent = increase * 100 / marketMakerInfomationArray[0].value;
                }
                catch (Exception)
                {
                    increasePercent = 0;
                }
                returnText = returnText + "\t\t------------------------------------------------------------------------------------------------------------------------------\r\n";
                returnText = returnText + "\t\t" + "合計" +
                    "\t" + oneMarketMakerInfomation.fValue.ToString("f0") +
                    "\t" + oneMarketMakerInfomation.pValue.ToString("f0") +
                    "\t" + oneMarketMakerInfomation.iValue.ToString("f0") +
                    "\t" + oneMarketMakerInfomation.value.ToString("f0") +
                    "\t\t" + increase.ToString("f0") +
                    "\t\t" + increasePercent.ToString("f2") + "%" +
                    "\r\n";
            }
            information = returnText;
            timeInformation = DateTime.Now;
            return returnText;
        }
        /*
         * checkMatchG 函式用來檢查該公司是否通過下列條件：
         *      (1) 每年股利無負值(含零值)
         *      (2) 每季 EPS 無負值(含零值)
         *      (3) 每年 EPS 無負值(含零值)
         *      (4) 日k、週k、月k值都小於 minKValue
         *      (5) 法人十日內買超 0.2% 以上
         *      (6) 低檔(比前高少70%)以上
         */
        private Double minKValue = 25;
        public String matchGMessage;
        public void checkMatchG()
        {
            this.matchG = true;
            /* check (1) */
            getDividend();
            for (var i = 0; i < this.dividend.Length; i++)
            {
                if (dividend[i] <= 0)
                {
                    this.matchG = false;
                    matchGMessage = matchGMessage +
                        "\t\t未通過：每年股利無負值(含零值)\r\n";
                    break;
                }
            }
            /* check (2) */
            getSeasonEPS();
            for (var i = 0; i < this.seasonEPS.Length; i++)
            {
                if (seasonEPS[i] <= 0)
                {
                    this.matchG = false;
                    matchGMessage = matchGMessage +
                        "\t\t未通過：每季 EPS 無負值(含零值)\r\n";
                    break;
                }
            }
            /* check (3) */
            getYearEPS();
            for (var i = 0; i < this.yearEPS.Length; i++)
            {
                if (yearEPS[i] <= 0)
                {
                    this.matchG = false;
                    matchGMessage = matchGMessage +
                        "\t\t未通過：每年 EPS 無負值(含零值)\r\n";
                    break;
                }
            }
            /* check (4) */
            var historyData80 = getHistoryData80(getRealHistoryDataArray("d"));
            var dayHistoryData80 = historyData80;
            KDJ[] kValueDay = null;
            KDJ[] kValueWeek = null;
            KDJ[] kValueMonth = null;
            if (historyData80.Length == 0)
            {
                new MessageWriter().appendMessage(name + "(" + id + ") 無法取得每日歷史資料!?\r\n", true);
            }
            else
            {
                kValueDay = kValue(historyData80);
            }
            historyData80 = getHistoryData80(getRealHistoryDataArray("w"));
            if (historyData80.Length == 0)
            {
                new MessageWriter().appendMessage(name + "(" + id + ") 無法取得每周歷史資料!?\r\n", true);
            }
            else
            {
                kValueWeek = kValue(historyData80);
            }
            historyData80 = getHistoryData80(getRealHistoryDataArray("m"));
            if (historyData80.Length == 0)
            {
                new MessageWriter().appendMessage(name + "(" + id + ") 無法取得每月歷史資料!?\r\n", true);
            }
            else
            {
                kValueMonth = kValue(historyData80);
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
            if (!((kValueDayToday < minKValue) && (kValueMonthToday < minKValue) && (kValueWeekToday < minKValue)))
            {
                this.matchG = false;
                matchGMessage = matchGMessage +
                    "\t\t未通過：日k、週k、月k值都小於 " + minKValue + "\r\n";
            }
            /* check (5) */
            MarketMakerInformation[] marketMakerInfomationArray = getMarketMaker(dayHistoryData80);
            MarketMakerInformation oneMarketMakerInfomation = marketMakerInfomationArray[marketMakerInfomationArray.Length - 1];
            Double increasePercent = 0;
            if (oneMarketMakerInfomation != null)
            {
                Double increase = oneMarketMakerInfomation.value - marketMakerInfomationArray[0].value;
                try
                {
                    increasePercent = increase * 100 / marketMakerInfomationArray[0].value;
                }
                catch (Exception)
                {
                    increasePercent = 0;
                }
            }
            if (increasePercent <= 0.2)
            {
                this.matchG = false;
                matchGMessage = matchGMessage +
                    "\t\t未通過：法人十日內買超 0.2% 以上\r\n";
            }
            /* check (6) */
            getPrevHighANdLowIndex();
            Double todayPrice = dayHistoryData80[dayHistoryData80.Length - 1].c;
            if (((highestIndex - todayPrice) / (highestIndex - lowestIndex)) <= 0.7)
            {
                this.matchG = false;
                matchGMessage = matchGMessage +
                    "\t\t未通過：低檔(比前高少70%)以上\r\n";
            }
        }
        /*
         * checkDatabase 函式用來檢查公司的資料庫檔案是否存在。
         */
        public void checkDatabase()
        {
            FileHelper fileHelper = new FileHelper();
            passCheckDatabase = true;
            String filename;
            filename = "company/" + id + "/baseInformation.dat";
            if (!fileHelper.Exists(filename))
            {
                passCheckDatabase = false;
            }
            filename = "company/" + id + "/day.dat";
            if (!fileHelper.Exists(filename))
            {
                passCheckDatabase = false;
            }
            filename = "company/" + id + "/week.dat";
            if (!fileHelper.Exists(filename))
            {
                passCheckDatabase = false;
            }
            filename = "company/" + id + "/month.dat";
            if (!fileHelper.Exists(filename))
            {
                passCheckDatabase = false;
            }
            filename = "company/" + id + "/dividend.dat";
            if (!fileHelper.Exists(filename))
            {
                passCheckDatabase = false;
            }
            filename = "company/" + id + "/earning.dat";
            if (!fileHelper.Exists(filename))
            {
                passCheckDatabase = false;
            }
            filename = "company/" + id + "/marginInformation.dat";
            if (!fileHelper.Exists(filename))
            {
                passCheckDatabase = false;
            }
            filename = "company/" + id + "/seasonEPS.dat";
            if (!fileHelper.Exists(filename))
            {
                passCheckDatabase = false;
            }
            filename = "company/" + id + "/yearEPS.dat";
            if (!fileHelper.Exists(filename))
            {
                passCheckDatabase = false;
            }
        }
    }
}
