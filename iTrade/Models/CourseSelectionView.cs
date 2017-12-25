using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class CourseSelectionView
    {
        public List<CourseSelectEditor> DataSelects { get; set;}

        public CourseSelectionView() 
        {
            this.DataSelects = new List<CourseSelectEditor>();
        }

        public IEnumerable<int> getSelectedIds() 
        {
            // Return an Enumerable containing the Id's of the selected job:       
            return (from p in this.DataSelects where p.Selected select p.CourseID).ToList();
        }
    }
}