using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Documents.Entities
{
    [Table("DocumentStore")]
    public class DocumentStoreEntity
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("DocumentStoreId")]
        public virtual List<FolderEntity> Folders { get; set; }
    }
}
