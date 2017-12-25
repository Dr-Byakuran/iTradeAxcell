using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class EmployeeView
    {
        public string UsersAdminID { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public IEnumerable<CompanySelection> CompanyList { get; set; }
    }
}