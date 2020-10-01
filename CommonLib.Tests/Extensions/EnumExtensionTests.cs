using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLib.Extensions.Tests
{
    [TestClass()]
    public class EnumExtensionTests
    {
        [TestMethod()]
        public void GetEnumListTest()
        {
            var testEnumList = EnumExtension.GetEnumList<TestEnum>();
            Assert.AreEqual(testEnumList[0], TestEnum.Field1);
            Assert.AreEqual(testEnumList[1], TestEnum.Field2);
            Assert.AreEqual(testEnumList[2], TestEnum.Field3);
        }

        enum TestEnum
        {
            Field1,
            Field2,
            Field3
        }
    }
}