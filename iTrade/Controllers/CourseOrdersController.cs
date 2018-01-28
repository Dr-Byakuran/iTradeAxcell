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
using System.Data.SqlClient;
using Microsoft.AspNet.Identity;

namespace iTrade.Controllers
{
    public class CourseOrdersController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();

        // GET: Sales
        public ActionResult Index()
        {

            return View();
        }

        // [ChildActionOnly] 
        public ActionResult _DisplayResults(string invtype)
        {
            DateTime datefrom = DateTime.Now.AddMonths(-12);

            var p = new List<CourseOrder>();
            p = db.CourseOrders.Where(x => x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.SorID).ToList();

            ViewBag.SalesID = 0;

            var staff = db.Staffs.Where(x => x.Email.Trim() == User.Identity.Name.Trim()).FirstOrDefault();
            if (staff != null)
            {
                ViewBag.StaffID = staff.StaffID;
            }

            return PartialView(p);
        }

        public ActionResult _OrderDetail(int? id, string act)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseOrder inv = db.CourseOrders.Find(id);
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
                .Where(x => (x.SorID == id))
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
            int SorID = sor.SorID;
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

                    //   UpdateKivDet(det);
                };
                db.SaveChanges();
            };

            UpdateContractAmount(SorID);

            //if (invtype == "CS")
            //{
            //    return RedirectToAction("CrEdit", new { id = SorID });
            //}
            //else
            //{
            //    return RedirectToAction("CrEdit", new { id = SorID });
            //}

            return Json(new { success = true, redirectUrl = Url.Action("CoEdit", "CourseOrders", new { id = SorID, str = "0" }) });

        }


        [HttpPost]
        public JsonResult _DisplayInvDetsSave(INVDET det)
        {
            if (ModelState.IsValid)
            {
                db.Entry(det).State = EntityState.Modified;
                db.SaveChanges();
            }

            //  update sales agreement total amount

            UpdateContractAmount(det.SorID);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult _DisplayInvDetsPrint(int id)
        {
            var p = new List<INVDET>();
            p = db.INVDETs
                .Where(x => (x.SorID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult CoEdit(int? id, string str)
        {
            CourseOrder inv = new CourseOrder();

            if (id == null || id == 0)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                string newno = GetMaxOrderNumber();
                inv.SorNo = newno;
                inv.InvID = 0;
                inv.QuoID = 0;
                inv.InvDate = DateTime.Now;
                inv.InvType = "CO";

                inv.CustNo = 0;
                inv.CustName = "Select student";
                inv.PreDiscAmount = 0;
                inv.Discount = 0;
                inv.Amount = 0;
                inv.Gst = 0;
                inv.Nett = 0;
                inv.IsPaid = false;
                inv.Status = "Draft";
                inv.PersonID = 0;
                inv.LocationID = 0;
                inv.CreatedBy = User.Identity.Name;
                inv.CreatedOn = DateTime.Now;

                if (ModelState.IsValid)
                {
                    db.CourseOrders.Add(inv);
                    db.SaveChanges();
                };

                return RedirectToAction("CoEdit", new { id = inv.SorID, str = "0" });

            }

            inv = db.CourseOrders.Find(id);
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
          //  ViewData["ClientsAll"] = db.Students.Where(x => x.AccType == "CR" && x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();
            ViewData["seGSTRate"] = GetGstRate();

            var item = db.CourseOrders.Where(x => x.SorID == id).FirstOrDefault();
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
        public JsonResult CoEdit([Bind(Include = "SorID,SorNo,QuoID,QuoNo,DorNo,InvID,InvNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,PostalCode,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PreDiscAmount,Discount,Amount,Gst,Nett,PaidAmount,PaymentTerms,Remark,PersonID,PersonName,CreatedOn")] CourseOrder inv)
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
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
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

            return Json(new { success = true, redirectUrl = Url.Action("CoEdit", "CourseOrders", new { id = 0, str = "0" }) });

        }

        public ActionResult View(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CourseOrder inv = db.CourseOrders.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            //ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            //ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

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
            return View(inv);
        }


        private void UpdateInvoice(string invno)
        {
            var inv = db.INVs.Where(x => x.InvNo == invno).FirstOrDefault();
            if (inv != null)
            {
                int sorid = inv.SorID;
                var sor = db.CourseOrders.Where(x => x.SorID == sorid).FirstOrDefault();
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

                }


            }
        }

        private void UpdateInvDetWithInvID(int SorID)
        {
            var inv = db.INVs.Where(x => x.SorID == SorID).FirstOrDefault();

            if (inv != null)
            {
                int newInvID = inv.InvID;

                var dets = db.INVDETs.Where(x => x.SorID == SorID).ToList();
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

        private void UpdateContractAmount(int id)
        {
            //   decimal sumPreDiscAmount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.PreDiscAmount) ?? 0;
            //   decimal sumDiscount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.Discount) ?? 0;
            decimal sumAmount = db.INVDETs.Where(c => c.SorID == id).Sum(c => (decimal?)c.Amount) ?? 0;

            decimal gst = GetGstRate();

            //     decimal sumGst = sumAmount * gst;
            //     decimal sumNett = sumAmount + sumGst;

            CourseOrder inv = db.CourseOrders.Find(id);
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

        public ActionResult _AddItem(int id)
        {
            var inv = db.CourseOrders.Find(id);

            var p = new INVDET();
            p.SorID = inv.SorID;
            p.SorNo = inv.SorNo;
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
            var ps = db.Courses.Where(x => x.CourseID == data.ItemID).FirstOrDefault();

            //decimal costprice = ps.CostPrice;
            //string costcode = Decimal2String(costprice);
            //data.Remark = costcode;

            data.Nett = data.Amount + data.Gst;

            var invdet1 = db.INVDETs.Where(x => x.SorID == data.SorID).ToList();
            double positioncount = invdet1.Count;
            data.Position = positioncount + 1;

            if (ModelState.IsValid)
            {
                db.INVDETs.Add(data);
                db.SaveChanges();
            };

            UpdateContractAmount(data.SorID);

            //   AddKivDet(data);

            int bundlecount = 0;



            var totalAmount = db.CourseOrders.Where(x => x.SorID == data.SorID).FirstOrDefault().Nett;
            var detCount = db.INVDETs.Count(x => x.SorID == data.SorID);

            ViewBag.Message = "1";
            var poptions = new List<PriceOption>();
            ViewData["PriceOptionsAll"] = poptions;
            ViewData["seGSTRate"] = GetGstRate();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return Json(new { success = true, totalamount = totalAmount, detcount = detCount, redirectUrl = Url.Action("CoEdit", "CourseOrders", new { id = data.SorID, str = "1" }) });

        }


        public ActionResult _AddMultiItem(int id)
        {
            var sor = db.CourseOrders.Where(x => x.SorID == id).FirstOrDefault();

            if (sor == null)
            {
                return null;
            }

            var dets = new List<INVDET>();

            return PartialView(dets);
        }

        [HttpPost]
        public ActionResult _AddMultiItem(List<INVDET> list)
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

                        var invdet1 = db.INVDETs.Where(x => x.SorID == SorID).ToList();
                        double positioncount = invdet1.Count;
                        det.Position = positioncount + 1;

                        if (ModelState.IsValid)
                        {
                            db.INVDETs.Add(det);
                            db.SaveChanges();
                        }

                    }

                };
                UpdateContractAmount(SorID);

                return RedirectToAction("CoEdit", new { id = SorID });
            }

        }

        public ActionResult _SearchResult(string txtSearch)
        {
            var plist = new List<Course>();
            if (!string.IsNullOrEmpty(txtSearch))
            {
                plist = db.Courses.Where(x => x.CourseName.Contains(txtSearch.Trim()) || x.CourseCode.StartsWith(txtSearch.Trim()) || x.CourseLevel.StartsWith(txtSearch.Trim())).OrderBy(x => x.CourseName).Take(200).ToList();
            }
            else
            {
                plist = db.Courses.OrderBy(x => x.CourseName).Take(200).ToList();
            }

            CourseSelectionView model = new CourseSelectionView();
            foreach (var p in plist)
            {
                var c = db.Courses.Find(p.CourseID);

                var selecteditor = new CourseSelectEditor()
                {
                    Selected = false,
                    CourseID = p.CourseID,
                    CourseCode = p.CourseCode,
                    CourseType = p.CourseType,
                    CourseName = p.CourseName,
                    CourseDesc = c.CourseDesc,
                    CourseCats = c.CourseCats,
                    Duration = c.Duration,
                    PreCourseID = c.PreCourseID,
                    PreCourseName = c.PreCourseName,
                    IsFocRevision = c.IsFocRevision,
                    Qty = c.Qty,
                    Unit = c.Unit,
                    BaseCurrency = c.BaseCurrency,
                    StudentPrice = p.StudentPrice,
                    PublicPrice = p.PublicPrice,
                    IsBundle = c.IsBundle,
                    Remark = p.Remark                  

                };
                model.DataSelects.Add(selecteditor);
            };

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult _SearchResult(CourseSelectionView model, string valSorID)
        //     public ActionResult _SubmitSelected(ProductSelectionView model)
        {
            //   int sorid = Convert.ToInt32(Request.Form["valSorID"]);
            int sorid = Convert.ToInt32(valSorID);
            var selectedview = new CourseSelectionView();
            // get the ids of the items selected:
            List<int> selectedIds = model.getSelectedIds().ToList();

            // Use the ids to retrieve the records for the selected job
            // from the database:
            var selectedCourses = db.Courses.Where(x => selectedIds.Contains(x.CourseID)).ToList();

            //var selectedProducts = (from x in db.Products
            //                    where selectedIds.Contains(x.ProductID)
            //                    select x).ToList();

            foreach (var p in selectedCourses)
            {
                // add this product into INVDET
                if (p != null)
                {
                    AddIntoINVDET(p.CourseID, sorid);
                }
            };

            //     return PartialView(selectedview);
            return Json(new { success = true });

        }

        private void AddIntoINVDET(int id, int sorid)
        {
            var sor = db.CourseOrders.Where(x => x.SorID == sorid).FirstOrDefault();

            if (sor == null)
            {
                return;
            }

            List<INVDET> dets = new List<INVDET>();
            var p = db.Courses.Find(id);

            if (p != null)
            {
                // 1. create det1 and add to dets
                // 2. if p.IsBundle == true, get sub det and add to dets
                // 3. save all dets

                var invdet1 = db.INVDETs.Where(x => x.RefItemID == 0 && x.SorID == sor.SorID).ToList();
                double positioncount = invdet1.Count;
                double qty = 1.0d;
                string selltype = "CS";

                var det1 = new INVDET();
                det1.QuoNo = sor.QuoNo;
                det1.SorID = sor.SorID;
                det1.SorNo = sor.SorNo;
                det1.InvID = sor.InvID;
                det1.InvNo = sor.InvNo;
                det1.CnID = 0;
                det1.CnNo = "";
                det1.InvType = sor.InvType;
                det1.ItemID = p.CourseID;
                det1.ItemCode = p.CourseCode;
                det1.ItemType = p.CourseType;
                det1.ItemName = p.CourseName;
                det1.SellType = selltype;
                det1.Qty = qty;
                det1.Unit = p.Unit;
                det1.UnitPrice = p.StudentPrice;
                det1.DiscountedPrice = p.StudentPrice;
                det1.Discount = 0m;

                det1.PreDiscAmount = p.StudentPrice * Convert.ToDecimal(qty);
                det1.Amount = p.StudentPrice * Convert.ToDecimal(qty);
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
                    foreach (var bb in p.CourseBundles)
                    {
                        bundlecount++;
                        var det2 = new INVDET();
                        det2.QuoNo = det1.QuoNo;
                        det2.SorID = det1.SorID;
                        det2.SorNo = det1.SorNo;
                        det2.InvID = det1.InvID;
                        det2.InvNo = det1.InvNo;
                        det2.CnID = det1.CnID;
                        det2.CnNo = det1.CnNo;
                        det2.InvType = det1.InvType;
                        det2.ItemID = bb.IncCourseID;
                        det2.ItemCode = bb.IncCourseCode;
                        det2.ItemType = bb.IncCourseType;
                        det2.ItemName = bb.IncCourseName;
                        det2.SellType = det1.SellType;
                        det2.Qty = bb.IncQty;
                        det2.Unit = bb.IncUnit;

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
                        det2.Remark = p.CourseID.ToString();

                        //det2.Position = 1;
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
                            db.INVDETs.Add(dd);
                            db.SaveChanges();
                        }


                     //   AddKivDet(dd);
                    }

                };

                UpdateContractAmount(sorid);
             //   UpdateKivDets(sorid);
            }
        }

        [HttpGet]
        public JsonResult _SubmitSalesOrderPreview(int SorID, Boolean CheckWithoutPayment, Boolean CheckBoxCash, Boolean CheckBoxNETS, Boolean CheckBoxCreditCard, Boolean CheckBoxCheque, string CheckBoxCashAmount, string CheckBoxNETSAmount, string CheckBoxCreditCardAmount, string CheckBoxChequeAmount, string CheckBoxChequeNumber)
        {
            CourseOrder oinv = db.CourseOrders.Find(SorID);
            Student client = db.Students.Find(oinv.CustNo);
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

            var sor = db.CourseOrders.Find(id);
            if (sor == null)
            {
                return Json(new { success = false, responseText = "The enrolment is not found. No valid data. Please refresh page." }, JsonRequestBehavior.AllowGet);
            };

            var oinvoice = db.INVs.Where(x => x.SorID == SorID).FirstOrDefault();
            INV invs = new INV();
            string sInvoiceStatus = "";
            Boolean IsPaid = false;

            if (oinvoice == null)
            {
                // ************  Get new invoice number *********************

                string newInvNo = "";

                newInvNo = GetMaxCashInvoiceNumber();

                //if (sor.InvType == "CS")
                //{
                //    newInvNo = GetMaxCashInvoiceNumber();
                //}
                //else
                //{
                //    newInvNo = GetMaxCreditInvoiceNumber();
                //};

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
                invs.PreDiscAmount = sor.PreDiscAmount;
                invs.Discount = sor.Discount;
                invs.Amount = sor.Amount;
                invs.Gst = sor.Gst;
                invs.Nett = sor.Nett;
                invs.PaidAmount = sor.PaidAmount;
                invs.Status = sor.Status;
                invs.PaymentStatus = sor.PaymentStatus;
                invs.PaymentTerms = sor.PaymentTerms;
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

                db.INVs.Add(invs);
                db.SaveChanges();

                UpdateInvDetWithInvID(SorID);
            }
            else
            {
                invs = oinvoice;
                sor.Status = "Invoiced";
                sor.InvNo = invs.InvNo;

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


            return Json(new
            {
                printUrl = Url.Action("PrintInvoiceAndDO", "Invoice", new { id = invs.InvID }),
                printInvUrl = Url.Action("PrintPreview", "Invoice", new { id = invs.InvID }),
                printDOUrl = Url.Action("DeliveryOrderPrint", "Invoice", new { id = invs.InvID }),
                redirectUrl = Url.Action("OrderProcessed", "Orders", new { id = sor.SorID }),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }





        [HttpGet]
        public JsonResult AutoComplete_ProductByItem(string search, int sorid)
        {
            List<INVDET> data = new List<INVDET>();
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


        public List<INVDET> _getProductItems(int pid, int sorid)
        {
            List<INVDET> dets = new List<INVDET>();
            var sor = db.CourseOrders.Find(sorid);
            var p = db.Products.Find(pid);

            if (p != null && sor != null)
            {
                var qty = 1;
                var det1 = new INVDET();
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
                        var det2 = new INVDET();
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
                        var det2 = new INVDET();
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

        [HttpGet]
        public JsonResult _DelItem(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var det = db.INVDETs.Find(id);
                double position = det.Position;

                int SorID = det.SorID;

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

                var detlist = db.INVDETs.Where(x => x.SorID == SorID && x.Position > position).ToList();

                foreach (var item in detlist)
                {
                     item.Position -= 1; 
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
 
                }

                trans.Complete();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

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

                int SorID = det.SorID;

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

                var detlist = db.INVDETs.Where(x => x.SorID == SorID && x.Position > position).ToList();

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
        public JsonResult _ItemMoveUp(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var inv = db.INVDETs.Where(x => x.DetID == id).FirstOrDefault();
                double currentposition = inv.Position;

                if (!inv.IsBundle)
                {
                    var nextinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition - 1).FirstOrDefault();
                    nextinv.Position += 1;
                    inv.Position -= 1;

                    db.Entry(nextinv).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(inv).State = EntityState.Modified;
                    db.SaveChanges();

                    if (nextinv.IsBundle)
                    {
                        var bundleinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == nextinv.DetID).ToList();

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
                        var nextinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition - 1).FirstOrDefault();
                        var bundleinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == inv.DetID).ToList();

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
                            bundleinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == nextinv.DetID).ToList();

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
                        var nextinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.Position == Math.Round(currentposition, 1)).FirstOrDefault();

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



        public ActionResult CoInvoiceIndex()
        {
            return View();
        }

        public ActionResult CoInvoiceView(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            INV inv = db.INVs.Find(id);
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

            var p = new List<INV>();

            p = db.INVs.Where(x => x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.InvID).ToList();

            return PartialView(p);

        }

        public ActionResult Enrolments()
        {
            return View();
        }

        public ActionResult BillItems()
        {
            return View();
        }

        public ActionResult _DisplayEnrolments()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            int BranchID = Convert.ToInt32(user.BranchID);

            //ViewBag.QuoteNumber = id;
            ViewData["EnrolmentAll"] = db.Enrolments.ToList();

            if (BranchID == 1)
            {
                var p = db.Enrolments.Where(x => x.CustNo != 0 && x.Status != "Void").OrderBy(x => x.EnrNo).ToList();
                return PartialView(p);
            }
            else
            {
                var p = db.Enrolments.Where(x => x.CustNo != 0 && x.Status != "Void" && x.BranchID == BranchID).OrderBy(x => x.EnrNo).ToList();
                return PartialView(p);
            }
        }

        public ActionResult _DisplayBillItems()
        {
            var p = db.BillItems.Where(x => x.CustNo != 0).OrderByDescending(x => x.BillForMonth).ThenBy(x => x.EnrID).OrderByDescending(x => x.CreatedOn).ToList();

            //ViewBag.QuoteNumber = id;
            return PartialView(p);
        }

        public ActionResult _DisplayEnrolmentByCust(int id)
        {
            var p = db.Enrolments.Where(x => x.Status != "Void" && x.CustNo == 0).OrderBy(x => x.CustName).ThenBy(x => x.EnrNo).ToList();

            if (id != 0)
            {
                p = db.Enrolments.Where(x => x.Status != "Void" &&  x.CustNo == id).OrderBy(x => x.EnrNo).ToList();
            };

            ViewData["EnrolmentAll"] = db.Enrolments.ToList();
            //ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _DisplayCourses(string invtype)
        {
            DateTime datefrom = DateTime.Now.AddMonths(-12);

            var p = new List<Pricebook>();

            p = db.Pricebooks.Where(x => x.IsValid == true).Take(1000).OrderByDescending(x => x.CourseName).ToList();
            ViewData["EnrolmentAll"] = db.Enrolments.ToList();

            return PartialView(p);

        }

        public ActionResult _DisplaySchedules(int id)
        {
            DateTime datefrom = DateTime.Now.AddMonths(-12);

            var p = new List<ClassSchedule>();

            p = db.ClassSchedules.Where(x => x.PriceID == id).Take(1000).OrderByDescending(x => x.CourseName).ToList();

            return PartialView(p);

        }

        public ActionResult EnrolEdit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrolment inv = db.Enrolments.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            //ViewData["ClientsAll"] = GetClientListByUser("ALL");
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();
            ViewData["StudentsAll"] = db.Students.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ThenBy(x => x.TutorName).ToList();

            return View(inv);
        }

        [HttpPost]
        //  [ValidateAntiForgeryToken]
        public JsonResult EnrolEdit([Bind(Include = "EnrID,EnrNo,EnrType,EnrDate,CustNo,CustName,CustName2,NRIC,PriceID,CourseID,CourseName,CourseCode,CourseType,CourseLevel,CourseDuration,TeacherLevel,OptionName,TutorID,TutorName,TermID,TermName,Weekday,StartDate,EndDate,StarTime,StartTimeValue,EndTime,EndTimeValue,RegisterFee,CourseFee,Deposit,SalesType,IsBillable,BillRemark,IsValid,Status,Remark,PersonID,PersonName,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn")] Enrolment pb)
        {
            pb.ModifiedBy = User.Identity.Name;
            pb.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(pb).State = EntityState.Modified;
                db.SaveChanges();
            };

            ViewBag.StudentNo = pb.CustNo;

            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StudentsAll"] = db.Students.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ThenBy(x => x.TutorName).ToList();

            return Json(new
            {
                success = true,
                paymentUrl = Url.Action("PrEdit", "Payment", new { custid = pb.CustNo }),
                redirectUrl = Url.Action("EnrolEdit", "CourseOrders", new { id = pb.CustNo })
            });


        }


        public ActionResult EnrolmentEdit()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            int BranchID = Convert.ToInt32(user.BranchID);

            var p = new Enrolment();
            p.IsBillable = true;
            p.IsValid = true;
            //  p.SarOn.DateCreated = DateTime.Now;
            //  ViewBag.ProductTypes = db.ProductGroups.ToList();

            //ViewData["ClientsAll"] = GetClientListByUser("ALL");

            if(BranchID == 1)
            {
                ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            }else
            {
                ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true && x.BranchID == BranchID).OrderBy(x => x.CustName).ToList();
            }

            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();           
            ViewData["StudentsAll"] = db.Students.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ThenBy(x => x.TutorName).ToList();
            ViewData["BranchAll"] = db.CompanyBranches.Where(x => x.IsActive == true).ToList();

            return View(p);
        }

        [HttpPost]
        //  [ValidateAntiForgeryToken]
        public JsonResult EnrolmentEdit([Bind(Include = "EnrID,EnrNo,EnrType,EnrDate,CustNo,CustName,CustName2,NRIC,PriceID,BranchID,BranchName,CourseID,CourseName,CourseCode,CourseType,CourseLevel,CourseDuration,TeacherLevel,OptionName,TutorID,TutorName,TermID,TermName,ScheduleID,Weekday,StartDate,EndDate,StarTime,StartTimeValue,EndTime,EndTimeValue,RegisterFee,CourseFee,Deposit,SalesType,IsBillable,BillRemark,IsValid,Status,Remark,PersonID,PersonName,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn")] Enrolment enr)
        {
            bool IsExist = db.Enrolments.Any(x => x.CustNo == enr.CustNo);

            string newNum = "";
            newNum = GetMaxEnrolNumber();
            enr.EnrNo = newNum;
            enr.CreatedBy = User.Identity.Name;
            enr.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Enrolments.Add(enr);
                db.SaveChanges();
            }

            if (!IsExist)
            {
                //    Create Registration if this is 1st enrolment

                decimal adminfee = 15;

                if (adminfee > 0)
                {
                    BillItem det1 = new BillItem();
                    det1.DetType = "OTHER";
                    det1.EnrID = enr.EnrID;
                    det1.EnrNo = enr.EnrNo;
                    det1.CustNo = enr.CustNo;
                    det1.CustName = enr.CustName;
                    det1.CustName2 = enr.CustName2;
                    det1.NRIC = enr.NRIC;
                    det1.BillForMonth = enr.EnrDate;

                    det1.ItemID = 0;
                    det1.ItemCode = "REGISTER_FEE";
                    det1.ItemType = "OT";
                    det1.ItemName = "Registration Fee";
                    det1.ItemDesc = "Registration Fee";
                    det1.Remark = "";
                    det1.SellType = "CS";
                    det1.Qty = 1;
                    det1.Unit = "";
                    det1.UnitPrice = adminfee;
                    det1.DiscountedPrice = adminfee;
                    det1.PreDiscAmount = adminfee;
                    det1.Discount = 0;
                    det1.Amount = adminfee;
                    det1.Gst = 0;
                    det1.Nett = det1.Amount + det1.Gst;
                    det1.IsBundle = false;

                    det1.IsBilled = false;
                    det1.IsPaid = false;
                    det1.IsLocked = false;

                    det1.SalesType = enr.SalesType;
                    det1.RefItemID = 0;
                    det1.InvRef = "";
                    det1.IsControlItem = false;
                    det1.LocationID = 0;
                    det1.LocationName = "";
                    det1.Position = 1;
                    det1.CreatedBy = User.Identity.Name;
                    det1.CreatedOn = DateTime.Now;

                    if (ModelState.IsValid)
                    {
                        db.BillItems.Add(det1);
                        db.SaveChanges();
                    }
                };

            }

            //var billitem = db.Enrolments.Where(x => x.CustNo == enr.CustNo).OrderByDescending(x => x.EnrID).FirstOrDefault();

            //var billitems = new List<Enrolment>();
            //billitems.Add(billitem);

            //int invid = 0;
        //    int invid = CreateInvoiceByStudent(pb.CustNo);
        ///   string mth = enr.EnrDate.ToShortDateString();

         //   CreateBillableItems(mth, billitems);

         //   CreateInvDets(invid, month, billitems);

            ViewBag.StudentNo = enr.CustNo;

            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StudentsAll"] = db.Students.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ThenBy(x => x.TutorName).ToList();

            return Json(new { success = true,
                              invUrl = Url.Action("BatchInvoice", "CourseOrders"),
                              paymentUrl = Url.Action("PrEdit", "Payment", new { custid = enr.CustNo }),
                              redirectUrl = Url.Action("EnrolmentEdit", "CourseOrders", new { id = enr.CustNo })
            });


        }

        public ActionResult BatchInvoice()
        {

            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StudentsAll"] = db.Students.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ThenBy(x => x.TutorName).ToList();

            List<MonthAll> min = new List<MonthAll>();
            MonthAll mo = new MonthAll();
            string time;
            for (int i = 6; i > 0; i--)
            {
                mo = new MonthAll();
                time = Convert.ToDateTime(DateTime.Now.AddMonths(+i)).ToString("MM/yyyy");
                mo.Time = time;
                min.Add(mo);
            }
            mo = new MonthAll();
            time = Convert.ToDateTime(DateTime.Now).ToString("MM/yyyy");
           
            mo.Time = time;
            min.Add(mo);
            for (int i = 1; i < 13; i++)
            {
                mo = new MonthAll();
                time = Convert.ToDateTime(DateTime.Now.AddMonths(-i)).ToString("MM/yyyy");
                mo.Time = time;
                min.Add(mo);
            }
            ViewData["MonthData"] = min;
            return View();
        }

        [HttpGet]
        public ActionResult _EnrolmentResult(string p1, string p2, string p3, string p4, string p5, string p6)
        {
            int custno = 0;
            int courseno = 0;
            string courselevel = "";
            int teacherno = 0;
            int weekday = -1;
            int year = DateTime.Now.Year;

            if (!string.IsNullOrEmpty(p1.Trim()))
            {
                custno = Convert.ToInt32(p1.Trim());
            }
            if (!string.IsNullOrEmpty(p2.Trim()))
            {
                courseno = Convert.ToInt32(p2.Trim());
            }
            if (!string.IsNullOrEmpty(p3.Trim()))
            {
                courselevel = p3.Trim();
            }
            if (!string.IsNullOrEmpty(p4.Trim()))
            {
                teacherno = Convert.ToInt32(p4.Trim());
            }
            if (!string.IsNullOrEmpty(p5.Trim()))
            {
                weekday = Convert.ToInt32(p5.Trim());
            }
            if (!string.IsNullOrEmpty(p6.Trim()))
            {
                year = Convert.ToInt32(p6.Trim());
            }

          //  var plist = new List<Enrolment>();

          var plist = db.Enrolments.Where(x => x.CustNo != 0 && x.Status != "Void").OrderBy(x => x.EnrNo).ToList();
          //   plist = db.Enrolments.Where(x => x.IsValid == true).OrderByDescending(x => x.EnrNo).Take(1000).ToList();

            if (custno != 0)
            {
                plist = db.Enrolments.Where(x => x.CustNo == custno).OrderByDescending(x => x.EnrNo).Take(1000).ToList();
            };
            if (courseno != 0)
            {
                plist = plist.Where(x => x.CourseID == courseno).OrderByDescending(x => x.EnrNo).Take(1000).ToList();
            };
            if (courselevel != "")
            {
                plist = plist.Where(x => x.CourseLevel.Contains(courselevel)).OrderByDescending(x => x.EnrNo).Take(1000).ToList();
            };
            if (teacherno != 0)
            {
                plist = plist.Where(x => x.TutorID == teacherno).OrderByDescending(x => x.EnrNo).Take(1000).ToList();
            };
            if (weekday != -1)
            {
                plist = plist.Where(x => x.Weekday == weekday).OrderByDescending(x => x.EnrNo).Take(1000).ToList();
            };

            plist = plist.Take(600).ToList();

            EnrolmentSelectionView model = new EnrolmentSelectionView();
            foreach (var p in plist)
            {
                var selecteditor = new EnrolmentSelectEditor()
                {
                    Selected = false,
                    EnrID = p.EnrID,
                    EnrNo = p.EnrNo,
                    EnrType = p.EnrType,
                    EnrDate = p.EnrDate,
                    CustNo = p.CustNo,
                    CustName = p.CustName,
                    CustName2 = p.CustName2,
                    NRIC = p.NRIC,
                    CourseID = p.CourseID,
                    CourseCode = p.CourseCode,
                    CourseType = p.CourseType,
                    CourseName = p.CourseName,
                    CourseLevel = p.CourseLevel,
                    CourseDuration = p.CourseDuration,
                    TeacherLevel = p.TeacherLevel,
                    OptionName = p.OptionName,
                    TutorID = p.TutorID,
                    TutorName = p.TutorName,
                    TermID = p.TermID,
                    TermName = p.TermName,
                    Weekday = p.Weekday,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    RegisterFee = p.RegisterFee,
                    CourseFee = p.CourseFee,
                    Deposit = p.Deposit,
                    SalesType = p.SalesType,
                    IsBillable = p.IsBillable,
                    BillRemark = p.BillRemark,
                    IsValid = p.IsValid,
                    Status = p.Status,
                    Remark = p.Remark,
                    PersonID = p.PersonID,
                    PersonName = p.PersonName,
                    CreatedBy = p.CreatedBy,
                    CreatedOn = p.CreatedOn

                };
                model.DataSelects.Add(selecteditor);
            };

            return PartialView(model);
        }


        [HttpPost]
        public ActionResult _EnrolmentResult(EnrolmentSelectionView model, string valSorID, string valMonth)
        //     public ActionResult _SubmitSelected(ProductSelectionView model)
        {
            //   int sorid = Convert.ToInt32(Request.Form["valSorID"]);
            int sorid = Convert.ToInt32(valSorID);
            var mth = Request.Form["valMonth"];
            var selectedview = new EnrolmentSelectionView();
            // get the ids of the items selected:
            List<int> selectedIds = model.getSelectedIds().ToList();

            // Use the selected ids to retrieve the selected enrolments
            // get customer no.s from the selected enrolments 
            var selectedItems = db.Enrolments.Where(x => selectedIds.Contains(x.EnrID)).ToList();

            List<int> custids = (from p in selectedItems select p.CustNo).Distinct().ToList();

            int invid = 0;
            foreach (var custid in custids) 
            {
                var billitems = selectedItems.Where(x => x.CustNo == custid).ToList();

                // create billable items
                var detitems = CreateBillableItems(custid, mth, billitems);

                string[] mths = mth.Split(',').ToArray();
                mths = mths.OrderBy(x => DateTime.Parse(x)).ToArray();

                foreach (var mm in mths)
                {
                    DateTime selectedMonth = DateTime.Parse(mm);
                    var dets = detitems.Where(x => x.BillForMonth.Month == selectedMonth.Month && x.BillForMonth.Year == selectedMonth.Year).ToList();

                    if (dets.Count > 0)
                    {
                        invid = CreateInvoiceByStudent(custid, mm);
                        CreateInvDets(invid, mm, dets);
                    }
                }

            }

            //foreach (var p in selectedItems)
            //{
            //    // add this product into INVDET
            //    if (p != null)
            //    {
            //        CreateInvoice(p.EnrID);
            //      //  AddIntoINVDET(p.CourseID, sorid);
            //    }
            //};

            //     return PartialView(selectedview);
          //  return Json(new { success = true });

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("View", "Sales", new { id = invid })
            });

        }

        public List<BillItem> CreateBillableItems(int custid, string month, List<Enrolment> billitems)
   //     private void CreateBillableItems(string month, List<Enrolment> billitems)
        {
            var bitems = new List<BillItem>();

            // check if admin fee is billed, if not then add to this bill
            var det0 = db.BillItems.Where(x => (x.CustNo == custid) && (x.DetType == "OTHER") && (x.ItemCode == "REGISTER_FEE") && (x.IsBilled == false)).FirstOrDefault();
            if (det0 != null)
            {
                bitems.Add(det0);
            }

            string[] mths = month.Split(',').ToArray();
          //  Array.Sort(mths);
            mths = mths.OrderBy(x => DateTime.Parse(x)).ToArray();
            
            foreach (var mth in mths)
            {
               // DateTime myDate = DateTime.Parse(mth);
                DateTime selectedMonth;
                if (DateTime.TryParse(mth, out selectedMonth))
                {
                    selectedMonth = DateTime.Parse(mth);
                }
                else
                {
                    selectedMonth = DateTime.Now;
                }

                //var firstDayOfMonth = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
                //var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // var today = DateTime.Today;
                var today = selectedMonth;
                //    var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

                // var givendate = DateTime.Today;
                var givendate = selectedMonth;
                var firstDayOfMonth = new DateTime(givendate.Year, givendate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                var daysInMonth = DateTime.DaysInMonth(givendate.Year, givendate.Month);


                billitems = billitems.Where(x => x.Status != "Void" && x.StartDate.Date <= lastDayOfMonth.Date).ToList();

                foreach (var enr in billitems)
                {
                    var days = "";
                    int countdays = 1;
                    if (enr != null)
                    {
                        // check if the course item already billed for the month, if not then create billable items
                        var item = db.BillItems.Where(x => (x.EnrID == enr.EnrID) && (x.DetType == "REGULAR") && (x.IsBilled == true) && (x.BillForMonth.Month == selectedMonth.Month && x.BillForMonth.Year == selectedMonth.Year)).FirstOrDefault();
                        if (item == null)
                        {
                            var items = db.BillItems.Where(x => (x.EnrID == enr.EnrID) && (x.DetType == "REGULAR") && (x.IsBilled == false) && (x.BillForMonth.Month == selectedMonth.Month && x.BillForMonth.Year == selectedMonth.Year)).ToList();
                            foreach (var k in items)
                            {
                                if (k != null)
                                {
                                    db.BillItems.Remove(k);
                                    db.SaveChanges();
                                }
                            }

                            // create course fee item


                            var startdate = firstDayOfMonth;
                            var enddate = lastDayOfMonth;
                            var weekday = enr.Weekday;

                            if (enr.StartDate >= startdate && enr.StartDate <= enddate)
                            {
                                startdate = enr.StartDate;
                            };

                            if (enr.EndDate != null && enr.EndDate <= lastDayOfMonth)
                            {
                                enddate = Convert.ToDateTime(enr.EndDate);
                            };

                            // default fromDay = 1, endDay = last day of the month;
                            int fromDay = firstDayOfMonth.Day;
                            int endDay = lastDayOfMonth.Day;
                            // var fromDay = 1;
                            // var endDay = DateTime.DaysInMonth(today.Year, today.Month);

                            // if enrolment start date after 1st day of current month, then assign fromDay to enrolment start date
                            if (startdate >= firstDayOfMonth)
                            {
                                fromDay = startdate.Day;
                            };
                            // if enrolment end date not null and end date < lastDayOfMonth, then assign endDay to enrolment end date
                            if (enddate != null && enddate <= lastDayOfMonth)
                            {
                                endDay = Convert.ToDateTime(enddate).Day;
                            };

                            string weekdayname = Enum.GetName(typeof(DayOfWeek), enr.Weekday);

                            var dates = getWeekdatesandDates(givendate.Month, givendate.Year);
                            dates = dates.Where(x => x.DayOfWeek.ToString() == (Enum.GetName(typeof(DayOfWeek), enr.Weekday))).ToList();
                            dates = dates.Where(x => x.Date >= startdate.Date && x.Date <= enddate.Date).ToList();

                            //var dates = Enumerable.Range(1, daysInMonth)
                            //                            .Select(n => new DateTime(today.Year, today.Month, n))
                            //                            .Where(date => date.DayOfWeek == DayOfWeek.Monday)
                            //                            .ToList();
                            foreach (var dd in dates)
                            {
                                days = days + dd.Date.Day.ToString() + "/" + dd.Date.Month.ToString() + ",";
                            }
                            if (!string.IsNullOrEmpty(days))
                            {
                                days = days.Remove(days.Length - 1, 1);
                            }

                            countdays = dates.Count();

                            // unit price base on per 4 unit (dates)
                            int baseqty = 4;

                            BillItem det = new BillItem();
                            det.DetType = "REGULAR";
                            det.EnrID = enr.EnrID;
                            det.EnrNo = enr.EnrNo;
                            det.CustNo = enr.CustNo;
                            det.CustName = enr.CustName;
                            det.CustName2 = enr.CustName2;
                            det.NRIC = enr.NRIC;
                            det.BillForMonth = selectedMonth;
                            det.ClassDates = days;

                            det.ItemID = enr.CourseID;
                            det.ItemCode = enr.CourseCode;
                            det.ItemType = enr.CourseType;

                            string weekdaytxt = Enum.GetName(typeof(DayOfWeek), enr.Weekday);

                            det.ItemName = enr.CourseName + " " + weekdaytxt + " " + enr.StartTimeValue + " - " + enr.EndTimeValue;
                            det.Remark = days;
                            det.ItemDesc = enr.CourseName + " " + weekdaytxt + " " + enr.StartTimeValue + " - " + enr.EndTimeValue;
                            det.SellType = "CS";
                            det.Qty = countdays;
                            det.Unit = "";
                            det.UnitPrice = enr.CourseFee;
                            det.DiscountedPrice = enr.CourseFee;
                            det.PreDiscAmount = enr.CourseFee;
                            det.Discount = 0;

                            decimal unitfee = System.Math.Round((det.DiscountedPrice / baseqty), 2, MidpointRounding.AwayFromZero);
                            det.Amount = System.Math.Round((unitfee * Convert.ToDecimal(det.Qty)), 2, MidpointRounding.AwayFromZero);

                            det.Gst = 0;
                            det.Nett = det.Amount + det.Gst;
                            det.IsBundle = false;
                            det.SalesType = enr.SalesType;
                            det.RefItemID = 0;
                            det.InvRef = "";
                            det.IsControlItem = false;
                            det.LocationID = 0;
                            det.LocationName = "";
                            det.Position = 1;
                            det.CreatedBy = User.Identity.Name;
                            det.CreatedOn = DateTime.Now;

                            if (ModelState.IsValid)
                            {
                                db.BillItems.Add(det);
                                db.SaveChanges();
                            }

                            bitems.Add(det);

                            // create course register fee item if any

                            if (enr.RegisterFee > 0)
                            {
                                BillItem det1 = new BillItem();
                                det1.DetType = "OTHER";
                                det1.EnrID = enr.EnrID;
                                det1.EnrNo = enr.EnrNo;
                                det1.CustNo = enr.CustNo;
                                det1.CustName = enr.CustName;
                                det1.CustName2 = enr.CustName2;
                                det1.NRIC = enr.NRIC;
                                det1.BillForMonth = selectedMonth;

                                det1.ItemID = 0;
                                det1.ItemCode = "REGISTER_FEE";
                                det1.ItemType = "OT";
                                det1.ItemName = "Course Register Fee";
                                det1.ItemDesc = "Course Register Fee";
                                det1.Remark = "";
                                det1.SellType = "CS";
                                det1.Qty = 1;
                                det1.Unit = "";
                                det1.UnitPrice = enr.RegisterFee;
                                det1.DiscountedPrice = enr.RegisterFee;
                                det1.PreDiscAmount = enr.RegisterFee;
                                det1.Discount = 0;
                                det1.Amount = enr.RegisterFee;
                                det1.Gst = 0;
                                det1.Nett = det1.Amount + det1.Gst;
                                det1.IsBundle = false;

                                det1.IsBilled = true;
                                det1.IsPaid = false;
                                det1.IsLocked = false;

                                det1.SalesType = enr.SalesType;
                                det1.RefItemID = 0;
                                det1.InvRef = "";
                                det1.IsControlItem = false;
                                det1.LocationID = 0;
                                det1.LocationName = "";
                                det1.Position = 1;
                                det1.CreatedBy = User.Identity.Name;
                                det1.CreatedOn = DateTime.Now;


                                if (ModelState.IsValid)
                                {
                                    db.BillItems.Add(det1);
                                    db.SaveChanges();
                                }

                                bitems.Add(det1);

                            };


                        }

                    }

                }


                // check if any redund items if yes then add to this bill
                var det2 = db.BillItems.Where(x => (x.CustNo == custid) && (x.DetType == "REFUND") && (x.IsBilled == false)).ToList();


                var ritems = db.BillItems.Where(x => (x.CustNo == custid) && (x.DetType == "REFUND") && (x.IsBilled == false) && (x.BillForMonth.Month <= selectedMonth.Month && x.BillForMonth.Year == selectedMonth.Year)).ToList();
                foreach (var r in ritems)
                {
                    if (r != null)
                    {
                        bitems.Add(r);
                    }
                }
            }

            return bitems;
        }

        public int CreateInvoiceByStudent(int custid, string month)
        {
            // ************  Get new invoice number *********************

            string newInvNo = "";
           // newInvNo = GetMaxCreditInvoiceNumber();

            var sp = db.Staffs.Where(x => x.Email == User.Identity.Name).FirstOrDefault();
            if (sp != null && !string.IsNullOrEmpty(sp.DepartmentName))
            {
                if (sp.DepartmentName == "JE")
                {
                    newInvNo = GetSerialNumber(203);
                }
                else
                {
                    newInvNo = GetSerialNumber(103);
                }
            }
            else
            {
                newInvNo = GetSerialNumber(103);
            }

            var cust = db.Clients.Find(custid);
            //  var sor = db.Enrolments.Find(enrid);

            INV invs = new INV();
            invs.InvNo = newInvNo;
            invs.SorID = 0;
            invs.InvType = "CO";
            invs.InvDate = DateTime.Today;
            //   invs.PoNo = sor.EnrID.ToString();
            invs.InvRef = month;
            invs.CustNo = cust.CustNo;
            invs.CustName = cust.CustName;
            invs.CustName2 = cust.CustName2;
            invs.BranchID = cust.BranchID;
            invs.Addr1 = cust.Addr1;
            invs.Addr2 = cust.Addr2;
            invs.Addr3 = cust.Addr3;
            invs.Addr4 = "";
            invs.Attn = "";
            invs.DeliveryAddress = "";
            invs.DeliveryTime = "";
            invs.DeliveryDate = null;
            invs.PreDiscAmount = 0;
            invs.Discount = 0;
            invs.Amount = 0;
            invs.Gst = 0;
            invs.Nett = 0;
            invs.PaidAmount = 0;
            invs.Status = "Invoiced";
            invs.PaymentStatus = "Unpaid";
            invs.PaymentTerms = "";
            invs.LocationID = 0;
            invs.LocationName = "";
            //    invs.Remark = sor.Remark;
            invs.PersonID = 0;
            invs.PersonName = "";

            invs.CreatedBy = User.Identity.Name;
            invs.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.INVs.Add(invs);
                db.SaveChanges();
            }

            int invid = GetLastInsertedId();

            //if (invid > 0)
            //{
            //    CreateInvDetWithInvID(invid);
            //}

            return invid;

        }


        private void CreateInvDets(int invid, string month, List<BillItem> billitems)
        {
            var inv = db.INVs.Where(x => x.InvID == invid).FirstOrDefault();

            if (inv != null)
            {
                foreach (var enr in billitems)
                {
                    var bi = db.BillItems.Find(enr.DetID);
                    if (bi != null)
                    {
                        bi.IsBilled = true;

                        if (ModelState.IsValid)
                        {
                            db.Entry(bi).State = EntityState.Modified;
                            db.SaveChanges();
                        };
                    }

                    double lastposition = db.INVDETs.Where(x => x.InvID == inv.InvID).Count(); 

                    INVDET det = new INVDET();
                    det.DetType = enr.DetType;
                    det.InvID = inv.InvID;
                    det.InvNo = inv.InvNo;
                    det.EorID = 0;
                    det.CnID = 0;
                    det.InvType = inv.InvType;
                    det.ItemID = enr.ItemID;
                    det.ItemCode = enr.ItemCode;
                    det.ItemType = enr.ItemType;
                    det.ItemName = enr.ItemName;
                    det.ItemDesc = enr.ItemDesc;
                    det.Remark = enr.Remark;
                    det.SellType = "CS";
                    det.Qty = enr.Qty;
                    det.Unit = enr.Unit;
                    det.UnitPrice = enr.UnitPrice;
                    det.DiscountedPrice = enr.DiscountedPrice;
                    det.PreDiscAmount = enr.PreDiscAmount;
                    det.Discount = enr.Discount;
                    det.Amount = enr.Amount;
                    det.Gst = enr.Gst;
                    det.Nett = enr.Nett;
                    det.IsBundle = false;
                    det.SalesType = enr.SalesType;
                    det.RefItemID = 0;
                    det.InvRef = "";
                    det.IsControlItem = false;
                    det.LocationID = 0;
                    det.LocationName = "";

                    det.Position = lastposition + 1;

                    if (ModelState.IsValid)
                    {
                        db.INVDETs.Add(det);
                        db.SaveChanges();
                    }                

                }

                UpdateInvoiceAmount(invid); 
            }
        }

        public List<DateTime> getWeekdatesandDates(int Month, int Year)
        {
            List<DateTime> weekdays = new List<DateTime>();

            DateTime firstOfMonth = new DateTime(Year, Month, 1);

            DateTime currentDay = firstOfMonth;
            while (firstOfMonth.Month == currentDay.Month)
            {
                DayOfWeek dayOfWeek = currentDay.DayOfWeek;
             //   if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
                    weekdays.Add(currentDay);

                currentDay = currentDay.AddDays(1);
            }

            return weekdays;
        }


        private void CreateInvoice(int enrid)
        {
            // ************  Get new invoice number *********************

            string newInvNo = "";
            newInvNo = GetMaxCreditInvoiceNumber();

            var sor = db.Enrolments.Find(enrid);
            var cust = db.Students.Find(sor.CustNo);
 
            // *** creating new invoice

            INV invs = new INV();
            invs.InvNo = newInvNo;
            invs.SorID = sor.EnrID;
            invs.InvType = "CO";
            invs.InvDate = DateTime.Today;
            invs.PoNo = sor.EnrID.ToString();
            invs.InvRef = sor.EnrNo;
            invs.CustNo = sor.CustNo;
            invs.CustName = sor.CustName;
            invs.CustName2 = sor.CustName2;
            invs.Addr1 = cust.Addr1;
            invs.Addr2 = cust.Addr2;
            invs.Addr3 = cust.Addr3;
            invs.Addr4 = "";
            invs.Attn = "";
            invs.DeliveryAddress = "";
            invs.DeliveryTime = "";
            invs.DeliveryDate = null;
            invs.PreDiscAmount = 0;
            invs.Discount = 0;
            invs.Amount = 0;
            invs.Gst = 0;
            invs.Nett = 0;
            invs.PaidAmount = 0;
            invs.Status = "Invoiced";
            invs.PaymentStatus = "Unpaid";
            invs.PaymentTerms = "";
            invs.LocationID = 0;
            invs.LocationName = "";
            invs.Remark = sor.Remark;
            invs.PersonID = sor.PersonID;
            invs.PersonName = sor.PersonName;

            invs.CreatedBy = User.Identity.Name;
            invs.CreatedOn = DateTime.Now;        

            if (ModelState.IsValid)
            {
                db.INVs.Add(invs);
                db.SaveChanges();                
            }

            int invid = GetLastInsertedId();

            if (invid > 0)
            {
                CreateInvDetWithInvID(invid);
            }


        }

        public int GetLastInsertedId()
        {
            return db.INVs.OrderByDescending(x => x.InvID).First().InvID;

        }

        private void CreateInvDetWithInvID(int invID)
        {
            var inv = db.INVs.Where(x => x.InvID == invID).FirstOrDefault();

            int enrid = Convert.ToInt32(inv.PoNo);
            var enr = db.Enrolments.Find(enrid);
            var days = "";
            int countdays = 1;

            if (enr != null)
            {
                var startdate = enr.StartDate;
                var enddate = enr.EndDate;
                var weekday = enr.Weekday;
                string weekdaytxt = ((DayOfWeek)enr.Weekday).ToString();
                //var today = DateTime.Today;
                //var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

                DateTime givendate = DateTime.Today;
                var firstDayOfMonth = new DateTime(givendate.Year, givendate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // default fromDay = 1, endDay = last day of the month;
                var fromDay = firstDayOfMonth.Day;
                var endDay = lastDayOfMonth.Day;
               // var fromDay = 1;
               // var endDay = DateTime.DaysInMonth(today.Year, today.Month);

                // if enrolment start date after 1st day of current month, then assign fromDay to enrolment start date
                if (startdate >= firstDayOfMonth)
                {
                    fromDay = startdate.Day;
                };
                // if enrolment end date not null and end date < lastDayOfMonth, then assign endDay to enrolment end date
                if (enddate != null && enddate <= lastDayOfMonth)
                {
                    endDay = Convert.ToDateTime(enddate).Day;
                };
                
                var dates = Enumerable.Range(fromDay, endDay)
                                            .Select(n => new DateTime(givendate.Date.Year, givendate.Date.Month, n))
                                            .Where(date => date.DayOfWeek.ToString() == weekdaytxt)
                                            .ToList();
                foreach (var dd in dates)
                {
                    days = days + dd.Date.Day.ToString() + "/" + dd.Date.Month.ToString() + ",";
                }
                days = days.Remove(days.Length - 1, 1);
                countdays = dates.Count();
            }

            if (inv != null)
            {
                // unit price base on per 4 unit (dates)
                int baseqty = 4;

                INVDET det = new INVDET();
                det.InvID = inv.InvID;
                det.InvNo = inv.InvNo;
                det.EorID = 0;
                det.CnID = 0;
                det.InvType = inv.InvType;
                det.ItemID = enr.CourseID;
                det.ItemCode = enr.CourseCode;
                det.ItemType = enr.CourseType;

                string weekdaytxt = ((DayOfWeek)enr.Weekday).ToString();
                det.ItemName = enr.CourseName + " " + weekdaytxt  + " " + enr.StartTimeValue + " - " + enr.EndTimeValue;
                det.Remark = days;
                det.SellType = "CS";
                det.Qty = countdays;
                det.Unit = "";
                det.UnitPrice = enr.CourseFee;
                det.DiscountedPrice = enr.CourseFee;
                det.PreDiscAmount = enr.CourseFee;
                det.Discount = 0;

                decimal unitfee = System.Math.Round((det.DiscountedPrice / baseqty), 2, MidpointRounding.AwayFromZero);
                det.Amount = System.Math.Round((unitfee * Convert.ToDecimal(det.Qty)), 2, MidpointRounding.AwayFromZero);

                det.Gst = 0;
                det.Nett = det.Amount + det.Gst;
                det.IsBundle = false;
                det.SalesType = enr.SalesType;
                det.RefItemID = 0;
                det.InvRef = "";
                det.IsControlItem = false;
                det.LocationID = 0;
                det.LocationName = "";
                det.Position = 1;

 
                if (ModelState.IsValid)
                {
                    db.INVDETs.Add(det);
                    db.SaveChanges();
                }

                UpdateInvoiceAmount(invID);
 
            }
        }

        private void UpdateInvoiceAmount(int id)
        {
            //   decimal sumPreDiscAmount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.PreDiscAmount) ?? 0;
            //   decimal sumDiscount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.Discount) ?? 0;
            decimal sumAmount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.Amount) ?? 0;

            decimal gst = GetGstRate();

            //     decimal sumGst = sumAmount * gst;
            //     decimal sumNett = sumAmount + sumGst;

            INV inv = db.INVs.Find(id);
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


        public JsonResult _GetPricebookByCourseID(string itemid)
        {
            if (itemid != null)
            {
                //   int cid = Convert.ToInt32(custno);
                int iid = Convert.ToInt32(itemid);

                var c = db.Pricebooks.Where(x => x.CourseID == iid && x.IsValid == true).ToList();

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

        public JsonResult _GetPricebookByPriceID(string itemid)
        {
            if (itemid != null)
            {
                //   int cid = Convert.ToInt32(custno);
                int iid = Convert.ToInt32(itemid);

                var c = db.Pricebooks.Where(x => x.PriceID == iid && x.IsValid == true).FirstOrDefault();

                if (c != null)
                {
                    return Json(new { success = true, result = c }, JsonRequestBehavior.AllowGet);
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

        public JsonResult _GetScheduleByID(string itemid)
        {
            if (itemid != null)
            {
                //   int cid = Convert.ToInt32(custno);
                int iid = Convert.ToInt32(itemid);

                var c = db.ClassSchedules.Where(x => x.ScheduleID == iid).FirstOrDefault();

                if (c != null)
                {
                    return Json(new { success = true, result = c }, JsonRequestBehavior.AllowGet);
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


        public void UpdetStatus(int ID)
        {
            
            Enrolment enrol = db.Enrolments.Find(ID);
            enrol.Status = "Void";
            db.Entry(enrol).State = EntityState.Modified;
            db.SaveChanges();
            Response.End();

            //return "1";
        }





    }
}