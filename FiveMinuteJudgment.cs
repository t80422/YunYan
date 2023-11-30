using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace YunYan
{
    internal class FiveMinuteJudgment
    {
        public Dictionary<string, ExportData> EvaluateDataTable(DataTable dataTable)
        {
            Dictionary<string, ExportData> exportDataDict = new Dictionary<string, ExportData>();

            if (dataTable.Rows.Count == 0)
            {
                return GetDefaultStatusExportDataDict();
            }

            //EvaluateRow(dataTable, exportDataDict);
            Test(dataTable, exportDataDict);

            return exportDataDict;
        }

        private Dictionary<string, ExportData> GetDefaultStatusExportDataDict()
        {
            var exportDataDict = new Dictionary<string, ExportData>();

            foreach (string field in Program.Codes)
            {
                exportDataDict.Add(field, new ExportData
                {
                    Value = 0,
                    Status = "32"
                });
            }
            return exportDataDict;
        }

        private void Test(DataTable dataTable, Dictionary<string, ExportData> exportDataDict)
        {
            foreach (string field in Program.Codes)
            {
                var temperatureFieldName = "sd_" + field;
                var rpmFieldName = temperatureFieldName + "_rpm";
                var validTemperatureIndexes = dataTable.AsEnumerable()
                                                       .Select(row => row.Field<double>(temperatureFieldName))
                                                       .ToList();
                double averageTemperature = 0;
                string status;

                averageTemperature = validTemperatureIndexes.Average();
                status = "10";
                exportDataDict[field] = new ExportData
                {
                    Value = averageTemperature,
                    Status = status
                };
            }
        }

        private void EvaluateRow(DataTable dataTable, Dictionary<string, ExportData> exportDataDict)
        {
            foreach (string field in Program.Codes)
            {
                var temperatureFieldName = "sd_" + field;
                var rpmFieldName = temperatureFieldName + "_rpm";

                // 過濾出符合溫度標準的值及其索引
                var validTemperatureIndexes = dataTable.AsEnumerable()
                                                       .Select((row, index) => new { Value = row.Field<double>(temperatureFieldName), Index = index })
                                                       .Where(x => LimitValidator.IsWithinTemperatureLimits(field, x.Value))
                                                       .ToList();

                // 只考慮符合溫度標準的轉數值
                var validRpmValues = validTemperatureIndexes.Select(x => dataTable.Rows[x.Index].Field<double>(rpmFieldName)).ToList();

                double averageTemperature = 0;
                double averageRpm = 0;
                string status;

                if (validTemperatureIndexes.Any())
                {
                    averageTemperature = validTemperatureIndexes.Select(x => x.Value).Average();
                    averageRpm = validRpmValues.Average();

                    if (LimitValidator.IsWithinRPMLimits(field, averageRpm))
                    {
                        // 符合核定操作範圍判斷
                        status = "10";
                    }
                    else
                    {
                        // 風車待機或降載
                        // 符合降仔操作範圍
                        status = "02";
                    }
                }
                else
                {
                    status = "31";
                }

                exportDataDict[field] = new ExportData
                {
                    Value = averageTemperature,
                    Status = status
                };
            }
        }
    }
}
