using System;
using System.Collections.Generic;
//
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Collections.Specialized;

namespace WebHttpClient
{
    /// <summary>
    /// 使用WebRequest請求(.Net 4.0)
    /// </summary>
    public class Client
    {
        #region Public Method
        /// <summary>
        /// 使用WebRequest發送請求並取得Response(有網站認證介面與Proxy的Uri設定)
        /// </summary>
        /// <param name="uriString">目的地Uri字串</param>
        /// <param name="method">Get/Post</param>
        /// <param name="sendData">null(Get)/bytes(Post)</param>
        /// <param name="errorMsg">異常輸出(default:"")</param>
        /// <param name="debugDisplay"></param>
        /// <param name="credential">認證用(default:null)</param>
        /// <param name="timeOut">請求的回應逾時(ms)</param>
        /// <param name="proxyString"></param>
        /// <param name="requestHeaders">新增請求Header資料</param>
        /// <returns>Response回應資料(Byte Array)</returns>
        public static byte[] GetResponse(string uriString, string method, byte[] sendData, out string errorMsg, ICredentials credential, int timeOut = 10000, NameValueCollection requestHeaders = null, bool debugDisplay = false, string proxyString = null)
        {
            #region variable
            WebRequest request = null;
            Stream dataStream = null;
            WebResponse response = null;
            byte[] result = null;
            Queue<byte> buffer = null;
            int readByte = -1;
            errorMsg = string.Empty;
            #endregion
            
            try
            {
                // 1.create request and setting timeout, credential, proxy(if has)
                request = CreateWebRequest(uriString, timeOut);
                // 2.set credential and proxy
                SetCredentialAndProxy(request, credential, proxyString);
                // 3.setting request method and send data(if has)
                SetReuqestHeaders(request, method, requestHeaders);

                // 4.Request data write to stream( if has data)
                if (!WriteRequestData(request, sendData, out errorMsg))
                {
                    throw new Exception(errorMsg);
                }
                
                // 5.等待並取得Server回應
                response = request.GetResponse();
                dataStream = response.GetResponseStream();
                
                // 檢視Request和Response內的屬性數據
                if (debugDisplay)
                {
                    ReflectionAllPropertyValue(request);
                    ReflectionAllPropertyValue(response);
                }
                buffer = new Queue<byte>();

                // 6.將Response的數據讀出並輸出
                while ((readByte = dataStream.ReadByte()) > -1)
                {
                    buffer.Enqueue((byte)readByte);
                } 
                result = buffer.ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Response Error:" + ex.Message + "\n " + ex.StackTrace);
                //throw ex;
                errorMsg = "Response Error:" + ex.Message + "\n " + ex.StackTrace;
            }
            finally
            {
                // 6.關閉Response連線
                dataStream.Close();
                response.Close();
            }
            return result;
        }

        /// <summary>
        /// 使用WebRequest發送請求並取得Response
        /// </summary>
        /// <param name="uriString">目的地Uri字串</param>
        /// <param name="method">GET/POST</param>
        /// <param name="errorMsg">異常訊息(default:"")</param>
        /// <param name="timeOut">請求的回應逾時(ms)</param>
        /// <param name="requestHeaders">要新增的Request Header</param>
        /// <param name="sendData">送出的請求參數數據</param>
        /// <returns>response content data</returns>
        public static byte[] GetResponse(string uriString, string method, out string errorMsg, int timeOut = 10000, NameValueCollection requestHeaders = null, params byte[] sendData)
        {
            #region variable
            WebRequest request = null;
            Stream dataStream = null;
            WebResponse response = null;
            byte[] result = null;
            Queue<byte> buffer = null;
            int readByte = -1;
            errorMsg = string.Empty;
            #endregion

            
            try
            {
                // 1.create request and setting timeout, credential, proxy(if has)
                request = CreateWebRequest(uriString, timeOut);
                // 2.setting request method and send data(if has)
                SetReuqestHeaders(request, method, requestHeaders);
                
                // 3.Request data write to stream( if has data)
                if (!WriteRequestData(request, sendData, out errorMsg))
                {
                    throw new Exception(errorMsg);
                }

                // 4.等待並取得Server回應
                response = request.GetResponse();
                dataStream = response.GetResponseStream();

                // 檢視Request和Response內的屬性數據
                //if (debugDisplay)
                //{
                //    ReflectionAllPropertyValue(request);
                //    ReflectionAllPropertyValue(response);
                //}
                buffer = new Queue<byte>();

                // 5.將Response的數據讀出並輸出
                while ((readByte = dataStream.ReadByte()) > -1)
                {
                    buffer.Enqueue((byte)readByte);
                }
                result = buffer.ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Response Error:" + ex.Message + "\n " + ex.StackTrace);
                //throw ex;
                errorMsg = "Response Error:" + ex.Message + "\n " + ex.StackTrace;
            }
            finally
            {
                // 6.關閉Response連線
                dataStream.Close();
                response.Close();
            }
            return result;
        }
        #endregion

