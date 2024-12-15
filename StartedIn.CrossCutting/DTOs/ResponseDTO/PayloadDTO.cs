using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class PayloadDTO<K>
    {
        public K Data { get; set; }
        public string Action { get; set; }
    }
}
