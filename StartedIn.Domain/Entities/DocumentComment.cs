using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class DocumentComment : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Document))]
        public string DocumentId { get; set; }
        [ForeignKey(nameof(CommentUser))]
        public string CommentUserId { get; set; }
        [MaxLength(500)]
        public string Content { get; set; }
        public User CommentUser { get; set; }
        public Document Document { get; set; }
    }
}
