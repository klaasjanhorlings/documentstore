using Documents.Entities;
using System.Data.Entity;

namespace Documents.Test
{
    public class Context: DbContext, IDocumentsContext
    {
        public DbSet<DocumentStoreEntity> DocumentStores { get; set; }
        public DbSet<FolderEntity> Folders { get; set; }
        public DbSet<DocumentEntity> Documents { get; set; }
    }
}
