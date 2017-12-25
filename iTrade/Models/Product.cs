using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class Product
    {
         public Product()
        {
            this.PriceOptions = new HashSet<PriceOption>();
            this.Pricebreaks = new HashSet<Pricebreak>();
            this.Productbundles = new HashSet<Productbundle>();
            this.ProductFOCs = new HashSet<ProductFOC>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "No")]
        public int ProductID { get; set; }

        [Display(Name = "Product Code")]
        [StringLength(60)]
        public string ProductCode { get; set; }

        [Display(Name = "Product Type")]
        [StringLength(45)]
        public string ProductType { get; set; }

        [Required]
        [Display(Name = "Product Name *")]
        [StringLength(200, ErrorMessage = "Product name can not be empty.", MinimumLength = 1)]
        public string ProductName { get; set; }

        [Display(Name = "Brand")]
        [StringLength(45)]
        public string Brand { get; set; }

        [Display(Name = "Model No")]
        [StringLength(45)]
        public string ModelNo { get; set; }

        [Display(Name = "Part Number")]
        [StringLength(45)]
        public string PartNo { get; set; }

        [Display(Name = "Product Description")]
        [StringLength(200)]
        [DataType(DataType.MultilineText)]
        public string ProductDesc { get; set; }

        [Display(Name = "Category(s)")]
        [StringLength(60)]
        public string ProductCats { get; set; }

        [Required]
        [Display(Name = "Sales SKU *")]
        [StringLength(60, ErrorMessage = "Sales SKU can not be empty.", MinimumLength = 1)]
        public string SKU { get; set; }

        [Display(Name = "SKU Purchase")]
        [StringLength(60)]
        public string SKUPurchase { get; set; }

        [Display(Name = "Barcode")]
        [StringLength(60)]
        public string Barcode { get; set; }

        [Display(Name = "Initial Stock Qty")]
        public double Qty { get; set; }

        [Display(Name = "Unit Of Measurement")]
        [StringLength(20)]
        public string Unit { get; set; }

        [Display(Name = "Base Currency")]
        [StringLength(15)]
        public string BaseCurrency { get; set; }

        [Display(Name = "Unit Cost Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal CostPrice { get; set; }

        [Display(Name = "Cost Code")]
        [StringLength(30)]
        public string CostCode { get; set; }

        [Display(Name = "List Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal RetailPrice { get; set; }

        [Display(Name = "Showroom Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal WholesalePrice { get; set; }

        [Display(Name = "Special Nett")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal DealerPrice { get; set; }

        [Display(Name = "Use Price Break?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean UsePricebreak { get; set; }

        [Display(Name = "Manage Inverntory?")]
        public Boolean ManageStock { get; set; }

        [Display(Name = "Min Stock Level")]
        public double MinStockQty { get; set; }

        [Display(Name = "Max Stock Level")]
        public double MaxStockQty { get; set; }

        [Display(Name = "Reorder Qty")]
        public double ReorderQty { get; set; }

        [Display(Name = "Is Bundle?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsBundle { get; set; }

        [Display(Name = "Is Active?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsActive { get; set; }

        [Display(Name = "Control Item?")]
        public Boolean IsControlItem { get; set; }

        [Display(Name = "Featured?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsFeatured { get; set; }

        public int SupplierID { get; set; }

        [Display(Name = "Supplier")]
        [StringLength(80)]
        public string SupplierName { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(100)]
        public string Remark { get; set; }

        [StringLength(150)]
        public string ImageUrl { get; set; }

        [StringLength(150)]
        public string ImageUrl2 { get; set; }

        [StringLength(80)]
        public string Dimension { get; set; }

        [StringLength(80)]
        public string Finishes { get; set; }

        [StringLength(80)]
        public string Lamp { get; set; }

        [StringLength(80)]
        public string Gear { get; set; }

        [StringLength(80)]
        public string Driver { get; set; }

        [StringLength(80)]
        public string Reference { get; set; }

        [Display(Name = "Country Code")]
        [StringLength(60)]
        public string CountryCode { get; set; }

        [Display(Name = "Created By")]
        [StringLength(60)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Last Modified")]
        [StringLength(60)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modibied On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ModifiedOn { get; set; }

        public virtual ICollection<PriceOption> PriceOptions { get; set; }
        public virtual ICollection<Pricebreak> Pricebreaks { get; set; }
        public virtual ICollection<Productbundle> Productbundles { get; set; }
        public virtual ICollection<ProductFOC> ProductFOCs { get; set; }

        internal void CreatePriceOptions(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                var po = new PriceOption();
                if (this.PriceOptions.Count == 0)
                {
                    po.IsDefault = true;
                }
                else
                {
                    po.IsDefault = false;
                }

                PriceOptions.Add(po);
            }
        }

        internal void CreatePricebreaks(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Pricebreaks.Add(new Pricebreak());
            }
        }

        internal void CreateProductbundles(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Productbundles.Add(new Productbundle());
            }
        }

        internal void CreateProductFOCs(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                ProductFOCs.Add(new ProductFOC());
            }
        }


    }

    public class PriceOption
    {
        [Key]
        public int OptionID { get; set; }

        [Display(Name = "Default Price?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsDefault { get; set; }

        [Display(Name = "Unit Of Measurement")]
        [StringLength(20)]
        public string Unit { get; set; }

        [Display(Name = "Unit Cost Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal CostPrice { get; set; }

        [Display(Name = "Cost Code")]
        [StringLength(20)]
        public string CostCode { get; set; }

        [Display(Name = "Unit Factor")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public double? UnitFactor { get; set; }

        [Display(Name = "List Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal RetailPrice { get; set; }

        [Display(Name = "Showroom Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal WholesalePrice { get; set; }

        [Display(Name = "Special Nett")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal DealerPrice { get; set; }

        public Boolean DeleteItem { get; set; }

    }

    public class Pricebreak
    {
        [Key]
        public int PriceID { get; set; }

        [Display(Name = "Break Qty")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public double BreakQty { get; set; }

        [Display(Name = "List Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal RetailPrice { get; set; }

        [Display(Name = "Showroom Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal WholesalePrice { get; set; }

        [Display(Name = "Special Nett")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal DealerPrice { get; set; }

        [Display(Name = "FOC Qty")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public double? FocQty { get; set; }

        public Boolean DeleteItem { get; set; }

    }

    public class Productbundle
    {
        [Key]
        public int BunleID { get; set; }

        [Display(Name = "Product ID")]
        public int IncProductID { get; set; }

        [Display(Name = "Product Type")]
        [StringLength(30)]
        public string IncProductType { get; set; }

        [Display(Name = "Product Name")]
        [StringLength(100)]
        public string IncProductName { get; set; }

        [Display(Name = "Sales SKU")]
        [StringLength(60)]
        public string IncSKU { get; set; }

        [Display(Name = "Model No")]
        [StringLength(30)]
        public string IncModelNo { get; set; }

        [Display(Name = "Qty Per Set")]
        public double IncQty { get; set; }

        public Boolean IsControlItem { get; set; }

        public double Position { get; set; }

        [Display(Name = "Remark")]
        [StringLength(30)]
        public string IncRemark { get; set; }

        public Boolean DeleteBundle { get; set; }

    }

    public class ProductFOC
    {
        [Key]
        public int FocID { get; set; }
         
        [Display(Name = "Product ID")]
        public int IncProductID { get; set; }

        [Display(Name = "Product Type")]
        [StringLength(30)]
        public string IncProductType { get; set; }

        [Display(Name = "Product Name")]
        [StringLength(100)]
        public string IncProductName { get; set; }

        [Display(Name = "Sales SKU")]
        [StringLength(60)]
        public string IncSKU { get; set; }

        [Display(Name = "Model No")]
        [StringLength(30)]
        public string IncModelNo { get; set; }

        [Display(Name = "Qty Per Set")]
        public double IncQty { get; set; }

        public Boolean IsControlItem { get; set; }

        public double Position { get; set; }

        [Display(Name = "Remark")]
        [StringLength(30)]
        public string IncRemark { get; set; }

        public Boolean DeleteFOC { get; set; }

    }


}