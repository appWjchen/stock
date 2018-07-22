using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace stock
{
    public delegate void showTextBoxMessageCallback(String text);
    public delegate void appendTextBoxMessageCallback(String text, bool endPosition);
    public partial class Form1 : Form
    {

        /* StockCategories stockCategories = new StockCategories();
        CompanyCategories companyCategories = new CompanyCategories(); */
        StockDatabase stockDatabase = null;
        Boolean createTwiceAttackFiles;
        Boolean isDebug = false;

        public void showTextBoxWarning(String text)
        {
            if (this.textBox1.InvokeRequired)
            {
                showTextBoxMessageCallback d = new showTextBoxMessageCallback(showTextBoxWarning);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox2.Text = text;
            }
        }
        public void appendTextBoxWarning(String text, bool endPosition)
        {
            if (this.textBox1.InvokeRequired)
            {
                appendTextBoxMessageCallback d = new appendTextBoxMessageCallback(appendTextBoxWarning);
                this.Invoke(d, new object[] { text, endPosition });
            }
            else
            {
                if (endPosition)
                {
                    textBox2.Text = textBox2.Text + text;
                }
                else
                {
                    textBox2.Text = text + textBox2.Text;
                }
            }
        }
        public void showTextBoxMessage(String text)
        {
            if (this.textBox1.InvokeRequired)
            {
                showTextBoxMessageCallback d = new showTextBoxMessageCallback(showTextBoxMessage);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text = text;
            }
        }
        public void appendTextBoxMessage(String text, bool endPosition)
        {
            if (this.textBox1.InvokeRequired)
            {
                appendTextBoxMessageCallback d = new appendTextBoxMessageCallback(appendTextBoxMessage);
                this.Invoke(d, new object[] { text, endPosition });
            }
            else
            {
                if (endPosition)
                {
                    textBox1.Text = textBox1.Text + text;
                }
                else
                {
                    textBox1.Text = text + textBox1.Text;
                }
            }
        }
        private void disableAllButtons()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
            button10.Enabled = false;
            button11.Enabled = false;
            button12.Enabled = false;
            button13.Enabled = false;
        }
        private void enableAllButtons()
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = true;
            button11.Enabled = true;
            button12.Enabled = true;
            button13.Enabled = true;
        }
        public void stockDatabaseCreateDatabaseCallback()
        {
            if (this.button1.InvokeRequired)
            {
                CreateStockDatabaseCallback d =
                    new CreateStockDatabaseCallback(stockDatabaseCreateDatabaseCallback);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if (createTwiceAttackFiles)
                {
                    /*
                    var analysisObj = new analysis(stockDatabase);
                    textBox1.Text = "更新建立二次攻擊檔案。\r\n\r\n";
                    analysisObj.doAttackAnalysis(300, true);
                     */
                    button6.Enabled = true;
                }
                else
                {
                    enableAllButtons();
                    textBox1.Text = "更新大盤資料庫完畢，可開始使用。\r\n\r\n";
                    var mesg = "資料庫更新週期：\r\n" +
                        "\t各公司歷史資料庫 => 每天一次\r\n" +
                        "\t各公司基本資料庫 => 每季一次(建議 2,5,8,11 月)\r\n" +
                        "\t各公司每月營收資料庫 => 每月一次(建議每月20日)\r\n" +
                        "\t各公司每年股利資料庫 => 每年一次(建議每年的 5,6 月)\r\n" +
                        "\r\n" +
                        "使用方式：\r\n" +
                        "\t(A) 「更新各公司歷史資料庫」 --> 「分析及篩選」\r\n" +
                        "\t(B) 或者，「更新各公司歷史資料庫」 --> 「更新各公司基本資料庫(可選)」 --> 「更新各公司每月營收資料庫(可選)」 --> 「更新各公司每年股利資料庫(可選)」 --> 「分析及篩選」\r\n" +
                        "\t(C) 或者，「更新所有資料庫」 --> 「分析及篩選」\r\n" +
                        "\t必要時：\r\n" +
                        "\t(D) 「 重設分類資料庫」 --> 「更新所有資料庫」 --> 「分析及篩選」\r\n" +
                        "\t\t必要的意思是指有公司下市或新公司上市。" +
                        "\r\n" +
                        "注意事項：\r\n" +
                        "\t(A) 「更新所有資料庫」=「更新各公司歷史資料庫」+「更新各公司基本資料庫」+「更新各公司每月營收資料庫」+「更新各公司每年股利資料庫」\r\n" +
                        "\t(B) 更新資料庫時，可能會有失敗的情形，例如 index 值停止不動，程式類似卡住，原因有：\r\n" +
                        "\t\t(1) index 公司下市，無法取得資料了。\r\n" +
                        "\t\t(2) 網路斷線。\r\n" +
                        "\t(C) 更新資料庫失敗時的處理方法：\r\n" +
                        "\t\t(1) 重啟程式，重新按正常操作再跑一次，如果還是會卡住，則按下列步驟處理。\r\n" +
                        "\t\t(1) 重啟程式，按「使用方式」的 (D) 項操作。\r\n" +
                        "\t\t(2) 或者，寫下卡住股票的公司代號，重啟程式，將公司代號輸入，按下「依公司代號刪除」按鈕，重啟程式，按「使用方式」的 (A) 項操作\r\n" +
                        "\t\t若按照更新失敗的處置方法去做，仍然無法排除問題，請回報卡住時的 index 值及公司名稱\r\n" +
                        "\t(D) 不要使用其它的按鈕選項，除非你知道它的作用。\r\n" +
                        "\r\n"
                        ;
                    textBox1.Text = textBox1.Text + mesg;
                }
            }
        }
        public void emptyCallback()
        {
            if (this.button1.InvokeRequired)
            {
                CreateHistoryDatabaseCallback d =
                    new CreateHistoryDatabaseCallback(emptyCallback);
                this.Invoke(d, new object[] { });
            }
            else
            {
                enableAllButtons();
            }

        }
        public Form1()
        {
#if DEBUG
            isDebug = true;
#endif
            InitializeComponent();
            if (!isDebug)
            {
                button7.Hide();
                button9.Hide();
                button10.Hide();
            }
            /* 檢查資料庫路徑是否設定 */
            createTwiceAttackFiles = false;
            if (FileHelper.databasePath == null)
            {
                FileHelper fileHelper = new FileHelper();
                var myDocumentPath = fileHelper.GetDocsPath();
                var databasePathFilename = myDocumentPath + "/databasePath.dat";
                if (fileHelper.Exists(databasePathFilename))
                {
                    FileHelper.databasePath = "";
                    var databasePath = fileHelper.ReadText(databasePathFilename);
                    FileHelper.databasePath = databasePath + "\\";
                }
                else
                {
                    FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
                    folderBrowserDialog1.Description = "請選擇股票貨料庫目錄((可以由 database.zip 解出得到)，\r\n若沒有資料庫目錄，可以「建立新資料夾」或「取消」。";

                    if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var databasePath = folderBrowserDialog1.SelectedPath;
                        FileHelper.databasePath = "";
                        fileHelper.WriteText(databasePathFilename, databasePath);
                        FileHelper.databasePath = databasePath + "\\";
                        if (!fileHelper.Exists("companyCatogories.dat"))
                        {
                            createTwiceAttackFiles = true;
                        }
                    }
                    else
                    {
                        // new CloseApplication().closeApplication();
                        var databasePath = myDocumentPath + "\\database";
                        FileHelper.databasePath = "";
                        fileHelper.WriteText(databasePathFilename, databasePath);
                        FileHelper.databasePath = databasePath + "\\";
                        createTwiceAttackFiles = true;
                    }
                }
            }
            /* 開始檢查並建立資科庫 */
            stockDatabase = new StockDatabase();
            textBox1.Text = "開始更新大盤資料庫，請稍待。";
            disableAllButtons();
            stockDatabase.createDatabase(stockDatabaseCreateDatabaseCallback);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            int startIndex = Convert.ToInt32(textBox3.Text);
            textBox1.Text = "開始更新各公司歷史資料庫，請稍待。";
            disableAllButtons();
            stockDatabase.createAllCompanyHistoryDatabase(startIndex,
                () =>
                {
                    new MessageWriter().showMessage("更新各公司歷史資料庫完畢。");
                    emptyCallback();
                }
            );
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int startIndex = Convert.ToInt32(textBox3.Text);
            textBox1.Text = "開始更新各公司基本資料庫，請稍待。";
            disableAllButtons();
            stockDatabase.createAllCompanyInformationDatabase(startIndex,
                () =>
                {
                    new MessageWriter().showMessage("更新各公司基本資料庫完畢。");
                    emptyCallback();
                }
            );
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int startIndex = Convert.ToInt32(textBox3.Text);
            textBox1.Text = "開始更新各公司每月營收資料庫，請稍待。";
            disableAllButtons();
            stockDatabase.createAllCompanyMonthEarningDatabase(startIndex,
                () =>
                {
                    new MessageWriter().showMessage("更新各公司每月營收資料庫完畢。");
                    emptyCallback();
                }
            );
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int startIndex = Convert.ToInt32(textBox3.Text);
            textBox1.Text = "開始更新各公司每年股利資料庫，請稍待。";
            disableAllButtons();
            stockDatabase.createAllCompanyDevidendDatabase(startIndex,
                () =>
                {
                    new MessageWriter().showMessage("更新各公司每年股利資料庫完畢。");
                    emptyCallback();
                }
            );

            /* 股票資本額排名測試
            stockDatabase.sortCompanyByCapital();
            String printText = "";
            for (int i = 0; i < stockDatabase.companies.Length; i++)
            {
                printText = printText + stockDatabase.companies[i].id + "\t" +
                    stockDatabase.companies[i].capital + "\r\n";
            }
            new MessageWriter().showMessage(printText);
             */
        }

        private void button5_Click(object sender, EventArgs e)
        {   // 重設分類資料庫
            FileHelper fileHelper = new FileHelper();
            fileHelper.Delete("companyCatogories.dat");
            fileHelper.Delete("stockCategories.dat");
            textBox1.Text = "開始更新大盤資料庫，請稍待。";
            disableAllButtons();
            stockDatabase.createDatabase(stockDatabaseCreateDatabaseCallback);
        }

        private void button6_Click(object sender, EventArgs e)
        {   // 更新所有資料庫
            int startIndex = Convert.ToInt32(textBox3.Text);
            textBox1.Text = "開始更新所有公司的各項資料庫，請稍待(很久)。";
            disableAllButtons();
            stockDatabase.createAllCompaniesDatabase(startIndex,
                () =>
                {
                    if (createTwiceAttackFiles)
                    {

                        var analysisObj = new analysis(stockDatabase);
                        new MessageWriter().showMessage("建立二次攻擊檔案中，請稍等。\r\n\r\n");
                        analysisObj.doAttackAnalysis(300, 1, true);

                    }
                    new MessageWriter().showMessage("更新所有公司的各項資料庫完畢。");
                    emptyCallback();
                }
            );
        }

        private void button7_Click(object sender, EventArgs e)
        {
            /* 測試 */
            var printText = "";
            /*
            HistoryData[] historyData = stockDatabase.companies[1].getRealHistoryDataArray("d");
            List<HistoryData> historyData80List = new List<HistoryData>();
            int historyDataLength = historyData.Length;
            for (int i = 0; i < 79; i++)
            {
                historyData80List.Add(historyData[historyDataLength - 79 + i]);
            }
            HistoryData[] historyData80 = historyData80List.ToArray();
            Indicator indicator = new Indicator(historyData80);

            // MACD 測試
            MACD[] macd = indicator.calcYahooMACD();

            String printText = stockDatabase.companies[1].name + "\r\n";
            for (int i = 0; i < macd.Length; i++)
            {
                printText = printText + i.ToString() + "\t" + historyData80[i].t +
                    "\t\tema12=" + macd[i].ema12.ToString("f5") +
                    "\t\tema26=" + macd[i].ema26.ToString("f5") +
                    "\t\tdiff9=" + macd[i].diff9.ToString("f5") +
                    "\t\tmacd=" + macd[i].macd.ToString("f5") + "\r\n";
            }
            */
            // Double[] rsvArray = indicator.calcRsvArray(9);
            /*String printText = stockDatabase.companies[1].name + "\r\n";
            for (int i = 0; i < rsvArray.Length; i++)
            {
                printText = printText + i.ToString() + "\t" + historyData80[i].t +
                    "\trsv=" + rsvArray[i].ToString("f5") + "\r\n";
            }
            */
            // KDJ[] kdjArray = indicator.calcKDJArray(rsvArray);
            /*
            String printText = stockDatabase.companies[1].name + "\r\n";
            for (int i = 0; i < kdjArray.Length; i++)
            {
                printText = printText + i.ToString() + "\t" + historyData80[i].t +
                    "\tK=" + kdjArray[i].K.ToString("f5") +
                    "\tD=" + kdjArray[i].D.ToString("f5") +
                    "\tJ=" + kdjArray[i].J.ToString("f5") +
                    "\r\n";
            }
            */
            /*
            Double[] rsiArray = indicator.calcRsiArray(5);
            String printText = stockDatabase.companies[1].name + "\r\n";
            for (int i = 0; i < kdjArray.Length; i++)
            {
                printText = printText + i.ToString() + "\t" + historyData80[i].t +
                    "\trsiArray=" + rsiArray[i].ToString("f5") +
                    "\r\n";
            }
            */
            /*
            String printText = "大盤:\r\n";
            HistoryData[] historyDataArray = stockDatabase.getDayHistoryData();
            Indicator indicator = new Indicator(historyDataArray);
            StockIndicator[] stockIndicaotr = indicator.getStockIndicatorArray(80);
            for (int i = 0; i < stockIndicaotr.Length; i++)
            {
                printText = printText + i.ToString() + "\t" + stockIndicaotr[i].time +
                    "\tK=" + stockIndicaotr[i].K.ToString("f5") +
                    "\tD=" + stockIndicaotr[i].D.ToString("f5") +
                    "\tJ=" + stockIndicaotr[i].J.ToString("f5") +
                    "\r\n";
            }
            */
            /* 資本排名測試
            printText = "";
            stockDatabase.sortCompanyByCapital();
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                printText = printText + stockDatabase.companies[i].name + "\r\n";
            }
             * */
            CompanyInformation[] companyInformationArray = stockDatabase.companies[0].getMarginInformation();
            CompanyInformation companyInformation = companyInformationArray[companyInformationArray.Length - 1];
            printText = printText + stockDatabase.companies[0].name + "\r\n" +
                stockDatabase.companies[0].id + "\r\n" +
                companyInformation.bookValuePerShare;
            new MessageWriter().showMessage(printText);
        }
        // analysis analysisObj = null;
        private void button8_Click(object sender, EventArgs e)
        {
            disableAllButtons();
            new MessageWriter().showMessage("開始進行分析及篩選，請耐心等候。");
            System.Windows.Forms.Application.DoEvents();
            var analysisObj = new analysis(stockDatabase);
            var printText = "";
            printText = analysisObj.doShort1ScoreAnalysis(80);
            printText = analysisObj.getSavedAnalysisScore(20) + "\r\n\r\n" + printText;
            printText = analysisObj.doFindCandidate() + printText;
            new MessageWriter().showMessage(printText);
            enableAllButtons();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            disableAllButtons();
            new MessageWriter().showMessage("開始二次攻擊測試，請耐心等候。");
            System.Windows.Forms.Application.DoEvents();
            var analysisObj = new analysis(stockDatabase);
            new MessageWriter().showMessage("");
            analysisObj.doAttackAnalysis(300, 300, false);
            new MessageWriter().appendMessage(
                    "\r\n",
                    true
                    );
            enableAllButtons();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            disableAllButtons();
            new MessageWriter().showMessage("最近十天有發動二次攻擊的股票：\r\n");
            FileHelper fileHelper = new FileHelper();
            for (int i = 0; i < stockDatabase.companies.Length; i++)
            {
                Company company = stockDatabase.companies[i];
                var filename = "company/" + company.id + "/twiceAttack.dat";
                if (fileHelper.Exists(filename))
                {
                    var text = fileHelper.ReadText(filename);
                    int Year = Convert.ToInt32(text.Substring(0, 4));
                    int Month = Convert.ToInt32(text.Substring(5, 2));
                    int Day = Convert.ToInt32(text.Substring(8, 2));
                    var newDate = new DateTime(Year, Month, Day);
                    var diff = (DateTime.Now - newDate).TotalDays;
                    if (diff < 10)
                    {
                        new MessageWriter().appendMessage(
                            company.name + "(" + company.id + ") " + text,
                            true
                            );
                    }
                }
            }
            enableAllButtons();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            disableAllButtons();
            var id = textBox4.Text;
            Company company = stockDatabase.getCompany(id);
            /*
            for (var i = 0; i < stockDatabase.companies.Length; i++)
            {
                if (stockDatabase.companies[i].id == id)
                {
                    company = stockDatabase.companies[i];
                    for (int a = i; a < stockDatabase.companies.Length - 1; a++)
                    {
                        stockDatabase.companies[a] = stockDatabase.companies[a + 1];
                    }
                    Array.Resize(ref stockDatabase.companies, stockDatabase.companies.Length - 1);
                    break;
                }
            }
            */
            if (company != null)
            {
                FileHelper fileHelper = new FileHelper();
                String allCompanyString = "";
                for (int i = 0; i < stockDatabase.companies.Length; i++)
                {
                    allCompanyString = allCompanyString + stockDatabase.companies[i].id
                        + " " + stockDatabase.companies[i].name + " " +
                        stockDatabase.companies[i].category + "\n";
                }
                fileHelper.WriteText("companyCatogories.dat", allCompanyString);
            }
            else
            {
                new MessageWriter().showMessage("找不到公司代號是 " + id + " 的股票!\r\n");
            }
            enableAllButtons();
        }
        private void button12_Click(object sender, EventArgs e)
        {
            disableAllButtons();
            var id = textBox4.Text;
            Company company = stockDatabase.getCompany(id);
            if (company != null)
            {
                new MessageWriter().showMessage("");
                String information = company.getInformatioon(stockDatabase);
                new MessageWriter().appendMessage(information + "\r\n", true);
                information = stockDatabase.getInformation();
                new MessageWriter().appendMessage(information, true);
            }
            else
            {
                new MessageWriter().showMessage("找不到公司代號是 " + id + " 的股票!\r\n");
            }
            enableAllButtons();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            disableAllButtons();
            new MessageWriter().showMessage("重置二次攻擊資料，請耐心等候。");
            System.Windows.Forms.Application.DoEvents();
            var analysisObj = new analysis(stockDatabase);
            new MessageWriter().showMessage("");
            analysisObj.doAttackAnalysis(300, 1, false);
            new MessageWriter().appendMessage(
                    "\r\n",
                    true
                    );
            enableAllButtons();
        }
    }
}
