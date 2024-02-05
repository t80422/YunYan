using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YunYan.Models;

namespace YunYan
{
    public partial class Form1 : Form
    {
        private string _divisoFileName = "_divisor.set";
        private string _savePathName = "SavePath.set";
        private ReportExporter _export = new ReportExporter();
        private int _tempMinute = 0;
        private int tempInsert = 0;

        private NModbus _modbus_15_1;
        private NModbus _modbus_15_2;
        private NModbus _modbus_10;
        private ModbusData _modbusData;

        public Form1()
        {
            InitializeComponent();
            InitializeModbus();

            try
            {
                _modbusData = new ModbusData(Utility.ReadConfigFile("Numerical Operations.set"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Exit();
            }
        }

        private void InitializeModbus()
        {
            _modbus_10 = new NModbus("192.168.0.10", 502, 1);
            _modbus_15_1 = new NModbus("192.168.0.15", 502, 1);
            _modbus_15_2 = new NModbus("192.168.0.15", 502, 2);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var savePaths = Utility.ReadConfigFile(_savePathName);
            if (savePaths != null)
            {
                txtSavePath.Text = savePaths[0];
                txtBackupPath.Text = savePaths.Length >= 2 ? savePaths[1] : "";
            }

            InitTextBoxNumberOnly();
            LoadNodeConditionData();

            if (!LimitValidator.LoadNodeLimitsFromDatabase())
            {
                tmrUpdate.Enabled = false;
                btnRestart.BackColor = Color.Red;
            }
            else
            {
                btnRestart.PerformClick();
            }

            //=========測試區==========           
            //timer1.Interval = 10000;
            //_export.SetCycleIntervalMinutes(10);
            //timer1.Enabled = false;
            //=========測試區==========
        }

        private void LoadNodeConditionData()
        {
            var ctrls = tpStatus.Controls;

            ctrls.OfType<GroupBox>().ToList().ForEach(x =>
            {
                var model = new NodeConditionModel();
                var name = x.Name.Replace("grp", "");
                model.LoadDataByName(name);

                var statusRadioButton = x.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Tag.ToString() == model.Status.ToString());
                if (statusRadioButton != null)
                    statusRadioButton.Checked = true;

                var ulTextBox = ctrls.OfType<TextBox>().FirstOrDefault(txt => txt.Name == "txt" + name + "_UL");
                var llTextBox = ctrls.OfType<TextBox>().FirstOrDefault(txt => txt.Name == "txt" + name + "_LL");
                var elTextBox = ctrls.OfType<TextBox>().FirstOrDefault(txt => txt.Name == "txt" + name + "_EL");

                if (ulTextBox != null)
                    ulTextBox.Text = model.UL.ToString();

                if (llTextBox != null)
                    llTextBox.Text = model.LL.ToString();

                if (elTextBox != null)
                    elTextBox.Text = model.EL.ToString();
            });
        }

        private void InitTextBoxNumberOnly()
        {
            tpStatus.Controls.OfType<TextBox>().ToList().ForEach(t => t.KeyPress += new KeyPressEventHandler(textBox_KeyPress));
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // 檢查輸入的是否是數字、控制字符或小數點
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true; // 若不是數字、控制字符或小數點，則阻止輸入
            }
            // 允許一個小數點
            else if (e.KeyChar == '.' && textBox.Text.Contains("."))
            {
                e.Handled = true; // 若已經有小數點，則阻止再次輸入小數點
            }
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            tmrUpdate.Enabled = true;
            BackColor = SystemColors.Control;
        }

