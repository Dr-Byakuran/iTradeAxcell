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
    public class QuotationController : ControllerBase
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

        // GET: Quotation
        public ActionResult Index()
        {
            return View();
        }

        // [ChildActionOnly] 
        public ActionResult _DisplayResults(string invtype, string invstatus)
        {
            DateTime datefrom = DateTime.Now.AddMonths(-12);

            var p = new List<Quotation>();
            if (invstatus == "")
            {
                p = db.Quotations.Where(x => x.InvType == invtype && x.InvDate >= datefrom).Take(600).OrderByDescending(x => x.QuoID).ToList();
                ViewBag.TableNo = 0;
            };
            if (invstatus == "Win")
            {
                p = db.Quotations.Where(x => x.InvType == invtype && x.InvDate >= datefrom && x.Status == "Win").Take(600).OrderByDescending(x => x.QuoID).ToList();
                ViewBag.TableNo = 1;
            };
            if (invstatus == "Pending Approval")
            {
                p = db.Quotations.Where(x => x.InvType == invtype && x.InvDate >= datefrom && x.Status == "Pending Approval").Take(600).OrderByDescending(x => x.QuoID).ToList();
                ViewBag.TableNo = 2;
            };
            if (invstatus == "Draft" || invstatus == "Rejected")
            {
                p = db.Quotations.Where(x => x.InvType == invtype && x.InvDate >= datefrom && (x.Status == "Draft" || x.Status == "Rejected")).OrderByDescending(x => x.QuoID).Take(10).ToList();
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


        public ActionResult View(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quotation inv = db.Quotations.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            //ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            //ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            ViewData["seGSTRate"] = GetGstRate();
            return View(inv);
        }

        // GET: Sales/Create
        public ActionResult Create()
        {
            var p = new Quotation();

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
        public ActionResult Create([Bind(Include = "QuoID,QuoNo,SorNo,InvNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,PhoneNo,FaxNo,DeliveryAddress,DeliveryDate,DeliveryTime,PaymentTerms,Status,Remark,PersonID,PersonName")] Quotation inv)
        {
            string str = GetMaxQuotationNumber();
         //   string str = invoice.GetInvoiceNumber(InvType.QN.ToString(), DateTime.Now, User.Identity.Name);

            inv.QuoNo = str;

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
                db.Quotations.Add(inv);
                db.SaveChanges();

                flag = true;
            };

            if (flag)
            {
                //CreateKIV(inv.SorID);

                return RedirectToAction("Edit", new { id = inv.QuoID, str = "1" });
            }

            ViewBag.Message = "1";

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
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
            Quotation inv = db.Quotations.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
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
        public ActionResult Edit([Bind(Include = "QuoID,QuoNo,SorNo,InvNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PreDiscAmount,Discount,Amount,Gst,Nett,PaymentTerms,Remark,PersonID,PersonName,CreatedOn")] Quotation inv)
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

                return RedirectToAction("Edit", new { id = inv.QuoID });
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            return View(inv);
        }

        public ActionResult _OrderDetail(int? id, string act)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quotation inv = db.Quotations.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewBag.Act = act;
            ViewData["seGSTRate"] = GetGstRate();

            return PartialView(inv);
        }


        public ActionResult _DisplayInvDets(string id, string act)
        {
            var p = new List<INVDET>();
            p = db.INVDETs
                .Where(x => (x.QuoNo == id))
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
            string invtype = sor.InvType;

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.INVDETs.Find(i.DetID);
                    if (det != null)
                    {
                        det.ItemDesc = i.ItemDesc;
                        det.Remark = i.Remark;
                        det.Qty = i.Qty;
                        det.Unit = i.Unit;
                        det.DiscountedPrice = i.DiscountedPrice;
                        det.Discount = det.DiscountedPrice - det.UnitPrice;
                        det.Amount = System.Math.Round((det.DiscountedPrice * Convert.ToDecimal(det.Qty)), 2, MidpointRounding.AwayFromZero);
                        det.Nett = det.Amount + det.Gst;

                        det.ModifiedBy = User.Identity.Name;
                        det.ModifiedOn = DateTime.Now;

                    }

                    //   UpdateKivDet(det);
                };
                db.SaveChanges();
            };

            string quono = sor.QuoNo;
            int SorID = db.Quotations.Where(x => x.QuoNo == quono).FirstOrDefault().QuoID;
            UpdateContractAmount(SorID);


            //if (invtype == "CS")
            //{
            //    return RedirectToAction("CrEdit", new { id = SorID });
            //}
            //else
            //{
            //    return RedirectToAction("CrEdit", new { id = SorID });
            //}

            return Json(new { success = true, redirectUrl = Url.Action("CrEdit", "Orders", new { id = SorID, str = "0" }) });

        }

        public ActionResult _DisplayInvDetsPrint(string id)
        {
            var p = new List<INVDET>();
            p = db.INVDETs
                .Where(x => (x.QuoNo == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _AddItem(int id)
        {
            var inv = db.Quotations.Find(id);

            var p = new INVDET();
            p.QuoNo = inv.QuoNo;
            p.InvType = inv.InvType;

            //   var getList = GetProductList();

            //  ViewData["ClientsAll"] = GetClientListByUser("CS");
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
            var ps = db.Products.Where(x => x.ProductID == -1).FirstOrDefault();

            if ((data.DetType == "PRODUCT") && (data.ItemID != 0))
            {
                ps = db.Products.Where(x => x.ProductID == data.ItemID).FirstOrDefault();
            }

            //decimal costprice = ps.CostPrice;
            //string costcode = Decimal2String(costprice);
            //data.Remark = costcode;

            data.Nett = data.Amount + data.Gst;

            var invdet1 = db.INVDETs.Where(x => x.RefItemID == 0 && x.QuoNo == data.QuoNo).ToList();
            double positioncount = invdet1.Count;
            data.Position = positioncount + 1;

            data.ModifiedBy = User.Identity.Name;
            data.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.INVDETs.Add(data);
                db.SaveChanges();
            };

            string quono = data.QuoNo;
            int SorID = db.Quotations.Where(x => x.QuoNo == quono).FirstOrDefault().QuoID;

            UpdateContractAmount(SorID);

            //   AddKivDet(data);

            int bundlecount = 0;

            var totalAmount = db.Quotations.Where(x => x.QuoID == SorID).FirstOrDefault().Nett;
            var detCount = db.INVDETs.Count(x => x.QuoNo == data.QuoNo);

            ViewBag.Message = "1";
            var poptions = new List<PriceOption>();
            ViewData["PriceOptionsAll"] = poptions;
            ViewData["seGSTRate"] = GetGstRate();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return Json(new { success = true, totalamount = totalAmount, detcount = detCount, redirectUrl = Url.Action("CrEdit", "Quotation", new { id = SorID, str = "1" }) });

        }


        public ActionResult _AddDetBundle(int id, double qty, decimal unitprice, decimal discprice, int SorID, string selltype, string remark)
        {
            var sor = db.Quotations.Where(x => x.QuoID == SorID).FirstOrDefault();

            if (sor == null)
            {
                return null;
            }

            var invtype = sor.InvType;


            var dets = new List<INVDET>();
            var p = db.Products.Find(id);

            if (p != null)
            {
                var invdet1 = db.INVDETs.Where(x => x.RefItemID == 0 && x.QuoNo == sor.QuoNo).ToList();
                double positioncount = invdet1.Count;
                var det1 = new INVDET();
                det1.QuoNo = sor.QuoNo;
                det1.SorID = 0;
                det1.SorNo = sor.SorNo;
                det1.InvID = 0;
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

                //det1.Position = 0;
                det1.Position = positioncount + 1;

                dets.Add(det1);

                int bundlecount = 0;

                foreach (var bb in p.Productbundles)
                {
                    bundlecount++;
                    var det2 = new INVDET();
                    det2.QuoNo = det1.QuoNo;
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

                    //det2.Position = 1;
                    det2.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());
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
                        var det2 = new INVDET();
                        det2.QuoNo = det1.QuoNo;
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

                        det2.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());
                        dets.Add(det2);

                    }
                }

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
                string quono = list.FirstOrDefault().QuoNo;
                int SorID = db.Quotations.Where(x => x.QuoNo == quono).FirstOrDefault().QuoID;

                //bool IsFirst = true;
                //int refid = 0;

                foreach (var det in list)
                {
                    if (det.Qty > 0)
                    {
                        if (ModelState.IsValid)
                        {
                            db.INVDETs.Add(det);
                            db.SaveChanges();
                        }

                        ////det.RefItemID = refid;
                        //det.Nett = det.Amount + det.Gst;

                        //if (ModelState.IsValid)
                        //{
                        //    db.INVDETs.Add(det);
                        //    db.SaveChanges();

                        //    if (IsFirst)
                        //    {
                        //        refid = det.DetID;
                        //        IsFirst = false;
                        //    }

                        //};
 
                    }

                };
                UpdateContractAmount(SorID); 

                return RedirectToAction("Edit", new { id = SorID });
            }

        }

        public ActionResult _EditDetBundle(int id)
        {
            var det = db.INVDETs.Find(id);
            if (det == null)
            {
                return HttpNotFound();
            }

            var dets = new List<INVDET>();
            dets.Add(det);

            var p = db.Products.Find(det.ItemID);

            if (p != null)
            {
                foreach (var bb in p.Productbundles)
                {
                    var det2 = new INVDET();
                    det2.SorID = det.SorID;
                    det2.ItemID = bb.IncProductID;
                    det2.ItemCode = bb.IncSKU;
                    det2.ItemType = bb.IncProductType;
                    det2.ItemName = bb.IncProductName;
                    det2.Qty = Convert.ToDouble(0);
                    det2.Unit = "";
                    det2.Remark = p.ProductID.ToString();
                    det2.IsControlItem = bb.IsControlItem;
                    det2.IsBundle = p.IsBundle;
                    det2.SalesType = "BundleItem";
                    det2.RefItemID = det.DetID;
                    det2.Position = bb.Position;

                    var tmp = db.INVDETs.Where(x => x.QuoNo == det.QuoNo && x.ItemCode == bb.IncSKU && x.RefItemID == det.DetID && x.SalesType == "BundleItem").FirstOrDefault();
                    if (tmp != null)
                    {
                        det2.DetID = tmp.DetID;
                        det2.Qty = tmp.Qty;
                        det2.Unit = tmp.Unit;

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
                        var det2 = new INVDET();
                        det2.SorID = det.SorID;
                        det2.ItemID = bb.IncProductID;
                        det2.ItemCode = bb.IncSKU;
                        det2.ItemType = bb.IncProductType;
                        det2.ItemName = bb.IncProductName;
                        det2.Qty = Convert.ToDouble(0);
                        det2.Unit = "";
                        det2.Remark = p.ProductID.ToString();
                        det2.IsControlItem = bb.IsControlItem;
                        det2.IsBundle = p.IsBundle;
                        det2.SalesType = "FOCItem";
                        det2.RefItemID = det.DetID;
                        det2.Position = bb.Position;

                        var tmp = db.INVDETs.Where(x => x.QuoNo == det.QuoNo && x.ItemCode == bb.IncSKU && x.RefItemID == det.DetID && x.SalesType == "FOCItem").FirstOrDefault();
                        if (tmp != null)
                        {
                            det2.DetID = tmp.DetID;
                            det2.Qty = tmp.Qty;
                            det2.Unit = tmp.Unit;
                            //   det2.SalesType = tmp.SalesType;
                        };

                        dets.Add(det2);

                    }
                }



            }

            return PartialView(dets);
        }

        [HttpPost]
        public ActionResult _EditDetBundle(List<INVDET> list)
        {
            string quono = list.FirstOrDefault().QuoNo;
            int SorID = list.FirstOrDefault().SorID;
            int refid = list.FirstOrDefault().DetID;
            var item1 = list.FirstOrDefault();

            var det0 = db.INVDETs.Find(refid);
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
                foreach (INVDET det in list)
                {
                    if (IsFirst)
                    {
                        IsFirst = false;
                    }
                    else
                    {
                        var det1 = db.INVDETs.Find(det.DetID);

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
                            det1 = new INVDET();
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

                            var detfirst = db.INVDETs.Where(x => x.DetID == refid && x.QuoNo == quono).FirstOrDefault();
                            var detlist = db.INVDETs.Where(x => x.RefItemID == refid && x.QuoNo == quono).ToList();

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


            return Json(new { success = true });

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
        public ActionResult _EditDet([Bind(Include = "DetID,QuoNo,SorID,SorNo,InvID,ItemID,ItemCode,ItemType,ItemName,Qty,Unit,UnitPrice,DiscountedPrice,PreDiscAmount,Discount,Amount,Gst,Nett,Remark,IsControlItem")] INVDET data)
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

            string quono = data.QuoNo;
            int SorID = db.Quotations.Where(x => x.QuoNo == quono).FirstOrDefault().QuoID;

            UpdateContractAmount(SorID);

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




        public JsonResult _AddOverallDiscount(string valInvID, string valDiscount, string valAmount, string valGst, string valNett)
        {
            int id = Convert.ToInt32(valInvID);
            decimal discount = Convert.ToDecimal(valDiscount);
            decimal amount = Convert.ToDecimal(valAmount);
            decimal gst = Convert.ToDecimal(valGst);
            decimal nett = Convert.ToDecimal(valNett);

            var inv = db.Quotations.Find(id);
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

            var inv = db.Quotations.Find(id);
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
            Quotation inv = db.Quotations.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            var salespay = db.SalesPaymentMethods.Where(x => x.SorID == id).ToList();

            decimal sPaidAmount = 0;
            foreach (var s in salespay)
            {
                sPaidAmount += s.Amount;
            }

            ViewData["seSalesPaymentAmount"] = sPaidAmount;
            var item = db.INVs.Where(x => x.SorID == id).FirstOrDefault();
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

        private void UpdateContractAmount(int id)
        {
            string quono = db.Quotations.Find(id).QuoNo;

            //   decimal sumPreDiscAmount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.PreDiscAmount) ?? 0;
            //   decimal sumDiscount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.Discount) ?? 0;
            decimal sumAmount = db.INVDETs.Where(c => c.QuoNo == quono).Sum(c => (decimal?)c.Amount) ?? 0;

            decimal gst = GetGstRate();

            //     decimal sumGst = sumAmount * gst;
            //     decimal sumNett = sumAmount + sumGst;

            Quotation inv = db.Quotations.Find(id);
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

                string quono = det.QuoNo;
                int SorID = db.Quotations.Where(x => x.QuoNo == quono).FirstOrDefault().QuoID;

                var dets = db.INVDETs.Where(x => x.QuoNo == det.QuoNo && x.RefItemID == det.DetID).ToList();
                foreach (var dd in dets)
                {
                    db.Entry(dd).State = EntityState.Deleted;
                }

                //   db.INVDETs.Remove(det);
                db.Entry(det).State = EntityState.Deleted;

                db.SaveChanges();

                //  update sales agreement total amount

                UpdateContractAmount(SorID);

                var detlist = db.INVDETs.Where(x => x.QuoNo == quono && x.Position > position).ToList();

                foreach (var item in detlist)
                { 
                    item.Position -= 1;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
 
                }

                trans.Complete();
            }

            return Json(new { success = true });
        }


        [HttpGet]
        public JsonResult _SubmitSalesOrderPreview(int SorID, Boolean CheckWithoutPayment, Boolean CheckBoxCash, Boolean CheckBoxNETS, Boolean CheckBoxCreditCard, Boolean CheckBoxCheque, string CheckBoxCashAmount, string CheckBoxNETSAmount, string CheckBoxCreditCardAmount, string CheckBoxChequeAmount, string CheckBoxChequeNumber)
        {
            Quotation oinv = db.Quotations.Find(SorID);
            Client client = db.Clients.Find(oinv.CustNo);

            if (oinv == null)
            {
                return Json(new { success = false, responseText = "The quotation is not found. No valid data. Please refresh page." }, JsonRequestBehavior.AllowGet);
            };


            oinv.Status = "Approved";

            if (ModelState.IsValid)
            {
                db.Entry(oinv).State = EntityState.Modified;
                db.SaveChanges();
            }


            return Json(new
            {
                printUrl = Url.Action("QuotationPrintPreview", "Invoice", new { id = oinv.QuoID }),
                redirectUrl = Url.Action("CrEdit", "Quotation", new { id = oinv.QuoID }),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult _ConvertToSalesOrder(int SorID)
        {
            Quotation qn = db.Quotations.Find(SorID);
            Client client = db.Clients.Find(qn.CustNo);

            if (qn == null || client == null)
            {
                return Json(new { success = false, responseText = "The quotation is not found. No valid data. Please refresh page." }, JsonRequestBehavior.AllowGet);
            };

            qn.Status = "Win";

            if (ModelState.IsValid)
            {
                db.Entry(qn).State = EntityState.Modified;
                db.SaveChanges();
            }

            var inv = new SalesOrder();

          //  string str = GetMaxOrderNumber();
         //   string str = invoice.GetInvoiceNumber(InvType.SO.ToString(), DateTime.Now, User.Identity.Name);
            string newno = "";
            var sp = db.Staffs.Find(qn.PersonID);
            if (sp != null && !string.IsNullOrEmpty(sp.DepartmentName))
            {
                if (sp.DepartmentName == "SR")
                {
                    newno = GetSerialNumber(201);
                }
                else
                {
                    newno = GetSerialNumber(101);
                }
            }
            else
            {
                newno = GetSerialNumber(101);
            }

            inv.SorNo = newno;
            inv.QuoID = qn.QuoID;
            inv.QuoNo = qn.QuoNo;
            inv.DorNo = "";
            inv.InvID = 0;
            inv.InvNo = "";

            if (client.AccType == "CR")
            {
                inv.InvType = "CR";
            }
            else
            {
                inv.InvType = "CS";
            };

            inv.InvDate = DateTime.Now;
            inv.PoNo = qn.PoNo;
            inv.InvRef = qn.InvRef;
            inv.CustNo = qn.CustNo;
            inv.AccType = client.AccType;
            inv.CustName = qn.CustName;
            inv.CustName2 = qn.CustName2;
            inv.Addr1 = qn.Addr1;
            inv.Addr2 = qn.Addr2;
            inv.Addr3 = qn.Addr3;
            inv.Addr4 = qn.Addr4;
            inv.Attn = qn.Attn;
            inv.PhoneNo = qn.PhoneNo;
            inv.FaxNo = qn.FaxNo;
            inv.DeliveryAddress = qn.DeliveryAddress;
            inv.DeliveryDate = qn.DeliveryDate;
            inv.DeliveryTime = qn.DeliveryTime;
            inv.PreDiscAmount = qn.PreDiscAmount;
            inv.Discount = qn.Discount;
            inv.Amount = qn.Amount;
            inv.Gst = qn.Gst;
            inv.Nett = qn.Nett;
            inv.PaidAmount = qn.PaidAmount;
            inv.IsPaid = qn.IsPaid;
            inv.PaidDate = qn.PaidDate;
            inv.PaymentTerms = qn.PaymentTerms;
            inv.Status = "Draft";
            inv.LocationID = qn.LocationID;
            inv.LocationName = qn.LocationName;
            inv.Remark = qn.Remark;           
            inv.PersonID = qn.PersonID;
            inv.PersonName = qn.PersonName;

            inv.CreatedBy = User.Identity.Name;
            inv.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.SalesOrders.Add(inv);
                db.SaveChanges();
            };

            UpdateInvDets(qn.QuoNo, inv.SorID, inv.SorNo, inv.InvType);

            string newUrl = "";

            if (inv.InvType == "PRO")
            {
                newUrl = Url.Action("ProEdit", "Orders", new { id = inv.SorID });
            } 
            else if (inv.InvType == "CR") {
                 newUrl = Url.Action("CrEdit", "Orders", new { id = inv.SorID });
            } 
            else {
                newUrl = Url.Action("Edit", "Orders", new { id = inv.SorID });
            }

            return Json(new
            {
                printUrl = Url.Action("PrintPreview", "Invoice", new { id = qn.QuoID }),
                redirectUrl = newUrl,
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }

        private void UpdateInvDets(string quono, int sorid, string sorno, string invtype)
        {
            var dets = db.INVDETs.Where(x => x.QuoNo == quono).ToList();

            foreach (var det in dets)
            {
                det.SorID = sorid;
                det.SorNo = sorno;
                det.InvType = invtype;

                if (ModelState.IsValid)
                {
                    db.Entry(det).State = EntityState.Modified;
                    db.SaveChanges();
                }               

            }

            UpdateKivDets(sorid);

        }

        private void UpdateKivDets(int SorID)
        {
            var list = db.INVDETs.Where(x => x.SorID == SorID).ToList();
            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.KIVDETs.Where(x => x.InvDetID == i.DetID).FirstOrDefault();
                    if (det != null)
                    {
                        det.InvDetID = i.DetID;
                        det.KivID = 0;
                        det.SorID = i.SorID;
                        det.InvID = i.InvID;
                        det.ItemID = i.ItemID;
                        det.ItemCode = i.ItemCode;
                        det.ItemName = i.ItemName;
                        det.OrderQty = i.Qty;
                        det.BalanceQty = i.Qty;
                        det.DeliverQty = 0.00;
                        det.KivBalanceQty = i.Qty;
                        det.SalesType = i.SalesType;
                        det.Position = i.Position;
                        det.Remark = i.Remark;

                    }
                    else
                    {
                        det = new KIVDET();
                        det.InvDetID = i.DetID;
                        det.KivID = 0;
                        det.SorID = i.SorID;
                        det.InvID = i.InvID;
                        det.ItemID = i.ItemID;
                        det.ItemCode = i.ItemCode;
                        det.ItemName = i.ItemName;
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

            var kivs = db.KIVDETs.Where(x => x.SorID == SorID).ToList();
            foreach (var kiv in kivs)
            {
                bool contains = db.INVDETs.Any(x => x.SorID == kiv.SorID && x.DetID == kiv.InvDetID);

                if (!contains)
                {
                    var k = db.KIVDETs.Find(kiv.DetID);
                    if (k != null)
                    {
                        db.KIVDETs.Remove(k);
                        db.SaveChanges();
                    }

                }

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

                var hPrices = (from i in db.INVDETs
                               join s in db.INVs on i.InvID equals s.InvID
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

                var hPrices = (from i in db.INVDETs
                               join s in db.INVs on i.InvID equals s.InvID
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
                var HPrice = (from i in db.INVDETs
                              join s in db.SalesOrders on i.SorID equals s.SorID
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
                var HPrice = (from i in db.INVDETs
                              join s in db.SalesOrders on i.SorID equals s.SorID
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


        [HttpGet]
        public ActionResult _ItemMoveUp(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var inv = db.INVDETs.Where(x => x.DetID == id).FirstOrDefault();
                string quNO = inv.QuoNo;
                double currentposition = inv.Position;

                if (!inv.IsBundle)
                {
                    //var nextinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition - 1).FirstOrDefault();
                    var nextinv = db.INVDETs.Where(x => x.QuoNo == quNO && x.Position == currentposition - 1).FirstOrDefault();
                    nextinv.Position += 1;
                    inv.Position -= 1;

                    db.Entry(nextinv).State = EntityState.Modified;
                    //db.SaveChanges();

                    db.Entry(inv).State = EntityState.Modified;
                    db.SaveChanges();


                    if (nextinv.IsBundle)
                    {
                        var bundleinv = db.INVDETs.Where(x => x.QuoNo == inv.QuoNo && x.RefItemID == nextinv.DetID).ToList();

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
                        var nextinv = db.INVDETs.Where(x => x.QuoNo == inv.QuoNo && x.Position == currentposition - 1).FirstOrDefault();
                        var bundleinv = db.INVDETs.Where(x => x.QuoNo == inv.QuoNo && x.RefItemID == inv.DetID).ToList();

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
                            bundleinv = db.INVDETs.Where(x => x.QuoNo == inv.QuoNo && x.RefItemID == nextinv.DetID).ToList();

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
                        var nextinv = db.INVDETs.Where(x => x.QuoNo == inv.QuoNo && x.Position == Math.Round(currentposition, 1)).FirstOrDefault();

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

            return Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult _ItemMoveDown(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var inv = db.INVDETs.Where(x => x.DetID == id).FirstOrDefault();
                double currentposition = inv.Position;

                if (!inv.IsBundle)
                {
                    var nextinv = db.INVDETs.Where(x => x.QuoNo == inv.QuoNo && x.Position == currentposition + 1).FirstOrDefault();
                    nextinv.Position -= 1;

                    inv.Position += 1;

                    db.Entry(nextinv).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(inv).State = EntityState.Modified;
                    db.SaveChanges();

                    if (nextinv.IsBundle)
                    {
                        var bundleinv = db.INVDETs.Where(x => x.QuoNo == inv.QuoNo && x.RefItemID == nextinv.DetID).ToList();

                        foreach (var item in bundleinv)
                        {
                            item.Position -= 1;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();
 
                        }
                    }
                }
                else
                {
                    if (inv.SalesType == "DefaultItem")
                    {
                        var nextinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition + 1).FirstOrDefault();

                        if (nextinv == null)
                        {
                            return Json(new { success = false, responseText = "You cannot move it down as it is the last item." }, JsonRequestBehavior.AllowGet);
                        }

                        var bundleinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == inv.DetID).ToList();

                        foreach (var item in bundleinv)
                        {
                            item.Position += 1;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();

                            var bundlekiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
                            bundlekiv.Position += 1;
                            db.Entry(bundlekiv).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        nextinv.Position -= 1;
                        inv.Position += 1;
 
 

                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();

  
                        if (nextinv.IsBundle)
                        {
                            bundleinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == nextinv.DetID).ToList();

                            foreach (var item in bundleinv)
                            {
                                item.Position -= 1;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();

                                var bundlekiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
                                bundlekiv.Position -= 1;
                                db.Entry(bundlekiv).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                    else if (inv.SalesType == "BundleItem" || inv.SalesType == "FOCItem")
                    {
                        var nextinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.Position == Math.Round(currentposition + 0.1, 1)).FirstOrDefault();
                        var nextkiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.Position == Math.Round(currentposition + 0.1, 1)).FirstOrDefault();

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
 
 

                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();
 
                    }
                }

                trans.Complete();
            }

            return Json(new
            {

            }, JsonRequestBehavior.AllowGet);
        }



        // ************************************   New quotation creation ************************

        // GET: Sales/Edit/5
        public ActionResult CrEdit(int? id, string str)
        {
            Quotation inv = new Quotation();

            if (id == null || id == 0)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                int custno = 0;
                string custname = "Select company";
                string acctype = "CR";

                var cust = db.Clients.Where(x => x.CustName == "CASH").FirstOrDefault();
                if (cust != null)
                {
                    custno = cust.CustNo;
                    custname = cust.CustName;
                    acctype = cust.AccType;
                };

                //string newno = GetMaxQuotationNumber();
                string newno = "";
                var sp = db.Staffs.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
                if (sp != null && !string.IsNullOrEmpty(sp.DepartmentName))
                {
                    if (sp.DepartmentName == "SR")
                    {
                        newno = GetSerialNumber(200);
                    }
                    else
                    {
                        newno = GetSerialNumber(100);
                    }
                }
                else
                {
                    newno = GetSerialNumber(100);
                }

                inv.QuoNo = newno;
                inv.InvDate = DateTime.Now;
                inv.InvType = "QN";
                inv.CustNo = custno;
                inv.CustName = custname;
                inv.AccType = acctype;
                inv.PreDiscAmount = 0;
                inv.Discount = 0;
                inv.Amount = 0;
                inv.Gst = 0;
                inv.Nett = 0;
                inv.IsPaid = false;
                inv.Status = "Draft";
                inv.PersonID = 0;

                if (sp != null)
                {
                    inv.PersonID = sp.StaffID;
                    inv.PersonName = sp.FirstName; 
                   
                }

                inv.LocationID = 0;
                inv.CreatedBy = User.Identity.Name;
                inv.CreatedOn = DateTime.Now;

                Client client = db.Clients.Where(c => c.CustName == "QTN").FirstOrDefault();
                if (client!=null)
                {
                    inv.CustNo = client.CustNo;
                    inv.CustName = client.CustName;
                    inv.Addr1 = client.Addr1;
                    inv.Addr2 = client.Addr2;
                    inv.Addr3 = client.Addr3;
                    inv.PaymentTerms = client.Terms;
                }
                if (ModelState.IsValid)
                {
                    db.Quotations.Add(inv);
                    db.SaveChanges();
                };

                return RedirectToAction("CrEdit", new { id = inv.QuoID, str = "0" });

            }

            inv = db.Quotations.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            bool isFound = false;
            var clist = GetClientListByUser("CR");
            foreach (var c in clist)
            {
                if (c.CustNo == inv.CustNo)
                {
                    isFound = true;
                }
            }

            if (!isFound)
            {
                var client = db.Clients.Where(x => x.CustNo == inv.CustNo).FirstOrDefault();
                clist.Add(client);
            }

            //    ViewData["ClientsAll"] = clist;
            ViewData["ClientsAll"] = db.Clients.Where(x => x.AccType == "CR" && x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();
            ViewData["seGSTRate"] = GetGstRate();

            var item = db.INVs.Where(x => x.QuoNo == inv.QuoNo).FirstOrDefault();
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
        //[ValidateAntiForgeryToken]
        public JsonResult CrEdit([Bind(Include = "QuoID,QuoNo,SorNo,InvNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,Addr4,Attn,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PreDiscAmount,Discount,Amount,Gst,Nett,PaymentTerms,Remark,PersonID,PersonName,CreatedOn")] Quotation inv)
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

            //  return RedirectToAction("CrEdit", new { id = inv.SorID });

            bool isFound = false;
            var clist = GetClientListByUser("CR");
            foreach (var c in clist)
            {
                if (c.CustNo == inv.CustNo)
                {
                    isFound = true;
                }
            }

            if (!isFound)
            {
                var client = db.Clients.Where(x => x.CustNo == inv.CustNo).FirstOrDefault();
                clist.Add(client);
            }

            //   ViewData["ClientsAll"] = clist;
            ViewData["ClientsAll"] = db.Clients.Where(x => x.AccType == "CR" && x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            ViewBag.SalesID = 0;

            var staff = db.Staffs.Where(x => x.Email.Trim() == User.Identity.Name.Trim()).FirstOrDefault();
            if (staff != null)
            {
                ViewBag.StaffID = staff.StaffID;
            }

            //   return View(inv);

            return Json(new { success = true, redirectUrl = Url.Action("CrEdit", "Quotation", new { id = 0, str = "0" }) });

        }

        public JsonResult _DelItem(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var det = db.INVDETs.Find(id);
                double position = det.Position;
                string quoNo = det.QuoNo;
                int SorID =  db.Quotations.Where(x => x.QuoNo == quoNo).FirstOrDefault().QuoID;//det.SorID;
                

                var dets = db.INVDETs.Where(x => x.SorID == det.SorID && x.RefItemID == det.DetID).ToList();
                foreach (var dd in dets)
                {
                    db.Entry(dd).State = EntityState.Deleted;
                }

                //   db.INVDETs.Remove(det);
                db.Entry(det).State = EntityState.Deleted;

                db.SaveChanges();

                //  update sales agreement total amount

                UpdateContractAmount(SorID);

                //UpdateKivDets(SorID);

                //var detlist = db.INVDETs.Where(x => x.SorID == SorID && x.Position > position).ToList();
                var detlist = db.INVDETs.Where(x => x.QuoNo == quoNo && x.Position > position).ToList();
                foreach (var item in detlist)
                {
                    //var bundleitem = db.KIVDETs.Where(y => y.SorID == SorID && y.InvDetID == item.DetID).FirstOrDefault();
                    var bundleitem = db.INVDETs.Where(y => y.RefItemID == item.DetID && y.QuoNo == quoNo).FirstOrDefault();
                    if (bundleitem!=null)
                    {
                        bundleitem.Position -= 1;
                        //db.Entry(bundleitem).State = EntityState.Modified;
                    }
                    item.Position -= 1;                    
                    db.Entry(item).State = EntityState.Modified; 
                    db.SaveChanges();
                }

                trans.Complete();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult SaveAsNew(int id)
        {
            Quotation quo = db.Quotations.Find(id);
            if (quo==null)
            {
                return HttpNotFound();
            }
            using (TransactionScope trans = new TransactionScope())
            {
                string quoNoOld = quo.QuoNo;
                string quoNoNew = GetMaxQuotationNumber();
                quo.QuoNo = quoNoNew;
                quo.InvDate = DateTime.Now.Date;
                quo.Status = "Draft";
                quo.CreatedBy = User.Identity.Name;
                quo.CreatedOn = DateTime.Now;
                quo.ModifiedBy = null;
                quo.ModifiedOn = null;
                quo.IsPaid = false;
                quo.PaidAmount = 0;
                //quo.PaymentStatus = null;
                quo.PaidDate = null;
                db.Entry(quo).State = EntityState.Added;
                db.SaveChanges();

                var detList = db.INVDETs.Where(d => d.QuoNo == quoNoOld && d.RefItemID == 0).ToList();
                if (detList != null && detList.Count > 0)
                {
                    #region
                    foreach (var det in detList)
                    {
                        var detIdOld = det.DetID;
                        det.QuoNo = quoNoNew;
                        det.SorID = 0;
                        det.SorNo = null;
                        det.InvID = 0;
                        det.InvNo = null;
                        det.EorID = 0;
                        det.DrOrderDetId = 0;
                        det.ModifiedBy = null;
                        det.ModifiedOn = null;
                        db.Entry(det).State = EntityState.Added;
                        db.SaveChanges();
                        var detIdNew = det.DetID;
                        if (det.IsBundle == true)
                        {
                            var det2List = db.INVDETs.Where(d => d.QuoNo == quoNoOld && d.RefItemID == detIdOld).ToList();
                            foreach (var det2 in det2List)
                            {
                                det2.QuoNo = quoNoNew;
                                det2.RefItemID = detIdNew;
                                det2.SorID = 0;
                                det2.SorNo = null;
                                det2.InvID = 0;
                                det2.InvNo = null;
                                det2.EorID = 0;
                                det2.DrOrderDetId = 0;
                                det2.ModifiedBy = null;
                                det2.ModifiedOn = null;
                                db.Entry(det2).State = EntityState.Added;
                                db.SaveChanges();
                            }
                        }
                    }
                    #endregion
                }
                db.SaveChanges();
                trans.Complete();
            }
            
            return RedirectToAction("CrEdit", new { id = quo.QuoID});
        }

    }
}