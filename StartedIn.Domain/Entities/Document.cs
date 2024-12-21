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
        [MaxLength(275)]
        public string DocumentName { get; set; }
        [MaxLength(5000)]
        public string? Description { get; set; }
        public string AttachmentLink { get; set; }
        [ForeignKey(nameof(ProjectApproval))]
        public string ProjectApprovalId { get; set; }
        public Project Project { get; set; }
        public ProjectApproval ProjectApproval { get; set; }
    }
}
