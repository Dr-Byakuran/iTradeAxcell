using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class LoginUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string SalesMember { get; set; }
    }
}