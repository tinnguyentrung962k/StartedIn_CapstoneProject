﻿using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Domain.Entities
{
    public class UserContract
    {
        public string ContractId { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset? SignedDate { get; set; }
        public bool IsReject { get; set; }
        public string? TransferToId { get; set; }
        public RoleInContract Role { get; set; }
        public virtual Contract Contract { get; set; }
        public virtual User User { get; set; }
    }
}
