using Documents.Entities;
using Documents.Storage;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Documents
{
    public sealed class DocumentStore
    {
        private IDocumentsContext Context;
        private IBlobStore BlobStore;
        private DocumentStoreEntity Entity;
        private Folder Root;

        public int Id { get { return Entity.Id; } }
        public Folder RootFolder { get { return Root = Root ?? GetRootFolder(); } }

        internal DocumentStore(DocumentStoreEntity entity, IDocumentsContext context, IBlobStore blobStore)
        {
            Entity = entity;
            Context = context;
            BlobStore = blobStore;
        }

        public void Delete()
        {
            RootFolder.DeleteRoot();
            Context.DocumentStores.Remove(Entity);
            ((DbContext)Context).SaveChanges();
        }

        private Folder GetRootFolder()
        {
            var root = Context.Folders
                .Where(f => f.DocumentStoreId == Id)
                .Include(f => f.Documents)
                .ToList()
                .Find(f => f.ParentFolderId == null);

            if (root == null)
                return null;
            
            return new Folder(root, this, Context, BlobStore);
        }
    }
}
