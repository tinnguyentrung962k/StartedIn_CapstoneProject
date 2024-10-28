using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class ContractCreateThreeModelsDTO
    {
        public ContractCreateDTO contractCreateDTO { get; set; }
        public List<EquityShareCreateDTO>? equityShareCreateDTOs { get; set; }
        public List<DisbursementCreateDTO>? disbursementCreateDTOs { get; set; } 
    }
}
