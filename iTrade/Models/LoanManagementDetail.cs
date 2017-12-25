using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class LoanManagementDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID")]
        public int LoanManagementDetailID { get; set; }

        [Display(Name = "Returned Item ID")]
        public int LoanManagementID { get; set; }

        [Display(Name = "Product No")]
        public int ItemID { get; set; }

        [Display(Name = "Product Code")]
        [StringLength(60)]
        public string ItemCode { get; set; }

        [Display(Name = "Product Type")]
        [StringLength(30)]
        public string ItemType { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        [StringLength(100)]
        public string ItemName { get; set; }

        [Display(Name = "Sell Type")]
        [StringLength(20)]
        public string SellType { get; set; }

        public double Qty { get; set; }

        [StringLength(15)]
        public string Unit { get; set; }

        public Boolean IsBundle { get; set; }

        [Display(Name = "Sales Type")]
        [StringLength(30)]
        public string SalesType { get; set; }

        [Display(Name = "Ref No")]
        public int RefItemID { get; set; }

        public Boolean IsControlItem { get; set; }

        public double Position { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<DateTime> StartDate { get; set; }

        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<DateTime> EndDate { get; set; }
    }
}