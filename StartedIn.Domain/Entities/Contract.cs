﻿using Microsoft.EntityFrameworkCore;
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

        [ForeignKey(nameof(DealOffer))]
        public string? DealOfferId { get; set; }

        public string? TerminationInitiatorId { get; set; }

        public string? TerminationReason { get; set; }

        public DateOnly? TerminationDate { get; set; }

        [MaxLength(50)]
        public string ContractName { get; set; }
        public ContractTypeEnum ContractType { get; set; }
        public string? SignNowDocumentId { get; set; }
        public ContractStatusEnum ContractStatus { get; set; }
        
        [MaxLength(4500)]
        public string ContractPolicy { get; set; }
        public string ContractIdNumber { get; set; }
        public DateTimeOffset? SignDeadline { get; set; }
        public DateOnly? ValidDate { get; set; }
        public DateOnly? ExpiredDate { get; set; }
        public string? AzureLink { get; set; }
        public Project Project { get; set; }
        public ICollection<UserContract> UserContracts { get; set; }
        public ICollection<ShareEquity>? ShareEquities { get; set; }
        public ICollection<Disbursement>? Disbursements { get; set; }
        public DealOffer? DealOffer { get; set; }

    }
}
