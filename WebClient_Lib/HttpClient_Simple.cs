using System;
using System.Text;
using System.Threading.Tasks;
//
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace WebClient_Lib
{
    /// <summary>
    /// 簡易Http傳輸物件(需要.Net 4.5)
    /// </summary>
    public class HttpClient_Simple : IHttpClient_Simple
    {
        private HttpClient _client;
        private Uri _uri;
        private Stopwatch timer;//用來確定是否逾時
        #region Constructor
        /// <summary>
        /// 設定等待要求逾時(ms)和指定目的地的Uri字串
        /// 沒設定後面要自己加入Uri
        /// </summary>
        /// <param name="milliseconds">等待要求逾時(ms)</param>
        /// <param name="uriString">指定目的地的Uri字串</param>
        public HttpClient_Simple(double milliseconds,string uriString = null)
        {
            this._uri = (uriString == null) ? null : new Uri(uriString, UriKind.Absolute);
            this._client = new HttpClient() 
            {
                BaseAddress = this._uri,
                MaxResponseContentBufferSize = 64000000,//Response buffer 64MB
                Timeout = TimeSpan.FromMilliseconds(milliseconds)//Gets or sets the number of milliseconds to wait before the request times out.
            };
            this.timer = new Stopwatch();
        }
        #endregion

        #region 非同步方法
        /// <summary>
        /// 使用(Post)傳出Request資料(string),並非同步方式等候Response資料(string)
        /// 若沒輸入Uri,則使用Constructor輸入的Uri當預設參數
        /// 若沒輸入Encoding,預設編碼為ASCII
        /// </summary>
        /// <param name="requestContent">http request body string</param>
        /// <param name="uriString">Specified Uri String</param>
        /// <param name="encode">Request資料的編碼方式</param>
        /// <returns>可等候的結果字串(http body)</returns>
        public async Task<string> AsyncPost_String(string requestContent, string uriString = null, Encoding encode = null)
        {
            Encoding encoding = (encode == null) ? Encoding.ASCII : encode;
            Uri uri = (uriString == null) ? this._uri : new Uri(uriString, UriKind.Absolute);
            HttpContent httpContent = new StringContent(requestContent, encoding);
            HttpResponseMessage response = await this._client.PostAsync(uri, httpContent);

            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();
            return result;
        }

        /// <summary>
        /// 使用(Post)傳出Request資料(byte[]),並非同步方式等候Response資料(byte[])
        /// 若沒輸入Uri,則使用Constructor輸入的Uri當預設參數
        /// </summary>
        /// <param name="requestContent">http request body byte array</param>
        /// <param name="uriString">Specified Uri String</param>
        /// <returns>可等候的結果陣列(http body)</returns>
        public async Task<byte[]> AsyncPost_Bytes(byte[] requestContent, string uriString = null)
        {
            Uri uri = (uriString == null) ? this._uri : new Uri(uriString, UriKind.Absolute);
            HttpContent httpContent = new ByteArrayContent(requestContent);
            HttpResponseMessage response = await this._client.PostAsync(uri, httpContent);

            response.EnsureSuccessStatusCode();

            byte[] result = await response.Content.ReadAsByteArrayAsync();
            return result;
        }
        #endregion

        #region 同步方法(有等待逾時)
        /// <summary>
        /// 使用(Post)傳出Request資料(string)並取得Response結果(string),等候逾時會拋出錯誤
        /// 若沒輸入Uri,則使用Constructor輸入的Uri當預設參數
        /// 若沒輸入Encoding,預設編碼為ASCII
        /// </summary>
        /// <param name="requestContent">http request body string</param>
        /// <param name="uriString">specified Uri string</param>
        /// <param name="encode">Request資料的編碼方式</param>
        /// <returns>Response http body(byte[])</returns>
        public string Post(string requestContent,string uriString = null,Encoding encode = null)
        {
            HttpResponseMessage response = null;
            string result = null;
            Encoding encoding = (encode == null) ? Encoding.ASCII : encode;
            Uri uri = (uriString == null) ? this._uri : new Uri(uriString, UriKind.Absolute);
            Stopwatch timer = new Stopwatch();
            //資料寫入Http Body
            HttpContent httpContent = new StringContent(requestContent, encoding);
            //Post出要求
            Task<HttpResponseMessage> taskResponse = this._client.PostAsync(uri, httpContent);
            //****************************************************************************************************
            //等待回應5秒 //怪怪的  Server端下中斷點會直接跳異常,但應該還沒逾時,也沒取消
            try
            {
                this.timer.Restart();
                taskResponse.Wait();
            }
            catch (AggregateException ex)
            {
                Debug.WriteLine("Waitting Response Error:" + ex.StackTrace);
                return null;
            }
            finally
            {
                this.timer.Stop();
                Debug.WriteLine("TimeSpend:" + ((decimal)this.timer.ElapsedTicks / Stopwatch.Frequency) + "s");
            }
            //****************************************************************************************************
            //Thread.Sleep(5000);
            //取得結果
            response = GetResult(taskResponse);
            if (response == null)
            {
                return null;
            }
            //確認成功
            response.EnsureSuccessStatusCode();
            //從Response讀取Http Body (byte array)
            Task<Stream> taskResponseStream = response.Content.ReadAsStreamAsync();

            if (!taskResponseStream.Wait(1000))
            {
                throw new TimeoutException("讀取Response資料逾時");
            }
            //讀取完成
            byte[] buffer = new byte[0x1000];
            int readCnt = taskResponseStream.Result.Read(buffer, 0, buffer.Length);
            Array.Resize(ref buffer, readCnt);
            result = BitConverter.ToString(buffer).Replace("-", "");
            return result;
        }

        /// <summary>
        /// 使用(Post)傳出Request資料(byte[])並取得Response結果(byte[]),等候逾時會拋出錯誤
        /// 若沒輸入Uri,則使用Constructor輸入的Uri當預設參數
        /// </summary>
        /// <param name="requestContent">http request body byte array</param>
        /// <param name="uriString">specified Uri string</param>
        /// <returns>Response http body(byte[])</returns>
        public byte[] Post(byte[] requestContent, string uriString = null)
        {
            HttpResponseMessage response = null;
            byte[] result = null;
            Uri uri = (uriString == null) ? this._uri : new Uri(uriString, UriKind.Absolute);

            //資料寫入Http Body
            HttpContent httpContent = new ByteArrayContent(requestContent);
            //Post出要求
            //this._client.Timeout = new TimeSpan(0, 0, 60);
            Task<HttpResponseMessage> taskResponse = this._client.PostAsync(uri, httpContent);
            //****************************************************************************************************
            try
            {
                this.timer.Restart();
                taskResponse.Wait();
            }
            catch (AggregateException ex)
            {
                Debug.WriteLine("Waitting Response Error:" + ex.StackTrace);
                return null;
            }
            finally
            {
                this.timer.Stop();
                Debug.WriteLine("TimeSpend:" + ((decimal)this.timer.ElapsedTicks / Stopwatch.Frequency) + "s");
            }
            //****************************************************************************************************
            //Thread.Sleep(5000);
            response = GetResult(taskResponse);
            if (response == null)
            {
                return null;
            }
            //確認成功
            response.EnsureSuccessStatusCode();
            //從Response讀取Http Body (byte array)
            Task<byte[]> taskResponseBytes = response.Content.ReadAsByteArrayAsync();
            //等1秒後丟異常
            if (!taskResponseBytes.Wait(2000))
            {
                throw new TimeoutException("讀取Response資料逾時");
            }
            //讀取完成
            result = taskResponseBytes.Result;
            return result;
        }

        /// <summary>
        /// 使用(GET)傳出Request資料(byte[])並取得Response結果(byte[]),等候逾時會拋出錯誤
        /// 若沒輸入Uri,則使用Constructor輸入的Uri當預設參數
        /// </summary>
        /// <param name="requestContent">http request body byte array</param>
        /// <param name="uriString">specified Uri string</param>
        /// <returns>Response http body(byte[])</returns>
        public byte[] Get(byte[] requestContent, string uriString = null)
        {
            HttpResponseMessage response = null;
            byte[] result = null;
            Uri uri = (uriString == null) ? this._uri : new Uri(uriString, UriKind.Absolute);
            //Post出要求
            Task<HttpResponseMessage> taskResponse = this._client.GetAsync(uri);
            //等待回應5秒後丟異常//怪怪的  Server端下中斷點會直接跳異常,但應該還沒逾時,也沒取消
            if (!taskResponse.Wait(5000))
            {
                throw new TimeoutException("等候Response逾時");
            }
            //Thread.Sleep(5000);
            //取得結果
            response = this.GetResult(taskResponse);
            if (response == null)
            {
                return null;
            }
            //確認成功
            response.EnsureSuccessStatusCode();
            //從Response讀取Http Body (byte array)
            Task<byte[]> taskResponseBytes = response.Content.ReadAsByteArrayAsync();
            //等2秒後丟異常
            if (!taskResponseBytes.Wait(2000))
            {
                throw new TimeoutException("讀取Response資料逾時");
            }
            //讀取完成
            result = taskResponseBytes.Result;
            Debug.WriteLine("");
            return result;
        }
        #endregion

        #region Private Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskResponse"></param>
        /// <returns></returns>
        private HttpResponseMessage GetResult(Task<HttpResponseMessage> taskResponse)
        {
            HttpResponseMessage response = null;
            switch (taskResponse.Status)
            {
                case TaskStatus.RanToCompletion:
                    //取得結果
                    response = taskResponse.Result;
                    break;
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                default:
                    return null;

            }
            return response;
        }
        #endregion
    }
}
