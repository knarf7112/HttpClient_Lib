using System;
using System.Collections.Generic;
//
using System.Net;
using System.IO;
using System.Diagnostics;

namespace WebHttpClient
{
    /// <summary>
    /// 使用WebRequest
    /// </summary>
    public class Client
    {
        /// <summary>
        /// 設定Uri,與資料並送出Request後等待Response
        /// </summary>
        /// <param name="uriString">目的地Uri</param>
        /// <param name="method">Get/Post</param>
        /// <param name="sendData">null(Get)/bytes(Post)</param>
        /// <param name="debugDisplay"></param>
        /// <param name="credential">認證用(default:null)</param>
        /// <param name="timeOut">要求逾時</param>
        /// <param name="proxyString"></param>
        /// <returns>回應資料(Byte Array)</returns>
        public static byte[] GetResponse(string uriString, string method,byte[] sendData,bool debugDisplay = false, ICredentials credential = null, int timeOut = 10000, string proxyString = null)
        {
            #region variable
            WebRequest request = null;
            Stream dataStream = null;
            WebResponse response = null;
            byte[] result = null;
            Queue<byte> buffer = null;
            int readByte = -1;
            #endregion

            try
            {
                // 1.create request and setting timeout, credential, proxy(if has)
                request = CreateWebRequest(uriString, timeOut, credential, proxyString);
                // 2.setting request method and send data(if has)
                SetReuqestData(request, method, sendData);

                // 3.等待並取得Server回應
                response = request.GetResponse();
                dataStream = response.GetResponseStream();

                // 檢視Request和Response內的屬性數據
                if (debugDisplay)
                {
                    ReflectionAllPropertyValue(request);
                    ReflectionAllPropertyValue(response);
                }
                buffer = new Queue<byte>();

                // 4.將Response的數據讀出並輸出
                while ((readByte = dataStream.ReadByte()) > -1)
                {
                    buffer.Enqueue((byte)readByte);
                } 
                result = buffer.ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Response Error:" + ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                // 5.關閉Response連線
                dataStream.Close();
                response.Close();
            }
            return result;
        }

        /// <summary>
        /// Create WebRequest by UriString and set timeout(ms), credential, proxy
        /// </summary>
        /// <param name="uriString">destination Uri</param>
        /// <param name="timeOut">逾時(ms)</param>
        /// <param name="credential">Web認證(如果有)</param>
        /// <param name="proxyString">代理(如果有)</param>
        /// <returns>WebRequest Object</returns>
        private static WebRequest CreateWebRequest(string uriString, int timeOut, ICredentials credential, string proxyString)
        {
            WebRequest request = null;

            //ref:http://www.dotblogs.com.tw/jaigi/archive/2012/09/29/75169.aspx
            request = WebRequest.Create(uriString);
            //time out setting
            request.Timeout = timeOut;
            //認證用
            request.Credentials = credential;//new NetworkCredential("ID","Password","Domain Name") ;

            //proxy setting 
            if (proxyString != null)
            {
                request.Proxy = new WebProxy(proxyString);
            }

            return request;
        }

        /// <summary>
        /// Setting WebRequest Medthod, ContentType, ContentLength, UserAgent, 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="method"></param>
        /// <param name="sendData"></param>
        private static void SetReuqestData(WebRequest request,string method,byte[] sendData)
        {
            Stream dataStream = null;
            //設定Method
            request.Method = method.ToUpper();
            //設定content內容長度
            request.ContentLength = (sendData == null) ? 0 : sendData.Length;
            //設定Client端Agent
            ((HttpWebRequest)request).UserAgent = "4+ Client";

            
            switch (method.ToUpper())
            {
                case "POST":
                    request.ContentType = "application/x-www-form-urlencoded";//"POST" ContentType
                    try
                    {
                        //Request資料寫入
                        dataStream = request.GetRequestStream();
                        dataStream.Write(sendData, 0, sendData.Length);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Request寫入異常:" + ex.Message + " \n" + ex.StackTrace);
                    }
                    finally
                    {
                        dataStream.Close();
                    }
                    break;
                case "GET":
                    request.ContentType = "text/plain";
                    break;
                default:
                    throw new Exception("Method Invalid:" + method);
            }
        }

        /// <summary>
        /// 用來看進入物件的所有有數據的屬性名稱與值
        /// </summary>
        /// <param name="obj">Reflected object</param>
        private static void ReflectionAllPropertyValue(object obj)
        {
            try
            {
                int count = 0;
                //get all properties from object
                System.Reflection.PropertyInfo[] properties = obj.GetType().GetProperties();

                foreach (var property in properties)
                {
                    object[] index = null;
                    //if the property is indexer and has length {ex: System.String [Item]}
                    //ref:http://stackoverflow.com/questions/6156577/targetparametercountexception-when-enumerating-through-properties-of-string
                    if (property.GetIndexParameters().Length > 0)
                    {
                        index = new object[] { "0" };
                    }
                    //get property value and casting to string
                    string value = property.GetValue(obj, index) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        //display on debug console
                        System.Diagnostics.Debug.WriteLine(count++ + ":" + property.Name + ":" + value);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Reflection Error:" + ex.StackTrace);
            }
        }
    }
}
