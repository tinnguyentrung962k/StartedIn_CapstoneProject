using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class DealOfferHistory : BaseAuditEntity<string>
    {
        [ForeignKey(nameof(DealOffer))]
        public string DealOfferId { get; set; }
        public string Content { get; set; } 
        public DealOffer DealOffer { get; set; } 
    }
}
