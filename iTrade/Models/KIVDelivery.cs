using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class KIVDeliveryIndexViewModel
    {
        public int KIVDelID { get; set; }

        [Display(Name = "Company")]
        public string CustName { get; set; }

        [Display(Name = "Invoice No")]
        public IEnumerable<KIVDeliveryDetail> InvIDs { get; set; }

        [Display(Name = "Delivery Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> DeliveryDate { get; set; }

        [Display(Name = "Time")]
        [StringLength(60)]
        public string DeliveryTime { get; set; }

        public KIVDeliveryIndexViewModel(int kivDelID, string custName, IEnumerable<KIVDeliveryDetail> InvIDs, Nullable<DateTime> deliveryDate, string deliveryTime)
        {
            KIVDelID = kivDelID;
            CustName = custName;
            InvIDs = InvIDs;
            DeliveryDate = deliveryDate;
            DeliveryTime = deliveryTime;
        }
    }

    public class KIVDeliveryAll
    {
        public int DetID { get; set; }

         [Display(Name = "Invoice No")]
        public int InvID { get; set; }

        [Display(Name = "Company")]
        public int CustNo { get; set; }

        [Display(Name = "Product No")]
        public int ItemID { get; set; }
        
        [Display(Name = "Product Name")]
        [StringLength(100)]
        public string ItemName { get; set; }

        [Display(Name = "Order Qty")]
        public double OrderQty { get; set; }

        [Display(Name = "Balance Qty")]
        public double BalanceQty { get; set; }

        [Display(Name = "Deliver Qty")]
        public double DeliveryQty { get; set; }

        [Display(Name = "KIV Balance")]
        public double KivBalanceQty { get; set; }

        [Display(Name = "Invoiced Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime InvDate { get; set; }
        public DateTime CreatedOn { get; set; }

        public KIVDeliveryAll(int detID, int InvID, int custNo, int itemID, string itemName, double orderQty, double balanceQty, double deliveryQty, double kivBalanceQty, DateTime invDate, DateTime createdOn)
        {
            DetID = detID;
            InvID = InvID;
            CustNo = custNo;
            ItemID = itemID;
            ItemName = itemName;
            OrderQty = orderQty;
            BalanceQty = balanceQty;
            DeliveryQty = deliveryQty;
            KivBalanceQty = kivBalanceQty;
            InvDate = invDate;
            CreatedOn = createdOn;
        }
    }

    public class KIVDeliveryCreateViewModel
    {
        public virtual KIVDelivery KIVDelivery { get; set; }
        public virtual INV Invoice { get; set; }
        public IList<KIVDeliveryAll> KIVDETs { get; set; }
    }

    public class KIVDelivery
    {
        public KIVDelivery()
        {
            this.Clients = new HashSet<Client>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Delivery ID")]
        public int KIVDelID { get; set; }

        [Display(Name = "Company")]
        public int CustNo { get; set; }

        [Display(Name = "Created By")]
        [StringLength(30)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Delivery Address")]
        [StringLength(100)]
        public string DeliveryAddress { get; set; }

        [Display(Name = "Delivery Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> DeliveryDate { get; set; }

        [Display(Name = "Time")]
        [StringLength(60)]
        public string DeliveryTime { get; set; }

        public virtual ICollection<Client> Clients { get; set; }
        internal void Client(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Clients.Add(new Client());
            }
        }
    }

    public class KIVDeliveryDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KIVDelDetailsID { get; set; }

        public int KIVDelID { get; set; }
        public int DetID { get; set; }
        public int InvID { get; set; }
        public double DeliveryQty { get; set; }
        public double BalanceQty { get; set; }
        public double KivBalanceQty { get; set; }
    }

    public class KIVDeliveryDetailForPrint
    {
        public int KIVDelDetailsID { get; set; }
        public int KIVDelID { get; set; }
        public int DetID { get; set; }
        public int InvID { get; set; }
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public double TotalDelivery { get; set; }
        public double DeliveryQty { get; set; }
        public double BalanceQty { get; set; }
        public double OrderQty { get; set; }
        public double KivBalanceQty { get; set; }

        public KIVDeliveryDetailForPrint(int kivDelDetailsID, int kivDelID, int detID, int InvID, int itemID, string itemName, double totalDelivery, double deliveryQty, double balanceQty, double orderQty, double kivBalanceQty)
        {
            KIVDelDetailsID = kivDelDetailsID;
            KIVDelID = kivDelID;
            DetID = detID;
            InvID = InvID;
            ItemID = itemID;
            ItemName = itemName;
            TotalDelivery = totalDelivery;
            DeliveryQty = deliveryQty;
            BalanceQty = balanceQty;
            OrderQty = orderQty;
            KivBalanceQty = kivBalanceQty;
        }
    }

    public class KIVDetailsForm
    {
        public string DetID { get; set; }
        public string KIVQty { get; set; }
    }

    public class KIVDeliveryPrintPreview
    {
        public virtual Client Client { get; set; }
        public virtual KIVDelivery KIVDelivery { get; set; }
        public IList<KIVDeliveryDetailForPrint> KIVDeliveryDetail { get; set; }
    }

    public class TotalKIVCount
    {
        public int ItemID { get; set; }
        public double DelQty { get; set; }
        public double KIVQty { get; set; }
    }

    public class KIVInvoice
    {
        public int ItemID { get; set; }
        public string InvID { get; set; }
        public double KIVQty { get; set; }
    }
}