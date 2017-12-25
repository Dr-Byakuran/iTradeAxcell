using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using iTrade.Models;
using System.Transactions;
using InvoiceNo;

namespace iTrade.Controllers
{
    public class PaymentController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();
        private InvoiceClass invoice = new InvoiceClass();

        public decimal GetGstRate()
        {
            var gstrate = db.GstRate.FirstOrDefault();
            if (gstrate == null)
            {
                GST o = new GST();
                o.GstRate = 7;
                db.GstRate.Add(o);
                db.SaveChanges();
            }

            decimal gst = (decimal?)db.GstRate.FirstOrDefault().GstRate ?? 0;

            return gst / 100;
        }

        public ActionResult Index(string txtSearch)
        {
            var result = db.PaymentReceipts.OrderByDescending(x => x.PrID).Take(200).ToList();

            if (!string.IsNullOrEmpty(txtSearch))
            {
                result = db.PaymentReceipts.Where(x => x.CustName.Contains(txtSearch) || x.PaymentMode.StartsWith(txtSearch)).OrderByDescending(x => x.PrID).Take(200).ToList();
            }

            return View(result);
        }

        public ActionResult Create()
        {
            var p = new PaymentReceipt();

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["seGSTRate"] = GetGstRate();

            return View(p);
        }

        // POST: Sales/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PrID,PrNo,CustNo,CustName,CustName2,AccType,Addr1,Addr2,Addr3,Addr4,Attn,PaymentDate,PaymentFor,PaymentMode,Amount,ChequeNumber,IsFullPayment,Status,Remark,PersonID,PersonName")] PaymentReceipt inv)
        {
            string str = GetMaxPaymentNumber();
         //   string str = invoice.GetInvoiceNumber(InvType.PR.ToString(), DateTime.Now, User.Identity.Name);

            inv.PrNo = str;
            inv.Amount = 0;
            inv.CreatedBy = User.Identity.Name;
            inv.CreatedOn = DateTime.Now;

            Boolean flag = false;

            if (ModelState.IsValid)
            {
                db.PaymentReceipts.Add(inv);
                db.SaveChanges();

                flag = true;
            };

            if (flag)
            {
                //CreateKIV(inv.SorID);

                return RedirectToAction("Edit", new { id = inv.PrID, str = "1" });
            }

            ViewBag.Message = "1";

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            ViewData["seGSTRate"] = GetGstRate();

            return View(inv);
        }

        public ActionResult View(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PaymentReceipt inv = db.PaymentReceipts.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["seGSTRate"] = GetGstRate();
            return View(inv);
        }
    
        public ActionResult Edit(int? id, string str)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PaymentReceipt inv = db.PaymentReceipts.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();
            ViewData["seGSTRate"] = GetGstRate();

            ViewBag.PageFrom = str;

