using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class PurchaseOrderView
    {
        public PurchaseOrder SalesOrderOn { get; set; }
        public List<PoINVDET> InvDetOn { get; set; }

    }
}