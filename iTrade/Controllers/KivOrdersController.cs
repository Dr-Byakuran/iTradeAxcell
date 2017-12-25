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
    public class KivOrdersController : ControllerBase
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

        // GET: KivOrders
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ProIndex()
        {
            return View();
        }

        public ActionResult IndexKor()
        {
            DateTime d = DateTime.Now.AddMonths(-12);
            var result = db.KIVs.Where(x=> x.InvType == "CR" && x.CreatedOn > d).OrderByDescending(x => x.InvNo).ThenBy(x => x.CustName).ToList();
            return View(result);

        }

        public ActionResult IndexPro()
        {
            DateTime d = DateTime.Now.AddMonths(-12);
            var result = db.KIVs.Where(x => x.InvType == "PRO" && x.CreatedOn > d).OrderByDescending(x => x.InvNo).ThenBy(x => x.CustName).ToList();
            return View(result);

        }

        public ActionResult IndexEor()
        {
            return View();

        }

        // [ChildActionOnly] 
        public ActionResult _DisplayResults(string invtype, string invstatus)
        {

            DateTime datefrom = DateTime.Now.AddMonths(-12);
            var p = new List<KivOrder>();

            if (invstatus == "")
            {
                p = db.KivOrders.Where(x => x.InvType == invtype &&  x.InvDate >= datefrom).Take(600).OrderByDescending(x => x.KorID).ToList();
                ViewBag.TableNo = 0;
            };
            if (invstatus == "Confirmed")
            {
                p = db.KivOrders.Where(x => x.InvType == invtype && x.InvDate >= datefrom && x.Status == "Confirmed").Take(600).OrderByDescending(x => x.KorID).ToList();
                ViewBag.TableNo = 1;
            };
            if (invstatus == "Pending Approval")
            {
                p = db.KivOrders.Where(x => x.InvType == invtype && x.InvDate >= datefrom && x.Status == "Pending Approval").Take(600).OrderByDescending(x => x.KorID).ToList();
                ViewBag.TableNo = 2;
            };
            if (invstatus == "Draft" || invstatus == "Rejected")
            {
                p = db.KivOrders.Where(x => x.InvType == invtype && x.InvDate >= datefrom && (x.Status == "Draft" || x.Status == "Rejected")).Take(600).OrderByDescending(x => x.KorID).ToList();
                ViewBag.TableNo = 3;
            };

            ViewBag.SalesID = 0;

            var staff = db.Staffs.Where(x => x.Email.Trim() == User.Identity.Name.Trim()).FirstOrDefault();
            if (staff != null)
            {
                ViewBag.StaffID = staff.StaffID;
            }


            return PartialView(p);
        }

        public ActionResult _DisplayEors(int id)
        {
            //   string str = "";
            switch (id)
            {
                case 0:
                    //     str = "";
                    ViewBag.TableNo = 0;
                    break;


            };

            var p = new List<KivExchangeOrder>();

            p = db.KivExchangeOrders.OrderByDescending(x => x.KivEorID).ToList();


            return PartialView(p);
        }

        public ActionResult _DisplayKivs(int id)
        {
            var p = db.KIVs.Where(x => x.InvType == "CR").OrderByDescending(x => x.InvNo).ThenBy(x => x.CustName).ToList();

            if (id != 0)
            {
                p = db.KIVs.Where(x => x.InvType == "CR" && x.CustNo == id).OrderBy(x => x.InvNo).ToList();
            }

            //ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _DisplayKivsPro(int id)
        {
            var p = db.KIVs.Where(x => x.InvType == "PRO").OrderByDescending(x => x.InvNo).ThenBy(x => x.CustName).ToList();

            if (id != 0)
            {
                p = db.KIVs.Where(x => x.InvType == "PRO" && x.CustNo == id).OrderBy(x => x.InvNo).ToList();
            }

            //ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _DisplayKivsByCust(int id)
        {
            var p = db.KIVs.Where(x => x.InvType == "CR" && x.CustNo == 0).OrderBy(x => x.CustName).ThenBy(x => x.InvNo).ToList();

            if (id != 0)
            {
                p = db.KIVs.Where(x => x.InvType == "CR" && x.CustNo == id).OrderBy(x => x.InvNo).ToList();
            };

            //ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _DisplayKivsByCustPro(int id)
        {
            var p = db.KIVs.Where(x => x.InvType == "PRO" && x.CustNo == 0).OrderBy(x => x.CustName).ThenBy(x => x.InvNo).ToList();

            if (id != 0)
            {
                p = db.KIVs.Where(x => x.InvType == "PRO" && x.CustNo == id).OrderBy(x => x.InvNo).ToList();
            };

            //ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _OrderDetail(int? id, string act)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KivOrder inv = db.KivOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewBag.KorNumber = id;
            ViewBag.Act = act;

            return PartialView(inv);
        }

        public ActionResult _EorDetail(int? id, string act)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KivExchangeOrder inv = db.KivExchangeOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewBag.KorNumber = id;
            ViewBag.Act = act;

            return PartialView(inv);
        }

        public ActionResult _EorDetailView(int? id, string act)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KivExchangeOrder inv = db.KivExchangeOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewBag.KorNumber = id;
            ViewBag.Act = act;

            return PartialView(inv);
        }

        public ActionResult _OrderDetailView(int? id, string act)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KivOrder inv = db.KivOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewBag.KorNumber = id;
            ViewBag.Act = act;

            return PartialView(inv);
        }


        public ActionResult _DisplayInvDets(int id, string act)
        {
            var p = new List<KivOrderDet>();
            p = db.KivOrderDets
                    .Where(x => (x.KorID == id))
                    .OrderBy(x => x.Position)
                    .ToList();

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        public ActionResult _DisplayInvDetsEor(int id, string act)
        {
            var p = new List<KivOrderDet>();
            p = db.KivOrderDets
                    .Where(x => (x.KivEorID == id))
                    .OrderBy(x => x.Position)
                    .ToList();

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        public ActionResult _DisplayInvDetsView(int id, string act)
        {
            var p = new List<KivOrderDet>();
            p = db.KivOrderDets
                    .Where(x => (x.KorID == id))
                    .OrderBy(x => x.Position)
                    .ToList();
 

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        public ActionResult _DisplayInvDetsViewEor(int id, string act)
        {
            var p = new List<KivOrderDet>();
            p = db.KivOrderDets
               .Where(x => (x.KivEorID == id))
               .OrderBy(x => x.Position)
               .ToList();

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }


        public ActionResult _DisplayInvDetsPrint(int id)
        {
            var p = new List<KivOrderDet>();
            p = db.KivOrderDets
                .Where(x => (x.KorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _DisplayKivDets(int id, string act)
        {
            var p = new List<KIVDET>();
            p = db.KIVDETs
                .Where(x => (x.KorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _DisplayKivDets(List<KIVDET> list)
        {
            if (list == null)
            {
                return null;
            }

            int KorID = list.FirstOrDefault().KorID;
            var kor = db.KivOrders.Find(KorID);

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.KIVDETs.Find(i.DetID);
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

            UpdateKivOrderDets(KorID);

            if (kor.InvType == "PRO")
            {
                return RedirectToAction("EditPro", new { id = KorID });
            }
            else
            {
                return RedirectToAction("Edit", new { id = KorID });
            }

        }

        public ActionResult _DisplayKivDetsEor(int id, string act)
        {
            var p = new List<KIVDET>(); 
            p = db.KIVDETs
            .Where(x => (x.KivEorID == id))
            .OrderBy(x => x.Position)
            .ToList(); 

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _DisplayKivDetsEor(List<KIVDET> list)
        {
            if (list == null)
            {
                return null;
            }

            int KivEorID = list.FirstOrDefault().KivEorID;
            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.KIVDETs.Find(i.DetID);
                    if (det != null)
                    {

                        det.OrderQty = i.OrderQty;
                        det.BalanceQty = i.BalanceQty;
                        det.DeliverQty = i.DeliverQty;
                        det.ExchangeQty = i.ExchangeQty;
                        det.KivBalanceQty = det.BalanceQty - det.ExchangeQty;

                    }

                    //   UpdateKivDet(det);
                };
                db.SaveChanges();
            };

            UpdateKivOrderDetsEor(KivEorID);

            //    UpdateKivDets(SorID);

            //    return Json(new { success = true });

            return RedirectToAction("EditEor", new { id = KivEorID });

        }


        private void UpdateKivOrderDetsEor(int KorID)
        {
            // get list from kivdets
            // loop list and check if kivdet is in kivorderdets
            // if qty = 0, remove items from kivorderdets
            // if true, update deliver order qty and calculate changed qty
            // if false, add item to kivorderdets, set default delivery qty = deliver order qty

            var kor = db.KivExchangeOrders.Find(KorID);

            var list = db.KIVDETs.Where(x => x.KivEorID == KorID).ToList();
            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.KivOrderDets.Where(x => x.KivEorID == KorID && x.InvDetID == i.InvDetID).FirstOrDefault();
                    if (det != null)
                    {
                        det.DeliverOrderQty = i.ExchangeQty;
                        det.ChangedQty = i.ExchangeQty - det.DeliverQty;

                    }
                    else
                    {
                        det = new KivOrderDet();
                        det.DetType = "KIVEC";
                        det.KivEorID = KorID;
                        det.KorID = 0;
                        det.KivID = 0;
                        det.InvID = i.InvID;
                        det.InvNo = kor.InvRef;
                        det.InvDetID = i.InvDetID;
                        det.ItemID = i.ItemID;
                        det.ItemCode = i.ItemCode;
                        det.ItemName = i.ItemName;
                        det.Unit = i.Unit;
                        det.SellType = "CS";
                        det.DeliverOrderQty = i.ExchangeQty;
                        det.DeliverQty = i.ExchangeQty;
                        det.ChangedQty = 0.00;
                        det.SalesType = i.SalesType;
                        det.RefItemID = 0;
                        det.Position = i.Position;
                        det.Remark = i.Remark;

                        db.Entry(det).State = EntityState.Added;

                    }
                };
                db.SaveChanges();
            };

            var odets = db.KivOrderDets.Where(x => x.KivEorID == KorID).ToList();
            foreach (var od in odets)
            {
                if (od.DeliverOrderQty == 0)
                {
                    // remove 
                    var subdets = db.KivOrderDets.Where(x => x.KivEorID == KorID && x.RefItemID == od.DetID && x.InvDetID == od.InvDetID).ToList();
                    foreach (var subdet in subdets)
                    {
                        if (subdet != null)
                        {
                            db.KivOrderDets.Remove(subdet);
                            db.SaveChanges();
                        }
                    }

                    db.KivOrderDets.Remove(od);
                    db.SaveChanges();

                }

            }


        }

        private void UpdateKivOrderDets(int KorID)
        {
            // get list from kivdets
            // loop list and check if kivdet is in kivorderdets
            // if qty = 0, remove items from kivorderdets
            // if true, update deliver order qty and calculate changed qty
            // if false, add item to kivorderdets, set default delivery qty = deliver order qty

            var kor = db.KivOrders.Find(KorID);

            var list = db.KIVDETs.Where(x => x.KorID == KorID).ToList();
            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.KivOrderDets.Where(x => x.KorID == KorID && x.InvDetID == i.InvDetID).FirstOrDefault();
                    if (det != null)
                    {
                        det.DeliverOrderQty = i.DeliverQty;
                        det.ChangedQty = i.DeliverQty - det.DeliverQty;
                        det.BalanceQty = i.DeliverQty;////////Balance
                    }
                    else
                    {
                        det = new KivOrderDet();
                        det.KorID = KorID;
                        det.KivID = i.KivID;//0;
                        det.InvID = i.InvID;
                        det.InvNo = kor.InvRef;
                        det.InvDetID = i.InvDetID;
                        det.ItemID = i.ItemID;
                        det.ItemCode = i.ItemCode;
                        det.ItemName = i.ItemName;
                        det.Unit = i.Unit;
                        det.SellType = "CS";
                        det.DeliverOrderQty = i.DeliverQty;
                        det.DeliverQty = i.DeliverQty;
                        det.BalanceQty = i.DeliverQty;///////////Balance
                        det.ChangedQty = 0.00;
                        det.SalesType = i.SalesType;
                        det.RefItemID = 0;
                        det.Position = i.Position;
                        det.Remark = i.Remark;

                        db.Entry(det).State = EntityState.Added;

                    }
                };
                db.SaveChanges();
            };

            var odets = db.KivOrderDets.Where(x => x.KorID == KorID).ToList();
            foreach (var od in odets)
            {
                if (od.DeliverOrderQty == 0)
                {
                    // remove 
                    var subdets = db.KivOrderDets.Where(x => x.KorID == KorID && x.RefItemID == od.DetID && x.InvDetID == od.InvDetID).ToList();
                    foreach (var subdet in subdets)
                    {
                        if (subdet != null)
                        {
                            db.KivOrderDets.Remove(subdet);
                            db.SaveChanges();
                        }
                    }

                    db.KivOrderDets.Remove(od);
                    db.SaveChanges();

                }

            }


        }



        public ActionResult _DisplayKivDetsPrint(int id)
        {
            var p = new List<KIVDET>();
            p = db.KIVDETs
                .Where(x => (x.KorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;


            return PartialView(p);
        }



        //public ActionResult _DisplayKivList(int id)
        //{
        //    var list = new List<KivListView>();

        //    var dets = (from t1 in db.INVs
        //                join t2 in db.KIVs on t1.InvID equals t2.InvID
        //                select new
        //                {
        //                    t1,
        //                    t2

        //                }).OrderBy(x => x.t1.CustName).ToList();

        //    foreach (var det in dets)
        //    {
        //        var p = new KivListView();
        //        p.KIV = det.t2;
        //        p.INV = det.t1;

        //        list.Add(p);
        //    }

        //    //ViewBag.QuoteNumber = id;

        //    return PartialView(list);
        //}

        // GET: Sales/Create
        public ActionResult Create()
        {
            var p = new KivOrder();

            //  p.SarOn.DateCreated = DateTime.Now;
            //  ViewBag.ProductTypes = db.ProductGroups.ToList();

            ViewData["ClientsAll"] = GetClientListByUser("ALL");

          //  ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(p);
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "KorID,KorNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,DeliveryAddress,DeliveryDate,DeliveryTime,PaymentTerms,Status,Remark,PersonID,PersonName")] KivOrder inv)
        {
            string str = GetMaxKivNumber();
            inv.KorNo = str;
            inv.CreatedBy = User.Identity.Name;
            inv.CreatedOn = DateTime.Now;

            Boolean flag = false;

            if (ModelState.IsValid)
            {
                db.KivOrders.Add(inv);
                db.SaveChanges();

                flag = true;
            };

            if (flag)
            {
               CreateKivDets(inv.KorID, inv.CustNo, inv.InvRef,"KIVOR");

                return RedirectToAction("Edit", new { id = inv.KorID, str = "1" });
            }

            ViewBag.Message = "1";

            ViewData["ClientsAll"] = GetClientListByUser("ALL");
       //     ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(inv);
        }

        public ActionResult CreatePro()
        {
            var p = new KivOrder();

            //  p.SarOn.DateCreated = DateTime.Now;
            //  ViewBag.ProductTypes = db.ProductGroups.ToList();

            ViewData["ClientsAll"] = GetClientListByUser("ALL");

            //  ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(p);
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePro([Bind(Include = "KorID,KorNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,DeliveryAddress,DeliveryDate,DeliveryTime,PaymentTerms,Status,Remark,PersonID,PersonName")] KivOrder inv)
        {
            string str = GetMaxKivNumber();
            inv.KorNo = str;
            inv.CreatedBy = User.Identity.Name;
            inv.CreatedOn = DateTime.Now;

            Boolean flag = false;

            if (ModelState.IsValid)
            {
                db.KivOrders.Add(inv);
                db.SaveChanges();

                flag = true;
            };

            if (flag)
            {
                CreateKivDets(inv.KorID, inv.CustNo, inv.InvRef, "KIVOR");

                return RedirectToAction("EditPro", new { id = inv.KorID, str = "1" });
            }

            ViewBag.Message = "1";

            ViewData["ClientsAll"] = GetClientListByUser("ALL");
            //     ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(inv);
        }


        [HttpGet]
        public JsonResult CreateByInvNo(string id)
        {         
            var p = db.KivOrders.Where(x => x.InvRef == id & x.Status.StartsWith("Pending Approval")).ToList();
            if (p.Count > 0)
            {
                return Json(new { success = false, responseText = "This invoice has pending KIV approval, please get approval before creating new KIV Delivery." }, JsonRequestBehavior.AllowGet);

            }

            var inv = db.INVs.Where(x => x.InvNo == id).FirstOrDefault();
            var ko = new KivOrder();
            Boolean flag = false;

            if (inv == null)
            {
                return Json(new { success = false, responseText = "The invoice number# " + id + " not in the system." }, JsonRequestBehavior.AllowGet);

            }
            else
            {
             //   string str = GetMaxKivNumber();

                string newno = "";
                var sp = db.Staffs.Find(inv.PersonID);
                if (sp != null && !string.IsNullOrEmpty(sp.DepartmentName))
                {
                    if (sp.DepartmentName == "SR")
                    {
                        newno = GetSerialNumber(206);
                    }
                    else
                    {
                        newno = GetSerialNumber(106);
                    }
                }
                else
                {
                    newno = GetSerialNumber(106);
                }

                ko.KorNo = newno;
                ko.InvType = inv.InvType;
                ko.InvDate = DateTime.Now;
                ko.PoNo = inv.PoNo;
                ko.InvRef = inv.InvNo;
                ko.CustNo = inv.CustNo;
                ko.CustName = inv.CustName;
                ko.CustName2 = inv.CustName2;
                ko.Addr1 = inv.Addr1;
                ko.Addr2 = inv.Addr2;
                ko.Addr3 = inv.Addr3;
                ko.Addr4 = inv.Addr4;
                ko.Attn = inv.Attn;
                ko.PhoneNo = inv.PhoneNo;
                ko.FaxNo = inv.FaxNo;
                ko.DeliveryAddress = inv.DeliveryAddress;
                ko.PaymentTerms = inv.PaymentTerms;
                ko.Status = "Draft";
                ko.LocationID = inv.LocationID;
                ko.LocationName = inv.LocationName;
                ko.PersonID = inv.PersonID;
                ko.PersonName = inv.PersonName;

                ko.CreatedBy = User.Identity.Name;
                ko.CreatedOn = DateTime.Now;

                if (ModelState.IsValid)
                {
                    db.KivOrders.Add(ko);
                    db.SaveChanges();

                    flag = true;
                };
            }


            if (flag)
            {
                CreateKivDets(ko.KorID, ko.CustNo, ko.InvRef, "KIVOR");

            }

            return Json(new { success = true, redirectUrl = Url.Action("Edit", "KivOrders", new { id = ko.KorID, str = "1" }) }, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public JsonResult CreateBySorNo(string id)
        {
            var inv = db.SalesOrders.Where(x => x.SorNo == id).FirstOrDefault();
            var ko = new KivOrder();
            Boolean flag = false;

            if (inv == null)
            {
                return Json(new { success = false, responseText = "The sales order# " + id + " not in the system." }, JsonRequestBehavior.AllowGet);

            } 
            else
            {
              //  string str = GetMaxKivNumber();

                string newno = "";
                var sp = db.Staffs.Find(inv.PersonID);
                if (sp != null && !string.IsNullOrEmpty(sp.DepartmentName))
                {
                    if (sp.DepartmentName == "SR")
                    {
                        newno = GetSerialNumber(206);
                    }
                    else
                    {
                        newno = GetSerialNumber(106);
                    }
                }
                else
                {
                    newno = GetSerialNumber(106);
                }

                ko.KorNo = newno;
                ko.InvType = inv.InvType;
                ko.InvDate = DateTime.Now;
                ko.PoNo = inv.PoNo;
                ko.InvRef = inv.SorNo;
                ko.CustNo = inv.CustNo;
                ko.CustName = inv.CustName;
                ko.CustName2 = inv.CustName2;
                ko.Addr1 = inv.Addr1;
                ko.Addr2 = inv.Addr2;
                ko.Addr3 = inv.Addr3;
                ko.Addr4 = inv.Addr4;
                ko.Attn = inv.Attn;
                ko.PhoneNo = inv.PhoneNo;
                ko.FaxNo = inv.FaxNo;
                ko.DeliveryAddress = inv.DeliveryAddress;
                ko.PaymentTerms = inv.PaymentTerms;
                ko.Status = "Draft";
                ko.LocationID = inv.LocationID;
                ko.LocationName = inv.LocationName;
                ko.PersonID = inv.PersonID;
                ko.PersonName = inv.PersonName;        

                ko.CreatedBy = User.Identity.Name;
                ko.CreatedOn = DateTime.Now;

                if (ModelState.IsValid)
                {
                    db.KivOrders.Add(ko);
                    db.SaveChanges();

                    flag = true;
                };
            }


            if (flag)
            {
                CreateKivDets(ko.KorID, ko.CustNo, ko.InvRef,"KIVOR");

            }

            if (ko.InvType == "PRO")
            {
                return Json(new { success = true, redirectUrl = Url.Action("EditPro", "KivOrders", new { id = ko.KorID, str = "1" }) }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = true, redirectUrl = Url.Action("Edit", "KivOrders", new { id = ko.KorID, str = "1" }) }, JsonRequestBehavior.AllowGet);

            }

 
        }


        private void CreateKivDets(int KorID, int custno, string invref, string invtype)
        {
            var list = db.KIVs.Where(x => x.CustNo == custno && x.InvNo == invref).ToList();

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                        var det = new KIVDET();

                        det.SorID = i.SorID;
                        det.InvID = i.InvID;

                        if (invtype == "KIVOR")
                        {
                            det.KorID = KorID;
                        }
                        if (invtype == "KIVEC")
                        {
                            det.KivEorID = KorID;
                        }

                        det.KivID = i.KivID;
                        det.InvType = invtype;
                        det.InvDetID = i.InvDetID;
                        det.ItemID = i.ProductID;
                        det.ItemCode = i.SKU;
                        det.ItemName = i.ProductName;
                        det.Unit = i.Unit;
                        det.OrderQty = i.OrderQty;
                        det.BalanceQty = i.BalanceQty;
                        det.DeliverQty = 0.00;
                        det.KivBalanceQty = 0.00;
                        det.SalesType = i.SalesType;
                        det.Position = i.Position;
                        det.Remark = i.Remark;

                        db.Entry(det).State = EntityState.Added;
                };
                db.SaveChanges();
            };

        }


        public ActionResult Edit(int? id, string str)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KivOrder inv = db.KivOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            ViewBag.PageFrom = str;

            return View(inv);
        }

        [HttpPost]
        //   [ValidateAntiForgeryToken]
        public JsonResult Edit([Bind(Include = "KorID,KorNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PaymentTerms,Remark,PersonID,PersonName,CreatedOn")] KivOrder inv)
        {
            inv.ModifiedBy = User.Identity.Name;
            inv.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();

                //string str = Request.Form["actionType"];

                //if (str == "SaveAndAdd")
                //{
                //    return RedirectToAction("Create", "KivOrders");
                //};

                //  return RedirectToAction("Edit", new { id = inv.KorID });
            }

            ViewData["ClientsAll"] = GetClientListByUser("ALL");
            //   ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            //  return View(inv);
            return Json(new { success = true, redirectUrl = Url.Action("Create", "KivOrders") });
        }
  


        public ActionResult EditPro(int? id, string str)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KivOrder inv = db.KivOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            ViewBag.PageFrom = str;

            return View(inv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPro([Bind(Include = "KorID,KorNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PaymentTerms,Remark,PersonID,PersonName,CreatedOn")] KivOrder inv)
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
                    return RedirectToAction("CreatePro", "KivOrders");
                };

                return RedirectToAction("EditPro", new { id = inv.KorID });
            }

            ViewData["ClientsAll"] = GetClientListByUser("ALL");
         //   ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            return View(inv);
        }
        
        public ActionResult View(int? id, string str)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KivOrder inv = db.KivOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            if (inv != null)
            {
                ViewBag.InvoiceNo = inv.KorID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };

            ViewData["ClientsAll"] = GetClientListByUser("ALL");
         //   ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            ViewBag.PageFrom = str;

            return View(inv);
        }

        
        public ActionResult _KivListByCust(int id)
        {
            var kor = db.KivOrders.Find(id);

            if (kor == null)
            {
                return HttpNotFound();
            }
            var list = db.KivOrderDets.Where(x => x.KorID == id).ToList();
            var detlist = new List<KivOrderDet>();
            //foreach (var li in list)
            //{
            //    int detid = li.DetID;
            //    detlist.Add(li);
            //}


            int custno = kor.CustNo;

            var dets = db.KIVs.Where(x => x.CustNo == custno && x.InvNo == kor.InvRef).ToList();

            //var dets = (from t1 in db.INVs
            //            join t2 in db.KIVs on t1.InvID equals t2.InvID
            //            where t1.CustNo == custno
            //            select new
            //            {
            //                t2.KivID,
            //                t2.InvID,
            //                t2.InvDetID,
            //                t2.ProductID,
            //                t2.SKU,
            //                t2.ProductName,
            //                t2.OrderQty,
            //                t2.BalanceQty,
            //                t2.SalesType,
            //                t2.Position,
            //                t2.Remark

            //            }).ToList();

            foreach (var det in dets)
            {
                //bool flag = list.Any(s => s.InvDetID.ToString().Contains(det.InvDetID.ToString()));
             //   bool flag = list.Any(s => s.Remark.ToString().Contains(det.Remark.ToString()));

                //if (!flag)
                //{
                    var i = new KivOrderDet
                    {
                        KorID = id,
                        KivID = det.KivID,
                        InvID = det.InvID,
                        InvNo = det.InvNo,
                        InvDetID = det.InvDetID,
                        ItemID = det.ProductID,
                        ItemCode = det.SKU,
                        ItemName = det.ProductName,
                        Unit = det.Unit,
                        SellType = "CS",
                        DeliverOrderQty = 0,
                        DeliverQty = 0,
                        ChangedQty = 0,
 
                        SalesType = det.SalesType,
                        RefItemID = 0,
                        Position = det.Position,
                        Remark = det.Remark
                    };
                    detlist.Add(i);
              //  }
            }


            //p = db.KivOrderDets
            //    .Where(x => (x.KorID == id))
            //    .OrderBy(x => x.Position)
            //    .ToList();

            ViewBag.KorNumber = id;

            return PartialView(detlist);
        }

        [HttpPost]
        public ActionResult _KivListByCust(List<KivOrderDet> list)
        {
            string strKorID = Request.Form["txtKorID2"];
            int KorID = int.Parse(strKorID);
            var kor = db.KivOrders.Find(KorID);

            //var newlist = list.Where(x => x.DeliverQty > 0).ToList();

            if (list == null)
            {
                if (kor.InvType == "PRO")
                {
                    return RedirectToAction("EditPro", new { id = KorID });
                }
                else
                {
                    return RedirectToAction("Edit", new { id = KorID });
                }
            }

            foreach (var i in list)
            {
                if (i != null && i.DeliverQty > 0)
                {
                    var det = new KivOrderDet();
                    det.KivID = i.KivID;
                    det.InvID = i.InvID;
                    det.KorID = KorID;
                    det.InvDetID = i.InvDetID;
                    det.ItemID = i.ItemID;
                    det.ItemCode = i.ItemCode;
                    det.ItemName = i.ItemName;
                    //det.OrderQty = i.OrderQty;
                    //det.BalanceQty = i.BalanceQty;
                    //det.DeliverQty = i.DeliverQty;
                    //det.KivBalanceQty = i.BalanceQty - i.DeliverQty;
                    det.SalesType = i.SalesType;
                    det.Position = i.Position;
                    det.Remark = i.Remark;
 


                    if (ModelState.IsValid)
                    {
                        db.Entry(det).State = EntityState.Added;
                        db.SaveChanges();
                    }

                }
            }


            if (kor.InvType == "PRO")
            {
                return RedirectToAction("EditPro", new { id = KorID });
            }
            else
            {
                return RedirectToAction("Edit", new { id = KorID });
            }

        }



        public ActionResult _DisplayKivOrderDets(int id)
        {
            var kor = db.KivOrders.Find(id);

            if (kor == null)
            {
                return HttpNotFound();
            }
            var list = db.KivOrderDets.Where(x => x.KorID == id).ToList();
            //var detlist = new List<KivOrderDet>();
            //foreach (var li in list)
            //{
            //    int detid = li.DetID;
            //    detlist.Add(li);
            //}


            //int custno = kor.CustNo;

            //var dets = (from t1 in db.INVs
            //            join t2 in db.KIVs on t1.InvID equals t2.InvID
            //            where t1.CustNo == custno
            //            select new 
            //            {
            //                t2.KivID,
            //                t2.InvID, 
            //                t2.InvDetID,
            //                t2.ProductID,
            //                t2.SKU,
            //                t2.ProductName,
            //                t2.OrderQty,
            //                t2.BalanceQty,
            //                t2.SalesType,
            //                t2.Position,
            //                t2.Remark

            //            }).ToList();
            


            //foreach (var det in dets)
            //{
            //    bool flag = list.Any(s => s.InvDetID.ToString().Contains(det.InvDetID.ToString()));

            //    if (!flag)
            //    {
            //        var i = new KivOrderDet
            //        {
            //            KivID = det.KivID,
            //            InvID = det.InvID,
            //            KorID = id,
            //            InvDetID = det.InvDetID,
            //            ItemID = det.ProductID,
            //            ItemCode = det.SKU,
            //            ItemName = det.ProductName,
            //            OrderQty = det.OrderQty,
            //            BalanceQty = det.BalanceQty,
            //            DeliverQty = 0,
            //            KivBalanceQty = det.BalanceQty,
            //            SalesType = det.SalesType,
            //            Position = det.Position,
            //            Remark = det.Remark
            //        };
            //        detlist.Add(i);
            //    }
            //}


            //p = db.KivOrderDets
            //    .Where(x => (x.KorID == id))
            //    .OrderBy(x => x.Position)
            //    .ToList();

            ViewBag.KorNumber = id;

            return PartialView(list);
        }

        [HttpPost]
        public ActionResult _DisplayKivOrderDets(List<KivOrderDet> list)
        {
            string strKorID = Request.Form["txtKorID"];
            int KorID = int.Parse(strKorID);

            //var newlist = list.Where(x => x.DeliverQty > 0).ToList();

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.KivOrderDets.Where(x => x.KorID == i.KorID && x.InvID == i.InvID && x.InvDetID == i.InvDetID).FirstOrDefault();
                    if (det != null)
                    {

                        //det.OrderQty = i.OrderQty;
                        //det.BalanceQty = i.BalanceQty;
                        //det.DeliverQty = i.DeliverQty;
                        //det.KivBalanceQty = det.BalanceQty - det.DeliverQty;
                     //   db.Entry(det).State = EntityState.Modified;
                    }
                    else
                    {
                        det = new KivOrderDet();
                        det.KivID = i.KivID;
                        det.InvID = i.InvID;
                        det.KorID = KorID;
                        det.InvDetID = i.InvDetID;
                        det.ItemID = i.ItemID;
                        det.ItemCode = i.ItemCode;
                        det.ItemName = i.ItemName;
                        //det.OrderQty = i.OrderQty;
                        //det.BalanceQty = i.BalanceQty;
                        //det.DeliverQty = i.DeliverQty;
                        //det.KivBalanceQty = i.BalanceQty - i.DeliverQty;
                        det.SalesType = i.SalesType;
                        det.Position = i.Position;
                        det.Remark = i.Remark;

                        db.Entry(det).State = EntityState.Added;
                    }

                    //   UpdateKivDet(det);
                };
                db.SaveChanges();
            };

            //var dets = db.KivOrderDets.Where(x => x.KorID == KorID).ToList();

            //foreach (var det in dets)
            //{
            //    if (det.DeliverQty == 0)
            //    {
            //        db.Entry(det).State = EntityState.Deleted;
            //        db.SaveChanges();
            //    }
            //}


            //    UpdateKivDets(SorID);

            //    return Json(new { success = true });

            return RedirectToAction("Edit", new { id = KorID });

        }

        public ActionResult _DisplayKivOrderDetsPrint(int id)
        {
            var p = new List<KivOrderDet>();
            p = db.KivOrderDets
                .Where(x => (x.KorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _PreviewOrder(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KivOrder inv = db.KivOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            var item = db.KivOrders.Where(x => x.KorID == id).FirstOrDefault();
            if (item != null)
            {
                ViewBag.InvoiceNo = item.KorID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };

            return PartialView(inv);
        }

        public ActionResult _KivSubmit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KivOrder inv = db.KivOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            var item = db.KivOrders.Where(x => x.KorID == id).FirstOrDefault();
            if (item != null)
            {
                ViewBag.InvoiceNo = item.KorID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };

            return PartialView(inv);
        }

        public ActionResult _DisplayKivOrderDetsIO(int id, string ch)
        {
            var kivOrderDets = new List<KivOrderDet>();
            kivOrderDets = db.KivOrderDets.Where(m => m.KorID == id).ToList();

            var dets = new List<KivOrderDet>();

            if (ch == "IN")
            {
                dets = kivOrderDets.Where(m => m.DeliverOrderQty != 0).ToList();
            }
            else if (ch == "OUT")
            {
                bool flag = kivOrderDets.Any(m => m.DeliverOrderQty == 0);
                if (flag)
                {
                    dets = kivOrderDets.Where(m => m.DeliverQty > 0).ToList();
 
                }
            }

            ViewBag.InOut = ch;
            ViewBag.QuoteNumber = id;

            return PartialView(dets);
        }

        [HttpGet]
        public JsonResult _SubmitKivOrderPreview(int SorID)
        {
            //var item = db.KivOrders.OrderByDescending(i => i.KorNo).FirstOrDefault();
            //int num = 160001;
            //if (item != null && item.KorNo >= num ) {
            //    num = item.KorNo + 1;
            //}

          //  string str = GetMaxKivNumber();

            KivOrder ko = db.KivOrders.Find(SorID);
          //  ko.KorNo = str;
            ko.Status = "Confirmed";
            ko.ModifiedBy = User.Identity.Name;
            ko.ModifiedOn = System.DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(ko).State = EntityState.Modified;
                db.SaveChanges();
            };

            UpdateKivQty(ko.KorID, 1);
          //  UpdateStockQty(ko.KorID, 1);

            return Json(new
            {
                printUrl = Url.Action("KivPrintPreview", "Invoice", new { id = ko.KorID }),
                printUrl3in1 = Url.Action("PrintKIVDelivery3in1", "Invoice", new { id = ko.KorID }),
                redirectUrl = Url.Action("Create", "KivOrders"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult _SubmitKivExchange(int SorID)
        {
            var p = db.KivExchangeOrders.Find(SorID);
            var k = db.KIVs.Where(x => x.InvNo == p.InvRef).FirstOrDefault();

            // add exchanged items to KIVDET
            var list = db.KivOrderDets.Where(x => x.KivEorID == SorID && x.SalesType == "ExchangedItem" && x.RefItemID != 0).ToList();

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = new KIVDET();

                    det.SorID = k.SorID;
                    det.InvID = k.InvID;
                    det.KorID = 0;
                    det.KivEorID = SorID;
                    det.KivID = 0;
                    det.InvType = "KIVEC";
                    det.InvDetID = i.InvDetID;
                    det.RefItemID = i.RefItemID;
                    det.ItemID = i.ItemID;
                    det.ItemCode = i.ItemCode;
                    det.ItemName = i.ItemName;
                    det.Unit = i.Unit;
                    det.OrderQty = 0;
                    det.BalanceQty = i.DeliverQty;
                    det.DeliverQty = 0.00;
                    det.ExchangeQty = 0.00;
                    det.KivBalanceQty = i.DeliverQty;
                    det.SalesType = i.SalesType;
                    det.Position = i.Position;
                    det.Remark = i.Remark;

                    db.Entry(det).State = EntityState.Added;
                };
                db.SaveChanges();
            };

            // add exchanged items to KIV

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var kiv = new KIV();
                    kiv.SorID = k.SorID;
                    kiv.InvID = k.InvID;
                    kiv.InvNo = i.InvNo;
                    kiv.InvDetID = i.InvDetID;
                    kiv.RefItemID = i.RefItemID;
                    kiv.InvDate = k.InvDate;
                    kiv.CustNo = k.CustNo;
                    kiv.CustName = k.CustName;
                    kiv.ProductID = i.ItemID;
                    kiv.ProductName = i.ItemName;
                    kiv.SKU = i.ItemCode;
                    kiv.Unit = i.Unit;
                    kiv.OrderQty = 0;
                    kiv.BalanceQty = i.DeliverQty;
                    kiv.SalesType = i.SalesType;
                    kiv.Position = i.Position;
                    kiv.Remark = i.Remark;
                    kiv.CreatedBy = User.Identity.Name;
                    kiv.CreatedOn = System.DateTime.Now;

                    db.Entry(kiv).State = EntityState.Added;
                };
                db.SaveChanges();
            };

 
            KivExchangeOrder ko = db.KivExchangeOrders.Find(SorID);
            //  ko.KorNo = str;
            ko.Status = "Confirmed";
            ko.ModifiedBy = User.Identity.Name;
            ko.ModifiedOn = System.DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(ko).State = EntityState.Modified;
                db.SaveChanges();
            };

            UpdateKivQtyEor(ko.KivEorID, 1);
            //  UpdateStockQty(ko.KorID, 1);

            return Json(new
            {
                printUrl = Url.Action("KivECPrintPreview", "Invoice", new { id = ko.KivEorID }), 
                redirectUrl = Url.Action("CreateEor", "KivOrders"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult _EditKivOrder(int SorID)
        {
            KivOrder ko = db.KivOrders.Find(SorID);
            //  ko.KorNo = str;
            ko.Status = "Draft";
            ko.ModifiedBy = User.Identity.Name;
            ko.ModifiedOn = System.DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(ko).State = EntityState.Modified;
                db.SaveChanges();
            };

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Edit", "KivOrders", new { id = ko.KorID }),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }

        private void UpdateStockQty(int KorID, int locid)
        {
            var dets = db.KivOrderDets.Where(x => x.KorID == KorID).ToList();
            foreach (var det in dets)
            {
                if (det.SalesType == "DefaultItem")
                {
                    // do stock out

                    UpdateStock_OnKiv(det.ItemID, det.DeliverOrderQty, "OUT");

                }

                if (det.SalesType == "ChangedItem")
                {
                    // do stock out

                    UpdateStock(det.ItemID, det.DeliverQty, "OUT");

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
                    p.StockOut = p.StockOut + qty;
                    p.InStock = p.InStock - qty;
                    p.OnHand = p.OnHand - qty;

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


        private void UpdateKivQty(int KorID, int locid)
        {
            var dets = db.KIVDETs.Where(x => x.KorID == KorID).ToList();
            foreach (var det in dets)
            {
                //var kiv = db.KIVs.Where(x => x.InvDetID == det.InvDetID && x.SKU == det.ItemCode).FirstOrDefault();
                var kiv = db.KIVs.Where(x => x.KivID == det.KivID).FirstOrDefault();
                if (kiv != null) {
                    kiv.BalanceQty = det.KivBalanceQty;
                    kiv.ModifiedBy = User.Identity.Name;
                    kiv.ModifiedOn = System.DateTime.Now;

                    if (ModelState.IsValid)
                    {
                        db.Entry(kiv).State = EntityState.Modified;
                        db.SaveChanges();
                    };
                }
            }

            var items = db.KivOrderDets.Where(x => x.KorID == KorID).ToList();
            foreach (var det in items)
            {
                if (det.SalesType == "DefaultItem")
                {
                    // do stock out

                    UpdateStock_OnKiv(det.ItemID, det.DeliverOrderQty, "OUT");

                }

                if ((det.SalesType == "DefaultItem") && (det.ChangedQty < 0))
                {
                    // do stock out

                    double qty = 0 - det.ChangedQty;

                    UpdateStock(det.ItemID, qty, "IN");

                }

                if (det.SalesType == "ChangedItem")
                {
                    // do stock out

                    UpdateStock(det.ItemID, det.DeliverQty, "OUT");

                }
            }

        }

        private void UpdateKivQtyEor(int KorID, int locid)
        {
            var dets = db.KIVDETs.Where(x => x.KivEorID == KorID).ToList();
            foreach (var det in dets)
            {
                //var kiv = db.KIVs.Where(x => x.InvDetID == det.InvDetID && x.SKU == det.ItemCode && x.RefItemID == det.RefItemID).FirstOrDefault();
                var kiv = db.KIVs.Where(x => x.KivID == det.KivID).FirstOrDefault();
                
                if (kiv != null)
                {
                    kiv.BalanceQty = det.KivBalanceQty;
                    kiv.ModifiedBy = User.Identity.Name;
                    kiv.ModifiedOn = System.DateTime.Now;

                    if (ModelState.IsValid)
                    {
                        db.Entry(kiv).State = EntityState.Modified;
                        db.SaveChanges();
                    };
                }
            }

            var items = db.KivOrderDets.Where(x => x.KivEorID == KorID).ToList();
            foreach (var det in items)
            {
                if (det.SalesType == "DefaultItem")
                {
                    // do kiv stock out 

                    UpdateStock_OnKiv(det.ItemID, det.DeliverOrderQty, "OUT");

                }

                if ((det.SalesType == "DefaultItem") && (det.ChangedQty < 0))
                {
                    // do master stock in

                    double qty = 0 - det.ChangedQty;

                    UpdateStock(det.ItemID, qty, "IN");

                }

                if (det.SalesType == "ExchangedItem" && det.RefItemID != 0)
                {
                    // do master stock out

                    UpdateStock(det.ItemID, det.DeliverQty, "OUT");

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

        public ActionResult _EditDet(int id)
        {
            var p = db.KivOrderDets.Find(id);

            if (p == null)
            {
                return HttpNotFound();
            }

            var product = db.Products.Where(x => x.ProductID == p.ItemID && x.SKU == p.ItemCode).FirstOrDefault();
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

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _EditDet([Bind(Include = "DetID,KivID,InvDetID,KorID,InvID,ItemID,ItemCode,ItemName,OrderQty,BalanceQty,DeliverQty,KivBalanceQty,SalesType,Position,Remark")] KivOrderDet data)
        {
            if (ModelState.IsValid)
            {
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }

            var product = db.Products.Where(x => x.ProductID == data.ItemID && x.SKU == data.ItemCode).FirstOrDefault();
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

        // change color for same model items

        public ActionResult _EditDet1(int id)
        {
            // get order items
            // list same model all products
            // loop through all products and check if the product is within order items
            // if yes, add the product and get existing qty from the order item
            // if no, add the product and set qty = 0

            var p = db.KivOrderDets.Find(id);

            if (p == null)
            {
                return HttpNotFound();
            }

            var detlist = new List<KivOrderDet>();
            detlist.Add(p);

            var product = db.Products.Where(x => x.SKU.ToUpper().Trim() == p.ItemCode.ToUpper().Trim()).FirstOrDefault();
            var productdata = GetProductList();

            var getList = productdata;

            if (product != null && product.ModelNo != null)
            {
                getList = productdata.Where(x => x.ModelNo != null)
                                    .Where(x => x.ModelNo.ToUpper().Trim() == product.ModelNo.ToUpper().Trim())
                                    .ToList().Distinct().OrderBy(x => x.ProductName).Take(100).ToList();

                if (getList == null)
                {
                    getList = productdata.Where(x => x.SKU.ToUpper().Trim() == p.ItemCode.ToUpper().Trim())
                                        .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
                }

            }
            else
            {
                getList = productdata.Where(x => x.SKU.ToUpper().Trim() == p.ItemCode.ToUpper().Trim())
                                    .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
            }

            foreach (var det in getList)
            {
                bool flag = detlist.Any(s => s.ItemCode.ToUpper().Trim() == det.SKU.ToUpper().Trim());

                if (!flag)
                {
                    var i = new KivOrderDet
                    {
                        KorID = p.KorID,
                        KivID = p.KivID,
                        InvID = p.InvID,
                        InvNo = p.InvNo,
                        InvDetID = p.InvDetID,
                        ItemID = det.ProductID,
                        ItemCode = det.SKU,
                        ItemName = det.ProductName,
                        Unit = det.Unit,
                        SellType = "CS",
                        DeliverOrderQty = 0,
                        DeliverQty = 0,
                        ChangedQty = 0,
                        SalesType = "ChangedItem",
                        RefItemID = p.DetID,
                        Position = p.Position + 1,
                        Remark = "Ref:" + p.DetID.ToString() + "- " + p.ItemName
                    };
                    detlist.Add(i);
                }
            }


            //   ViewData["ProductChangeList"] = getList;

            return PartialView(detlist);
        }

        [HttpPost]
        public ActionResult _EditDet1(List<KivOrderDet> list)
        {
            string strKorID = Request.Form["txtKorID3"].Trim();
            int KorID = int.Parse(strKorID);

            //var newlist = list.Where(x => x.DeliverQty > 0).ToList();

            if (list == null)
            {
                return RedirectToAction("Edit", new { id = KorID });
            }

            foreach (var i in list)
            {
                if (i != null)
                {
                    if (i.SalesType != "ChangedItem")
                    {
                        var det = new KivOrderDet();
                        if (ModelState.IsValid)
                        {
                            det.DetID = i.DetID;
                            det.KorID = i.KorID;
                            det.KivID = i.KivID;
                            det.InvID = i.InvID;
                            det.InvNo = i.InvNo;
                            det.InvDetID = i.InvDetID;
                            det.ItemID = i.ItemID;
                            det.ItemCode = i.ItemCode;
                            det.ItemName = i.ItemName;
                            det.Unit = i.Unit;
                            det.DeliverOrderQty = i.DeliverOrderQty;
                            det.DeliverQty = i.DeliverQty;
                            det.ChangedQty = i.DeliverQty - i.DeliverOrderQty;
                            det.SalesType = i.SalesType;
                            det.RefItemID = i.RefItemID;
                            det.Position = i.Position;
                            det.Remark = i.Remark;

                            db.Entry(det).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (i.DeliverQty > 0)
                        {
                            var det = new KivOrderDet();
                            det.DetID = i.DetID;
                            det.KorID = i.KorID;
                            det.KivID = i.KivID;
                            det.InvID = i.InvID;
                            det.InvNo = i.InvNo;
                            det.InvDetID = i.InvDetID;
                            det.ItemID = i.ItemID;
                            det.ItemCode = i.ItemCode;
                            det.ItemName = i.ItemName;
                            det.Unit = i.Unit;
                            det.DeliverOrderQty = i.DeliverOrderQty;
                            det.DeliverQty = i.DeliverQty;
                            det.ChangedQty = i.DeliverQty - i.DeliverOrderQty;
                            det.SalesType = i.SalesType;
                            det.RefItemID = i.RefItemID;
                            det.Position = i.Position;
                            det.Remark = i.Remark;

                            if (ModelState.IsValid)
                            {
                                db.Entry(det).State = EntityState.Added;
                                db.SaveChanges();
                            }
                        }

                    }

                }
            }


            //    UpdateKivDets(SorID);

            return Json(new { success = true });

            //   return RedirectToAction("Edit", new { id = KorID });

        }

        public ActionResult _EditDet1Eor(int id)
        {
            // get order items
            // list same model all products
            // loop through all products and check if the product is within order items
            // if yes, add the product and get existing qty from the order item
            // if no, add the product and set qty = 0

            var p = db.KivOrderDets.Find(id);

            if (p == null)
            {
                return HttpNotFound();
            }

            var detlist = new List<KivOrderDet>();
            detlist.Add(p);

            var product = db.Products.Where(x => x.SKU.ToUpper().Trim() == p.ItemCode.ToUpper().Trim()).FirstOrDefault();
            var productdata = GetProductList();

            var getList = productdata;

            if (product != null && product.ModelNo != null)
            {
                getList = productdata.Where(x => x.ModelNo != null)
                                    .Where(x => x.ModelNo.ToUpper().Trim() == product.ModelNo.ToUpper().Trim())
                                    .ToList().Distinct().OrderBy(x => x.ProductName).Take(100).ToList();

                if (getList == null)
                {
                    getList = productdata.Where(x => x.SKU.ToUpper().Trim() == p.ItemCode.ToUpper().Trim())
                                        .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
                }

            }
            else
            {
                getList = productdata.Where(x => x.SKU.ToUpper().Trim() == p.ItemCode.ToUpper().Trim())
                                    .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
            }

            foreach (var det in getList)
            {
                bool flag = detlist.Any(s => s.ItemCode.ToUpper().Trim() == det.SKU.ToUpper().Trim());

                if (!flag)
                {
                    var i = new KivOrderDet
                    {
                        DetType = "KIVEC",
                        KorID = p.KorID,
                        KivEorID = p.KivEorID,
                        KivID = p.KivID,
                        InvID = p.InvID,
                        InvNo = p.InvNo,
                        InvDetID = p.InvDetID,
                        ItemID = det.ProductID,
                        ItemCode = det.SKU,
                        ItemName = det.ProductName,
                        Unit = det.Unit,
                        SellType = "CS",
                        DeliverOrderQty = 0,
                        DeliverQty = 0,
                        ChangedQty = 0,
                        SalesType = "ExchangedItem",
                        RefItemID = p.DetID,
                        Position = p.Position + 1,
                        Remark = "Ref:" + p.DetID.ToString() + "- " + p.ItemName
                    };
                    detlist.Add(i);
                }
            }


            //   ViewData["ProductChangeList"] = getList;

            return PartialView(detlist);
        }

        [HttpPost]
        public ActionResult _EditDet1Eor(List<KivOrderDet> list)
        {
            string strKorID = Request.Form["txtKorID3"].Trim();
            int KorID = int.Parse(strKorID);

            //var newlist = list.Where(x => x.DeliverQty > 0).ToList();

            if (list == null)
            {
                return RedirectToAction("EditEor", new { id = KorID });
            }

            foreach (var i in list)
            {
                if (i != null)
                {
                    if (i.SalesType == "ExchangedItem" && i.RefItemID != 0)
                    {
                        if (i.DeliverQty > 0)
                        {
                            var det = new KivOrderDet();
                            det.DetID = i.DetID;
                            det.DetType = i.DetType;
                            det.KorID = i.KorID;
                            det.KivEorID = i.KivEorID;
                            det.KivID = i.KivID;
                            det.InvID = i.InvID;
                            det.InvNo = i.InvNo;
                            det.InvDetID = i.InvDetID;
                            det.ItemID = i.ItemID;
                            det.ItemCode = i.ItemCode;
                            det.ItemName = i.ItemName;
                            det.Unit = i.Unit;
                            det.DeliverOrderQty = i.DeliverOrderQty;
                            det.DeliverQty = i.DeliverQty;
                            det.ChangedQty = i.DeliverQty - i.DeliverOrderQty;
                            det.SalesType = i.SalesType;
                            det.RefItemID = i.RefItemID;
                            det.Position = i.Position;
                            det.Remark = i.Remark;

                            if (ModelState.IsValid)
                            {
                                db.Entry(det).State = EntityState.Added;
                                db.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        var det = new KivOrderDet();
                        if (ModelState.IsValid)
                        {
                            det.DetType = i.DetType;
                            det.DetID = i.DetID;
                            det.KorID = i.KorID;
                            det.KivEorID = i.KivEorID;
                            det.KivID = i.KivID;
                            det.InvID = i.InvID;
                            det.InvNo = i.InvNo;
                            det.InvDetID = i.InvDetID;
                            det.ItemID = i.ItemID;
                            det.ItemCode = i.ItemCode;
                            det.ItemName = i.ItemName;
                            det.Unit = i.Unit;
                            det.DeliverOrderQty = i.DeliverOrderQty;
                            det.DeliverQty = i.DeliverQty;
                            det.ChangedQty = i.DeliverQty - i.DeliverOrderQty;
                            det.SalesType = i.SalesType;
                            det.RefItemID = i.RefItemID;
                            det.Position = i.Position;
                            det.Remark = i.Remark;

                            db.Entry(det).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                    }

                }
            }


            //    UpdateKivDets(SorID);

            return Json(new { success = true });

            //   return RedirectToAction("Edit", new { id = KorID });

        }




        public ActionResult _EditDet2(int id)
        {
            // get order items
            // list same model all products
            // loop through all products and check if the product is within order items
            // if yes, add the product and get existing qty from the order item
            // if no, add the product and set qty = 0

            var p = db.KivOrderDets.Find(id);

            if (p == null)
            {
                return HttpNotFound();
            }

            var detlist = new List<KivOrderDet>();
            detlist.Add(p);

            var product = db.Products.Where(x => x.SKU.ToUpper().Trim() == p.ItemCode.ToUpper().Trim()).FirstOrDefault();
            var productdata = GetProductList();

            var getList = productdata;

            if (product != null && product.ModelNo != null)
            {
                getList = productdata.Where(x => x.ModelNo != null)
                                    .Where(x => x.ModelNo.ToUpper().Trim() == product.ModelNo.ToUpper().Trim())
                                    .ToList().Distinct().OrderBy(x => x.ProductName).Take(100).ToList();

                if (getList == null)
                {
                    getList = productdata.Where(x => x.SKU.ToUpper().Trim() == p.ItemCode.ToUpper().Trim())
                                        .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
                }

            }
            else
            {
                getList = productdata.Where(x => x.SKU.ToUpper().Trim() == p.ItemCode.ToUpper().Trim())
                                    .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
            }

            foreach (var det in getList)
            {
                bool flag = detlist.Any(s => s.ItemCode.ToUpper().Trim() == det.SKU.ToUpper().Trim());

                if (!flag)
                {
                    var i = new KivOrderDet
                    {
                        KorID = p.KorID,
                        KivID = p.KivID,
                        InvID = p.InvID,
                        InvNo = p.InvNo,
                        InvDetID = p.InvDetID,
                        ItemID = det.ProductID,
                        ItemCode = det.SKU,
                        ItemName = det.ProductName,
                        Unit = det.Unit,
                        SellType = "CS",
                        DeliverOrderQty = 0,
                        DeliverQty = 0,
                        ChangedQty = 0,
                        SalesType = "ChangedItem",
                        RefItemID = p.DetID,
                        Position = p.Position + 1,
                        Remark = "Ref:" + p.DetID.ToString() + "- " + p.ItemName
                    };
                    detlist.Add(i);
                }
            }


         //   ViewData["ProductChangeList"] = getList;

            return PartialView(detlist);
        }

        [HttpPost]
        public ActionResult _EditDet2(List<KivOrderDet> list)
        {
            string strKorID = Request.Form["txtKorID3"].Trim();
            int KorID = int.Parse(strKorID);

            //var newlist = list.Where(x => x.DeliverQty > 0).ToList();

            if (list == null)
            {
                return RedirectToAction("Edit", new { id = KorID });
            }

            foreach (var i in list)
            {
                if (i != null)
                {
                    if (i.SalesType != "ChangedItem")
                    {
                        var det = new KivOrderDet();
                        if (ModelState.IsValid)
                        {
                            det.DetID = i.DetID;
                            det.KorID = i.KorID;
                            det.KivID = i.KivID;
                            det.InvID = i.InvID;
                            det.InvNo = i.InvNo;
                            det.InvDetID = i.InvDetID;
                            det.ItemID = i.ItemID;
                            det.ItemCode = i.ItemCode;
                            det.ItemName = i.ItemName;
                            det.Unit = i.Unit;
                            det.DeliverOrderQty = i.DeliverOrderQty;
                            det.DeliverQty = i.DeliverQty;
                            det.ChangedQty = i.DeliverQty - i.DeliverOrderQty;
                            det.SalesType = i.SalesType;
                            det.RefItemID = i.RefItemID;
                            det.Position = i.Position;
                            det.Remark = i.Remark;

                            db.Entry(det).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (i.DeliverQty > 0)
                        {
                            var det = new KivOrderDet();
                            det.DetID = i.DetID;
                            det.KorID = i.KorID;
                            det.KivID = i.KivID;
                            det.InvID = i.InvID;
                            det.InvNo = i.InvNo;
                            det.InvDetID = i.InvDetID;
                            det.ItemID = i.ItemID;
                            det.ItemCode = i.ItemCode;
                            det.ItemName = i.ItemName;
                            det.Unit = i.Unit;
                            det.DeliverOrderQty = i.DeliverOrderQty;
                            det.DeliverQty = i.DeliverQty;
                            det.ChangedQty = i.DeliverQty - i.DeliverOrderQty;
                            det.SalesType = i.SalesType;
                            det.RefItemID = i.RefItemID;
                            det.Position = i.Position;
                            det.Remark = i.Remark;

                            if (ModelState.IsValid)
                            {
                                db.Entry(det).State = EntityState.Added;
                                db.SaveChanges();
                            }
                        }

                    }

                }
            }


            //    UpdateKivDets(SorID);

                return Json(new { success = true });

          //   return RedirectToAction("Edit", new { id = KorID });

        }


        public ActionResult _EditDet3(int id)
        {
            var p = db.KivOrderDets.Find(id);

            if (p == null)
            {
                return HttpNotFound();
            }

            var detlist = new List<KivOrderDet>();
         //   detlist.Add(p);

            var product = db.Products.Where(x => x.ProductID == p.ItemID).FirstOrDefault();
            var productdata = GetProductList();

            var getList = productdata;

            if (product != null && product.ModelNo != null)
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

            foreach (var det in getList)
            {
                bool flag = db.KivOrderDets.Any(s => s.ItemCode == det.SKU);

                if (flag)
                {
                    var i = new KivOrderDet
                    {
                        KorID = p.KorID,
                        KivID = p.KivID,
                        InvID = p.InvID,
                        InvNo = p.InvNo,
                        InvDetID = p.InvDetID,
                        ItemID = det.ProductID,
                        ItemCode = det.SKU,
                        ItemName = det.ProductName,
                        Unit = det.Unit,
                        SellType = "CS",
                        DeliverOrderQty = 0,
                        //OrderQty = 0,
                        //BalanceQty = 0,
                        //DeliverQty = 0,
                        //KivBalanceQty = 0,
                        SalesType = "ChangedItem",
                        Position = p.Position + 1,
                        Remark = "Ref:" + p.DetID.ToString() + "- " + p.ItemName
                    };
                    detlist.Add(i);
                }
            }


            //   ViewData["ProductChangeList"] = getList;

            return PartialView(detlist);
        }

        [HttpPost]
        public ActionResult _EditDet3(List<KivOrderDet> list)
        {
            string strKorID = Request.Form["txtKorID3"].Trim();
            int KorID = int.Parse(strKorID);

            //var newlist = list.Where(x => x.DeliverQty > 0).ToList();

            if (list == null)
            {
                return RedirectToAction("Edit", new { id = KorID });
            }

            foreach (var i in list)
            {
                if (i != null)
                {
                    if (i.SalesType != "ChangedItem")
                    {
                        var det = new KivOrderDet();
                        if (ModelState.IsValid)
                        {
                            det.DetID = i.DetID;
                            det.KivID = i.KivID;
                            det.InvID = i.InvID;
                            det.KorID = i.KorID;
                            det.InvDetID = i.InvDetID;
                            det.ItemID = i.ItemID;
                            det.ItemCode = i.ItemCode;
                            det.ItemName = i.ItemName;
                            //det.OrderQty = i.OrderQty;
                            //det.BalanceQty = i.BalanceQty;
                            //det.DeliverQty = i.DeliverQty;
                            //det.KivBalanceQty = i.BalanceQty - i.DeliverQty;
                            det.SalesType = i.SalesType;
                            det.Position = i.Position;
                            det.Remark = i.Remark;

                            db.Entry(det).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (i.DeliverQty > 0)
                        {
                            var det = new KivOrderDet();
                            det.KivID = i.KivID;
                            det.InvID = i.InvID;
                            det.KorID = i.KorID;
                            det.InvDetID = i.InvDetID;
                            det.ItemID = i.ItemID;
                            det.ItemCode = i.ItemCode;
                            det.ItemName = i.ItemName;
                            //det.OrderQty = i.OrderQty;
                            //det.BalanceQty = i.BalanceQty;
                            //det.DeliverQty = i.DeliverQty;
                            //det.KivBalanceQty = 0;
                            det.SalesType = i.SalesType;
                            det.Position = i.Position;
                            det.Remark = i.Remark;

                            if (ModelState.IsValid)
                            {
                                db.Entry(det).State = EntityState.Added;
                                db.SaveChanges();
                            }
                        }

                    }

                }
            }


            //    UpdateKivDets(SorID);

            return Json(new { success = true });

            //   return RedirectToAction("Edit", new { id = KorID });

        }



        public ActionResult _DelDet(int id = 0)
        {
            KivOrderDet det = db.KivOrderDets.Find(id);
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
                var det = db.KivOrderDets.Find(id);
                if (det != null)
                {
                    db.Entry(det).State = EntityState.Deleted;
                    db.SaveChanges();
                }

                trans.Complete();
            }

            return Json(new { success = true });
        }


        public List<ProductSelection> GetProductList()
        {
            List<ProductSelection> getList = (from p in db.Products
                                              where p.IsActive == true
                                              select new ProductSelection
                                              {
                                                  ProductID = p.ProductID,
                                                  SKU = p.SKU,
                                                  Barcode = p.Barcode,
                                                  ProductType = p.ProductType,
                                                  ProductName = p.ProductName,
                                                  ModelNo = p.ModelNo,
                                                  IsBundle = p.IsBundle,
                                                  Unit = p.Unit,
                                                  CostPrice = p.CostPrice,
                                                  SellPrice = p.RetailPrice,
                                                  IsControlItem = p.IsControlItem,
                                                  AvailableQty = 0

                                              }).ToList();

            return getList;

        }


        public JsonResult _GetInvoiceNos(string custno, string invtype)
        {
            if (custno != null)
            {
                int cid = Convert.ToInt32(custno);
                var kivs = db.KIVs.Where(x => x.CustNo == cid && x.InvType == invtype).ToList();

                var c = kivs.Select(i => i.InvNo).ToList().Distinct();

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


        // KIV Exchange Order module

        public ActionResult CreateEor()
        {
            var p = new KivExchangeOrder();

            ViewData["ClientsAll"] = GetClientListByUser("ALL");

            //  ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(p);
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEor([Bind(Include = "KivEorID,KivEorNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,DeliveryAddress,DeliveryDate,DeliveryTime,PaymentTerms,Status,Remark,PersonID,PersonName")] KivExchangeOrder inv)
        {
            string str = GetMaxKivEorNumber();
            inv.KivEorNo = str;
            inv.CreatedBy = User.Identity.Name;
            inv.CreatedOn = DateTime.Now;

            Boolean flag = false;

            if (ModelState.IsValid)
            {
                db.KivExchangeOrders.Add(inv);
                db.SaveChanges();

                flag = true;
            };

            if (flag)
            {
                CreateKivDets(inv.KivEorID, inv.CustNo, inv.InvRef,"KIVEC");

                return RedirectToAction("EditEor", new { id = inv.KivEorID, str = "1" });
            }

            ViewBag.Message = "1";

            ViewData["ClientsAll"] = GetClientListByUser("ALL");
            //     ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(inv);
        }

        public ActionResult EditEor(int? id, string str)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KivExchangeOrder inv = db.KivExchangeOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            ViewBag.PageFrom = str;

            return View(inv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEor([Bind(Include = "KivEorID,KivEorNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PaymentTerms,Remark,PersonID,PersonName,CreatedOn")] KivExchangeOrder inv)
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
                    return RedirectToAction("CreateEor", "KivOrders");
                };

                return RedirectToAction("EditEor", new { id = inv.KivEorID });
            }

            ViewData["ClientsAll"] = GetClientListByUser("ALL");
            //   ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            return View(inv);
        }

        public ActionResult ViewEor(int? id, string str)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KivExchangeOrder inv = db.KivExchangeOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            if (inv != null)
            {
                ViewBag.InvoiceNo = inv.KivEorID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };

            ViewData["ClientsAll"] = GetClientListByUser("ALL");
            //   ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            ViewBag.PageFrom = str;

            return View(inv);
        }

        public ActionResult KivHistory(int id)
        {
            ViewBag.KivId = id;
            return View();
        }

        public JsonResult DrOrderDetail(int id)
        {
            var result = (from a in db.DrOrderDets
                          join b in db.KivOrderDets
                          on a.KivOrderDetID equals b.DetID
                          join c in db.DrOrders
                          on a.DorID equals c.Id
                          where b.KivID == id
                          select new
                          {
                              OrderNO = c.DorNo,
                              OrderDate = c.InvDate,
                              Sku = a.ItemCode,
                              ProductName = a.ItemName,
                              RequestQty = b.DeliverOrderQty,
                              Qty = a.DeliverQty,
                              Remark = a.Remark,
                              Status=c.Status
                          }).ToList().Select(r => new
                          {
                              OrderNO = r.OrderNO,
                              OrderDate = r.OrderDate.ToString("dd/MM/yyyy"),
                              Sku = r.Sku,
                              ProductName = r.ProductName,
                              RequestQty = r.RequestQty,
                              Qty = r.Qty,
                              Remark = r.Remark,
                              Status=r.Status ?? "Draft"
                          }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
                       
        }
        public ActionResult IndexDrSubmit()
        {
            DateTime datefrom = DateTime.Now.AddMonths(-12);
            //var result = db.KivOrders.Where(x => x.InvDate >= datefrom && x.Status == "Confirmed" && (x.Status2 == null || x.Status2 != "Completed")).Take(600).OrderByDescending(x => x.KorID).ToList();
            var result = db.KivOrders.Where(x => x.InvType == "PRO" && x.InvDate >= datefrom && x.Status == "Confirmed").Take(600).OrderByDescending(x => x.KorID).ToList();
            ViewBag.PageFrom = "DrSubmission";
            return View(result);
        }

        public JsonResult _ConvertToDeliveryOrder(List<KivOrderDet> korDets)
        {
            decimal gstrate = GetGstRate();
            int detId = 0;
            double BalanceQty = 0;
            string msgErr = "The Delivery Request is not found.No valid data.Please refresh page.";
            if (korDets.Count > 0)
            {
                int korId = korDets.FirstOrDefault().KorID;
                KivOrder kivOrder = db.KivOrders.Find(korId);

                string newno = "";
                var sp = db.Staffs.Find(kivOrder.PersonID);
                if (sp != null && !string.IsNullOrEmpty(sp.DepartmentName))
                {
                    if (sp.DepartmentName == "SR")
                    {
                        newno = GetSerialNumber(207);
                    }
                    else
                    {
                        newno = GetSerialNumber(107);
                    }
                }
                else
                {
                    newno = GetSerialNumber(107);
                }


                DrOrder drOrder = new DrOrder
                {

                    DorNo = newno,

                    KivOrderId = kivOrder.KorID,
                    InvType = kivOrder.InvType,
                    InvDate = DateTime.Now.Date,
                    //PoNo,
                    InvRef = kivOrder.InvRef,
                    CustNo = kivOrder.CustNo,
                    CustName = kivOrder.CustName,
                    CustName2 = kivOrder.CustName2,
                    Addr1 = kivOrder.Addr1,
                    Addr2 = kivOrder.Addr2,
                    Addr3 = kivOrder.Addr3,
                    Addr4 = kivOrder.Addr4,
                    Attn = kivOrder.Attn,
                    PhoneNo = kivOrder.PhoneNo,
                    FaxNo = kivOrder.FaxNo,
                    DeliveryAddress = kivOrder.DeliveryAddress,
                    DeliveryDate = kivOrder.DeliveryDate,
                    DeliveryTime = kivOrder.DeliveryTime,
                    PaymentTerms = kivOrder.PaymentTerms,
                    Status = "Draft",
                    LocationID = kivOrder.LocationID,
                    LocationName = kivOrder.LocationName,
                    Remark = kivOrder.Remark,
                    PersonID = kivOrder.PersonID,
                    PersonName = kivOrder.PersonName,
                    CreatedBy = User.Identity.Name,
                    CreatedOn = DateTime.Now
                };
                List<DrOrderDet> drOrderDets = new List<DrOrderDet>();
                foreach (var item in korDets)
                {
                    #region
                    detId = item.DetID;
                    BalanceQty = item.BalanceQty;
                    var vDet = (from a in db.KivOrderDets
                                join b in db.INVDETs
                                on a.InvDetID equals b.DetID
                                where (a.DetID == detId && a.BalanceQty > 0)
                                select new
                                {
                                    a.DetID,
                                    b.DetType,
                                    a.InvDetID,
                                    a.ItemID,
                                    a.ItemCode,
                                    a.ItemName,
                                    a.SellType,
                                    a.Unit,
                                    a.BalanceQty,
                                    b.ItemType,
                                    b.ItemDesc,
                                    b.UnitPrice,
                                    b.DiscountedPrice,
                                    b.UnitCostPrice,
                                    b.PreDiscAmount,
                                    b.Discount,
                                    b.Amount,
                                    b.Gst,
                                    b.Nett,
                                    b.IsBundle,
                                    b.SalesType,
                                    b.RefItemID,
                                    b.Remark
                                }).FirstOrDefault();
                    if (vDet == null)
                    {
                        msgErr = "Data No Found!";
                        goto EndAction;
                    }
                    if (vDet.BalanceQty < BalanceQty)
                    {
                        msgErr = "Deliver Qty can not greater than Balance Qty, please check again.";
                        goto EndAction;
                    }
                    DrOrderDet det = new DrOrderDet
                    {
                        DetType = vDet.DetType,
                        InvDetID = vDet.InvDetID,
                        KivOrderDetID = vDet.DetID,
                        ItemID = vDet.ItemID,
                        ItemCode = vDet.ItemCode,
                        ItemType = vDet.ItemType,
                        ItemName = vDet.ItemName,
                        ItemDesc = vDet.ItemDesc,
                        SellType = vDet.SellType,
                        Unit = vDet.Unit,
                        DeliverQty = BalanceQty,//vDet.BalanceQty,
                        UnitPrice = vDet.UnitPrice,
                        DiscountedPrice = vDet.DiscountedPrice,
                        UnitCostPrice = vDet.UnitCostPrice,
                        PreDiscAmount = vDet.PreDiscAmount,
                        Discount = vDet.Discount,
                        //Amount = vDet.DiscountedPrice * (decimal)BalanceQty,//item.Amount,
                        //Gst = vDet.Gst,
                        //Nett = vDet.Nett,

                        Amount = System.Math.Round((vDet.DiscountedPrice * Convert.ToDecimal(BalanceQty)), 2, MidpointRounding.AwayFromZero),
                        Gst = 0,
                        Nett = System.Math.Round((vDet.DiscountedPrice * Convert.ToDecimal(BalanceQty)), 2, MidpointRounding.AwayFromZero),

                        IsBundle = vDet.IsBundle,
                        SalesType = vDet.SalesType,
                        RefItemID = vDet.RefItemID,
                        Position = drOrderDets.Count + 1,
                        Remark = vDet.Remark
                    };
                    //if (det.Gst >= 0)
                    //{
                    //    det.Gst = det.Amount * 0.07M;
                    //    det.Nett = det.Amount + det.Gst;
                    //}
                        det.Gst = System.Math.Round((det.Amount * gstrate), 2, MidpointRounding.AwayFromZero);
                        det.Nett = det.Amount + det.Gst;
                    drOrderDets.Add(det);
                    #endregion
                }
                drOrder.DrOrderDets = drOrderDets;
                drOrder.PreDiscAmount = drOrderDets.Sum(d => d.Amount);
                drOrder.Discount = 0;
                drOrder.Amount = drOrder.PreDiscAmount;
                //drOrder.Gst= drOrderDets.Sum(d => d.Gst);
                //drOrder.Nett = drOrderDets.Sum(d => d.Nett);


                drOrder.Gst = System.Math.Round(drOrder.Amount * gstrate, 2, MidpointRounding.AwayFromZero);
                drOrder.Nett = drOrder.Amount + drOrder.Gst;
                db.DrOrders.Add(drOrder);
                db.SaveChanges();
                string newUrl = "/DrOrder/Details/" + drOrder.Id.ToString();

                return Json(new
                {
                    //printUrl = Url.Action("PrintPreview", "Invoice", new { id = 1 }),
                    redirectUrl = newUrl,
                    isRedirect = true
                }, JsonRequestBehavior.AllowGet);
            }
        //int KorId = 0;
        /*KivOrder kivOrder = db.KivOrders.Find(KorId);
        if (kivOrder == null || kivOrder.Status != "Confirmed" || (kivOrder.Status2 != null && kivOrder.Status2 == "Completed"))
        {

        }
        else
        {
                
            var dets = (from a in db.KivOrderDets
                        join b in db.INVDETs
                        on a.InvDetID equals b.DetID
                        where (a.KorID == kivOrder.KorID && a.BalanceQty>0)
                        select new {a.DetID,a.DetType , a.InvDetID,a.ItemID,a.ItemCode,a.ItemName,a.SellType, a.Unit,a.BalanceQty,
                        b.UnitPrice,b.DiscountedPrice,b.UnitCostPrice,b.PreDiscAmount,b.Discount,b.Amount,b.Gst,b.Nett,b.IsBundle,b.SalesType,b.RefItemID,b.Remark
                        }).ToList();
                //db.KivOrderDets.Where(d => d.KorID == kivOrder.KorID && d.DeliverQty > d.CompletedQty).ToList();
            if (dets.Count > 0)
            {
                DrOrder drOrder = new DrOrder
                {
                    DorNo = GetSerialNumber(11),
                    InvType = kivOrder.InvType,
                    InvDate = DateTime.Now,
                    //PoNo,
                    InvRef = kivOrder.InvRef,
                    CustNo = kivOrder.CustNo,
                    CustName = kivOrder.CustName,
                    CustName2 = kivOrder.CustName2,
                    Addr1 = kivOrder.Addr1,
                    Addr2 = kivOrder.Addr2,
                    Addr3 = kivOrder.Addr3,
                    Addr4 = kivOrder.Addr4,
                    Attn = kivOrder.Attn,
                    PhoneNo = kivOrder.PhoneNo,
                    FaxNo = kivOrder.FaxNo,
                    DeliveryAddress = kivOrder.DeliveryAddress,
                    DeliveryDate = kivOrder.DeliveryDate,
                    DeliveryTime = kivOrder.DeliveryTime,
                    PaymentTerms = kivOrder.PaymentTerms,
                    Status = "Draft",
                    LocationID = kivOrder.LocationID,
                    LocationName = kivOrder.LocationName,
                    Remark = kivOrder.Remark,
                    PersonID = kivOrder.PersonID,
                    PersonName = kivOrder.PersonName,
                    CreatedBy = User.Identity.Name,
                    CreatedOn = DateTime.Now
                };
                List<DrOrderDet> drOrderDets = new List<DrOrderDet>();
                foreach (var item in dets)
                {
                    DrOrderDet det = new DrOrderDet
                    {
                        DetType = item.DetType,
                        InvDetID = item.InvDetID,
                        KivOrderDetID = item.DetID,
                        ItemID = item.ItemID,
                        ItemCode = item.ItemCode,
                        ItemName = item.ItemName,
                        SellType = item.SellType,
                        Unit = item.Unit,
                        DeliverQty = item.BalanceQty,
                        UnitPrice = item.UnitPrice,
                        DiscountedPrice = item.DiscountedPrice,
                        UnitCostPrice = item.UnitCostPrice,
                        PreDiscAmount = item.PreDiscAmount,
                        Discount = item.Discount,
                        Amount = item.UnitPrice * (decimal)item.BalanceQty,//item.Amount,
                        Gst = item.Gst,
                        Nett = item.Nett,
                        IsBundle = item.IsBundle,
                        SalesType = item.SalesType,
                        RefItemID = item.RefItemID,
                        Position = drOrderDets.Count + 1,
                        Remark = item.Remark
                    };
                    if (det.Gst>0)
                    {
                        det.Gst = det.Amount *  0.07M;
                        det.Nett = det.Amount + det.Gst;
                    }
                    drOrderDets.Add(det);
                }
                drOrder.DrOrderDets = drOrderDets;
                db.DrOrders.Add(drOrder);
                db.SaveChanges();
                string newUrl = "/DrOrder/Details/"+ drOrder.Id.ToString();
                    
                return Json(new
                {
                    //printUrl = Url.Action("PrintPreview", "Invoice", new { id = 1 }),
                    redirectUrl = newUrl,
                    isRedirect = true
                }, JsonRequestBehavior.AllowGet);
            }
        }*/
        EndAction:
            return Json(new { success = false, responseText = msgErr }, JsonRequestBehavior.AllowGet);

        }

    }
}