        private void GetModbusData()
        {
            var modbus10 = _modbus_10.GetData();
            _modbusData.Wind = modbus10[6];
            _modbusData.ADAM_6017_05 = modbus10[4];
            _modbusData.ADAM_6017_03 = modbus10[2];
            _modbusData.AC_001 = modbus10[5];

            var modbus15_1 = _modbus_15_1.GetData();
            _modbusData.PDI_200 = modbus15_1[16];
            _modbusData.PI_300 = modbus15_1[17];
            _modbusData.PI_420 = modbus15_1[18];
            _modbusData.PI_600 = modbus15_1[19];
            _modbusData.TI_160_1 = modbus15_1[24];
            _modbusData.TI_600_3 = modbus15_1[25];
            _modbusData.TI_200_1 = modbus15_1[26];
            _modbusData.TI_200_2 = modbus15_1[27];
            _modbusData.TI_200_3 = modbus15_1[28];
            _modbusData.TI_150 = modbus15_1[29];
            _modbusData.TI_200_5 = modbus15_1[30];

            var modbus15_2 = _modbus_15_2.GetData();
            _modbusData.TI_300 = modbus15_2[0];
            _modbusData.TI_350 = modbus15_2[1];
            _modbusData.TI_160_2 = modbus15_2[2];
            _modbusData.TI_200_4 = modbus15_2[3];
            _modbusData.TI_370_1 = modbus15_2[8];
            _modbusData.TI_370_2 = modbus15_2[9];
            _modbusData.TI_380 = modbus15_2[10];
            _modbusData.TI_361 = modbus15_2[11];
            _modbusData.TI_362 = modbus15_2[12];
            _modbusData.TI_381 = modbus15_2[13];
            _modbusData.TI_382 = modbus15_2[14];
            _modbusData.TI_600_1 = modbus15_2[16];
            _modbusData.TI_600_2 = modbus15_2[17];
            _modbusData.TI_900 = modbus15_2[18];
            _modbusData.TI_642_1 = modbus15_2[19];
            _modbusData.TI_642_2 = modbus15_2[20];
            _modbusData.TI_643_1 = modbus15_2[21];
            _modbusData.TI_643_2 = modbus15_2[22];
            _modbusData.TI_350_1 = modbus15_2[5];
            _modbusData.TI_350_2 = modbus15_2[6];

            lblTI_150.Text = _modbusData.TI_150.ToString();
            lblTI_160_1.Text = _modbusData.TI_160_1.ToString();
            lblTI_160_2.Text = _modbusData.TI_160_2.ToString();
            lblTI_380.Text = _modbusData.TI_380.ToString();
            lblPI_420.Text = _modbusData.PI_420.ToString();
            lblTI_361.Text = _modbusData.TI_361.ToString();
            lblTI_362.Text = _modbusData.TI_362.ToString();
            lblTI_370_1.Text = _modbusData.TI_370_1.ToString();
            lblTI381.Text = _modbusData.TI_381.ToString();
            lblTI_382.Text = _modbusData.TI_382.ToString();
            lblTI_370_2.Text = _modbusData.TI_370_2.ToString();
            lblTI_380_1.Text = _modbusData.TI_380.ToString();
            lblTI_200_3.Text = _modbusData.TI_200_3.ToString();
            lblTI_200_1.Text = _modbusData.TI_200_1.ToString();
            lblTI_200_5.Text = _modbusData.TI_200_5.ToString();
            lblTI_200_4.Text = _modbusData.TI_200_4.ToString();
            lblPDI_200.Text = _modbusData.PDI_200.ToString();
            lblTI_200_2.Text = _modbusData.TI_200_2.ToString();
            lblTI_200_12.Text = _modbusData.TI_200_12.ToString();
            lblTI_200_6.Text = _modbusData.TI_200_6.ToString();
            lblPI_300.Text = _modbusData.PI_300.ToString();
            lblTI_300.Text = _modbusData.TI_300.ToString();
            lblTI_350_1.Text = _modbusData.TI_350_1.ToString();
            lblTI_350.Text = _modbusData.TI_350.ToString();
            lblTI_350_2.Text = _modbusData.TI_350_2.ToString();
            lblTI_643_1.Text = _modbusData.TI_643_1.ToString();
            lblTI_642_2.Text = _modbusData.TI_642_2.ToString();
            lblTI_642_1.Text = _modbusData.TI_642_1.ToString();
            lblTI_643_2.Text = _modbusData.TI_643_2.ToString();
            lblA201.Text = _modbusData.A201.ToString();
            lbl9O1A201.Text = _modbusData._9OA201.ToString();
            lblTI_600_1.Text = _modbusData.TI_600_1.ToString();
            lblPI_600.Text = _modbusData.PI_600.ToString();
            lblTI_600_3.Text = _modbusData.TI_600_3.ToString();
            lblTI_600_2.Text = _modbusData.TI_600_2.ToString();
            lbl948P201.Text = _modbusData._948P201.ToString();
            lblP201.Text = _modbusData.P201.ToString();
            lbl937P201.Text = _modbusData._937P201.ToString();
            lblTI_900.Text = _modbusData.TI_900.ToString();
            txtTI_900.Text = _modbusData.TI_900.ToString();
            txtWind.Text = _modbusData.Wind.ToString();
        }

