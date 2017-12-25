using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class ProductSelectionView
    {
        public List<ProductSelectEditor> productselects { get; set;}

        public ProductSelectionView() 
        {
            this.productselects = new List<ProductSelectEditor>();
        }

        public IEnumerable<int> getSelectedIds() 
        {
            // Return an Enumerable containing the Id's of the selected job:       
            return (from p in this.productselects where p.Selected select p.ProductID).ToList();
        }
    }
}