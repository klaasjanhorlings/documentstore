using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Documents.Entities;
using System.Data.Entity;
using System.Linq;
using Moq;
using Documents.Storage;

namespace Documents.Test
{
    [TestClass]
    public class FolderTest
    {
        IDocumentsContext Context;
        Documents Documents;
        DocumentStore Store;

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
            Store = Documents.CreateStore();
        }

        [TestMethod]
        public void Folder_CreateFolder_ReturnsValidFolder()
        {
            var folder = Store.RootFolder.CreateFolder("Hallo");

            Assert.IsNotNull(folder, "Returns a null");
            Assert.AreEqual(Store.RootFolder.Id, folder.Parent.Id, "Returned does not object refer to original folder as parent");
            Assert.AreEqual("Hallo", folder.Name, "Returned object does not have passed name");
        }

        [TestMethod]
        public void Folder_CreateFolder_UpdatesGraph()
        {
            var child = Store.RootFolder.CreateFolder("Child A");
            Assert.IsNotNull(Store.RootFolder.Folders.FirstOrDefault(f => f.Name == "Child A"),
                "Root folder does not contain expected folder");

            child.CreateFolder("Grand Child");
            Assert.IsNotNull(child.Folders.FirstOrDefault(f => f.Name == "Grand Child"),
                "Folder does not contain expected folder");
        }

        [TestMethod]
        public void Folder_CreateFolder_IsPersisted()
        {
            var child = Store.RootFolder.CreateFolder("Child");
            var grandChild = child.CreateFolder("Grand Child");

            // Use new context to make sure we dont load a cached entity
            var retrievedStore = (new Documents(new Context(), new Mock<IBlobStore>().Object)).GetStore(Store.Id);
            var rootFolders = retrievedStore.RootFolder.Folders;
            var retrievedChild = rootFolders.FirstOrDefault(f => f.Name == "Child");
            Assert.IsNotNull(retrievedChild,
                "Root folder does not contain expected folder");
            Assert.IsNotNull(retrievedChild.Parent,
                "Folder Child does not refer to Parent");
            Assert.IsNotNull(retrievedChild.Folders.FirstOrDefault(f => f.Name == "Grand Child"),
                "Folder Child does not contain expected folder");
            Assert.IsNull(rootFolders.FirstOrDefault(f => f.Name == "Grand Child"),
                "Root folder contains unexpected ancestor folder");
        }

        [TestMethod]
        public void Folder_Move_UpdatesGraph()
        {
            var child = Store.RootFolder.CreateFolder("Child");
            var grandChild = child.CreateFolder("Grand Child");
            grandChild.Move(Store.RootFolder);

            Assert.AreEqual(Store.RootFolder.Id, grandChild.Parent.Id, "Parent of moved object is not set to requested target");
            Assert.IsNotNull(Store.RootFolder.Folders.FirstOrDefault(f => f.Name == "Grand Child"), "Target does not contain moved folder");
            Assert.IsNull(child.Folders.FirstOrDefault(f => f.Name == "Grand Child"), "Original parent still contains moved folder");
        }

        [TestMethod]
        public void Folder_Move_IsPersisted()
        {
            var child = Store.RootFolder.CreateFolder("Child");
            var grandChild = child.CreateFolder("Grand Child");
            grandChild.Move(Store.RootFolder);

            var retrievedStore = (new Documents(new Context(), new Mock<IBlobStore>().Object)).GetStore(Store.Id);
            var retrievedChild = retrievedStore.RootFolder.Folders.FirstOrDefault(f => f.Name == "Child");

            Assert.IsNotNull(retrievedStore.RootFolder.Folders.FirstOrDefault(f => f.Name == "Grand Child"), 
                "Root should but doesnt contain moved folder");
            Assert.IsNull(retrievedChild.Folders.FirstOrDefault(f => f.Name == "Grand Child"),
                "Original folder should'nt but does contain moved folder");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Folder_Move_ThrowsExceptionWhenMovingToAnotherStore()
        {
            var child = Store.RootFolder.CreateFolder("Child");
            var storeB = Documents.CreateStore();
            child.Move(storeB.RootFolder);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Folder_Move_ThrowsExceptionWhenMovingRoot()
        {
            var child = Store.RootFolder.CreateFolder("Child");
            Store.RootFolder.Move(child);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Folder_Move_ThrowsExceptionWhenMovingToDescendent()
        {
            var child = Store.RootFolder.CreateFolder("Child");
            var grandChild = child.CreateFolder("Child");
            child.Move(grandChild);
        }

        [TestMethod]
        public void Folder_Rename_UpdatesNameAndIsPersisted()
        {
            var child = Store.RootFolder.CreateFolder("Child");
            child.Rename("Kid");

            Assert.AreEqual("Kid", child.Name, "Name isn't updated");

            var retrievedStore = (new Documents(new Context(), new Mock<IBlobStore>().Object)).GetStore(Store.Id);
            Assert.IsNotNull(retrievedStore.RootFolder.Folders.FirstOrDefault(f => f.Name == "Kid"),
                "Root should but doesnt contain renamed folder");
        }
        
        [TestMethod]
        public void Folder_Delete_UpdatesGraph()
        {
            var child = Store.RootFolder.CreateFolder("Child");
            child.Delete();
            Assert.AreEqual(0, Store.RootFolder.Folders.Count, "Parent Folder still contains deleted folder");
        }

        [TestMethod]
        public void Folder_Delete_IsPersisted()
        {
            var child = Store.RootFolder.CreateFolder("Child");
            child.Delete();

            var retrievedStore = (new Documents(new Context(), new Mock<IBlobStore>().Object)).GetStore(Store.Id);
            Assert.AreEqual(0, retrievedStore.RootFolder.Folders.Count, "Parent Folder still contains deleted folder");
        }
    }
}
