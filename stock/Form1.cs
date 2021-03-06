﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

/*
 * 股票分析及選程式(stock.sln)大約是開始於 2014 年尾，原本是用 JavaScript 語言在
 * Node.js + Electron 平台上開發，大約在 2017 年中移植到 C# 語言，改變開發語言的
 * 最主要原因在於除錯的方便性，Visual Studio 2010 就可以有不錯的開發環境了，加上
 * C# Windows 程式要移植到 Android 及 iOS 也很容易，所以不再用 JavaScript 語言做
 * 開發了。
 * 專案還有一個 Visual Studio 2018 版本的方案，裡面維持了 Windows, Android 及 iOS
 * 的開發相容函式及專案，原本在開發時會保持各專案的相容性，讓程式可以在 Android 上
 * 順利執行，但在 2018 年中開始全力用 Windows 專案來開發，以加快完成速度。
 * 2018年9月實際可用來分析及篩選股票(含追踪系統)的可運行版本大致完成，由於還有更多
 * 的想法要測試，所以仍然持續開發中。
 *      22018.09.10     開始波段絕對極值法的統計分析及開發
 */
namespace stock
{
    public delegate void showTextBoxMessageCallback(String text);
    public delegate void appendTextBoxMessageCallback(String text, bool endPosition);
    public partial class Form1 : Form
    {

        /* StockCategories stockCategories = new StockCategories();
        CompanyCategories companyCategories = new CompanyCategories(); */
        StockDatabase stockDatabase = null;
        Trace stockTrace = null;
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
            button14.Enabled = false;
            button15.Enabled = false;
            button16.Enabled = false;
            button17.Enabled = false;
            button18.Enabled = false;
            button19.Enabled = false;
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
            button14.Enabled = true;
            button15.Enabled = true;
            button16.Enabled = true;
            button17.Enabled = true;
            button18.Enabled = true;
            button19.Enabled = true;
            button10.Focus();
        }
        private void clearMessage()
        {
            new MessageWriter().showMessage("");
            new WarningWriter().showMessage("");
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
                stockTrace = new Trace(stockDatabase, listView1, label7);
                stockDatabase.stockTrace = stockTrace;
                if (createTwiceAttackFiles)
                {
                    /*
                    var analysisObj = new analysis(stockDatabase);
                    textBox1.Text = "更新建立二次攻擊檔案。\r\n\r\n";
                    analysisObj.doAttackAnalysis(300, true);
                     */
                    button6.Enabled = true;
                    textBox1.Text = "更新大盤資料庫完畢，請按「更新所有資料庫」繼續。\r\n\r\n";
                }
                else
                {
                    enableAllButtons();
                    clearMessage();
                    textBox1.Text = "更新大盤資料庫完畢，可開始使用。\r\n\r\n";
                    var mesg = "資料庫更新週期：\r\n" +
                        "\t各公司歷史資料庫 => 每天一次\r\n" +
                        "\t各公司基本資料庫 => 每季一次(建議 2,5,8,11 月)\r\n" +
                        "\t各公司每月營收資料庫 => 每月一次(建議每月20日)\r\n" +
                        "\t各公司每年股利資料庫 => 每年一次(建議每年的 5,6 月)\r\n" +
                        "\r\n" +
                        "使用方式：\r\n" +
                        "\t(A) 「更新各公司歷史資料庫」 --> 「分析及篩選」\r\n" +
                        "\t(B) 或者，「更新各公司歷史資料庫」 --> 「更新各公司基本資料庫(可選)」 --> 「更新各公司每月營收資料庫(可選)」\r\n" +
                        "\t        --> 「更新各公司每年股利資料庫(可選)」 --> 「分析及篩選」\r\n" +
                        "\t(C) 或者，「更新所有資料庫」 --> 「分析及篩選」\r\n" +
                        "\t必要時：\r\n" +
                        "\t(D) 「 重設分類資料庫」 --> 「更新所有資料庫」 --> 「分析及篩選」\r\n" +
                        "\t        必要的意思是指有公司下市或新公司上市。" +
                        "\r\n" +
                        "注意事項：\r\n" +
                        "\t(A) 「更新所有資料庫」=「更新各公司歷史資料庫」+「更新各公司基本資料庫」+「更新各公司每月營收資料庫」\r\n" +
                        "\t        +「更新各公司每年股利資料庫」\r\n" +
                        "\t(B) 更新資料庫時，可能會有失敗的情形，例如 index 值停止不動，程式類似卡住，原因有：\r\n" +
                        "\t\t(1) index 公司下市，無法取得資料了。\r\n" +
                        "\t\t(2) 網路斷線。\r\n" +
                        "\t(C) 更新資料庫失敗時的處理方法：\r\n" +
                        "\t\t(1) 重啟程式，重新按正常操作再跑一次，如果還是會卡住，則按下列步驟處理。\r\n" +
                        "\t\t(1) 重啟程式，按「使用方式」的 (D) 項操作。\r\n" +
                        "\t\t(2) 或者，寫下卡住股票的公司代號，重啟程式，將公司代號輸入，按下「依公司代號刪除」按鈕，重啟程式，\r\n" +
                        "\t        按「使用方式」的 (A) 項操作\r\n" +
                        "\t\t若按照更新失敗的處置方法去做，仍然無法排除問題，請回報卡住時的 index 值及公司名稱\r\n" +
                        "\t(D) 不要使用其它的按鈕選項，除非你知道它的作用。\r\n" +
                        "\r\n"
                        ;
                    textBox1.Text = textBox1.Text + mesg;
                }
                button10.Focus();
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
            _desiredLocation = new Point(20, 20);
            listView1.FullRowSelect = true;
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
                    clearMessage();
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
                    clearMessage();
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
                    clearMessage();
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
                    clearMessage();
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
                        clearMessage();
                        var analysisObj = new analysis(stockDatabase);
                        new MessageWriter().showMessage("建立二次攻擊檔案中，請稍等。\r\n\r\n");
                        analysisObj.doAttackAnalysis(300, 1, true);

                    }
                    clearMessage();
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
            Company company = stockDatabase.getCompany("1227");
            company.createInfomationDatabase(
                () =>
                {
                    CompanyInformation[] companyInformationArray = company.getMarginInformation();
                    CompanyInformation companyInformation = companyInformationArray[companyInformationArray.Length - 1];
                    printText = printText + company.name + "\r\n" +
                        company.id + "\r\n" +
                        companyInformation.bookValuePerShare;
                    new MessageWriter().showMessage(printText);
                }
                );

