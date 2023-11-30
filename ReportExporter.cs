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
        private Dictionary<string, List<double>> _hourlyData = new Dictionary<string, List<double>>();
        private List<Dictionary<string, ExportData>> _hourData = new List<Dictionary<string, ExportData>>();
        private int _cycleIntervalMinutes = 60;

        public string SavePath { get; set; }

        public void FetchAndProcessData(DateTime time)
        {
            _time = time;

            var dic = new Dictionary<string, object>
            {
                {"end",_time},
                {"start",_time.AddMinutes(-5)}
            };
            DataTable dt;

            using (MySQL sql = new MySQL())
            {
                //取得五分鐘資料
                dt = sql.SelectTable("SELECT * FROM sensor_data WHERE sd_time BETWEEN @start AND @end", dic);
            }

            var sj = new FiveMinuteJudgment();
            var lstData = sj.EvaluateDataTable(dt);
            _hourData.Add(lstData);

            string content = "";

            foreach (var data in lstData)
            {
                content += DataString(data.Key, data.Value.Value, data.Value.Status, false) + "\n";
            }

            if (_time.Minute % _cycleIntervalMinutes == 0)
            {
                var hour = new HourJudgment();

                foreach (var kvp in hour.ProcessData(_hourData, false, false, false))
                {
                    content += DataString(kvp.Key, kvp.Value.Value, kvp.Value.Status, true) + "\n";
                }

                _hourlyData.Clear(); // 清除資料，為下一小時做準備
            }

            ExportFile(content);
        }

        /// <summary>
        /// 設定循環時間,可用於縮短測試時間
        /// </summary>
        /// <param name="minutes"></param>
        public void SetCycleIntervalMinutes(int minutes)
        {
            _cycleIntervalMinutes = minutes;
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

            var rocYear = _time.Year - 1991;
            var formatValue = Math.Round(value, 2).ToString("#0.0#").PadLeft(isHourFormat ? 9 : 7, '0');
            var timeFormat = isHourFormat ? "MMddHH" : "MMddHHmm";

            return code + "  " + rocYear.ToString("000") + _time.ToString(timeFormat) + formatValue + "   " + status;
        }

        private void ExportFile(string content)
        {
            var fileName = "E1" + DateTime.Now.ToString("MMddHHmm") + ".H76";
            var filePath = Path.Combine(SavePath, fileName);

            content = "100H52A1242E01\n" + content;

            try
            {
                File.WriteAllText(filePath, content);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
