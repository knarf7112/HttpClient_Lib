using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//
using System.Net.Http;

//Test 1
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Net;

namespace WebClient_Lib
{
    class Program
    {
        static void Main()
        {

            byte[] inputData = new byte[] { 0x20, 0x00,  //KeyNo(1 Byte) + KeyVer(1 Byte)
                0x04, 0x87, 0x3A, 0xBA, 0x8D, 0x2C, 0x80,//UID(7 Bytes)
                0x4E, 0xF6, 0x10, 0x41, 0xAB, 0xE8, 0xB0, 0xEF, 0x8B, 0x32, 0xA6, 0x27, 0xB1, 0x9D, 0x83, 0xAA//E(RanB) (16 bytes)
            };
            
            string keyLabel = "2ICH3F0000" + inputData[0].ToString() + "A"; //byte[0]
            string KeyVersion = inputData[1].ToString();                    //byte[1]
            string uid = BitConverter.ToString(inputData, 2, 7).Replace("-","");            //byte[2~8]
            string enc_RanB = BitConverter.ToString(inputData, 9, 16).Replace("-", "");       //byte[9~24]
            //=========================
            string uri = "http://10.27.88.164:1539/AutoloadHandler.ashx";
                        //@"http://10.27.88.164:1538/AuthHandler.ashx";
            IHttpClient_Simple test = new HttpClient_Simple(3000, uri);
            string data = "32" + "00" + "04873ABA8D2C80" + "4EF61041ABE8B0EF8B32A627B19D83AA";
            
            byte[] dataBytes = Encoding.ASCII.GetBytes(data);


            //*****************************************************
            //回傳Byte Array
            byte[] result = test.Post(dataBytes);
            Console.WriteLine("回傳資料(hex):\n" + BitConverter.ToString(result).Replace("-", ""));
            //Console.WriteLine("回傳資料(Encoding):" + Encoding.ASCII.GetString(result));
            //*****************************************************
            //回傳傳String
            string result2 = test.Post(data);

            Console.WriteLine("回傳資料2(hex):\n" + result2);
            Console.ReadKey();
        }
        //測試異常 => 回傳 ha ha
        static void Main4()
        {
            string uri = @"http://10.27.88.164:1538/AuthHandler.ashx";
            IHttpClient_Simple test = new HttpClient_Simple(3000, uri);
            byte[] data = test.Post(new byte[2],uri);
            Console.WriteLine("回傳資料(hex):" + BitConverter.ToString(data).Replace("-",""));
            Console.WriteLine("回傳資料(Encoding):" + Encoding.ASCII.GetString(data));
            Console.ReadKey();
        }
        static void Main5(string[] args)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            Console.ReadKey();
            for (int i = 0; i < 100; i++)
            {
                Console.Write(r.Next(0, 4096) + ", ");
            }
                //PostTo();
                Console.ReadKey();
        }

        static async void PostTo()
        {
            Console.WriteLine("開始 ... ");
            HttpClient client = new HttpClient();
            HttpContent content = new StringContent("",Encoding.ASCII);
            Console.WriteLine("開始送出 ... ");
            Uri uri = new Uri("http://10.27.88.164:1538/AuthHandler.ashx");

            HttpResponseMessage rsp = await client.PostAsync(uri, content);
            
            rsp.EnsureSuccessStatusCode();
            Console.WriteLine("開始等回來的資料 ... ");
            string result = await rsp.Content.ReadAsStringAsync();
            Console.WriteLine("結果:" + result);
            
        }

        #region Test 2
        /// <summary>
        /// HttpClient Memory leak ? => Answer
        /// ref:http://stackoverflow.com/questions/14075026/httpclient-crawling-results-in-memory-leak
        /// </summary>
        /// <param name="args"></param>
        static void Main2(string[] args)
        {

            ServicePointManager.DefaultConnectionLimit = 500;
            CrawlAsync().ContinueWith(task => Console.WriteLine("***DONE!"));
            Console.ReadLine();
        }

