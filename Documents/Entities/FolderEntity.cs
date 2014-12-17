
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Documents.Entities
{
    [Table("Folders")]
    public class FolderEntity
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }

        [ForeignKey("ParentFolderId")]
        public virtual List<FolderEntity> Folders { get; set; }

        [ForeignKey("ParentFolderId")]
        public virtual List<DocumentEntity> Documents { get; set; }

        public int? ParentFolderId { get; set; }
        public virtual FolderEntity ParentFolder { get; set; }
        public int DocumentStoreId { get; set; }
        public virtual DocumentStoreEntity DocumentStore { get; set; }
    }
}
