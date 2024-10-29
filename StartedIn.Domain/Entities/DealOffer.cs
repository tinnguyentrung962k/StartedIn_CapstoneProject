﻿using StartedIn.Domain.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class DealOffer : BaseEntity<string>
    {
        [ForeignKey(nameof(Project))]
        public string ProjectId { get; set; }
        
        [ForeignKey(nameof(Investor))]
        public string InvestorId { get; set; }

        [Column(TypeName = "decimal(14,3)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal EquityShareOffer { get; set; }

        [MaxLength(500)]
        public string? TermCondition { get; set; }
        
        [MaxLength(50)]
        public string DealStatus { get; set; }
        public Project Project { get; set; }
        public User Investor { get; set; }
    }
}
