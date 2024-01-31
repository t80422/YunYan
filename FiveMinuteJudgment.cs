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

            EvaluateRow(dataTable, exportDataDict);

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

        private void EvaluateRow(DataTable dataTable, Dictionary<string, ExportData> exportDataDict)
        {
            foreach (string nodeName in Program.Codes)
            {
                var temperatureFieldName = "sd_" + nodeName;
                var rpmFieldName = temperatureFieldName + "_rpm";
                var statusFieldName = "sd_" + nodeName + "_status";

                //判斷有沒有正常數據
                var nodeRows = dataTable.AsEnumerable().Select(row => new { Temperture = row.Field<double>(temperatureFieldName), Status = row.Field<string>(statusFieldName) });

                //異常狀態(校正:20 維修:31)
                if (nodeRows.Where(r => r.Status == "10").Count() == 0)
                {
                    //取出最後一筆不正常點位的資料
                    var lastRow = nodeRows.LastOrDefault();
                    exportDataDict.Add(nodeName, new ExportData()
                    {
                        Value = lastRow.Temperture,
                        Status = lastRow.Status.ToString(),
                    });
                }
                else
                {
                    //計算五分中平均值
                    var avg=nodeRows.Where(row => row.Status == "10").Average(r=>r.Temperture);
                    
                    if (LimitValidator.IsWithinTemperatureLimits(nodeName, avg))
                    {
                        exportDataDict.Add(nodeName, new ExportData()
                        {
                            Value = avg,
                            Status ="10"
                        });
                    }
                    else
                    {
                        exportDataDict.Add(nodeName, new ExportData()
                        {
                            Value = avg,
                            Status ="11"
                        });
                    }
                }
            }
        }
    }
}
