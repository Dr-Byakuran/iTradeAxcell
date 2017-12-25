using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iTrade.Models
{
    public class UserCompanySelection
    {
        public int ID { get; set; }
        public string UsersAdminID { get; set; }
        public int CompanyID { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedOn { get; set; }
        public Boolean IsDefault { get; set; }

        public Company CompanyData { get; set; }
        public string CompanyName { get; set; }
        public ApplicationUser UserAdminData { get; set; }
        public string DisplayName { get; set; }

        public List<Company> CompanyList { get; set; }
        public List<SelectListItem> UserList { get; set; }
    }
}