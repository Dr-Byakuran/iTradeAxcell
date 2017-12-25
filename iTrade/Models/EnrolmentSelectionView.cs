using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class EnrolmentSelectionView
    {
        public List<EnrolmentSelectEditor> DataSelects { get; set;}

        public EnrolmentSelectionView() 
        {
            this.DataSelects = new List<EnrolmentSelectEditor>();
        }

        public IEnumerable<int> getSelectedIds() 
        {
            // Return an Enumerable containing the Id's of the selected job:       
            return (from p in this.DataSelects where p.Selected select p.EnrID).ToList();
        }
    }
}