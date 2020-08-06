using Microsoft.VisualStudio.TestTools.UnitTesting;
using MFCSoftware.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFCSoftware.Models;
using System.Diagnostics;

namespace MFCSoftware.Common.Tests
{
    [TestClass()]
    public class DbStorageTests
    {
        private string[] units = { "SCCM", "SLM", "ULM", "SCM", "CCM" };

        [TestMethod()]
        public void InsertFlowDataTest()
        {
            Random random = new Random();
            for (int index = 0; index < 1000; index++)
            {
                int addr = index % 5;
                var flow = new FlowData()
                {
                    CurrentFlow = (float)random.NextDouble() * 1000,
                    AccuFlow = (float)random.NextDouble() * 1000,
                    Unit = units[addr]
                };
                DbStorage.InsertFlowData(addr, flow);
            }
        }

        [TestMethod()]
        public void QueryLast2HoursFlowDataTest()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var flows = DbStorage.QueryLastest2HoursFlowData(1);
            watch.Stop();
            Console.WriteLine($"Get {flows.Count} Flows.");
            Console.WriteLine(watch.ElapsedMilliseconds);
            Assert.IsTrue(flows.Count > 0);
        }
    }
}