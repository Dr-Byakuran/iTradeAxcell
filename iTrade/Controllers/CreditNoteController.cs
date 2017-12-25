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
    public class CreditNoteController : ControllerBase
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

        // GET: CreditNote
        public ActionResult Index(string txtSearch)
        {
            var result = db.CreditNotes.OrderByDescending(x => x.CnID).Take(200).ToList();

            if (!string.IsNullOrEmpty(txtSearch))
            {
                result = db.CreditNotes.Where(x => x.CustName.Contains(txtSearch) || x.CnNo.StartsWith(txtSearch)).OrderByDescending(x => x.CnID).Take(200).ToList();
            }

            return View(result);
        }

        // GET: Sales/Details/5
        public ActionResult View(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreditNote inv = db.CreditNotes.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            //ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            //ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            var item = db.INVs.Where(x => x.InvNo == inv.InvNo).FirstOrDefault();
            if (item != null)
            {
                ViewBag.InvoiceNo = item.InvID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };

            ViewData["seGSTRate"] = GetGstRate();
            return View(inv);
        }

        // GET: Sales/Create
        public ActionResult Create()
        {
            var p = new CreditNote();

            //var cust = db.Clients.Where(x => x.AccType == "CS" && x.CustName == "CASH SALES").FirstOrDefault();
            //if (cust != null)
            //{
            //    p.CustNo = cust.CustNo;
            //    p.CustName = cust.CustName;

            //}

            //  p.SarOn.DateCreated = DateTime.Now;
            //  ViewBag.ProductTypes = db.ProductGroups.ToList();

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
        public ActionResult Create([Bind(Include = "CnID,CnNo,InvID,InvNo,InvType,DocType,InvDate,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,PaymentTerms,Status,Remark,PersonID,PersonName")] CreditNote inv)
        {
            string str = GetMaxCreditNoteNumber();
            inv.CnNo = str;
            inv.PreDiscAmount = 0;
            inv.Discount = 0;
            inv.Amount = 0;
            inv.Gst = 0;
            inv.Nett = 0;
            inv.NettInWords = "";
 
            inv.CreatedBy = User.Identity.Name;
            inv.CreatedOn = DateTime.Now;

            Boolean flag = false;

            if (ModelState.IsValid)
            {
                db.CreditNotes.Add(inv);
                db.SaveChanges();

                flag = true;
            };

            if (flag)
            {
                //CreateKIV(inv.SorID);

                return RedirectToAction("Edit", new { id = inv.CnID, str = "1" });
            }

            ViewBag.Message = "1";

            ViewData["ClientsAll"] = db.Clients.Where(x => x.AccType == "CS" && x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            ViewData["seGSTRate"] = GetGstRate();

            return View(inv);
        }

        // GET: Sales/Edit/5
        public ActionResult Edit(int? id, string str)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreditNote inv = db.CreditNotes.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();
            ViewData["seGSTRate"] = GetGstRate();

            var item = db.INVs.Where(x => x.InvNo == inv.InvNo).FirstOrDefault();
            if (item != null)
            {
                ViewBag.InvoiceNo = item.InvID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };

            ViewBag.PageFrom = str;

            return View(inv);
        }

        // POST: Sales/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CnID,CnNo,InvID,InvNo,InvType,DocType,InvDate,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,Amount,Gst,Nett,NettInWords,PaymentTerms,Status,Remark,PersonID,PersonName,CreatedOn")] CreditNote inv)
        //public ActionResult Edit([Bind(Include = "SorID,QuoID,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,PostalCode,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PreDiscAmount,Discount,Amount,Gst,Nett,PaymentTerms,Remark,PersonID,PersonName,CreatedOn")] SalesOrder inv)
        {
            inv.ModifiedBy = User.Identity.Name;
            inv.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();

                string str = Request.Form["actionType"];

                if (str == "SaveAndAdd")
                {
                    return RedirectToAction("Create");
                };

                return RedirectToAction("Edit", new { id = inv.CnID });
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            return View(inv);
        }

        // GET: Sales/Edit/5
        public ActionResult _OrderDetail(int? id, string act)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreditNote inv = db.CreditNotes.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewBag.Act = act;
            ViewData["seGSTRate"] = GetGstRate();

            return PartialView(inv);
        }

        public ActionResult _CreditNoteInvList(int? id, string act)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var inv = db.INVs.Where(x => (x.CustNo == id) && ((x.Nett - x.PaidAmount) > 0)).OrderBy(x => x.InvNo).ToList();

            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewBag.Act = act;
            ViewData["seGSTRate"] = GetGstRate();

            return PartialView(inv);
        }


        [HttpGet]
        public JsonResult _SaveKnockOff(int SorID, string PayFor)
        {
            // Get current Credit Note and its Knock off list
            // Get list of new knock off list, include invid and amount
            // Update Knock off list

          //  int cnid = Convert.ToInt32(SorID);

            var sor = db.CreditNotes.Find(SorID);
            if (sor == null)
            {
                return Json(new { success = false, responseText = "The credit note is not found. No valid data. Please refresh page." }, JsonRequestBehavior.AllowGet);
            }

            string payfor = "";
            var invnos = new List<int>();

            var pays = PayFor.Split('|').ToList();
            foreach (var pay in pays)
            {
                string[] str = pay.Split('$');
                int invid = Convert.ToInt32(str[0].Trim());
                decimal payamount = Convert.ToDecimal(str[1].Trim());

                invnos.Add(invid);

                string invNo = db.INVs.Find(invid).InvNo;

                if (payfor == "")
                {
                    payfor = invNo;
                }
                else
                {
                    payfor = payfor + "," + invNo;
                }

                UpdateCreditNoteDets(SorID, invid, payamount);

            }

            var dets = db.CreditNoteDets.Where(x => x.CnID == SorID).ToList();
            foreach (var det in dets)
            {
                bool exists = invnos.Contains(det.InvID);
                if (!exists)
                {
                    db.CreditNoteDets.Remove(det);
                    db.SaveChanges();
                }

            }


            return Json(new { success = true, redirectUrl = Url.Action("Edit", "CreditNote", new { id = SorID, str = "0" }) }, JsonRequestBehavior.AllowGet);


            //return Json(new
            //{
            //    printUrl = Url.Action("ReceiptPrintPreview", "Invoice", new { id = sor.CnID }),
            //    redirectUrl = Url.Action("OrderProcessed", "Payment", new { id = sor.PrID }),
            //    isRedirect = true
            //}, JsonRequestBehavior.AllowGet);
        }

        private void UpdateCreditNoteDets(int cnid, int invid, decimal payamount)
        {
            var det = db.CreditNoteDets.Where(x => x.CnID == cnid && x.InvID == invid).FirstOrDefault();

            if (det != null)
            {
                det.KnockOffAmount = payamount;
                if (ModelState.IsValid)
                {
                    db.Entry(det).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                var cn = db.CreditNotes.Find(cnid);
                var inv = db.INVs.Find(invid);
                if (cn !=  null && inv != null) {
                  var newdet = new CreditNoteDet();
                    newdet.CnID = cn.CnID;
                    newdet.CnNo = cn.CnNo;
                    newdet.InvID = inv.InvID;
                    newdet.InvNo = inv.InvNo;
                    newdet.InvType = inv.InvType;
                    newdet.DocType = cn.DocType;
                    newdet.InvDate = inv.InvDate;
                    newdet.CustNo = inv.CustNo;
                    newdet.CustName = inv.CustName;
                    newdet.CustName2 = inv.CustName2;
                    newdet.Amount = inv.Amount;
                    newdet.Gst = inv.Gst;
                    newdet.Nett = inv.Nett;
                    newdet.PaidAmount = inv.PaidAmount;
                    newdet.KnockOffAmount = payamount;
                    newdet.Status = "Draft";
                    newdet.Remark = "";
                    newdet.RefItemID = 0;
                    newdet.Position = 0;

                    if (ModelState.IsValid)
                    {
                        db.CreditNoteDets.Add(newdet);
                        db.SaveChanges();
                    }

                }
            }
        }




        public ActionResult _DisplayInvDets(int id, string act)
        {
            var p = new List<INVDET>();
            p = db.INVDETs
                .Where(x => (x.CnID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        public ActionResult _DisplayInvDetsPrint(int id)
        {
            var p = new List<INVDET>();
            p = db.INVDETs
                .Where(x => (x.CnID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _DisplayKnockOffs(int id, string act)
        {
            var p = new List<CreditNoteDet>();
            p = db.CreditNoteDets
                .Where(x => (x.CnID == id))
                .OrderBy(x => x.InvNo)
                .ToList();

            decimal sumAmount = db.CreditNoteDets.Where(c => c.CnID == id).Sum(c => (decimal?)c.KnockOffAmount) ?? 0;

            ViewBag.SumKnockOff = sumAmount;
            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _DisplayKnockOffs(List<CreditNoteDet> list)
        {
            if (list == null)
            {
                return null;
            }
            
            int SorID = list.FirstOrDefault().CnID;
            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.CreditNoteDets.Find(i.DetID);
                    if (det != null)
                    {

                        det.KnockOffAmount = i.KnockOffAmount;

                    }

                    //   UpdateKivDet(det);
                };
                db.SaveChanges();
            };

            //    UpdateKivDets(SorID);

            //    return Json(new { success = true });

            return RedirectToAction("Edit", new { id = SorID });

        }

        public ActionResult _AddItem(int id)
        {
            var inv = db.CreditNotes.Find(id);

            var p = new INVDET();
            p.CnID = inv.CnID;
            p.CnNo = inv.CnNo;
            p.InvType = inv.InvType;

            //   var getList = GetProductList();

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //     ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //  ViewData["ProductData"] = getList;

            var poptions = new List<PriceOption>();
            ViewData["PriceOptionsAll"] = poptions;
            ViewData["seGSTRate"] = GetGstRate();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return PartialView(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddItem(INVDET data)
        {
            var ps = db.Products.Where(x => x.ProductID == data.ItemID).FirstOrDefault();

            //decimal costprice = ps.CostPrice;
            //string costcode = Decimal2String(costprice);
            //data.Remark = costcode;

            data.Nett = data.Amount + data.Gst;

            var invdet1 = db.INVDETs.Where(x => x.RefItemID == 0 && x.SorID == data.SorID).ToList();
            double positioncount = invdet1.Count;
            data.Position = positioncount + 1;

            if (ModelState.IsValid)
            {
                db.INVDETs.Add(data);
                db.SaveChanges();
            };

       //     UpdateKivDets(data.SorID);

            //   AddKivDet(data);

         //   int bundlecount = 0;

            //if (ps.IsBundle)
            //{
            //    foreach (var pb in ps.Productbundles)
            //    {
            //        var p = db.Products.Where(x => x.SKU == pb.IncSKU).FirstOrDefault();

            //        if (p != null)
            //        {
            //            bundlecount++;
            //            var det = new INVDET();
            //            det.SorID = data.SorID;
            //            det.InvID = data.InvID;
            //            det.InvType = data.InvType;
            //            det.ItemID = data.ItemID;
            //            det.ItemCode = pb.IncSKU;
            //            det.ItemType = data.ItemType;
            //            det.ItemName = pb.IncProductName;
            //            det.SellType = data.SellType;
            //            det.Qty = Convert.ToDouble(pb.IncQty);
            //            det.Unit = p.Unit;
            //            det.SalesType = "BundleItem";
            //            det.RefItemID = data.DetID;
            //            det.Remark = "";
            //            det.IsControlItem = p.IsControlItem;
            //            det.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());

            //            if (ModelState.IsValid)
            //            {
            //                db.INVDETs.Add(det);
            //                db.SaveChanges();

            //            };
            //            UpdateKivDets(det.SorID);
            //            //  AddKivDet(det);
            //        }

            //    }
            //}



            UpdateContractAmount(data.CnID);

            var totalAmount = db.CreditNotes.Where(x => x.CnID == data.CnID).FirstOrDefault().Nett;
            var detCount = db.INVDETs.Count(x => x.CnID == data.CnID);

            ViewBag.Message = "1";
            var poptions = new List<PriceOption>();
            ViewData["PriceOptionsAll"] = poptions;
            ViewData["seGSTRate"] = GetGstRate();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return Json(new { success = true, totalamount = totalAmount, detcount = detCount, redirectUrl = Url.Action("Edit", "CreditNote", new { id = data.CnID, str = "1" }) });

        }



        public ActionResult _AddDet(int id)
        {
            var inv = db.CreditNotes.Find(id);
            var p = new INVDET();
            p.CnID = inv.CnID;
            p.CnNo = inv.CnNo;

            //   var getList = GetProductList();

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //     ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //  ViewData["ProductData"] = getList;

            decimal gst = GetGstRate();
            ViewBag.GstRate = gst;
            ViewBag.CN_Number = id;

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _AddDet(INVDET data)
        {
            data.Nett = data.Amount + data.Gst;
            int count = db.INVDETs.Where(x => x.CnID == data.CnID).Count();
            data.Position = count + 1;

            if (ModelState.IsValid)
            {
                db.INVDETs.Add(data);
                db.SaveChanges();
            };

       //     UpdateContractAmount(data.CnID);

            ViewBag.Message = "1";

            decimal gst = GetGstRate();
            ViewBag.GstRate = gst;

            return Json(new { redirectUrl = Url.Action("Edit", "CreditNote", new { id = data.CnID }) });

        }

        private void UpdateContractAmount(int id)
        {
            //   decimal sumPreDiscAmount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.PreDiscAmount) ?? 0;
            //   decimal sumDiscount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.Discount) ?? 0;
            decimal sumAmount = db.INVDETs.Where(c => c.CnID == id).Sum(c => (decimal?)c.Amount) ?? 0;

            decimal gst = GetGstRate();

            //     decimal sumGst = sumAmount * gst;
            //     decimal sumNett = sumAmount + sumGst;

            CreditNote inv = db.CreditNotes.Find(id);
            if (inv != null)
            {
                inv.PreDiscAmount = sumAmount;

                if (sumAmount == 0)
                {
                    inv.Discount = 0;
                };
                //    inv.Discount = sumDiscount;

                inv.Amount = sumAmount + inv.Discount;
                inv.Gst = System.Math.Round(inv.Amount * gst, 2, MidpointRounding.AwayFromZero);
                inv.Nett = inv.Amount + inv.Gst;

                if (ModelState.IsValid)
                {
                    db.Entry(inv).State = EntityState.Modified;
                    db.SaveChanges();
                };
            };

        }


        public ActionResult _DelDet(int id = 0)
        {
            INVDET det = db.INVDETs.Find(id);
            if (det == null)
            {
                return HttpNotFound();
            }
            return PartialView("_DelDet", det);
        }


        [HttpPost, ActionName("_DelDet")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var det = db.INVDETs.Find(id);
                double position = det.Position;

                int SorID = det.CnID;

                var dets = db.INVDETs.Where(x => x.CnID == det.CnID && x.RefItemID == det.DetID).ToList();
                foreach (var dd in dets)
                {
                    db.Entry(dd).State = EntityState.Deleted;
                }

                //   db.INVDETs.Remove(det);
                db.Entry(det).State = EntityState.Deleted;

                db.SaveChanges();

                //  update sales agreement total amount

                UpdateContractAmount(SorID);

                trans.Complete();
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public JsonResult _SubmitCreditNoteOrder(int SorID)
        {
         //   string str = invoice.GetInvoiceNumber(InvType.CN.ToString(), DateTime.Now, User.Identity.Name);

            CreditNote ko = db.CreditNotes.Find(SorID);
        //    ko.CnNo = str; 
            ko.Status = "Confirmed";
            ko.ModifiedBy = User.Identity.Name;
            ko.ModifiedOn = System.DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(ko).State = EntityState.Modified;
                db.SaveChanges();
            };

            var dets = db.CreditNoteDets.Where(x => x.CnID == SorID).ToList();
            foreach (var det in dets)
            {
              //  det.CnNo = str;
                det.Status = "Confirmed";
                if (ModelState.IsValid)
                {
                    db.Entry(det).State = EntityState.Modified;
                    db.SaveChanges();
                };

                // update each invoice knock off amount to SalesPaymentMethod

                var pay = new SalesPaymentMethod();
                pay.InvID = det.InvID;
                pay.InvType = det.InvType;
                pay.PaymentDate = DateTime.Now;
                pay.RecordedFrom = "CN";
                pay.PaymentMethod = "CreditNote";
                pay.Amount = det.KnockOffAmount;
                pay.IsFullPayment = false;
                pay.CreatedBy = User.Identity.Name;
                pay.CreatedOn = DateTime.Now;

                if (ModelState.IsValid)
                {
                    db.SalesPaymentMethods.Add(pay);
                    db.SaveChanges();
                };

                UpdateINV(det.InvID, det.KnockOffAmount);

            }

            return Json(new
            {
                printUrl = Url.Action("CreditNotePrintPreview", "Invoice", new { id = ko.CnID }),
                redirectUrl = Url.Action("Create", "CreditNote"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }

        private void UpdateINV(int invno, decimal payamount)
        {
            var inv = db.INVs.Find(invno);
            var sor = db.SalesOrders.Find(inv.SorID);

            inv.PaidAmount = inv.PaidAmount + payamount;
            sor.PaidAmount = sor.PaidAmount + payamount;

            if (inv.PaidAmount >= inv.Nett)
            {
                inv.PaymentStatus = "Full Paid";
                inv.IsPaid = true;
                sor.PaymentStatus = "Full Paid";
                sor.IsPaid = true;
            }
            if (inv.PaidAmount < inv.Nett)
            {
                inv.PaymentStatus = "Partially Paid";
                inv.IsPaid = false;
                sor.PaymentStatus = "Partially Paid";
                sor.IsPaid = false;
            }
            if (inv.PaidAmount == 0)
            {
                inv.PaymentStatus = "Unpaid";
                inv.IsPaid = false;
                sor.PaymentStatus = "Unpaid";
                sor.IsPaid = false;
            }

            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();
            };

            if (ModelState.IsValid)
            {
                db.Entry(sor).State = EntityState.Modified;
                db.SaveChanges();
            };


        }

 


    }
}