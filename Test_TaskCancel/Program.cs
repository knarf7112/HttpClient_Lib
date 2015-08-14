using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//


namespace Test_TaskCancel
{
    class Program
    {
        /// <summary>
        /// Task 任務取消測試
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var tokenSource = new CancellationTokenSource();
            var task1 = Task.Factory.StartNew(
                    () => DoWork(tokenSource.Token), tokenSource.Token
                );
            //用此任務取消上面的指派任務
            var task2 = Task.Factory.StartNew(() => CancelTask(tokenSource));
            //Thread.Sleep(5000);

            // ok, let's cancel it (well, let's "request it be cancelled")
            //tokenSource.Cancel();

            // wait for the task to "finish"
            task1.Wait();

            Console.WriteLine("Main End...");
            Console.ReadKey();
        }

        static void DoWork(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Do useful stuff here
                Console.WriteLine("Working!!!");
                Thread.Sleep(1000);
            }
        }

        static void CancelTask(CancellationTokenSource cancelSource)
        {
            int i = 0;
            while (i < 5)
            {
                Console.WriteLine("i=" + i);
                i++;
                Thread.Sleep(1000);
            }
            Console.WriteLine("執行取消task1任務");
            cancelSource.Cancel();
        }
    }
}