        private static async Task CrawlAsync()
        {

            int numberOfCores = Environment.ProcessorCount;
            List<string> requestUris = File.ReadAllLines(@"C:\Users\Tugberk\Downloads\links.txt").ToList();
            ConcurrentDictionary<int, Tuple<Task, HttpRequestMessage>> tasks = new ConcurrentDictionary<int, Tuple<Task, HttpRequestMessage>>();
            List<HttpRequestMessage> requestsToDispose = new List<HttpRequestMessage>();

            var httpClient = new HttpClient();

            for (int i = 0; i < numberOfCores; i++)
            {

                string requestUri = requestUris.First();
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
                Task task = MakeCall(httpClient, requestMessage);
                tasks.AddOrUpdate(task.Id, Tuple.Create(task, requestMessage), (index, t) => t);
                requestUris.RemoveAt(0);
            }

            while (tasks.Values.Count > 0)
            {

                Task task = await Task.WhenAny(tasks.Values.Select(x => x.Item1));

                Tuple<Task, HttpRequestMessage> removedTask;
                tasks.TryRemove(task.Id, out removedTask);
                removedTask.Item1.Dispose();
                removedTask.Item2.Dispose();

                if (requestUris.Count > 0)
                {

                    var requestUri = requestUris.First();
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    Task newTask = MakeCall(httpClient, requestMessage);
                    tasks.AddOrUpdate(newTask.Id, Tuple.Create(newTask, requestMessage), (index, t) => t);
                    requestUris.RemoveAt(0);
                }

                GC.Collect(0);
                GC.Collect(1);
                GC.Collect(2);
            }

            httpClient.Dispose();
        }

        private static async Task MakeCall(HttpClient httpClient, HttpRequestMessage requestMessage)
        {

            Console.WriteLine("**Starting new request for {0}!", requestMessage.RequestUri);
            var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            Console.WriteLine("**Request is completed for {0}! Status Code: {1}", requestMessage.RequestUri, response.StatusCode);

            using (response)
            {
                if (response.IsSuccessStatusCode)
                {
                    using (response.Content)
                    {

                        Console.WriteLine("**Getting the HTML for {0}!", requestMessage.RequestUri);
                        string html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        Console.WriteLine("**Got the HTML for {0}! Legth: {1}", requestMessage.RequestUri, html.Length);
                    }
                }
                else if (response.Content != null)
                {

                    response.Content.Dispose();
                }
            }
        }
        #endregion

        #region Test 1
        /// <summary>
        /// HttpClient Memory leak?
        /// Ref:http://stackoverflow.com/questions/14075026/httpclient-crawling-results-in-memory-leak
        /// </summary>
        public static void Main1()
        {
            int waiting = 0;
            const int MaxWaiting = 100;
            var httpClient = new HttpClient();
            foreach (var link in File.ReadAllLines("links.txt"))
            {

                while (waiting >= MaxWaiting)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Waiting ...");
                }
                httpClient.GetAsync(link)//link like => http://10.11.22.33:80/test.ashx?key=123
                    .ContinueWith(t =>
                    {
                        try
                        {
                            var httpResponseMessage = t.Result;
                            if (httpResponseMessage.IsSuccessStatusCode)
                                httpResponseMessage.Content.LoadIntoBufferAsync()
                                    .ContinueWith(t2 =>
                                    {
                                        if (t2.IsFaulted)
                                        {
                                            httpResponseMessage.Dispose();
                                            Console.ForegroundColor = ConsoleColor.Magenta;
                                            Console.WriteLine(t2.Exception);
                                        }
                                        else
                                        {
                                            httpResponseMessage.Content.
                                                ReadAsStringAsync()
                                                .ContinueWith(t3 =>
                                                {
                                                    Interlocked.Decrement(ref waiting);

                                                    try
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.White;

                                                        Console.WriteLine(httpResponseMessage.RequestMessage.RequestUri);
                                                        string s =
                                                            t3.Result;

                                                    }
                                                    catch (Exception ex3)
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Yellow;

                                                        Console.WriteLine(ex3);
                                                    }
                                                    httpResponseMessage.Dispose();
                                                });
                                        }
                                    }
                                    );
                        }
                        catch (Exception e)
                        {
                            Interlocked.Decrement(ref waiting);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(e);
                        }
                    }
                    );

                Interlocked.Increment(ref waiting);

            }

            Console.Read();
        }
        #endregion
    }
}
