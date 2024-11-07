using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Auth
{
    public class RefreshTokenDTO
    {
        public required string RefreshToken { get; set; }
    }
}
