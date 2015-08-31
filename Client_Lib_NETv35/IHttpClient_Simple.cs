using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client_Lib_NETv35
{
    public interface IHttpClient_Simple
    {
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
