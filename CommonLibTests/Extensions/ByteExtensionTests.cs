using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CommonLib.Extensions.Tests
{
    [TestClass()]
    public class ByteExtensionTests
    {
        [TestMethod()]
        public void CheckCRC16ByDefaultTest()
        {
            byte[] data = { 0x01, 0x03, 0x02, 0x29, 0x04, 0xA6, 0x17 };
            Stopwatch watch = new Stopwatch();
            watch.Start();
            bool check = data.CheckCRC16ByDefault();
            watch.Stop();
            Debug.WriteLine(watch.ElapsedMilliseconds);
            Assert.IsTrue(check);
        }
    }
}