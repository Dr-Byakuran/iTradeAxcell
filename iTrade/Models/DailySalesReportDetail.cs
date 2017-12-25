using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class DailySalesReportDetail
    {
        public DateTime RDate { get; set; }
        public string CompanyName { get; set; }
        public string InvoiceNo { get; set; }
        public string ChequeNo { get; set; }
        public string TelephoneNo { get; set; }
        public decimal Amount { get; set; } 
    }
}