using Documents.Entities;
using Documents.Storage;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Documents
{
    public class Document
    {
        private DocumentEntity Entity;
        private IDocumentsContext Context;
        private IBlobStore BlobStore;

        public int Id { get { return Entity.Id; } }
        public string Name { get { return Entity.Name; } }
        public Folder Parent { get; private set; }

        internal DocumentStore DocumentStore { get; private set; }

        internal Document(DocumentEntity entity, DocumentStore documentStore, IDocumentsContext context, IBlobStore blobStore)
        {
            Entity = entity;
            Context = context;
            BlobStore = blobStore;
            DocumentStore = documentStore;
        }

        internal Document(Folder parent, DocumentEntity entity, DocumentStore documentStore, IDocumentsContext context, IBlobStore blobStore)
            : this(entity, documentStore, context, blobStore)
        {
            Parent = parent;
        }

        public void Rename(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            Entity.Name = name;
            ((DbContext)Context).SaveChanges();
        }

        public void Move(Folder folder) 
        {
            if (folder == null)
                throw new ArgumentNullException("target");

            if (DocumentStore.Id != folder.DocumentStore.Id)
                throw new InvalidOperationException("Cannot move to another DocumentStore");

            Entity.ParentFolderId = folder.Id;
            ((DbContext)Context).SaveChanges();

            folder.Move(this);
            Parent = folder;
        }

        public void Delete() 
        {
            var referenceCount = Context.Documents.Count(d => d.BlobReference == Entity.BlobReference);

            if (referenceCount == 1)
            {
                BlobStore.Delete(Convert.FromBase64String(Entity.BlobReference));
            }

            Context.Documents.Remove(Entity);
            ((DbContext)Context).SaveChanges();

            if (Parent != null)
            {
                Parent.Delete(this);
            }
        }
    }
}
