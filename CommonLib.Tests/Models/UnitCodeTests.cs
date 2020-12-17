using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLib.Models.Tests
{
    [TestClass()]
    public class UnitCodeTests
    {
        [TestMethod()]
        public void GetUnitCodesFromConfigurationTest()
        {
            Assert.IsTrue(UnitCode.GetUnitCodesFromConfiguration().Count > 0);
        }
    }
}