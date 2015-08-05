using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebClient_Lib
{
    interface IHttpClient_Simple
    {
        /// <summary>
        /// 使用(Post)傳出Request資料(string),並非同步方式等候Response資料(string)
        /// 若沒輸入Uri,則使用Constructor輸入的Uri當預設參數
        /// 若沒輸入Encoding,預設編碼為ASCII
        /// </summary>
        /// <param name="requestContent">http request body string</param>
        /// <param name="uriString">Specified Uri String</param>
        /// <param name="encode">Request資料的編碼方式</param>
        /// <returns>可等候的結果字串(http body)</returns>
        Task<string> AsyncPost_String(string requestContent, string uriString = null, Encoding encode = null);

        /// <summary>
        /// 使用(Post)傳出Request資料(byte[]),並非同步方式等候Response資料(byte[])
        /// 若沒輸入Uri,則使用Constructor輸入的Uri當預設參數
        /// </summary>
        /// <param name="requestContent">http request body byte array</param>
        /// <param name="uriString">Specified Uri String</param>
        /// <returns>可等候的結果陣列(http body)</returns>
        Task<byte[]> AsyncPost_Bytes(byte[] requestContent, string uriString = null);

        /// <summary>
        /// 使用(Post)傳出Request資料(string)並取得Response結果(string),等候逾時會拋出錯誤
        /// 若沒輸入Uri,則使用Constructor輸入的Uri當預設參數
        /// 若沒輸入Encoding,預設編碼為ASCII
        /// </summary>
        /// <param name="requestContent">http request body string</param>
        /// <param name="uriString">specified Uri string</param>
        /// <param name="encode">Request資料的編碼方式</param>
        /// <returns>Response http body(byte[])</returns>
        string Post(string requestContent, string uriString = null, Encoding encode = null);

        /// <summary>
        /// 使用(Post)傳出Request資料(byte[])並取得Response結果(byte[]),等候逾時會拋出錯誤
        /// 若沒輸入Uri,則使用Constructor輸入的Uri當預設參數
        /// </summary>
        /// <param name="requestContent">http request body byte array</param>
        /// <param name="uriString">specified Uri string</param>
        /// <returns>Response http body(byte[])</returns>
        byte[] Post(byte[] requestContent, string uriString = null);

        /// <summary>
        /// 使用(GET)傳出Request資料(byte[])並取得Response結果(byte[]),等候逾時會拋出錯誤
        /// 若沒輸入Uri,則使用Constructor輸入的Uri當預設參數
        /// </summary>
        /// <param name="requestContent">http request body byte array</param>
        /// <param name="uriString">specified Uri string</param>
        /// <returns>Response http body(byte[])</returns>
        byte[] Get(byte[] requestContent, string uriString = null);
    }
}
