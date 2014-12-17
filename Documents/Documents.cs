using Documents.Entities;
using Documents.Storage;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Documents
{
    public class Documents : IDisposable
    {
        private IDocumentsContext Context;
        private IBlobStore BlobStore;
        
        public Documents(IDocumentsContext context, IBlobStore blobStore)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (blobStore == null)
                throw new ArgumentNullException("blobStore");

            Context = context;
            BlobStore = blobStore;
        }

        public DocumentStore CreateStore()
        {
            var store = new DocumentStoreEntity();
            store.Folders = new List<FolderEntity>();
            store.Folders.Add(new FolderEntity
            {
                Name = "RootFolder",
                DocumentStore = store
            });

            Context.DocumentStores.Add(store);
            ((DbContext)Context).SaveChanges();

            return new DocumentStore(store, Context, BlobStore);
        }

        public DocumentStore GetStore(int id)
        {
            var store = Context.DocumentStores.Find(id);
            if (store == null)
                return null;

            return new DocumentStore(store, Context, BlobStore);
        }

        public void Dispose()
        {
            Context = null;
        }
    }
}
