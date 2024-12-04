using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Asset
{
    public class AssetUpdateDTO
    {
        public int RemainQuantity { get; set; }
        public AssetStatus Status { get; set; }
    }
}
