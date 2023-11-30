using System;
using System.Collections.Generic;
using System.Linq;

namespace YunYan
{
    internal class HourJudgment
    {
        public Dictionary<string, ExportData> ProcessData(List<Dictionary<string, ExportData>> dataList, bool isPermittedLoadReduction, bool isInLoadReductionRange, bool isInApprovedOperationRange)
        {
            var result = new Dictionary<string, ExportData>();
            var allKeys = dataList.SelectMany(dict => dict.Keys).Distinct();

            foreach (var key in allKeys)
            {
                var exportDatasForKey = dataList.Where(dict => dict.ContainsKey(key)).Select(dict => dict[key]).ToList();
                var processedData = ProcessExportDatasForKey(exportDatasForKey, isPermittedLoadReduction, isInLoadReductionRange, isInApprovedOperationRange);
                result[key] = processedData;
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
                    Value = exportDatas.Where(ed => ed.Status == mostCommonAbnormalState).Average(ed => ed.Value),
                    Status = mostCommonAbnormalState
                };
            }

            //規則4:正常數據
            if (isPermittedLoadReduction)
            {
                double averageValue = exportDatas.Where(ed => ed.Status == "00").Average(ed => ed.Value);
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
                    Value = exportDatas.Where(ed => ed.Status == "00").Average(ed => ed.Value),
                    Status = isInApprovedOperationRange ? "10" : "11"
                };
            }
        }
    }
}
