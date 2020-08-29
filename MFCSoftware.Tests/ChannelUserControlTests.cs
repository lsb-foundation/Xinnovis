using Microsoft.VisualStudio.TestTools.UnitTesting;
using MFCSoftware.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MFCSoftware.Models;

namespace MFCSoftware.Views.Tests
{
    [TestClass()]
    public class ChannelUserControlTests
    {
        [TestMethod()]
        public void ExportExcelForTestTest()
        {
            string userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            string file = Path.Combine(userProfile, "test.xlsx");
            string[] units = "SCCM|SLM|UCCM|CCM".Split('|');
            Random random = new Random();
            List<FlowData> flows = new List<FlowData>();
            float accuFlow = 0f;
            for (int index = 0; index < 1000; index++)
            {
                float currFlow = (float)random.NextDouble();
                accuFlow += currFlow;
                FlowData data = new FlowData
                {
                    CollectTime = DateTime.Now,
                    CurrentFlow = currFlow,
                    Unit = units[index % 4],
                    AccuFlow = accuFlow,
                    AccuFlowUnit = "SCCM"
                };
                flows.Add(data);
            }
            ChannelUserControl.ExportExcelForTest(file, flows);
        }
    }
}