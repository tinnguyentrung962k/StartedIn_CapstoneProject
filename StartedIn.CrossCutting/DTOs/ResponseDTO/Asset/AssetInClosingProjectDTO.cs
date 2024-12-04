using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Asset
{
    public class AssetInClosingProjectDTO : IdentityResponseDTO
    {
        public string AssetName { get; set; }
        public int RemainQuantity { get; set; }
    }
}
