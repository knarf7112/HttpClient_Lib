using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebClient_Lib;
using System.Text;
using System.Diagnostics;

namespace WebClient_Lib_UnitTest
{
    [TestClass]
    public class UnitTest_HttpClient_Simple
    {
        private IHttpClient_Simple Client { get; set; }

        [TestInitialize]
        public void Init()
        {
            string uri = "http://10.27.88.164:1539/AutoloadHandler.ashx";
                      //@"http://10.27.88.164:1539/AuthHandler.ashx";
            Client = new HttpClient_Simple(30000, uri);

        }

        [TestMethod]
        public void TestMethod1()
        {
            byte[] inputData = new byte[] { 0x20, 0x00,  //KeyNo(1 Byte) + KeyVer(1 Byte)
                0x04, 0x87, 0x3A, 0xBA, 0x8D, 0x2C, 0x80,//UID(7 Bytes)
                0x4E, 0xF6, 0x10, 0x41, 0xAB, 0xE8, 0xB0, 0xEF, 0x8B, 0x32, 0xA6, 0x27, 0xB1, 0x9D, 0x83, 0xAA//E(RanB) (16 bytes)
            };

            string keyLabel = "2ICH3F0000" + inputData[0].ToString() + "A"; //byte[0]
            string KeyVersion = inputData[1].ToString();                    //byte[1]
            string uid = BitConverter.ToString(inputData, 2, 7).Replace("-", "");            //byte[2~8]
            string enc_RanB = BitConverter.ToString(inputData, 9, 16).Replace("-", "");       //byte[9~24]
            //=========================
            string data = "32" + "00" + "04873ABA8D2C80" + "4EF61041ABE8B0EF8B32A627B19D83AA";

            byte[] dataBytes = Encoding.ASCII.GetBytes(data);


            //*****************************************************
            //回傳Byte Array
            byte[] result = Client.Post(dataBytes);
            if (result == null)
            {
                Debug.WriteLine("回傳資料(hex): null");
            }
            else
            {
                Debug.WriteLine("回傳資料(hex):\n" + BitConverter.ToString(result).Replace("-", ""));
            }
            //Console.WriteLine("回傳資料(Encoding):" + Encoding.ASCII.GetString(result));
            //*****************************************************
            //回傳傳String
            //string result2 = Client.Post(data);

            //Debug.WriteLine("回傳資料2(hex):\n" + result2);

        }
    }
}
