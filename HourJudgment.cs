using System;
using System.Collections.Generic;
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
        public Dictionary<string, ExportData> ProcessData(List<Dictionary<string, ExportData>> dataList, double last9O1, bool isPermittedLoadReduction, bool isInLoadReductionRange, bool isInApprovedOperationRange)
        {
            var result = new Dictionary<string, ExportData>();
            var allKeys = dataList.SelectMany(dict => dict.Keys).Distinct();

            foreach (var key in allKeys)
            {
                var exportDatasForKey = dataList.Where(dict => dict.ContainsKey(key)).Select(dict => dict[key]).ToList();

                if (key == "9O1A201")
                {
                    result[key] = ProcessExportDatasFor9O1(exportDatasForKey, last9O1, isPermittedLoadReduction, isInLoadReductionRange, isInApprovedOperationRange);
                }
                else
                {
                    var processedData = ProcessExportDatasForKey(exportDatasForKey, isPermittedLoadReduction, isInLoadReductionRange, isInApprovedOperationRange);
                    result[key] = processedData;
                }
            }
            return result;
        }

        private ExportData ProcessExportDatasForKey(List<ExportData> exportDatas, bool isPermittedLoadReduction, bool isInLoadReductionRange, bool isInApprovedOperationRange)
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

            //規則4:正常數據
            if (isPermittedLoadReduction)
            {
                double averageValue = exportDatas.Average(ed => ed.Value);
                return new ExportData
                {
                    Value = averageValue,
                    Status = isInLoadReductionRange ? "02" : "11"
                };
            }
            else
            {
                return new ExportData
                {
                    Value = exportDatas.Average(ed => ed.Value),
                    Status = isInApprovedOperationRange ? "10" : "11"
                };
            }
        }

        private ExportData ProcessExportDatasFor9O1(List<ExportData> exportDatas, double last9O1, bool isPermittedLoadReduction, bool isInLoadReductionRange, bool isInApprovedOperationRange)
        {
            exportDatas.Reverse();
            //var value = exportDatas.Zip(exportDatas.Skip(1), (a, b) => a.Value - b.Value).Sum();
            var value = last9O1 - exportDatas.Last().Value;

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
            if (isPermittedLoadReduction)
            {
                return new ExportData
                {
                    Value = value,
                    Status = isInLoadReductionRange ? "02" : "11"
                };
            }
            else
            {
                return new ExportData
                {
                    Value = value,
                    Status = isInApprovedOperationRange ? "10" : "11"
                };
            }
        }
    }
}
