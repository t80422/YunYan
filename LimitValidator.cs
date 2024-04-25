using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace YunYan
{
    static class LimitValidator
    {
        public static Dictionary<string, NodeLimit> Nodes;

        static LimitValidator()
        {
            Nodes = new Dictionary<string, NodeLimit>();
            foreach (var node in Program.Codes)
            {
                var nl = new NodeLimit();
                Nodes.Add(node, nl);
            }
        }

        public struct NodeLimit
        {
            public double? UpperLimit;
            public double? LowerLimit;
            public double? ErrorLimit;
            public int RPMUpperLimit;
            public int RPMLowerLimit;
            public int Status;
        }

        public static bool LoadNodeLimitsFromDatabase()
        {
            bool result = false;

            try
            {
                DataTable dt;

                using (MySQL sql = new MySQL())
                {
                    dt = sql.SelectTable("SELECT * FROM node_condition");
                };

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("請先設定點位的上下限與狀態並存檔,再點擊繼續執行");
                }
                else
                {
                    GetNodeStatus(dt);
                    GetNodeLimit(dt);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                // 處理異常情況
                MessageBox.Show($"無法從數據庫加載數據。詳細信息：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return result;
        }

        private static void GetNodeLimit(DataTable dt)
        {
            string[] nodes = { "9F4E203", "9E4A201", "9D1A201", "937P201" };

            var nodeLimits = dt.AsEnumerable()
                               .Where(row => nodes.Contains(row.Field<string>("nc_name")))
                               .ToDictionary(
                                   row => row.Field<string>("nc_name"),
                                   row => new NodeLimit
                                   {
                                       UpperLimit = row.Field<double>("nc_UL"),
                                       LowerLimit = row.Field<double>("nc_LL"),
                                       ErrorLimit = row.Field<double>("nc_EL")
                                   });

            foreach (var nodeLimit in nodeLimits)
            {
                Nodes[nodeLimit.Key] = nodeLimit.Value;
            }
        }

        private static void GetNodeStatus(DataTable dt)
        {
            foreach (string node in Program.Codes)
            {
                var row = dt.AsEnumerable().FirstOrDefault(r => r.Field<string>("nc_name") == node);
                if (row != null)
                {
                    NodeLimit nodeLimit = new NodeLimit() { Status = row.Field<int>("nc_status") };
                    Nodes[node] = nodeLimit;
                }
            }
        }

        /// <summary>
        /// 判斷是否逾限
        /// </summary>
        /// <param name="name">點位名稱</param>
        /// <param name="value">值</param>
        /// <returns>Trun:未逾限 False:逾限</returns>
        public static bool IsWithinTemperatureLimits(string name, double value)
        {
            if (Nodes.TryGetValue(name, out NodeLimit nodeLimit))
            {
                if (nodeLimit.UpperLimit.HasValue && nodeLimit.LowerLimit.HasValue)
                {
                    return value >= nodeLimit.LowerLimit.Value && value <= nodeLimit.UpperLimit.Value;
                }
            }
            return true;
        }
    }
}