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

namespace iTrade.Controllers
{
    public class LoanManagementsController : Controller
    {
        private StarDbContext db = new StarDbContext();

        // GET: LoanManagements
        public ActionResult Index()
        {
            return View(db.LoanManagements.ToList());
        }

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

            var p = new List<LoanManagement>();

            p = db.LoanManagements.OrderByDescending(x => x.LoanManagementID).ToList();


            return PartialView(p);
        }

        // GET: LoanManagements/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LoanManagement loanManagement = db.LoanManagements.Find(id);
            if (loanManagement == null)
            {
                return HttpNotFound();
            }
            return View(loanManagement);
        }

        // GET: LoanManagements/Create
        public ActionResult Create()
        {
            var loan = new LoanManagement();
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            return View(loan);
        }

        // POST: LoanManagements/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "LoanManagementID,QuoID,InvID,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,PostalCode,LoaningAddress,LoaningDate,LoaningTime,PaymentTerms,Status,Remark,PersonID,PersonName,CreatedBy,CreatedOn")] LoanManagement loanManagement)
        {
            if (ModelState.IsValid)
            {
                loanManagement.CreatedBy = User.Identity.Name;
                loanManagement.CreatedOn = DateTime.Now;
                db.LoanManagements.Add(loanManagement);
                db.SaveChanges();

                return RedirectToAction("Edit", new { id = loanManagement.LoanManagementID });
            }
            ViewBag.Message = "1";

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(loanManagement);
        }

        // GET: LoanManagements/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LoanManagement loanManagement = db.LoanManagements.Find(id);
            if (loanManagement == null)
            {
                return HttpNotFound();
            }
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            return View(loanManagement);
        }

        // POST: LoanManagements/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "LoanManagementID,QuoID,InvID,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,PostalCode,LoaningAddress,LoaningDate,LoaningTime,PaymentTerms,Status,Remark,PersonID,PersonName,ModifiedBy,ModifiedOn")] LoanManagement loanManagement)
        {
            if (ModelState.IsValid)
            {
                loanManagement.ModifiedBy = User.Identity.Name;
                loanManagement.ModifiedOn = DateTime.Now;
                db.Entry(loanManagement).State = EntityState.Modified;
                db.SaveChanges();
                ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
                ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
                ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
                return RedirectToAction("Index");
            }
            return View(loanManagement);
        }

        // GET: LoanManagements/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LoanManagement loanManagement = db.LoanManagements.Find(id);
            if (loanManagement == null)
            {
                return HttpNotFound();
            }
            return View(loanManagement);
        }

        // POST: LoanManagements/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    LoanManagement loanManagement = db.LoanManagements.Find(id);
        //    db.LoanManagements.Remove(loanManagement);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult _DisplayInvDets(int id)
        {
            var p = new List<LoanManagementDetail>();
            p = db.LoanManagementDetails
                .Where(x => (x.LoanManagementID == id))
                .OrderBy(x => x.LoanManagementDetailID)
                .ToList();

            var r = db.LoanManagements.Where(y => y.LoanManagementID == id).FirstOrDefault();
            ViewBag.ReturnedItemStatus = r.Status;
            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _AddDet(int id)
        {
            var p = new LoanManagementDetail();
            p.LoanManagementID = id;

            //   var getList = GetProductList();

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //     ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //  ViewData["ProductData"] = getList;

            return PartialView(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddDet(LoanManagementDetail data)
        {
            var ps = db.Products.Where(x => x.ProductID == data.ItemID).FirstOrDefault();

            int iID = data.LoanManagementID;

            decimal costprice = ps.CostPrice;
            string costcode = Decimal2String(costprice);
            data.Remark = costcode;

            var invdet1 = db.LoanManagementDetails.Where(x => x.RefItemID == 0 && x.LoanManagementID == data.LoanManagementID).ToList();
            double positioncount = invdet1.Count;
            data.Position = positioncount + 1;

            if (ModelState.IsValid)
            {
                db.LoanManagementDetails.Add(data);
                db.SaveChanges();
            };

            //   AddKivDet(data);

            int bundlecount = 0;

            foreach (var pb in ps.Productbundles)
            {
                var p = db.Products.Where(x => x.SKU == pb.IncSKU).FirstOrDefault();

                if (p != null)
                {
                    bundlecount++;
                    var det = new LoanManagementDetail();
                    det.LoanManagementID = data.LoanManagementID;
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
                        db.LoanManagementDetails.Add(det);
                        db.SaveChanges();

                    };
                }

            }

            ViewBag.Message = "1";

            return Json(new { redirectUrl = Url.Action("Edit//" + iID.ToString(), "LoanManagements") });

        }

        public ActionResult _AddDetBundle(int id, int qty, int SorID)
        {
            var sor = db.LoanManagements.Where(x => x.LoanManagementID == SorID).FirstOrDefault();

            if (sor == null)
            {
                return null;
            }

            var invtype = sor.InvType;


            var dets = new List<LoanManagementDetail>();
            var p = db.Products.Find(id);

            if (p != null)
            {
                var invdet1 = db.LoanManagementDetails.Where(x => x.RefItemID == 0 && x.LoanManagementID == sor.LoanManagementID).ToList();
                double positioncount = invdet1.Count;
                var det1 = new LoanManagementDetail();
                det1.LoanManagementID = SorID;
                //    det1.InvID = 0;
                det1.ItemID = p.ProductID;
                det1.ItemCode = p.SKU;
                det1.ItemType = p.ProductType;
                det1.ItemName = p.ProductName;
                det1.Qty = Convert.ToDouble(qty);
                det1.Unit = p.Unit;
                det1.Remark = "";
                det1.IsControlItem = p.IsControlItem;
                det1.IsBundle = p.IsBundle;
                det1.RefItemID = 0;
                det1.SalesType = "DefaultItem";
                //det1.Position = 0;
                det1.Position = positioncount + 1;

                //det1.Discount = 0;

                if (p.UsePricebreak)
                {
                    var breakqtys = p.Pricebreaks.Where(x => x.BreakQty >= 0).OrderByDescending(x => x.BreakQty).ToList();

                    foreach (var bq in breakqtys)
                    {
                        if (det1.Qty >= bq.BreakQty)
                        {
                            decimal price1 = Convert.ToDecimal(bq.DealerPrice);

                            if (invtype == "CS")
                            {
                                price1 = Convert.ToDecimal(bq.RetailPrice);
                            }
                            //det1.Discount = 0;

                            break;
                        }
                    }

                };



                dets.Add(det1);

                int bundlecount = 0;

                foreach (var bb in p.Productbundles)
                {
                    bundlecount++;
                    var det2 = new LoanManagementDetail();
                    det2.LoanManagementID = SorID;
                    det2.ItemID = bb.IncProductID;
                    det2.ItemCode = bb.IncSKU;
                    det2.ItemType = bb.IncProductType;
                    det2.ItemName = bb.IncProductName;
                    det2.Qty = bb.IncQty * qty;
                    det2.Unit = p.Unit;
                    det2.Remark = p.ProductID.ToString();
                    det2.IsControlItem = bb.IsControlItem;
                    det2.IsBundle = p.IsBundle;
                    det2.RefItemID = 0;
                    det2.SalesType = "BundleItem";
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
                        var breakqtys = p.Pricebreaks.Where(x => x.BreakQty != null && x.BreakQty >= 0).OrderByDescending(x => x.BreakQty).ToList();

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
                        var det2 = new LoanManagementDetail();
                        det2.LoanManagementID = SorID;
                        det2.ItemID = bb.IncProductID;
                        det2.ItemCode = bb.IncSKU;
                        det2.ItemType = bb.IncProductType;
                        det2.ItemName = bb.IncProductName;
                        det2.Qty = bb.IncQty * focqty;
                        det2.Unit = p.Unit;
                        det2.Remark = p.ProductID.ToString();
                        det2.IsControlItem = bb.IsControlItem;
                        det2.IsBundle = p.IsBundle;
                        det2.RefItemID = 0;
                        det2.SalesType = "FOCItem";
                        //det2.Position = 2;
                        det2.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());
                        dets.Add(det2);

                    }
                }

            }

            return PartialView(dets);
        }

        [HttpPost]
        public ActionResult _AddDetBundle(List<LoanManagementDetail> list)
        {
            if (list == null)
            {
                return PartialView(list);
            }
            else
            {
                int SorID = list.FirstOrDefault().LoanManagementID;

                bool IsFirst = true;
                int refid = 0;

                foreach (var det in list)
                {
                    if (det.Qty > 0)
                    {
                        det.RefItemID = refid;

                        if (ModelState.IsValid)
                        {
                            db.LoanManagementDetails.Add(det);
                            db.SaveChanges();

                            if (IsFirst)
                            {
                                refid = det.LoanManagementDetailID;
                                IsFirst = false;
                            }

                        };
                    }

                };
                return RedirectToAction("Edit", new { id = SorID });
            }

        }

        public ActionResult _EditDet(int id)
        {
            var p = db.LoanManagementDetails.Find(id);

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

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _EditDet([Bind(Include = "LoanManagementDetailID,LoanManagementID,ItemID,ItemCode,ItemType,ItemName,Qty,Unit,Remark,IsControlItem,StartDate,EndDate")] LoanManagementDetail data)
        {
            Convert.ToDouble(data.Qty);

            if (ModelState.IsValid)
            {
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }

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

        public ActionResult _EditDetBundle(int id)
        {
            var det = db.LoanManagementDetails.Find(id);
            if (det == null)
            {
                return HttpNotFound();
            }

            var dets = new List<LoanManagementDetail>();
            dets.Add(det);

            var p = db.Products.Find(det.ItemID);

            if (p != null)
            {
                foreach (var bb in p.Productbundles)
                {
                    var det2 = new LoanManagementDetail();
                    det2.LoanManagementID = det.LoanManagementID;
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
                    det2.RefItemID = det.LoanManagementDetailID;
                    det2.Position = bb.Position;

                    var tmp = db.LoanManagementDetails.Where(x => x.LoanManagementID == det.LoanManagementID && x.ItemCode == bb.IncSKU && x.RefItemID == det.LoanManagementDetailID && x.SalesType == "BundleItem").FirstOrDefault();
                    if (tmp != null)
                    {
                        det2.LoanManagementDetailID = tmp.LoanManagementDetailID;
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
                        var det2 = new LoanManagementDetail();
                        det2.LoanManagementID = det.LoanManagementID;
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
                        det2.RefItemID = det.LoanManagementDetailID;
                        det2.Position = bb.Position;

                        var tmp = db.LoanManagementDetails.Where(x => x.LoanManagementID == det.LoanManagementID && x.ItemCode == bb.IncSKU && x.RefItemID == det.LoanManagementDetailID && x.SalesType == "FOCItem").FirstOrDefault();
                        if (tmp != null)
                        {
                            det2.LoanManagementDetailID = tmp.LoanManagementDetailID;
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
        public ActionResult _EditDetBundle(List<LoanManagementDetail> list)
        {
            int SorID = list.FirstOrDefault().LoanManagementID;
            int refid = list.FirstOrDefault().LoanManagementDetailID;
            var item1 = list.FirstOrDefault();

            var det0 = db.LoanManagementDetails.Find(refid);
            if (det0 != null)
            {
                det0.Qty = item1.Qty;

                if (ModelState.IsValid)
                {
                    db.Entry(det0).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            if (ModelState.IsValid)
            {
                bool IsFirst = true;
                foreach (LoanManagementDetail det in list)
                {
                    if (IsFirst)
                    {
                        IsFirst = false;
                    }
                    else
                    {
                        var det1 = db.LoanManagementDetails.Find(det.LoanManagementDetailID);

                        if (det1 != null)
                        {
                            det1.Qty = det.Qty;

                        }
                        else
                        {
                            det1 = new LoanManagementDetail();
                            det1.LoanManagementID = SorID;
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

                            var detfirst = db.LoanManagementDetails.Where(x => x.LoanManagementDetailID == refid && x.LoanManagementID == SorID).FirstOrDefault();
                            var detlist = db.LoanManagementDetails.Where(x => x.RefItemID == refid && x.LoanManagementID == SorID).ToList();

                            //det1.Position = Convert.ToDouble(det.Position);
                            det1.Position = Convert.ToDouble(detfirst.Position + "." + (detlist.Count + 1));

                            db.Entry(det1).State = EntityState.Added;

                        }
                    }

                };
                db.SaveChanges();
            };

            return Json(new { success = true });

        }

        public ActionResult _DelDet(int id = 0)
        {
            LoanManagementDetail det = db.LoanManagementDetails.Find(id);
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
                var det = db.LoanManagementDetails.Find(id);
                double position = det.Position;

                int SorID = det.LoanManagementID;

                var dets = db.LoanManagementDetails.Where(x => x.LoanManagementID == det.LoanManagementID && x.RefItemID == det.LoanManagementDetailID).ToList();
                foreach (var dd in dets)
                {
                    db.Entry(dd).State = EntityState.Deleted;
                }

                //   db.INVDETs.Remove(det);
                db.Entry(det).State = EntityState.Deleted;

                db.SaveChanges();

                trans.Complete();
            }

            return Json(new { success = true });
        }

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

        public JsonResult AutoCompleteSelected(string search)
        {
            if (search != null)
            {
                int custno = Convert.ToInt32(search);
                var c = db.Clients
                           .Where(x => x.CustNo == custno)
                           .ToList().FirstOrDefault();

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
        public JsonResult AutoComplete_Product(string search)
        {
            var getList = GetProductList();
            var data = getList;

            string[] words = search.ToUpper().Split(' ').ToArray();

            //      var results = from p in getList
            //                          .Select(x => x.ProductName.ToUpper())
            //                          .Where(x => words.All(y => x.Contains(y)))
            //                    select p;



            //       var result = from p in getList
            //                    let w = p.ProductName.ToUpper().Split(new char[] { '.', '-', '!', ' ', ';', ':', ',' },
            //                                               StringSplitOptions.RemoveEmptyEntries)
            //                    where (w.Distinct().Intersect(words).Count() == words.Count()) || (p.SKU.ToUpper().StartsWith(search.ToUpper()))
            //                     select p;

            var result = from p in getList
                         let w = p.ProductName.ToUpper().Split(new char[] { '.', '-', '!', ' ', ';', ':', ',' },
                                                    StringSplitOptions.RemoveEmptyEntries)
                         where (w.Distinct().Intersect(words).Count() == words.Count()) ||
                         ((p.SKU.ToUpper().Contains(search.ToUpper())) ||
                         (p.ProductName.ToUpper().Contains(search.ToUpper())))
                         //             || (p.Barcode.ToUpper() == search.ToUpper()))
                         select p;


            data = result.ToList();



            if (data == null)
            {
                return null;
            }
            else
            {
                return Json(data.Take(100).ToList(), JsonRequestBehavior.AllowGet);
            }


        }
        public JsonResult AutoCompleteSelected_Product(string search)
        {
            if (search != null)
            {
                var getList = db.Products.Where(p => p.IsActive == true).ToList();

                int newid = Convert.ToInt32(search);
                var c = getList.Where(x => x.ProductID == newid).FirstOrDefault();

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

                var pbreaks = p.Pricebreaks.Where(x => x.BreakQty != null).OrderBy(x => x.BreakQty).ToList();

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

        public JsonResult _ConvertToInvoice(string valSorID)
        {
            int id = Convert.ToInt32(valSorID);

            var sor = db.LoanManagements.Find(id);
            if (sor == null)
            {
                return null;
            };

            sor.Status = "Confirmed";
            if (ModelState.IsValid)
            {
                db.Entry(sor).State = EntityState.Modified;
                db.SaveChanges();

                var r = db.LoanManagementDetails.Where(x => x.LoanManagementID == id).ToList();

                foreach (var item in r)
                {
                    var ps = db.Products.Where(y => y.ProductID == item.ItemID).FirstOrDefault();
                    var oStock = db.Stocks.Where(x => x.ProductID == item.ItemID).FirstOrDefault();
                    oStock.StockIn += item.Qty;
                    db.Entry(oStock).State = EntityState.Modified;
                    var oStockDet = db.StockDets.Where(x => x.ProductID == item.ItemID).FirstOrDefault();
                    oStockDet.StockIn += item.Qty;
                    db.Entry(oStockDet).State = EntityState.Modified;
                    StockTransaction oStockTrans = new StockTransaction();
                    oStockTrans.ProductID = item.ItemID;
                    oStockTrans.SKU = ps.SKU;
                    oStockTrans.ProcessType = "IN";
                    oStockTrans.Qty = item.Qty;
                    oStockTrans.RefNo = item.LoanManagementID.ToString();
                    oStockTrans.SourceFrom = "LoanManagements";
                    oStockTrans.CreatedBy = User.Identity.Name;
                    oStockTrans.CreatedOn = DateTime.Now;
                    db.StockTransactions.Add(oStockTrans);
                    db.SaveChanges();
                }
            };

            return Json(new
            {
                redirectUrl = Url.Action("Index", "LoanManagements"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult _DisplaySalesItems(int id)
        {
            var p = new List<LoanManagementDetail>();
            p = db.LoanManagementDetails
                .Where(x => (x.LoanManagementID == id))
                .OrderBy(x => x.Position)
                .ToList();

            return PartialView(p);
        }
    }
}
