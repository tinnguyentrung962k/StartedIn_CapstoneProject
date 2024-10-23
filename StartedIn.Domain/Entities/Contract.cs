using StartedIn.CrossCutting.Enum;
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
    public class Contract : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Project))]
        public string ProjectId { get; set; }

        [MaxLength(50)]
        public string ContractName { get; set; }

        public ContractType ContractType { get; set; }
        public string AttachmentLink { get; set; }
        
        [MaxLength(50)]
        public string ContractPolicy { get; set; }

        public DateOnly? ValidDate { get; set; }
        public DateOnly? ExpiredDate { get; set; }
        public Project Project { get; set; }
        public IEnumerable<UserContract> UserContracts { get; set; }

    }
}
