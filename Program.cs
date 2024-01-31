using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YunYan
{
    static class Program
    {
        // 為應用程序定義一個唯一的名稱
        private const string AppName = "YunYan";

        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 嘗試創建一個命名的互斥鎖
            bool createdNew;
            using (Mutex mutex = new Mutex(true, AppName, out createdNew))
            {
                // 檢查互斥鎖是否是新創建的
                if (createdNew)
                {
                    Console.WriteLine("應用程序已啟動");

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());

                    Console.ReadLine(); // 保持應用程序開啟
                }
                else
                {
                    MessageBox.Show("應用程序已在運行，將關閉新實例。");
                    // 如果已經有一個實例在運行，則關閉當前實例
                }
            }
        }

        public static string[] Codes = { "9F3E201", "9F4E201", "9F4E203", "9E4A201", "9D1A201", "937P201", "936P201", "9O1A201", "948P201" };
    }

    public static class Utility
    {
        /// <summary>
        /// 計算排氣風量
        /// </summary>
        /// <param name="exhTemp">排氣溫度</param>
        /// <param name="pa">大氣壓力</param>
        /// <param name="ps">靜壓力</param>
        /// <param name="a">管道截面積</param>
        /// <param name="v">排氣流速</param>
        /// <returns>排氣風量</returns>
        public static decimal CalculateExhaustAirflow(decimal exhTemp, decimal pa, decimal ps, decimal a, decimal v)
        {
            return 273 / (273 + exhTemp) * (pa + ps) / 760 * 60 * a * v;
        }

        /// <summary>
        /// 建立設定檔
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public static void CreateOrUpdateConfigFile(string fileName, string content)
        {
            try
            {
                var filePath = Path.Combine(Application.StartupPath, fileName);
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static string[] ReadConfigFile(string fileName)
        {
            try
            {
                var filePath = Path.Combine(Application.StartupPath, fileName);
                return File.ReadAllLines(filePath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void LineNotify(string msg)
        {
            var apiURL = ConfigurationManager.AppSettings["LinNotifyURL"];
            var uuids = ConfigurationManager.AppSettings["uuid"];
            var uuidParts = uuids.Split(',');

            for (int i = 0; i < uuidParts.Length; i++)
            {
                var uuid = uuidParts[i];
                var getdata = "uuid=" + uuid + "&mydata=";

                //發送文字編碼
                var webBrowser = new WebBrowser();
                var content = "點位異常:" + msg;
                webBrowser.Navigate(apiURL + getdata + System.Net.WebUtility.UrlEncode(content));
                Log.LogMsg(content);
            }
        }
    }

    public static class Log
    {
        public static void LogMsg(string message)
        {
            try
            {
                // 獲取 Log 資料夾的路徑
                string logDirectory = Path.Combine(Application.StartupPath, "Log");

                // 檢查 Log 資料夾是否存在，如果不存在則創建
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                // 設置日誌文件的完整路徑
                string filePath = Path.Combine(logDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");

                // 創建日誌信息
                string msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

                // 寫入日誌文件
                File.AppendAllText(filePath, msg + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Log Error");
            }
        }

    }
}
