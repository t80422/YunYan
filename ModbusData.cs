using System;
using System.Collections.Generic;

namespace YunYan
{
    internal class ModbusData
    {
        /// <summary>
        /// 數據的校正值集合
        /// </summary>
        private Dictionary<string, Correction> _correction = new Dictionary<string, Correction>();
        private double pDI_200;
        private double pI_300;
        private double pI_420;
        private double pI_600;
        private double tI_160_1;
        private double tI_600_3;
        private double tI_150;
        private double tI_160_2;
        private double tI_361;
        private double tI_362;
        private double tI_370_1;
        private double tI_381;
        private double tI_382;
        private double tI_370_2;
        private double tI_200_3;
        private double tI_200_1;
        private double tI_200_5;
        private double tI_200_4;
        private double tI_200_2;
        private double tI_200_12;
        private double tI_200_6;
        private double tI_300;
        private double tI_350_1;
        private double tI_350;
        private double tI_350_2;
        private double tI_643_1;
        private double tI_642_2;
        private double tI_642_1;
        private double tI_643_2;
        private double a201;
        private double _9OA2011;
        private double tI_600_1;
        private double tI_600_2;
        private double _948P2011;
        private double p201;
        private double _937P2011;
        private double tI_900;
        private double tI_380;
        private double wind;

        private struct Correction
        {
            public double Division;
            public double Addition;
        };
        private double Calculate(string key, double value)
        {
            double div = 1;
            double add = 0;

            if (_correction.ContainsKey(key))
            {
                var cor = _correction[key];
                div = cor.Division;
                add = cor.Addition;
            }

            return (value / div) + add;
        }

