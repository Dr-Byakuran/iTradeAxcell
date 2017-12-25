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
    public class ExchangeController : ControllerBase
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

        // GET: Exchange
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _DisplayResults(string invtype)
        {
            DateTime datefrom = DateTime.Now.AddMonths(-12);

            var p = new List<ExchangeOrder>();

            p = db.ExchangeOrders.Where(x => x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.EorID).ToList();

            return PartialView(p);
        }

        public ActionResult View(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExchangeOrder inv = db.ExchangeOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            //ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            //ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            //var item = db.INVs.Where(x => x.SorID == id).FirstOrDefault();
            //if (item != null)
            //{
            //    ViewBag.InvoiceNo = item.InvID;
            //}
            //else
            //{
            //    ViewBag.InvoiceNo = "";
            //};

            ViewData["seGSTRate"] = GetGstRate();
            return View(inv);
        }

        public ActionResult Create()
        {
            var p = new ExchangeOrder();

            //var cust = db.Clients.Where(x => x.AccType == "CS" && x.CustName == "CASH SALES").FirstOrDefault();
            //if (cust != null)
            //{
            //    p.CustNo = cust.CustNo;
            //    p.CustName = cust.CustName;

            //}

            //  p.SarOn.DateCreated = DateTime.Now;
            //  ViewBag.ProductTypes = db.ProductGroups.ToList();

            ViewData["ClientsAll"] = GetClientListByUser("ALL");
         //   ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["seGSTRate"] = GetGstRate();

            return View(p);
        }

        // POST: Sales/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EorID,EorNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,DeliveryAddress,DeliveryDate,DeliveryTime,PaymentTerms,Status,Remark,PersonID,PersonName")] ExchangeOrder inv)
        {
            string str = GetMaxExchangeNumber();
         //   string str = invoice.GetInvoiceNumber(InvType.DO.ToString(), DateTime.Now, User.Identity.Name);

            inv.EorNo = str;

            inv.PreDiscAmount = 0;
            inv.Discount = 0;
            inv.Amount = 0;
            inv.Gst = 0;
            inv.Nett = 0;
            inv.IsPaid = false;
            inv.CreatedBy = User.Identity.Name;
            inv.CreatedOn = DateTime.Now;

            Boolean flag = false;

            if (ModelState.IsValid)
            {
                db.ExchangeOrders.Add(inv);
                db.SaveChanges();

                flag = true;
            };

            if (flag)
            {
                //CreateKIV(inv.SorID);

                return RedirectToAction("Edit", new { id = inv.EorID, str = "1" });
            }

            ViewBag.Message = "1";

            ViewData["ClientsAll"] = GetClientListByUser("ALL");
         //   ViewData["ClientsAll"] = db.Clients.Where(x => x.AccType == "CS" && x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            ViewData["seGSTRate"] = GetGstRate();

            return View(inv);
        }

        public ActionResult Edit(int? id, string str)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExchangeOrder inv = db.ExchangeOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = GetClientListByUser("ALL");
          //  ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();
            ViewData["seGSTRate"] = GetGstRate();

            //var item = db.INVs.Where(x => x.SorID == id).FirstOrDefault();
            //if (item != null)
            //{
            //    ViewBag.InvoiceNo = item.InvID;
            //}
            //else
            //{
            //    ViewBag.InvoiceNo = "";
            //};

            ViewBag.PageFrom = str;

            return View(inv);
        }

        // POST: Sales/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EorID,EorNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PreDiscAmount,Discount,Amount,Gst,Nett,PaymentTerms,Remark,PersonID,PersonName,CreatedOn")] ExchangeOrder inv)
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

                return RedirectToAction("Edit", new { id = inv.EorID });
            }

            ViewData["ClientsAll"] = GetClientListByUser("ALL");

         //   ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
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
            ExchangeOrder inv = db.ExchangeOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewBag.Act = act;
            ViewData["seGSTRate"] = GetGstRate();

            return PartialView(inv);
        }


        public ActionResult _DisplayInvDets(int id, string act)
        {
            var p = new List<INVDET>();
            p = db.INVDETs
                .Where(x => (x.EorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _DisplayInvDets(List<INVDET> list)
        {
            if (list == null)
            {
                return null;
            }

            var sor = list.FirstOrDefault();
            int SorID = sor.EorID;
            string invtype = sor.InvType;

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.INVDETs.Find(i.DetID);
                    if (det != null)
                    {
                        det.Remark = i.Remark;
                        det.Qty = i.Qty;
                        det.Unit = i.Unit;
                        det.DiscountedPrice = i.DiscountedPrice;
                        det.Discount = det.DiscountedPrice - det.UnitPrice;
                        det.Amount = System.Math.Round((det.DiscountedPrice * Convert.ToDecimal(det.Qty)), 2, MidpointRounding.AwayFromZero);
                        det.Nett = det.Amount + det.Gst;

                    }

                };
                db.SaveChanges();
            };

            UpdateContractAmount(SorID);

            return RedirectToAction("Edit", new { id = SorID });

            //  return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult _DisplayOrderDets(int id, string act)
        {
            var p = new List<ExchangeOrderDet>();
            p = db.ExchangeOrderDets
                .Where(x => (x.EorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        public ActionResult _AddItem(int id)
        {
            var inv = db.ExchangeOrders.Find(id);

            var det = new INVDET();
          //  var det = new ExchangeOrderDet();
            det.EorID = inv.EorID;
            det.EorNo = inv.EorNo;
            det.InvType = inv.InvType;

            //   var getList = GetProductList();

            ViewData["ClientsAll"] = GetClientListByUser("ALL");
         //   ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //     ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //  ViewData["ProductData"] = getList;

            var poptions = new List<PriceOption>();
            ViewData["PriceOptionsAll"] = poptions;
            ViewData["seGSTRate"] = GetGstRate();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return PartialView(det);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddItem(INVDET data)
        {
            var ps = db.Products.Where(x => x.ProductID == data.ItemID).FirstOrDefault();

            data.Nett = data.Amount + data.Gst;

            var invdet1 = db.INVDETs.Where(x => x.EorID == data.EorID).ToList();
            double positioncount = invdet1.Count;
            data.Position = positioncount + 1;

            if (ModelState.IsValid)
            {
                db.INVDETs.Add(data);
                db.SaveChanges();
            };

            UpdateContractAmount(data.EorID);
            var totalAmount = db.ExchangeOrders.Where(x => x.EorID == data.EorID).FirstOrDefault().Nett;
            var detCount = db.INVDETs.Count(x => x.EorID == data.EorID);

            ViewBag.Message = "1";
            var poptions = new List<PriceOption>();
            ViewData["PriceOptionsAll"] = poptions;
            ViewData["seGSTRate"] = GetGstRate();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return Json(new { success = true, totalamount = totalAmount, detcount = detCount, redirectUrl = Url.Action("Edit", "Exchange", new { id = data.EorID, str = "1" }) });

        }

        public ActionResult _AddDetBundle(int id, double qty, decimal unitprice, decimal discprice, int SorID, string selltype, string remark, string invref, int locid, string locname)
        {
            var sor = db.ExchangeOrders.Where(x => x.EorID == SorID).FirstOrDefault();

            if (sor == null)
            {
                return null;
            }

            var invtype = sor.InvType;


            var dets = new List<INVDET>();
            var p = db.Products.Find(id);

            if (p != null)
            {
                var invdet1 = db.INVDETs.Where(x => x.RefItemID == 0 && x.EorID == sor.EorID).ToList();
                double positioncount = invdet1.Count;

                var det1 = new INVDET();
                det1.EorID = sor.EorID;
                det1.EorNo = sor.EorNo;
                det1.InvType = sor.InvType;
                det1.ItemID = p.ProductID;
                det1.ItemCode = p.SKU;
                det1.ItemType = p.ProductType;
                det1.ItemName = p.ProductName;
                det1.SellType = selltype;
                det1.Qty = Convert.ToDouble(qty);
                det1.Unit = p.Unit;
                det1.UnitPrice = unitprice;
                det1.DiscountedPrice = discprice;

                if (selltype != "RT")
                {
                    det1.Discount = discprice - unitprice;
                }
                else
                {
                    det1.Discount = 0;
                }

                //if (p.UsePricebreak)
                //{
                //    var breakqtys = p.Pricebreaks.Where(x => x.BreakQty != null && x.BreakQty >= 0).OrderByDescending(x => x.BreakQty).ToList();

                //    foreach (var bq in breakqtys)
                //    {
                //        if (det1.Qty >= bq.BreakQty)
                //        {
                //            decimal price1 = Convert.ToDecimal(bq.DealerPrice);

                //            if (invtype == "CS")
                //            {
                //                price1 = Convert.ToDecimal(bq.RetailPrice);
                //            }
                //            det1.UnitPrice = price1;
                //            det1.DiscountedPrice = price1;
                //            det1.Discount = 0;

                //            break;
                //        }
                //    }

                //};


                det1.PreDiscAmount = det1.UnitPrice * Convert.ToDecimal(qty);
                det1.Amount = det1.DiscountedPrice * Convert.ToDecimal(qty);
                det1.Gst = 0;
                det1.Nett = det1.Amount + det1.Gst;

                det1.IsBundle = p.IsBundle;
                det1.SalesType = "ExchangeItem";
                det1.RefItemID = 0;
                det1.InvRef = invref;
                det1.LocationID = locid;
                det1.LocationName = locname;
                det1.IsControlItem = p.IsControlItem;

                det1.Remark = remark;

                //det1.Position = 0;
                det1.Position = positioncount + 1;

                dets.Add(det1);

                int bundlecount = 0;

                var list = db.Products.Where(x => x.ModelNo.ToUpper().Trim() == p.ModelNo.ToUpper().Trim() && x.IsActive == true).ToList();

                if (!string.IsNullOrEmpty(p.ModelNo))
                {
                    list = db.Products.Where(x => x.ModelNo != null)
                                        .Where(x => x.ModelNo.ToUpper().StartsWith(p.ModelNo.ToUpper()))
                                        .ToList().Distinct().OrderBy(x => x.ProductName).Take(30).ToList();

                }
                else
                {
                    list = db.Products.Where(x => x.ModelNo.ToUpper().Trim() == p.ModelNo.ToUpper().Trim() && x.IsActive == true).ToList();
                }

                foreach (var bb in list)
                {
                    bundlecount++;
                    var det2 = new INVDET();
                    det2.EorID = det1.EorID;
                    det2.EorNo = det1.EorNo;
                    det2.InvType = det1.InvType;
                    det2.ItemID = bb.ProductID;
                    det2.ItemCode = bb.SKU;
                    det2.ItemType = bb.ProductType;
                    det2.ItemName = bb.ProductName;
                    det2.SellType = "CS";
                    det2.Qty = 0;
                    det2.Unit = p.Unit;

                    det2.IsBundle = false;
                    det2.SalesType = "ExchangeItem";
                    det2.RefItemID = det1.DetID;
                    det2.InvRef = invref;
                    det2.LocationID = locid;
                    det2.LocationName = locname;
                    det2.IsControlItem = bb.IsControlItem;
                    det2.Remark = "[REF: " +  p.ProductName + "]";

                    det2.Position = det1.Position + bundlecount;
                  //  det2.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());
                    dets.Add(det2);

                }

                ViewBag.HasFOC = "False";


            }

            return PartialView(dets);
        }

        [HttpPost]
        public ActionResult _AddDetBundle(List<INVDET> list)
        {
            if (list == null)
            {
                return PartialView(list);
            }
            else
            {
                int SorID = list.FirstOrDefault().EorID;

                //bool IsFirst = true;
                //int refid = 0;

                foreach (var det in list)
                {
                    if (det.Qty > 0)
                    {
                        var invdet1 = db.INVDETs.Where(x => x.SorID == SorID).ToList();
                        double positioncount = invdet1.Count;
                        det.Position = positioncount + 1;

                        if (ModelState.IsValid)
                        {
                            db.INVDETs.Add(det);
                            db.SaveChanges();
                        }

                        //AddKivDet(det);
                    }

                };

                UpdateContractAmount(SorID);

                //UpdateKivDets(SorID);

                return RedirectToAction("Edit", new { id = SorID });
            }

        }

        public ActionResult _DelDet(int id = 0)
        {
            ExchangeOrderDet det = db.ExchangeOrderDets.Find(id);
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
                var det = db.ExchangeOrderDets.Find(id);
                if (det != null)
                {
                    db.Entry(det).State = EntityState.Deleted;
                    db.SaveChanges();
                }

                trans.Complete();
            }

            return Json(new { success = true });
        }



        public ActionResult _PreviewOrder(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExchangeOrder inv = db.ExchangeOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();


            ViewBag.InvoiceNo = inv.EorNo;

            return PartialView(inv);
        }

        [HttpGet]
        public JsonResult _SubmitKivOrderPreview(int SorID)
        {
            ExchangeOrder ko = db.ExchangeOrders.Find(SorID);
            ko.Status = "Confirmed";
            ko.ModifiedBy = User.Identity.Name;
            ko.ModifiedOn = System.DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(ko).State = EntityState.Modified;
                db.SaveChanges();
            };

            //UpdateStockQty(invs.InvID, invs.LocationID);
            //UpdateKivQty(ko.KorID, 1);

            return Json(new
            {
                printUrl = Url.Action("ExchangePrintPreview", "Invoice", new { id = ko.EorID }),
                redirectUrl = Url.Action("Create", "Exchange"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _DisplayOrderDetsPreview(int id)
        {
            var p = new List<INVDET>();
            p = db.INVDETs
                .Where(x => (x.EorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        private void UpdateContractAmount(int id)
        {
            //   decimal sumPreDiscAmount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.PreDiscAmount) ?? 0;
            //   decimal sumDiscount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.Discount) ?? 0;
            decimal sumAmount = db.INVDETs.Where(c => c.EorID == id).Sum(c => (decimal?)c.Amount) ?? 0;

            decimal gst = GetGstRate();

            //     decimal sumGst = sumAmount * gst;
            //     decimal sumNett = sumAmount + sumGst;

            ExchangeOrder inv = db.ExchangeOrders.Find(id);
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

        [HttpGet]
        public JsonResult _DelItem(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var det = db.INVDETs.Find(id);
                double position = det.Position;

                int EorID = det.EorID;

                var dets = db.INVDETs.Where(x => x.EorID == det.EorID && x.RefItemID == det.DetID).ToList();
                foreach (var dd in dets)
                {
                    db.Entry(dd).State = EntityState.Deleted;
                }

                //   db.INVDETs.Remove(det);
                db.Entry(det).State = EntityState.Deleted;

                db.SaveChanges();

                //  update sales agreement total amount

                UpdateContractAmount(EorID);


                trans.Complete();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult _ItemMoveUp(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var inv = db.INVDETs.Where(x => x.DetID == id).FirstOrDefault();
                double currentposition = inv.Position;

                if (!inv.IsBundle)
                {
                    var nextinv = db.INVDETs.Where(x => x.EorID == inv.EorID && x.Position == currentposition - 1).FirstOrDefault();
                    if (nextinv != null)
                    {
                        inv.Position = nextinv.Position;

                        nextinv.Position = currentposition;
                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    inv.Position -= 1;
                    if (inv.Position < 1)
                    {
                        inv.Position = 1;
                    }

                    db.Entry(inv).State = EntityState.Modified;
                    db.SaveChanges();

                    if (nextinv.IsBundle)
                    {
                        var bundleinv = db.INVDETs.Where(x => x.EorID == inv.EorID && x.RefItemID == nextinv.DetID).ToList();

                        foreach (var item in bundleinv)
                        {
                            item.Position += 1;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    if (inv.SalesType == "DefaultItem")
                    {
                        var nextinv = db.INVDETs.Where(x => x.EorID == inv.EorID && x.Position == currentposition - 1).FirstOrDefault();
                        var bundleinv = db.INVDETs.Where(x => x.EorID == inv.EorID && x.RefItemID == inv.DetID).ToList();

                        foreach (var item in bundleinv)
                        {
                            item.Position -= 1;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        nextinv.Position += 1;
                        inv.Position -= 1;

                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();

                        if (nextinv.IsBundle)
                        {
                            bundleinv = db.INVDETs.Where(x => x.EorID == inv.EorID && x.RefItemID == nextinv.DetID).ToList();

                            foreach (var item in bundleinv)
                            {
                                item.Position += 1;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                    else if (inv.SalesType == "BundleItem" || inv.SalesType == "FOCItem")
                    {
                        currentposition -= 0.1;
                        var nextinv = db.INVDETs.Where(x => x.EorID == inv.EorID && x.Position == Math.Round(currentposition, 1)).FirstOrDefault();
                        nextinv.Position = Math.Round(nextinv.Position + 0.1, 1);
                        inv.Position = Math.Round(inv.Position - 0.1, 1);

                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                trans.Complete();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }




    }
}