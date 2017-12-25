using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class INVDETSelection
    {
        public int InvNO { get; set; }
        public DateTime InvDate { get; set; }
        public string CustName { get; set; }
        public double Qty { get; set; }
        public decimal Nett { get; set; }
    }
}