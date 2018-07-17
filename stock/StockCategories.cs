/*
  StockCategories 類別 ：
    方法成員： 
      00. overrideDatabase(callback)
          由 https://tw.stock.yahoo.com/h/getclass.php 下載 Yahoo 類股當日行情網頁，
          裡面有各類股的連結，由其中找出各類股的分類名稱陣列(stockCategoriesName)及
          各類股的連結網址陣列(stockCategoriesLink)。
          此函式會在 createDatabase 找不到資料庫檔案時被呼叫。
          也可以主動由程式呼叫重建類別資料庫。
      01. createDatabase(callback)
          建立台股各類股的名稱及超連結資料庫，建立完成後調用 callback 函式
          台股各類股名稱由 https://tw.stock.yahoo.com/h/getclass.php 網址取得
          產生的資料庫名稱為 stockCategories.dat，內容如下：
              水泥 https://tw.stock.yahoo.com/s/list.php?c=%A4%F4%AAd&rr=0.68880700 1489121891
              食品 https://tw.stock.yahoo.com/s/list.php?c=%AD%B9%AB%7E&rr=0.68881300 1489121891
              ...
          每個類股名稱佔一行，連結佔一行
      02. getTotalCatogories(callback)
          取得各股類別的總數，由於各股連結資料可能要由檔案或網路中取得，
          所以只能用非同步的模式取得各股類別的總數，各股類別的總數 totalCatogories
          會由 callback(totalCatogories) 傳回。
      03. getNames(callback)
          取得各股類別的名稱字串陣列，由於各股連結資料可能要由檔案或網路中取得，
          所以只能用非同步的模式取得各股類別的名稱字串陣列，各股類別的名稱字串陣列
          names 會由 callback(names) 傳回。
      04. getLinks(callback)
          取得各股類別的連結字串陣列，由於各股連結資料可能要由檔案或網路中取得，
          所以只能用非同步的模式取得各股類別的連結字串陣列，各股類別的連結字串陣列
          links 會由 callback(links) 傳回。
      05. getCategory(index, callback)
          取得第 index 各股類別名稱及連結的物件 
            { 
              name : 類股名稱,
              link : 類股網頁連結
            }
          此物件將由 callback(categoryObject) 傳回。
      例如：
          var stockCategories = new StockCategories();
          stockCategories.getCategory(
            0,
            function (categoryObject) {
              console.log(categoryObject);
              require('fs').writeFileSync("data.txt","name : "+
              categoryObject.name+", link : "+categoryObject.link);
              var categoryObjectString=require('fs').readFileSync("data.txt","utf8");
              console.log(categoryObjectString);
            }
          );
          上面程式片段建立一個 StockCategories 台股類別的資料庫於 database\
          stockCatogories.dat 檔案中，並取得第 0 筆分類的物件 { name : 類股名稱
          , link : 類股網頁連結 }

    資料成員：
      以下資料成員在 createDatabase() 或 overrideDatabase() 之後可用
      01. public int totalCategories;
          股市類別的分類數量
      02. public String[] stockCategoriesLink;
          股市類別的分類網頁連結字串的陣列
      03. public String[] stockCategoriesName;
          股市類別的分類名稱字串的陣列
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stock
{
    public delegate void CreateStockCategoriesCallback();
    public delegate void GetTotalCatogoriesCallback(int totalCategories);
    public delegate void GetNamesCallback(String[] stockCategoriesName);
    public delegate void GetLinksCallback(String[] stockCategoriesLink);
    public delegate void GetCategoryCallback(StockCategoriesLinkName stockCategory);

    public class StockCategoriesLinkName
    {
        public String Link;
        public String Name;
    }

    class StockCategories
    {
        public int totalCategories;
        public String[] stockCategoriesLink;
        public String[] stockCategoriesName;
        public String stockCategoriesString;
        CreateStockCategoriesCallback createStockDatabaseCallback;
        HttpHelper httpHelper = new HttpHelper();
        FileHelper fileHelper = new FileHelper();
        List<String> stockCategoriesLinkList = new List<string>();
        List<String> stockCategoriesNameList = new List<string>();
        /*
         *   StockCategories 類別的建構式
         */
        public StockCategories()
        {
            totalCategories = 0;
            stockCategoriesLink = null;
            stockCategoriesName = null;
        }
        /*
         * 函式 processStockCategorieslinkNameString 由傳入的字串 linkNameString
         * 找到股票分類一個類別的網頁連結字串(LinkString)及名稱字串，並放入到
         * stockCategoriesLinkList 串列及stockCategoriesNameList 串列中。
         */
        private void processStockCategorieslinkNameString(String linkNameString)
        {
            int hrefStartPos = linkNameString.IndexOf("href=");
            int hrefEndPos = linkNameString.IndexOf("\">");
            hrefStartPos = hrefStartPos + 6;
            String LinkString = linkNameString.Substring(hrefStartPos, hrefEndPos - hrefStartPos);
            LinkString = "https://tw.stock.yahoo.com" + LinkString;
            stockCategoriesLinkList.Add(LinkString);
            linkNameString = linkNameString.Substring(hrefEndPos + 2);
            stockCategoriesNameList.Add(linkNameString.Substring(0, linkNameString.Length - 4));
        }
        /* 函式 saveStockCategoriesDatabase 用來將股票分類的類別之網頁連結字串陣列及
         * 名稱字串陣列寫入到 stockCategories.dat 檔案中。
         * 檔案所在目錄是整個資料庫的目錄，在 Windows 中是
         *      C:\Users\WjChen\Documents\stock\database\
         * 在 Android 中是
         *      /storage/sdcard0/download/database 或
         *      /storage/emulated/0/download/database
         * 資料庫目錄放在 FileHelper.*.cs 和平台相關的程式碼中，不要放在此檔案中。
         */
        public void saveStockCategoriesDatabase()
        {
            String stockCategoriesString = "";
            for (var i = 0; i < totalCategories; i++)
            {
                stockCategoriesString = stockCategoriesString + stockCategoriesName[i] + "\n";
                stockCategoriesString = stockCategoriesString + stockCategoriesLink[i] + "\n";
            }
            fileHelper.WriteText(@"stockCategories.dat", stockCategoriesString);
        }
        /*
         * 函式 getHttpSourceCallback 是由網路下載股票分類的類別網頁的回呼函式，
         * 在此回呼函式中，負責由下載的頁內容中找出各類別的連結及名稱，並放入
         * stockCategoriesLink 及 stockCategoriesName 陣列中。
         */
        public void getHttpSourceCallback(String httpSource)
        {
            /*
             * 由網頁內容中找出含股票分類類別表格的 <table> 出來
             */
            int table2Index = httpSource.IndexOf("<!----table 2 ------------------------------>");
            httpSource = httpSource.Substring(table2Index - 1);
            int table3Index = httpSource.IndexOf("<!----table 3 ------------------------------>");
            httpSource = httpSource.Remove(table3Index);
            /*
             * 由找出的股票分類類別表格 <table> 中找出各 <td> 出來，
             * 每格項目包含分類網頁的連結及名稱。
             */
            List<String> tempLinkNameStringList = new List<string>();
            int tdStartPos = httpSource.IndexOf("<td");
            int tdEndPos = httpSource.IndexOf("</td>");
            do
            {
                tempLinkNameStringList.Add(httpSource.Substring(tdStartPos, tdEndPos - tdStartPos));
                httpSource = httpSource.Substring(tdEndPos + 1);
                tdStartPos = httpSource.IndexOf("<td");
                tdEndPos = httpSource.IndexOf("</td>");
            } while ((tdStartPos != -1) && (tdEndPos != -1));
            stockCategoriesString = "";
            String[] tempLinkNameStringArray = tempLinkNameStringList.ToArray();
            /*
             * 初始分類連結及名稱的串列，然後呼叫 processStockCategorieslinkNameString
             * 一一處理每個分類連結及名稱的字串。
             */
            stockCategoriesLinkList = new List<string>();
            stockCategoriesNameList = new List<string>();
            for (int i = 0; i < tempLinkNameStringArray.Count(); i++)
            {
                processStockCategorieslinkNameString(tempLinkNameStringArray[i]);
            }
            /*
             * 移除後面 6 項和憑證、ETF 相關的分類
             */
            int count = stockCategoriesLinkList.Count();
            for (int i = count - 1; i > (count - 7); i--)
            {
                stockCategoriesLinkList.RemoveAt(i);
            }
            count = stockCategoriesNameList.Count();
            for (int i = count - 1; i > (count - 7); i--)
            {
                stockCategoriesNameList.RemoveAt(i);
            }
            /*
             * 將分類連結及名稱的串列轉換為陣列
             */
            stockCategoriesLink = stockCategoriesLinkList.ToArray();
            stockCategoriesName = stockCategoriesNameList.ToArray();
            totalCategories = Math.Min(stockCategoriesLink.Count(), stockCategoriesName.Count());
            saveStockCategoriesDatabase();
            createStockDatabaseCallback();
        }
        /*
            由 https://tw.stock.yahoo.com/h/getclass.php 下載 Yahoo 類股當日行情網頁，
            裡面有各類股的連結，由其中找出各類股的分類名稱陣列(stockCategoriesName)及
            各類股的連結網址陣列(stockCategoriesLink)。
            此函式會在 createDatabase 找不到資料庫檔案時被呼叫。
            也可以主動由程式呼叫重建類別資料庫。
         */
        public void overrideDatabase(CreateStockCategoriesCallback callback)
        {
            createStockDatabaseCallback = callback;
            httpHelper.getHttpSource("https://tw.stock.yahoo.com/h/getclass.php", "big5", getHttpSourceCallback);
        }
        /*
            建立台股各類股的名稱及超連結資料庫，建立完成後調用 callback 函式
            台股各類股名稱由 https://tw.stock.yahoo.com/h/getclass.php 網址取得
            產生的資料庫名稱為 stockCategories.dat，內容如下：
                水泥 https://tw.stock.yahoo.com/s/list.php?c=%A4%F4%AAd&rr=0.68880700 1489121891
                食品 https://tw.stock.yahoo.com/s/list.php?c=%AD%B9%AB%7E&rr=0.68881300 1489121891
                ...
            每個類股名稱佔一行，連結佔一行
         */
        public void createDatabase(CreateStockCategoriesCallback callback)
        {
            createStockDatabaseCallback = callback;
            if (fileHelper.Exists("stockCategories.dat"))
            {
                stockCategoriesString = fileHelper.ReadText("stockCategories.dat");
                string[] stringSeparators = new string[] { "\n" };
                string[] resultSplit = stockCategoriesString.Split(stringSeparators,
                    StringSplitOptions.RemoveEmptyEntries);
                stockCategoriesLinkList = new List<string>();
                stockCategoriesNameList = new List<string>();
                for (int i = 0; i < resultSplit.Count(); i = i + 2)
                {
                    stockCategoriesNameList.Add(resultSplit[i]);
                    stockCategoriesLinkList.Add(resultSplit[i + 1]);
                }
                stockCategoriesLink = stockCategoriesLinkList.ToArray();
                stockCategoriesName = stockCategoriesNameList.ToArray();
                totalCategories = Math.Min(stockCategoriesLink.Count(), stockCategoriesName.Count());
                createStockDatabaseCallback();
            }
            else
            {
                overrideDatabase(callback);
            }
        }
        /*
            取得各股類別的總數，由於各股連結資料可能要由檔案或網路中取得，
            所以只能用非同步的模式取得各股類別的總數，各股類別的總數 totalCatogories
            會由 callback(totalCatogories) 傳回。
         */
        GetTotalCatogoriesCallback getTotalCatogoriesCallback;
        public void getTotalCategoriesCallback()
        {
            getTotalCatogoriesCallback(totalCategories);
        }
        public void getTotalCatogories(GetTotalCatogoriesCallback callback)
        {
            getTotalCatogoriesCallback = callback;
            if (totalCategories == 0)
            {
                createDatabase(getTotalCategoriesCallback);
            }
            else
            {
                getTotalCatogoriesCallback(totalCategories);
            }
        }
        /*
            取得各股類別的名稱字串陣列，由於各股連結資料可能要由檔案或網路中取得，
            所以只能用非同步的模式取得各股類別的名稱字串陣列，各股類別的名稱字串陣列
            names 會由 callback(names) 傳回。
         */
        GetNamesCallback getNamesCallbackTemp;
        public void getNamesCallback()
        {
            getNamesCallbackTemp(stockCategoriesName);
        }
        public void getNames(GetNamesCallback callback)
        {
            getNamesCallbackTemp = callback;
            if (totalCategories == 0)
            {
                createDatabase(getNamesCallback);
            }
            else
            {
                getNamesCallbackTemp(stockCategoriesName);
            }
        }
        /*
            取得各股類別的連結字串陣列，由於各股連結資料可能要由檔案或網路中取得，
            所以只能用非同步的模式取得各股類別的連結字串陣列，各股類別的連結字串陣列
            links 會由 callback(links) 傳回。
         */
        GetLinksCallback getLinksCallbackTemp;
        public void getLinksCallback()
        {
            getLinksCallbackTemp(stockCategoriesLink);
        }
        public void getLinks(GetLinksCallback callback)
        {
            getLinksCallbackTemp = callback;
            if (totalCategories == 0)
            {
                createDatabase(getLinksCallback);
            }
            else
            {
                getLinksCallbackTemp(stockCategoriesLink);
            }
        }
        /*
            取得第 index 各股類別名稱及連結的物件 
                { 
                    name : 類股名稱,
                    link : 類股網頁連結
                }
            此物件將由 callback(categoryObject) 傳回。
         */
        GetCategoryCallback getCategoryCallbackTemp;
        int getCategoryIndex;
        public void getCategoryCallback()
        {
            StockCategoriesLinkName stockCategory = new StockCategoriesLinkName();
            stockCategory.Link = stockCategoriesLink[getCategoryIndex];
            stockCategory.Name = stockCategoriesName[getCategoryIndex];
            getCategoryCallbackTemp(stockCategory);
        }
        public void getCategory(int index, GetCategoryCallback callback)
        {
            getCategoryCallbackTemp = callback;
            getCategoryIndex = index;
            if (totalCategories == 0)
            {
                createDatabase(getCategoryCallback);
            }
            else
            {
                getCategoryCallback();
            }
         }
    }
}
