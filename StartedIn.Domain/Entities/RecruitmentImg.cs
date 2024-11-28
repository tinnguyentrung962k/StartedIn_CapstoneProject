using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class RecruitmentImg : BaseEntity<string>
    {
        [ForeignKey(nameof(Recruitment))]
        public string RecruitmentId { get; set; }
        public string FileName { get; set; }
        public string ImageUrl { get; set; }
        public Recruitment Recruitment { get; set; }
    }
}
