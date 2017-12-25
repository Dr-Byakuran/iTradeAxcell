using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class CompanySelection
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string ChineseName { get; set; }
        public string Address { get; set; }
        public string TelephoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string EmailAddress { get; set; }
        public string BusinessRegNo { get; set; }
        public string GSTRegNo { get; set; }
        public byte[] LogoImage { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedOn { get; set; }
        public Boolean IsDefault { get; set; }
    }
}