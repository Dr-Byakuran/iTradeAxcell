using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class Purchase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "No")]
        public int PurID { get; set; }

        [Display(Name = "Date In")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> DateIn { get; set; }

        [Display(Name = "Batch No")]
        public string BatchNo { get; set; }

        [Display(Name = "No")]
        public int ProductID { get; set; }

        [Display(Name = "Product Code")]
        [StringLength(60)]
        public string ProductCode { get; set; }

        [Display(Name = "Product Type")]
        [StringLength(45)]
        public string ProductType { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        [StringLength(200, ErrorMessage = "Product name can not be empty.", MinimumLength = 1)]
        public string ProductName { get; set; }

        [Display(Name = "Material")]
        [StringLength(60)]
        public string Material { get; set; }

        [Display(Name = "Length")]
        public double Length { get; set; }

        [Display(Name = "Length Unit")]
        [StringLength(20)]
        public string LengthUnit { get; set; }

        [StringLength(20)]
        public string Thickness { get; set; }

        [StringLength(100)]
        public string Dimension { get; set; }

        public double Qty { get; set; }

        [StringLength(20)]
        public string Unit { get; set; }

        [Display(Name = "Buying Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal BuyingPriceORG { get; set; }

        [StringLength(15)]
        public string Currency { get; set; }

        [Display(Name = "Ex-Rate")]
        [DisplayFormat(DataFormatString = "{0:#,##0.0000#}", ApplyFormatInEditMode = true)]
        public decimal ExRate { get; set; }

        [Display(Name = "Buying Price (SGD)")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal BuyingPriceSGD { get; set; }

        public double Weight { get; set; }

        [StringLength(20)]
        public string UOM { get; set; }

        [StringLength(60)]
        public string Country { get; set; }

        [Display(Name = "Supplier NO")]
        public int SupplierID { get; set; }

        [Display(Name = "Supplier Name")]
        [StringLength(100)]
        public string SupplierName { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

        [Display(Name = "Created By")]
        [StringLength(60)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedOn { get; set; }

    }
}