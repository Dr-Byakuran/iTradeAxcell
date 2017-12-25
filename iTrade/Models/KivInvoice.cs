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

    public class KivInvoiceIndexViewModel
    {
        public IList<KivOrder> InvoicesList { get; set; }
        public SelectList Customer { get; private set; }

        public KivInvoiceIndexViewModel(IList<KivOrder> invoicesList, IEnumerable customer)
        {
            InvoicesList = invoicesList;
            Customer = new SelectList(customer, "CustNo", "CustName");
        }
    }

    public class KivInvoiceViewModel
    {
        public virtual KivOrder Invoice { get; set; }
        public virtual Client Client { get; set; }
        public IList<KivOrderDet> INVDETs { get; set; }

    }
}