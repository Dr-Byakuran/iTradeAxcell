using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using iTrade.Models;


namespace iTrade.Controllers
{
    //[Authorize]
    public class ControllerBase : Controller
    {
        private StarDbContext db = new StarDbContext();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }


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

        //************************** Autocomplete for Customers ******************************//


        public JsonResult AutoCompleteSelectedStudent(string search)
        {
            if (search != null)
            {
                int custno = Convert.ToInt32(search);
                var c = db.Students
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


        public JsonResult AutoComplete(string search)
        {
            var data = db.Clients
                       .Where(x => ((x.CustName.ToUpper().StartsWith(search.ToUpper())) || (x.CustNo.ToString().StartsWith(search))) && ((x.IsActive == true)))
                       .ToList().Distinct().ToList();

            //   var result = data.Where(x => x.HeatNo.ToLower().StartsWith(search.ToLower())).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //  [HttpPost]
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

        //************************** Autocomplete for Vendors ******************************//

        //  [HttpPost]
        public JsonResult AutoCompleteSelectedVendor(string search)
        {
            if (search != null)
            {
                int custno = Convert.ToInt32(search);
                var c = db.Vendors
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

        //************************** Autocomplete for Products ******************************//

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

        //  [HttpPost]
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
                                                  CostCode = p.CostCode,
                                                  SellPrice = p.RetailPrice,
                                                  IsControlItem = p.IsControlItem,
                                                  AvailableQty = 0

                                              }).ToList();

            return getList;

        }

        public string GetMaxStudentNumber()
        {
            Int32 initNumber = 1001;
            string maxNumber = "";
            var maxTopNo = (from max in db.Students
                            where (max.AccNo != null || max.AccNo != "")
                            select max.AccNo).Max();
            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo);
            }

            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = maxno.ToString();

            return maxNumber;
        }