            return View(inv);
        }

        // POST: Sales/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PrID,PrNo,CustNo,CustName,CustName2,AccType,Addr1,Addr2,Addr3,Addr4,Attn,PaymentDate,PaymentFor,PaymentMode,Amount,ChequeNumber,IsFullPayment,Status,Remark,PersonID,PersonName,CreatedBy,CreatedOn")] PaymentReceipt inv)
        {

            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();

                string str = Request.Form["actionType"];

                if (str == "SaveAndAdd")
                {
                    return RedirectToAction("Create");
                };

                return RedirectToAction("Edit", new { id = inv.PrID });
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            return View(inv);
        }

        public ActionResult _OrderDetail(int? id, string act)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var inv = db.INVs.Where(x => x.CustNo == id && x.IsPaid == false).OrderBy(x => x.InvNo).ToList();

            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewBag.Act = act;
            ViewData["seGSTRate"] = GetGstRate();

            return PartialView(inv);
        }

        public ActionResult _OrderDetailView(int? id, string act)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var pr = db.PaymentReceipts.Where(x => x.PrID == id).FirstOrDefault();
            var paylist = pr.Remark;

            if (paylist == null)
            {
                return HttpNotFound();
            }

            List<int> invnos = new List<int>();
            List<string> pays = paylist.Split('|').ToList();
            if (pays != null)
            {
                foreach (var pay in pays)
                {
                    string[] str = pay.Split('$');
                    int invid = Convert.ToInt32(str[0].Trim());
                    invnos.Add(invid);
                }
            }            

            var inv = db.INVs.Where(x => x.Status != "Void" && (invnos.Contains(x.InvID))).OrderBy(x => x.InvNo).ToList();

            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewBag.Act = act;
            ViewData["seGSTRate"] = GetGstRate();
            ViewData["InvList"] = pays;

            return PartialView(inv);
        }



        [HttpGet]
        public JsonResult _SubmitPayment(int SorID, Boolean CheckWithPayment, Boolean CheckBoxCash, Boolean CheckBoxNETS, Boolean CheckBoxCreditCard, Boolean CheckBoxCheque, Boolean CheckBoxCreditNote, string CheckBoxCashAmount, string CheckBoxNETSAmount, string CheckBoxCreditCardAmount, string CheckBoxChequeAmount, string CheckBoxCreditNoteAmount, string CheckBoxNETSNumber, string CheckBoxCreditCardNumber, string CheckBoxChequeNumber, string CheckBoxCreditNoteNumber, string PayFor)

   //     public JsonResult _SubmitPayment(int SorID, Boolean CheckWithoutPayment, Boolean CheckBoxCash, Boolean CheckBoxNETS, Boolean CheckBoxCreditCard, Boolean CheckBoxCheque, string CheckBoxCashAmount, string CheckBoxNETSAmount, string CheckBoxCreditCardAmount, string CheckBoxChequeAmount, string CheckBoxChequeNumber, string PayFor)
        {
            //SalesOrder oinv = db.SalesOrders.Find(SorID);

            PaymentReceipt oinv = db.PaymentReceipts.Find(SorID);
            Client client = db.Clients.Find(oinv.CustNo);
            decimal dPaymentAmount = 0;
            decimal dOriginalNett = 0;
            decimal dTotalPaid = 0;
            decimal dOutstandingAmount = 0;
            Boolean bFullPayment = false;
            string payby = "";
            string payfor = "";

            var pays = PayFor.Split('|').ToList();
            foreach (var pay in pays)
            {
                string[] str = pay.Split('$');
                int invid = Convert.ToInt32(str[0].Trim());
                decimal payamount = Convert.ToDecimal(str[1].Trim());

                string invNo = db.INVs.Find(invid).InvNo;

                if (payfor == "")
                {
                    payfor = invNo;
                }
                else
                {
                    payfor = payfor + "," + invNo;
                }

                UpdateINV(invid, payamount);

            }



            Boolean isWithoutPayment = !CheckWithPayment;

            if (CheckBoxCash)
            {               
                dPaymentAmount += Convert.ToDecimal(CheckBoxCashAmount);
                payby += "Cash";
            }

            if (CheckBoxNETS)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxNETSAmount);
                if (payby == "")
                {
                    payby = "NETS";
                }
                else
                {
                    payby += ",NETS";
                }
 
            }

            if (CheckBoxCreditCard)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxCreditCardAmount);
                if (payby == "")
                {
                    payby = "Credit Card";
                }
                else
                {
                    payby += ",Credit Card";
                }
            }

            if (CheckBoxCheque)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxChequeAmount);
                if (payby == "")
                {
                    payby = "Cheque";
                }
                else
                {
                    payby += ",Cheque";
                }
            }

            if (CheckBoxCreditNote)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxCreditNoteAmount);
                if (payby == "")
                {
                    payby = "Credit Note";
                }
                else
                {
                    payby += ",Credit Note";
                }
            }


            if (client.AccType == "CR")
            {
                //Check credit limit
            }

            int id = Convert.ToInt32(SorID);

            var sor = db.PaymentReceipts.Find(id);
            if (sor == null)
            {
                return Json(new { success = false, responseText = "The sales order is not found. No valid data. Please refresh page." }, JsonRequestBehavior.AllowGet);
            }
            else {

                sor.Amount = dPaymentAmount;
                sor.PaymentMode = payby;
                sor.PaymentFor = payfor;
                sor.Status = "Completed";
                sor.Remark = PayFor;

                if (ModelState.IsValid)
                {
                    db.Entry(sor).State = EntityState.Modified;
                    db.SaveChanges();

                };
            
            };

            var oinvoice = db.INVs.Where(x => x.SorID == SorID).FirstOrDefault();
            INV invs = new INV();
            string sInvoiceStatus = "";
            Boolean IsPaid = false;

            if (oinvoice == null)
            {
 
            }
            else
            {
                invs = oinvoice;

                if (!isWithoutPayment)
                {
                  
                    if (dOutstandingAmount == dPaymentAmount)
                    {
                        sInvoiceStatus = "Full Paid";
                        IsPaid = true;
                    }
                    else
                    {
                        sInvoiceStatus = "Partially Paid";
                    }

                    if (ModelState.IsValid)
                    {
                        invs.Status = sInvoiceStatus;
                        invs.IsPaid = IsPaid;
                        invs.PaidDate = DateTime.Now;

                        db.Entry(invs).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(sor).State = EntityState.Modified;
                        db.SaveChanges();

                    };
                }
            }

            if (!isWithoutPayment)
            {
                List<SalesPaymentMethod> l = new List<SalesPaymentMethod>();
                SalesPaymentMethod oPay = new SalesPaymentMethod();
                if (CheckBoxCash && Convert.ToDecimal(CheckBoxCashAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = 0,
                        InvID = 0,
                        PrID = SorID,
                        InvType = client.AccType,
                        PaymentDate = oinv.PaymentDate,
                        RecordedFrom = "PR",

                        PaymentMethod = "Cash",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxCashAmount),
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (CheckBoxNETS && Convert.ToDecimal(CheckBoxNETSAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = 0,
                        InvID = 0,
                        PrID = SorID,
                        InvType = client.AccType,
                        PaymentDate = oinv.PaymentDate,
                        RecordedFrom = "PR",

                        PaymentMethod = "NETS",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxNETSAmount),
                        ChequeNumber = CheckBoxNETSNumber,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }


                if (CheckBoxCreditCard && Convert.ToDecimal(CheckBoxCreditCardAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = 0,
                        InvID = 0,
                        PrID = SorID,
                        InvType = client.AccType,
                        PaymentDate = oinv.PaymentDate,
                        RecordedFrom = "PR",

                        PaymentMethod = "Credit Card",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxCreditCardAmount),
                        ChequeNumber = CheckBoxCreditCardNumber,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (CheckBoxCheque && Convert.ToDecimal(CheckBoxChequeAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = 0,
                        InvID = 0,
                        PrID = SorID,
                        InvType = client.AccType,
                        PaymentDate = oinv.PaymentDate,
                        RecordedFrom = "PR",

                        PaymentMethod = "Cheque",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxChequeAmount),
                        ChequeNumber = CheckBoxChequeNumber,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (CheckBoxCreditNote && Convert.ToDecimal(CheckBoxCreditNoteAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = 0,
                        InvID = 0,
                        PrID = SorID,
                        InvType = client.AccType,
                        PaymentDate = oinv.PaymentDate,
                        RecordedFrom = "PR",

                        PaymentMethod = "Credit Note",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxCreditCardAmount),
                        ChequeNumber = CheckBoxCreditNoteNumber,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (l.Count > 0)
                {
                    for (int i = 0; i <= l.Count - 1; i++)
                    {
                        db.SalesPaymentMethods.Add(l[i]);
                        db.SaveChanges();
                    }
                }
            }


 

            return Json(new
            {
                printUrl = Url.Action("ReceiptPrintPreview", "Invoice", new { id = sor.PrID }),
                redirectUrl = Url.Action("OrderProcessed", "Payment", new { id = sor.PrID }),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult _SubmitPaymentByInvID(int SorID, Boolean CheckWithoutPayment, Boolean CheckBoxCash, Boolean CheckBoxNETS, Boolean CheckBoxCreditCard, Boolean CheckBoxCheque, string CheckBoxCashAmount, string CheckBoxNETSAmount, string CheckBoxCreditCardAmount, string CheckBoxChequeAmount, string CheckBoxChequeNumber, string PayFor)
        {
            //SalesOrder oinv = db.SalesOrders.Find(SorID);

            var inv = db.INVs.Find(SorID);
            PaymentReceipt oinv = new PaymentReceipt();

            string newprno = invoice.GetInvoiceNumber(InvType.PR.ToString(), DateTime.Now, User.Identity.Name);
            oinv.PrNo = newprno;
            oinv.CustNo = inv.CustNo;
            oinv.CustName = inv.CustName;
            oinv.CustName2 = inv.CustName2;
            oinv.AccType = inv.InvType;
            oinv.Addr1 = inv.Addr1;
            oinv.Addr2 = inv.Addr2;
            oinv.Addr3 = inv.Addr3;
            oinv.Addr4 = inv.Addr4;
            oinv.Attn = inv.Attn;
            oinv.PaymentDate = DateTime.Now;
            oinv.PaymentFor = inv.InvNo;
            oinv.PaymentMode = "";
            oinv.IsFullPayment = false;
            oinv.PersonID = inv.PersonID;
            oinv.PersonName = inv.PersonName;

            oinv.CreatedBy = User.Identity.Name;
            oinv.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.PaymentReceipts.Add(oinv);
                db.SaveChanges();
            };

            Client client = db.Clients.Find(oinv.CustNo);
            decimal dPaymentAmount = 0;
            decimal dOriginalNett = 0;
            decimal dTotalPaid = 0;
            decimal dOutstandingAmount = 0;
            Boolean bFullPayment = false;
            string payby = "";
            string payfor = inv.InvNo;

            Boolean isWithoutPayment = CheckWithoutPayment;

            if (CheckBoxCash)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxCashAmount);
                payby += "Cash";
            }

            if (CheckBoxNETS)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxNETSAmount);
                if (payby == "")
                {
                    payby = "NETS";
                }
                else
                {
                    payby += ",NETS";
                }

            }

            if (CheckBoxCreditCard)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxCreditCardAmount);
                if (payby == "")
                {
                    payby = "Credit Card";
                }
                else
                {
                    payby += ",Credit Card";
                }
            }

            if (CheckBoxCheque)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxChequeAmount);
                if (payby == "")
                {
                    payby = "Cheque";
                }
                else
                {
                    payby += ",Cheque";
                }
            }

            //if (dOriginalNett == dPaymentAmount)
            //    bFullPayment = true;

            ////var dbpayment = db.SalesPaymentMethods.ToList().Where(x => x.PrID == SorID);

            ////foreach (var item in dbpayment)
            ////{
            ////    dTotalPaid += item.Amount;
            ////}

            //dOutstandingAmount = dOriginalNett - dTotalPaid;

            //if (dOutstandingAmount < dPaymentAmount)
            //{
            //    return Json(new { success = false, responseText = "The total amount (" + dPaymentAmount.ToString("C") + ") you input is more than the nett amount (" + dOutstandingAmount.ToString("C") + "). Please change again." }, JsonRequestBehavior.AllowGet);
            //}

            if (client.AccType == "CR")
            {
                //Check credit limit
            }

            int id = Convert.ToInt32(SorID);

            var sor = db.PaymentReceipts.Find(oinv.PrID);
            if (sor == null)
            {
                return Json(new { success = false, responseText = "No valid data. Please refresh page." }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                sor.Amount = dPaymentAmount;
                sor.PaymentMode = payby;
                sor.PaymentFor = payfor;
                sor.Status = "Completed";
                sor.Remark = inv.InvID + "$" + dPaymentAmount;

                if (ModelState.IsValid)
                {
                    db.Entry(sor).State = EntityState.Modified;
                    db.SaveChanges();

                };

            };

            SorID = sor.PrID;

            var oinvoice = db.INVs.Where(x => x.InvID == inv.InvID).FirstOrDefault();
            INV invs = new INV();
            string sInvoiceStatus = "";
            Boolean IsPaid = false;

            if (oinvoice == null)
            {

            }
            else
            {
                invs = oinvoice;

                if (!isWithoutPayment)
                {
                    if (dOutstandingAmount == dPaymentAmount)
                    {
                        sInvoiceStatus = "Full Paid";
                        IsPaid = true;
                    }
                    else
                    {
                        sInvoiceStatus = "Partially Paid";
                    }

                    if (ModelState.IsValid)
                    {
                        invs.Status = sInvoiceStatus;
                        invs.IsPaid = IsPaid;
                        invs.PaidDate = DateTime.Now;

                        db.Entry(invs).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(sor).State = EntityState.Modified;
                        db.SaveChanges();

                    };

                       UpdateINV(inv.InvID, dPaymentAmount);
                }
            }

            if (!isWithoutPayment)
            {
                List<SalesPaymentMethod> l = new List<SalesPaymentMethod>();
                SalesPaymentMethod oPay = new SalesPaymentMethod();
                if (CheckBoxCash && Convert.ToDecimal(CheckBoxCashAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = 0,
                        InvID = 0,
                        PrID = SorID,
                        InvType = client.AccType,
                        PaymentDate = oinv.PaymentDate,
                        RecordedFrom = "PR",

                        PaymentMethod = "Cash",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxCashAmount),
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (CheckBoxNETS && Convert.ToDecimal(CheckBoxNETSAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = 0,
                        InvID = 0,
                        PrID = SorID,
                        InvType = client.AccType,
                        PaymentDate = oinv.PaymentDate,
                        RecordedFrom = "PR",

                        PaymentMethod = "NETS",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxNETSAmount),
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (CheckBoxCreditCard && Convert.ToDecimal(CheckBoxCreditCardAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = 0,
                        InvID = 0,
                        PrID = SorID,
                        InvType = client.AccType,
                        PaymentDate = oinv.PaymentDate,
                        RecordedFrom = "PR",

                        PaymentMethod = "Credit Card",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxCreditCardAmount),
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (CheckBoxCheque && Convert.ToDecimal(CheckBoxChequeAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = 0,
                        InvID = 0,
                        PrID = SorID,
                        InvType = client.AccType,
                        PaymentDate = oinv.PaymentDate,
                        RecordedFrom = "PR",

                        PaymentMethod = "Cheque",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxChequeAmount),
                        ChequeNumber = CheckBoxChequeNumber,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (l.Count > 0)
                {
                    for (int i = 0; i <= l.Count - 1; i++)
                    {
                        db.SalesPaymentMethods.Add(l[i]);
                        db.SaveChanges();
                    }
                }
            }




            return Json(new
            {
                printUrl = Url.Action("ReceiptPrintPreview", "Invoice", new { id = sor.PrID }),
                redirectUrl = Url.Action("OrderProcessed", "Payment", new { id = sor.PrID }),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }



        private void UpdateINV(int invno, decimal payamount)
        {
            var inv = db.INVs.Find(invno);
            inv.PaidAmount = inv.PaidAmount + payamount;
            if (inv.PaidAmount >= inv.Nett)
            {
                inv.PaymentStatus = "Full Paid";
                inv.IsPaid = true;
            }
            if (inv.PaidAmount < inv.Nett)
            {
                inv.PaymentStatus = "Partially Paid";
                inv.IsPaid = false;
            }
            if (inv.PaidAmount == 0)
            {
                inv.PaymentStatus = "Unpaid";
                inv.IsPaid = false;
            }

            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();
            };


            var sor = db.SalesOrders.Find(inv.SorID);
            if (sor != null)
            {
                sor.PaidAmount = inv.PaidAmount;
                sor.PaymentStatus = inv.PaymentStatus;
                sor.IsPaid = inv.IsPaid;

                if (ModelState.IsValid)
                {
                    db.Entry(sor).State = EntityState.Modified;
                    db.SaveChanges();
                };
            }

        }

        public ActionResult Payment()
        {
            var inv = new INV();

            ViewData["InvoicesAll"] = db.INVs.Where(x => x.Nett > x.PaidAmount).OrderBy(x => x.InvID).ToList();

            return View(inv);
        }

        // GET: Sales/Details/5
        [HttpPost]
        public ActionResult Payment(string invno)
        {
            if (string.IsNullOrEmpty(invno))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var inv = db.INVs.Where(x => x.InvNo == invno.Trim()).FirstOrDefault();
            if (inv == null)
            {
                return HttpNotFound();
            }

            if (inv != null)
            {
                ViewBag.InvoiceNo = inv.InvID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };

            ViewData["InvoicesAll"] = db.INVs.Where(x => x.Nett > x.PaidAmount).OrderBy(x => x.InvID).ToList();
            ViewData["seGSTRate"] = GetGstRate();
            return View(inv);
        }




    }
}