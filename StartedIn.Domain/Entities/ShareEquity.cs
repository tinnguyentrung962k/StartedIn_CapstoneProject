using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class ShareEquity : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Contract))]
        public string ContractId { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; }

        [Column(TypeName = "decimal(14,3)")]
        public decimal SharePrice { get; set; }

        public int? ShareQuantity { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Percentage { get; set; }
        public RoleInTeam StakeHolderType { get; set; }
        public DateOnly? DateAssigned { get; set; }
        public Contract Contract { get; set; }
        public User User { get; set; }
    }
}
