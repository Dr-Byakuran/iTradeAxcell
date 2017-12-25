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
    public class PoKivOrdersController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();
        private InvoiceClass invoice = new InvoiceClass();

        // GET: PoKivOrders
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult IndexKor()
        {
            return View();

        }

        // [ChildActionOnly] 
        public ActionResult _DisplayResults(int id)
        {
            //   string str = "";
            switch (id)
            {
                case 0:
                    //     str = "";
                    ViewBag.TableNo = 0;
                    break;


            };

            var p = new List<PoKivOrder>();

            p = db.PoKivOrders.OrderByDescending(x => x.KorID).ToList();


            return PartialView(p);
        }

        public ActionResult _DisplayKivs(int id)
        {
            var p = db.PoKIVs.OrderByDescending(x => x.InvNo).ThenBy(x => x.CustName).ToList();

            if (id != 0)
            {
                p = db.PoKIVs.Where(x => x.CustNo == id).OrderBy(x => x.InvNo).ToList();
            }

            //ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _DisplayKivsByCust(int id)
        {
            var p = db.PoKIVs.Where(x => x.CustNo == 0).OrderBy(x => x.CustName).ThenBy(x => x.InvNo).ToList();

            if (id != 0)
            {
                p = db.PoKIVs.Where(x => x.CustNo == id).OrderBy(x => x.InvNo).ToList();
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
            PoKivOrder inv = db.PoKivOrders.Find(id);
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
            PoKivOrder inv = db.PoKivOrders.Find(id);
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
            var p = new List<PoKivOrderDet>();
            p = db.PoKivOrderDets
                .Where(x => (x.KorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        public ActionResult _DisplayInvDetsView(int id, string act)
        {
            var p = new List<PoKivOrderDet>();
            p = db.PoKivOrderDets
                .Where(x => (x.KorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }


        public ActionResult _DisplayInvDetsPrint(int id)
        {
            var p = new List<PoKivOrderDet>();
            p = db.PoKivOrderDets
                .Where(x => (x.KorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _DisplayKivDets(int id, string act)
        {
            var p = new List<PoKIVDET>();
            p = db.PoKIVDETs
                .Where(x => (x.KorID == id))
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

            int KorID = list.FirstOrDefault().KorID;
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

            UpdatePoKivOrderDets(KorID);

            //    UpdateKivDets(SorID);

            //    return Json(new { success = true });

            return RedirectToAction("Edit", new { id = KorID });

        }

        private void UpdatePoKivOrderDets(int KorID)
        {
            // get list from kivdets
            // loop list and check if kivdet is in PoKivOrderdets
            // if qty = 0, remove items from PoKivOrderdets
            // if true, update deliver order qty and calculate changed qty
            // if false, add item to PoKivOrderdets, set default delivery qty = deliver order qty

            var kor = db.PoKivOrders.Find(KorID);

            var list = db.PoKIVDETs.Where(x => x.KorID == KorID).ToList();
            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.PoKivOrderDets.Where(x => x.KorID == KorID && x.InvDetID == i.InvDetID).FirstOrDefault();
                    if (det != null)
                    {
                        det.DeliverOrderQty = i.DeliverQty;
                        det.ChangedQty = i.DeliverQty - det.DeliverQty;

                    }
                    else
                    {
                        det = new PoKivOrderDet();
                        det.KorID = KorID;
                        det.KivID = 0;
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

            var odets = db.PoKivOrderDets.Where(x => x.KorID == KorID).ToList();
            foreach (var od in odets)
            {
                if (od.DeliverOrderQty == 0)
                {
                    // remove 
                    var subdets = db.PoKivOrderDets.Where(x => x.KorID == KorID && x.RefItemID == od.DetID && x.InvDetID == od.InvDetID).ToList();
                    foreach (var subdet in subdets)
                    {
                        if (subdet != null)
                        {
                            db.PoKivOrderDets.Remove(subdet);
                            db.SaveChanges();
                        }
                    }

                    db.PoKivOrderDets.Remove(od);
                    db.SaveChanges();

                }

            }


        }




        public ActionResult _DisplayKivDetsPrint(int id)
        {
            var p = new List<PoKIVDET>();
            p = db.PoKIVDETs
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
        //                join t2 in db.PoKIVs on t1.InvID equals t2.InvID
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
            var p = new PoKivOrder();

            //  p.SarOn.DateCreated = DateTime.Now;
            //  ViewBag.ProductTypes = db.ProductGroups.ToList();

            ViewData["ClientsAll"] = GetVendorListByUser("ALL");

          //  ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(p);
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "KorID,KorNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,DeliveryAddress,DeliveryDate,DeliveryTime,PaymentTerms,Status,Remark,PersonID,PersonName")] PoKivOrder inv)
        {
            string str = GetMaxPoKivNumber();
            inv.KorNo = str;
            inv.CreatedBy = User.Identity.Name;
            inv.CreatedOn = DateTime.Now;

            Boolean flag = false;

            if (ModelState.IsValid)
            {
                db.PoKivOrders.Add(inv);
                db.SaveChanges();

                flag = true;
            };

            if (flag)
            {
               CreateKivDets(inv.KorID, inv.CustNo, inv.InvRef);

                return RedirectToAction("Edit", new { id = inv.KorID, str = "1" });
            }

            ViewBag.Message = "1";

            ViewData["ClientsAll"] = GetVendorListByUser("ALL");
       //     ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(inv);
        }

        [HttpGet]
        public JsonResult CreateByInvNo(string id)
        {
            var inv = db.PoINVs.Where(x => x.InvNo == id).FirstOrDefault();
            var ko = new PoKivOrder();
            Boolean flag = false;

            if (inv == null)
            {
                return Json(new { success = false, responseText = "The invoice number# " + id + " not in the system." }, JsonRequestBehavior.AllowGet);

            } 
            else
            {
                string str = GetMaxPoKivNumber();
                ko.KorNo = str;
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
                    db.PoKivOrders.Add(ko);
                    db.SaveChanges();

                    flag = true;
                };
            }


            if (flag)
            {
                CreateKivDets(ko.KorID, ko.CustNo, ko.InvRef);

            }

            return Json(new { success = true, redirectUrl = Url.Action("Edit", "PoKivOrders", new { id = ko.KorID, str = "1" }) }, JsonRequestBehavior.AllowGet);
 
        }


        private void CreateKivDets(int KorID, int custno, string invref)
        {
            var list = db.PoKIVs.Where(x => x.CustNo == custno && x.InvNo == invref).ToList();

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                        var det = new PoKIVDET();
                        det.SorID = i.SorID;
                        det.InvID = i.InvID;
                        det.KorID = KorID;
                        det.KivID = 0;
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
            PoKivOrder inv = db.PoKivOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            ViewBag.PageFrom = str;

            return View(inv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "KorID,KorNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PaymentTerms,Remark,PersonID,PersonName,CreatedOn")] PoKivOrder inv)
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
                    return RedirectToAction("Create", "PoKivOrders");
                };

                return RedirectToAction("Edit", new { id = inv.KorID });
            }

            ViewData["ClientsAll"] = GetVendorListByUser("ALL");
         //   ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
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
            PoKivOrder inv = db.PoKivOrders.Find(id);
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

            ViewData["ClientsAll"] = GetVendorListByUser("ALL");
         //   ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            ViewBag.PageFrom = str;

            return View(inv);
        }

        
        public ActionResult _KivListByCust(int id)
        {
            var kor = db.PoKivOrders.Find(id);

            if (kor == null)
            {
                return HttpNotFound();
            }
            var list = db.PoKivOrderDets.Where(x => x.KorID == id).ToList();
            var detlist = new List<PoKivOrderDet>();
            //foreach (var li in list)
            //{
            //    int detid = li.DetID;
            //    detlist.Add(li);
            //}


            int custno = kor.CustNo;

            var dets = db.PoKIVs.Where(x => x.CustNo == custno && x.InvNo == kor.InvRef).ToList();

            //var dets = (from t1 in db.INVs
            //            join t2 in db.PoKIVs on t1.InvID equals t2.InvID
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
                    var i = new PoKivOrderDet
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


            //p = db.PoKivOrderDets
            //    .Where(x => (x.KorID == id))
            //    .OrderBy(x => x.Position)
            //    .ToList();

            ViewBag.KorNumber = id;

            return PartialView(detlist);
        }

        [HttpPost]
        public ActionResult _KivListByCust(List<PoKivOrderDet> list)
        {
            string strKorID = Request.Form["txtKorID2"];
            int KorID = int.Parse(strKorID);

            //var newlist = list.Where(x => x.DeliverQty > 0).ToList();

            if (list == null)
            {
                return RedirectToAction("Edit", new { id = KorID });
            }

            foreach (var i in list)
            {
                if (i != null && i.DeliverQty > 0)
                {
                    var det = new PoKivOrderDet();
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


            //    UpdateKivDets(SorID);

            //    return Json(new { success = true });

            return RedirectToAction("Edit", new { id = KorID });

        }



        public ActionResult _DisplayKivOrderDets(int id)
        {
            var kor = db.PoKivOrders.Find(id);

            if (kor == null)
            {
                return HttpNotFound();
            }
            var list = db.PoKivOrderDets.Where(x => x.KorID == id).ToList();
            //var detlist = new List<PoKivOrderDet>();
            //foreach (var li in list)
            //{
            //    int detid = li.DetID;
            //    detlist.Add(li);
            //}


            //int custno = kor.CustNo;

            //var dets = (from t1 in db.INVs
            //            join t2 in db.PoKIVs on t1.InvID equals t2.InvID
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
            //        var i = new PoKivOrderDet
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


            //p = db.PoKivOrderDets
            //    .Where(x => (x.KorID == id))
            //    .OrderBy(x => x.Position)
            //    .ToList();

            ViewBag.KorNumber = id;

            return PartialView(list);
        }

        [HttpPost]
        public ActionResult _DisplayKivOrderDets(List<PoKivOrderDet> list)
        {
            string strKorID = Request.Form["txtKorID"];
            int KorID = int.Parse(strKorID);

            //var newlist = list.Where(x => x.DeliverQty > 0).ToList();

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.PoKivOrderDets.Where(x => x.KorID == i.KorID && x.InvID == i.InvID && x.InvDetID == i.InvDetID).FirstOrDefault();
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
                        det = new PoKivOrderDet();
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

            //var dets = db.PoKivOrderDets.Where(x => x.KorID == KorID).ToList();

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
            var p = new List<PoKivOrderDet>();
            p = db.PoKivOrderDets
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
            PoKivOrder inv = db.PoKivOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            var item = db.PoKivOrders.Where(x => x.KorID == id).FirstOrDefault();
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

        [HttpGet]
        public JsonResult _SubmitKivOrderPreview(int SorID)
        {
            //var item = db.PoKivOrders.OrderByDescending(i => i.KorNo).FirstOrDefault();
            //int num = 160001;
            //if (item != null && item.KorNo >= num ) {
            //    num = item.KorNo + 1;
            //}

          //  string str = GetMaxKivNumber();

            PoKivOrder ko = db.PoKivOrders.Find(SorID);
          //  ko.KorNo = str;
            ko.Status = "Confirmed";
            ko.ModifiedBy = User.Identity.Name;
            ko.ModifiedOn = System.DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(ko).State = EntityState.Modified;
                db.SaveChanges();
            };

            UpdatePoKivQty(ko.KorID, 1);
          //  UpdateStockQty(ko.KorID, 1);

            return Json(new
            {
                printUrl = Url.Action("KivPrintPreview", "Invoice", new { id = ko.KorID }),
                redirectUrl = Url.Action("Create", "PoKivOrders"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }

        private void UpdateStockQty(int KorID, int locid)
        {
            var dets = db.PoKivOrderDets.Where(x => x.KorID == KorID).ToList();
            foreach (var det in dets)
            {
                if (det.SalesType == "DefaultItem")
                {
                    // do stock out

                    UpdateStock_OnPoKiv(det.ItemID, det.DeliverOrderQty, "OUT");

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


        private void UpdatePoKivQty(int KorID, int locid)
        {
            var dets = db.PoKIVDETs.Where(x => x.KorID == KorID).ToList();
            foreach (var det in dets)
            {
                var kiv = db.PoKIVs.Where(x => x.InvDetID == det.InvDetID && x.SKU == det.ItemCode).FirstOrDefault();
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

            var items = db.PoKIVDETs.Where(x => x.KorID == KorID).ToList();
            foreach (var det in items)
            {
                if ((det.SalesType == "DefaultItem") && (det.DeliverQty > 0))
                {
                    // do stock out

                    UpdateStock_OnPoKiv(det.ItemID, det.DeliverQty, "OUT");

                }

                if ((det.SalesType == "DefaultItem") && (det.DeliverQty > 0))
                {
                    // do stock in

                    double qty = det.DeliverQty;

                    UpdateStock(det.ItemID, qty, "IN");

                }

                //if (det.SalesType == "ChangedItem")
                //{
                //    // do stock out

                //    UpdateStock(det.ItemID, det.DeliverQty, "OUT");

                //}
            }

        }

        private void UpdateStock_OnPoKiv(int productId, double qty, string processType)
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

        public ActionResult _EditDet(int id)
        {
            var p = db.PoKivOrderDets.Find(id);

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
        public ActionResult _EditDet([Bind(Include = "DetID,KivID,InvDetID,KorID,InvID,ItemID,ItemCode,ItemName,OrderQty,BalanceQty,DeliverQty,KivBalanceQty,SalesType,Position,Remark")] PoKivOrderDet data)
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

            var p = db.PoKivOrderDets.Find(id);

            if (p == null)
            {
                return HttpNotFound();
            }

            var detlist = new List<PoKivOrderDet>();
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
                    var i = new PoKivOrderDet
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
        public ActionResult _EditDet1(List<PoKivOrderDet> list)
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
                        var det = new PoKivOrderDet();
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
                            var det = new PoKivOrderDet();
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


        public ActionResult _EditDet2(int id)
        {
            // get order items
            // list same model all products
            // loop through all products and check if the product is within order items
            // if yes, add the product and get existing qty from the order item
            // if no, add the product and set qty = 0

            var p = db.PoKivOrderDets.Find(id);

            if (p == null)
            {
                return HttpNotFound();
            }

            var detlist = new List<PoKivOrderDet>();
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
                    var i = new PoKivOrderDet
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
        public ActionResult _EditDet2(List<PoKivOrderDet> list)
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
                        var det = new PoKivOrderDet();
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
                            var det = new PoKivOrderDet();
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
            var p = db.PoKivOrderDets.Find(id);

            if (p == null)
            {
                return HttpNotFound();
            }

            var detlist = new List<PoKivOrderDet>();
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
                bool flag = db.PoKivOrderDets.Any(s => s.ItemCode == det.SKU);

                if (flag)
                {
                    var i = new PoKivOrderDet
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
        public ActionResult _EditDet3(List<PoKivOrderDet> list)
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
                        var det = new PoKivOrderDet();
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
                            var det = new PoKivOrderDet();
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
            PoKivOrderDet det = db.PoKivOrderDets.Find(id);
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
                var det = db.PoKivOrderDets.Find(id);
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


        public JsonResult _GetInvoiceNos(string custno)
        {
            if (custno != null)
            {
                int cid = Convert.ToInt32(custno);
                var kivs = db.PoKIVs.Where(x => x.CustNo == cid).ToList();

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



    }
}