        public string GetMaxClientNumber()
        {
            Int32 initNumber = 1001;
            string maxNumber = "";
            var maxTopNo = (from max in db.Clients
                            where (max.AccNo != null || max.AccNo != "")
                            select max.AccNo).Max();
            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo);
            }

            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = maxno.ToString();

            return maxNumber;
        }

        public string GetMaxEnrolNumber()
        {
            Int32 initNumber = 16001;
            string maxNumber = "";
            var maxTopNo = (from max in db.Enrolments
                            where (max.EnrNo != null || max.EnrNo != "")
                            select max.EnrNo).Max();
            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo);
            }

            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = maxno.ToString();

            return maxNumber;
        }


        public string GetMaxQuotationNumber()
        {
            Int32 initNumber = 16000100;
            string maxNumber = "";
            var maxTopNo = (from max in db.Quotations
                            where (max.QuoNo != null || max.QuoNo != "")
                            select max.QuoNo).Max();
            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("QN", ""));
            }

            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = "QN" + maxno.ToString();

            return maxNumber;
        }

        public string GetMaxOrderNumber()
        {
            Int32 initNumber = 16002000;
            string maxNumber = "";
            var maxTopNo = (from max in db.SalesOrders
                            where (max.SorNo != null || max.SorNo != "")
                            select max.SorNo).Max();
            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("SO", ""));
            }

            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = "SO" + maxno.ToString();
 
            return maxNumber;  
        }

        public string GetMaxCashInvoiceNumber()
        {
            Int32 initNumber = 160001;
            string maxNumber = "";
            var maxTopNo = (from max in db.INVs
                            where ((max.InvNo != null || max.InvNo != "") && (max.InvType == "CS") && (!max.InvNo.StartsWith("CR")) && (!max.InvNo.StartsWith("CS")))
                            select max.InvNo).Max();

            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("CS", ""));
            }
            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = maxno.ToString();
        //    maxNumber = "CS" + maxno.ToString();

            return maxNumber;          
        }

        public string GetMaxCreditInvoiceNumber()
        {
            Int32 initNumber = 80001;
            string maxNumber = "";
            var maxTopNo = (from max in db.INVs
                            where (max.InvNo != null || max.InvNo != "")
                            select max.InvNo).Max();

            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("CR", ""));
            }
            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = "CR" + maxno.ToString();

            return maxNumber;
        }

        public string GetMaxKivNumber()
        {
            Int32 initNumber = 151616;
            string maxNumber = "";
            var maxTopNo = (from max in db.KivOrders 
                            where ((max.KorNo != null || max.KorNo != ""))
                            select max.KorNo).Max();

            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("CF", ""));
            }
            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = "CF" + maxno.ToString();

            return maxNumber;
        }

        public string GetMaxKivEorNumber()
        {
            Int32 initNumber = 300;
            string maxNumber = "";
            var maxTopNo = (from max in db.KivExchangeOrders
                            where ((max.KivEorNo != null || max.KivEorNo != ""))
                            select max.KivEorNo).Max();

            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("KIVEC", ""));
            }
            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = "KIVEC" + maxno.ToString();

            return maxNumber;
        }

        public string GetMaxExchangeNumber()
        {
            Int32 initNumber = 15654;
            string maxNumber = "";
            var maxTopNo = (from max in db.ExchangeOrders 
                            where ((max.EorNo != null || max.EorNo != ""))
                            select max.EorNo).Max();

            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("EC", ""));
            }
            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = "EC" + maxno.ToString();

            return maxNumber;
        }

        public string GetMaxCreditNoteNumber()
        {
            Int32 initNumber = 150708;
            string maxNumber = "";
            var maxTopNo = (from max in db.CreditNotes
                            where ((max.CnNo != null || max.CnNo != ""))
                            select max.CnNo).Max();

            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("CN", ""));
            }
            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = "CN" + maxno.ToString();

            return maxNumber;
        }

        public string GetMaxPaymentNumber()
        {
            Int32 initNumber = 160100;
            string maxNumber = "";
            var maxTopNo = (from max in db.PaymentReceipts
                            where ((max.PrNo != null || max.PrNo != ""))
                            select max.PrNo).Max();

            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("PY", ""));
            }
            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = "PY" + maxno.ToString();

            return maxNumber;
        }

        public string GetMaxPaymentVoucherNumber()
        {
            Int32 initNumber = 16300;
            string maxNumber = "";
            var maxTopNo = (from max in db.PaymentVouchers
                            where ((max.PvNo != null || max.PvNo != ""))
                            select max.PvNo).Max();

            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("PV", ""));
            }
            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = "PV" + maxno.ToString();

            return maxNumber;
        }

        public string GetMaxPurchaseOrderNumber()
        {
            Int32 initNumber = 1606000;
            string maxNumber = "";
            var maxTopNo = (from max in db.PurchaseOrders
                            where (max.SorNo != null || max.SorNo != "")
                            select max.SorNo).Max();
            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("PO", ""));
            }

            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = "PO" + maxno.ToString();

            return maxNumber;
        }

        public string GetMaxPurchaseInvoiceNumber()
        {
            Int32 initNumber = 20160100;
            string maxNumber = "";
            var maxTopNo = (from max in db.PoINVs
                            where (max.InvNo != null || max.InvNo != "")
                            select max.InvNo).Max();

            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("PI", ""));
            }
            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = "PI" + maxno.ToString();

            return maxNumber;
        }

        public string GetMaxPoKivNumber()
        {
            Int32 initNumber = 151616;
            string maxNumber = "";
            var maxTopNo = (from max in db.PoKivOrders
                            where ((max.KorNo != null || max.KorNo != ""))
                            select max.KorNo).Max();

            Int32 maxno = 0;
            if (!string.IsNullOrEmpty(maxTopNo))
            {
                maxno = int.Parse(maxTopNo.Replace("GRN", ""));
            }
            if (maxno >= initNumber)
            {
                maxno = maxno + 1;
            }
            else
            {
                maxno = initNumber;
            }
            maxNumber = "GRN" + maxno.ToString();

            return maxNumber;
        }


        public List<Client> GetClientListByUser(string invtype)
        {
            // invtype value: ALL or CS or CR

            List<Client> getList = new List<Client>();

            if (User.IsInRole("Admin") || User.IsInRole("Management"))
            {
                if (invtype == "ALL")
                {
                    getList = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
                }
                else
                {
                    getList = db.Clients.Where(x => x.AccType == invtype && x.IsActive == true).OrderBy(x => x.CustName).ToList();
                }
            }
            else
            {
                // check login user
                // get staffid by login user
                // list customers by current staff

                var staff = db.Staffs.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

                if (staff != null)
                {
                    if (invtype == "ALL")
                    {
                        getList = db.Clients.Where(x => (x.SalesPersonID == staff.StaffID || x.SalesPersonID == 0) && x.IsActive == true).OrderBy(x => x.CustName).ToList();
                    }
                    else
                    {
                        getList = db.Clients.Where(x => x.AccType == invtype && (x.SalesPersonID == staff.StaffID || x.SalesPersonID == 0) && x.IsActive == true).OrderBy(x => x.CustName).ToList();
                    }

                }
                else
                {
                    if (invtype == "ALL")
                    {
                        getList = db.Clients.Where(x => x.SalesPersonID == 0 && x.IsActive == true).OrderBy(x => x.CustName).ToList();
                    }
                    else
                    {
                        getList = db.Clients.Where(x => x.AccType == invtype && x.SalesPersonID == 0 && x.IsActive == true).OrderBy(x => x.CustName).ToList();
                    }
                }

            };

            return getList;

        }

        public List<Vendor> GetVendorListByUser(string invtype)
        {
            // invtype value: ALL or CS or CR

            List<Vendor> getList = new List<Vendor>();

            if (User.IsInRole("Admin") || User.IsInRole("Management"))
            {
                if (invtype == "ALL")
                {
                    getList = db.Vendors.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
                }
                else
                {
                    getList = db.Vendors.Where(x => x.AccType == invtype && x.IsActive == true).OrderBy(x => x.CustName).ToList();
                }
            }
            else
            {
                // check login user
                // get staffid by login user
                // list customers by current staff

                var staff = db.Staffs.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

                if (staff != null)
                {
                    if (invtype == "ALL")
                    {
                        getList = db.Vendors.Where(x => (x.SalesPersonID == staff.StaffID || x.SalesPersonID == 0) && x.IsActive == true).OrderBy(x => x.CustName).ToList();
                    }
                    else
                    {
                        getList = db.Vendors.Where(x => x.AccType == invtype && (x.SalesPersonID == staff.StaffID || x.SalesPersonID == 0) && x.IsActive == true).OrderBy(x => x.CustName).ToList();
                    }

                }
                else
                {
                    if (invtype == "ALL")
                    {
                        getList = db.Vendors.Where(x => x.SalesPersonID == 0 && x.IsActive == true).OrderBy(x => x.CustName).ToList();
                    }
                    else
                    {
                        getList = db.Vendors.Where(x => x.AccType == invtype && x.SalesPersonID == 0 && x.IsActive == true).OrderBy(x => x.CustName).ToList();
                    }
                }

            };

            return getList;

        }

        protected string GetSerialNumber(int typeCode)
        {
            string result = "";
            SerialNumber serial = db.SerialNumbers.Where(s => s.TypeCode == typeCode).FirstOrDefault();
            if (serial == null)
            {
                serial = new SerialNumber { Id = 0, TypeCode = typeCode };

                // HQ OUTLET
                if (typeCode == 100)
                {

                    serial.Prefix = "HQ-QTN";
                    serial.CurrNumber = 0;
                }
                if (typeCode == 101)
                {

                    serial.Prefix = "HQ-SOR";
                    serial.CurrNumber = 0;
                }
                if (typeCode == 102)
                {

                    serial.Prefix = "HQ-PRJ";
                    serial.CurrNumber = 0;
                }
                if (typeCode == 103)
                {

                    serial.Prefix = "HQ-INV";
                    serial.CurrNumber = 0;
                }
                if (typeCode == 104)
                {

                    serial.Prefix = "HQ-CRN";
                    serial.CurrNumber = 0;
                }
                if (typeCode == 105)
                {

                    serial.Prefix = "HQ-DBN";
                    serial.CurrNumber = 0;
                }

                if (typeCode == 106)
                {

                    serial.Prefix = "HQ-DR";
                    serial.CurrNumber = 0;
                }
                if (typeCode == 107)
                {

                    serial.Prefix = "HQ-DO";
                    serial.CurrNumber = 0;
                }
                if (typeCode == 108)
                {

                    serial.Prefix = "HQ-PY";
                    serial.CurrNumber = 0;
                }


                //  OTHER OUTLET

                if (typeCode == 200)
                {

                    serial.Prefix = "SR-QTN";
                    serial.CurrNumber = 80000;
                }

                if (typeCode == 201)
                {

                    serial.Prefix = "SR-SOR";
                    serial.CurrNumber = 80000;
                }
                if (typeCode == 202)
                {

                    serial.Prefix = "SR-PRJ";
                    serial.CurrNumber = 80000;
                }
                if (typeCode == 203)
                {

                    serial.Prefix = "SR-INV";
                    serial.CurrNumber = 80000;
                }
                if (typeCode == 204)
                {

                    serial.Prefix = "SR-CRN";
                    serial.CurrNumber = 80000;
                }
                if (typeCode == 205)
                {

                    serial.Prefix = "SR-DBN";
                    serial.CurrNumber = 80000;
                }

                if (typeCode == 206)
                {

                    serial.Prefix = "SR-DR";
                    serial.CurrNumber = 80000;
                }
                if (typeCode == 207)
                {

                    serial.Prefix = "SR-DO";
                    serial.CurrNumber = 80000;
                }
                if (typeCode == 208)
                {

                    serial.Prefix = "SR-PY";
                    serial.CurrNumber = 80000;
                }                

            }

            string prefix = serial.Prefix;
            DateTime dt = DateTime.Now;
            string yy = dt.Year.ToString().Substring(2, 2);
            if (dt.Month == 12)
            {
                yy = dt.AddYears(1).ToString().Substring(2, 2);
            }

            int currNum = serial.CurrNumber;
            currNum++;

            result = prefix + yy + "-" + currNum.ToString("D5");

            serial.CurrNumber = currNum;
            if (serial.Id == 0)
            {
                db.SerialNumbers.Add(serial);
            }
            db.SaveChanges();

            return result;
        }


    }
}