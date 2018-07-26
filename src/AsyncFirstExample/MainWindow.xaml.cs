#define USE_SYNC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// Add a using directive and a reference for System.Net.Http;
using System.Net.Http;

namespace AsyncFirstExample
{
    public partial class MainWindow : Window
    {
        // Mark the event handler with async so you can use await in it.
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            resultsTextBox.Text += "\r\n";

            // Call and await separately.
            //Task<int> getLengthTask = AccessTheWebAsync();
            //// You can do independent work here.
            //int contentLength = await getLengthTask;

            try
            {
                // Violate rule: Avoid async void method
                //DoAsyncVoidWithError();

                // Fix: Avoid async void method
                await DoAsyncTaskWithError();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }    

            int contentLength = await AccessTheWebAsync();

            resultsTextBox.Text +=
                String.Format("\r\nLength of the downloaded string: {0}.\r\n", contentLength);

        }


        // Three things to note in the signature:
        //  - The method has an async modifier. 
        //  - The return type is Task or Task<T>. (See "Return Types" section.)
        //    Here, it is Task<int> because the return statement returns an integer.
        //  - The method name ends in "Async."
        async Task<int> AccessTheWebAsync()
        { 
            // You need to add a reference to System.Net.Http to declare client.
            HttpClient client = new HttpClient();

            // GetStringAsync returns a Task<string>. That means that when you await the
            // task you'll get a string (urlContents).
            Task<string> getStringTask = client.GetStringAsync("http://msdn.microsoft.com");
            //string urlContents = await getStringTask;

            // You can do work here that doesn't rely on the string from GetStringAsync.
            DoIndependentWork();

            // The await operator suspends AccessTheWebAsync.
            //  - AccessTheWebAsync can't continue until getStringTask is complete.
            //  - Meanwhile, control returns to the caller of AccessTheWebAsync.
            //  - Control resumes here when getStringTask is complete. 
            //  - The await operator then retrieves the string result from getStringTask.
            //string urlContents = await getStringTask;
            await getStringTask.ConfigureAwait(false);

            // The return statement specifies an integer result.
            // Any methods that are awaiting AccessTheWebAsync retrieve the length value.
            return getStringTask.Result.Length;
        }


        void DoIndependentWork()
        {
            resultsTextBox.Text += "[DoIndependentWork] Working . . . . . . .\r\n";
        }

        async void DoAsyncVoidWithError()
        {
            resultsTextBox.Text += "[DoAsyncWithError] Working..........\r\n";

            await Task.Delay(2000);

            throw new Exception("I'm sorry!");
        }

        async Task DoAsyncTaskWithError()
        {
            resultsTextBox.Text += "[DoAsyncTaskWithError] Working..........\r\n";

            await Task.Delay(2000);

            throw new Exception("I'm sorry!");
        }

        #region Async All the Way

        private void DeadlockButton_Click(object sender, RoutedEventArgs e)
        {
            resultsTextBox.Text += "\r\n";

            resultsTextBox.Text += "[DeadlockTest] Working..........\r\n";

            // DEADLOCK, Console app run OK
            int contentLength = AccessTheWebAsync().Result;

            resultsTextBox.Text +=
                String.Format("\r\nLength of the downloaded string: {0}.\r\n", contentLength);
        }

        private void DeadlockFreeButton_Click(object sender, RoutedEventArgs e)
        {
            resultsTextBox.Text += "\r\n";

            resultsTextBox.Text += "[DeadlockFreeTest] Working..........\r\n";

            // NO DEADLOCK, UI lags some seconds
            int contentLength = AccessTheWebAsyncWithouDeadlock().Result;

            resultsTextBox.Text +=
                String.Format("\r\nLength of the downloaded string: {0}.\r\n", contentLength);
        }

        async Task<int> AccessTheWebAsyncWithouDeadlock()
        {
            Task<int> t = AccessTheWebAsync();
            await t.ConfigureAwait(false);

            return t.Result;
        }

        #endregion

        #region Await on multiple async method 

//#if USE_SYNC
#if !USE_SYNC
        private void AwaitMultipleAsyncButton_Click(object sender, RoutedEventArgs e)
        {
            // Application will freeze 8 seconds
            CallWithSync();
        }
#else
        private async void AwaitMultipleAsyncButton_Click(object sender, RoutedEventArgs e)
        {
            resultsTextBox.Text += "\r\n";

            // Application won't freeze

            // Run synchronous
            //await CallWithAsync();

            // Run asynchronous
            await MultipleAsyncMethodsWithCombinators();
        }
#endif
        private void CallWithSync()
        {
            string result = DoTask1("Task1");
            string result1 = DoTask2("Task2");
            Console.WriteLine(result);
            Console.WriteLine(result1);
        }

        private async Task CallWithAsync()
        {
            string result = await DoTask1Async("Task1");
            string result1 = await DoTask2Async("Task2");
            Console.WriteLine(result);
            Console.WriteLine(result1);
        }

        private async Task MultipleAsyncMethodsWithCombinators()
        {
            Task<string> t1 = DoTask1Async("Task1");
            Task<string> t2 = DoTask2Async("Task2");

            await Task.WhenAll(t1, t2);
            Console.WriteLine("Finished both methods.\n " +
           
           "Result 1: {0}\n Result 2: {1}", t1.Result, t2.Result);
        }

        public string DoTask1(string msg)
        {
            resultsTextBox.Text += "[DoTask1] Working..........\r\n";
            System.Threading.Thread.Sleep(5000);
            resultsTextBox.Text += "[DoTask1] Done..........\r\n";

            return msg.ToUpper();
        }

        public string DoTask2(string msg)
        {
            resultsTextBox.Text += "[DoTask2] Working..........\r\n";
            System.Threading.Thread.Sleep(3000);
            resultsTextBox.Text += "[DoTask2] Done..........\r\n";

            return msg.ToUpper();
        }

        public async Task<string> DoTask1Async(string msg)
        {
            resultsTextBox.Text += "[DoTask1Async] Working..........\r\n";
            await Task.Delay(5000);
            resultsTextBox.Text += "[DoTask1Async] Done..........\r\n";

            return msg.ToUpper();
        }

        public async Task<string> DoTask2Async(string msg)
        {
            resultsTextBox.Text += "[DoTask2Async] Working..........\r\n";
            await Task.Delay(3000);
            resultsTextBox.Text += "[DoTask2Async] Done..........\r\n";

            return msg.ToUpper();
        }

#endregion
    }
}

// Sample Output:

// Working . . . . . . .

// Length of the downloaded string: 41564.
