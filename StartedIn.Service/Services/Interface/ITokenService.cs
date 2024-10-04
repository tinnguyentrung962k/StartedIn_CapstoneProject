using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface ITokenService
    {
        public string CreateTokenForAccount(User user);

        public string GenerateRefreshToken();
    }
}
