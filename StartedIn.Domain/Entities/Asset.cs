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
    public class Asset : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(Project))]
        public string ProjectId { get; set; }

        [MaxLength(100)]
        public string AssetName { get; set; }
        
        [Column(TypeName = "decimal(14,3)")]
        public decimal? Price { get; set; }

        public DateOnly? PurchaseDate { get; set; }
        public int? Quantity { get; set; }
        public AssetStatus? Status { get; set; }
        
        [MaxLength(50)]
        public string? SerialNumber { get; set; }
        public Transaction? Transaction { get; set; }
        public Project Project { get; set; }
    }
}
