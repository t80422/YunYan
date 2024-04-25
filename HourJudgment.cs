using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace YunYan
{
    internal class HourJudgment
    {
        /// <summary>
        /// 處理該小時內的五分鐘數值
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="last9O1">前次最後一筆 9O1A201 數值</param>
        /// <param name="isPermittedLoadReduction"></param>
        /// <param name="isInLoadReductionRange"></param>
        /// <param name="isInApprovedOperationRange"></param>
        /// <returns></returns>
        public Dictionary<string, ExportData> ProcessData(List<Dictionary<string, ExportData>> dataList, DateTime dateTime)
        {
            var result = new Dictionary<string, ExportData>();
            var allKeys = dataList.SelectMany(dict => dict.Keys).Distinct();

            foreach (var key in allKeys)
            {
                var exportDatasForKey = dataList.Where(dict => dict.ContainsKey(key)).Select(dict => dict[key]).ToList();

                if (key == "9O1A201")
                {
                    result[key] = ProcessExportDatasFor9O1(exportDatasForKey, dateTime);
                }
                else
                {
                    var processedData = ProcessExportDatasForKey(exportDatasForKey,key);
                    result[key] = processedData;
                }
            }
            return result;
        }

        private ExportData ProcessExportDatasForKey(List<ExportData> exportDatas,string nodeName)
        {
            // 規則 1: 檢查是否有 12 個數值
            if (exportDatas.Count != 12)
            {
                return new ExportData
                {
                    Value = exportDatas.Average(ed => ed.Value),
                    Status = "32"
                };
            }

            // 規則 2: 狀態為 00 的數值
            var status00Count = exportDatas.Count(ed => ed.Status == "00");
            if (status00Count > 6)
            {
                return new ExportData
                {
                    Value = exportDatas.Average(ed => ed.Value),
                    Status = "00"
                };
            }
            else if (status00Count == 6)
            {
                return exportDatas.Last();
            }

            // 規則 3: 異常狀態處理
            var abnormalStates = new[] { "20", "30", "31" };
            var abnormalDataCount = exportDatas.Count(ed => abnormalStates.Contains(ed.Status));

            if (abnormalDataCount > exportDatas.Count / 2)
            {
                var mostCommonAbnormalState = exportDatas
                    .GroupBy(ed => ed.Status)
                    .Where(g => abnormalStates.Contains(g.Key))
                    .OrderByDescending(g => g.Count())
                    .First().Key;

                return new ExportData
                {
                    Value = exportDatas.Average(ed => ed.Value),
                    Status = mostCommonAbnormalState
                };
            }

            //規則4:檢查逾限
            //var overLimitCount = exportDatas.Count(x => x.Status == "11");
            string status;

            //if (overLimitCount <= 6)
            //    status = "10";
            //else if (overLimitCount > 6)
            //    status = "11";
            var value = exportDatas.Average(x=>x.Value);
            if (LimitValidator.IsWithinTemperatureLimits(nodeName, value))
            {
                status = "10";
            }
            else
            {
                status = "11";
            }

            return new ExportData()
            {
                Status = status,
                Value = value
            };
        }

        private ExportData ProcessExportDatasFor9O1(List<ExportData> exportDatas, DateTime dateTime)
        {
            var last9O1 = GetLastFiveMinute9O1(dateTime.AddHours(-1));
            var now9O1 = GetLastFiveMinute9O1(dateTime);
            var value = last9O1 - now9O1;

            // 規則 1: 檢查是否有 12 個數值
            if (exportDatas.Count != 12)
            {
                return new ExportData
                {
                    Value = value,
                    Status = "32"
                };
            }

            // 規則 2: 狀態為 00 的數值
            var status00Count = exportDatas.Count(ed => ed.Status == "00");
            if (status00Count > 6)
            {
                return new ExportData
                {
                    Value = value,
                    Status = "00"
                };
            }
            else if (status00Count == 6)
            {
                return new ExportData()
                {
                    Value = value,
                    Status = exportDatas.Last().Status,
                };
            }

            // 規則 3: 異常狀態處理
            var abnormalStates = new[] { "20", "30", "31" };
            var abnormalDataCount = exportDatas.Count(ed => abnormalStates.Contains(ed.Status));

            if (abnormalDataCount > exportDatas.Count / 2)
            {
                var mostCommonAbnormalState = exportDatas
                    .GroupBy(ed => ed.Status)
                    .Where(g => abnormalStates.Contains(g.Key))
                    .OrderByDescending(g => g.Count())
                    .First().Key;

                return new ExportData
                {
                    Value = value,
                    Status = mostCommonAbnormalState
                };
            }

            //規則4:正常數據
            return new ExportData
            {
                Value = value,
                Status = "10"
            };

        }

        /// <summary>
        /// 取得該小時最後五分鐘活性碳量(9O1A201)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private double GetLastFiveMinute9O1(DateTime dateTime)
        {
            var query = "SELECT sd_9O1A201 FROM sensor_data WHERE sd_time BETWEEN @start AND @end";
            var startDateTime = dateTime.AddMinutes(-4);
            var parameter = new Dictionary<string, object>()
            {
                {"start",startDateTime.ToString("yyyy-MM-dd HH:mm:00") },
                {"end",dateTime.ToString("yyyy-MM-dd HH:mm:59") }
            };

            try
            {
                using (MySQL sql = new MySQL())
                {
                    var dt = sql.SelectTable(query, parameter);
                    if (dt != null)
                    {
                        double avg = dt.AsEnumerable()
                                       .Select(row => row.Field<double>("sd_9O1A201"))
                                       .Average();

                        return avg;
                    }
                    else
                    {
                        Log.LogMsg("GetLastFiveMinute9O1-找不到資料");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogMsg("GetLastFiveMinute9O1-" + ex.Message);
            }
            return 0;
        }
    }
}
