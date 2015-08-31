using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Collections.Specialized;

namespace Client_Lib_NETv35
{
    public class HttpClient_Simple : IHttpClient_Simple
    {
        private WebRequest _client = null;
        private Uri _uri;
        private Stopwatch timer;//用來確定是否逾時
        private double timeout = -1;
        public HttpClient_Simple(double milliseconds, string uriString = null)
        {
            this.timeout = milliseconds;
            CreateWebRequest(uriString);
            this.timer = new Stopwatch();
        }

        
        /// <summary>
        /// (Method:Post)送出Http Request等待回傳Http Response資料
        /// </summary>
        /// <param name="requestContent">請求發送的資料</param>
        /// <param name="uriString">目的地Uri String</param>
        /// <returns>Response data(Body)</returns>
        public byte[] Post(byte[] requestContent, string uriString = null)
        {
            string errMsg = "";
            byte[] responseResult = null;
            // 1.check or create web request
            CreateWebRequest(uriString);
            // 2.add headers
            SetReuqestHeaders(this._client, "POST", null);
            // 3.Write Request data 
            if (!WriteRequestData(this._client, requestContent, out errMsg))
            {
                throw new Exception(errMsg);
            }
            // 4.waiting for response data
            responseResult = GetResponse(this._client, out errMsg);
            if (responseResult == null)
            {
                throw new Exception(errMsg);
            }
            return responseResult;
        }

        /// <summary>
        /// (Method:Get)送出Http Request等待回傳Http Response資料
        /// </summary>
        /// <param name="requestContent"></param>
        /// <param name="uriString"></param>
        /// <returns></returns>
        public byte[] Get(byte[] requestContent, string uriString = null)
        {
            string errMsg = "";
            byte[] responseResult = null;
            // 1.check or create web request
            CreateWebRequest(uriString);
            // 2.add headers
            SetReuqestHeaders(this._client, "GET", null);
            // 3.Write Request data 
            if (WriteRequestData(this._client, null, out errMsg))
            {
                throw new Exception(errMsg);
            }
            // 4.waiting for response data
            responseResult = GetResponse(this._client, out errMsg);
            if (responseResult == null)
            {
                throw new Exception(errMsg);
            }
            return responseResult;
        }


        #region Private Method
        /// <summary>
        /// create WebRequest from uri String
        /// </summary>
        /// <param name="uriString">destination uri</param>
        private void CreateWebRequest(string uriString)
        {
            if (uriString != null)
            {
                this._uri = new Uri(uriString, UriKind.Absolute);
                this._client = WebRequest.CreateDefault(this._uri);
                this._client.Timeout = (int)this.timeout;
            }
        }

        /// <summary>
        /// Setting WebRequest Medthod, ContentType, ContentLength, UserAgent, 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="method"></param>
        /// <param name="sendData"></param>
        private void SetReuqestHeaders(WebRequest request, string method, NameValueCollection headers)
        {
            HttpWebRequest httpWebRequest = httpWebRequest = ((HttpWebRequest)request);
            //default setting
            //設定Method
            httpWebRequest.Method = method.ToUpper();
            //設定Client端Agent
            httpWebRequest.UserAgent = "4+ Client";
            //完成後關閉連線
            httpWebRequest.KeepAlive = false;
            //httpWebRequest.Connection = "Close";

            switch (method.ToUpper())
            {
                case "POST":
                    request.ContentType = "application/x-www-form-urlencoded";
                    break;
                case "GET":
                    //request.ContentType = "text/html";
                    //break;
                default:
                    request.ContentType = "text/plain";
                    break;
            }
            //新增的header參數
            if (headers != null && headers.Count > 0)
            {
                //request.Headers.Add(headers);//輸入Content-Type時不給這樣增加  機車ㄟ
                //原因如下:
                //ref1:http://blog.useasp.net/archive/2013/09/03/the-methods-to-dispose-http-header-cannot-add-to-webrequest-headers.aspx
                //ref2:https://msdn.microsoft.com/zh-cn/library/system.net.httpwebrequest.headers(v=vs.110).aspx
                foreach (string headerName in headers.Keys)
                {
                    switch (headerName.ToUpper())
                    {
                        case "ACCEPT":
                            httpWebRequest.Accept = headers[headerName];
                            break;
                        case "CONNECTION":
                            if (headers[headerName] != "Keep-alive")
                            {
                                httpWebRequest.KeepAlive = false;
                                //httpWebRequest.Connection = headers[headerName];
                            }
                            else
                            {
                                httpWebRequest.KeepAlive = true;
                                //httpWebRequest.Connection = headers[headerName];
                            }
                            break;
                        case "CONTENT-LENGTH":
                            httpWebRequest.ContentLength = Convert.ToInt64(headers[headerName]);
                            break;
                        case "CONTENT-TYPE":
                            httpWebRequest.ContentType = headers[headerName];
                            break;
                        case "EXPECT":
                            httpWebRequest.Expect = headers[headerName];
                            break;
                            //.Net 3.5 haven't this header's properties
                        //case "DATE":
                        //    httpWebRequest.Date = DateTime.Parse(headers[headerName]);
                        //    break;
                        //case "HOST":
                        //    httpWebRequest.Host = headers[headerName];
                        //    break;
                        case "IF-MODIFIED-SINCE":
                            httpWebRequest.IfModifiedSince = DateTime.Parse(headers[headerName]);
                            break;
                        case "RANGE":
                            //httpWebRequest.AddRange()
                            //範圍不知道要怎加,先跳過
                            break;
                        case "REFERER":
                            httpWebRequest.Referer = headers[headerName];
                            break;
                        case "TRANSFER-ENCODING":
                            httpWebRequest.TransferEncoding = headers[headerName];
                            break;
                        case "USER-AGENT":
                            httpWebRequest.UserAgent = headers[headerName];
                            break;
                        default:
                            request.Headers.Add(headerName, headers[headerName]);
                            break;

                    }

                }

            }
        }

        /// <summary>
        /// Request data write to Stream
        /// </summary>
        /// <param name="request">請求</param>
        /// <param name="requestData">請求的參數資料</param>
        /// <param name="errMsg">異常訊息(default:"")</param>
        /// <returns>請求寫入成功/失敗</returns>
        private bool WriteRequestData(WebRequest request, byte[] requestData, out string errMsg)
        {
            Stream dataStream = null;
            errMsg = "";
            try
            {
                if (requestData != null && requestData.Length > 0)
                {
                    request.ContentLength = requestData.Length;
                    dataStream = request.GetRequestStream();
                    dataStream.Write(requestData, 0, requestData.Length);

                }
                else
                {
                    request.ContentLength = 0;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Request Write Error: {0} \n{1}", ex.Message);
                errMsg = String.Format("Request Write Error: {0} \n{1}", ex.Message, ex.StackTrace);
                //request.ContentLength = 0;
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
        /// 使用WebRequest發送請求並取得Response
        /// </summary>
        /// <param name="errorMsg">異常訊息(default:"")</param>
        /// <returns>response content data</returns>
        public byte[] GetResponse(WebRequest request, out string errorMsg)
        {
            #region variable
            Stream dataStream = null;
            WebResponse response = null;
            byte[] result = null;
            Queue<byte> buffer = null;
            int readByte = -1;
            errorMsg = string.Empty;
            #endregion


            try
            {

                // 等待並取得Server回應
                response = request.GetResponse();
                dataStream = response.GetResponseStream();

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
                if (dataStream != null)
                {
                    dataStream.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
            }
            return result;
        }

        #endregion
    }
}
