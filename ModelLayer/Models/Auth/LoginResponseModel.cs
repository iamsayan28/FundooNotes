using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.Models.Auth
{
    public class LoginResponseModel
    {
        public string Token { get; set; } = "";
        public DateTime ExpiryTimeUtc { get; set; }
    }
}
