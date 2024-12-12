using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.Enum
{
    public enum ContractStatusEnum
    {
        DRAFT = 1,
        SENT = 2,
        COMPLETED = 3,
        CANCELLED = 4,
        EXPIRED = 5,
        WAITINGFORLIQUIDATION = 6,
    }
}
