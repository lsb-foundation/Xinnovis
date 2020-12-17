using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLib.Models.Tests
{
    [TestClass()]
    public class GasTypeCodeTests
    {
        [TestMethod()]
        public void GetGasTypeCodesFromConfigurationTest()
        {
            Assert.IsTrue(GasTypeCode.GetGasTypeCodesFromConfiguration().Count > 0);
        }
    }
}