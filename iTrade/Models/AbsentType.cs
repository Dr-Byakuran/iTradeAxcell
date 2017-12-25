using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class AbsentType
    {
        [Key]
        public int ID { get; set; }
        public string AbsentCode { get; set; }
        public string AbsentName { get; set; }
    }
}