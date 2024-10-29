﻿using System;
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
        public DateOnly? SignedDate { get; set; }
        public virtual Contract Contract { get; set; }
        public virtual User User { get; set; }
    }
}