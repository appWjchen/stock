/*
  CompanyCategories 類別：
    方法成員：
      00. overideDatabase() 
          強制建立 companyCatogories.dat 資料庫
      01. createDatabase(stockCategories, callback)
          此方法用以建立各公司所屬類別的資料庫，資料庫檔案是 database\companyCategories.dat
          檔案中，檔案中每一家公司佔一筆記錄，格式是
            公司代號  公司名稱  所屬類別\n
          並建立此物件的內部資料：
            totalCompanies   : 公司總數
            companyIds       : 公司代號陣列
            companyNames     : 公司名稱陣列
            companyCategories  : 公司所屬類別陣列
      02. getTotalCompanies(callback)
          此方法用來取得所有公司的總數，由於調用此方法時，公司總數未必已經取得，仍然可能
          需要從網路或檔案中得到所有公司資料陣列，因此要用非同步的方式回調 callback，
          並傳回 totalCompany 之值。
      03. getCompanyInfo(index, callback)
          此方法取得第 index 個公司的資料物件，如下：
            {
              id : 公司代號,
              name : 公司名稱,
              category : 公司所屬類股名稱
            }
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stock
{
    public delegate void CreateCompanyCategoriesCallback();
    public delegate void DownloadOneCategory(int index);
    public delegate void GetTotalCompaniesCallback(int totalCompanies);
    public delegate void GetCompanyInfoCallback(CompanyCategory companyCategory);

    public class CompanyCategory
    {
        public String id;
        public String name;
        public String category;
    }

    class CompanyCategories
    {
        public int totalCompanies;
        public CompanyCategory[] companyCategories;
        public String companyCategoriesString;
        List<CompanyCategory> companyCategoriesList;
        CreateCompanyCategoriesCallback createCompanyCategoriesCallback;
        StockCategories stockCategories;
        int stockCategoriesLength;
        FileHelper fileHelper = new FileHelper();
        HttpHelper httpHelper = new HttpHelper();

        /*
         * CompanyCategories 建構式
         */
        public CompanyCategories()
        {
            totalCompanies = 0;
            companyCategories = null;
        }

        /*
            函式 downloadOneCategory 是用來下載一個股票分類網頁的回呼
            函式，每次下載完一個分類的網頁，會再次回呼自身。
            傳入參數 index 是欲下載分類在 stockCategories 陣列的索引。
         */
        public void downloadOneCategory(int index)
        {
            if (index < stockCategoriesLength)
            {   // 尚未下載全部分類
                String categoryName = stockCategories.stockCategoriesName[index];
                String categoryLink = stockCategories.stockCategoriesLink[index];
                httpHelper.getHttpSource(categoryLink,
                  "big5",
                  (String httpSource) =>
                  {   // 下載單一分類網頁，處理該分類網頁，以取得公司 id 及 name
                      int startPos = httpSource.IndexOf("yui-text-left");
                      int endPos = httpSource.IndexOf("addendum");
                      httpSource = httpSource.Substring(startPos,
                          endPos - startPos);
                      startPos = httpSource.IndexOf("<table");
                      endPos = httpSource.LastIndexOf("</table>");
                      httpSource = httpSource.Substring(startPos + 6,
                          endPos - startPos);
                      startPos = httpSource.IndexOf("<table");
                      endPos = httpSource.LastIndexOf("</table>");
                      httpSource = httpSource.Substring(startPos + 6,
                          endPos - startPos);
                      startPos = httpSource.IndexOf("<table");
                      endPos = httpSource.LastIndexOf("</table>");
                      httpSource = httpSource.Substring(startPos + 6,
                          endPos - startPos);
                      startPos = httpSource.IndexOf(".html>");
                      while (startPos != -1)
                      {
                          httpSource = httpSource.Substring(startPos + 6);
                          endPos = httpSource.IndexOf("</a>");
                          String oneCompanyString = httpSource.Substring(0, endPos);
                          String[] oneCompany = oneCompanyString.Split(
                              new char[] { ' ' });
                          CompanyCategory companyCategory = new CompanyCategory();
                          companyCategory.id = oneCompany[0];
                          companyCategory.name = oneCompany[1];
                          companyCategory.category = categoryName;
                          companyCategoriesList.Add(companyCategory);
                          startPos = httpSource.IndexOf(".html>");
                      }
                      downloadOneCategory(index + 1);
                  }
              );
            }
            else
            {   // 已下載完全部分類，將 CompanyCategories 寫入資料庫
                companyCategories = companyCategoriesList.ToArray();
                totalCompanies = companyCategories.Count();
                Array.Sort(
                    companyCategories,
                    delegate(CompanyCategory companyCategories1,
                    CompanyCategory companyCategories2)
                    {
                        return companyCategories1.id.CompareTo(companyCategories2.id);
                    }
                );
                String allCompanyString = "";
                for (int i = 0; i < totalCompanies; i++)
                {
                    allCompanyString = allCompanyString + companyCategories[i].id
                        + " " + companyCategories[i].name + " " +
                        companyCategories[i].category + "\n";
                }
                fileHelper.WriteText("companyCatogories.dat", allCompanyString);
                createCompanyCategoriesCallback();
            }
        }
        /*
            函式 getTotalCategoriesCallback 是用 stockCategories.getTotalCatogories
            時，要回呼的回呼函式，用來得到 totalCategories 數值。
         */
        public void getTotalCategoriesCallback(int totalCategories)
        {
            stockCategoriesLength = totalCategories;
            if (stockCategoriesLength != 0)
            {
                downloadOneCategory(0);
            }
        }
        /*
            函式 onStockCategoriesCreated 是在 overrideDatabase 方法中，
            若 stackCategories 未建立，會先建立並且回呼的函式。
         */
        public void onStockCategoriesCreated()
        {
            stockCategories.getTotalCatogories(getTotalCategoriesCallback);
        }
        /*
            函式 overideDatabase(stockCategories, callback) 
            強制建立 companyCatogories.dat 資料庫
         */
        public void overideDatabase(StockCategories stockCategoriesLocal,
            CreateCompanyCategoriesCallback callback)
        {
            companyCategoriesString = "";
            createCompanyCategoriesCallback = callback;
            stockCategories = stockCategoriesLocal;
            companyCategoriesList = new List<CompanyCategory>();
            if (stockCategories == null)
            {
                stockCategories = new StockCategories();
                stockCategories.createDatabase(onStockCategoriesCreated);
            }
            else
            {
                onStockCategoriesCreated();
            }
        }
        /*
            函式 createDatabase(stockCategories, callback)
            用以建立各公司所屬類別的資料庫，資料庫檔案是 database\companyCategories.dat
            檔案中，檔案中每一家公司佔一筆記錄，格式是
                公司代號  公司名稱  所屬類別\n
            並建立此物件的內部資料：
                totalCompanies   : 公司總數
                companyIds       : 公司代號陣列
                companyNames     : 公司名稱陣列
                companyCategories  : 公司所屬類別陣列
         */
        public void createDatabase(StockCategories stockCategoriesLocal,
            CreateCompanyCategoriesCallback callback)
        {
            createCompanyCategoriesCallback = callback;
            stockCategories = stockCategoriesLocal;
            companyCategoriesList = new List<CompanyCategory>();
            if (fileHelper.Exists("companyCatogories.dat"))
            {
                String companyDataString = fileHelper.ReadText("companyCatogories.dat");
                String[] companyDataStringSplit = companyDataString.Split(
                    new String[] { "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
                totalCompanies = companyDataStringSplit.Length;
                Array.Sort(companyDataStringSplit);
                companyCategoriesList = new List<CompanyCategory>();
                for (int i = 0; i < totalCompanies; i++)
                {
                    String oneCompany = companyDataStringSplit[i];
                    String[] oneCompanySplit = oneCompany.Split(
                        new char[] { ' ' }
                        );
                    CompanyCategory companyCategory = new CompanyCategory();
                    companyCategory.id = oneCompanySplit[0];
                    companyCategory.name = oneCompanySplit[1];
                    companyCategory.category = oneCompanySplit[2];
                    companyCategoriesList.Add(companyCategory);
                }
                companyCategories = companyCategoriesList.ToArray();
                createCompanyCategoriesCallback();
            }
            else
            {
                overideDatabase(stockCategories, createCompanyCategoriesCallback);
            }
        }
        /*
            函式 getTotalCompanies(callback)
            用來取得所有公司的總數，由於調用此方法時，公司總數未必已經取得，仍然可能
            需要從網路或檔案中得到所有公司資料陣列，因此要用非同步的方式回調 callback，
            並傳回 totalCompany 之值。
         */
        public void getTotalCompanies(GetTotalCompaniesCallback callback)
        {
            if (totalCompanies == 0)
            {   // 尚未建立資料庫，呼叫 createDatabase 建立資料庫
                createDatabase(
                    null,
                    () =>   // 資料庫建立完畢會呼叫的回呼函式
                    {
                        callback(totalCompanies);
                    }
                );
            }
            else
            {   // 資料庫已存在，直接回呼
                callback(totalCompanies);
            }
        }
        /*
            函式 getCompanyInfo(index, callback)
            用來取得第 index 個公司的資料物件，如下：
                {
                    id : 公司代號,
                    name : 公司名稱,
                    category : 公司所屬類股名稱
                }
         */
        public void getCompanyInfo(int index,
            GetCompanyInfoCallback callback)
        {
            if (totalCompanies == 0)
            {   // 尚未建立資料庫，呼叫 createDatabase 建立資料庫
                createDatabase(
                    null,
                    () =>   // 資料庫建立完畢會呼叫的回呼函式
                    {
                        callback(companyCategories[index]);
                    }
                );
            }
            else
            {   // 資料庫已存在，直接回呼
                callback(companyCategories[index]);
            }
        }
    }
}
