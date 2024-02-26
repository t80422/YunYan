using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using YunYan;

namespace FileManagement_YunYan
{
    static class YunYanFile
    {
        public static void RegenerateMissingFiles(string folderPath, DateTime dateTime)
        {
            var fileNames = GetMissingFiles(folderPath, dateTime);

            if (fileNames.Count == 0)
            {
                MessageBox.Show("無未產生的檔案", "補產生檔案");
                return;
            }

            foreach (var fileName in fileNames)
            {
                //解析檔案名稱以獲取日期和時間
                var fileDateTime = ParseDateTimeFromFileName(fileName, dateTime);

                // 產生檔案
                var export = new ReportExporter_new(folderPath);
                export.GenerateFile(fileDateTime);
            }
        }

        /// <summary>
        /// 解析檔案名稱的日期和時間
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static DateTime ParseDateTimeFromFileName(string fileName, DateTime dateTime)
        {
            //檔案名稱格式如:"E1MMddHHmm.H76"
            var match = Regex.Match(fileName, @"E1(\d{2})(\d{2})(\d{2})(\d{2}).H76");

            if (match.Success)
            {
                int year = dateTime.Year;
                int month = int.Parse(match.Groups[1].Value);
                int day = int.Parse(match.Groups[2].Value);
                int hour = int.Parse(match.Groups[3].Value);
                int minute = int.Parse(match.Groups[4].Value);

                return new DateTime(year, month, day, hour, minute, 0);
            }
            else
            {
                var msg = "無效的檔案名稱";
                Log.LogMsg("FunctionName:ParseDateTimeFromFileName " + msg);
                throw new FormatException(msg);
            }
        }

        /// <summary>
        /// 檢查選定期間內的檔案是否存在,並取得不存在的檔案名稱
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns>未產生的檔案列表</returns>
        private static List<string> GetMissingFiles(string folderPath, DateTime dateTime)
        {
            //初始化未生成檔案的列表
            List<string> missingFiles = new List<string>();

            //計算當日應該生成的所有檔案名稱
            List<string> expectedFileNames = GenerateExpectedFileNames(dateTime);

            // 直接檢查這些預期檔案是否存在於資料夾中
            foreach (var expectedFile in expectedFileNames)
            {
                var filePath = Path.Combine(folderPath, expectedFile);

                if (!File.Exists(filePath))
                {
                    missingFiles.Add(expectedFile);
                }
            }

            return missingFiles;
        }

        /// <summary>
        /// 取得當日應該產生的檔案
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns>當日應該產生的檔案列表</returns>
        private static List<string> GenerateExpectedFileNames(DateTime dateTime)
        {
            List<string> fileNames = new List<string>();
            var now = DateTime.Now;

            // 設定當日的起始時間為00:00，結束時間為23:55，每五分鐘生成一個檔案名稱
            var startTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
            var endTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 55, 0);

            //如果endTime超過了當前時間,則調整為當前時間
            if (endTime > now)
            {
                endTime = now.AddMinutes(-(now.Minute % 5));//取得最近的五分鐘
            }

            for (var time = startTime; time <= endTime; time = time.AddMinutes(5))
            {
                var fileName = "E1" + time.ToString("MMddHHmm") + ".H76";
                fileNames.Add(fileName);
            }

            return fileNames;
        }
    }
}
