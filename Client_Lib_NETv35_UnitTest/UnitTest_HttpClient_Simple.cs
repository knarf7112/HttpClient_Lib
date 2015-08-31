using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Client_Lib_NETv35;
using System.Text;
using System.Diagnostics;
namespace Client_Lib_NETv35_UnitTest
{
    [TestClass]
    public class UnitTest_HttpClient_Simple
    {
        private IHttpClient_Simple client;

        [TestInitialize]
        public void Init()
        {
            string uriString = "http://10.27.88.164/iBon/AutoLoad";
            this.client = new HttpClient_Simple(30000, uriString);
        }

        [TestMethod]
        public void Test_Post()
        {
            string expected = "reuqest Error";
            byte[] sendData = Encoding.ASCII.GetBytes(expected);
            byte[] responseData = this.client.Post(sendData);
            string responseString = Encoding.ASCII.GetString(responseData);
            Debug.WriteLine("Client送出: " + expected);
            Debug.WriteLine("Client收到: " + responseString);
        }

        [TestCleanup]
        public void Clean()
        {

        }
    }
}
