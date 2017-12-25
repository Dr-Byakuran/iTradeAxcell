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
    public class PurchaseOrderController : ControllerBase
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

        // GET: Sales
        public ActionResult Index()
        {

            return View();
        }

        // [ChildActionOnly] 
        public ActionResult _DisplayResults(string invtype)
        {
            DateTime datefrom = DateTime.Now.AddMonths(-12);

            var p = new List<PurchaseOrder>();
            p = db.PurchaseOrders.Where(x => x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.SorID).ToList();


            ViewBag.SalesID = 0;

            var staff = db.Staffs.Where(x => x.Email.Trim() == User.Identity.Name.Trim()).FirstOrDefault();
            if (staff != null)
            {
                ViewBag.StaffID = staff.StaffID;
            }

            return PartialView(p);
        }

        public ActionResult PurchaseInvs()
        {
            return View();
        }

        public ActionResult ViewPI(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PoINV inv = db.PoINVs.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            //ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            //ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            if (inv != null)
            {
                ViewBag.InvoiceNo = inv.InvID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };

            ViewData["seGSTRate"] = GetGstRate();
            return View(inv);
        }

        // [ChildActionOnly] 
        public ActionResult _DisplayInvs(string invtype)
        {
            DateTime datefrom = DateTime.Now.AddMonths(-12);

            var p = new List<PoINV>();

            p = db.PoINVs.Where(x => x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.InvID).ToList();

            return PartialView(p);

        }


        public ActionResult _DisplayOrders(string invtype, string invstatus)
        {
            DateTime datefrom = DateTime.Now.AddMonths(-12);

            var p = new List<PurchaseOrder>();

            ViewBag.TableNo = 0;

            if (invstatus == "Confirmed")
            {
                p = db.PurchaseOrders.Where(x => x.Status == "Invoiced" && x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.SorID).ToList();
                ViewBag.TableNo = 1;
            }
            else
            {
                p = db.PurchaseOrders.Where(x => x.Status == "Pending Approval" && x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.SorID).ToList();

            }


            return PartialView(p);
        }

        public ActionResult _DisplayPickingList(string invtype, string invstatus)
        {
            DateTime datefrom = DateTime.Now.AddMonths(-12);

            var p = new List<PurchaseOrder>();

            ViewBag.TableNo = 0;

            if (invstatus == "New")
            {
                p = db.PurchaseOrders.Where(x => ((x.Status == "Pending Approval") || (x.Status == "Invoiced")) && x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.SorID).ToList();
                ViewBag.TableNo = 1;
            }
            else
            {
                if (invstatus == "Confirmed")
                {
                    p = db.PurchaseOrders.Where(x => x.Status == "Pending Delivery" && x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.SorID).ToList();
                    ViewBag.TableNo = 1;
                }
            }

            //if (invstatus == "Pending Delivery")
            //{
            //    p = db.PurchaseOrders.Where(x => x.Status == "Pending Delivery" && x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.SorID).ToList();
            //    ViewBag.TableNo = 1;
            //}
            //else
            //{
            //    p = db.PurchaseOrders.Where(x => x.Status == "Invoiced" && x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.SorID).ToList();

            //}


            return PartialView(p);
        }

        public ActionResult _DisplayDoList(string invtype, string invstatus)
        {
            DateTime datefrom = DateTime.Now.AddMonths(-12);

            var p = new List<PurchaseOrder>();

            ViewBag.TableNo = 0;

            if (invstatus == "Delivered")
            {
                p = db.PurchaseOrders.Where(x => x.Status == "Delivered" && x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.SorID).ToList();
                ViewBag.TableNo = 1;
            }
            else
            {
                p = db.PurchaseOrders.Where(x => x.Status == "Pending Delivery" && x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.SorID).ToList();

            }


            return PartialView(p);
        }


        // GET: Sales/Details/5
        public ActionResult View(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            //ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            //ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            var item = db.PoINVs.Where(x => x.SorID == id).FirstOrDefault();
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

        public ActionResult CrProcess(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            //ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            //ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            var item = db.PoINVs.Where(x => x.SorID == id).FirstOrDefault();
            if (item != null)
            {
                ViewBag.InvoiceNo = item.InvID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };

            ViewBag.NewInvoiceNo = GetMaxPurchaseInvoiceNumber();

            ViewData["seGSTRate"] = GetGstRate();
            return View(inv);
        }

        public ActionResult Picking(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            //ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            //ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            var item = db.PoINVs.Where(x => x.SorID == id).FirstOrDefault();
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

        public ActionResult DoProcess(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            //ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            //ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            var item = db.PoINVs.Where(x => x.SorID == id).FirstOrDefault();
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
            var p = new PurchaseOrder();

            //var cust = db.Vendors.Where(x => x.AccType == "CS" && x.CustName == "CASH SALES").FirstOrDefault();
            //if (cust != null)
            //{
            //    p.CustNo = cust.CustNo;
            //    p.CustName = cust.CustName;

            //}

            //  p.SarOn.DateCreated = DateTime.Now;
            //  ViewBag.ProductTypes = db.ProductGroups.ToList();

            //   ViewData["ClientsAll"] = GetVendorListByUser("CS");
            //   ViewData["ClientsAll"] = db.Vendors.Where(x => x.AccType == "CS" && x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["seGSTRate"] = GetGstRate();
            ViewData["CurrencyAll"] = db.CurrencySettings.Where(x => x.IsActive == true).OrderBy(x => x.CurrencyName).ToList();

            return View(p);
        }

        // POST: Sales/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SorID,SorNo,QuoID,QuoNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,DeliveryAddress,DeliveryDate,DeliveryTime,PaymentTerms,Status,Remark,PersonID,PersonName")] PurchaseOrder inv)
        {
            string str = GetMaxPurchaseOrderNumber();   
            //  string str = invoice.GetInvoiceNumber(InvType.SO.ToString(), DateTime.Now, User.Identity.Name);

            inv.SorNo = str;
            inv.InvType = "PO";
            inv.CurrencyName = "SGD";
            inv.ExRate = Convert.ToDecimal(1.0);
            inv.PreDiscAmount = 0;
            inv.Discount = 0;
            inv.Amount = 0;
            inv.Gst = 0;
            inv.Nett = 0;
            inv.IsPaid = false;

            if (inv.PersonID == 0)
            {
                inv.PersonName = null;
            }

            inv.CreatedBy = User.Identity.Name;
            inv.CreatedOn = DateTime.Now;

            Boolean flag = false;

            if (ModelState.IsValid)
            {
                db.PurchaseOrders.Add(inv);
                db.SaveChanges();

                flag = true;
            };

            if (flag)
            {
                //CreateKIV(inv.SorID);

                return RedirectToAction("Edit", new { id = inv.SorID, str = "1" });
            }

            ViewBag.Message = "1";

            //    ViewData["ClientsAll"] = GetVendorListByUser("CS");
            //   ViewData["ClientsAll"] = db.Vendors.Where(x => x.AccType == "CS" && x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CurrencyAll"] = db.CurrencySettings.Where(x => x.IsActive == true).OrderBy(x => x.CurrencyName).ToList();
            ViewData["seGSTRate"] = GetGstRate();

            return View(inv);
        }

        private void CreatePoKIV(int SorID)
        {
            var k = new PoKIV();
            k.SorID = SorID;
            k.CreatedBy = User.Identity.Name;
            k.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.PoKIVs.Add(k);
                db.SaveChanges();
            };

        }

        // GET: Sales/Edit/5
        public ActionResult Edit(int? id, string str)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            bool isFound = false;
            var clist = GetVendorListByUser("CS");
            foreach (var c in clist)
            {
                if (c.CustNo == inv.CustNo)
                {
                    isFound = true;
                }
            }

            if (!isFound)
            {
                var client = db.Vendors.Where(x => x.CustNo == inv.CustNo).FirstOrDefault();
                clist.Add(client);
            }

            //   ViewData["ClientsAll"] = clist;
            ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();
            ViewData["seGSTRate"] = GetGstRate();

            var item = db.PoINVs.Where(x => x.SorID == id).FirstOrDefault();
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
        public ActionResult Edit([Bind(Include = "SorID,SorNo,QuoID,QuoNo,DorNo,InvID,InvNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,DeliveryAddress,DeliveryDate,DeliveryTime,Status,CurrencyName,ExRate,PreDiscAmount,Discount,Amount,Gst,Nett,PaymentTerms,Remark,PersonID,PersonName,CreatedOn")] PurchaseOrder inv)
        //public ActionResult Edit([Bind(Include = "SorID,QuoID,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,PostalCode,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PreDiscAmount,Discount,Amount,Gst,Nett,PaymentTerms,Remark,PersonID,PersonName,CreatedOn")] PurchaseOrder inv)
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

                return RedirectToAction("Edit", new { id = inv.SorID });
            }

            bool isFound = false;
            var clist = GetVendorListByUser("CS");
            foreach (var c in clist)
            {
                if (c.CustNo == inv.CustNo)
                {
                    isFound = true;
                }
            }

            if (!isFound)
            {
                var client = db.Vendors.Where(x => x.CustNo == inv.CustNo).FirstOrDefault();
                clist.Add(client);
            }

            //   ViewData["ClientsAll"] = clist;
            ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            return View(inv);
        }


        // GET: Sales/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }
            return View(inv);
        }

        // POST: Sales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedSAR(int id)
        {
            PurchaseOrder inv = db.PurchaseOrders.Find(id);
            db.PurchaseOrders.Remove(inv);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        // GET: Sales/Edit/5
        public ActionResult _OrderDetail(int? id, string act)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder inv = db.PurchaseOrders.Find(id);
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
            var p = new List<PoINVDET>();
            p = db.PoINVDETs
                .Where(x => (x.SorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _DisplayInvDets(List<PoINVDET> list)
        {
            if (list == null)
            {
                return null;
            }

            var sor = list.FirstOrDefault();
            int SorID = sor.SorID;
            string invtype = sor.InvType;

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.PoINVDETs.Find(i.DetID);
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

                    //   UpdateKivDet(det);
                };
                db.SaveChanges();
            };

            UpdateContractAmount(SorID);
            UpdateKivDets(SorID);

            if (invtype == "CS")
            {
                return RedirectToAction("CrEdit", new { id = SorID });
            }
            else
            {
                return RedirectToAction("CrEdit", new { id = SorID });
            }

            //  return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public JsonResult _DisplayInvDetsSave(PoINVDET det)
        {
            if (ModelState.IsValid)
            {
                db.Entry(det).State = EntityState.Modified;
                db.SaveChanges();
            }

            //  update sales agreement total amount

            UpdateContractAmount(det.SorID);

          //  UpdateKivDets(det);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult _DisplayInvDetsPrint(int id)
        {
            var p = new List<PoINVDET>();
            p = db.PoINVDETs
                .Where(x => (x.SorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }


        public ActionResult _DisplayKivDets(int id, string act)
        {
            var p = new List<PoKIVDET>();
            p = db.PoKIVDETs
                .Where(x => (x.SorID == id && x.KorID == 0))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _DisplayKivDets(List<PoKIVDET> list)
        {
            if (list == null)
            {
                return null;
            }

            int SorID = list.FirstOrDefault().SorID;

            var sor = db.PurchaseOrders.Find(SorID);

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.PoKIVDETs.Find(i.DetID);
                    if (det != null)
                    {

                        det.OrderQty = i.OrderQty;
                        det.BalanceQty = i.BalanceQty;
                        det.DeliverQty = i.DeliverQty;
                        det.KivBalanceQty = det.BalanceQty - det.DeliverQty;

                    }

                    //   UpdateKivDet(det);
                };
                db.SaveChanges();
            };

            //    UpdateKivDets(SorID);

            //    return Json(new { success = true });

            if (sor.InvType == "CR")
            {
                return RedirectToAction("CrEdit", new { id = SorID });
            }
            else
            {
                return RedirectToAction("CrEdit", new { id = SorID });
            }

        }


        public ActionResult _DisplayKivDetsPrint(int id)
        {
            var p = new List<PoKIVDET>();
            p = db.PoKIVDETs
                .Where(x => (x.SorID == id && x.KorID == 0))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;


            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _DisplayQuoteDets(List<PoINVDET> list)
        {
            if (ModelState.IsValid)
            {
                var sarid = 0;

                foreach (var i in list)
                {

                    var c = db.PoINVDETs.Where(a => a.SorID.Equals(i.SorID)).FirstOrDefault();

                    if (c != null)
                    {
                        c.ItemName = i.ItemName;
                        c.Qty = i.Qty;
                        c.UnitPrice = i.UnitPrice;
                        c.Amount = i.Amount;

                        sarid = i.SorID;
                    }

                }

                db.SaveChanges();

                ViewBag.Message = "Successfully Updated.";
                ViewBag.QuoteNumber = sarid;

                //  update sales agreement total amount
                UpdateContractAmount(sarid);

                //   return RedirectToAction("Edit", new { id = sarid });

            }

            else
            {

                ViewBag.Message = "Failed ! Please try again.";

            }
            return PartialView(list);

        }


        public ActionResult _AddItem(int id)
        {
            var inv = db.PurchaseOrders.Find(id);

            var p = new PoINVDET();
            p.SorID = inv.SorID;
            p.SorNo = inv.SorNo;
            p.InvType = inv.InvType;

            //   var getList = GetProductList();

            //  ViewData["ClientsAll"] = GetVendorListByUser("CS");
            ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
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
        public ActionResult _AddItem(PoINVDET data)
        {
            var ps = db.Products.Where(x => x.ProductID == data.ItemID).FirstOrDefault();

            //decimal costprice = ps.CostPrice;
            //string costcode = Decimal2String(costprice);
            //data.Remark = costcode;

            data.Nett = data.Amount + data.Gst;

            var invdet1 = db.PoINVDETs.Where(x => x.SorID == data.SorID).ToList();
            double positioncount = invdet1.Count;
            data.Position = positioncount + 1;

            if (ModelState.IsValid)
            {
                db.PoINVDETs.Add(data);
                db.SaveChanges();
            };

            UpdateKivDets(data.SorID);
            UpdateContractAmount(data.SorID);

            //   AddKivDet(data);

            int bundlecount = 0;



            var totalAmount = db.PurchaseOrders.Where(x => x.SorID == data.SorID).FirstOrDefault().Nett;
            var detCount = db.PoINVDETs.Count(x => x.SorID == data.SorID);

            ViewBag.Message = "1";
            var poptions = new List<PriceOption>();
            ViewData["PriceOptionsAll"] = poptions;
            ViewData["seGSTRate"] = GetGstRate();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return Json(new { success = true, totalamount = totalAmount, detcount = detCount, redirectUrl = Url.Action("CrEdit", "Orders", new { id = data.SorID, str = "1" }) });

        }


        public ActionResult _AddDet(int id)
        {
            var p = new PoINVDET();
            p.SorID = id;

            //   var getList = GetProductList();

            ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //   ViewData["ClientsAll"] = GetVendorListByUser("CS");
            ViewData["seGSTRate"] = GetGstRate();
            //     ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //  ViewData["ProductData"] = getList;

            return PartialView(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddDet(PoINVDET data)
        {
            var ps = db.Products.Where(x => x.ProductID == data.ItemID).FirstOrDefault();

            decimal costprice = ps.CostPrice;
            string costcode = Decimal2String(costprice);
            data.Remark = costcode;

            data.Nett = data.Amount + data.Gst;

            var invdet1 = db.PoINVDETs.Where(x => x.RefItemID == 0 && x.SorID == data.SorID).ToList();
            double positioncount = invdet1.Count;
            data.Position = positioncount + 1;

            if (ModelState.IsValid)
            {
                db.PoINVDETs.Add(data);
                db.SaveChanges();
            };

            UpdateKivDets(data.SorID);

            //   AddKivDet(data);

            int bundlecount = 0;

            foreach (var pb in ps.Productbundles)
            {
                var p = db.Products.Where(x => x.SKU == pb.IncSKU).FirstOrDefault();

                if (p != null)
                {
                    bundlecount++;
                    var det = new PoINVDET();
                    det.SorID = data.SorID;
                    det.InvID = data.InvID;
                    det.ItemID = data.ItemID;
                    det.ItemCode = pb.IncSKU;
                    det.ItemType = data.ItemType;
                    det.ItemName = pb.IncProductName;
                    det.Qty = Convert.ToDouble(pb.IncQty);
                    det.Unit = p.Unit;
                    det.Remark = "Bundle Item";
                    det.IsControlItem = p.IsControlItem;
                    det.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());

                    if (ModelState.IsValid)
                    {
                        db.PoINVDETs.Add(det);
                        db.SaveChanges();

                    };
                    UpdateKivDets(det.SorID);
                    //  AddKivDet(det);
                }

            }


            UpdateContractAmount(data.SorID);

            ViewBag.Message = "1";

            return Json(new { redirectUrl = Url.Action("CrEdit", "Orders", new { id = data.SorID }) });

        }

        private void UpdateKivDets(int SorID)
        {
            var list = db.PoINVDETs.Where(x => x.SorID == SorID).ToList();
            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.PoKIVDETs.Where(x => x.InvDetID == i.DetID).FirstOrDefault();
                    if (det != null)
                    {
                        det.InvDetID = i.DetID;
                        det.KivID = 0;
                        det.KorID = 0;
                        det.SorID = i.SorID;
                        det.InvID = i.InvID;
                        det.ItemID = i.ItemID;
                        det.ItemCode = i.ItemCode;
                        det.ItemName = i.ItemName;
                        det.Unit = i.Unit;
                        det.OrderQty = i.Qty;
                        det.BalanceQty = i.Qty;
                        det.DeliverQty = 0.00;
                        det.KivBalanceQty = i.Qty;
                        det.SalesType = i.SalesType;
                        det.Position = i.Position;
                        det.Remark = i.Remark;

                        db.Entry(det).State = EntityState.Modified;

                    }
                    else
                    {
                        det = new PoKIVDET();
                        det.InvDetID = i.DetID;
                        det.KivID = 0;
                        det.KorID = 0;
                        det.SorID = i.SorID;
                        det.InvID = i.InvID;
                        det.ItemID = i.ItemID;
                        det.ItemCode = i.ItemCode;
                        det.ItemName = i.ItemName;
                        det.Unit = i.Unit;
                        det.OrderQty = i.Qty;
                        det.BalanceQty = i.Qty;
                        det.DeliverQty = 0.00;
                        det.KivBalanceQty = i.Qty;
                        det.SalesType = i.SalesType;
                        det.Position = i.Position;
                        det.Remark = i.Remark;

                        db.Entry(det).State = EntityState.Added;

                    }
                };
                db.SaveChanges();
            };

            var kivs = db.PoKIVDETs.Where(x => x.SorID == SorID).ToList();
            foreach (var kiv in kivs)
            {
                bool contains = db.PoINVDETs.Any(x => x.SorID == kiv.SorID && x.DetID == kiv.InvDetID);

                if (!contains)
                {
                    var k = db.PoKIVDETs.Find(kiv.DetID);
                    if (k != null)
                    {
                        db.PoKIVDETs.Remove(k);
                        db.SaveChanges();
                    }

                }

            }

        }


        public ActionResult _AddDetBundle(int id, double qty, decimal unitprice, decimal discprice, int SorID, string selltype, string remark)
        {
            var sor = db.PurchaseOrders.Where(x => x.SorID == SorID).FirstOrDefault();

            if (sor == null)
            {
                return null;
            }

            var invtype = sor.InvType;


            var dets = new List<PoINVDET>();
            var p = db.Products.Find(id);

            if (p != null)
            {
                var invdet1 = db.PoINVDETs.Where(x => x.SorID == sor.SorID).ToList();
                double positioncount = invdet1.Count;
                var det1 = new PoINVDET();
                det1.SorID = sor.SorID;
                det1.SorNo = sor.SorNo;
                det1.InvID = sor.InvID;
                det1.InvNo = sor.InvNo;
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
                det1.SalesType = "Bundle";
                det1.RefItemID = 0;
                det1.InvRef = "";
                det1.IsControlItem = p.IsControlItem;

                det1.Remark = remark;
                det1.Position = positioncount + 1;

                dets.Add(det1);

                int bundlecount = 0;

                foreach (var bb in p.Productbundles)
                {
                    bundlecount++;
                    var det2 = new PoINVDET();
                    det2.SorID = det1.SorID;
                    det2.SorNo = det1.SorNo;
                    det2.InvID = det1.InvID;
                    det2.InvNo = det1.InvNo;
                    det2.InvType = det1.InvType;
                    det2.ItemID = bb.IncProductID;
                    det2.ItemCode = bb.IncSKU;
                    det2.ItemType = bb.IncProductType;
                    det2.ItemName = bb.IncProductName;
                    det2.SellType = det1.SellType;
                    det2.Qty = bb.IncQty * qty;
                    det2.Unit = p.Unit;

                    det2.IsBundle = false;
                    det2.SalesType = "BundleItem";
                    det2.RefItemID = det1.DetID;
                    det2.InvRef = "";
                    det2.IsControlItem = bb.IsControlItem;
                    det2.Remark = "";

                    det2.Position = det1.Position + bundlecount;
                    //  det2.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());
                    dets.Add(det2);

                }

                ViewBag.HasFOC = "False";

                if (p.ProductFOCs.Count > 0)
                {
                    ViewBag.HasFOC = "True";

                    double focqty = 0.00;

                    if (p.UsePricebreak)
                    {
                        var breakqtys = p.Pricebreaks.Where(x => x.BreakQty >= 0).OrderByDescending(x => x.BreakQty).ToList();

                        foreach (var bq in breakqtys)
                        {
                            if (det1.Qty >= bq.BreakQty)
                            {
                                if (!string.IsNullOrEmpty(bq.FocQty.ToString()))
                                {
                                    focqty = Convert.ToDouble(bq.FocQty);
                                };
                                break;
                            }
                        }

                    };

                    foreach (var bb in p.ProductFOCs)
                    {
                        bundlecount++;
                        var det2 = new PoINVDET();
                        det2.SorID = det1.SorID;
                        det2.SorNo = det1.SorNo;
                        det2.InvID = det1.InvID;
                        det2.InvNo = det1.InvNo;
                        det2.InvType = det1.InvType;
                        det2.ItemID = bb.IncProductID;
                        det2.ItemCode = bb.IncSKU;
                        det2.ItemType = bb.IncProductType;
                        det2.ItemName = bb.IncProductName;
                        det2.SellType = det1.SellType;
                        det2.Qty = bb.IncQty * qty;
                        det2.Unit = p.Unit;

                        det2.IsBundle = false;
                        det2.SalesType = "FOCItem";
                        det2.RefItemID = det1.DetID;
                        det2.InvRef = "";
                        det2.IsControlItem = bb.IsControlItem;
                        det2.Remark = "";

                        det2.Position = det1.Position + bundlecount;
                        //  det2.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());
                        dets.Add(det2);

                    }
                }

            }

            return PartialView(dets);
        }

        [HttpPost]
        public ActionResult _AddDetBundle(List<PoINVDET> list)
        {
            if (list == null)
            {
                return PartialView(list);
            }
            else
            {
                int SorID = list.FirstOrDefault().SorID;

                //bool IsFirst = true;
                //int refid = 0;

                foreach (var det in list)
                {
                    if (det.Qty > 0)
                    {
                        var invdet1 = db.PoINVDETs.Where(x => x.SorID == SorID).ToList();
                        double positioncount = invdet1.Count;
                        det.Position = positioncount + 1;

                        if (ModelState.IsValid)
                        {
                            db.PoINVDETs.Add(det);
                            db.SaveChanges();
                        }

                        AddKivDet(det);
                    }

                };
                UpdateContractAmount(SorID);

                UpdateKivDets(SorID);

                return RedirectToAction("CrEdit", new { id = SorID });
            }

        }

        public ActionResult _EditDetBundle(int id)
        {
            var det = db.PoINVDETs.Find(id);
            if (det == null)
            {
                return HttpNotFound();
            }

            var dets = new List<PoINVDET>();
            dets.Add(det);

            var p = db.Products.Find(det.ItemID);

            if (p != null)
            {
                foreach (var bb in p.Productbundles)
                {
                    var det2 = new PoINVDET();
                    det2.SorID = det.SorID;
                    det2.ItemID = bb.IncProductID;
                    det2.ItemCode = bb.IncSKU;
                    det2.ItemType = bb.IncProductType;
                    det2.ItemName = bb.IncProductName;
                    det2.Qty = Convert.ToDouble(0);
                    det2.Unit = "";

                    det2.IsControlItem = bb.IsControlItem;
                    det2.IsBundle = p.IsBundle;
                    det2.SalesType = "BundleItem";
                    det2.RefItemID = det.DetID;
                    det2.Position = bb.Position;

                    var tmp = db.PoINVDETs.Where(x => x.SorID == det.SorID && x.ItemCode == bb.IncSKU && x.RefItemID == det.DetID && x.SalesType == "BundleItem").FirstOrDefault();
                    if (tmp != null)
                    {
                        det2.DetID = tmp.DetID;
                        det2.Qty = tmp.Qty;
                        det2.Unit = tmp.Unit;
                        det2.Remark = tmp.Remark;

                        //    det2.SalesType = tmp.SalesType;
                    };

                    dets.Add(det2);

                }

                ViewBag.HasFOC = "False";

                if (p.ProductFOCs.Count > 0)
                {
                    ViewBag.HasFOC = "True";
                    foreach (var bb in p.ProductFOCs)
                    {
                        var det2 = new PoINVDET();
                        det2.SorID = det.SorID;
                        det2.ItemID = bb.IncProductID;
                        det2.ItemCode = bb.IncSKU;
                        det2.ItemType = bb.IncProductType;
                        det2.ItemName = bb.IncProductName;
                        det2.Qty = Convert.ToDouble(0);
                        det2.Unit = "";

                        det2.IsControlItem = bb.IsControlItem;
                        det2.IsBundle = p.IsBundle;
                        det2.SalesType = "FOCItem";
                        det2.RefItemID = det.DetID;
                        det2.Position = bb.Position;

                        var tmp = db.PoINVDETs.Where(x => x.SorID == det.SorID && x.ItemCode == bb.IncSKU && x.RefItemID == det.DetID && x.SalesType == "FOCItem").FirstOrDefault();
                        if (tmp != null)
                        {
                            det2.DetID = tmp.DetID;
                            det2.Qty = tmp.Qty;
                            det2.Unit = tmp.Unit;
                            det2.Remark = tmp.Remark;
                            //   det2.SalesType = tmp.SalesType;
                        };

                        dets.Add(det2);

                    }
                }



            }

            return PartialView(dets);
        }

        [HttpPost]
        public ActionResult _EditDetBundle(List<PoINVDET> list)
        {
            int SorID = list.FirstOrDefault().SorID;
            int refid = list.FirstOrDefault().DetID;
            var item1 = list.FirstOrDefault();

            var det0 = db.PoINVDETs.Find(refid);
            if (det0 != null)
            {
                det0.Qty = item1.Qty;
                det0.DiscountedPrice = item1.DiscountedPrice;
                det0.Amount = item1.DiscountedPrice * Convert.ToDecimal(item1.Qty);
                det0.Gst = item1.Gst;
                det0.Nett = item1.Amount + item1.Gst;

                if (ModelState.IsValid)
                {
                    db.Entry(det0).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            if (ModelState.IsValid)
            {
                bool IsFirst = true;
                foreach (PoINVDET det in list)
                {
                    if (IsFirst)
                    {
                        IsFirst = false;
                    }
                    else
                    {
                        var det1 = db.PoINVDETs.Find(det.DetID);

                        if (det1 != null)
                        {
                            det1.Qty = det.Qty;
                            det1.DiscountedPrice = det.DiscountedPrice;
                            det1.Amount = det.DiscountedPrice * Convert.ToDecimal(det.Qty);
                            det1.Gst = det.Gst;
                            det1.Nett = det.Amount + det.Gst;

                        }
                        else
                        {
                            det1 = new PoINVDET();
                            det1.SorID = SorID;
                            det1.InvID = det.InvID;
                            det1.ItemID = det.ItemID;
                            det1.ItemCode = det.ItemCode;
                            det1.ItemType = det.ItemType;
                            det1.ItemName = det.ItemName;
                            det1.Qty = Convert.ToDouble(det.Qty);
                            det1.Unit = det.Unit;
                            det1.Remark = det.Remark;
                            det1.IsControlItem = det.IsControlItem;
                            det1.IsBundle = det.IsBundle;
                            det1.SalesType = det.SalesType;
                            det1.RefItemID = refid;

                            var detfirst = db.PoINVDETs.Where(x => x.DetID == refid && x.SorID == SorID).FirstOrDefault();
                            var detlist = db.PoINVDETs.Where(x => x.RefItemID == refid && x.SorID == SorID).ToList();

                            //det1.Position = Convert.ToDouble(det.Position);
                            det1.Position = Convert.ToDouble(detfirst.Position + "." + (detlist.Count + 1));
                            det1.UnitPrice = det.UnitPrice;
                            det1.DiscountedPrice = det.DiscountedPrice;
                            det1.PreDiscAmount = det.PreDiscAmount;
                            det1.Discount = det.Discount;
                            det1.Amount = det.Amount;
                            det1.Gst = det.Gst;
                            det1.Nett = det.Nett;

                            db.Entry(det1).State = EntityState.Added;

                        }
                    }

                };
                db.SaveChanges();
            };

            //   UpdateKivDet(det);

            UpdateContractAmount(SorID);
            UpdateKivDets(SorID);

            return Json(new { success = true });

        }




        //   [HttpPost]
        public JsonResult _AddBundleProductSave(InvdetBundle ib)
        {
            var det1 = new PoINVDET();
            det1.SorID = ib.InvDetOn.SorID;
            det1.InvID = ib.InvDetOn.InvID;
            det1.ItemID = ib.InvDetOn.ItemID;
            det1.ItemCode = ib.InvDetOn.ItemCode;
            det1.ItemType = ib.InvDetOn.ItemType;
            det1.ItemName = ib.InvDetOn.ItemName;
            det1.Qty = ib.InvDetOn.Qty;
            det1.Unit = ib.InvDetOn.Unit;
            det1.Remark = "Pkg item";
            det1.RefItemID = ib.InvDetOn.ItemID;
            det1.IsControlItem = ib.InvDetOn.IsControlItem;

            det1.Nett = ib.InvDetOn.Amount + ib.InvDetOn.Gst;

            if (ModelState.IsValid)
            {
                db.PoINVDETs.Add(det1);
                db.SaveChanges();
            };

            int refid = det1.DetID;

            AddKivDet(det1);

            var ps = new Product();

            ps = ib.ProductOn;

            foreach (var pb in ps.Productbundles)
            {
                var p = db.Products.Where(x => x.SKU == pb.IncSKU).FirstOrDefault();

                if (p != null)
                {
                    var det = new PoINVDET();
                    det.SorID = det1.SorID;
                    det.InvID = det1.InvID;
                    det.ItemID = det1.ItemID;
                    det.ItemCode = pb.IncSKU;
                    det.ItemType = det1.ItemType;
                    det.ItemName = pb.IncProductName;
                    det.Qty = Convert.ToDouble(pb.IncQty);
                    det.Unit = p.Unit;
                    det.Remark = "(Pkg item)";
                    det.RefItemID = ps.ProductID;
                    det.IsControlItem = p.IsControlItem;

                    if (ModelState.IsValid)
                    {
                        db.PoINVDETs.Add(det);
                        db.SaveChanges();

                    };
                    AddKivDet(det);
                }

            }


            UpdateContractAmount(det1.SorID);


            //  return PartialView(ib);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }


        public ActionResult _DisplayBundleItems(int id)
        {

            var p = db.Products.Find(id);

            if (p.IsBundle)
            {
                return PartialView(p.Productbundles.ToList());
            }
            else
            {
                return PartialView(null);
            }
        }

        //  [HttpGet]
        public JsonResult _AddBundleItem(List<Productbundle> list, int SorID, int InvID)
        {
            var p = db.Products.Where(x => x.Productbundles.FirstOrDefault().BunleID == list.FirstOrDefault().BunleID).FirstOrDefault();

            foreach (var pb in list)
            {
                var det = new PoINVDET();
                det.SorID = SorID;
                det.InvID = InvID;
                det.ItemID = pb.IncProductID;
                det.ItemCode = pb.IncSKU;
                //    det.ItemType = data.ItemType;
                det.ItemName = pb.IncProductName;
                det.Qty = Convert.ToDouble(pb.IncQty);
                det.Unit = p.Unit;
                det.Remark = "Bundle Item";
                det.IsControlItem = p.IsControlItem;

                if (ModelState.IsValid)
                {
                    db.PoINVDETs.Add(det);
                    db.SaveChanges();

                };
                AddKivDet(det);

            };



            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }



        private void AddKivDet(PoINVDET data)
        {
            var det = new PoKIVDET();
            det.KivID = 0;
            det.KorID = 0;
            det.InvDetID = data.DetID;
            det.SorID = data.SorID;
            det.InvID = data.InvID;
            det.ItemID = data.ItemID;
            det.ItemCode = data.ItemCode;
            det.ItemName = data.ItemName;
            det.Unit = data.Unit;
            det.OrderQty = data.Qty;
            det.BalanceQty = data.Qty;
            det.DeliverQty = data.Qty;
            det.Position = data.Position;
            det.KivBalanceQty = 0.00;
            det.SalesType = data.SalesType;
            det.Remark = data.Remark;

            if (ModelState.IsValid)
            {
                db.PoKIVDETs.Add(det);
                db.SaveChanges();

            };

        }


        public ActionResult _EditDet(int id)
        {
            var p = db.INVDETs.Find(id);

            if (p == null)
            {
                return HttpNotFound();
            }

            var product = db.Products.Where(x => x.SKU == p.ItemCode).FirstOrDefault();
            var productdata = GetProductList();

            var getList = productdata;

            if (!string.IsNullOrEmpty(product.ModelNo))
            {
                getList = productdata.Where(x => x.ModelNo != null)
                                    .Where(x => x.ModelNo.ToUpper().StartsWith(product.ModelNo.ToUpper()))
                                    .ToList().Distinct().OrderBy(x => x.ProductName).Take(30).ToList();

                if (getList == null)
                {
                    getList = productdata.Where(x => x.SKU == p.ItemCode)
                                        .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
                }

            }
            else
            {
                getList = productdata.Where(x => x.SKU == p.ItemCode)
                                    .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
            }




            ViewData["ProductChangeList"] = getList;
            ViewData["seGSTRate"] = GetGstRate();

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _EditDet(INVDET data)
        {
            Convert.ToDouble(data.Qty);
            Convert.ToDecimal(data.Discount);
            Convert.ToDecimal(data.DiscountedPrice);
            Convert.ToDecimal(data.PreDiscAmount);
            Convert.ToDecimal(data.UnitPrice);
            Convert.ToDecimal(data.Amount);
            Convert.ToDecimal(data.Gst);
            Convert.ToDecimal(data.Nett);

            if (ModelState.IsValid)
            {
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }

            //  update sales agreement total amount

            UpdateContractAmount(data.SorID);

            UpdateKivDets(data);

            var product = db.Products.Where(x => x.SKU == data.ItemCode).FirstOrDefault();
            var productdata = GetProductList();

            var getList = productdata;

            if (!string.IsNullOrEmpty(product.ModelNo))
            {
                getList = productdata.Where(x => x.ModelNo != null)
                                    .Where(x => x.ModelNo.ToUpper().StartsWith(product.ModelNo.ToUpper()))
                                    .ToList().Distinct().OrderBy(x => x.ProductName).Take(30).ToList();

                if (getList == null)
                {
                    getList = productdata.Where(x => x.SKU == data.ItemCode)
                                        .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
                }

            }
            else
            {
                getList = productdata.Where(x => x.SKU == data.ItemCode)
                                    .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
            }

            ViewData["ProductChangeList"] = getList;

            return Json(new { success = true });
        }

        private void UpdateKivDets(INVDET data)
        {
            var det = db.PoKIVDETs.Where(x => x.SorID == data.SorID && x.InvDetID == data.DetID).FirstOrDefault();

            if (det != null)
            {
                det.OrderQty = data.Qty;
                det.BalanceQty = data.Qty;
                det.DeliverQty = data.Qty;
                det.KivBalanceQty = 0;

                if (ModelState.IsValid)
                {
                    db.Entry(det).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }


        }



        //  [HttpGet]
        public JsonResult _AddOverallDiscount(string valInvID, string valDiscount, string valAmount, string valGst, string valNett)
        {
            int id = Convert.ToInt32(valInvID);
            decimal discount = Convert.ToDecimal(valDiscount);
            decimal amount = Convert.ToDecimal(valAmount);
            decimal gst = Convert.ToDecimal(valGst);
            decimal nett = amount + gst;

            var inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                return null;
            };
            inv.Discount = discount;
            inv.Amount = amount;
            inv.Gst = gst;
            inv.Nett = nett;

            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        //  [HttpGet]
        public JsonResult _ChangeGST(string valSorID, string valZeroRated)
        {
            int id = Convert.ToInt32(valSorID);
            string isZeroRated = valZeroRated;
            decimal gstrate = GetGstRate();

            var inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                return null;
            };

            if (isZeroRated == "Yes")
            {
                inv.Gst = 0;
                inv.Nett = inv.Amount;
            }
            else
            {
                inv.Gst = System.Math.Round(inv.Amount * gstrate, 2, MidpointRounding.AwayFromZero);
                inv.Nett = inv.Amount + inv.Gst;
            }



            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }


        public ActionResult _PreviewOrder(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            var salespay = db.SalesPaymentMethods.Where(x => x.SorID == id).ToList();

            decimal sPaidAmount = 0;
            foreach (var s in salespay)
            {
                sPaidAmount += s.Amount;
            }

            ViewData["seSalesPaymentAmount"] = sPaidAmount;
            var item = db.PoINVs.Where(x => x.SorID == id).FirstOrDefault();
            if (item != null)
            {
                ViewBag.InvoiceNo = item.InvID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };
            ViewData["seGSTRate"] = GetGstRate();

            return PartialView(inv);
        }



        public ActionResult OrderProcessed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            var salespay = db.SalesPaymentMethods.Where(x => x.SorID == id).ToList();

            decimal sPaidAmount = 0;
            foreach (var s in salespay)
            {
                sPaidAmount += s.Amount;
            }

            ViewData["seSalesPaymentAmount"] = sPaidAmount;
            var item = db.PoINVs.Where(x => x.SorID == id).FirstOrDefault();
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


        //  [HttpGet]
        public JsonResult _ConvertToInvoice(string valSorID)
        {
            int id = Convert.ToInt32(valSorID);

            var sor = db.PurchaseOrders.Find(id);
            if (sor == null)
            {
                return null;
            };

            // *** creating new invoice

            PoINV inv = new PoINV();
            inv.SorID = sor.SorID;
            inv.InvType = sor.InvType;
            inv.InvDate = sor.InvDate;
            inv.PoNo = sor.PoNo;
            inv.InvRef = sor.InvRef;
            inv.CustNo = sor.CustNo;
            inv.CustName = sor.CustName;
            inv.CustName2 = sor.CustName2;
            inv.Addr1 = sor.Addr1;
            inv.Addr2 = sor.Addr2;
            inv.Addr3 = sor.Addr3;
            inv.Attn = sor.Attn;
            inv.DeliveryAddress = sor.DeliveryAddress;
            inv.CurrencyName = sor.CurrencyName;
            inv.ExRate = sor.ExRate;
            inv.PreDiscAmount = sor.PreDiscAmount;
            inv.Discount = sor.Discount;
            inv.Amount = sor.Amount;
            inv.Gst = sor.Gst;
            inv.Nett = sor.Nett;
            inv.Status = "Confirmed";
            inv.PaymentTerms = sor.PaymentTerms;
            inv.Remark = sor.Remark;
            inv.PersonID = sor.PersonID;
            inv.PersonName = sor.PersonName;
            inv.IsPaid = false;
            inv.CreatedBy = User.Identity.Name;
            inv.CreatedOn = DateTime.Now;


            if (ModelState.IsValid)
            {
                db.PoINVs.Add(inv);
                db.SaveChanges();
            }

            sor.Status = "Invoiced";
            sor.InvID = inv.InvID;

            if (ModelState.IsValid)
            {
                db.Entry(sor).State = EntityState.Modified;
                db.SaveChanges();
            };

            UpdateInvDetWithInvID(inv.SorID);
            UpdateKivDetWithInvID(inv.SorID);

            return Json(new
            {
                redirectUrl = Url.Action("OrderProcessed", "Orders", new { id = sor.SorID }),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);

        }

        private void UpdateInvDetWithInvID(int SorID)
        {
            var inv = db.PoINVs.Where(x => x.SorID == SorID).FirstOrDefault();

            if (inv != null)
            {
                int newInvID = inv.InvID;

                var dets = db.PoINVDETs.Where(x => x.SorID == SorID).ToList();
                foreach (var det in dets)
                {
                    det.InvID = newInvID;

                    if (ModelState.IsValid)
                    {
                        db.Entry(det).State = EntityState.Modified;
                        db.SaveChanges();
                    }


                }
            }
        }

        private void UpdateKivDetWithInvID(int SorID)
        {
            var inv = db.PoINVs.Where(x => x.SorID == SorID).FirstOrDefault();

            if (inv != null)
            {
                int newInvID = inv.InvID;

                var dets = db.PoKIVDETs.Where(x => x.SorID == SorID).ToList();
                foreach (var det in dets)
                {
                    det.InvID = newInvID;

                    if (ModelState.IsValid)
                    {
                        db.Entry(det).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }

        // Sales Items

        public ActionResult AddSalesItemEdit(int id)
        {
            PoINVDET det = new PoINVDET();

            PurchaseOrder inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                inv.SorID = 0;
            }
            else
            {
                det.SorID = inv.SorID;
            };


            return PartialView(det);
        }

        [HttpPost]
        public ActionResult AddSalesItemEdit(PoINVDET det)
        {
            if (ModelState.IsValid)
            {
                db.Entry(det).State = EntityState.Modified;
                db.PoINVDETs.Add(det);
                db.SaveChanges();

                // update total sales amount

                UpdateContractAmount(det.SorID);

                return RedirectToAction("CrEdit", new { id = det.SorID });
            }
            return View(det);
        }



        public ActionResult AddSalesItem(int id)
        {
            PoINVDET det = new PoINVDET();

            PurchaseOrder inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                inv.SorID = 0;
            }
            else
            {
                det.SorID = inv.SorID;
            };


            return PartialView(det);
        }

        [HttpPost]
        public ActionResult AddSalesItem(PoINVDET det)
        {
            if (ModelState.IsValid)
            {
                db.Entry(det).State = EntityState.Modified;
                db.PoINVDETs.Add(det);
                db.SaveChanges();

                return RedirectToAction("CrEdit", new { id = det.SorID });
            }
            return View(det);
        }



        [HttpPost]
        public ActionResult _UpdateSalesItem([Bind(Include = "SorID,Qty,UnitPrice,Amount")] PoINVDET det)
        {

            if (ModelState.IsValid)
            {
                db.Entry(det).State = EntityState.Modified;
                db.SaveChanges();

                UpdateContractAmount(det.SorID);

                return RedirectToAction("Manage", new { id = det.SorID });
            };

            ViewBag.Message = "Not updated.";

            return PartialView(det);
        }



        private void UpdateContractAmount(int id)
        {
            //   decimal sumPreDiscAmount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.PreDiscAmount) ?? 0;
            //   decimal sumDiscount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.Discount) ?? 0;
            decimal sumAmount = db.PoINVDETs.Where(c => c.SorID == id).Sum(c => (decimal?)c.Amount) ?? 0;

            decimal gst = GetGstRate();

            //     decimal sumGst = sumAmount * gst;
            //     decimal sumNett = sumAmount + sumGst;

            PurchaseOrder inv = db.PurchaseOrders.Find(id);
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
                var det = db.PoINVDETs.Find(id);
                double position = det.Position;

                int SorID = det.SorID;

                var dets = db.PoINVDETs.Where(x => x.SorID == det.SorID && x.RefItemID == det.DetID).ToList();
                foreach (var dd in dets)
                {
                    db.Entry(dd).State = EntityState.Deleted;
                }

                //   db.PoINVDETs.Remove(det);
                db.Entry(det).State = EntityState.Deleted;

                db.SaveChanges();

                //  update sales agreement total amount

                UpdateContractAmount(SorID);

                UpdateKivDets(SorID);

                var detlist = db.PoINVDETs.Where(x => x.SorID == SorID && x.Position > position).ToList();

                foreach (var item in detlist)
                {
                    var bundleitem = db.PoKIVDETs.Where(y => y.SorID == SorID && y.InvDetID == item.DetID).FirstOrDefault();
                    item.Position -= 1;
                    bundleitem.Position -= 1;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(bundleitem).State = EntityState.Modified;
                    db.SaveChanges();
                }

                trans.Complete();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }



        public ActionResult _DelDet(int id = 0)
        {
            PoINVDET det = db.PoINVDETs.Find(id);
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
                var det = db.PoINVDETs.Find(id);
                double position = det.Position;

                int SorID = det.SorID;

                var dets = db.PoINVDETs.Where(x => x.SorID == det.SorID && x.RefItemID == det.DetID).ToList();
                foreach (var dd in dets)
                {
                    db.Entry(dd).State = EntityState.Deleted;
                }

                //   db.PoINVDETs.Remove(det);
                db.Entry(det).State = EntityState.Deleted;

                db.SaveChanges();

                //  update sales agreement total amount

                UpdateContractAmount(SorID);

                UpdateKivDets(SorID);

                var detlist = db.PoINVDETs.Where(x => x.SorID == SorID && x.Position > position).ToList();

                foreach (var item in detlist)
                {
                    var bundleitem = db.PoKIVDETs.Where(y => y.SorID == SorID && y.InvDetID == item.DetID).FirstOrDefault();
                    item.Position -= 1;
                    bundleitem.Position -= 1;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(bundleitem).State = EntityState.Modified;
                    db.SaveChanges();
                }

                trans.Complete();
            }

            return Json(new { success = true });
        }


        public ActionResult _Summary(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            else
            {
                var inv = db.PurchaseOrders.Find(id);
                var det = new List<PoINVDET>();
                det = db.PoINVDETs
                .Where(x => (x.SorID == id))
                .OrderBy(x => x.DetID)
                .ToList();

                var qv = new PurchaseOrderView
                {
                    SalesOrderOn = inv,
                    InvDetOn = det
                };

                ViewBag.InvNumber = id;
                ViewData["seGSTRate"] = GetGstRate();


                return PartialView(qv);
            }

        }



        public ActionResult InvPrint(int id)
        {
            PurchaseOrder sar = db.PurchaseOrders.Find(id);
            ViewData["seGSTRate"] = GetGstRate();

            return View(sar);
        }

        public ActionResult MultiPagesPrint(int id)
        {
            PurchaseOrder sar = db.PurchaseOrders.Find(id);

            var count = db.PoINVDETs.Count(x => x.SorID == id);

            ViewBag.ItemCount = count;
            ViewData["seGSTRate"] = GetGstRate();

            return View(sar);
        }


        public ActionResult _DisplaySalesItems(int id, int pageid)
        {
            var p = new List<PoINVDET>();
            p = db.PoINVDETs
                .Where(x => (x.SorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.PageID = pageid;

            return PartialView(p);
        }


        //public List<ProductSelection> GetProductList()
        //{
        //    List<ProductSelection> getList = (from p in db.Products
        //                                      where p.IsActive == true
        //                                      select new ProductSelection
        //                                      {
        //                                          ProductID = p.ProductID,
        //                                          SKU = p.SKU,
        //                                          Barcode = p.Barcode,
        //                                          ProductType = p.ProductType,
        //                                          ProductName = p.ProductName,
        //                                          ModelNo = p.ModelNo,
        //                                          IsBundle = p.IsBundle,
        //                                          Unit = p.Unit,
        //                                          CostPrice = p.CostPrice,
        //                                          SellPrice = p.RetailPrice,
        //                                          IsControlItem = p.IsControlItem,
        //                                          AvailableQty = 0

        //                                      }).ToList();

        //    return getList;

        //}



        //public JsonResult AutoComplete(string search)
        //{
        //    var data = db.Vendors
        //               .Where(x => ((x.CustName.ToUpper().StartsWith(search.ToUpper())) || (x.CustNo.ToString().StartsWith(search))) && ((x.IsActive == true)))
        //               .ToList().Distinct().ToList();

        //    //   var result = data.Where(x => x.HeatNo.ToLower().StartsWith(search.ToLower())).ToList();

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}

        ////  [HttpPost]
        //public JsonResult AutoCompleteSelected(string search)
        //{
        //    if (search != null)
        //    {
        //        int custno = Convert.ToInt32(search);
        //        var c = db.Vendors
        //                   .Where(x => x.CustNo == custno)
        //                   .ToList().FirstOrDefault();

        //        if (c != null)
        //        {

        //            return Json(new { result = c }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return null;
        //        };

        //    }
        //    else
        //    {
        //        return null;
        //    }

        //}

        //[HttpGet]
        //public JsonResult AutoComplete_Product(string search)
        //{
        //    var getList = GetProductList();
        //    var data = getList;

        //    string[] words = search.ToUpper().Split(' ').ToArray();

        //    var result = from p in getList
        //                 let w = p.ProductName.ToUpper().Split(new char[] { '.', '-', '!', ' ', ';', ':', ',' },
        //                                            StringSplitOptions.RemoveEmptyEntries)
        //                 where (w.Distinct().Intersect(words).Count() == words.Count()) ||
        //                 ((p.SKU.ToUpper().Contains(search.ToUpper())) ||
        //                 (p.ProductName.ToUpper().Contains(search.ToUpper()))) 
        //    //             || (p.Barcode.ToUpper() == search.ToUpper()))
        //                 select p;


        //    data = result.ToList();



        //    if (data == null)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return Json(data.Take(100).ToList(), JsonRequestBehavior.AllowGet);
        //    }


        //}

        ////  [HttpPost]
        //public JsonResult AutoCompleteSelected_Product(string search)
        //{
        //    if (search != null)
        //    {
        //        var getList = db.Products.Where(p => p.IsActive == true).ToList();

        //        int newid = Convert.ToInt32(search);
        //        var c = getList.Where(x => x.ProductID == newid).FirstOrDefault();

        //        if (c != null)
        //        {

        //            return Json(new { result = c }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return null;
        //        };

        //    }
        //    else
        //    {
        //        return null;
        //    }

        //}

        // display cost code

        public string Decimal2String(Decimal val)
        {
            string retVal = string.Empty;

            retVal = val.ToString().Replace("1", "A").Replace("2", "I").Replace("3", "K").Replace("4", "C").Replace("5", "H").Replace("6", "N").Replace("7", "M").Replace("8", "E").Replace("9", "R").Replace("0", "Y").Replace(".", "0");

            string finalResult;

            string[] x = retVal.Select(c => c.ToString()).ToArray();

            for (int i = 0; i < x.Length - 1; i++)
            {
                for (int j = i + 1; j < x.Length; j++)
                {
                    if (x[i] == x[j])
                    {

                        x[j] = x[j].Replace(x[j], "S");

                    }

                }
            };

            finalResult = string.Join("", x);
            return finalResult;

        }

        //Start Wallace Add-On
        public JsonResult AutoCompleteSelected2(string search)
        {
            if (search != null)
            {
                int custno = Convert.ToInt32(search);
                var Daddress = (from t in db.Vendors
                                where (t.CustNo == custno)
                                let Addr1 = t.Addr1
                                let Addr2 = t.Addr2
                                let Addr3 = t.Addr3
                                let PostalCode = t.PostalCode
                                select new { DeliveryAddress = Addr1 + " " + Addr2 + " " + Addr3 + " " + PostalCode }).FirstOrDefault();
                var c = Daddress;
                if (c != null)
                {

                    return Json(new { result = c }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                };

            }
            else
            {
                return null;
            }

        }

        [HttpGet]
        public JsonResult GetCreditSettings(int? custNo)
        {
            ClientCreditSetting clientCreditSetting = new ClientCreditSetting();
            clientCreditSetting = db.ClientCreditSetting.Where(m => m.CustNo == custNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();
            return Json(clientCreditSetting, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetPriceOptions(string itemid)
        {
            if (itemid != null)
            {
                //   int cid = Convert.ToInt32(custno);
                int iid = Convert.ToInt32(itemid);

                var p = db.Products.Where(x => x.ProductID == iid).FirstOrDefault();

                var poptions = p.PriceOptions.ToList();

                var c = poptions;

                if (c != null)
                {
                    return Json(new { result = c }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                };
            }
            else
            {
                return null;
            }
        }

        public JsonResult GetPriceOptionByUnit(string itemid, string itemunit)
        {
            if (itemid != null)
            {
                //   int cid = Convert.ToInt32(custno);
                int iid = Convert.ToInt32(itemid);

                var p = db.Products.Where(x => x.ProductID == iid).FirstOrDefault();

                var poptions = p.PriceOptions.Where(x => x.Unit == itemunit).FirstOrDefault();

                var c = poptions;

                if (c != null)
                {
                    return Json(new { result = c }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                };
            }
            else
            {
                return null;
            }
        }

        public JsonResult GetPricebreaks(string custno, string itemid)
        {
            if (custno != null && itemid != null)
            {
                int cid = Convert.ToInt32(custno);
                int iid = Convert.ToInt32(itemid);

                var p = db.Products.Where(x => x.ProductID == iid).FirstOrDefault();

                var pbreaks = p.Pricebreaks.Where(x => x.BreakQty > 0).OrderBy(x => x.BreakQty).ToList();

                var c = pbreaks;

                if (c != null)
                {
                    return Json(new { result = c }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                };
            }
            else
            {
                return null;
            }
        }

        public JsonResult GetInventory(string itemid)
        {
            if (itemid != null)
            {
                //   int cid = Convert.ToInt32(custno);
                int iid = Convert.ToInt32(itemid);

                var p = db.Stocks.Where(x => x.ProductID == iid).FirstOrDefault();

                var c = p;

                if (c != null)
                {
                    return Json(new { result = c }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                };
            }
            else
            {
                return null;
            }
        }


        public JsonResult GetHistPricesByCust(string custno, string itemid)
        {
            if (custno != null && itemid != null)
            {
                int cid = Convert.ToInt32(custno);
                int iid = Convert.ToInt32(itemid);

                var p = db.Products.Where(x => x.ProductID == iid).FirstOrDefault();

                var hPrices = (from i in db.PoINVDETs
                               join s in db.PoINVs on i.InvID equals s.InvID
                               where (s.CustNo == cid && i.ItemID == iid)
                               select new
                               {
                                   InvID = s.InvID,
                                   InvDate = s.InvDate,
                                   CustName = s.CustName,
                                   Qty = i.Qty,
                                   Unit = i.Unit,
                                   Price = i.DiscountedPrice

                               }).Take(3).ToList();

                var c = hPrices;

                if (c != null)
                {
                    return Json(new { result = c }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                };
            }
            else
            {
                return null;
            }
        }

        public JsonResult GetHistPricesByCustOther(string custno, string itemid)
        {
            if (custno != null && itemid != null)
            {
                int cid = Convert.ToInt32(custno);
                int iid = Convert.ToInt32(itemid);

                var p = db.Products.Where(x => x.ProductID == iid).FirstOrDefault();

                var hPrices = (from i in db.PoINVDETs
                               join s in db.PoINVs on i.InvID equals s.InvID
                               where (s.CustNo != cid && i.ItemID == iid)
                               select new
                               {
                                   InvID = s.InvID,
                                   InvDate = s.InvDate,
                                   CustName = s.CustName,
                                   Qty = i.Qty,
                                   Unit = i.Unit,
                                   Price = i.DiscountedPrice

                               }).Take(3).ToList();

                var c = hPrices;

                if (c != null)
                {
                    return Json(new { result = c }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                };
            }
            else
            {
                return null;
            }
        }


        public JsonResult GetCustPriceDetail(string custno, string itemid)
        {
            if (custno != null && itemid != null)
            {
                int cid = Convert.ToInt32(custno);
                int iid = Convert.ToInt32(itemid);
                var HPrice = (from i in db.PoINVDETs
                              join s in db.PurchaseOrders on i.SorID equals s.SorID
                              where (s.CustNo == cid && i.ItemID == iid)
                              select new { HistoryPrice = i.DiscountedPrice }).Take(3).ToList();
                var c = HPrice;
                if (c != null)
                {
                    return Json(new { result = c }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                };
            }
            else
            {
                return null;
            }
        }
        public JsonResult GetNonCustPriceDetail(string custno, string itemid)
        {
            if (custno != null && itemid != null)
            {
                int cid = Convert.ToInt32(custno);
                int iid = Convert.ToInt32(itemid);
                var HPrice = (from i in db.PoINVDETs
                              join s in db.PurchaseOrders on i.SorID equals s.SorID
                              where (s.CustNo != cid && i.ItemID == iid)
                              select new { HistoryPrice = i.DiscountedPrice }).Take(3).ToList();
                var c = HPrice;
                if (c != null)
                {
                    return Json(new { result = c }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                };
            }
            else
            {
                return null;
            }
        }
        //End Wallace Add-On

        public ActionResult MultiPagesPrintPage(int id)
        {
            PurchaseOrder sar = db.PurchaseOrders.Find(id);

            var count = db.PoINVDETs.Count(x => x.SorID == id);

            ViewBag.ItemCount = count;

            return View(sar);
        }

        public ActionResult _MultiPagesPrintModal(int id)
        {
            PurchaseOrder sar = db.PurchaseOrders.Find(id);

            PurchaseOrderSelection o = new PurchaseOrderSelection()
            {
                SorID = sar.SorID,
                InvType = sar.InvType,
                InvDate = sar.InvDate,
                PoNo = sar.PoNo,
                CustNo = sar.CustNo,
                CustName = sar.CustName,
                Addr1 = sar.Addr1,
                Addr2 = sar.Addr2,
                Addr3 = sar.Addr3,
                Addr4 = "",
                Attn = sar.Attn,
                DeliveryAddress = sar.DeliveryAddress,
                DeliveryDate = sar.DeliveryDate,
                DeliveryTime = sar.DeliveryTime,
                CurrencyName = sar.CurrencyName,
                ExRate = sar.ExRate,
                PreDiscAmount = sar.PreDiscAmount,
                Discount = sar.Discount,
                Amount = sar.Amount,
                Gst = sar.Gst,
                Nett = sar.Nett,
                PaidAmount = sar.PaidAmount,
                Status = sar.Status,
                PaymentStatus = sar.PaymentStatus,
                PaymentTerms = sar.PaymentTerms,
                LocationID = sar.LocationID,
                LocationName = sar.LocationName,
                Remark = sar.Remark,
                PersonID = sar.PersonID,
                PersonName = sar.PersonName,
                IsPaid = sar.IsPaid,
                PaidDate = sar.PaidDate,
                CreatedBy = sar.CreatedBy,
                CreatedOn = sar.CreatedOn,
                ModifiedBy = sar.ModifiedBy,
                ModifiedOn = sar.ModifiedOn
            };

            o.SalesPaymentMethodList = (from t1 in db.SalesPaymentMethods
                                        where t1.SorID == o.SorID
                                        select new
                                        {
                                            a = t1
                                        }).ToList().Select(x => new SalesPaymentMethod()
                                        {
                                            SalesPaymentMethodID = x.a.SalesPaymentMethodID,
                                            SorID = x.a.SalesPaymentMethodID,
                                            PaymentMethod = x.a.PaymentMethod,
                                            Amount = x.a.Amount,
                                            CreatedBy = x.a.CreatedBy,
                                            CreatedOn = x.a.CreatedOn
                                        });

            var count = db.PoINVDETs.Count(x => x.SorID == id);

            ViewBag.ItemCount = count;
            ViewData["seGSTRate"] = GetGstRate();

            return PartialView(o);
        }


        public ActionResult _OrderPayment(int id)
        {
            PurchaseOrder sar = db.PurchaseOrders.Find(id);

            PurchaseOrderSelection o = new PurchaseOrderSelection()
            {
                SorID = sar.SorID,
                InvType = sar.InvType,
                InvDate = sar.InvDate,
                PoNo = sar.PoNo,
                CustNo = sar.CustNo,
                CustName = sar.CustName,
                Addr1 = sar.Addr1,
                Addr2 = sar.Addr2,
                Addr3 = sar.Addr3,
                Addr4 = "",
                Attn = sar.Attn,
                DeliveryAddress = sar.DeliveryAddress,
                DeliveryDate = sar.DeliveryDate,
                DeliveryTime = sar.DeliveryTime,
                PreDiscAmount = sar.PreDiscAmount,
                Discount = sar.Discount,
                Amount = sar.Amount,
                Gst = sar.Gst,
                Nett = sar.Nett,
                PaidAmount = sar.PaidAmount,
                Status = sar.Status,
                PaymentStatus = sar.PaymentStatus,
                PaymentTerms = sar.PaymentTerms,
                LocationID = sar.LocationID,
                LocationName = sar.LocationName,
                Remark = sar.Remark,
                PersonID = sar.PersonID,
                PersonName = sar.PersonName,
                IsPaid = sar.IsPaid,
                PaidDate = sar.PaidDate,
                CreatedBy = sar.CreatedBy,
                CreatedOn = sar.CreatedOn,
                ModifiedBy = sar.ModifiedBy,
                ModifiedOn = sar.ModifiedOn
            };

            o.SalesPaymentMethodList = (from t1 in db.SalesPaymentMethods
                                        where t1.SorID == o.SorID
                                        select new
                                        {
                                            a = t1
                                        }).ToList().Select(x => new SalesPaymentMethod()
                                        {
                                            SalesPaymentMethodID = x.a.SalesPaymentMethodID,
                                            SorID = x.a.SalesPaymentMethodID,
                                            PaymentMethod = x.a.PaymentMethod,
                                            Amount = x.a.Amount,
                                            CreatedBy = x.a.CreatedBy,
                                            CreatedOn = x.a.CreatedOn
                                        });

            var count = db.PoINVDETs.Count(x => x.SorID == id);

            ViewBag.ItemCount = count;

            return PartialView(o);
        }



        [HttpGet]
        public void SubmitPaymentMethod(int SorID, List<SalesPaymentMethod> PaymentMethodList, Boolean FullPayment)
        {
            SalesPaymentMethod o = new SalesPaymentMethod();
            if (PaymentMethodList.Count > 0)
            {
                for (int i = 0; i <= PaymentMethodList.Count - 1; i++)
                {
                    o = new SalesPaymentMethod()
                    {
                        SorID = SorID,
                        PaymentMethod = PaymentMethodList[i].PaymentMethod,
                        Amount = PaymentMethodList[i].Amount,
                        ChequeNumber = PaymentMethodList[i].ChequeNumber,
                        IsFullPayment = FullPayment
                    };

                    db.SalesPaymentMethods.Add(o);
                    db.SaveChanges();
                }
            }
        }


        [HttpGet]
        public JsonResult _SubmitPurchaseOrder(int SorID)
        {
            PurchaseOrder sor = db.PurchaseOrders.Find(SorID);

            if (string.IsNullOrEmpty(sor.DeliveryDate.ToString()))
            {
                return Json(new { success = false, responseText = "The delivery date can not be empty. Please change again." }, JsonRequestBehavior.AllowGet);
            }

            sor.Status = "Pending Approval";

            if (ModelState.IsValid)
            {
                db.Entry(sor).State = EntityState.Modified;
                db.SaveChanges();
            };

            // move stock to allocated

            UpdateStockOnOrderQty(sor.SorID, sor.LocationID, "IN");

            return Json(new
            {
                success = true,
                printUrl = Url.Action("POPrintPreview", "Invoice", new { id = sor.SorID }),
                redirectUrl = Url.Action("CrCreate", "PurchaseOrders"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult _RejectOrder(int SorID)
        {
            PurchaseOrder sor = db.PurchaseOrders.Find(SorID);

            //if (string.IsNullOrEmpty(sor.DeliveryDate.ToString()))
            //{
            //    return Json(new { success = false, responseText = "The delivery date can not be empty. Please change again." }, JsonRequestBehavior.AllowGet);
            //}

            sor.Status = "Rejected";

            if (ModelState.IsValid)
            {
                db.Entry(sor).State = EntityState.Modified;
                db.SaveChanges();
            };

            // move stock to allocated

            UpdateStockOnOrderQty(sor.SorID, sor.LocationID, "OUT");

            return Json(new
            {
                success = true,
                printUrl = Url.Action("POPrintPreview", "Invoice", new { id = sor.SorID }),
                redirectUrl = Url.Action("CrProcessIndex", "PurchaseOrders"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult _ConfirmPicking(int SorID)
        {
            PurchaseOrder sor = db.PurchaseOrders.Find(SorID);
            var inv = db.PoINVs.Where(x => x.SorID == SorID).FirstOrDefault();

            //if (string.IsNullOrEmpty(sor.DeliveryDate.ToString()))
            //{
            //    return Json(new { success = false, responseText = "The delivery date can not be empty. Please change again." }, JsonRequestBehavior.AllowGet);
            //}

            sor.Status = "Pending Delivery";

            if (ModelState.IsValid)
            {
                db.Entry(sor).State = EntityState.Modified;
                db.SaveChanges();
            };

            return Json(new
            {
                success = true,
                printUrl = Url.Action("PickingListPrint", "Invoice", new { id = inv.InvID }),
                redirectUrl = Url.Action("Create", "Orders"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult _ConfirmDelivery(int SorID)
        {
            PurchaseOrder sor = db.PurchaseOrders.Find(SorID);
            var inv = db.PoINVs.Where(x => x.SorID == SorID).FirstOrDefault();

            //if (string.IsNullOrEmpty(sor.DeliveryDate.ToString()))
            //{
            //    return Json(new { success = false, responseText = "The delivery date can not be empty. Please change again." }, JsonRequestBehavior.AllowGet);
            //}

            sor.Status = "Delivered";

            if (ModelState.IsValid)
            {
                db.Entry(sor).State = EntityState.Modified;
                db.SaveChanges();
            };

            UpdateStockQty(inv.InvID, inv.LocationID);

            return Json(new
            {
                success = true,
                printUrl = Url.Action("DeliveryOrderPrint", "Invoice", new { id = inv.InvID }),
                redirectUrl = Url.Action("CrCreate", "PurchaseOrders"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult _SubmitPurchaseOrderPreview(int SorID, Boolean CheckWithoutPayment, Boolean CheckBoxCash, Boolean CheckBoxNETS, Boolean CheckBoxCreditCard, Boolean CheckBoxCheque, string CheckBoxCashAmount, string CheckBoxNETSAmount, string CheckBoxCreditCardAmount, string CheckBoxChequeAmount, string CheckBoxChequeNumber)
        {
            PurchaseOrder oinv = db.PurchaseOrders.Find(SorID);
            Vendor client = db.Vendors.Find(oinv.CustNo);
            decimal dPaymentAmount = 0;
            decimal dOriginalNett = oinv.Nett;
            decimal dTotalPaid = 0;
            decimal dOutstandingAmount = 0;
            Boolean bFullPayment = false;

            Boolean isWithoutPayment = CheckWithoutPayment;

            if (CheckBoxCash)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxCashAmount);
            }

            if (CheckBoxNETS)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxNETSAmount);
            }

            if (CheckBoxCreditCard)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxCreditCardAmount);
            }

            if (CheckBoxCheque)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxChequeAmount);
            }

            if (dOriginalNett == dPaymentAmount)
                bFullPayment = true;

            var dbpayment = db.SalesPaymentMethods.ToList().Where(x => x.SorID == SorID);

            foreach (var item in dbpayment)
            {
                dTotalPaid += item.Amount;
            }

            dOutstandingAmount = dOriginalNett - dTotalPaid;

            if (dOutstandingAmount < dPaymentAmount)
            {
                return Json(new { success = false, responseText = "The total amount (" + dPaymentAmount.ToString("N") + ") you input is more than the nett amount (" + dOutstandingAmount.ToString("C") + "). Please change again." }, JsonRequestBehavior.AllowGet);
            }

            if (client.AccType == "CR")
            {
                //Check credit limit
            }

            int id = Convert.ToInt32(SorID);

            var sor = db.PurchaseOrders.Find(id);
            if (sor == null)
            {
                return Json(new { success = false, responseText = "The order is not found. No valid data. Please refresh page." }, JsonRequestBehavior.AllowGet);
            };

            var oinvoice = db.PoINVs.Where(x => x.SorID == SorID).FirstOrDefault();
            PoINV invs = new PoINV();
            string sInvoiceStatus = "";
            Boolean IsPaid = false;

            if (oinvoice == null)
            {
                // ************  Get new invoice number *********************

                string newInvNo = "";
                newInvNo = GetMaxPurchaseInvoiceNumber();

                sor.Status = "Invoiced";
                sor.InvNo = newInvNo;

                if (isWithoutPayment)
                {
                    sor.PaymentStatus = "Unpaid";
                    sInvoiceStatus = "Unpaid";
                    IsPaid = false;
                }
                else
                {
                    sor.PaidAmount = dPaymentAmount;

                    if (dOutstandingAmount == dPaymentAmount)
                    {
                        sor.PaymentStatus = "Full Paid";
                        sInvoiceStatus = "Full Paid";
                        IsPaid = true;
                    }
                    else
                    {
                        sor.PaymentStatus = "Partially Paid";
                        sInvoiceStatus = "Partially Paid";
                    }
                }

                if (ModelState.IsValid)
                {
                    db.Entry(sor).State = EntityState.Modified;
                    db.SaveChanges();
                };

                // *** creating new invoice

                invs.InvNo = newInvNo;
                invs.SorID = sor.SorID;
                invs.InvType = sor.InvType;
                invs.InvDate = sor.InvDate;
                invs.PoNo = sor.PoNo;
                invs.InvRef = sor.InvRef;
                invs.CustNo = sor.CustNo;
                invs.CustName = sor.CustName;
                invs.CustName2 = sor.CustName2;
                invs.Addr1 = sor.Addr1;
                invs.Addr2 = sor.Addr2;
                invs.Addr3 = sor.Addr3;
                invs.Addr4 = sor.Addr4;
                invs.Attn = sor.Attn;
                invs.DeliveryAddress = sor.DeliveryAddress;
                invs.DeliveryTime = sor.DeliveryTime;
                invs.DeliveryDate = sor.DeliveryDate;
                invs.CurrencyName = sor.CurrencyName;
                invs.ExRate = sor.ExRate;
                invs.PreDiscAmount = sor.PreDiscAmount;
                invs.Discount = sor.Discount;
                invs.Amount = sor.Amount;
                invs.Gst = sor.Gst;
                invs.Nett = sor.Nett;
                invs.PaidAmount = sor.PaidAmount;
                invs.Status = sor.Status;
                invs.PaymentStatus = sor.PaymentStatus;
                invs.PaymentTerms = sor.PaymentTerms;
                invs.SupplierInvNo = sor.SupplierInvNo;
                invs.SupplierInvDate = sor.SupplierInvDate;
                invs.PaymentDueDate = sor.PaymentDueDate;
                invs.LocationID = sor.LocationID;
                invs.LocationName = sor.LocationName;
                invs.Remark = sor.Remark;
                invs.PersonID = sor.PersonID;
                invs.PersonName = sor.PersonName;
                invs.LocationID = 0;
                invs.LocationName = "";

                if (isWithoutPayment)
                {
                    invs.IsPaid = false;
                    invs.PaidDate = null;
                }
                else
                {
                    invs.IsPaid = IsPaid;
                    invs.PaidDate = DateTime.Now;
                }

                invs.CreatedBy = User.Identity.Name;
                invs.CreatedOn = DateTime.Now;

                db.PoINVs.Add(invs);
                db.SaveChanges();
                
                sor.InvID = invs.InvID;

                if (ModelState.IsValid)
                {
                    db.Entry(sor).State = EntityState.Modified;
                    db.SaveChanges();
                };

                UpdateInvDetWithInvID(SorID);
                UpdateKivDetWithInvID(SorID);
            }
            else
            {
                invs = oinvoice;
                sor.Status = "Invoiced";
                sor.InvNo = invs.InvNo;
                sor.InvID = invs.InvID;

                if (!isWithoutPayment)
                {
                    sor.PaidAmount = dPaymentAmount;
                    invs.PaidAmount = sor.PaidAmount;

                    if (dOutstandingAmount == dPaymentAmount)
                    {
                        sor.PaymentStatus = "Full Paid";
                        sInvoiceStatus = "Full Paid";
                        IsPaid = true;
                    }
                    else
                    {
                        sor.PaymentStatus = "Partially Paid";
                        sInvoiceStatus = "Partially Paid";
                    }

                    invs.Status = sInvoiceStatus;
                    invs.IsPaid = IsPaid;
                    invs.PaidDate = DateTime.Now;

                }
                if (ModelState.IsValid)
                {
                    db.Entry(invs).State = EntityState.Modified;
                    db.SaveChanges();

                    sor.InvID = invs.InvID;
                    db.Entry(sor).State = EntityState.Modified;
                    db.SaveChanges();

                };

                // copy order details to inv

                if (!string.IsNullOrEmpty(sor.InvNo))
                {
                    UpdateInvoice(sor.InvNo);
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
                        SorID = SorID,
                        InvID = invs.InvID,
                        PrID = 0,
                        InvType = invs.InvType,
                        PaymentDate = DateTime.Now,
                        RecordedFrom = invs.InvType,

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
                        SorID = SorID,
                        InvID = invs.InvID,
                        PrID = 0,
                        InvType = invs.InvType,
                        PaymentDate = DateTime.Now,
                        RecordedFrom = invs.InvType,

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
                        SorID = SorID,
                        InvID = invs.InvID,
                        PrID = 0,
                        InvType = invs.InvType,
                        PaymentDate = DateTime.Now,
                        RecordedFrom = invs.InvType,

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
                        SorID = SorID,
                        InvID = invs.InvID,
                        PrID = 0,
                        InvType = invs.InvType,
                        PaymentDate = DateTime.Now,
                        RecordedFrom = invs.InvType,

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


            //    UpdateStockQty(invs.InvID, invs.LocationID);
            CreatePoKivQty(invs.InvID, invs.LocationID);

            return Json(new
            {
                printUrl = Url.Action("PrintPIAndArrivalNote", "Invoice", new { id = invs.InvID }),
                printInvUrl = Url.Action("PrintPI", "Invoice", new { id = invs.InvID }),
                printDOUrl = Url.Action("PrintArrivalNote", "Invoice", new { id = invs.InvID }),
                redirectUrl = Url.Action("OrderProcessed", "PurchaseOrder", new { id = sor.SorID }),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }

        private void CreatePoKivQty(int InvID, int locid)
        {
            var inv = db.PoINVs.Where(x => x.InvID == InvID).FirstOrDefault();

            // if invno exist, remove all existing kiv items
            var kivs = db.PoKIVs.Where(x => x.InvNo == inv.InvNo).ToList();
            if (kivs != null)
            {
                foreach (var ki in kivs)
                {
                    if (ki != null)
                    {
                        db.PoKIVs.Remove(ki);
                        db.SaveChanges();
                    }
                }
            }

            var dets = db.PoKIVDETs.Where(x => x.InvID == InvID).ToList();
            foreach (var det in dets)
            {
                if (det != null && inv != null)
                {
                    if (det.KivBalanceQty > 0)
                    {
                        var kiv = new PoKIV();
                        kiv.SorID = det.SorID;
                        kiv.InvID = det.InvID;
                        kiv.InvNo = inv.InvNo;
                        kiv.InvDetID = det.InvDetID;
                        kiv.InvDate = inv.InvDate;
                        kiv.CustNo = inv.CustNo;
                        kiv.CustName = inv.CustName;
                        kiv.ProductID = det.ItemID;
                        kiv.ProductName = det.ItemName;
                        kiv.SKU = det.ItemCode;
                        kiv.Unit = det.Unit;
                        kiv.OrderQty = det.OrderQty;
                        kiv.BalanceQty = det.KivBalanceQty;
                        kiv.SalesType = det.SalesType;
                        kiv.Position = det.Position;
                        kiv.Remark = det.Remark;
                        kiv.CreatedBy = User.Identity.Name;
                        kiv.CreatedOn = System.DateTime.Now;

                        if (ModelState.IsValid)
                        {
                            db.PoKIVs.Add(kiv);
                            db.SaveChanges();
                        }

                        //if (det.KivBalanceQty > 0)
                        //{
                        //    UpdateStock_OnKiv(det.ItemID, det.KivBalanceQty, "IN");
                        //}

                    }
                }
            }

        }

        private void UpdateStock_OnKiv(int productId, double qty, string processType)
        {
            var p = db.Stocks.Where(x => x.ProductID == productId).FirstOrDefault();

            if (p != null)
            {
                if (processType == "IN")
                {
                    p.OnKiv = p.OnKiv + qty;

                };
                if (processType == "OUT")
                {
                    p.OnKiv = p.OnKiv - qty;

                };

                if (ModelState.IsValid)
                {
                    db.Entry(p).State = EntityState.Modified;
                    db.SaveChanges();
                };
            }

        }

        private void UpdateStockQty(int InvID, int locid)
        {
            var dets = db.PoINVDETs.Where(x => x.InvID == InvID).ToList();
            foreach (var det in dets)
            {
                if (det != null)
                {
                    if (det.Qty > 0)
                    {
                        UpdateStock(det.ItemID, det.Qty, "OUT");
                    }
                }
            }

        }

        private void UpdateStock(int productId, double qty, string processType)
        {
            var p = db.Stocks.Where(x => x.ProductID == productId).FirstOrDefault();

            if (p != null)
            {
                if (processType == "IN")
                {
                    p.StockIn = p.StockIn + qty;
                    p.InStock = p.InStock + qty;
                    p.OnHand = p.OnHand + qty;

                };
                if (processType == "OUT")
                {
                    p.Allocated = p.Allocated - qty;
                    p.StockOut = p.StockOut + qty;
                    p.InStock = p.InStock - qty;

                };
                if (processType == "ADJ")
                {
                    p.StockAdjusted = qty;
                };

                if (ModelState.IsValid)
                {
                    db.Entry(p).State = EntityState.Modified;
                    db.SaveChanges();
                };
            }

        }

        private void UpdateStockOnOrderQty(int SorID, int locid, string processType)
        {
            var dets = db.PoINVDETs.Where(x => x.SorID == SorID).ToList();
            foreach (var det in dets)
            {
                if (det != null)
                {
                    if (det.Qty > 0)
                    {
                        UpdateStockOnOrder(det.ItemID, det.Qty, processType);
                    }
                }
            }

        }

        private void UpdateStockOnOrder(int productId, double qty, string processType)
        {
            var p = db.Stocks.Where(x => x.ProductID == productId).FirstOrDefault();

            if (p != null)
            {
                if (processType == "IN")
                {
                    p.OnOrder = p.OnOrder + qty;
 

                };
                if (processType == "OUT")
                {
                    p.OnOrder = p.OnOrder - qty;
 
                };

                if (ModelState.IsValid)
                {
                    db.Entry(p).State = EntityState.Modified;
                    db.SaveChanges();
                };
            }

        }


        private void UpdateStockAllocatedQty(int SorID, int locid, string processType)
        {
            var dets = db.PoINVDETs.Where(x => x.SorID == SorID).ToList();
            foreach (var det in dets)
            {
                if (det != null)
                {
                    if (det.Qty > 0)
                    {
                        UpdateStockAllocated(det.ItemID, det.Qty, processType);
                    }
                }
            }

        }

        private void UpdateStockAllocated(int productId, double qty, string processType)
        {
            var p = db.Stocks.Where(x => x.ProductID == productId).FirstOrDefault();

            if (p != null)
            {
                if (processType == "IN")
                {
                    p.OnHand = p.OnHand + qty;
                    p.Allocated = p.Allocated - qty;

                };
                if (processType == "OUT")
                {
                    p.OnHand = p.OnHand - qty;
                    p.Allocated = p.Allocated + qty;
                };

                if (ModelState.IsValid)
                {
                    db.Entry(p).State = EntityState.Modified;
                    db.SaveChanges();
                };
            }

        }



        [HttpGet]
        public JsonResult _ItemMoveUp(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var inv = db.PoINVDETs.Where(x => x.DetID == id).FirstOrDefault();
                var kiv = db.PoKIVDETs.Where(x => x.InvDetID == id).FirstOrDefault();
                double currentposition = inv.Position;

                if (!inv.IsBundle)
                {
                    var nextinv = db.PoINVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition - 1).FirstOrDefault();
                    var nextkiv = db.PoKIVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition - 1).FirstOrDefault();
                    nextinv.Position += 1;
                    inv.Position -= 1;
                    nextkiv.Position += 1;
                    kiv.Position -= 1;

                    db.Entry(nextinv).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(inv).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(nextkiv).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(kiv).State = EntityState.Modified;
                    db.SaveChanges();

                    if (nextinv.IsBundle)
                    {
                        var bundleinv = db.PoINVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == nextinv.DetID).ToList();

                        foreach (var item in bundleinv)
                        {
                            item.Position += 1;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();

                            var bundlekiv = db.PoKIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
                            bundlekiv.Position += 1;
                            db.Entry(bundlekiv).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    if (inv.SalesType == "DefaultItem")
                    {
                        var nextinv = db.PoINVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition - 1).FirstOrDefault();
                        var bundleinv = db.PoINVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == inv.DetID).ToList();
                        var nextkiv = db.PoKIVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition - 1).FirstOrDefault();

                        foreach (var item in bundleinv)
                        {
                            item.Position -= 1;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();

                            var bundlekiv = db.PoKIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
                            bundlekiv.Position -= 1;
                            db.Entry(bundlekiv).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        nextinv.Position += 1;
                        inv.Position -= 1;
                        nextkiv.Position += 1;
                        kiv.Position -= 1;

                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(nextkiv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(kiv).State = EntityState.Modified;
                        db.SaveChanges();

                        if (nextinv.IsBundle)
                        {
                            bundleinv = db.PoINVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == nextinv.DetID).ToList();

                            foreach (var item in bundleinv)
                            {
                                item.Position += 1;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();

                                var bundlekiv = db.PoKIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
                                bundlekiv.Position += 1;
                                db.Entry(bundlekiv).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                    else if (inv.SalesType == "BundleItem" || inv.SalesType == "FOCItem")
                    {
                        currentposition -= 0.1;
                        var nextinv = db.PoINVDETs.Where(x => x.SorID == inv.SorID && x.Position == Math.Round(currentposition, 1)).FirstOrDefault();
                        var nextkiv = db.PoKIVDETs.Where(x => x.SorID == inv.SorID && x.Position == Math.Round(currentposition, 1)).FirstOrDefault();
                        nextinv.Position = Math.Round(nextinv.Position + 0.1, 1);
                        inv.Position = Math.Round(inv.Position - 0.1, 1);
                        nextkiv.Position = Math.Round(nextkiv.Position + 0.1, 1);
                        kiv.Position = Math.Round(kiv.Position - 0.1, 1);

                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(nextkiv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(kiv).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                trans.Complete();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult _ItemMoveDown(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var inv = db.PoINVDETs.Where(x => x.DetID == id).FirstOrDefault();
                var kiv = db.PoKIVDETs.Where(x => x.InvDetID == id).FirstOrDefault();
                double currentposition = inv.Position;

                if (!inv.IsBundle)
                {
                    var nextinv = db.PoINVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition + 1).FirstOrDefault();
                    var nextkiv = db.PoKIVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition + 1).FirstOrDefault();
                    nextinv.Position -= 1;
                    nextkiv.Position -= 1;
                    inv.Position += 1;
                    kiv.Position += 1;

                    db.Entry(nextinv).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(nextkiv).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(inv).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(kiv).State = EntityState.Modified;
                    db.SaveChanges();

                    if (nextinv.IsBundle)
                    {
                        var bundleinv = db.PoINVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == nextinv.DetID).ToList();

                        foreach (var item in bundleinv)
                        {
                            item.Position -= 1;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();

                            var bundlekiv = db.PoKIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
                            bundlekiv.Position -= 1;
                            db.Entry(bundlekiv).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    if (inv.SalesType == "DefaultItem")
                    {
                        var nextinv = db.PoINVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition + 1).FirstOrDefault();
                        var nextkiv = db.PoKIVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition + 1).FirstOrDefault();

                        if (nextinv == null)
                        {
                            return Json(new { success = false, responseText = "You cannot move it down as it is the last item." }, JsonRequestBehavior.AllowGet);
                        }

                        var bundleinv = db.PoINVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == inv.DetID).ToList();

                        foreach (var item in bundleinv)
                        {
                            item.Position += 1;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();

                            var bundlekiv = db.PoKIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
                            bundlekiv.Position += 1;
                            db.Entry(bundlekiv).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        nextinv.Position -= 1;
                        inv.Position += 1;
                        nextkiv.Position -= 1;
                        kiv.Position += 1;

                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(nextkiv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(kiv).State = EntityState.Modified;
                        db.SaveChanges();

                        if (nextinv.IsBundle)
                        {
                            bundleinv = db.PoINVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == nextinv.DetID).ToList();

                            foreach (var item in bundleinv)
                            {
                                item.Position -= 1;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();

                                var bundlekiv = db.PoKIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
                                bundlekiv.Position -= 1;
                                db.Entry(bundlekiv).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                    else if (inv.SalesType == "BundleItem" || inv.SalesType == "FOCItem")
                    {
                        var nextinv = db.PoINVDETs.Where(x => x.SorID == inv.SorID && x.Position == Math.Round(currentposition + 0.1, 1)).FirstOrDefault();
                        var nextkiv = db.PoKIVDETs.Where(x => x.SorID == inv.SorID && x.Position == Math.Round(currentposition + 0.1, 1)).FirstOrDefault();

                        //if (nextinv.SalesType == "FOCItem")
                        //{
                        //    return Json(new { success = false, responseText = "You are not allowed to move it down as there is FOC item below." }, JsonRequestBehavior.AllowGet);
                        //}

                        if (nextinv == null)
                        {
                            return Json(new { success = false, responseText = "You cannot move it down as it is the last item." }, JsonRequestBehavior.AllowGet);
                        }

                        nextinv.Position = Math.Round(nextinv.Position - 0.1, 1);
                        inv.Position = Math.Round(inv.Position + 0.1, 1);
                        nextkiv.Position = Math.Round(nextkiv.Position - 0.1, 1);
                        kiv.Position = Math.Round(kiv.Position + 0.1, 1);

                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(nextkiv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(kiv).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                trans.Complete();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }





        //************************* For Credit Sales *******************************************************// 

        public ActionResult CreditSales()
        {
            return View();
        }

        public ActionResult CrProcessIndex()
        {
            return View();
        }

        public ActionResult PickingList()
        {
            return View();

        }

        public ActionResult DeliveryOrder()
        {
            return View();
        }

        // GET: Sales/Create
        public ActionResult CrCreate()
        {
            var p = new PurchaseOrder();

            //  p.SarOn.DateCreated = DateTime.Now;
            //  ViewBag.ProductTypes = db.ProductGroups.ToList();

            //    ViewData["ClientsAll"] = GetVendorListByUser("CR");
            ViewData["ClientsAll"] = db.Vendors.Where(x => x.AccType == "CR" && x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["seGSTRate"] = GetGstRate();

            ViewBag.SalesID = 0;

            var staff = db.Staffs.Where(x => x.Email.Trim() == User.Identity.Name.Trim()).FirstOrDefault();
            if (staff != null)
            {
                ViewBag.StaffID = staff.StaffID;
            }

            return View(p);
        }

        // POST: Sales/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrCreate([Bind(Include = "SorID,SorNo,QuoID,QuoNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,DeliveryAddress,DeliveryDate,DeliveryTime,PaymentTerms,Status,Remark,PersonID,PersonName")] PurchaseOrder inv)
        {

            string str = GetMaxPurchaseOrderNumber(); 
            //   string str = invoice.GetInvoiceNumber(InvType.SO.ToString(), DateTime.Now, User.Identity.Name);

            inv.SorNo = str;

            inv.CurrencyName = "SGD";
            inv.ExRate = 1;
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
                db.PurchaseOrders.Add(inv);
                db.SaveChanges();

                flag = true;
            };

            if (flag)
            {
                //CreateKIV(inv.SorID);

                return RedirectToAction("CrEdit", new { id = inv.SorID, str = "1" });
            }

            ViewBag.Message = "1";

            //   ViewData["ClientsAll"] = GetVendorListByUser("CR");
            ViewData["ClientsAll"] = db.Vendors.Where(x => x.AccType == "CR" && x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CurrencyAll"] = db.CurrencySettings.Where(x => x.IsActive == true).OrderBy(x => x.CurrencyName).ToList();
            ViewData["seGSTRate"] = GetGstRate();

            ViewBag.SalesID = 0;

            var staff = db.Staffs.Where(x => x.Email.Trim() == User.Identity.Name.Trim()).FirstOrDefault();
            if (staff != null)
            {
                ViewBag.StaffID = staff.StaffID;
            }

            return View(inv);
        }

        // GET: Sales/Edit/5
        public ActionResult CrEdit(int? id, string str)
        {
            PurchaseOrder inv = new PurchaseOrder();

            if (id == null || id == 0)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                string newno = GetMaxPurchaseOrderNumber();
                inv.SorNo = newno;
                inv.InvID = 0;
                inv.QuoID = 0;
                inv.InvDate = DateTime.Now;
                inv.InvType = "CR";

                inv.CustNo = 0;
                inv.CustName = "Select company";
                inv.CurrencyName = "SGD";
                inv.ExRate = 1;
                inv.PreDiscAmount = 0;
                inv.Discount = 0;
                inv.Amount = 0;
                inv.Gst = 0;
                inv.Nett = 0;
                inv.PaidAmount = 0;
                inv.IsPaid = false;
                inv.Status = "Draft";
                inv.PersonID = 0;
                inv.LocationID = 0;
                inv.CreatedBy = User.Identity.Name;
                inv.CreatedOn = DateTime.Now;

                if (ModelState.IsValid)
                {
                    db.PurchaseOrders.Add(inv);
                    db.SaveChanges();
                };

                return RedirectToAction("CrEdit", new { id = inv.SorID, str = "0" });

            }


            inv = db.PurchaseOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            bool isFound = false;
            var clist = GetVendorListByUser("CR");
            foreach (var c in clist)
            {
                if (c.CustNo == inv.CustNo)
                {
                    isFound = true;
                }
            }

            if (!isFound)
            {
                var client = db.Vendors.Where(x => x.CustNo == inv.CustNo).FirstOrDefault();
                clist.Add(client);
            }

            //    ViewData["ClientsAll"] = clist;
            ViewData["ClientsAll"] = db.Vendors.Where(x => x.AccType == "CR" && x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();
            ViewData["seGSTRate"] = GetGstRate();
            ViewData["CurrencyAll"] = db.CurrencySettings.Where(x => x.IsActive == true).OrderBy(x => x.CurrencyName).ToList();

            var item = db.PoINVs.Where(x => x.SorID == id).FirstOrDefault();
            if (item != null)
            {
                ViewBag.InvoiceNo = item.InvID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };

            ViewBag.PageFrom = str;

            ViewBag.SalesID = 0;

            var staff = db.Staffs.Where(x => x.Email.Trim() == User.Identity.Name.Trim()).FirstOrDefault();
            if (staff != null)
            {
                ViewBag.StaffID = staff.StaffID;
            }

            return View(inv);
        }

        // POST: Sales/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
      //  [ValidateAntiForgeryToken]
        public JsonResult CrEdit([Bind(Include = "SorID,SorNo,QuoID,QuoNo,DorNo,InvID,InvNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,PostalCode,DeliveryAddress,DeliveryDate,DeliveryTime,Status,CurrencyName,ExRate,PreDiscAmount,Discount,Amount,Gst,Nett,PaidAmount,PaymentTerms,SupplierInvNo,SupplierInvDate,PaymentDueDate,Remark,PersonID,PersonName,CreatedOn")] PurchaseOrder inv)
        {
            if (inv.PersonID == 0)
            {
                inv.PersonName = null;
            }
            inv.ModifiedBy = User.Identity.Name;
            inv.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();
            }

            string str = Request.Form["actionType"];

            if (str == "SaveAndAdd")
            {
                //return RedirectToAction("CrCreate");
                //   return RedirectToAction("CrEdit", new { id = 0 });
            };



            //if (inv.PersonID == 0)
            //{
            //    inv.PersonName = null;
            //}
            //inv.ModifiedBy = User.Identity.Name;
            //inv.ModifiedOn = DateTime.Now;

            //if (ModelState.IsValid)
            //{
            //    db.Entry(inv).State = EntityState.Modified;
            //    db.SaveChanges();
            //}

            //string str = Request.Form["actionType"];

            //if (str == "SaveAndAdd")
            //{
            //    return RedirectToAction("CrCreate");
            //};

            //return RedirectToAction("CrEdit", new { id = inv.SorID });

            bool isFound = false;
            var clist = GetVendorListByUser("CR");
            foreach (var c in clist)
            {
                if (c.CustNo == inv.CustNo)
                {
                    isFound = true;
                }
            }

            if (!isFound)
            {
                var client = db.Vendors.Where(x => x.CustNo == inv.CustNo).FirstOrDefault();
                clist.Add(client);
            }

            //   ViewData["ClientsAll"] = clist;
            ViewData["ClientsAll"] = db.Vendors.Where(x => x.AccType == "CR" && x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();
            ViewData["CurrencyAll"] = db.CurrencySettings.Where(x => x.IsActive == true).OrderBy(x => x.CurrencyName).ToList();

            ViewBag.SalesID = 0;

            var staff = db.Staffs.Where(x => x.Email.Trim() == User.Identity.Name.Trim()).FirstOrDefault();
            if (staff != null)
            {
                ViewBag.StaffID = staff.StaffID;
            }

          //  return View(inv);
            return Json(new { success = true, redirectUrl = Url.Action("CrEdit", "PurchaseOrder", new { id = 0, str = "0" }) });
        }

        private void UpdateInvoice(string invno)
        {
            var inv = db.INVs.Where(x => x.InvNo == invno).FirstOrDefault();
            if (inv != null)
            {
                int sorid = inv.SorID;
                var sor = db.PurchaseOrders.Where(x => x.SorID == sorid).FirstOrDefault();
                if (sor != null)
                {
                    // copy sor details to inv

                    inv.CustNo = sor.CustNo;
                    inv.CustName = sor.CustName;
                    inv.CustName2 = sor.CustName2;
                    inv.Addr1 = sor.Addr1;
                    inv.Addr2 = sor.Addr2;
                    inv.Addr3 = sor.Addr3;
                    inv.Addr4 = sor.Addr4;
                    inv.Attn = sor.Attn;
                    inv.PhoneNo = sor.PhoneNo;
                    inv.FaxNo = sor.FaxNo;
                    inv.DeliveryAddress = sor.DeliveryAddress;
                    inv.DeliveryDate = sor.DeliveryDate;
                    inv.DeliveryTime = sor.DeliveryTime;
                    inv.InvType = sor.InvType;
                    inv.InvDate = sor.InvDate;
                    inv.PreDiscAmount = sor.PreDiscAmount;
                    inv.Discount = sor.Discount;
                    inv.Amount = sor.Amount;
                    inv.Gst = sor.Gst;
                    inv.Nett = sor.Nett;
                    inv.PaidAmount = sor.PaidAmount;
                    inv.IsPaid = sor.IsPaid;
                    inv.PaidDate = sor.PaidDate;
                    inv.Status = sor.Status;
                    inv.PaymentStatus = sor.PaymentStatus;
                    inv.PaymentTerms = sor.PaymentTerms;
                    inv.LocationID = sor.LocationID;
                    inv.LocationName = sor.LocationName;
                    inv.Remark = sor.Remark;
                    inv.PersonID = sor.PersonID;
                    inv.PersonName = sor.PersonName;
                    inv.ModifiedBy = User.Identity.Name;
                    inv.ModifiedOn = DateTime.Now;

                    if (ModelState.IsValid)
                    {
                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    UpdateInvDetWithInvID(inv.SorID);
                    UpdateKivDetWithInvID(inv.SorID);

                }


            }
        }


        public ActionResult _AddMultiItem(int id)
        {
            var sor = db.PurchaseOrders.Where(x => x.SorID == id).FirstOrDefault();

            if (sor == null)
            {
                return null;
            }

            var dets = new List<PoINVDET>();

            return PartialView(dets);
        }

        [HttpPost]
        public ActionResult _AddMultiItem(List<PoINVDET> list)
        {
            if (list == null)
            {
                return PartialView(list);
            }
            else
            {
                int SorID = list.FirstOrDefault().SorID;

                //bool IsFirst = true;
                //int refid = 0;

                foreach (var det in list)
                {
                    if (det.Qty > 0)
                    {

                        var invdet1 = db.PoINVDETs.Where(x => x.SorID == SorID).ToList();
                        double positioncount = invdet1.Count;
                        det.Position = positioncount + 1;

                        if (ModelState.IsValid)
                        {
                            db.PoINVDETs.Add(det);
                            db.SaveChanges();
                        }

                        AddKivDet(det);
                    }

                };
                UpdateContractAmount(SorID);

                UpdateKivDets(SorID);

                return RedirectToAction("CrEdit", new { id = SorID });
            }

        }

        public ActionResult _SearchResult(string txtSearch)
        {
            var plist = new List<Product>();
            if (!string.IsNullOrEmpty(txtSearch))
            {
                plist = db.Products.Where(x => x.ProductName.Contains(txtSearch.Trim()) || x.SKU.StartsWith(txtSearch.Trim()) || x.ModelNo.StartsWith(txtSearch.Trim())).Take(200).ToList();
            }

            ProductSelectionView model = new ProductSelectionView();
            foreach (var p in plist)
            {
                var selecteditor = new ProductSelectEditor()
                {
                    Selected = false,
                    ProductID = p.ProductID,
                    SKU = p.SKU,
                    ProductName = p.ProductName,
                    ModelNo = p.ModelNo,
                    Unit = p.Unit,
                    Qty = p.Qty,
                    CostPrice = p.CostPrice,
                    CostCode = p.CostCode,
                    RetailPrice = p.RetailPrice

                };
                model.productselects.Add(selecteditor);
            };

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult _SearchResult(ProductSelectionView model, string valSorID)
        //     public ActionResult _SubmitSelected(ProductSelectionView model)
        {
            //   int sorid = Convert.ToInt32(Request.Form["valSorID"]);
            int sorid = Convert.ToInt32(valSorID);
            var selectedview = new ProductSelectionView();
            // get the ids of the items selected:
            List<int> selectedIds = model.getSelectedIds().ToList();

            // Use the ids to retrieve the records for the selected job
            // from the database:
            var selectedProducts = db.Products.Where(x => selectedIds.Contains(x.ProductID)).ToList();

            //var selectedProducts = (from x in db.Products
            //                    where selectedIds.Contains(x.ProductID)
            //                    select x).ToList();

            foreach (var p in selectedProducts)
            {
                // add this product into INVDET
                if (p != null)
                {
                    AddIntoINVDET(p.ProductID, sorid);
                }
            };

            //     return PartialView(selectedview);
            return Json(new { success = true });

        }

        private void AddIntoINVDET(int id, int sorid)
        {
            var sor = db.PurchaseOrders.Where(x => x.SorID == sorid).FirstOrDefault();

            if (sor == null)
            {
                return;
            }

            List<PoINVDET> dets = new List<PoINVDET>();
            var p = db.Products.Find(id);

            if (p != null)
            {
                // 1. create det1 and add to dets
                // 2. if p.IsBundle == true, get sub det and add to dets
                // 3. save all dets

                var invdet1 = db.PoINVDETs.Where(x => x.RefItemID == 0 && x.SorID == sor.SorID).ToList();
                double positioncount = invdet1.Count;
                double qty = 1.0d;
                string selltype = "CS";

                var det1 = new PoINVDET();
                det1.QuoNo = sor.QuoNo;
                det1.SorID = sor.SorID;
                det1.SorNo = sor.SorNo;
                det1.InvID = sor.InvID;
                det1.InvNo = sor.InvNo;
                det1.CnID = 0;
                det1.CnNo = "";
                det1.InvType = sor.InvType;
                det1.ItemID = p.ProductID;
                det1.ItemCode = p.SKU;
                det1.ItemType = p.ProductType;
                det1.ItemName = p.ProductName;
                det1.SellType = selltype;
                det1.Qty = qty;
                det1.Unit = p.Unit;
                det1.UnitPrice = p.RetailPrice;
                det1.DiscountedPrice = p.RetailPrice;
                det1.Discount = 0m;

                det1.PreDiscAmount = p.RetailPrice * Convert.ToDecimal(qty);
                det1.Amount = p.RetailPrice * Convert.ToDecimal(qty);
                det1.Gst = 0m;
                det1.Nett = det1.Amount + det1.Gst;

                det1.IsBundle = p.IsBundle;
                det1.SalesType = "DefaultItem";
                det1.RefItemID = 0;
                det1.InvRef = "";
                det1.IsControlItem = p.IsControlItem;
                det1.LocationID = 0;
                det1.LocationName = "";
                det1.Remark = "";
                //det1.Position = 0;
                det1.Position = positioncount + 1;

                dets.Add(det1);

                int bundlecount = 0;
                if (p.IsBundle == true)
                {
                    foreach (var bb in p.Productbundles)
                    {
                        bundlecount++;
                        var det2 = new PoINVDET();
                        det2.QuoNo = det1.QuoNo;
                        det2.SorID = det1.SorID;
                        det2.SorNo = det1.SorNo;
                        det2.InvID = det1.InvID;
                        det2.InvNo = det1.InvNo;
                        det2.CnID = det1.CnID;
                        det2.CnNo = det1.CnNo;
                        det2.InvType = det1.InvType;
                        det2.ItemID = bb.IncProductID;
                        det2.ItemCode = bb.IncSKU;
                        det2.ItemType = bb.IncProductType;
                        det2.ItemName = bb.IncProductName;
                        det2.SellType = det1.SellType;
                        det2.Qty = bb.IncQty * qty;
                        det2.Unit = det1.Unit;

                        det2.UnitPrice = 0m;
                        det2.DiscountedPrice = 0m;
                        det2.Discount = 0m;
                        det2.PreDiscAmount = 0m;
                        det2.Amount = 0m;
                        det2.Gst = 0m;
                        det2.Nett = 0m;

                        det2.IsBundle = false;
                        det2.SalesType = "BundleItem";
                        det2.RefItemID = det1.DetID;
                        det2.InvRef = "";
                        det2.IsControlItem = bb.IsControlItem;
                        det2.LocationID = 0;
                        det2.LocationName = "";
                        det2.Remark = p.ProductID.ToString();

                        //det2.Position = 1;
                        det2.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());
                        dets.Add(det2);

                    }
                }

                if (p.ProductFOCs.Count > 0)
                {
                    double focqty = 0.00;

                    if (p.UsePricebreak)
                    {
                        var breakqtys = p.Pricebreaks.Where(x => x.BreakQty >= 0).OrderByDescending(x => x.BreakQty).ToList();

                        foreach (var bq in breakqtys)
                        {
                            if (det1.Qty >= bq.BreakQty)
                            {
                                if (!string.IsNullOrEmpty(bq.FocQty.ToString()))
                                {
                                    focqty = Convert.ToDouble(bq.FocQty);
                                };
                                break;
                            }
                        }

                    };

                    foreach (var bb in p.ProductFOCs)
                    {
                        bundlecount++;
                        var det2 = new PoINVDET();
                        det2.QuoNo = det1.QuoNo;
                        det2.SorID = det1.SorID;
                        det2.SorNo = det1.SorNo;
                        det2.InvID = det1.InvID;
                        det2.InvNo = det1.InvNo;
                        det2.CnID = det1.CnID;
                        det2.CnNo = det1.CnNo;
                        det2.InvType = det1.InvType;
                        det2.ItemID = bb.IncProductID;
                        det2.ItemCode = bb.IncSKU;
                        det2.ItemType = bb.IncProductType;
                        det2.ItemName = bb.IncProductName;
                        det2.SellType = det1.SellType;
                        det2.Qty = bb.IncQty * qty;
                        det2.Unit = det1.Unit;

                        det2.UnitPrice = 0m;
                        det2.DiscountedPrice = 0m;
                        det2.Discount = 0m;
                        det2.PreDiscAmount = 0m;
                        det2.Amount = 0m;
                        det2.Gst = 0m;
                        det2.Nett = 0m;

                        det2.IsBundle = false;
                        det2.SalesType = "FOCItem";
                        det2.RefItemID = det1.DetID;
                        det2.InvRef = "";
                        det2.IsControlItem = bb.IsControlItem;
                        det2.LocationID = 0;
                        det2.LocationName = "";
                        det2.Remark = p.ProductID.ToString();
                        det2.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());

                        dets.Add(det2);
                    }
                }


                foreach (var dd in dets)
                {
                    if (dd != null)
                    {
                        var errors = ModelState.Values.SelectMany(v => v.Errors);

                        if (ModelState.IsValid)
                        {
                            db.PoINVDETs.Add(dd);
                            db.SaveChanges();
                        }


                        AddKivDet(dd);
                    }

                };

                UpdateContractAmount(sorid);
                UpdateKivDets(sorid);
            }
        }


        [HttpGet]
        public JsonResult AutoComplete_ProductByItem(string search, int sorid)
        {
            List<PoINVDET> data = new List<PoINVDET>();
            var getList = GetProductList();

            string[] words = search.ToUpper().Split(' ').ToArray();

            var result = from p in getList
                         let w = p.ProductName.ToUpper().Split(new char[] { ' ', ';', ':', ',' },
                                                    StringSplitOptions.RemoveEmptyEntries)
                         where (w.Distinct().Intersect(words).Count() == words.Count()) ||
                         ((p.SKU.ToUpper().StartsWith(search.ToUpper())) || (p.SKU.ToUpper().Contains(search.ToUpper())) || (p.ProductName.ToUpper().Contains(search.ToUpper())) || ((words.Except(w.Distinct().Intersect(words)).Any(wo => p.ProductName.ToUpper().Contains(wo)))) ||
                         (p.ProductName.ToUpper().StartsWith(search.ToUpper())))
                         select p;


            foreach (var word in words)
            {

                result = from p in result
                         where ((p.ProductName.ToUpper() + " " + p.SKU.ToUpper()).Contains(word))
                         select p;
            }

            foreach (var item in result)
            {
                var list = _getProductItems(item.ProductID, sorid).ToList();
                foreach (var li in list)
                {
                    if (li != null)
                    {
                        data.Add(li);
                    }
                }
            }


            if (data == null)
            {
                return null;
            }
            else
            {
                return Json(data.Take(100).ToList(), JsonRequestBehavior.AllowGet);
            }


        }


        public List<PoINVDET> _getProductItems(int pid, int sorid)
        {
            List<PoINVDET> dets = new List<PoINVDET>();
            var sor = db.PurchaseOrders.Find(sorid);
            var p = db.Products.Find(pid);

            if (p != null && sor != null)
            {
                var qty = 1;
                var det1 = new PoINVDET();
                det1.SorID = sor.SorID;
                det1.SorNo = sor.SorNo;
                det1.InvID = sor.InvID;
                det1.InvNo = sor.InvNo;
                det1.InvType = sor.InvType;
                det1.ItemID = p.ProductID;
                det1.ItemCode = p.SKU;
                det1.ItemType = p.ProductType;
                det1.ItemName = p.ProductName;
                det1.SellType = "CS";
                det1.Qty = Convert.ToDouble(1);
                det1.Unit = p.Unit;
                det1.UnitPrice = p.RetailPrice;
                det1.DiscountedPrice = p.RetailPrice;
                det1.Discount = 0;

                det1.PreDiscAmount = det1.UnitPrice * Convert.ToDecimal(qty);
                det1.Amount = det1.DiscountedPrice * Convert.ToDecimal(qty);
                det1.Gst = 0;
                det1.Nett = det1.Amount + det1.Gst;

                det1.IsBundle = p.IsBundle;

                if (p.IsBundle == true)
                {
                    det1.SalesType = "Bundle";
                }
                else
                {
                    det1.SalesType = "DefaultItem";
                }

                det1.RefItemID = 0;
                det1.InvRef = "";
                det1.IsControlItem = p.IsControlItem;

                det1.Remark = "";

                dets.Add(det1);

                int bundlecount = 0;

                if (p.IsBundle == true)
                {
                    foreach (var bb in p.Productbundles)
                    {
                        bundlecount++;
                        var det2 = new PoINVDET();
                        det2.SorID = det1.SorID;
                        det2.SorNo = det1.SorNo;
                        det2.InvID = det1.InvID;
                        det2.InvNo = det1.InvNo;
                        det2.InvType = det1.InvType;
                        det2.ItemID = bb.IncProductID;
                        det2.ItemCode = bb.IncSKU;
                        det2.ItemType = bb.IncProductType;
                        det2.ItemName = bb.IncProductName;
                        det2.SellType = det1.SellType;
                        det2.Qty = bb.IncQty * qty;
                        det2.Unit = p.Unit;

                        det2.IsBundle = false;
                        det2.SalesType = "BundleItem";
                        det2.RefItemID = det1.DetID;
                        det2.InvRef = "";
                        det2.IsControlItem = bb.IsControlItem;
                        det2.Remark = p.ProductID.ToString();

                        dets.Add(det2);

                    }
                }

                ViewBag.HasFOC = "False";

                if (p.ProductFOCs.Count > 0)
                {
                    ViewBag.HasFOC = "True";

                    double focqty = 0.00;

                    if (p.UsePricebreak)
                    {
                        var breakqtys = p.Pricebreaks.Where(x => x.BreakQty >= 0).OrderByDescending(x => x.BreakQty).ToList();

                        foreach (var bq in breakqtys)
                        {
                            if (det1.Qty >= bq.BreakQty)
                            {
                                if (!string.IsNullOrEmpty(bq.FocQty.ToString()))
                                {
                                    focqty = Convert.ToDouble(bq.FocQty);
                                };
                                break;
                            }
                        }

                    };

                    foreach (var bb in p.ProductFOCs)
                    {
                        bundlecount++;
                        var det2 = new PoINVDET();
                        det2.SorID = det1.SorID;
                        det2.SorNo = det1.SorNo;
                        det2.InvID = det1.InvID;
                        det2.InvNo = det1.InvNo;
                        det2.InvType = det1.InvType;
                        det2.ItemID = bb.IncProductID;
                        det2.ItemCode = bb.IncSKU;
                        det2.ItemType = bb.IncProductType;
                        det2.ItemName = bb.IncProductName;
                        det2.SellType = det1.SellType;
                        det2.Qty = bb.IncQty * qty;
                        det2.Unit = p.Unit;

                        det2.IsBundle = false;
                        det2.SalesType = "FOCItem";
                        det2.RefItemID = det1.DetID;
                        det2.InvRef = "";
                        det2.IsControlItem = bb.IsControlItem;
                        det2.Remark = p.ProductID.ToString();

                        dets.Add(det2);

                    }
                }

            }

            return dets;
        }







    }
}