            /*
            EarningInformation[] earningInformation = stockDatabase.companies[0].getEarning();
            if (earningInformation.Length > 0)
            {
                EarningInformation lastEaringInformation = earningInformation[earningInformation.Length - 1];
                printText = printText + lastEaringInformation.year + " 年度每月營收：\r\n";
                printText = printText + "\t月份\t營收(元)\t\t與去年比較營收增長(%)\r\n";
                for (var i = 0; i < lastEaringInformation.earningString.Length; i++)
                {
                    if (i >= 12) break;
                    if (lastEaringInformation.earningString[i] != "-")
                    {
                        printText = printText + "\t" + (i + 1) + "月\t"
                            + lastEaringInformation.earningString[i] +
                            "\t\t" + lastEaringInformation.increasePercentCompareToLastYear[i].ToString("f2") +
                            "\r\n";
                    }
                }
                new MessageWriter().showMessage(printText);
            }
            */
        }
        // analysis analysisObj = null;
        private void button8_Click(object sender, EventArgs e)
        {
            disableAllButtons();
            new MessageWriter().showMessage("開始進行分析及篩選，請耐心等候。");
            System.Windows.Forms.Application.DoEvents();
            var analysisObj = new analysis(stockDatabase);
            clearMessage();
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
            int test = 3;
            if (test == 1)
            {
                /* 此段程式用來檢查單一公司的波段搜尋結果是否正確 */
                LipAnalysis lipAnalysis = new LipAnalysis(stockDatabase);
                Company company = stockDatabase.getCompany("1786");
                company.lipHipDataList = lipAnalysis.findLipHipData(company.getRealHistoryDataArray("m"), 60);
                var lipHipDataList = company.lipHipDataList;
                var msgText = "";
                msgText = msgText + company.name + "(" + company.id + ")極值搜尋:\r\n";
                for (var i = 0; i < lipHipDataList.Count(); i++)
                {
                    LipHipData oneLipHitData = lipHipDataList[i];
                    if (oneLipHitData.type)
                    {
                        // 波段高點 
                        msgText = msgText + "\t波段高點， index = " + oneLipHitData.index +
                            " ,日期 = " + oneLipHitData.date.ToString("yyyy/MM/dd") +
                            " ,高點 = " + oneLipHitData.value +
                            "\r\n";
                    }
                    else
                    {
                        // 波段低點 
                        msgText = msgText + "\t波段低點， index = " + oneLipHitData.index +
                            " ,日期 = " + oneLipHitData.date.ToString("yyyy/MM/dd") +
                            " ,低點 = " + oneLipHitData.value +
                            "\r\n";
                    }
                }
                company.waveDataList = lipAnalysis.findWaveDataList(company.lipHipDataList);
                for (var i = 0; i < company.waveDataList.Count(); i++)
                {
                    WaveData oneWavedata = company.waveDataList[i];
                    if (oneWavedata.type)
                    {
                        // 波段上漲
                        msgText = msgText + "\t波段上漲， 起始日期=" + oneWavedata.startDate.ToString("yyyy/MM/dd") +
                            " ,起始價格 = " + oneWavedata.startPrice.ToString("f2") +
                            " ,結束日期 = " + oneWavedata.endDate.ToString("yyyy/MM/dd") +
                            " ,結束價格 = " + oneWavedata.endPrice.ToString("f2") +
                            " ,漲幅 = " + oneWavedata.diffPercent.ToString("f2") +
                            " ,時間 = " + oneWavedata.diffDays + " 天" +
                            "\r\n";
                    }
                    else
                    {
                        // 波段下跌
                        msgText = msgText + "\t波段下跌， 起始日期=" + oneWavedata.startDate.ToString("yyyy/MM/dd") +
                            " ,起始價格 = " + oneWavedata.startPrice.ToString("f2") +
                            " ,結束日期 = " + oneWavedata.endDate.ToString("yyyy/MM/dd") +
                            " ,結束價格 = " + oneWavedata.endPrice.ToString("f2") +
                            " ,跌幅 = " + oneWavedata.diffPercent.ToString("f2") +
                            " ,時間 = " + oneWavedata.diffDays + " 天" +
                            "\r\n";
                    }
                }
                new MessageWriter().appendMessage(msgText, true);
            }
            else if (test == 2)
            {
                /* 
                 * 此段程式用來做所有股票波段上漲及下跌幅度的統計 
                 * 20181004 結果
                 * 所有個股波幅統計:
                 *      上漲波幅最大百分比=17862.96%
                 *      上漲波幅最小百分比=6.81%
                 *      上漲波幅平均百分比=239.27%
                 *      上漲波幅最長時間=3013天
                 *      上漲波幅最短時間=86天
                 *      上漲波幅平均時間=503天
                 *      下跌波幅最大百分比=99.85%
                 *      下跌波幅最小百分比=7.75%
                 *      下跌波幅平均百分比=60.23%
                 *      下跌波幅最長時間=3167天
                 *      下跌波幅最短時間=85天
                 *      下跌波幅平均時間=578天
                 *      最長上漲時間公司是：佳格(1227)
                 *      最短上漲時間公司是：三芳(1307)
                 *      最大上漲幅度公司是：欣巴巴(9906)
                 *      最小上漲幅度公司是：台新戊特(2887E)
                 *      最長下跌時間公司是：怡華(1456)
                 *      最短下跌時間公司是：永裕(1323)
                 *      最大下跌幅度公司是：吉祥全(2491)
                 *      最小下跌幅度公司是：士電(1503)
                 */
                LipAnalysis lipAnalysis = new LipAnalysis(stockDatabase);
                lipAnalysis.findAllLipHipDataList();
                lipAnalysis.findAllWaveDataList();
                lipAnalysis.findAllWaveStatisticInformation();
                String msgText = "大盤漲幅搜尋:\r\n";
                for (var i = 0; i < stockDatabase.waveDataList.Count(); i++)
                {
                    WaveData oneWavedata = stockDatabase.waveDataList[i];
                    if (oneWavedata.type)
                    {
                        // 波段上漲
                        msgText = msgText + "\t波段上漲， 起始日期=" + oneWavedata.startDate.ToString("yyyy/MM/dd") +
                            " ,起始價格 = " + oneWavedata.startPrice.ToString("f2") +
                            " ,結束日期 = " + oneWavedata.endDate.ToString("yyyy/MM/dd") +
                            " ,結束價格 = " + oneWavedata.endPrice.ToString("f2") +
                            " ,漲幅 = " + oneWavedata.diffPercent.ToString("f2") +
                            " ,時間 = " + oneWavedata.diffDays + " 天" +
                            "\r\n";
                    }
                    else
                    {
                        // 波段下跌
                        msgText = msgText + "\t波段下跌， 起始日期=" + oneWavedata.startDate.ToString("yyyy/MM/dd") +
                            " ,起始價格 = " + oneWavedata.startPrice.ToString("f2") +
                            " ,結束日期 = " + oneWavedata.endDate.ToString("yyyy/MM/dd") +
                            " ,結束價格 = " + oneWavedata.endPrice.ToString("f2") +
                            " ,跌幅 = " + oneWavedata.diffPercent.ToString("f2") +
                            " ,時間 = " + oneWavedata.diffDays + " 天" +
                            "\r\n";
                    }
                }
                msgText = msgText + "大盤波幅統計:\r\n";
                if (stockDatabase.waveStatisticInformation.totalUpCount > 0)
                {
                    msgText = msgText + "\t上漲波幅最大百分比=" + stockDatabase.waveStatisticInformation.maxUpDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t上漲波幅最小百分比=" + stockDatabase.waveStatisticInformation.minUpDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t上漲波幅平均百分比=" + stockDatabase.waveStatisticInformation.averageUpDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t上漲波幅最長時間=" + stockDatabase.waveStatisticInformation.maxUpDiffDate.ToString("f0") + "天\r\n";
                    msgText = msgText + "\t上漲波幅最短時間=" + stockDatabase.waveStatisticInformation.minUpDiffDate.ToString("f0") + "天\r\n";
                    msgText = msgText + "\t上漲波幅平均時間=" + stockDatabase.waveStatisticInformation.averageUpDiffDate.ToString("f0") + "天\r\n";
                }
                if (stockDatabase.waveStatisticInformation.totalDownCount > 0)
                {
                    msgText = msgText + "\t下跌波幅最大百分比=" + stockDatabase.waveStatisticInformation.maxDownDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t下跌波幅最小百分比=" + stockDatabase.waveStatisticInformation.minDownDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t下跌波幅平均百分比=" + stockDatabase.waveStatisticInformation.averageDownDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t下跌波幅最長時間=" + stockDatabase.waveStatisticInformation.maxDownDiffDate.ToString("f0") + "天\r\n";
                    msgText = msgText + "\t下跌波幅最短時間=" + stockDatabase.waveStatisticInformation.minDownDiffDate.ToString("f0") + "天\r\n";
                    msgText = msgText + "\t下跌波幅平均時間=" + stockDatabase.waveStatisticInformation.averageDownDiffDate.ToString("f0") + "天\r\n";
                }
                Company company = stockDatabase.companies[0];
                msgText = msgText + company.name + "漲幅搜尋:\r\n";
                for (var i = 0; i < company.waveDataList.Count(); i++)
                {
                    WaveData oneWavedata = company.waveDataList[i];
                    if (oneWavedata.type)
                    {
                        // 波段上漲
                        msgText = msgText + "\t波段上漲， 起始日期=" + oneWavedata.startDate.ToString("yyyy/MM/dd") +
                            " ,起始價格 = " + oneWavedata.startPrice.ToString("f2") +
                            " ,結束日期 = " + oneWavedata.endDate.ToString("yyyy/MM/dd") +
                            " ,結束價格 = " + oneWavedata.endPrice.ToString("f2") +
                            " ,漲幅 = " + oneWavedata.diffPercent.ToString("f2") +
                            " ,時間 = " + oneWavedata.diffDays + " 天" +
                            "\r\n";
                    }
                    else
                    {
                        // 波段下跌
                        msgText = msgText + "\t波段下跌， 起始日期=" + oneWavedata.startDate.ToString("yyyy/MM/dd") +
                            " ,起始價格 = " + oneWavedata.startPrice.ToString("f2") +
                            " ,結束日期 = " + oneWavedata.endDate.ToString("yyyy/MM/dd") +
                            " ,結束價格 = " + oneWavedata.endPrice.ToString("f2") +
                            " ,跌幅 = " + oneWavedata.diffPercent.ToString("f2") +
                            " ,時間 = " + oneWavedata.diffDays + " 天" +
                            "\r\n";
                    }
                }
                msgText = msgText + "個股波幅統計:\r\n";
                if (company.waveStatisticInformation.totalUpCount > 0)
                {
                    msgText = msgText + "\t上漲波幅最大百分比=" + company.waveStatisticInformation.maxUpDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t上漲波幅最小百分比=" + company.waveStatisticInformation.minUpDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t上漲波幅平均百分比=" + company.waveStatisticInformation.averageUpDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t上漲波幅最長時間=" + company.waveStatisticInformation.maxUpDiffDate.ToString("f0") + "天\r\n";
                    msgText = msgText + "\t上漲波幅最短時間=" + company.waveStatisticInformation.minUpDiffDate.ToString("f0") + "天\r\n";
                    msgText = msgText + "\t上漲波幅平均時間=" + company.waveStatisticInformation.averageUpDiffDate.ToString("f0") + "天\r\n";
                }
                if (company.waveStatisticInformation.totalDownCount > 0)
                {
                    msgText = msgText + "\t下跌波幅最大百分比=" + company.waveStatisticInformation.maxDownDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t下跌波幅最小百分比=" + company.waveStatisticInformation.minDownDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t下跌波幅平均百分比=" + company.waveStatisticInformation.averageDownDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t下跌波幅最長時間=" + company.waveStatisticInformation.maxDownDiffDate.ToString("f0") + "天\r\n";
                    msgText = msgText + "\t下跌波幅最短時間=" + company.waveStatisticInformation.minDownDiffDate.ToString("f0") + "天\r\n";
                    msgText = msgText + "\t下跌波幅平均時間=" + company.waveStatisticInformation.averageDownDiffDate.ToString("f0") + "天\r\n";
                }
                msgText = msgText + "所有個股波幅統計:\r\n";
                if (stockDatabase.waveStatisticInformationAllCompany.totalUpCount > 0)
                {
                    msgText = msgText + "\t上漲波幅最大百分比=" + stockDatabase.waveStatisticInformationAllCompany.maxUpDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t上漲波幅最小百分比=" + stockDatabase.waveStatisticInformationAllCompany.minUpDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t上漲波幅平均百分比=" + stockDatabase.waveStatisticInformationAllCompany.averageUpDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t上漲波幅最長時間=" + stockDatabase.waveStatisticInformationAllCompany.maxUpDiffDate.ToString("f0") + "天\r\n";
                    msgText = msgText + "\t上漲波幅最短時間=" + stockDatabase.waveStatisticInformationAllCompany.minUpDiffDate.ToString("f0") + "天\r\n";
                    msgText = msgText + "\t上漲波幅平均時間=" + stockDatabase.waveStatisticInformationAllCompany.averageUpDiffDate.ToString("f0") + "天\r\n";
                }
                if (stockDatabase.waveStatisticInformationAllCompany.totalDownCount > 0)
                {
                    msgText = msgText + "\t下跌波幅最大百分比=" + stockDatabase.waveStatisticInformationAllCompany.maxDownDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t下跌波幅最小百分比=" + stockDatabase.waveStatisticInformationAllCompany.minDownDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t下跌波幅平均百分比=" + stockDatabase.waveStatisticInformationAllCompany.averageDownDiffPercent.ToString("f2") + "%\r\n";
                    msgText = msgText + "\t下跌波幅最長時間=" + stockDatabase.waveStatisticInformationAllCompany.maxDownDiffDate.ToString("f0") + "天\r\n";
                    msgText = msgText + "\t下跌波幅最短時間=" + stockDatabase.waveStatisticInformationAllCompany.minDownDiffDate.ToString("f0") + "天\r\n";
                    msgText = msgText + "\t下跌波幅平均時間=" + stockDatabase.waveStatisticInformationAllCompany.averageDownDiffDate.ToString("f0") + "天\r\n";
                }
                new MessageWriter().showMessage(msgText);
                enableAllButtons();
            }
            else if (test == 3)
            {
                /* 此段程式用來做長期買點分析演算法的統計 */
                disableAllButtons();
                var printText = "";
                /* (1) 用極值搜尋找出大盤及各公司的波段極值 */
                LipAnalysis lipAnalysis = new LipAnalysis(stockDatabase);
                lipAnalysis.findAllLipHipDataList();
                lipAnalysis.findAllWaveDataList();
                lipAnalysis.findAllWaveStatisticInformation();
                /* (2.1) 找出至少4個波段的股票，第一個極值是極大值的股票，且第二個波段下跌達 70% 的股票 */
                // Boolean[] needAnalysis = new Boolean[stockDatabase.companies.Length];
                for (var i = 0; i < stockDatabase.companies.Length; i++)
                {
                    Company company = stockDatabase.companies[i];
                    // needAnalysis[i] = false;
                    if ((company.waveDataList.Count() >= 4) && 
                        (company.waveDataList[0].type == true) && 
                        (company.waveDataList[1].diffPercent > 70))
                    {
                        // 尋找買點，由下降波段極小值點，向後尋找第一個轉折點(斜率由負轉正)。
                        // needAnalysis[i] = true;
                        var dateMin = company.waveDataList[1].endDate;
                        for (var k = 0; k < company.lipHipDataList.Count(); k++)
                        {
                            var lipPoint = company.lipHipDataList[k];
                            var dateLip = lipPoint.date;
                            if (dateLip > dateMin)
                            {
                                // 買點?!
                                printText = printText +
                                    "\t" + company.name +
                                    "\t(" + company.id + ")" +
                                    "\r\n";
                            }
                        }
                    }
                }
                new MessageWriter().showMessage(printText);
                enableAllButtons();
            }
        }
        /* 
        // 列出10 天內連續二次攻擊的股票
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
        */
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
                clearMessage();
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
            clearMessage();
            new MessageWriter().appendMessage(
                    "\r\n",
                    true
                    );
            enableAllButtons();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_columnWidthChanged(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = listView1.Columns[e.ColumnIndex].Width;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private Point _desiredLocation;
        private void Form1_LocationChanged(object sender, EventArgs e)
        {
            if (this.Location != _desiredLocation)
            {
                this.Location = _desiredLocation;
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            disableAllButtons();
            var id = textBox4.Text;
            Company company = stockDatabase.getCompany(id);
            if (company != null)
            {
                stockTrace.addCompany(company, "N");
            }
            else
            {
                new MessageWriter().showMessage("找不到公司代號是 " + id + " 的股票!\r\n");
            }
            enableAllButtons();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            disableAllButtons();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                String id = item.SubItems[0].Text;
                stockTrace.remove(id);
            }
            enableAllButtons();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stockTrace.save();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            disableAllButtons();
            MessageWriter messageWriter = new MessageWriter();
            messageWriter.showMessage("追踪股票資訊：\r\n");
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                String id = item.SubItems[0].Text;
                Company company = stockDatabase.getCompany(id);
                TraceCompany traceCompany = stockTrace.findTraceCompany(id);
                messageWriter.appendMessage("\t股票代號：" + company.id + "\r\n", true);
                messageWriter.appendMessage("\t股票名稱：" + company.name + "\r\n", true);
                messageWriter.appendMessage("\t股票追踪資訊(含分數意義)：\r\n" , true);
                messageWriter.appendMessage(traceCompany.passScoreTestExplain + "\r\n", true);
                messageWriter.appendMessage(company.getInformatioon(stockDatabase) + "\r\n", true);
                messageWriter.appendMessage(stockDatabase.getInformation() + "\r\n", true);
            }
            enableAllButtons();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            disableAllButtons();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                String id = item.SubItems[0].Text;
                listView1.Items[id].UseItemStyleForSubItems = false;
                for (var k = 0; k < listView1.Items[id].SubItems.Count; k++)
                {
                    listView1.Items[id].SubItems[k].ForeColor = Color.DarkBlue;
                }
                TraceCompany traceCompany = stockTrace.findTraceCompany(id);
                traceCompany.hasBought = true;
                if (traceCompany.upPercent > 0)
                {
                    listView1.Items[traceCompany.id].SubItems[6].ForeColor = Color.Red;
                }
                else if (traceCompany.upPercent <= 0)
                {
                    listView1.Items[traceCompany.id].SubItems[6].ForeColor = Color.Green;
                }
                else
                {
                    listView1.Items[traceCompany.id].SubItems[6].ForeColor = Color.Black;
                }
            }
            enableAllButtons();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            disableAllButtons();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                String id = item.SubItems[0].Text;
                listView1.Items[id].UseItemStyleForSubItems = false;
                for (var k = 0; k < listView1.Items[id].SubItems.Count; k++)
                {
                    listView1.Items[id].SubItems[k].ForeColor = Color.OrangeRed;
                }
                TraceCompany traceCompany = stockTrace.findTraceCompany(id);
                traceCompany.hasBought = false;
                if (traceCompany.upPercent > 0)
                {
                    listView1.Items[traceCompany.id].SubItems[6].ForeColor = Color.Red;
                }
                else if (traceCompany.upPercent <= 0)
                {
                    listView1.Items[traceCompany.id].SubItems[6].ForeColor = Color.Green;
                }
                else
                {
                    listView1.Items[traceCompany.id].SubItems[6].ForeColor = Color.Black;
                }
                listView1.Items[traceCompany.id].SubItems[6].Font = new Font(listView1.Items[traceCompany.id].SubItems[6].Font,
                    listView1.Items[traceCompany.id].SubItems[6].Font.Style | FontStyle.Bold);
            }
            enableAllButtons();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            disableAllButtons();

            enableAllButtons();
        }
    }
}
