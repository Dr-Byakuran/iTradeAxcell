using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.ComponentModel;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class InvoiceIndexViewModel
    {
        public IList<INV> InvoicesList { get; set; }
        public SelectList Customer { get; private set; }

        public InvoiceIndexViewModel(IList<INV> invoicesList, IEnumerable customer)
        {
            InvoicesList = invoicesList;
            Customer = new SelectList(customer, "CustNo", "CustName");
        }
    }

    public class InvoiceViewModel
    {
        public virtual INV Invoice { get; set; }
        public virtual Client Client { get; set; }
        public IList<INVDET> INVDETs { get; set; }
        public IList<KIVDET> KIVDETs { get; set; }
    }

    public class SalesOrderViewModel
    {
        public virtual SalesOrder SalesOrder { get; set; }
        public virtual Client Client { get; set; }
        public IList<INVDET> INVDETs { get; set; }
        public IList<KIVDET> KIVDETs { get; set; }
    }

    public class DeliveryViewModel
    {
        public virtual INV Invoice { get; set; }
        public virtual SalesOrder SalesOrder { get; set; }
        public virtual Client Client { get; set; }
        public IList<INVDET> INVDETs { get; set; }
        public IList<KIVDET> KIVDETs { get; set; }
    }

    public class StatementAccountViewModel
    {
        public virtual Client Client { get; set; }
        public List<StatementAccount> StatementAccount { get; set; }
    }

    public class StatementAccount
    {
        public int CustNo { get; set; }
        public int InvID { get; set; }
        public int SalesPaymentMethodID { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime InvDate { get; set; }

        public StatementAccount(int custNo, int InvID, int salesPaymentMethodID, string paymentMethod, decimal amount, DateTime createdOn, DateTime invDate)
        {
            CustNo = custNo;
            InvID = InvID;            
            SalesPaymentMethodID = salesPaymentMethodID;
            PaymentMethod = paymentMethod;
            Amount = amount;
            CreatedOn = createdOn;
            InvDate = invDate;
        }
    }
}