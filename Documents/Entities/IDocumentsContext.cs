using System.Data.Entity;

namespace Documents.Entities
{
    public interface IDocumentsContext
    {
        DbSet<DocumentStoreEntity> DocumentStores { get; set; }
        DbSet<FolderEntity> Folders { get; set; }
        DbSet<DocumentEntity> Documents { get; set; }
    }
}
