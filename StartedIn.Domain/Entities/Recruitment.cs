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
    public class Recruitment : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Project))]
        public string ProjectId { get; set; }
        [MaxLength(255)]
        public string Title { get; set; }
        [MaxLength(5000)]
        public string Content { get; set; }
        public bool IsOpen { get; set; }
        public Project Project { get; set; }
        public ICollection<RecruitmentImg> RecruitmentImgs { get; set; }
    }
}
