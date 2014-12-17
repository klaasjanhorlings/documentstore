using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Documents.Storage;
using Moq;
using System.Text;

namespace Documents.Test
{
    [TestClass]
    public class BlobStoreTest
    {
        private static string PathRoot;
        private static Mock<IPathStrategy> PathStrategyMock;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            PathRoot = Path.Combine(new string[] { Path.GetTempPath(), "docTest" });
            Directory.CreateDirectory(PathRoot);

            PathStrategyMock = new Mock<IPathStrategy>();
        }

        [ClassCleanup]
        public static void CleanUp()
        {
            Directory.Delete(PathRoot, true);
        }

        [TestMethod]
        public void BlobStore_Store_ReturnsSha1HashForPassedStream()
        {
            var expectedHash = new byte[] { 0x59, 0xd9, 0xa6, 0xdf, 0x06, 0xb9, 0xf6, 0x10, 0xf7, 0xdb, 0x8e, 0x03, 0x68, 0x96, 0xed, 0x03, 0x66, 0x2d, 0x16, 0x8f };
            using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("Hallo")))
            {

            }
        }
    }
}
