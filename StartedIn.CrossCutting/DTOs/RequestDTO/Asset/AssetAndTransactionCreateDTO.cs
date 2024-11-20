using StartedIn.CrossCutting.DTOs.RequestDTO.Transaction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Asset
{
    public class AssetAndTransactionCreateDTO
    {
        [Required]
        public AssetCreateDTO Asset { get; set; }
        public TransactionInAssetCreateDTO? Transaction { get; set; }
    }
}