        private void btnSave_status_Click(object sender, EventArgs e)
        {
            //點位狀態
            var ctrls = tpStatus.Controls;

            ctrls.OfType<GroupBox>().ToList().ForEach(x =>
            {
                var model = new NodeConditionModel();
                model.Name = x.Name.Replace("grp", "");
                model.Status = int.Parse(x.Controls.OfType<RadioButton>().First(r => r.Checked).Tag.ToString());

                var ulTextBox = ctrls.OfType<TextBox>().FirstOrDefault(txt => txt.Name == "txt" + model.Name + "_UL");
                var llTextBox = ctrls.OfType<TextBox>().FirstOrDefault(txt => txt.Name == "txt" + model.Name + "_LL");
                var elTextBox = ctrls.OfType<TextBox>().FirstOrDefault(txt => txt.Name == "txt" + model.Name + "_EL");

                model.UL = ParseDoubleFromTextBox(ulTextBox) ?? model.UL;
                model.LL = ParseDoubleFromTextBox(llTextBox) ?? model.LL;
                model.EL = ParseDoubleFromTextBox(elTextBox) ?? model.EL;

                model.InsertOrUpdateData();
            });

            LimitValidator.LoadNodeLimitsFromDatabase();

            //輸出檔案
            var content = txtSavePath.Text + "\n" + txtBackupPath.Text;
            Utility.CreateOrUpdateConfigFile(_savePathName,content );
            MessageBox.Show("存檔成功");
        }

        private double? ParseDoubleFromTextBox(TextBox textBox)
        {
            if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
            {
                if (double.TryParse(textBox.Text, out double result))
                {
                    return result;
                }
            }
            return null;
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            try
            {
                #region 寫入資料庫

                //每分鐘存到資料庫
                if (tempInsert != DateTime.Now.Minute)
                {
                    tempInsert = DateTime.Now.Minute;

                    var dic = new Dictionary<string, object>()
                    {
                        { "sd_time",DateTime.Now},
                        { "sd_9F3E201", _modbusData.TI_200_1},
                        { "sd_9F4E201", _modbusData.TI_200_3},
                        { "sd_9F4E203", _modbusData.TI_300},
                        { "sd_9E4A201", _modbusData.TI_600_1},
                        { "sd_9D1A201", _modbusData.ADAM_6017_05},
                        { "sd_937P201", _modbusData._937P201},
                        { "sd_936P201", _modbusData.ADAM_6017_03},
                        { "sd_9O1A201", _modbusData.AC_001},
                        { "sd_948P201", _modbusData.Wind},
                    };

                    //檢查有無逾限
                    foreach (var kvp in dic)
                    {
                        try
                        {
                            var node = kvp.Key.Replace("sd_", "");
                            if (!LimitValidator.IsWithinTemperatureLimits(node, double.Parse(kvp.Value.ToString())))
                            {
                                Utility.LineNotify(node + " " + kvp.Value);
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    foreach (var gb in tpStatus.Controls.OfType<GroupBox>())
                    {
                        var key = gb.Name.Replace("grp", "sd_") + "_status";
                        var value = gb.Controls.OfType<RadioButton>().FirstOrDefault(rb => rb.Checked)?.Tag?.ToString();
                        dic[key] = value; // 添加或更新鍵值對到 dic 字典中
                    }

                    using (MySQL sql = new MySQL())
                    {
                        sql.InsertTable("sensor_data", dic);
                    }
                }

                #endregion

                // 檢查當前時間是否為每五分鐘的整數倍
                if (DateTime.Now.Minute % 5 == 0)
                {
                    // 確保 SavePath 有效的路徑
                    if (!string.IsNullOrEmpty(_savePathName))
                    {
                        // 確保每五分鐘只執行一次
                        if (DateTime.Now.Minute != _tempMinute)
                        {
                            _tempMinute = DateTime.Now.Minute;

                            // 設置儲存路徑並處理數據
                            _export.SavePath = txtSavePath.Text;
                            _export.BackupPath = txtBackupPath.Text;
                            _export.FetchAndProcessData(DateTime.Now);
                        }
                    }
                    else
                    {
                        Log.LogMsg("未輸出檔案 無存檔路徑");
                    }
                }
                else
                {
                    _tempMinute = -1; // 重置 _tempMinute，以便能夠再次觸發操作
                }
            }
            catch (Exception ex)
            {
                Log.LogMsg(ex.Message + "\n" + ex.StackTrace);
                tmrGetData.Enabled = false;
            }
        }

        private void tmrGetData_Tick(object sender, EventArgs e)
        {
            GetModbusData();
        }
    }
}
