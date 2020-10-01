using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace MFCSoftware.Models.Tests
{
    [TestClass()]
    public class ResolveActionAttributeTests
    {
        [TestMethod()]
        public void CheckAutomaticallyTest()
        {
            Func<SerialCommandType, string> getActionName = sct =>
            {
                var attr = sct.GetType().GetField(sct.ToString()).GetCustomAttribute<ResolveActionAttribute>();
                if (attr == null) return string.Empty;
                return attr.ActionName;
            };

            //不需要自动解析的情况
            byte[] data = new byte[1];
            SerialCommandType type = SerialCommandType.ReadFlow;
            Assert.ThrowsException<ArgumentException>(() => ResolveActionAttribute.CheckAutomatically(data, type), "AutoReset is false.");

            //需要自动解析但传入数组内容不正确的情况
            data = new byte[] { 0x01, 0x06, 0x02, 0x00, 0x09, 0x00, 0x00 };     //校验位不起作用
            type = SerialCommandType.ZeroPointCalibration;
            Assert.AreEqual(ResolveActionAttribute.CheckAutomatically(data, type), getActionName(type) + "失败。");

            //正确的情况
            data = new byte[] { 0x01, 0x06, 0x02, 0x00, 0x01, 0x00, 0x00 };     //校验位不起作用
            type = SerialCommandType.ZeroPointCalibration;
            Assert.AreEqual(ResolveActionAttribute.CheckAutomatically(data, type), getActionName(type) + "成功。");
        }
    }
}