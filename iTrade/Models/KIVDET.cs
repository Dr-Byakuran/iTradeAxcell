using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class KIVDET
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Detail ID")]
        public int DetID { get; set; }

        [Display(Name = "Sales Order#")]
        public int SorID { get; set; }

        [Display(Name = "Invoice No#")]
        public int InvID { get; set; }

        [Display(Name = "KIV Order#")]
        public int KorID { get; set; }

        [Display(Name = "Kiv Exchanger#")]
        public int KivEorID { get; set; }

        [Display(Name = "KIV No")]
        public int KivID { get; set; }

        [Display(Name = "Type")]
        [StringLength(15)]
        public string InvType { get; set; }

        [Display(Name = "InvDet ID")]
        public int InvDetID { get; set; }

        [Display(Name = "Ref No")]
        public int RefItemID { get; set; }

        [Display(Name = "Product No")]
        public int ItemID { get; set; }

        [Display(Name = "Product Code")]
        [StringLength(60)]
        public string ItemCode { get; set; }

        [Display(Name = "Product Name")]
        [StringLength(200)]
        public string ItemName { get; set; }

        [StringLength(15)]
        public string Unit { get; set; }

        [Display(Name = "Order Qty")]
        public double OrderQty { get; set; }

        [Display(Name = "Balance Qty")]
        public double BalanceQty { get; set; }

        [Display(Name = "Deliver Qty")]
        public double DeliverQty { get; set; }

        [Display(Name = "Exchange Qty")]
        public double ExchangeQty { get; set; }

        [Display(Name = "KIV Balance")]
        public double KivBalanceQty { get; set; }

        [Display(Name = "Sales Type")]
        [StringLength(30)]
        public string SalesType { get; set; }

        public double Position { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

    }
}