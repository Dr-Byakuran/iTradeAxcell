using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class KivOrderDet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Detail ID")]
        public int DetID { get; set; }

        [Display(Name = "Type")]
        [StringLength(15)]
        public string DetType { get; set; }

        [Display(Name = "KIV Order#")]
        public int KorID { get; set; }

        [Display(Name = "KivExchange ID")]
        public int KivEorID { get; set; }

        [Display(Name = "KIV No")]
        public int KivID { get; set; }

        [Display(Name = "Invoice No")]
        public int InvID { get; set; }

        [Display(Name = "Invoice#")]
        [StringLength(25)]
        public string InvNo { get; set; }

        [Display(Name = "InvDet ID")]
        public int InvDetID { get; set; }

        [Display(Name = "Product No")]
        public int ItemID { get; set; }

        [Display(Name = "Product Code")]
        [StringLength(60)]
        public string ItemCode { get; set; }

        [Display(Name = "Product Name")]
        [StringLength(200)]
        public string ItemName { get; set; }

        [Display(Name = "Sell Type")]
        [StringLength(20)]
        public string SellType { get; set; }

        [StringLength(15)]
        public string Unit { get; set; }

        [Display(Name = "Deliver Order Qty")]
        public double DeliverOrderQty { get; set; }

        [Display(Name = "Deliver Qty")]
        public double DeliverQty { get; set; }

        [Display(Name = "Qty Changed")]
        public double ChangedQty { get; set; }

        //[Display(Name = "Completed Qty")]
        //public double CompletedQty { get; set; }
        [Display(Name = "Balance Qty")]
        public double BalanceQty { get; set; }

        [Display(Name = "Sales Type")]
        [StringLength(30)]
        public string SalesType { get; set; }

        [Display(Name = "Ref No")]
        public int RefItemID { get; set; }

        public double Position { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

    }
}