using System.Collections.Generic;
using System.Data;

namespace YunYan.Models
{
    internal class NodeConditionModel
    {
        private const string _tableName = "node_condition";
        private MySQL _mySQL = new MySQL();

        public int ID { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 節點狀態 1:正常 2:
        /// </summary>
        public int Status { get; set; }
        public double UL { get; set; }
        public double LL { get; set; }
        public double EL { get; set; }

        private void InsertData()
        {
            var dic = new Dictionary<string, object>()
            {
                { "nc_name",Name},
                { "nc_status",Status},
                { "nc_UL",UL},
                { "nc_LL",LL},
                { "nc_EL",EL},
            };

            _mySQL.InsertTable(_tableName, dic);
        }

        private void UpdateData()
        {
            var dic = new Dictionary<string, object>()
            {
                { "nc_name",Name},
                { "nc_status",Status},
                { "nc_UL",UL},
                { "nc_LL",LL},
                { "nc_EL",EL},
            };

            _mySQL.UpdateTable(_tableName, dic, $"nc_id = {ID}");
        }

        public void InsertOrUpdateData()
        {
            // 檢查是否已存在相同名稱的資料
            var checkQuery = $"SELECT nc_id FROM {_tableName} WHERE nc_name = @name";
            var parameters = new Dictionary<string, object> { { "name", Name } };
            var dataTable = _mySQL.SelectTable(checkQuery, parameters);

            // 根據檢查結果決定是插入還是更新
            if (dataTable.Rows.Count == 0)
            {
                // 插入新資料
                InsertData();
            }
            else
            {
                // 更新現有資料
                ID = (int)dataTable.Rows[0]["nc_id"];
                UpdateData();
            }
        }

        public void LoadDataByName(string name)
        {
            var query = $"SELECT * FROM {_tableName} WHERE nc_name = @name";
            var parameters = new Dictionary<string, object> { { "name", name } };
            var dataTable = _mySQL.SelectTable(query, parameters);

            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];
                ID = row.Field<int>("nc_id");
                Name = row.Field<string>("nc_name");
                Status = row.Field<int>("nc_status");
                UL = row.Field<double>("nc_UL");
                LL = row.Field<double>("nc_LL");
                EL = row.Field<double>("nc_EL");
            }
        }

    }
}