        #region Private Method
        /// <summary>
        /// Create WebRequest by UriString and set timeout(ms), credential, proxy
        /// </summary>
        /// <param name="uriString">destination Uri</param>
        /// <param name="timeOut">逾時(ms)</param>
        /// <param name="credential">Web認證(如果有)</param>
        /// <param name="proxyString">代理(如果有)</param>
        /// <returns>WebRequest Object</returns>
        private static WebRequest CreateWebRequest(string uriString, int timeOut)
        {
            WebRequest request = null;

            //ref:http://www.dotblogs.com.tw/jaigi/archive/2012/09/29/75169.aspx
            request = WebRequest.Create(uriString);
            //time out setting
            request.Timeout = timeOut;

            return request;
        }

        /// <summary>
        /// 設定認證與代理
        /// </summary>
        /// <param name="request">請求</param>
        /// <param name="credential">認證介面</param>
        /// <param name="proxyString">代理UriString</param>
        private static void SetCredentialAndProxy(WebRequest request, ICredentials credential, string proxyUriString)
        {
            //認證用
            request.Credentials = credential;//new NetworkCredential("ID","Password","Domain Name") ;

            //proxy setting 
            request.Proxy = new WebProxy(proxyUriString);
        }

        /// <summary>
        /// Setting WebRequest Medthod, ContentType, ContentLength, UserAgent, 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="method"></param>
        /// <param name="sendData"></param>
        private static void SetReuqestHeaders(WebRequest request, string method, NameValueCollection headers)
        {
            Stream dataStream = null;
            string errMsg = string.Empty;
            //設定Method
            request.Method = method.ToUpper();
            //設定Client端Agent
            ((HttpWebRequest)request).UserAgent = "4+ Client";
            switch (method.ToUpper())
            {
                case "POST":
                    request.ContentType = "application/x-www-form-urlencoded";
                    break;
                case "GET":
                    request.ContentType = "text/html";
                    break;
                default:
                    request.ContentType = "text/plain";
                    break;
            }
            //新增header參數
            if (headers != null && headers.Count > 0)
            {
                request.Headers.Add(headers);
            }
        }

        /// <summary>
        /// Request data write to Stream
        /// </summary>
        /// <param name="request">請求</param>
        /// <param name="requestData">請求的參數資料</param>
        /// <param name="errMsg">異常訊息(default:"")</param>
        /// <returns>請求寫入成功/失敗</returns>
        private static bool WriteRequestData(WebRequest request,byte[] requestData,out string errMsg)
        {
            Stream dataStream = null;
            errMsg = ""; 
            try
            {
                if (requestData != null && requestData.Length > 0)
                {
                    dataStream = request.GetRequestStream();
                    dataStream.Write(requestData, 0, requestData.Length);
                    request.ContentLength = requestData.Length;
                }
                else
                {
                    request.ContentLength = 0;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Request Write Error: {0} \n{1}", ex.Message, ex.StackTrace);
                errMsg = String.Format("Request Write Error: {0} \n{1}", ex.Message, ex.StackTrace);
                request.ContentLength = 0;
                return false;
            }
            finally
            {
                if (dataStream != null)
                {
                    dataStream.Close();
                }
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
        #endregion
    }
}
