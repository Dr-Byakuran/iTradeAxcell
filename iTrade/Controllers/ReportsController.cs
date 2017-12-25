using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using iTrade.Models;

namespace iTrade.Controllers
{
    public class ReportsController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();

        // GET: Reports
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ReportByPeriod()
        {
            var p = new List<INV>();

            var d1 = DateTime.Now;

            p = db.INVs.Where(x => x.InvDate.Date == d1.Date && x.Status != "Void").OrderBy(x => x.InvID).ToList();

            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d1.ToShortDateString();

            return View(p);
        }

        [HttpGet]
        public ActionResult ReportByPeriod(string fromDate, string toDate)
        {
            DateTime d1 = DateTime.Today;
            DateTime d2 = DateTime.Today;

            if (!string.IsNullOrEmpty(fromDate))
            {
                d1 = Convert.ToDateTime(fromDate);
            };
            if (!string.IsNullOrEmpty(toDate))
            {
                d2 = Convert.ToDateTime(toDate);
            };
 
            var p = new List<INV>();           

            p = db.INVs.Where(x => (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvID).ToList();

            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d2.ToShortDateString();

            return View(p);
        }

        public ActionResult ReportByDailySales()
        {
            decimal dTotalDailyCash = 0;
            decimal dTotalCreditSales = 0;
            decimal dTotalSales = 0;
            decimal dReceivedByCash = 0;
            decimal dReceivedByNETS = 0;
            decimal dReceivedByCreditCard = 0;
            decimal dReceivedByCheque = 0;
            decimal dReceivedByCreditNote = 0;
            decimal dTotalReceived = 0;
            decimal dTotalDifference = 0;

         //   var p = new List<INV>();

            var dt = DateTime.Today;

        //    p = db.INVs.Where(x => x.InvDate.Day == dt.Day && x.InvDate.Month == dt.Month && x.InvDate.Year == dt.Year && x.Status != "Void").OrderBy(x => x.InvID).ToList();

            var dailysales = db.SalesPaymentMethods.Where(y => y.RecordedFrom != "CN" && y.CreatedOn.Day == dt.Day && y.CreatedOn.Month == dt.Month && y.CreatedOn.Year == dt.Year).ToList();

            foreach (var item in dailysales)
            {
                if (item.InvType == "CS")
                    dTotalDailyCash += item.Amount;                    
                else
                    dTotalCreditSales += item.Amount;
            }

            dTotalSales = dTotalDailyCash + dTotalCreditSales;

            var paylist = db.SalesPaymentMethods.Where(y => y.RecordedFrom != "CN" && y.CreatedOn.Day == dt.Day && y.CreatedOn.Month == dt.Month && y.CreatedOn.Year == dt.Year).ToList();
            foreach (var item in paylist)
            {
                if (item.PaymentMethod == "Cash")
                    dReceivedByCash += item.Amount;
                else if (item.PaymentMethod == "NETS")
                    dReceivedByNETS += item.Amount;
                else if (item.PaymentMethod == "Credit Card")
                    dReceivedByCreditCard += item.Amount;
                else if (item.PaymentMethod == "Cheque")
                    dReceivedByCheque += item.Amount;
                else if (item.PaymentMethod == "Credit Note")
                    dReceivedByCreditNote += item.Amount;
            }

            dTotalReceived = dReceivedByCash + dReceivedByNETS + dReceivedByCreditCard + dReceivedByCheque + dReceivedByCreditNote;

            dTotalDifference = dTotalSales - dTotalReceived;

            // Cash sales detail listing
            var Cashsalesdetail = (from t1 in db.INVs
                               join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                               where (t1.InvType == "CS" && t2.RecordedFrom != "CN" && t2.CreatedOn.Day == dt.Day && t2.CreatedOn.Month == dt.Month && t2.CreatedOn.Year == dt.Year)
                               select new DailySalesReportDetail
                               {
                                   RDate = t1.InvDate,
                                   CompanyName = t1.CustName,
                                   InvoiceNo = t1.InvNo,
                                   Amount = t2.Amount
                               }).ToList();

            // Get Cash sales received from Payment Records

            DateTime dtoday = DateTime.Today;

            var cashSalesPRs = _getPaymentRecords("CS", dtoday);

            if (cashSalesPRs != null)
            {
                foreach (var pr in cashSalesPRs)
                {
                    Cashsalesdetail.Add(pr);
                }
            }


            // Credit sales detail listing
            var salesdetail = (from t1 in db.INVs
                              join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                               where (t1.InvType == "CR" && t2.RecordedFrom != "CN" && t2.CreatedOn.Day == dt.Day && t2.CreatedOn.Month == dt.Month && t2.CreatedOn.Year == dt.Year)
                              select new DailySalesReportDetail
                              {
                                  RDate = t1.InvDate,
                                  CompanyName = t1.CustName,
                                  InvoiceNo = t1.InvNo,
                                  Amount = t2.Amount
                              }).ToList();

            // Get Credit sales received from Payment Records

            var creditSalesPRs = _getPaymentRecords("CR", dtoday);

            if (creditSalesPRs != null)
            {
                foreach (var pr in creditSalesPRs)
                {
                    salesdetail.Add(pr);
                }
            }

            // List payment by cheque
            var chequedetail = (from t1 in db.INVs
                               join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                               join t3 in db.Clients on t1.CustNo equals t3.CustNo
                                where (t2.RecordedFrom != "CN" && t2.PaymentDate.Day == dt.Day && t2.PaymentDate.Month == dt.Month && t2.PaymentDate.Year == dt.Year && t2.PaymentMethod == "Cheque")
                               select new DailySalesReportDetail
                               {
                                   ChequeNo = t2.ChequeNumber,
                                   CompanyName = t1.CustName,
                                   InvoiceNo = t1.InvNo,
                                   TelephoneNo = t3.PhoneNo,
                                   Amount = t2.Amount
                               }).ToList();

            var chequeSalesPRs = _getPaymentRecordsByPayMode(dtoday, "Cheque");

            if (chequeSalesPRs != null)
            {
                foreach (var pr in chequeSalesPRs)
                {
                    chequedetail.Add(pr);
                }
            }

            // List payment by Nets

            var netsdetail = (from t1 in db.INVs
                                join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                                join t3 in db.Clients on t1.CustNo equals t3.CustNo
                                where (t2.RecordedFrom != "CN" && t2.PaymentDate.Day == dt.Day && t2.PaymentDate.Month == dt.Month && t2.PaymentDate.Year == dt.Year && t2.PaymentMethod == "NETS")
                                select new DailySalesReportDetail
                                {
                                    ChequeNo = t2.ChequeNumber,
                                    CompanyName = t1.CustName,
                                    InvoiceNo = t1.InvNo,
                                    TelephoneNo = t3.PhoneNo,
                                    Amount = t2.Amount
                                }).ToList();

            var netsSalesPRs = _getPaymentRecordsByPayMode(dtoday, "NETS");

            if (netsSalesPRs != null)
            {
                foreach (var pr in netsSalesPRs)
                {
                    netsdetail.Add(pr);
                }
            }

            // List payment by Credit Card

            var creditcarddetail = (from t1 in db.INVs
                              join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                              join t3 in db.Clients on t1.CustNo equals t3.CustNo
                              where (t2.RecordedFrom != "CN" && t2.PaymentDate.Day == dt.Day && t2.PaymentDate.Month == dt.Month && t2.PaymentDate.Year == dt.Year && t2.PaymentMethod == "Credit Card")
                              select new DailySalesReportDetail
                              {
                                  ChequeNo = t2.ChequeNumber,
                                  CompanyName = t1.CustName,
                                  InvoiceNo = t1.InvNo,
                                  TelephoneNo = t3.PhoneNo,
                                  Amount = t2.Amount
                              }).ToList();

            var creditcardSalesPRs = _getPaymentRecordsByPayMode(dtoday, "Credit Card");

            if (creditcardSalesPRs != null)
            {
                foreach (var pr in creditcardSalesPRs)
                {
                    creditcarddetail.Add(pr);
                }
            }

            // List payment by Credit Note

            var creditnotedetail = (from t1 in db.INVs
                                    join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                                    join t3 in db.Clients on t1.CustNo equals t3.CustNo
                                    where (t2.RecordedFrom != "CN" && t2.PaymentDate.Day == dt.Day && t2.PaymentDate.Month == dt.Month && t2.PaymentDate.Year == dt.Year && t2.PaymentMethod == "Credit Note")
                                    select new DailySalesReportDetail
                                    {
                                        ChequeNo = t2.ChequeNumber,
                                        CompanyName = t1.CustName,
                                        InvoiceNo = t1.InvNo,
                                        TelephoneNo = t3.PhoneNo,
                                        Amount = t2.Amount
                                    }).ToList();

            var creditnoteSalesPRs = _getPaymentRecordsByPayMode(dtoday, "Credit Note");

            if (creditnoteSalesPRs != null)
            {
                foreach (var pr in creditnoteSalesPRs)
                {
                    creditnotedetail.Add(pr);
                }
            }


            ViewBag.TotalDailyCash = dTotalDailyCash;
            ViewBag.TotalCreditSales = dTotalCreditSales;
            ViewBag.TotalSales = dTotalSales;
            ViewBag.ReceivedByCash = dReceivedByCash;
            ViewBag.ReceivedByNETS = dReceivedByNETS;
            ViewBag.ReceivedByCreditCard = dReceivedByCreditCard;
            ViewBag.ReceivedByCheque = dReceivedByCheque;
            ViewBag.ReceivedByCreditNote = dReceivedByCreditNote;
            ViewBag.TotalReceived = dTotalReceived;
            ViewBag.TotalDifference = dTotalDifference;

            ViewBag.CashSalesDetail = Cashsalesdetail;
            ViewBag.CSalesDetail = salesdetail;
            ViewBag.CChequeDetail = chequedetail;
            ViewBag.CNetsDetail = netsdetail;
            ViewBag.CCreditCardDetail = creditcarddetail;
            ViewBag.CCreditNoteDetail = creditnotedetail;

            ViewBag.StartDate = dt.ToShortDateString();

            Session["sePrintDailySales"] = dt.Date;

            return View();
        }



        private List<DailySalesReportDetail> _getPaymentRecords(string invtype, DateTime valdate)
        {
            List<DailySalesReportDetail> dsReports = new List<DailySalesReportDetail>();

        //    DateTime dtoday = DateTime.Today;

            var sps = db.SalesPaymentMethods.Where(x => x.PrID != 0 && DbFunctions.TruncateTime(x.PaymentDate) == DbFunctions.TruncateTime(valdate)).Select(x => x.PrID).Distinct().ToList();
            if (sps != null)
            {
                foreach (var sp in sps)
                {
                    var pr = db.PaymentReceipts.Find(sp);
                    if (pr != null)
                    {
                        var plist = pr.Remark;

                        if (plist != null)
                        {
                            List<int> invnos = new List<int>();
                            List<string> pays = plist.Split('|').ToList();
                            if (pays != null)
                            {
                                foreach (var pay in pays)
                                {
                                    string[] str = pay.Split('$');
                                    int invid = Convert.ToInt32(str[0].Trim());
                                    decimal pamount = Convert.ToDecimal(str[1].Trim());
                                    var inv = db.INVs.Where(x => x.Status != "Void" && x.InvID == invid && x.InvType == invtype).FirstOrDefault();

                                    if (inv != null)
                                    {
                                        var repo = new DailySalesReportDetail();
                                        repo.RDate = inv.InvDate;
                                        repo.CompanyName = inv.CustName;
                                        if (!string.IsNullOrEmpty(inv.CustName2))
                                        {
                                            repo.CompanyName = repo.CompanyName + " - " + inv.CustName2;
                                        }
                                        repo.InvoiceNo = inv.InvNo;
                                        repo.Amount = pamount;

                                        dsReports.Add(repo);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return dsReports;
        }

        private List<DailySalesReportDetail> _getPaymentRecordsByPayMode(DateTime valdate, string paymode)
        {
            List<DailySalesReportDetail> dsReports = new List<DailySalesReportDetail>();

          //  DateTime dtoday = DateTime.Today;

            var sps = db.SalesPaymentMethods.Where(x => x.PrID != 0 && DbFunctions.TruncateTime(x.PaymentDate) == DbFunctions.TruncateTime(valdate) && x.PaymentMethod == paymode).ToList();
            if (sps != null)
            {
                foreach (var sp in sps)
                {
                    var pr = db.PaymentReceipts.Find(sp.PrID);
                    if (pr != null)
                    {
                        var repo = new DailySalesReportDetail();
                        repo.RDate = sp.PaymentDate;
                        repo.CompanyName = pr.CustName;
                        if (!string.IsNullOrEmpty(pr.CustName2))
                        {
                            repo.CompanyName = repo.CompanyName + " - " + pr.CustName2;
                        }
                        repo.InvoiceNo = pr.PaymentFor;
                        repo.Amount = sp.Amount;
                        repo.ChequeNo = sp.ChequeNumber;                      

                        dsReports.Add(repo);
                    }

                }
            }

            return dsReports;
        }


        [HttpGet]
        public ActionResult ReportByDailySales(string fromDate)
        {
            decimal dTotalDailyCash = 0;
            decimal dTotalCreditSales = 0;
            decimal dTotalSales = 0;
            decimal dReceivedByCash = 0;
            decimal dReceivedByNETS = 0;
            decimal dReceivedByCreditCard = 0;
            decimal dReceivedByCheque = 0;
            decimal dReceivedByCreditNote = 0;

            decimal dTotalReceived = 0;
            decimal dTotalDifference = 0;

            var p = new List<INV>();

            var dt = DateTime.Today;

            if (!string.IsNullOrEmpty(fromDate))
            {
                dt = Convert.ToDateTime(fromDate);
            }

            var dailysales = db.SalesPaymentMethods.Where(y => y.RecordedFrom != "CN" && y.CreatedOn.Day == dt.Day && y.CreatedOn.Month == dt.Month && y.CreatedOn.Year == dt.Year).ToList();

            foreach (var item in dailysales)
            {
                if (item.InvType == "CS")
                    dTotalDailyCash += item.Amount;
                else
                    dTotalCreditSales += item.Amount;
            }

            //if (!string.IsNullOrEmpty(fromDate))
            //{
            //    string[] datesplit = fromDate.Split('/');
            //    dt = Convert.ToDateTime(datesplit[2] + "/" + datesplit[0] + "/" + datesplit[1]);
            //};

            //p = db.INVs.Where(x => x.InvDate.Day == dt.Day && x.InvDate.Month == dt.Month && x.InvDate.Year == dt.Year && x.Status != "Void").OrderBy(x => x.InvID).ToList();

            //foreach (var item in p)
            //{
            //    if (item.InvType == "CR")
            //        dTotalCreditSales += item.Nett;
            //    else
            //        dTotalDailyCash += item.Nett;
            //}

            dTotalSales = dTotalDailyCash + dTotalCreditSales;

            var paylist = db.SalesPaymentMethods.Where(y => y.RecordedFrom != "CN" && y.CreatedOn.Day == dt.Day && y.CreatedOn.Month == dt.Month && y.CreatedOn.Year == dt.Year).ToList();
            foreach (var item in paylist)
            {
                if (item.PaymentMethod == "Cash")
                    dReceivedByCash += item.Amount;
                else if (item.PaymentMethod == "NETS")
                    dReceivedByNETS += item.Amount;
                else if (item.PaymentMethod == "Credit Card")
                    dReceivedByCreditCard += item.Amount;
                else if (item.PaymentMethod == "Cheque")
                    dReceivedByCheque += item.Amount;
                else if (item.PaymentMethod == "Credit Note")
                    dReceivedByCreditNote += item.Amount;
            }

            dTotalReceived = dReceivedByCash + dReceivedByNETS + dReceivedByCreditCard + dReceivedByCheque + dReceivedByCreditNote;

            dTotalDifference = dTotalSales - dTotalReceived;

            // Cash sales detail listing
            var Cashsalesdetail = (from t1 in db.INVs
                                   join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                                   where (t1.InvType == "CS" && t2.RecordedFrom != "CN" && t2.CreatedOn.Day == dt.Day && t2.CreatedOn.Month == dt.Month && t2.CreatedOn.Year == dt.Year)
                                   select new DailySalesReportDetail
                                   {
                                       RDate = t1.InvDate,
                                       CompanyName = t1.CustName,
                                       InvoiceNo = t1.InvNo,
                                       Amount = t2.Amount
                                   }).ToList();

            // Get Cash sales received from Payment Records

            DateTime dtoday = dt;

            var cashSalesPRs = _getPaymentRecords("CS", dtoday);

            if (cashSalesPRs != null)
            {
                foreach (var pr in cashSalesPRs)
                {
                    Cashsalesdetail.Add(pr);
                }
            }

            // Credit sales detail listing
            var salesdetail = (from t1 in db.INVs
                               join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                               where (t1.InvType == "CR" && t2.RecordedFrom != "CN" && t2.CreatedOn.Day == dt.Day && t2.CreatedOn.Month == dt.Month && t2.CreatedOn.Year == dt.Year)
                               select new DailySalesReportDetail
                               {
                                   RDate = t1.InvDate,
                                   CompanyName = t1.CustName,
                                   InvoiceNo = t1.InvNo,
                                   Amount = t2.Amount
                               }).ToList();

            // Get Credit sales received from Payment Records

            var creditSalesPRs = _getPaymentRecords("CR", dtoday);

            if (creditSalesPRs != null)
            {
                foreach (var pr in creditSalesPRs)
                {
                    salesdetail.Add(pr);
                }
            }

            // List payment by cheque
            var chequedetail = (from t1 in db.INVs
                                join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                                join t3 in db.Clients on t1.CustNo equals t3.CustNo
                                where (t2.RecordedFrom != "CN" && t2.PaymentDate.Day == dt.Day && t2.PaymentDate.Month == dt.Month && t2.PaymentDate.Year == dt.Year && t2.PaymentMethod == "Cheque")
                                select new DailySalesReportDetail
                                {
                                    ChequeNo = t2.ChequeNumber,
                                    CompanyName = t1.CustName,
                                    InvoiceNo = t1.InvNo,
                                    TelephoneNo = t3.PhoneNo,
                                    Amount = t2.Amount
                                }).ToList();

            var chequeSalesPRs = _getPaymentRecordsByPayMode(dtoday, "Cheque");

            if (chequeSalesPRs != null)
            {
                foreach (var pr in chequeSalesPRs)
                {
                    chequedetail.Add(pr);
                }
            }

            // List payment by Nets

            var netsdetail = (from t1 in db.INVs
                              join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                              join t3 in db.Clients on t1.CustNo equals t3.CustNo
                              where (t2.RecordedFrom != "CN" && t2.PaymentDate.Day == dt.Day && t2.PaymentDate.Month == dt.Month && t2.PaymentDate.Year == dt.Year && t2.PaymentMethod == "NETS")
                              select new DailySalesReportDetail
                              {
                                  ChequeNo = t2.ChequeNumber,
                                  CompanyName = t1.CustName,
                                  InvoiceNo = t1.InvNo,
                                  TelephoneNo = t3.PhoneNo,
                                  Amount = t2.Amount
                              }).ToList();

            var netsSalesPRs = _getPaymentRecordsByPayMode(dtoday, "NETS");

            if (netsSalesPRs != null)
            {
                foreach (var pr in netsSalesPRs)
                {
                    netsdetail.Add(pr);
                }
            }

            // List payment by Credit Card

            var creditcarddetail = (from t1 in db.INVs
                                    join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                                    join t3 in db.Clients on t1.CustNo equals t3.CustNo
                                    where (t2.RecordedFrom != "CN" && t2.PaymentDate.Day == dt.Day && t2.PaymentDate.Month == dt.Month && t2.PaymentDate.Year == dt.Year && t2.PaymentMethod == "Credit Card")
                                    select new DailySalesReportDetail
                                    {
                                        ChequeNo = t2.ChequeNumber,
                                        CompanyName = t1.CustName,
                                        InvoiceNo = t1.InvNo,
                                        TelephoneNo = t3.PhoneNo,
                                        Amount = t2.Amount
                                    }).ToList();

            var creditcardSalesPRs = _getPaymentRecordsByPayMode(dtoday, "Credit Card");

            if (creditcardSalesPRs != null)
            {
                foreach (var pr in creditcardSalesPRs)
                {
                    creditcarddetail.Add(pr);
                }
            }

            // List payment by Credit Note

            var creditnotedetail = (from t1 in db.INVs
                                    join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                                    join t3 in db.Clients on t1.CustNo equals t3.CustNo
                                    where (t2.RecordedFrom != "CN" && t2.PaymentDate.Day == dt.Day && t2.PaymentDate.Month == dt.Month && t2.PaymentDate.Year == dt.Year && t2.PaymentMethod == "Credit Note")
                                    select new DailySalesReportDetail
                                    {
                                        ChequeNo = t2.ChequeNumber,
                                        CompanyName = t1.CustName,
                                        InvoiceNo = t1.InvNo,
                                        TelephoneNo = t3.PhoneNo,
                                        Amount = t2.Amount
                                    }).ToList();

            var creditnoteSalesPRs = _getPaymentRecordsByPayMode(dtoday, "Credit Note");

            if (creditnoteSalesPRs != null)
            {
                foreach (var pr in creditnoteSalesPRs)
                {
                    creditnotedetail.Add(pr);
                }
            }


            ViewBag.TotalDailyCash = dTotalDailyCash;
            ViewBag.TotalCreditSales = dTotalCreditSales;
            ViewBag.TotalSales = dTotalSales;
            ViewBag.ReceivedByCash = dReceivedByCash;
            ViewBag.ReceivedByNETS = dReceivedByNETS;
            ViewBag.ReceivedByCreditCard = dReceivedByCreditCard;
            ViewBag.ReceivedByCheque = dReceivedByCheque;
            ViewBag.ReceivedByCreditNote = dReceivedByCreditNote;
            ViewBag.TotalReceived = dTotalReceived;
            ViewBag.TotalDifference = dTotalDifference;

            ViewBag.CashSalesDetail = Cashsalesdetail;
            ViewBag.CSalesDetail = salesdetail;
            ViewBag.CChequeDetail = chequedetail;
            ViewBag.CNetsDetail = netsdetail;
            ViewBag.CCreditCardDetail = creditcarddetail;
            ViewBag.CCreditNoteDetail = creditnotedetail;

            ViewBag.StartDate = dt.ToShortDateString();

            Session["sePrintDailySales"] = dt.Date;

            return View();
        }

        [HttpGet]
        public ActionResult ViewHistory(string txtSearch, string fromDate, string toDate)
        {
            DateTime d1 = DateTime.Today.AddDays(-90);
            DateTime d2 = DateTime.Today;

            if (!string.IsNullOrEmpty(fromDate))
            {
                d1 = Convert.ToDateTime(fromDate);
            };
            if (!string.IsNullOrEmpty(toDate))
            {
                d2 = Convert.ToDateTime(toDate);
            };


            List<InvDetView> getList = (from t1 in db.INVDETs
                                              join t2 in db.INVs
                                                  on t1.InvID equals t2.InvID
                                        where (t1.ItemName.Contains(txtSearch) || t1.ItemCode == txtSearch || t2.InvID.ToString() == txtSearch || t2.CustName.Contains(txtSearch)) && ((t2.InvDate >= d1 && t2.InvDate <= d2) && (t2.Status != "Void")) 
                                              select new InvDetView
                                              {
                                                  InvDet = t1,
                                                  InvDate = t2.InvDate,
                                                  CustNo = t2.CustNo,
                                                  CustName = t2.CustName,
                                                  PersonName = t2.PersonName,                                         

                                              }).ToList();


            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d2.ToShortDateString();
            ViewBag.SearchWords = txtSearch;

            return View(getList);
        }

        public ActionResult PrintDailySales()
        {
            decimal dTotalDailyCash = 0;
            decimal dTotalCreditSales = 0;
            decimal dTotalSales = 0;
            decimal dReceivedByCash = 0;
            decimal dReceivedByNETS = 0;
            decimal dReceivedByCreditCard = 0;
            decimal dReceivedByCheque = 0;
            decimal dReceivedByCreditNote = 0;
            decimal dTotalReceived = 0;
            decimal dTotalDifference = 0;

            string fromDate = "";

            if (Session["sePrintDailySales"] != null && Session["sePrintDailySales"] != "")
            {
                DateTime dNew = Convert.ToDateTime(Session["sePrintDailySales"].ToString());
                fromDate = dNew.Month + "/" + dNew.Day + "/" + dNew.Year;
            }

            var p = new List<INV>();

            var dt = DateTime.Now;

            if (!string.IsNullOrEmpty(fromDate))
            {
                string[] datesplit = fromDate.Split('/');
                dt = Convert.ToDateTime(datesplit[2] + "/" + datesplit[0] + "/" + datesplit[1]);
            };

            var dailysales = db.SalesPaymentMethods.Where(y => y.RecordedFrom != "CN" && y.CreatedOn.Day == dt.Day && y.CreatedOn.Month == dt.Month && y.CreatedOn.Year == dt.Year).ToList();

            foreach (var item in dailysales)
            {
                if (item.InvType == "CS")
                    dTotalDailyCash += item.Amount;
                else
                    dTotalCreditSales += item.Amount;
            }


            dTotalSales = dTotalDailyCash + dTotalCreditSales;

            var paylist = db.SalesPaymentMethods.Where(y => y.RecordedFrom != "CN" && y.CreatedOn.Day == dt.Day && y.CreatedOn.Month == dt.Month && y.CreatedOn.Year == dt.Year).ToList();
            foreach (var item in paylist)
            {
                if (item.PaymentMethod == "Cash")
                    dReceivedByCash += item.Amount;
                else if (item.PaymentMethod == "NETS")
                    dReceivedByNETS += item.Amount;
                else if (item.PaymentMethod == "Credit Card")
                    dReceivedByCreditCard += item.Amount;
                else if (item.PaymentMethod == "Cheque")
                    dReceivedByCheque += item.Amount;
                else if (item.PaymentMethod == "Credit Note")
                    dReceivedByCreditNote += item.Amount;
            }

            dTotalReceived = dReceivedByCash + dReceivedByNETS + dReceivedByCreditCard + dReceivedByCheque + dReceivedByCreditNote;

            dTotalDifference = dTotalSales - dTotalReceived;

            // Cash sales detail listing
            var Cashsalesdetail = (from t1 in db.INVs
                                   join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                                   where (t1.InvType == "CS" && t2.RecordedFrom != "CN" && t2.CreatedOn.Day == dt.Day && t2.CreatedOn.Month == dt.Month && t2.CreatedOn.Year == dt.Year)
                                   select new DailySalesReportDetail
                                   {
                                       RDate = t1.InvDate,
                                       CompanyName = t1.CustName,
                                       InvoiceNo = t1.InvNo,
                                       Amount = t2.Amount
                                   }).ToList();

            // Get Cash sales received from Payment Records

            DateTime dtoday =dt;

            var cashSalesPRs = _getPaymentRecords("CS", dtoday);

            if (cashSalesPRs != null)
            {
                foreach (var pr in cashSalesPRs)
                {
                    Cashsalesdetail.Add(pr);
                }
            }


            // Credit sales detail listing
            var salesdetail = (from t1 in db.INVs
                               join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                               where (t1.InvType == "CR" && t2.RecordedFrom != "CN" && t2.CreatedOn.Day == dt.Day && t2.CreatedOn.Month == dt.Month && t2.CreatedOn.Year == dt.Year)
                               select new DailySalesReportDetail
                               {
                                   RDate = t1.InvDate,
                                   CompanyName = t1.CustName,
                                   InvoiceNo = t1.InvNo,
                                   Amount = t2.Amount
                               }).ToList();

            // Get Credit sales received from Payment Records

            var creditSalesPRs = _getPaymentRecords("CR", dtoday);

            if (creditSalesPRs != null)
            {
                foreach (var pr in creditSalesPRs)
                {
                    salesdetail.Add(pr);
                }
            }

            // List payment by cheque
            var chequedetail = (from t1 in db.INVs
                               join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                               join t3 in db.Clients on t1.CustNo equals t3.CustNo
                                where (t2.RecordedFrom != "CN" && t2.PaymentDate.Day == dt.Day && t2.PaymentDate.Month == dt.Month && t2.PaymentDate.Year == dt.Year && t2.PaymentMethod == "Cheque")
                               select new DailySalesReportDetail
                               {
                                   ChequeNo = t2.ChequeNumber,
                                   CompanyName = t1.CustName,
                                   InvoiceNo = t1.InvNo,
                                   TelephoneNo = t3.PhoneNo,
                                   Amount = t2.Amount
                               }).ToList();

            var chequeSalesPRs = _getPaymentRecordsByPayMode(dtoday, "Cheque");

            if (chequeSalesPRs != null)
            {
                foreach (var pr in chequeSalesPRs)
                {
                    chequedetail.Add(pr);
                }
            }

            // List payment by Nets

            var netsdetail = (from t1 in db.INVs
                                join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                                join t3 in db.Clients on t1.CustNo equals t3.CustNo
                                where (t2.RecordedFrom != "CN" && t2.PaymentDate.Day == dt.Day && t2.PaymentDate.Month == dt.Month && t2.PaymentDate.Year == dt.Year && t2.PaymentMethod == "NETS")
                                select new DailySalesReportDetail
                                {
                                    ChequeNo = t2.ChequeNumber,
                                    CompanyName = t1.CustName,
                                    InvoiceNo = t1.InvNo,
                                    TelephoneNo = t3.PhoneNo,
                                    Amount = t2.Amount
                                }).ToList();

            var netsSalesPRs = _getPaymentRecordsByPayMode(dtoday, "NETS");

            if (netsSalesPRs != null)
            {
                foreach (var pr in netsSalesPRs)
                {
                    netsdetail.Add(pr);
                }
            }

            // List payment by Credit Card

            var creditcarddetail = (from t1 in db.INVs
                              join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                              join t3 in db.Clients on t1.CustNo equals t3.CustNo
                              where (t2.RecordedFrom != "CN" && t2.PaymentDate.Day == dt.Day && t2.PaymentDate.Month == dt.Month && t2.PaymentDate.Year == dt.Year && t2.PaymentMethod == "Credit Card")
                              select new DailySalesReportDetail
                              {
                                  ChequeNo = t2.ChequeNumber,
                                  CompanyName = t1.CustName,
                                  InvoiceNo = t1.InvNo,
                                  TelephoneNo = t3.PhoneNo,
                                  Amount = t2.Amount
                              }).ToList();

            var creditcardSalesPRs = _getPaymentRecordsByPayMode(dtoday, "Credit Card");

            if (creditcardSalesPRs != null)
            {
                foreach (var pr in creditcardSalesPRs)
                {
                    creditcarddetail.Add(pr);
                }
            }

            // List payment by Credit Note

            var creditnotedetail = (from t1 in db.INVs
                                    join t2 in db.SalesPaymentMethods on t1.InvID equals t2.InvID
                                    join t3 in db.Clients on t1.CustNo equals t3.CustNo
                                    where (t2.RecordedFrom != "CN" && t2.PaymentDate.Day == dt.Day && t2.PaymentDate.Month == dt.Month && t2.PaymentDate.Year == dt.Year && t2.PaymentMethod == "Credit Note")
                                    select new DailySalesReportDetail
                                    {
                                        ChequeNo = t2.ChequeNumber,
                                        CompanyName = t1.CustName,
                                        InvoiceNo = t1.InvNo,
                                        TelephoneNo = t3.PhoneNo,
                                        Amount = t2.Amount
                                    }).ToList();

            var creditnoteSalesPRs = _getPaymentRecordsByPayMode(dtoday, "Credit Note");

            if (creditnoteSalesPRs != null)
            {
                foreach (var pr in creditnoteSalesPRs)
                {
                    creditnotedetail.Add(pr);
                }
            }


            ViewBag.TotalDailyCash = dTotalDailyCash;
            ViewBag.TotalCreditSales = dTotalCreditSales;
            ViewBag.TotalSales = dTotalSales;
            ViewBag.ReceivedByCash = dReceivedByCash;
            ViewBag.ReceivedByNETS = dReceivedByNETS;
            ViewBag.ReceivedByCreditCard = dReceivedByCreditCard;
            ViewBag.ReceivedByCheque = dReceivedByCheque;
            ViewBag.ReceivedByCreditNote = dReceivedByCreditNote;
            ViewBag.TotalReceived = dTotalReceived;
            ViewBag.TotalDifference = dTotalDifference;

            ViewBag.CashSalesDetail = Cashsalesdetail;
            ViewBag.CSalesDetail = salesdetail;
            ViewBag.CChequeDetail = chequedetail;
            ViewBag.CNetsDetail = netsdetail;
            ViewBag.CCreditCardDetail = creditcarddetail;
            ViewBag.CCreditNoteDetail = creditnotedetail;

            ViewBag.StartDate = dt.ToShortDateString();

            return View();
        }

        public ActionResult CashSalesList()
        {
            var p = new List<INV>();

            var d1 = DateTime.Now;

            p = db.INVs.Where(x => x.InvType == "CS" && x.InvDate.Date == d1.Date && x.Status != "Void").OrderBy(x => x.InvNo).ToList();

            decimal sumAmount = db.INVs.Where(x => x.InvType == "CS" && x.InvDate.Date == d1.Date && x.Status != "Void").Sum(c => (decimal?)c.Amount) ?? 0;
            decimal sumGST = db.INVs.Where(x => x.InvType == "CS" && x.InvDate.Date == d1.Date && x.Status != "Void").Sum(c => (decimal?)c.Gst) ?? 0;
            decimal sumNett = db.INVs.Where(x => x.InvType == "CS" && x.InvDate.Date == d1.Date && x.Status != "Void").Sum(c => (decimal?)c.Nett) ?? 0;

            ViewBag.TotalAmount = sumAmount;
            ViewBag.TotalGST = sumGST;
            ViewBag.TotalNett = sumNett;

            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d1.ToShortDateString();

            return View(p);
        }

        [HttpGet]
        public ActionResult CashSalesList(string fromDate, string toDate)
        {
            DateTime d1 = DateTime.Today;
            DateTime d2 = DateTime.Today;

            if (!string.IsNullOrEmpty(fromDate))
            {
                d1 = Convert.ToDateTime(fromDate);
            };
            if (!string.IsNullOrEmpty(toDate))
            {
                d2 = Convert.ToDateTime(toDate);
            };

            var p = new List<INV>();

            p = db.INVs.Where(x => (x.InvType == "CS") && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();

            decimal sumAmount = db.INVs.Where(x => (x.InvType == "CS") && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
            decimal sumGST = db.INVs.Where(x => (x.InvType == "CS") && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
            decimal sumNett = db.INVs.Where(x => (x.InvType == "CS") && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;

            ViewBag.TotalAmount = sumAmount;
            ViewBag.TotalGST = sumGST;
            ViewBag.TotalNett = sumNett;

            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d2.ToShortDateString();

            return View(p);
        }

        public ActionResult CreditSalesList()
        {
            var p = new List<INV>();

            var d1 = DateTime.Now;

            p = db.INVs.Where(x => x.InvType == "CR" && x.InvDate.Date == d1.Date && x.Status != "Void").OrderBy(x => x.InvNo).ToList();

            decimal sumAmount = db.INVs.Where(x => x.InvType == "CR" && x.InvDate.Date == d1.Date && x.Status != "Void").Sum(c => (decimal?)c.Amount) ?? 0;
            decimal sumGST = db.INVs.Where(x => x.InvType == "CR" && x.InvDate.Date == d1.Date && x.Status != "Void").Sum(c => (decimal?)c.Gst) ?? 0;
            decimal sumNett = db.INVs.Where(x => x.InvType == "CR" && x.InvDate.Date == d1.Date && x.Status != "Void").Sum(c => (decimal?)c.Nett) ?? 0;

            ViewBag.TotalAmount = sumAmount;
            ViewBag.TotalGST = sumGST;
            ViewBag.TotalNett = sumNett;

            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d1.ToShortDateString();

            return View(p);
        }

        [HttpGet]
        public ActionResult CreditSalesList(string fromDate, string toDate, string invType)
        {
            DateTime d1 = DateTime.Today;
            DateTime d2 = DateTime.Today;
            decimal sumAmount = 0;
            decimal sumGST = 0;
            decimal sumNett = 0;


            if (!string.IsNullOrEmpty(fromDate))
            {
                d1 = Convert.ToDateTime(fromDate);
            };
            if (!string.IsNullOrEmpty(toDate))
            {
                d2 = Convert.ToDateTime(toDate);
            };

            var p = new List<INV>();

            if (!string.IsNullOrEmpty(invType))
            {
                if (invType == "All")
                {
                    p = db.INVs.Where(x => (x.InvType == "CR") && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();
                    sumAmount = db.INVs.Where(x => (x.InvType == "CR") && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
                    sumGST = db.INVs.Where(x => (x.InvType == "CR") && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
                    sumNett = db.INVs.Where(x => (x.InvType == "CR") && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;
                }
                if (invType == "Paid")
                {
                    p = db.INVs.Where(x => (x.InvType == "CR") && (x.IsPaid == true) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();
                    sumAmount = db.INVs.Where(x => (x.InvType == "CR") && (x.IsPaid == true) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
                    sumGST = db.INVs.Where(x => (x.InvType == "CR") && (x.IsPaid == true) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
                    sumNett = db.INVs.Where(x => (x.InvType == "CR") && (x.IsPaid == true) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;

                }
                if (invType == "Unpaid")
                {
                    p = db.INVs.Where(x => (x.InvType == "CR") && (x.IsPaid == false) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();
                    sumAmount = db.INVs.Where(x => (x.InvType == "CR") && (x.IsPaid == false) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
                    sumGST = db.INVs.Where(x => (x.InvType == "CR") && (x.IsPaid == false) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
                    sumNett = db.INVs.Where(x => (x.InvType == "CR") && (x.IsPaid == false) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;

                }

            }
            else
            {
                p = db.INVs.Where(x => (x.InvType == "CR") && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();
                sumAmount = db.INVs.Where(x => (x.InvType == "CR") && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
                sumGST = db.INVs.Where(x => (x.InvType == "CR") && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
                sumNett = db.INVs.Where(x => (x.InvType == "CR") && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;

            }


            ViewBag.TotalAmount = sumAmount;
            ViewBag.TotalGST = sumGST;
            ViewBag.TotalNett = sumNett;

            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d2.ToShortDateString();

            return View(p);
        }

        public ActionResult SalesAnalysisByBill()
        {
            var p = new List<INV>();

            //var d1 = DateTime.Now;

            //p = db.INVs.Where(x => x.InvDate.Date == d1.Date && x.Status != "Void").OrderBy(x => x.InvNo).ToList();

            //decimal sumAmount = db.INVs.Where(x => x.InvDate.Date == d1.Date && x.Status != "Void").Sum(c => (decimal?)c.Amount) ?? 0;
            //decimal sumGST = db.INVs.Where(x => x.InvDate.Date == d1.Date && x.Status != "Void").Sum(c => (decimal?)c.Gst) ?? 0;
            //decimal sumNett = db.INVs.Where(x => x.InvDate.Date == d1.Date && x.Status != "Void").Sum(c => (decimal?)c.Nett) ?? 0;

            //ViewBag.TotalAmount = sumAmount;
            //ViewBag.TotalGST = sumGST;
            //ViewBag.TotalNett = sumNett;

            //ViewBag.StartDate = d1.ToShortDateString();
            //ViewBag.EndDate = d1.ToShortDateString();
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewBag.CustomerNo = 0;
            ViewBag.InvoiceType = "";
            return View(p);
        }

        [HttpGet]
        public ActionResult SalesAnalysisByBill(string custNo, string fromDate, string toDate, string invType)
        {
            int custno = Convert.ToInt32(custNo);
            DateTime d1 = DateTime.Today;
            DateTime d2 = DateTime.Today;
            decimal sumAmount = 0;
            decimal sumGST = 0;
            decimal sumNett = 0;           


            if (!string.IsNullOrEmpty(fromDate))
            {
                d1 = Convert.ToDateTime(fromDate);
            };
            if (!string.IsNullOrEmpty(toDate))
            {
                d2 = Convert.ToDateTime(toDate);
            };

            var p = new List<INV>();

            if (!string.IsNullOrEmpty(invType))
            {
                if (invType == "All")
                {
                    p = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();
                    sumAmount = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
                    sumGST = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
                    sumNett = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;
                }
                if (invType == "Paid")
                {
                    p = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == true) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();
                    sumAmount = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == true) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
                    sumGST = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == true) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
                    sumNett = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == true) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;

                }
                if (invType == "Unpaid")
                {
                    p = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == false) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();
                    sumAmount = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == false) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
                    sumGST = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == false) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
                    sumNett = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == false) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;

                }

            }
            else
            {
                p = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();
                sumAmount = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
                sumGST = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
                sumNett = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;

            }


            ViewBag.TotalAmount = sumAmount;
            ViewBag.TotalGST = sumGST;
            ViewBag.TotalNett = sumNett;

            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d2.ToShortDateString();

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewBag.CustomerNo = custno;
            ViewBag.InvoiceType = invType;

            return View(p);
        }

        public ActionResult SalesAnalysisByBillPrint(string custNo, string fromDate, string toDate, string invType)
        {
            int custno = Convert.ToInt32(custNo);
            DateTime d1 = DateTime.Today;
            DateTime d2 = DateTime.Today;
            decimal sumAmount = 0;
            decimal sumGST = 0;
            decimal sumNett = 0;


            if (!string.IsNullOrEmpty(fromDate))
            {
                d1 = Convert.ToDateTime(fromDate);
            };
            if (!string.IsNullOrEmpty(toDate))
            {
                d2 = Convert.ToDateTime(toDate);
            };

            var p = new List<INV>();

            if (!string.IsNullOrEmpty(invType))
            {
                if (invType == "All")
                {
                    p = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();
                    sumAmount = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
                    sumGST = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
                    sumNett = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;
                }
                if (invType == "Paid")
                {
                    p = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == true) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();
                    sumAmount = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == true) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
                    sumGST = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == true) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
                    sumNett = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == true) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;

                }
                if (invType == "Unpaid")
                {
                    p = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == false) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();
                    sumAmount = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == false) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
                    sumGST = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == false) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
                    sumNett = db.INVs.Where(x => (x.CustNo == custno) && (x.IsPaid == false) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;

                }

            }
            else
            {
                p = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).OrderBy(x => x.InvNo).ToList();
                sumAmount = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Amount) ?? 0;
                sumGST = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Gst) ?? 0;
                sumNett = db.INVs.Where(x => (x.CustNo == custno) && (x.InvDate >= d1 && x.InvDate <= d2) && (x.Status != "Void")).Sum(c => (decimal?)c.Nett) ?? 0;

            }


            ViewBag.TotalAmount = sumAmount;
            ViewBag.TotalGST = sumGST;
            ViewBag.TotalNett = sumNett;

            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d2.ToShortDateString();

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewBag.CustomerNo = custno;
            ViewBag.InvoiceType = invType;

            return View(p);
        }




    }
}