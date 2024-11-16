using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.ShareEquity
{
    public class ShareEquitySummaryDTO
    {
        public string UserFullName { get; set; }
        public string UserId { get; set; }
        public decimal TotalPercentage { get; set; }
        public DateOnly LatestShareDate { get; set; }
    }
}
