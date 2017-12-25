using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MvcRazorToPdf;
using System.Web.Mvc;
using iTrade.Models;

namespace iTrade.Controllers
{
    public class KIVDeliveryController : Controller
    {
        private StarDbContext db = new StarDbContext();

        public ActionResult Index()
        {
            List<KIVDelivery> KIVDelivery = db.KIVDelivery.ToList();
            List<KIVDeliveryDetail> KIVDeliveryDetails = db.KIVDeliveryDetails.ToList();

            var query =
                (from k in KIVDelivery
                 select new
                 {
                     KIVDelID = k.KIVDelID,
                     CustName = db.Clients.Find(k.CustNo).CustName,
                     InvIDs = from s in KIVDeliveryDetails
                              where s.KIVDelID == k.KIVDelID
                              select s
                     ,
                     DeliveryDate = k.DeliveryDate,
                     DeliveryTime = k.DeliveryTime
                 }).Distinct();

            IEnumerable<KIVDeliveryIndexViewModel> queryKIV = from item in query.AsEnumerable()
                                                              select new KIVDeliveryIndexViewModel(item.KIVDelID, item.CustName, item.InvIDs, item.DeliveryDate, item.DeliveryTime);
            return View(queryKIV);
        }

        [HttpGet]
        public ActionResult Create()
        {
            KIVDeliveryCreateViewModel KIVDeliveryCreate = new KIVDeliveryCreateViewModel();

            KIVDelivery KIVDelivery = new KIVDelivery();
            INV invoice = new INV();
            IList<KIVDeliveryAll> KIVDETs = new List<KIVDeliveryAll>();

            KIVDeliveryCreate.Invoice = invoice;
            KIVDeliveryCreate.KIVDelivery = KIVDelivery;
            KIVDeliveryCreate.KIVDETs = KIVDETs;

            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();

            return View(KIVDeliveryCreate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string[] DetID, string[] TxtID, string strCustNo, string strLocation, KIVDelivery kivDelivery, INV invoice)
        {
            //Get KIV Delivery
            int arrayCount = DetID.Length;
            List<string> listDetId = new List<string>(DetID);
            List<string> listTxtId = new List<string>(TxtID);
            List<KIVDetailsForm> KIVDetails = new List<KIVDetailsForm>(arrayCount);

            for (int i = 0; i < arrayCount ; i++)
            {
                KIVDetailsForm KIVDetailsForm = new KIVDetailsForm();
                KIVDetailsForm.DetID = listDetId[i].ToString();
                KIVDetailsForm.KIVQty = listTxtId[i].ToString();
                KIVDetails.Add(KIVDetailsForm);
            }
            

            //Get CustNo
            int tempVal = 0;
            int? nullVal = Int32.TryParse(strCustNo, out tempVal) ? tempVal : (int?)null;
            int custNo = nullVal ?? 0;

            //Get Delivery Address
            string strAddress=string.Empty;
            if (kivDelivery.DeliveryAddress == null) strAddress = invoice.Addr1 + " " + invoice.Addr2 + " " + invoice.Addr3;
            else strAddress = kivDelivery.DeliveryAddress;

            using (var transaction = db.Database.BeginTransaction())
            {
               
            }

            KIVDelivery KIVDelivery = new KIVDelivery();
            db.KIVDelivery.Add(KIVDelivery);
            KIVDelivery.CustNo = custNo;
            KIVDelivery.DeliveryAddress = strAddress.TrimEnd();
            KIVDelivery.DeliveryDate = kivDelivery.DeliveryDate;
            KIVDelivery.DeliveryTime = kivDelivery.DeliveryTime;
            KIVDelivery.CreatedBy = User.Identity.Name;
            KIVDelivery.CreatedOn = DateTime.Now;
            db.SaveChanges();

            foreach (var item in KIVDetails)
            {
                int tempValDet = 0;
                double tempValKIVQty = 0;
                int? nullValDet = Int32.TryParse(item.DetID, out tempValDet) ? tempValDet : (int?)null;
                double? nullValKIVQty = Double.TryParse(item.KIVQty, out tempValKIVQty) ? tempValKIVQty : (double?)null;
                int detID = nullValDet ?? 0;
                double kivQty = nullValKIVQty ?? 0;

                if (kivQty > 0)
                {
                    double balQty = 0, delQty = 0, kivBalQty = 0, origBalQty = 0;

                    KIVDET KIVDET = db.KIVDETs.Find(detID);
                    origBalQty = KIVDET.BalanceQty;
                    balQty = origBalQty - kivQty;
                    delQty = KIVDET.DeliverQty + kivQty;
                    kivBalQty = KIVDET.KivBalanceQty - kivQty;

                    db.Entry(KIVDET).State = EntityState.Modified;
                    KIVDET.DeliverQty = delQty;
                    KIVDET.BalanceQty = kivBalQty;
                    KIVDET.KivBalanceQty = kivBalQty;
                    db.SaveChanges();

                    KIVDeliveryDetail KIVDeliveryDetail = new KIVDeliveryDetail();
                    db.KIVDeliveryDetails.Add(KIVDeliveryDetail);
                    KIVDeliveryDetail.InvID = KIVDET.InvID;
                    KIVDeliveryDetail.KIVDelID = KIVDelivery.KIVDelID;
                    KIVDeliveryDetail.DetID = detID;
                    KIVDeliveryDetail.DeliveryQty = kivQty;
                    KIVDeliveryDetail.BalanceQty = origBalQty;
                    KIVDeliveryDetail.KivBalanceQty = kivBalQty;
                    db.SaveChanges();
                }

            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            KIVDeliveryCreateViewModel KIVDeliveryCreate = new KIVDeliveryCreateViewModel();

            KIVDelivery KIVDelivery = db.KIVDelivery.Find(id);
            var Client = db.Clients.Where(m => m.CustNo == KIVDelivery.CustNo).ToList();
            INV invoice = new INV();
            IList<KIVDeliveryAll> KIVDETs = new List<KIVDeliveryAll>();

            KIVDelivery.Clients = Client;
            KIVDeliveryCreate.Invoice = invoice;
            KIVDeliveryCreate.KIVDelivery = KIVDelivery;
            KIVDeliveryCreate.KIVDETs = KIVDETs;

            return View(KIVDeliveryCreate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string[] DetID, string[] TxtID, string strCustNo, string strLocation, KIVDelivery kivDelivery, INV invoice)
        {
            //Get KIV Delivery
            int arrayCount = DetID.Length;
            List<string> listDetId = new List<string>(DetID);
            List<string> listTxtId = new List<string>(TxtID);
            List<KIVDetailsForm> KIVDetails = new List<KIVDetailsForm>(arrayCount);

            for (int i = 0; i < arrayCount; i++)
            {
                KIVDetailsForm KIVDetailsForm = new KIVDetailsForm();
                KIVDetailsForm.DetID = listDetId[i].ToString();
                KIVDetailsForm.KIVQty = listTxtId[i].ToString();
                KIVDetails.Add(KIVDetailsForm);
            }


            //Get CustNo
            int tempVal = 0;
            int? nullVal = Int32.TryParse(strCustNo, out tempVal) ? tempVal : (int?)null;
            int custNo = nullVal ?? 0;

            //Get Delivery Address
            string strAddress = string.Empty;
            if (kivDelivery.DeliveryAddress == null) strAddress = invoice.Addr1 + " " + invoice.Addr2 + " " + invoice.Addr3;
            else strAddress = kivDelivery.DeliveryAddress;

            //KIVDelivery KIVDelivery = new KIVDelivery();
            //db.KIVDelivery.Add(KIVDelivery);
            //KIVDelivery.CustNo = custNo;
            //KIVDelivery.DeliveryAddress = strAddress.TrimEnd();
            //KIVDelivery.DeliveryDate = kivDelivery.DeliveryDate;
            //KIVDelivery.DeliveryTime = kivDelivery.DeliveryTime;
            //KIVDelivery.CreatedBy = User.Identity.Name;
            //KIVDelivery.CreatedOn = DateTime.Now;

            db.Entry(kivDelivery).State = EntityState.Modified;
            kivDelivery.DeliveryAddress = strAddress;
            db.SaveChanges();

            foreach (var item in KIVDetails)
            {
                int tempValDet = 0;
                double tempValKIVQty = 0;
                int? nullValDet = Int32.TryParse(item.DetID, out tempValDet) ? tempValDet : (int?)null;
                double? nullValKIVQty = Double.TryParse(item.KIVQty, out tempValKIVQty) ? tempValKIVQty : (double?)null;
                int detID = nullValDet ?? 0;
                double kivQty = nullValKIVQty ?? 0;

                if (kivQty > 0)
                {
                    double balQty = 0, delQty = 0, kivBalQty = 0, origBalQty = 0, origKivBalQty = 0, origDelQty = 0, origDelQtyKiv = 0;

                    var _KIVDeliveryDetail = db.KIVDeliveryDetails.Where(m => m.DetID == detID && m.KIVDelID == kivDelivery.KIVDelID).ToList();
                    if (_KIVDeliveryDetail.Count > 0)
                    {
                        KIVDeliveryDetail KIVDeliveryDetail = db.KIVDeliveryDetails.Find(_KIVDeliveryDetail.FirstOrDefault().KIVDelDetailsID);
                        origDelQtyKiv = KIVDeliveryDetail.DeliveryQty;


                        KIVDET KIVDET = db.KIVDETs.Find(detID);
                        origBalQty = KIVDET.BalanceQty;
                        origKivBalQty = KIVDET.KivBalanceQty;
                        balQty = origBalQty - kivQty + origDelQtyKiv;
                        delQty = KIVDET.OrderQty - (KIVDET.KivBalanceQty - kivQty + origDelQtyKiv);
                        kivBalQty = KIVDET.KivBalanceQty - kivQty + origDelQtyKiv;

                        db.Entry(KIVDET).State = EntityState.Modified;
                        KIVDET.DeliverQty = delQty;
                        KIVDET.BalanceQty = balQty;
                        KIVDET.KivBalanceQty = kivBalQty;
                        db.SaveChanges();

                        db.Entry(KIVDeliveryDetail).State = EntityState.Modified;
                        KIVDeliveryDetail.DeliveryQty = kivQty;
                        KIVDeliveryDetail.BalanceQty = origBalQty + origDelQtyKiv;
                        KIVDeliveryDetail.KivBalanceQty = kivBalQty;
                        db.SaveChanges();
                    }
                    //KIVDeliveryDetail KIVDeliveryDetail = new KIVDeliveryDetail();
                    //db.KIVDeliveryDetails.Add(KIVDeliveryDetail);
                    //KIVDeliveryDetail.InvID = KIVDET.InvID;
                    //KIVDeliveryDetail.KIVDelID = KIVDelivery.KIVDelID;
                    //KIVDeliveryDetail.DetID = detID;
                    //KIVDeliveryDetail.DeliveryQty = kivQty;
                    //KIVDeliveryDetail.BalanceQty = origBalQty;
                    //KIVDeliveryDetail.KivBalanceQty = kivBalQty;
                    //db.SaveChanges();

                }

            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult _KIVDETs(int custNo, int kivDelID)
        {
            List<INV> INVs = db.INVs.Where(m => m.CustNo == custNo).ToList();
            List<KIVDET> KIVDETs = db.KIVDETs.ToList();

            var query =
                (from i in INVs
                 join k in KIVDETs on i.InvID equals k.InvID
                 where k.KivBalanceQty > 0
                 select new
                 {
                     DetID = k.DetID,
                     InvID = i.InvID,
                     CustNo = i.CustNo,
                     ItemID = k.ItemID,
                     ItemName = k.ItemName,
                     OrderQty = k.OrderQty,
                     BalanceQty = k.BalanceQty,
                     DeliveryQty = k.DeliverQty,
                     KivBalanceQty = k.KivBalanceQty,
                     InvDate = i.InvDate,
                     CreatedOn = i.CreatedOn
                 }).Distinct();

            IEnumerable<KIVDeliveryAll> queryKIV = from item in query.AsEnumerable()
                                                   select new KIVDeliveryAll(item.DetID, item.InvID, item.CustNo, item.ItemID, item.ItemName, item.OrderQty, item.BalanceQty, item.DeliveryQty, item.KivBalanceQty, item.InvDate, item.CreatedOn);

            KIVDeliveryCreateViewModel KIVDeliveryCreate = new KIVDeliveryCreateViewModel();

             KIVDelivery KIVDelivery = new KIVDelivery();
             if (kivDelID > 0) KIVDelivery = db.KIVDelivery.Find(kivDelID);
           
            INV invoice = new INV();

            KIVDeliveryCreate.Invoice = invoice;
            KIVDeliveryCreate.KIVDelivery = KIVDelivery;
            KIVDeliveryCreate.KIVDETs = queryKIV.ToList();

            return PartialView("_KIVDETs", KIVDeliveryCreate);
        }

        public ActionResult PrintPreview(int id)
        {
            string loginName = User.Identity.Name;
            int custNo = 0;

            Staff staff = db.Staffs.Where(m => m.Email == loginName).FirstOrDefault();
            if (staff != null) ViewBag.PrintedBy = string.Format("{0} {1}", staff.LastName, staff.FirstName);
            //ViewBag.HeaderURL = string.Format("{0}/../../../assets/img/ACHHeader.jpg", Request.Url);

            KIVDeliveryPrintPreview kivDeliveryPrintPreview = new KIVDeliveryPrintPreview();
            KIVDelivery kivDelivery = db.KIVDelivery.Find(id);
            Client client = db.Clients.Find(kivDelivery.CustNo);
            List<KIVDeliveryDetail> kivDeliveryDetail = db.KIVDeliveryDetails.Where(m => m.KIVDelID == id).ToList();

            List<KIVDelivery> KIVDelivery = db.KIVDelivery.Where(m => m.KIVDelID == id).ToList();
            List<KIVDeliveryDetail> KIVDeliveryDetails = db.KIVDeliveryDetails.Where(m => m.KIVDelID == id).ToList();
            List<KIVDET> KIVDETs = db.KIVDETs.ToList();

            var query =
                (from k in KIVDelivery
                 join d in KIVDeliveryDetails on k.KIVDelID equals d.KIVDelID
                 join i in KIVDETs on d.DetID equals i.DetID
                 select new
                 {
                     KIVDelDetailsID = d.KIVDelDetailsID,
                     KIVDelID = d.KIVDelID,
                     DetID =  d.DetID,
                     InvID = d.InvID,
                     ItemID = i.ItemID,
                     ItemName = i.ItemName,
                     TotalDelivery = i.DeliverQty,
                     DeliveryQty = d.DeliveryQty,
                     BalanceQty = d.BalanceQty,
                     OrderQty = i.OrderQty,
                     KivBalanceQty = d.KivBalanceQty
                 }).Distinct();

            IEnumerable<KIVDeliveryDetailForPrint> queryKIV = from item in query.AsEnumerable()
                                                              select new KIVDeliveryDetailForPrint(item.KIVDelDetailsID, item.KIVDelID, item.DetID, item.InvID, item.ItemID, item.ItemName, item.TotalDelivery, item.DeliveryQty, item.BalanceQty, item.OrderQty, item.KivBalanceQty);

            ViewBag.HeaderURL = string.Format("{0}", Request.Url);
            ViewBag.KIVCount = kivDeliveryDetail.Count;

            kivDeliveryPrintPreview.KIVDelivery = kivDelivery;
            kivDeliveryPrintPreview.KIVDeliveryDetail = queryKIV.ToList();
            kivDeliveryPrintPreview.Client = client;

            return new PdfActionResult(kivDeliveryPrintPreview, (writer, document) =>
            {
                Rectangle A4 = new RectangleReadOnly(595, 842);
                document = new Document(A4, 88f, 88f, 0f, 0f);
                document.SetPageSize(A4);
                document.NewPage();
            });
        }

        public JsonResult GetKIVDetails(int custNo)
        {
            List<KIVDET> KIVDETs = db.KIVDETs.ToList();
            List<INV> INVs = db.INVs.ToList();


            var query =
                (from k in KIVDETs
                 join i in INVs on k.InvID equals i.InvID
                 where k.KivBalanceQty > 0
                 select new
                 {
                     InvID = k.InvID,
                     CustNo = i.CustNo,
                     CustName = i.CustName,
                     PoNo = i.PoNo,
                     Amount = i.Amount,
                     TotalKIV = 6,
                     InvDate = i.InvDate
                 }).Distinct();

            //IEnumerable<KIVDeliveryIndex> queryKIV = from item in query.AsEnumerable()
            //                                         select new KIVDeliveryIndex(item.InvID, item.CustNo, item.CustName, item.PoNo, item.Amount, item.TotalKIV, item.InvDate);



            return Json(query, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoComplete(string search)
        {
            var data = db.Clients
                       .Where(x => ((x.CustName.ToUpper().StartsWith(search.ToUpper())) || (x.CustNo.ToString().StartsWith(search))) && ((x.IsActive == true)))
                       .ToList().Distinct().ToList();

            //   var result = data.Where(x => x.HeatNo.ToLower().StartsWith(search.ToLower())).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
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
        public JsonResult GetCreditSettings(int? custNo)
        {
            ClientCreditSetting clientCreditSetting = new ClientCreditSetting();
            clientCreditSetting = db.ClientCreditSetting.Where(m => m.CustNo == custNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();
            return Json(clientCreditSetting, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetKIVDETs(int custNo)
        {
            List<INV> INVs = db.INVs.Where(m => m.CustNo == custNo).ToList();
            List<KIVDET> KIVDETs = db.KIVDETs.ToList();

            var query =
                (from i in INVs
                 join k in KIVDETs on i.InvID equals k.InvID
                 where k.KivBalanceQty > 0
                 select new
                 {
                     DetID = k.DetID,
                     InvID = i.InvID,
                     CustNo = i.CustNo,
                     ItemID = k.ItemID,
                     ItemName = k.ItemName,
                     OrderQty = k.OrderQty,
                     BalanceQty = k.BalanceQty,
                     DeliveryQty = k.DeliverQty,
                     KivBalanceQty = k.KivBalanceQty,
                     InvDate = i.InvDate,
                     CreatedOn = i.CreatedOn
                 }).Distinct();

            IEnumerable<KIVDeliveryAll> queryKIV = from item in query.AsEnumerable()
                                                   select new KIVDeliveryAll(item.DetID, item.InvID, item.CustNo, item.ItemID, item.ItemName, item.OrderQty, item.BalanceQty, item.DeliveryQty, item.KivBalanceQty, item.InvDate, item.CreatedOn);

            return Json(queryKIV, JsonRequestBehavior.AllowGet);
        }
    }
}
