using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace YunYan
{
    internal class ReportExporter
    {
        private DateTime _time;
        //private List<Dictionary<string, ExportData>> _hourData = new List<Dictionary<string, ExportData>>();
        //private double temp9O1 = 0;

        public string SavePath { get; set; }
        public string BackupPath { get; set; }

        public void FetchAndProcessData(DateTime time)
        {
            _time = time;

            var dic = new Dictionary<string, object>
            {
                {"end",_time.ToString("yyyy-MM-dd HH:mm:59")},
                {"start",_time.AddMinutes(-4).ToString("yyyy-MM-dd HH:mm:00")}
            };
            DataTable dt;

            using (MySQL sql = new MySQL())
            {
                //取得五分鐘資料
                dt = sql.SelectTable("SELECT * FROM sensor_data WHERE sd_time BETWEEN @start AND @end", dic);
            }

            var sj = new FiveMinuteJudgment();
            var lstData = sj.EvaluateDataTable(dt);
            string content = "";

            foreach (var data in lstData)
            {
                content += DataString(data.Key, data.Value.Value, data.Value.Status, false) + "\n";

                if (data.Value.Status != "10")
                {
                    Utility.LineNotify($"點位:{data.Key} 狀態異常:{data.Value.Status}");
                }
            }

            if (_time.Minute == 0)
            {
                List<Dictionary<string, ExportData>> _hourData = new List<Dictionary<string, ExportData>>();
                var startTime = Convert.ToDateTime(_time.AddHours(-1).ToString("yyyy/MM/dd HH:01:00"));

                for (DateTime t = startTime; t < _time; t = t.AddMinutes(5))
                {
                    dt = GetFiveMinuteDataFromDb(t);
                    sj = new FiveMinuteJudgment();
                    lstData = sj.EvaluateDataTable(dt);
                    _hourData.Add(lstData);
                }

                var hour = new HourJudgment();

                foreach (var kvp in hour.ProcessData(_hourData, _time))
                {
                    content += DataString(kvp.Key, kvp.Value.Value, kvp.Value.Status, true) + "\n";
                }

                _hourData.Clear(); // 清除資料，為下一小時做準備
            }
            ExportFile(content);
        }

        /// <summary>
        /// 產生輸出文字
        /// </summary>
        /// <param name="code">監控代碼</param>
        /// <param name="value">值</param>
        /// <param name="status">狀態</param>
        /// <param name="isHourFormat">是否為整點</param>
        /// <returns></returns>
        private string DataString(string code, double value, string status, bool isHourFormat)
        {
            if (isHourFormat && code.StartsWith("9"))
            {
                code = "2" + code.Substring(1);
            }

            var rocYear = _time.Year - 1911;
            var formatValue = Math.Round(value, 2).ToString("#0.0#").PadRight(7, ' ');

            return code + "  " + rocYear.ToString("000") + _time.ToString("MMddHHmm") + formatValue + "   " + status;
        }

        private void ExportFile(string content)
        {
            try
            {
                var fileName = "E1" + DateTime.Now.ToString("MMddHHmm") + ".H76";
                content = "100H52A1242E01\n" + content;

                var filePath = Path.Combine(SavePath, fileName);
                File.WriteAllText(filePath, content);

                if (BackupPath != "")
                {
                    var backupPath = Path.Combine(BackupPath, fileName);
                    File.WriteAllText(backupPath, content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 從資料庫取得該五分鐘數據
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private static DataTable GetFiveMinuteDataFromDb(DateTime dateTime)
        {
            var query = "SELECT * FROM sensor_data WHERE sd_time BETWEEN @start AND @end";
            var parameters = new Dictionary<string, object>()
            {
                {"end",dateTime.AddMinutes(4).ToString("yyyy-MM-dd HH:mm:59")},
                {"start",dateTime.ToString("yyyy-MM-dd HH:mm:00")}
            };
            var dt = new DataTable();

            using (MySQL sql = new MySQL())
            {
                dt = sql.SelectTable(query, parameters);
            }

            return dt;
        }
    }
}