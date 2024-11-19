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
    public class InvestmentCall : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Project))]
        public string ProjectId { get; set; }

        [Column(TypeName = "decimal(14,3)")]
        public decimal TargetCall { get; set; }

        [Column(TypeName = "decimal(14,3)")]
        public decimal AmountRaised { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal EquityShareCall { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal RemainAvailableEquityShare { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public InvestmentCallStatus Status { get; set; }
        public int TotalInvestor { get; set; }
        public Project Project { get; set; }
        public ICollection<DealOffer>? DealOffers { get; set; }
    }
}
