using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Net;
/*
 * 此類別在 Windows.Form 中的使用方法大致如下：
 * getHttpSourceCallback 是一個回呼函式，在呼叫
 * HttpHelper.getHttpSource 後，會呼叫此回呼函式，
 * 並將網頁內容 httpSource 傳給該回呼函式。

        public void getHttpSourceCallback(String httpSource)
        {
            if (textBox1.InvokeRequired)
            {
                GetHttpSourceCallback d = new GetHttpSourceCallback(getHttpSourceCallback);
                this.Invoke(d, new object[] { httpSource });
            }
            else
            {
                textBox1.Text = httpSource;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            HttpHelper httpHelper = new HttpHelper();
            httpHelper.getHttpSource("https://tw.stock.yahoo.com/h/getclass.php", "utf-8", getHttpSourceCallback);
        }
*/
namespace stock
{
    public delegate void GetHttpSourceCallback(String httpSource);

    class HttpHelper
    {
        String httpSource;
        WebRequest webRequest;
        String httpEncoding;
        GetHttpSourceCallback getHttpSourceCallback;

        private void webRequestCallback(IAsyncResult result)
        {
            try
            {
                Stream stream = webRequest.EndGetResponse(result).GetResponseStream();
                StreamReader streamReader = new StreamReader(
                    stream, System.Text.Encoding.GetEncoding(httpEncoding)
                    );
                httpSource = streamReader.ReadToEnd();
            }
            catch (Exception exception)
            {
                httpSource = exception.Message;
            }
            getHttpSourceCallback(httpSource);
        }

        public void getHttpSource(String url, String encoding, GetHttpSourceCallback callback)
        {
            httpSource = "";
            Uri uri = new Uri(url);
            httpEncoding = encoding;
            getHttpSourceCallback = callback;
            webRequest = WebRequest.Create(uri);
            webRequest.BeginGetResponse(webRequestCallback, null);
        }
    }
}
