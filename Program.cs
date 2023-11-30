using System;
using System.IO;
using System.Windows.Forms;
//using static System.Net.Mime.MediaTypeNames;

namespace YunYan
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static string[] Codes = { "9F3E201", "9F4E201", "9F4E203", "9E4A201", "9D1A201", "937P201", "936P201", "9O1A201", "948P201"};
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
    }
}
