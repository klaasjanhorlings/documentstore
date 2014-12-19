using Documents.Entities;
using Documents.Storage;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;

namespace Documents
{
    public sealed class Folder
    {
        private FolderEntity Entity;
        private IDocumentsContext Context;
        private IBlobStore BlobStore;
        private List<Folder> _Folders;
        private List<Document> _Documents;

        public int Id { get { return Entity.Id; } }
        public string Name { get { return Entity.Name; } }
        public Folder Parent { get; private set; }

        internal DocumentStore DocumentStore { get; private set; }

        public IReadOnlyList<Folder> Folders { get { return _Folders.AsReadOnly(); } }
        public IReadOnlyList<Document> Documents { get { return _Documents.AsReadOnly(); } }

        internal Folder(FolderEntity entity, DocumentStore documentStore, IDocumentsContext context, IBlobStore blobStore)
        {
            Entity = entity;
            Context = context;
            BlobStore = blobStore;
            DocumentStore = documentStore;

            InitializeChildren();
        }

        private void InitializeChildren()
        {
            _Folders = new List<Folder>();
            _Documents = new List<Document>();

            if (Entity.Folders != null)
            {
                _Folders.AddRange(Entity.Folders.Select(folder => new Folder(this, folder, DocumentStore, Context, BlobStore)).ToList());
            }

            if (Entity.Documents != null)
            {
                _Documents.AddRange(Entity.Documents.Select(document => new Document(document, DocumentStore, Context, BlobStore)).ToList());
            }
        }

        private Folder(Folder parent, FolderEntity entity, DocumentStore documentStore, IDocumentsContext context, IBlobStore blobStore)
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

        public void Move(Folder target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            if (Parent == null)
                throw new InvalidOperationException("Cannot move root folder");

            if (Entity.DocumentStoreId != target.Entity.DocumentStoreId)
                throw new InvalidOperationException("Cannot move to another DocumentStore");

            if (IsDescendent(target))
                throw new InvalidOperationException("Cannot move to a descendent");

            Parent._Folders.Remove(this);
            Parent = target;
            Parent._Folders.Add(this);

            Entity.ParentFolder = target.Entity;
            ((DbContext)Context).SaveChanges();
        }

        internal void Move(Document document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            if (document.Parent != null)
                document.Parent._Documents.Remove(document);

            _Documents.Add(document);
        }

        private bool IsDescendent(Folder folder)
        {
            if (Folders.Contains(folder))
                return true;

            return Folders.Any(f => f.IsDescendent(folder));
        }

        public void Delete()
        {
            if (Parent == null)
                throw new InvalidOperationException("Cannot delete root folder. Delete DocumentStore instead");

            // Delete all child folders
            foreach (var childFolder in Folders)
            {
                childFolder.Delete();
            }

            // Update graph
            Parent._Folders.Remove(this);

            // And persist self
            ((DbContext)Context).Entry<FolderEntity>(Entity).State = EntityState.Deleted;
            ((DbContext)Context).SaveChanges();
        }

        internal void DeleteRoot() 
        {
            foreach (var childFolder in Folders)
            {
                childFolder.Delete();
            }
            
            ((DbContext)Context).Entry<FolderEntity>(Entity).State = EntityState.Deleted;
            ((DbContext)Context).SaveChanges();
        }

        internal void Delete(Document document)
        {
            this._Documents.Remove(document);
        }

        public Folder CreateFolder(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            var folderEntity = CreateFolderEntity(name);
            var folder = new Folder(this, folderEntity, DocumentStore, Context, BlobStore);

            _Folders.Add(folder);

            return folder;
        }

        private FolderEntity CreateFolderEntity(string name)
        {
            var folderEntity = new FolderEntity
            {
                Name = name,
                ParentFolder = Entity,
                DocumentStore = Entity.DocumentStore,
            };
            Context.Folders.Add(folderEntity);
            ((DbContext)Context).SaveChanges();
            return folderEntity;
        }

        public Document CreateDocument(string name, Stream stream)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            if (stream == null)
                throw new ArgumentNullException("stream");

            var hash = BlobStore.Store(stream);

            var documentEntity = CreateDocumentEntity(name, hash);
            var document = new Document(this, documentEntity, DocumentStore, Context, BlobStore);

            _Documents.Add(document);

            return document;
        }

        private DocumentEntity CreateDocumentEntity(string name, byte[] hash)
        {
            var documentEntity = new DocumentEntity
            {
                Name = name,
                DocumentStore = Entity.DocumentStore,
                ParentFolder = Entity,
                BlobReference = Convert.ToBase64String(hash)
            };
            Context.Documents.Add(documentEntity);
            ((DbContext)Context).SaveChanges();
            return documentEntity;
        }
    }
}
