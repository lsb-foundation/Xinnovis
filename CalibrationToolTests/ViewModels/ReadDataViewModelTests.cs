using Microsoft.VisualStudio.TestTools.UnitTesting;
using CalibrationTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalibrationTool.Models;
using System.Reflection;

namespace CalibrationTool.ViewModels.Tests
{
    [TestClass()]
    public class ReadDataViewModelTests
    {
        [TestMethod()]
        public void SetDebugDataTest()
        {
            ReadDataViewModel reader = new ReadDataViewModel();
            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();
            foreach(var property in typeof(DebugData).GetProperties())
            {
                var attr = property.GetCustomAttribute<DebugDataMapperAttribute>();
                if(attr != null)
                {
                    var kv = new KeyValuePair<string, string>(attr.MappedKey, attr.MappedKey);
                    keyValues.Add(kv);
                }
            }
            foreach(var kv in keyValues)
            {
                reader.SetDebugData(kv);
            }

            Assert.AreEqual(reader.SN, "SN");
        }
    }
}