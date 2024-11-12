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
    public class Document : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Project))]
        public string ProjectId { get; set; }
        [MaxLength(50)]
        public string DocumentName { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public string AttachmentLink { get; set; }
        public Project Project { get; set; }
    }
}
