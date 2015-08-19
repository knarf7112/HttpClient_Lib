﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
//
using System.Web;

namespace HttpRequest_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpRequest rqt1 = new HttpRequest("123.txt", "http://10.27.88.164:1539/TxLogFileUpLoadHandler.ashx", "Test");
            Uri uri = new Uri("http://10.27.88.164:1539/TxLogFileUpLoadHandler.ashx", UriKind.Absolute);
            WebRequest rqt = WebRequest.Create(uri);
            rqt.ContentType = "application/octet-stream";
            ((HttpWebRequest)rqt).UserAgent = ".NET Framework Example Client";
            rqt.Method = "POST";

            string postData = "this is test";
            byte[] postDataBytes = Encoding.ASCII.GetBytes(postData);

            rqt.ContentLength = postDataBytes.Length;

            Stream dataStream = rqt.GetRequestStream();
            dataStream.Write(postDataBytes, 0, postDataBytes.Length);
            dataStream.Close();

            WebResponse rsp = rqt.GetResponse();
            Console.WriteLine("Response Status:" + ((HttpWebResponse)rsp).StatusDescription);

            dataStream = rsp.GetResponseStream();
            byte[] responseBuffer = new byte[0x1000];
            int rspCnt = dataStream.Read(responseBuffer, 0, responseBuffer.Length);
            string responseString = Encoding.ASCII.GetString(responseBuffer, 0, rspCnt);
            Console.WriteLine("Response string:" + responseString);

            dataStream.Close();
            Console.ReadKey();
        }
    }
}
