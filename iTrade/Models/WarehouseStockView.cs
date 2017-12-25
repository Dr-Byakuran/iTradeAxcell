using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class WarehouseStockView
    {
        public WarehouseStock Stock { get; set; }
        public Warehouse Warehouse { get; set; }
        public Product Product { get; set; }
    }
}