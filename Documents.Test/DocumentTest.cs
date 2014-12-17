using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Documents.Entities;
using System.Data.Entity;
using Moq;
using Documents.Storage;
using System.IO;
using System.Linq;
using System.Text;

namespace Documents.Test
{
    [TestClass]
    public class DocumentTest
    {
        IDocumentsContext Context;
        Documents Documents;
        DocumentStore Store;
        Mock<IBlobStore> BlobMock;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            Database.SetInitializer<Context>(new DropCreateDatabaseAlways<Context>());
        }

        [TestInitialize]
        public void Setup()
        {
            Context = new Context();
            ((DbContext)Context).Database.Initialize(false);

            BlobMock = new Mock<IBlobStore>();
            Documents = new Documents(Context, BlobMock.Object);
            Store = Documents.CreateStore();
        }

        [TestMethod]
        public void Folder_CreateDocument_ReturnsValidDocument()
        {
            Document document;
            using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("Hallo")))
            { 
                document = Store.RootFolder.CreateDocument("Test", stream);
            }

            Assert.IsNotNull(document);
        }

        [TestMethod]
        public void Folder_CreateDocument_UpdatesGraph()
        {
            Document document;
            using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("Hallo")))
            {
                document = Store.RootFolder.CreateDocument("Test", stream);
            }

            Assert.IsNotNull(Store.RootFolder.Documents.FirstOrDefault(d => d.Name == "Test"),
                "RootFolder does not contain created document");
        }

        [TestMethod]
        public void Folder_CreateDocument_IsPersisted()
        {
            Document document;
            using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("Hallo")))
            {
                document = Store.RootFolder.CreateDocument("Test", stream);
            }

            var retrievedStore = (new Documents(new Context(), new Mock<IBlobStore>().Object)).GetStore(Store.Id);

            Assert.IsNotNull(retrievedStore.RootFolder.Documents.FirstOrDefault(d => d.Name == "Test"),
                "RootFolder does not contain created document");
        }

        [TestMethod]
        public void Folder_CreateDocument_CallsBlobStore()
        {
            using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("Hallo")))
            {
                BlobMock.Setup(blob => blob.Store(stream)).Returns(new byte[]{}).Verifiable();
                Store.RootFolder.CreateDocument("Test", stream);
            }
            BlobMock.Verify();
        }

        [TestMethod]
        public void Document_Rename_UpdatesNameAndIsPersisted()
        {
            Document document;
            using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("Hallo")))
            {
                document = Store.RootFolder.CreateDocument("Test", stream);
            }

            document.Rename("Renamed");
            Assert.AreEqual("Renamed", document.Name, "Name does not match passed rename");

            var retrievedStore = (new Documents(new Context(), new Mock<IBlobStore>().Object)).GetStore(Store.Id);
            Assert.IsNotNull(retrievedStore.RootFolder.Documents.FirstOrDefault(d => d.Name == "Renamed"),
                "RootFolder does not contain renamed document");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Document_Move_ThrowsExceptionWhenMovingToAnotherStore()
        {
            using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("Hallo")))
            {
                var document = Store.RootFolder.CreateDocument("Test", stream);
                var storeB = Documents.CreateStore();
                document.Move(storeB.RootFolder);
            }
        }

        [TestMethod]
        public void Document_Move_UpdatesGraph()
        {
            using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("Hallo")))
            {
                var document = Store.RootFolder.CreateDocument("Test", stream);
                var folder = Store.RootFolder.CreateFolder("Folder");
                document.Move(folder);

                Assert.AreEqual(folder.Id, document.Parent.Id, "Parent of moved object is not set to requested target");
                Assert.IsNotNull(folder.Documents.FirstOrDefault(d => d.Name == "Test"), "Target does not contain moved document");
                Assert.IsNull(Store.RootFolder.Documents.FirstOrDefault(d => d.Name == "Test"), "Original parent still contains moved folder");
            }
        }

        [TestMethod]
        public void Document_Move_IsPersisted()
        {
            using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("Hallo")))
            {
                var document = Store.RootFolder.CreateDocument("Test", stream);
                var folder = Store.RootFolder.CreateFolder("Folder");
                document.Move(folder);

                var retrievedStore = (new Documents(new Context(), BlobMock.Object)).GetStore(Store.Id);
                var retrievedFolder = retrievedStore.RootFolder.Folders.First();
                Assert.IsNotNull(retrievedFolder.Documents.FirstOrDefault(d => d.Name == "Test"), "Target does not contain moved document");
            }
        }

        [TestMethod]
        public void Document_Delete_UpdatesGraph()
        {
            using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("Hallo")))
            {
                var document = Store.RootFolder.CreateDocument("Test", stream);
                document.Delete();

                Assert.IsNull(Store.RootFolder.Documents.FirstOrDefault(d => d.Name == "Test"), "Target still contains deleted document");
            }
        }

        [TestMethod]
        public void Document_Delete_IsPersisted()
        {
            using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("Hallo")))
            {
                var document = Store.RootFolder.CreateDocument("Test", stream);
                document.Delete();

                var retrievedStore = (new Documents(new Context(), BlobMock.Object)).GetStore(Store.Id);
                Assert.IsNull(retrievedStore.RootFolder.Documents.FirstOrDefault(d => d.Name == "Test"), "Target still contains deleted document");
            }
        }

        [TestMethod]
        public void Document_Delete_CallsBlobStoreOnLastCopy()
        {
            BlobMock.Setup(blob => blob.Store(It.IsAny<Stream>())).Returns(new byte[] { 1, 1, 2, 3, 5, 8 });
            BlobMock.Setup(blob => blob.Delete(It.IsAny<byte[]>())).Verifiable();
            using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("Hallo")))
            {
                var docA = Store.RootFolder.CreateDocument("Test A", stream);
                stream.Position = 0;
                var docB = Store.RootFolder.CreateDocument("Test B", stream);

                docA.Delete();
                BlobMock.Verify(blob => blob.Delete(It.IsAny<byte[]>()), Times.Never, "Blob is deleted while there is still a reference left");
                
                docB.Delete();
                BlobMock.Verify(blob => blob.Delete(It.IsAny<byte[]>()), Times.Once, "Blob is not deleted when there are no references left");
            }
        }
    }
}
