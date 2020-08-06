using Microsoft.VisualStudio.TestTools.UnitTesting;
using CalibrationTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalibrationTool.Models;
using System.Reflection;
using CalibrationTool.ResolveUtils;
using CommonLib.Communication.Serial;

namespace CalibrationTool.ViewModels.Tests
{
    [TestClass()]
    public class ReadDataViewModelTests
    {
        private string dataToTest = @"modbus addr:1
modbus baud:2
SN:0058 0062 354E 5001 2033 3058
K of 1-5V:0.004500
D of 1-5V:10.430000
D of 4-20mA:10.430000
Type of GAS:Air
Range:30
UNIT:SLM
V0 of cali flow:474.975464mv
V1 of cali flow:518.366760mv
V2 of cali flow:603.537903mv
V3 of cali flow:1029.318726mv
V4 of cali flow:1251.243652mv
V5 of cali flow:1405.174072mv
V6 of cali flow:1540.992798mv
V7 of cali flow:1668.872437mv
V8 of cali flow:1792.106323mv
V9 of cali flow:1911.281982mv
V10 of cali flow:2023.729858mv
V11 of cali flow:2200.037354mv
V12 of cali flow:2321.963135mv
V13 of cali flow:2401.676025mv
V14 of cali flow:2465.815430mv
V15 of cali flow:2521.425293mv
V16 of cali flow:2568.173096mv
V17 of cali flow:2607.337402mv
V18 of cali flow:2639.390869mv
V19 of cali flow:2667.857910mv
V20 of cali flow:2692.401611mv
V21 of cali flow:0.000000mv
K1 of cali flow:0.018348
K2 of cali flow:0.021944
K3 of cali flow:0.025686
K4 of cali flow:0.028988
K5 of cali flow:0.032404
K6 of cali flow:0.035773
K7 of cali flow:0.039434
K8 of cali flow:0.042602
K9 of cali flow:0.045108
K10 of cali flow:0.047977
K11 of cali flow:0.051364
K12 of cali flow:0.054235
K13 of cali flow:0.057687
K14 of cali flow:0.060270
K15 of cali flow:0.063925
K16 of cali flow:0.066073
K17 of cali flow:0.066146
K18 of cali flow:0.069004
K19 of cali flow:0.071234
K20 of cali flow:0.072846
K21 of cali flow:0.000000
T of cali flow:0.00";

        [TestMethod()]
        public void SetDebugDataTest()
        {
            ReadDataViewModel reader = new ReadDataViewModel();
            IResolve<byte[], List<KeyValuePair<string, string>>> debugResolve = new DebugDataResolve();
            byte[] bytes = Encoding.Default.GetBytes(dataToTest);
            var data = debugResolve.Resolve(bytes);
            foreach(var kv in data)
            {
                reader.SetDebugData(kv);
            }
            Assert.AreEqual(reader.DebugData.SN, "0058 0062 354E 5001 2033 3058");
            Assert.AreEqual(reader.DebugData.Unit, "SLM");
            Assert.AreEqual(reader.DebugData.Range, "30");
            Assert.AreEqual(reader.DebugData.GasType, "Air");
            Assert.AreEqual(reader.DebugData.KOf1_5, "0.004500");
            Assert.AreEqual(reader.DebugData.DOf1_5, "10.430000");
            Assert.AreEqual(reader.DebugData.TOfCaliFlow, "0.00");
            Assert.AreEqual(reader.DebugData.ModbusAddr, "1");
            Assert.AreEqual(reader.DebugData.ModbusBaud, BaudRateCode.GetBaudRateCodes().FirstOrDefault(v => v.Code == 2).BaudRate.ToString());
        }
    }
}