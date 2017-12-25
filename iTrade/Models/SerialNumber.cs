using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iTrade.Models
{
    public class SerialNumber
    {
        public int Id { get; set; }        
        [Index(IsUnique = true)]
        public int TypeCode { get; set; }
        public string TypeName { get; set; }
        public string Prefix { get; set; }
        public int CurrNumber { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}