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
        public string ContractType { get; set; }
        public string? SignNowDocumentId { get; set; }
        public string ContractStatus { get; set; }
        
        [MaxLength(4500)]
        public string ContractPolicy { get; set; }

        public DateOnly? ValidDate { get; set; }
        public DateOnly? ExpiredDate { get; set; }
        public Project Project { get; set; }
        public IEnumerable<UserContract> UserContracts { get; set; }
        public IEnumerable<ShareEquity>? ShareEquities { get; set; }
        public IEnumerable<Disbursement>? Disbursements { get; set; }

    }
}
