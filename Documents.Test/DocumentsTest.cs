using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;
using Documents.Entities;
using Moq;
using Documents.Storage;

namespace Documents.Test
{
    [TestClass]
    public class DocumentsTest
    {
        IDocumentsContext Context;
        Documents Documents;

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

            var blobMoq = new Mock<IBlobStore>();
            Documents = new Documents(Context, blobMoq.Object);
        }

        [TestMethod]
        public void Documents_CreateStore_ReturnsValidStore()
        {
            var store = Documents.CreateStore();

            Assert.IsNotNull(store, "Return value is null");
            Assert.IsNotNull(store.RootFolder, "Returned storeA returns null as Root");
        }

        [TestMethod]
        public void Documents_GetStore_ReturnsValidStore()
        {
            var store = Documents.CreateStore();
            var retrievedStore = Documents.GetStore(store.Id);

            Assert.IsNotNull(retrievedStore, "Return value is null");
        }
    }
}
