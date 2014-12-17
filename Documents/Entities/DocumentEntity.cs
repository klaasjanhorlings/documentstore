using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Documents.Entities
{
    [Table("Documents")]
    public class DocumentEntity
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MinLength(1)]
        public string Name { get; set; }
        public string BlobReference { get; set; }

        public int ParentFolderId { get; set; }
        public virtual FolderEntity ParentFolder { get; set; }
        public int? DocumentStoreId { get; set; }
        public virtual DocumentStoreEntity DocumentStore { get; set; }
    }
}
