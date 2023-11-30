using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MySqlConnector;

namespace YunYan
{
    internal class MySQL : IDisposable
    {
        private MySqlConnection _conn;

        public MySQL()
        {
            var connectionString = "Server=localhost;Port=3307;Database=yunyan;Uid=root;";

            _conn = new MySqlConnection(connectionString);

            try
            {
                _conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("資料庫連結失敗", ex);
            }
            finally
            {
                _conn.Close();
            }
        }

        public DataTable SelectTable(string query, Dictionary<string, object> parameters)
        {
            var dt = new DataTable();

            try
            {
                _conn.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, _conn))
                {
                    foreach (var p in parameters)
                    {
                        cmd.Parameters.AddWithValue(p.Key, p.Value);
                    }

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            _conn.Close();

            return dt;
        }

        public void InsertTable(string tableName, Dictionary<string, object> parameters)
        {
            var sql = $"INSERT INTO {tableName} ({string.Join(",", parameters.Keys)}) VALUES ({string.Join(",", parameters.Keys.Select(x => $"@{x}"))})";

            try
            {
                _conn.Open();

                using (MySqlCommand cmd = new MySqlCommand(sql, _conn))
                {
                    parameters.ToList().ForEach(p => cmd.Parameters.AddWithValue($"@{p.Key}", p.Value));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            _conn.Close();
        }

        public void Dispose()
        {
            if (_conn != null)
            {
                if (_conn.State == ConnectionState.Open)
                {
                    _conn.Close();
                }
                _conn.Dispose();
                _conn = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
