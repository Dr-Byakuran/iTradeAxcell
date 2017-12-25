using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class InvView
    {
        public INV InvOn { get; set; }
        public List<INVDET> InvDetOn { get; set; }

    }
}