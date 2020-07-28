using Microsoft.VisualStudio.TestTools.UnitTesting;
using CalibrationTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CalibrationTool.ViewModels.Tests
{
    [TestClass()]
    public class SerialPortViewModelTests
    {
        [TestMethod()]
        public void GetSerialPortFullNamesTest()
        {
            SerialPortViewModel vm = new SerialPortViewModel();
            string[] portNames = vm.GetSerialPortFullNames();
            foreach (string name in portNames)
                Debug.WriteLine(name);
            Assert.AreEqual(portNames.Length, 2);
        }
    }
}