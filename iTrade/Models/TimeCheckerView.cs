using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class TimeCheckerView
    {
        [Key]
        public int ID { get; set; }

        public List<TimeChecker> TimeCheckerList { get; set; }

        public TimeCheckerView() 
        {
            this.TimeCheckerList = new List<TimeChecker>();
        }

    }
}