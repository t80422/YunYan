using System;
using System.Collections.Generic;
using System.Linq;

namespace YunYan
{
    static class LimitValidator
    {
        public static Dictionary<string, double> TemperatureUpperLimit;
        public static Dictionary<string, double> TemperatureLowerLimit;
        public static Dictionary<string, int> RPMUpperLimit;
        public static Dictionary<string, int> RPMLowerLimit;

        static LimitValidator()
        {
            TemperatureUpperLimit = InitializeLimits<double>();
            TemperatureLowerLimit = InitializeLimits<double>();
            RPMUpperLimit = InitializeLimits<int>();
            RPMLowerLimit = InitializeLimits<int>();
        }

        private static Dictionary<string, T> InitializeLimits<T>()
        {
            var keys = Program.Codes.ToList();
            return keys.ToDictionary(key => key, key => default(T));
        }

        /// <summary>
        /// 設定溫度上下限
        /// </summary>
        /// <param name="target">目標點位</param>
        /// <param name="upperValue">上限值</param>
        /// <param name="lowerValue">下限值</param>
        public static void SetTemperatureLimits(string target, double upperValue, double lowerValue)
        {
            TemperatureUpperLimit[target] = upperValue;
            TemperatureLowerLimit[target] = lowerValue;
        }

        /// <summary>
        /// 設定轉數上下限
        /// </summary>
        /// <param name="target">目標點位</param>
        /// <param name="upperValue">上限值</param>
        /// <param name="lowerValue">下限值</param>
        public static void SetRPMLimits(string target, int upperValue, int lowerValue)
        {
            RPMUpperLimit[target] = upperValue;
            RPMLowerLimit[target] = lowerValue;
        }

        public static bool IsWithinTemperatureLimits(string name, double value)
        {
            // 檢查名稱是否存在於上下限字典中
            if (!TemperatureUpperLimit.ContainsKey(name) || !TemperatureLowerLimit.ContainsKey(name))            
                throw new ArgumentException(name + "不存在於上下限設定中。");           

            // 獲取上限和下限
            double upperLimit = TemperatureUpperLimit[name];
            double lowerLimit = TemperatureLowerLimit[name];

            // 判斷值是否在範圍內
            return value >= lowerLimit && value <= upperLimit;
        }

        public static bool IsWithinRPMLimits(string name, double value)
        {
            // 檢查名稱是否存在於上下限字典中
            if (!TemperatureUpperLimit.ContainsKey(name) || !TemperatureLowerLimit.ContainsKey(name))            
                throw new ArgumentException(name + "不存在於上下限設定中。");

            // 獲取上限和下限
            double upperLimit = RPMUpperLimit[name];
            double lowerLimit = RPMLowerLimit[name];

            // 判斷值是否在範圍內
            return value >= lowerLimit && value <= upperLimit;
        }
    }
}