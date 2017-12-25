using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class InvDetView
    {
        public INVDET InvDet { get; set; }

        public int InvID { get; set; }
        public DateTime InvDate { get; set; }
        public int CustNo { get; set; }
        public string CustName { get; set; }
        public string PersonName { get; set; }
 

    }
}