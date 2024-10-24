using SignNow.Net.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Customize
{
    public class InputContractField
    {
        public int PageNumber { get; set; }

        public FieldType Type { get; set; }

        public string Name { get; set; }
        public string Role { get; set; }
        public bool Required { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
