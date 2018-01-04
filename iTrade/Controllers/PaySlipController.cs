using iTextSharp.text;
using iTextSharp.text.pdf;
using iTrade.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace iTrade.Controllers
{
    public class PaySlipController : Controller
    {
        private StarDbContext db = new StarDbContext();
        // GET: PaySlip
        public ActionResult Index(string txtFilter)
        {
            var result = db.PaySlips.Take(200).ToList();

            if (!string.IsNullOrEmpty(txtFilter))
            {
                result = db.PaySlips.Where(x => x.TutorName.Contains(txtFilter) || x.TutorCode.StartsWith(txtFilter)).Take(200).ToList();
            }

            return View(result);
        }

        public ActionResult Create()
        {
            PaySlip stu = new PaySlip();
            ViewData["TutorAll"] = db.Tutors.Where(x => x.IsActive == true).ToList();
            return View(stu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PaySlip det)
        {
            if (ModelState.IsValid)
            {
                db.PaySlips.Add(det);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(det);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PaySlip stu = db.PaySlips.Find(id);
            if (stu == null)
            {
                return HttpNotFound();
            }
            ViewData["TutorAll"] = db.Tutors.Where(x => x.IsActive == true).ToList();
            return View(stu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PaySlip ps)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ps).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex){
                }
            }

            return View(ps);
        }

        public ActionResult Detail(int id)
        {
            var p = new List<PaySlipDetail>();
            p = db.PaySlipDetails.Where(x => x.PaySlipID == id).ToList();

            return PartialView(p);
        }

        public ActionResult Item(int id)
        {
            PaySlip ps = db.PaySlips.Find(id);
            PaySlipDetail p = new PaySlipDetail();
            p.PaySlipID = ps.PaySlipID;
            p.TutorCode = ps.TutorCode;

            ViewData["TutorRateAll"] = db.TutorRates.Where(x => x.TutorCode == ps.TutorCode).ToList();
            return PartialView(p);
        }

        public void AddItem(PaySlipDetail det)
        {
            var pay = db.PaySlipDetails.Where(m => m.PaySlipID == det.PaySlipID).ToList();
            PaySlip paySlip = db.PaySlips.Where(m => m.PaySlipID == det.PaySlipID).FirstOrDefault();
            TutorRate tut = db.TutorRates.Where(m => m.CourseCode == det.ClassCode).FirstOrDefault();

            double payCount = pay.Count;
            det.Position = payCount + 1;
            det.ClassDesc = tut.CourseName;

            paySlip.Total += det.Amount;

            db.PaySlipDetails.Add(det);
            int x = db.SaveChanges();
            int p = det.PaySlipID;

            if (x != 0)
            {
                Response.Redirect("Edit/" + p);
            }

        }

        public JsonResult AutoCompleteSelected(string search)
        {
            int tuID = Convert.ToInt32(search);
            if (search != null)
            {
                var c = db.Tutors
                           .Where(x => x.TutorID == tuID)
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

        public JsonResult AutoSelect(string classCode,string classType,double quantity)
        {
            if (classCode != null && classType !=null && quantity !=0)
            {
                var c = db.TutorRates.Where(x => x.CourseCode == classCode && x.ClassType == classType && x.MinAttend <= quantity && x.MaxAttend >=quantity).FirstOrDefault();

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

        public ActionResult DeleteConfirmed(int id)
        {
            var det = db.PaySlipDetails.Find(id);

            if (det != null)
            {
                db.Entry(det).State = EntityState.Deleted;
                db.SaveChanges();
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PrintPaySlip(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptPaySlip.rdlc";
            localReport.EnableExternalImages = true;

            var payslip = new List<PaySlip>();
            var payslipdet = new List<PaySlipDetail>();

            payslip = db.PaySlips.Where(m => m.PaySlipID == id).ToList();
            payslipdet = db.PaySlipDetails.Where(m => m.PaySlipID == id).ToList();
        
            ReportDataSource rdsPaySlip = new ReportDataSource("DataSet1", payslip);
            ReportDataSource rdsPaySlipDet = new ReportDataSource("DataSet2", payslipdet);

            localReport.DataSources.Add(rdsPaySlip);
            localReport.DataSources.Add(rdsPaySlipDet);

            localReport.Refresh();

            string reportType = "PDF";
            string mimeType;
            string encoding;
            string fileNameExtension = "pdf";

            string deviceInfo = @"<DeviceInfo>
                        <OutputFormat>PDF</OutputFormat>
                       <PageWidth>8.27in</PageWidth>
                        <PageHeight>11.69in</PageHeight>
                        <MarginTop>0.1in</MarginTop>
                        <MarginLeft>0.1in</MarginLeft>
                        <MarginRight>0in</MarginRight>
                        <MarginBottom>0in</MarginBottom>
                    </DeviceInfo>";

            Warning[] warnings;

            string[] streams;

            byte[] renderedBytes;

            renderedBytes = localReport.Render(
                            reportType,
                            deviceInfo,
                            out mimeType,
                            out encoding,
                            out fileNameExtension,
                            out streams,
                            out warnings);

            var doc = new Document();
            var reader = new PdfReader(renderedBytes);

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/docInvoice.pdf"), FileMode.Create))
            {
                PdfStamper stamper = new PdfStamper(reader, fs);
                string Printer = "";
                if (Printer == null || Printer == "")
                {
                    stamper.JavaScript = "var pp = getPrintParams();pp.interactive = pp.constants.interactionLevel.automatic;pp.printerName = getPrintParams().printerName;print(pp);\r";
                    stamper.Close();
                }
                else
                {
                    stamper.JavaScript = "var pp = getPrintParams();pp.interactive = pp.constants.interactionLevel.automatic;pp.printerName = " + Printer + ";print(pp);\r";
                    stamper.Close();
                }
            }

            reader.Close();

            FileStream fss = new FileStream(Server.MapPath("~/Reports/docInvoice.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/docInvoice.pdf"));
            return File(bytes, "application/pdf");
        }

    }
}