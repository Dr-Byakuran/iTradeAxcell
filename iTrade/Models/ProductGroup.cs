using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class ProductGroup
    {
        [Key]
        public int ID { get; set; }
        public string ProductType { get; set; }

    }
}