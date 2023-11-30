using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YunYan
{
    public partial class Form1 : Form
    {
        private string _divisoFileName = "_divisor.set";
        private string _savePathName = "SavePath.set";
        private ReportExporter _export = new ReportExporter();
        private int _timerCount = 0;

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

        private void tmrData_Tick(object sender, EventArgs e)
        {
            //ShowData();

            _timerCount += 5;

            try
            {
                //每分鐘存到資料庫
                if (_timerCount == 60)
                {
                    using (MySQL sql = new MySQL())
                    {
                        sql.InsertTable("sensor_data", GetData());
                    }

                    _timerCount = 0;
                }

                //每五分鐘輸出檔案
                if (DateTime.Now.Minute % 5 == 0 && !string.IsNullOrEmpty(txtSavePath.Text))
                {
                    _export.SavePath = txtSavePath.Text;
                    _export.FetchAndProcessData(DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                tpConversion.BackColor = Color.Red;
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                tmrData.Enabled = false;
            }
        }

        //private void ShowData()
        //{
        //    //會有一次有資料一次沒資料的情況
        //    do
        //    {
        //        GetNModbusData();
        //    } while (txt9D1A201.Text == "0.00");

        //}

        //private void GetNModbusData()
        //{
        //    var nModbus15 = new NModbus("192.168.0.15", 502);
        //    var uid15_1 = nModbus15.GetData(1);
        //    var uid15_2 = nModbus15.GetData(2);

        //    txt9F3E201_sc.Text = uid15_1[26].ToString();
        //    txt9F4E201_sc.Text = uid15_1[28].ToString();
        //    txt9F4E203_sc.Text = uid15_2[0].ToString();
        //    txt9E4A201_sc.Text = uid15_2[16].ToString();

        //    nModbus15.Dispose();

        //    var nModbus10 = new NModbus("192.168.0.10", 502);
        //    var uid10 = nModbus10.GetData(1);

        //    txt9D1A201_sc.Text = uid10[4].ToString();
        //    txt936P201_sc.Text = uid10[2].ToString();
        //    txt9O1A201_sc.Text = uid10[5].ToString();
        //    txt948P201_sc.Text = uid10[6].ToString();

        //    nModbus10.Dispose();
        //}

        private Dictionary<string, object> GetData()
        {
            var dicData = new Dictionary<string, object>
            {
                { "sd_time",DateTime.Now},
                { "sd_9F3E201", txt9F3E201.Text},
                { "sd_9F4E201", txt9F4E201.Text},
                { "sd_9F4E203", txt9F4E203.Text},
                { "sd_9E4A201", txt9E4A201.Text},
                { "sd_9D1A201", txt9D1A201.Text},
                { "sd_937P201", txt937P201.Text},
                { "sd_936P201", txt936P201.Text},
                { "sd_9O1A201", txt9O1A201.Text},
                { "sd_948P201", txt948P201.Text},
                { "sd_9F3E201_sc", txt9F3E201_sc.Text},
                { "sd_9F4E201_sc", txt9F4E201_sc.Text},
                { "sd_9F4E203_sc", txt9F4E203_sc.Text},
                { "sd_9E4A201_sc", txt9E4A201_sc.Text},
                { "sd_9D1A201_sc", txt9D1A201_sc.Text},
                { "sd_936P201_sc", txt936P201_sc.Text},
                { "sd_9O1A201_sc", txt9O1A201_sc.Text},
                { "sd_948P201_sc", txt948P201_sc.Text},
            };

            return dicData;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //ShowData();
            //SetNumericUpDown_divisor();

            //var savePaths = Utility.ReadConfigFile(_savePathName);
            //if (savePaths != null)
            //    txtSavePath.Text = savePaths[0];

            ////CorrectionValue(new object(), EventArgs.Empty);
            //InitializeEventHandlers();

            //foreach (var name in Program.Codes)
            //{
            //    LimitValidator.SetTemperatureLimits(name, 100, 0);
            //    LimitValidator.SetRPMLimits(name, 10, 10);
            //}
            //=========測試區==========           
            //tmrData.Interval = 10000;
            //_export.SetCycleIntervalMinutes(10);
            tmrData.Enabled = false;
            //=========測試區==========
        }

        private void InitializeEventHandlers()
        {
            var numericUpDownControls = tpConversion.Controls.OfType<NumericUpDown>();

            foreach (var control in numericUpDownControls)
            {
                if (control.Name.Contains("_divisor") || control.Name.Contains("_as"))
                {
                    control.ValueChanged += new EventHandler(CorrectionValue);
                }
            }

            var textBoxControls = tpConversion.Controls.OfType<TextBox>().Where(x => x.Name.Contains("_sc"));

            foreach (var control in textBoxControls)
            {
                control.TextChanged += new EventHandler(CorrectionValue);
            }
        }

        /// <summary>
        /// 計算值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CorrectionValue(object sender, EventArgs e)
        {
            //var nudsDivisor = tpConversion.Controls.OfType<NumericUpDown>().Where(x => x.Name.Contains("_divisor") && x.Name != "nud948P201_divisor");
            var nudsDivisor = tpConversion.Controls.OfType<NumericUpDown>().Where(x => x.Name.Contains("_divisor"));
            foreach (var nud in nudsDivisor)
            {
                var ctrls = tpConversion.Controls;
                var match = Regex.Match(nud.Name, @"nud(.+)_divisor");
                var name = match.Groups[1].Value;
                var source = ctrls.OfType<TextBox>().First(x => x.Name == "txt" + name + "_sc");
                var target = ctrls.OfType<TextBox>().First(x => x.Name == "txt" + name);
                var addAndSub = ctrls.OfType<NumericUpDown>().First(x => x.Name == "nud" + name + "_as");

                target.Text = ((decimal.Parse(source.Text) / nud.Value) + addAndSub.Value).ToString("F2");
            }

            //txt948P201.Text = Utility.CalculateExhaustAirflow((decimal)95.5, (decimal)761.3, (decimal)-0.14, (decimal)0.385, ((decimal.Parse(txt948P201_sc.Text) / nud948P201_divisor.Value) - nud948P201_as.Value)).ToString();
            txt937P201.Text = (double.Parse(txt948P201.Text) * (double.Parse(txt936P201.Text) - 21) / 10).ToString("F2");

        }

        private void SetNumericUpDown_divisor()
        {
            var content = Utility.ReadConfigFile(_divisoFileName);

            if (content == null || content.Length == 0)
            {
                tpConversion.Controls.OfType<NumericUpDown>().Where(x => x.Name.Contains("_divisor")).ToList().ForEach(x => x.Value = 1);
            }
            else
            {
                foreach (var line in content)
                {
                    var parts = line.Split('=');
                    var name = parts[0];
                    var value = decimal.Parse(parts[1]);

                    tpConversion.Controls.OfType<NumericUpDown>().FirstOrDefault(x => x.Name == name).Value = value;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var content = string.Join(Environment.NewLine,
                 tpConversion.Controls.OfType<NumericUpDown>()
                 .Where(x => x.Name.Contains("_divisor"))
                 .Select(x => x.Name + "=" + x.Value.ToString()));

            Utility.CreateOrUpdateConfigFile(_divisoFileName, content);

            //存檔位置
            Utility.CreateOrUpdateConfigFile(_savePathName, txtSavePath.Text);
            MessageBox.Show("儲存成功");
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            tmrData.Enabled = true;
            BackColor = SystemColors.Control;
        }

        private void tmrModbus_Tick(object sender, EventArgs e)
        {
            var modbus10 = _modbus_10.GetData();
            _modbusData.Wind = modbus10[6];

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
            lblTI_160_1.Text=_modbusData.TI_160_1.ToString();
            lblTI_160_2.Text = _modbusData.TI_160_2.ToString();
            lblTI_380.Text=_modbusData.TI_380.ToString();
            lblPI_420.Text=_modbusData.PI_420.ToString();
            lblTI_361.Text=_modbusData.TI_361.ToString();
            lblTI_362.Text=_modbusData.TI_362.ToString();
            lblTI_370_1.Text=_modbusData.TI_370_1.ToString();
            lblTI381.Text=_modbusData.TI_381.ToString();
            lblTI_382.Text=_modbusData.TI_382.ToString();
            lblTI_370_2.Text=_modbusData.TI_370_2.ToString();
            lblTI_380_1.Text=_modbusData.TI_380.ToString() ;
            lblTI_200_3.Text=_modbusData.TI_200_3.ToString() ;
            lblTI_200_1.Text=_modbusData.TI_200_1.ToString() ;
            lblTI_200_5.Text=_modbusData.TI_200_5 .ToString() ;
            lblTI_200_4.Text=_modbusData.TI_200_4.ToString() ;
            lblPDI_200.Text=_modbusData.PDI_200.ToString() ;
            lblTI_200_2.Text=_modbusData.TI_200_2.ToString() ;
            lblTI_200_12.Text=_modbusData.TI_200_12.ToString() ;
            lblTI_200_6.Text = _modbusData.TI_200_6.ToString();
            lblPI_300.Text=_modbusData.PI_300.ToString() ;
            lblTI_300.Text=_modbusData.TI_300.ToString() ;
            lblTI_350_1.Text=_modbusData.TI_350_1.ToString() ;
            lblTI_350.Text=_modbusData.TI_350.ToString() ;
            lblTI_350_2.Text=_modbusData.TI_350_2.ToString() ;
            lblTI_643_1.Text = _modbusData.TI_643_1.ToString();
            lblTI_642_2.Text=_modbusData.TI_642_2.ToString() ;
            lblTI_642_1.Text=_modbusData.TI_642_1.ToString() ;
            lblTI_643_2.Text=_modbusData.TI_643_2.ToString() ;
            lblA201.Text = _modbusData.A201.ToString();
            lbl9O1A201.Text=_modbusData._9OA201.ToString() ;
            lblTI_600_1.Text=_modbusData.TI_600_1 .ToString() ;
            lblPI_600.Text=_modbusData.PI_600.ToString() ;
            lblTI_600_3.Text=_modbusData .TI_600_3.ToString() ;
            lblTI_600_2.Text=_modbusData.TI_600_2.ToString() ;
            lbl948P201.Text=_modbusData._948P201.ToString() ;
            lblP201.Text = _modbusData.P201.ToString();
            lbl937P201.Text=_modbusData._937P201 .ToString() ;
            lblTI_900.Text=_modbusData.TI_900.ToString() ;
            txtTI_900.Text=_modbusData.TI_900 .ToString() ;
            txtWind.Text=_modbusData.Wind.ToString() ;
        }
    }
}