        public ModbusData(string[] corrections)
        {
            string key = "";

            try
            {
                for (int i = 1; i < corrections.Length; i++)
                {
                    var parts = corrections[i].Split(':');

                    if (parts.Length != 3)
                    {
                        throw new ArgumentException($"修正參數格式錯誤: {corrections[i]}");
                    }

                    key = parts[0];

                    if (string.IsNullOrEmpty(key))
                    {
                        throw new ArgumentException($"第{i}行參數名稱位填寫");
                    }

                    var division = double.Parse(parts[1]) == 0 ? 1 : double.Parse(parts[1]);

                    Correction correction = new Correction
                    {
                        Division = division,
                        Addition = double.Parse(parts[2])
                    };

                    _correction.Add(key, correction);
                }
            }
            catch (FormatException ex)
            {
                throw new FormatException($"參數設定錯誤,請檢查: {key}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"初始化 ModbusData 時發生錯誤: {ex.Message}", ex);
            }
        }

        public double CC_101_HZ { get; set; }
        public double CC_101_rpm { get; set; }
        public double TI_150 { get => Calculate("TI_150", tI_150); set => tI_150 = value; }
        public double RK_150_HZ { get; set; }
        public double RK_150_rpm { get; set; }
        public double SC_151_HZ { get; set; }
        public double SC_151_rpm { get; set; }
        public double CC_163_HZ { get; set; }
        public double CC_163_rpm { get; set; }
        public double TI_160_1 { get => Calculate("TI_160_1", tI_160_1); set => tI_160_1 = value; }
        public double TI_160_2 { get => Calculate("TI_160_2", tI_160_2); set => tI_160_2 = value; }
        public double TI_380 { get =>Calculate("TI_380", tI_380); set => tI_380 = value; }
        public double PI_420 { get => Calculate("PI_420", pI_420); set => pI_420 = value; }
        public double FDF_360_HZ { get; set; }
        public double FDF_360_rpm { get; set; }
        public double TI_361 { get => Calculate("TI_361", tI_361); set => tI_361 = value; }
        public double TI_362 { get => Calculate("TI_362", tI_362); set => tI_362 = value; }
        public double TI_370_1 { get => Calculate("TI_370_1", tI_370_1); set => tI_370_1 = value; }
        public double TI_381 { get => Calculate("TI_381", tI_381); set => tI_381 = value; }
        public double FDF_380_HZ { get; set; }
        public double FDF_380_rpm { get; set; }
        public double TI_382 { get => Calculate("TI_382", tI_382); set => tI_382 = value; }
        public double TI_370_2 { get => Calculate("TI_370_2", tI_370_2); set => tI_370_2 = value; }
        public double SC_370_HZ { get; set; }
        public double SC_370_rpm { get; set; }
        public double CC_220_HZ { get; set; }
        public double CC_220_rpm { get; set; }
        public double FDF_210_HZ { get; set; }
        public double FDF_210_rpm { get; set; }
        public double TI_200_3 { get => Calculate("TI_200_3", tI_200_3); set => tI_200_3 = value; }
        public double TI_200_1 { get => Calculate("TI_200_1", tI_200_1); set => tI_200_1 = value; }
        public double TI_200_2 { get => Calculate("TI_200_2", tI_200_2); set => tI_200_2 = value; }
        public double TI_200_12 { get => Calculate("TI_200_12", tI_200_12); set => tI_200_12 = value; }
        public double TI_200_4 { get => Calculate("TI_200_4", tI_200_4); set => tI_200_4 = value; }
        public double TI_200_5 { get => Calculate("TI_200_5", tI_200_5); set => tI_200_5 = value; }
        public double TI_200_6 { get => Calculate("TI_200_6", tI_200_6); set => tI_200_6 = value; }
        public double PDI_200 { get => Calculate("PDI_200", pDI_200); set => pDI_200 = value; }
        public double FDF_310_HZ { get; set; }
        public double FDF_310_rpm { get; set; }
        public double PI_300 { get => Calculate("PI_300", pI_300); set => pI_300 = value; }
        public double TI_300 { get => Calculate("TI_300", tI_300); set => tI_300 = value; }
        public double TI_350_1 { get => Calculate("TI_350_1", tI_350_1); set => tI_350_1 = value; }
        public double TI_350 { get => Calculate("TI_350", tI_350); set => tI_350 = value; }
        public double TI_350_2 { get => Calculate("TI_350_2", tI_350_2); set => tI_350_2 = value; }
        public double FDF_320_HZ { get; set; }
        public double FDF_320_rpm { get; set; }
        public double TI_643_1 { get => Calculate("TI_643_1", tI_643_1); set => tI_643_1 = value; }
        public double TI_642_2 { get => Calculate("TI_642_2", tI_642_2); set => tI_642_2 = value; }
        public double TI_642_1 { get => Calculate("TI_642_1", tI_642_1); set => tI_642_1 = value; }
        public double TI_643_2 { get => Calculate("TI_643_2", tI_643_2); set => tI_643_2 = value; }
        public double A201 { get => Calculate("A201", a201); set => a201 = value; }
        public double _9OA201 { get => Calculate("9O1A201", _9OA2011); set => _9OA2011 = value; }
        public double TI_600_1 { get => Calculate("TI_600_1", tI_600_1); set => tI_600_1 = value; }
        public double PI_600 { get => Calculate("PI_600", pI_600); set => pI_600 = value; }
        public double TI_600_2 { get => Calculate("TI_600_2", tI_600_2); set => tI_600_2 = value; }
        public double TI_600_3 { get => Calculate("TI_600_3", tI_600_3); set => tI_600_3 = value; }
        public double IDF_900_HZ { get; set; }
        public double IDF_900_rpm { get; set; }
        public double _948P201 { get => Calculate("_948P201", _948P2011); set => _948P2011 = value; }
        public double P201 { get => Calculate("P201", p201); set => p201 = value; }
        public double _937P201 { get => Calculate("937P201", _937P2011); set => _937P2011 = value; }
        public double TI_900 { get => Calculate("TI_900", tI_900); set => tI_900 = value; }
        public double Wind { get =>Calculate("wind", wind); set => wind = value; }
    }
}
