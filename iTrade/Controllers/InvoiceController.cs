using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MvcRazorToPdf;
using Microsoft.Reporting.WebForms;
using CLK.AspNet.Identity;
using iTrade.Models;
using InvoiceNo;


namespace iTrade.Controllers
{
    public class InvoiceController : Controller
    {
        private StarDbContext db = new StarDbContext();
        InvoiceClass inv = new InvoiceClass();
        
        // GET: Inventory
        public ActionResult Index()
        {
            DateTime curr = DateTime.Now;
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MinValue;
            string strStartDate = String.Format("{0}/{1}/{2}", "1", curr.Month.ToString(), curr.Year.ToString());
            string strEndDate = String.Format("{0}/{1}/{2}", DateTime.DaysInMonth(curr.Year, curr.Month), curr.Month.ToString(), curr.Year.ToString());
            startDate = DateTime.Parse(strStartDate);
            endDate = DateTime.Parse(strEndDate);

            ViewBag.StartDate = strStartDate;
            ViewBag.EndDate = strEndDate;
            ViewBag.CompanyName = "All Customers";

            IList<INV> invoices = db.INVs.OrderByDescending(m => m.CreatedOn).ToList();
            IList<Client> clientList = db.Clients.ToList();
            InvoiceIndexViewModel invoiceIndexViewModel = new InvoiceIndexViewModel(invoices, db.Clients);

            return View(invoiceIndexViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FormCollection collection)
        {
            string invNo = inv.GetInvoiceNumber(InvType.CR.ToString(), DateTime.Now, "User");
            int InvoiceId = 0, tempVal = 0;
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MinValue;
            string strStartDate = string.Format("{0} {1}", collection.GetValue("startDate").AttemptedValue, "00:00:00");
            string strEndDate = string.Format("{0} {1}", collection.GetValue("endDate").AttemptedValue, "23:59:59");
            string strCustNo = collection.GetValue("CustNo").AttemptedValue;
            string strCustName = "All Customers";
            
            startDate = DateTime.Parse(strStartDate);
            endDate = DateTime.Parse(strEndDate);

            int? nullVal = Int32.TryParse(strCustNo, out tempVal) ? tempVal : (int?)null;
            int CustNo = nullVal ?? 0;

            Client client = db.Clients.Find(CustNo);

            if (client != null)
            {
                strCustName = client.CustName;
            }

            ViewBag.CompanyName = strCustName;
            ViewBag.CustNo = strCustNo;
            ViewBag.StartDate = collection.GetValue("startDate").AttemptedValue;
            ViewBag.EndDate = collection.GetValue("endDate").AttemptedValue;

            IList<INV> invoices = new List<INV>();

            if (CustNo > 0) db.INVs.Where(m => m.CreatedOn >= startDate && m.CreatedOn <= endDate).OrderByDescending(m => m.CreatedOn).ToList();
            else if (CustNo > 0) db.INVs.Where(m => m.CreatedOn >= startDate && m.CreatedOn <= endDate).OrderByDescending(m => m.CreatedOn).ToList();

            IList<Client> clientList = db.Clients.ToList();
            InvoiceIndexViewModel invoiceIndexViewModel = new InvoiceIndexViewModel(invoices, db.Clients);

            return View(invoiceIndexViewModel);
        }

        public ActionResult AmascoInvoice(int id)
        {
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptAmascoInvoice.rdlc";

            int custNo = 0;
            var client = new List<Client>();
            var inv = new List<INV>();
            var invDets = new List<INVDET>();
            var kivDets = new List<KIVDET>();

            inv = db.INVs.Where(m => m.InvID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.InvID == id).ToList();
            kivDets = db.KIVDETs.Where(m => m.InvID == id).ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet4", kivDets);
            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);
            localReport.Refresh();

            string reportType = "PDF";
            string mimeType;
            string encoding;
            string fileNameExtension = "pdf";

            string deviceInfo = @"<DeviceInfo>
                        <OutputFormat>PDF</OutputFormat>
                       <PageWidth>8.27in</PageWidth>
                        <PageHeight>11.69in</PageHeight>
                        <MarginTop>0.3in</MarginTop>
                        <MarginLeft>0.8in</MarginLeft>
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/AmascoInvoice.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/AmascoInvoice.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/AmascoInvoice.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult PrintPreview(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptInvoice.rdlc";
            localReport.EnableExternalImages = true;

            int custNo = 0;
            var client = new List<Client>();
            var inv = new List<INV>();
            var invDets = new List<INVDET>();
            var kivDets = new List<KIVDET>();

            inv = db.INVs.Where(m => m.InvID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.InvID == id && m.CnID == 0).ToList();
            kivDets = db.KIVDETs.Where(m => m.InvID == id && m.KorID == 0 && m.SalesType != "Bundle").ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet4", kivDets);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

        public ActionResult PrintPreviewCopy(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptInvoiceCopy.rdlc";

            int custNo = 0;
            var client = new List<Client>();
            var inv = new List<INV>();
            var invDets = new List<INVDET>();
            var kivDets = new List<KIVDET>();

            inv = db.INVs.Where(m => m.InvID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.InvID == id && m.CnID == 0).ToList();
            kivDets = db.KIVDETs.Where(m => m.InvID == id && m.KorID == 0 && m.SalesType != "Bundle").ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet4", kivDets);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/docInvoiceCopy.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/docInvoiceCopy.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/docInvoiceCopy.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult PrintInvoiceAndDO(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptInvoice.rdlc";

            int custNo = 0;
            var client = new List<Client>();
            var inv = new List<INV>();
            var invDets = new List<INVDET>();
            var kivDets = new List<KIVDET>();

            inv = db.INVs.Where(m => m.InvID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.InvID == id && m.CnID == 0).ToList();
            kivDets = db.KIVDETs.Where(m => m.InvID == id && m.KorID == 0).ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet4", kivDets);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/InvoiceOrg.pdf"), FileMode.Create))
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

            //  generate invoice SALES copy pdf file

            localReport.Dispose();
            localReport.ReportPath = @"ReportTemplate/rptInvoiceCopy1.rdlc";

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
            localReport.Refresh();

            renderedBytes = localReport.Render(
                            reportType,
                            deviceInfo,
                            out mimeType,
                            out encoding,
                            out fileNameExtension,
                            out streams,
                            out warnings);

            var doc2 = new Document();
            var reader2 = new PdfReader(renderedBytes);

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/InvoiceCopy1.pdf"), FileMode.Create))
            {
                PdfStamper stamper = new PdfStamper(reader2, fs);
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

            reader2.Close();


            //  generate invoice ACCOUNT copy pdf file

            localReport.Dispose();
            localReport.ReportPath = @"ReportTemplate/rptInvoiceCopy2.rdlc";

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
            localReport.Refresh();

            renderedBytes = localReport.Render(
                            reportType,
                            deviceInfo,
                            out mimeType,
                            out encoding,
                            out fileNameExtension,
                            out streams,
                            out warnings);

            var doc3 = new Document();
            var reader3 = new PdfReader(renderedBytes);

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/InvoiceCopy2.pdf"), FileMode.Create))
            {
                PdfStamper stamper = new PdfStamper(reader3, fs);
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

            reader3.Close();


            //  generate DO pdf file

            localReport.Dispose();
            localReport.ReportPath = @"ReportTemplate/rptDeliveryOrder.rdlc";

            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.InvID == id && m.CnID == 0 && m.IsBundle == false).ToList();
            kivDets = db.KIVDETs.Where(m => m.InvID == id && m.KorID == 0 && m.SalesType != "Bundle").ToList();

            rdsClient = new ReportDataSource("DataSet1", client);
            rdsInv = new ReportDataSource("DataSet2", inv);
            rdsInvDets = new ReportDataSource("DataSet3", invDets);
            rdsKivDets = new ReportDataSource("DataSet4", kivDets);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
            localReport.Refresh();

            renderedBytes = localReport.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            var doc4 = new Document();
            var reader4 = new PdfReader(renderedBytes);

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/DeliveryOrder.pdf"), FileMode.Create))
            {
                PdfStamper stamper = new PdfStamper(reader4, fs);
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

            reader4.Close();


            // Merge 2 pdf files
         //   MergePDFs(Server.MapPath("~/Reports/InvoiceData.pdf"), Server.MapPath("~/Reports/InvoiceOrg.pdf"), Server.MapPath("~/Reports/InvoiceCopy.pdf"));
            MergePDFs(Server.MapPath("~/Reports/Summary.pdf"), Server.MapPath("~/Reports/InvoiceOrg.pdf"), Server.MapPath("~/Reports/InvoiceCopy1.pdf"), Server.MapPath("~/Reports/InvoiceCopy2.pdf"), Server.MapPath("~/Reports/DeliveryOrder.pdf"));

            // Display result

            FileStream fss = new FileStream(Server.MapPath("~/Reports/Summary.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();

            System.IO.File.Delete(Server.MapPath("~/Reports/Summary.pdf"));
            return File(bytes, "application/pdf");
        }


        private void MergePDFs(string outPutFilePath, params string[] filesPath)
        {
            List<PdfReader> readerList = new List<PdfReader>();
            foreach (string filePath in filesPath)
            {
                PdfReader pdfReader = new PdfReader(filePath);
                readerList.Add(pdfReader);
            }

            //Define a new output document and its size, type
            Document document = new Document(PageSize.A4, 0, 0, 0, 0);
            //Create blank output pdf file and get the stream to write on it.
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outPutFilePath, FileMode.Create));
            document.Open();

            foreach (PdfReader reader in readerList)
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    PdfImportedPage page = writer.GetImportedPage(reader, i);
                    document.Add(iTextSharp.text.Image.GetInstance(page));
                }
            }
            document.Close();

        }

        public ActionResult DeliveryOrderPrint(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptDeliveryOrder.rdlc";

            int custNo = 0;
            var client = new List<Client>();
            var inv = new List<INV>();
            var invDets = new List<INVDET>();
            var kivDets = new List<KIVDET>();

            inv = db.INVs.Where(m => m.InvID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.InvID == id && m.CnID == 0 && m.IsBundle == false).ToList();
            kivDets = db.KIVDETs.Where(m => m.InvID == id && m.KorID == 0 && m.SalesType != "Bundle").ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet4", kivDets);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/DeliveryOrder.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/DeliveryOrder.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];            
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/DeliveryOrder.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult SOPrintPreview(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();
            localReport.ReportPath = @"ReportTemplate/rptSalesOrder.rdlc";

            localReport.EnableExternalImages = true;

            int custNo = 0;
            var client = new List<Client>();
            var inv = new List<SalesOrder>();
            var invDets = new List<INVDET>();
            var kivDets = new List<KIVDET>();

            inv = db.SalesOrders.Where(m => m.SorID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.SorID == id && m.CnID == 0).ToList();
            kivDets = db.KIVDETs.Where(m => m.SorID == id && m.KorID == 0 && m.SalesType != "Bundle").ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet4", kivDets);

            ReportParameter pUserName = new ReportParameter("UserName", userName);
            
            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/Summary.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/Summary.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/Summary.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult PrintCreditNote(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptCreditNote01.rdlc";

            int custNo = 0;
            var creditNote = new List<CreditNote>();
            var creditNoteDets = new List<CreditNoteDet>();
            var invDets = new List<INVDET>();
            var client = new List<Client>();

            creditNote = db.CreditNotes.Where(m => m.CnID == id).ToList();
            if (creditNote.Count > 0)
            {
                custNo = creditNote.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.CnID == id).ToList();
            creditNoteDets = db.CreditNoteDets.Where(m => m.CnID == id).ToList();

            ReportDataSource rdsCreditNote = new ReportDataSource("DataSet1", creditNote);
            ReportDataSource rdsCreditNoteDets = new ReportDataSource("DataSet2", creditNoteDets);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsClient = new ReportDataSource("DataSet4", client);

            string docType="Credit Note1";
            ReportParameter pUserName = new ReportParameter("UserName", userName);
            ReportParameter pDocType = new ReportParameter("docType", docType);

            localReport.DataSources.Add(rdsCreditNote);
            localReport.DataSources.Add(rdsCreditNoteDets);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsClient);

            localReport.SetParameters(new ReportParameter[] { pUserName, pDocType });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/CreditNote.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/CreditNote.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/CreditNote.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult PrintKIVDelivery(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptKIVDelivery.rdlc";

            int custNo = 0;
            var kivOrder = new List<KivOrder>();
            var kivOrderDets = new List<KivOrderDet>();
            var kivOrderDetsIN = new List<KivOrderDet>();
            var kivOrderDetsOUT = new List<KivOrderDet>();
            var kivDets = new List<KIVDET>();
            var client = new List<Client>();

            kivOrder = db.KivOrders.Where(m => m.KorID == id).ToList();
            if (kivOrder.Count > 0)
            {
                custNo = kivOrder.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            kivOrderDets = db.KivOrderDets.Where(m => m.KorID == id).ToList();
            kivOrderDetsIN = kivOrderDets.Where(m => m.DeliverOrderQty != 0).ToList();
            bool flag = kivOrderDets.Any(m => m.DeliverOrderQty == 0);
            if (flag)
            {
                kivOrderDetsOUT = kivOrderDets.Where(m => m.DeliverQty > 0).ToList();
                //kivOrderDetsOUT = kivOrderDets.Where(m => (m.DeliverOrderQty == 0) || (m.ChangedQty < 0 && m.DeliverQty > 0)).ToList();
            }

            kivDets = db.KIVDETs.Where(m => m.KorID == id).ToList();

            ReportDataSource rdsKivOrder = new ReportDataSource("DataSet1", kivOrder);
            ReportDataSource rdsKivOrderDetsIn = new ReportDataSource("DataSet2", kivOrderDetsIN);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet3", kivDets);
            ReportDataSource rdsClient = new ReportDataSource("DataSet4", client);
            ReportDataSource rdsKivOrderDetsOut = new ReportDataSource("DataSet5", kivOrderDetsOUT);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsKivOrder);
            localReport.DataSources.Add(rdsKivOrderDetsIn);
            localReport.DataSources.Add(rdsKivDets);
            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsKivOrderDetsOut);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/KIVDelivery.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/KIVDelivery.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/KIVDelivery.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult PrintKIVDeliveryCopy(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptKIVDeliveryCopy.rdlc";

            int custNo = 0;
            var kivOrder = new List<KivOrder>();
            var kivOrderDets = new List<KivOrderDet>();
            var kivOrderDetsIN = new List<KivOrderDet>();
            var kivOrderDetsOUT = new List<KivOrderDet>();
            var kivDets = new List<KIVDET>();
            var client = new List<Client>();

            kivOrder = db.KivOrders.Where(m => m.KorID == id).ToList();
            if (kivOrder.Count > 0)
            {
                custNo = kivOrder.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            kivOrderDets = db.KivOrderDets.Where(m => m.KorID == id).ToList();
            kivOrderDetsIN = kivOrderDets.Where(m => m.DeliverOrderQty != 0).ToList();
            bool flag = kivOrderDets.Any(m => m.DeliverOrderQty == 0);
            if (flag)
            {
                kivOrderDetsOUT = kivOrderDets.Where(m => m.DeliverQty > 0).ToList();
                //kivOrderDetsOUT = kivOrderDets.Where(m => (m.DeliverOrderQty == 0) || (m.ChangedQty < 0 && m.DeliverQty > 0)).ToList();
            }

            kivDets = db.KIVDETs.Where(m => m.KorID == id).ToList();

            ReportDataSource rdsKivOrder = new ReportDataSource("DataSet1", kivOrder);
            ReportDataSource rdsKivOrderDetsIn = new ReportDataSource("DataSet2", kivOrderDetsIN);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet3", kivDets);
            ReportDataSource rdsClient = new ReportDataSource("DataSet4", client);
            ReportDataSource rdsKivOrderDetsOut = new ReportDataSource("DataSet5", kivOrderDetsOUT);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsKivOrder);
            localReport.DataSources.Add(rdsKivOrderDetsIn);
            localReport.DataSources.Add(rdsKivDets);
            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsKivOrderDetsOut);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/KIVDeliveryCopy.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/KIVDeliveryCopy.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/KIVDeliveryCopy.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult PrintKIVDelivery3in1(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptKIVDelivery.rdlc";

            int custNo = 0;
            var kivOrder = new List<KivOrder>();
            var kivOrderDets = new List<KivOrderDet>();
            var kivOrderDetsIN = new List<KivOrderDet>();
            var kivOrderDetsOUT = new List<KivOrderDet>();
            var kivDets = new List<KIVDET>();
            var client = new List<Client>();

            kivOrder = db.KivOrders.Where(m => m.KorID == id).ToList();
            if (kivOrder.Count > 0)
            {
                custNo = kivOrder.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            kivOrderDets = db.KivOrderDets.Where(m => m.KorID == id).ToList();
            kivOrderDetsIN = kivOrderDets.Where(m => m.DeliverOrderQty != 0).ToList();
            bool flag = kivOrderDets.Any(m => m.DeliverOrderQty == 0);
            if (flag)
            {
                kivOrderDetsOUT = kivOrderDets.Where(m => m.DeliverQty > 0).ToList();
                //kivOrderDetsOUT = kivOrderDets.Where(m => (m.DeliverOrderQty == 0) || (m.ChangedQty < 0 && m.DeliverQty > 0)).ToList();
            }

            kivDets = db.KIVDETs.Where(m => m.KorID == id).ToList();

            ReportDataSource rdsKivOrder = new ReportDataSource("DataSet1", kivOrder);
            ReportDataSource rdsKivOrderDetsIn = new ReportDataSource("DataSet2", kivOrderDetsIN);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet3", kivDets);
            ReportDataSource rdsClient = new ReportDataSource("DataSet4", client);
            ReportDataSource rdsKivOrderDetsOut = new ReportDataSource("DataSet5", kivOrderDetsOUT);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsKivOrder);
            localReport.DataSources.Add(rdsKivOrderDetsIn);
            localReport.DataSources.Add(rdsKivDets);
            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsKivOrderDetsOut);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/KIVDelivery.pdf"), FileMode.Create))
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

            // generate sales copy

            localReport.Dispose();
            localReport.ReportPath = @"ReportTemplate/rptKIVDeliveryCopy1.rdlc";

            localReport.DataSources.Add(rdsKivOrder);
            localReport.DataSources.Add(rdsKivOrderDetsIn);
            localReport.DataSources.Add(rdsKivDets);
            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsKivOrderDetsOut);

            localReport.SetParameters(new ReportParameter[] { pUserName });
            localReport.Refresh();
 
            renderedBytes = localReport.Render(
                            reportType,
                            deviceInfo,
                            out mimeType,
                            out encoding,
                            out fileNameExtension,
                            out streams,
                            out warnings);

            var doc1 = new Document();
            var reader1 = new PdfReader(renderedBytes);

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/KivCopy1.pdf"), FileMode.Create))
            {
                PdfStamper stamper = new PdfStamper(reader1, fs);
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

            reader1.Close();

            localReport.Dispose();
            localReport.ReportPath = @"ReportTemplate/rptKIVDeliveryCopy2.rdlc";

            localReport.DataSources.Add(rdsKivOrder);
            localReport.DataSources.Add(rdsKivOrderDetsIn);
            localReport.DataSources.Add(rdsKivDets);
            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsKivOrderDetsOut);

            localReport.SetParameters(new ReportParameter[] { pUserName });
            localReport.Refresh();

            renderedBytes = localReport.Render(
                            reportType,
                            deviceInfo,
                            out mimeType,
                            out encoding,
                            out fileNameExtension,
                            out streams,
                            out warnings);

            var doc2 = new Document();
            var reader2 = new PdfReader(renderedBytes);

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/KivCopy2.pdf"), FileMode.Create))
            {
                PdfStamper stamper = new PdfStamper(reader2, fs);
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

            reader2.Close();


            // Merge 3 pdf files
            MergePDFs(Server.MapPath("~/Reports/KivSummary.pdf"), Server.MapPath("~/Reports/KIVDelivery.pdf"), Server.MapPath("~/Reports/KivCopy1.pdf"), Server.MapPath("~/Reports/KivCopy2.pdf"));

            // Display result

            FileStream fss = new FileStream(Server.MapPath("~/Reports/KivSummary.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();

            System.IO.File.Delete(Server.MapPath("~/Reports/KivSummary.pdf"));
            return File(bytes, "application/pdf");


            //FileStream fss = new FileStream(Server.MapPath("~/Reports/KIVDelivery.pdf"), FileMode.Open);
            //byte[] bytes = new byte[fss.Length];
            //fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            //fss.Close();
            //System.IO.File.Delete(Server.MapPath("~/Reports/KIVDelivery.pdf"));
            //return File(bytes, "application/pdf");
        }

        public ActionResult CSPrintPreview(int id)
        {
            //PdfWriter writer1 = new PdfWriter();
            //Document document1 = new Document();

            string loginName = User.Identity.Name;
            int custNo = 0;

            Staff staff = db.Staffs.Where(m => m.Email == loginName).FirstOrDefault();
            if (staff != null) ViewBag.PrintedBy = string.Format("{0} {1}", staff.LastName, staff.FirstName);
            //ViewBag.HeaderURL = string.Format("{0}/../../../assets/img/ACHHeader.jpg", Request.Url);

            InvoiceViewModel invoiceViewModel = new InvoiceViewModel();
            INV inv = db.INVs.Find(id);
            Client client = new Client();
            List<INVDET> iNVDETs = db.INVDETs.Where(m => m.InvID == id).ToList();
            List<KIVDET> KIVDETs = db.KIVDETs.Where(m => m.InvID == id).ToList();
            if (inv != null)
            {
                custNo = inv.CustNo;
                client = db.Clients.Find(custNo);
            }

            ViewBag.HeaderURL = string.Format("{0}", Request.Url);
            ViewBag.INVCount = iNVDETs.Count;
            ViewBag.KIVCount = KIVDETs.Count;
            ViewBag.PageCount = iNVDETs.Count % 15;

            invoiceViewModel.Invoice = inv;
            invoiceViewModel.Client = client;
            invoiceViewModel.INVDETs = iNVDETs;
            invoiceViewModel.KIVDETs = KIVDETs;

            return new PdfActionResult(invoiceViewModel, (writer, document) =>
            {
                Rectangle A4 = new RectangleReadOnly(595, 842);
                document = new Document(A4, 88f, 88f, 0, 0);
                document.SetPageSize(A4);
                document.NewPage();
            });
        }

        public ActionResult DOPrintPreview(int id, int InvID)
        {
            int custNo = 0;

            var so = new List<SalesOrder>();
            var client = new List<Client>();
            var inv = new List<INV>();
            var invDets = new List<INVDET>();

            so = db.SalesOrders.Where(m => m.SorID == id).ToList();
            inv = db.INVs.Where(m => m.InvID == InvID).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.InvID == id).ToList();

            //string urls = Request.Url.ToString();
            //string[] url = urls.Split('?');
            //string finalUrl = url.ElementAt(0).ToString();

            //ViewBag.HeaderURL = string.Format("{0}", finalUrl);


            //deliveryViewModel.SalesOrder = salesOrder;
            //deliveryViewModel.Invoice = inv;
            //deliveryViewModel.Client = client;
            //deliveryViewModel.INVDETs = iNVDETs;
            //deliveryViewModel.KIVDETs = KIVDETs;

            //return new PdfActionResult(deliveryViewModel, (writer, document) =>
            //{
            //    Rectangle A4 = new RectangleReadOnly(595, 842);
            //    document = new Document(A4, 88f, 88f, 0, 0);
            //    document.SetPageSize(A4);
            //    document.NewPage();
            //});

            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptDO.rdlc";

            ReportDataSource rdsSO = new ReportDataSource("DataSet1", so);
            ReportDataSource rdsClient = new ReportDataSource("DataSet2", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet3", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet4", invDets);
            localReport.DataSources.Add(rdsSO);
            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/DO.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/DO.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/DO.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult SAPrintPreview(int id, int year, int month)
        {
            //PdfWriter writer1 = new PdfWriter();
            //Document document1 = new Document();

            string loginName = User.Identity.Name;
            string paymentTerm = string.Empty;
            int custNo = 0;

            Staff staff = db.Staffs.Where(m => m.Email == loginName).FirstOrDefault();
            if (staff != null) ViewBag.PrintedBy = string.Format("{0} {1}", staff.LastName, staff.FirstName);
            //ViewBag.HeaderURL = string.Format("{0}/../../../assets/img/ACHHeader.jpg", Request.Url);

            StatementAccountViewModel statementAccountViewModel = new StatementAccountViewModel();
            List<SalesPaymentMethod> payment = db.SalesPaymentMethods.ToList();
            List<INV> inv = db.INVs.Where(m => m.CustNo == id).OrderBy(m=>m.InvID).ToList();
            Client client = db.Clients.Find(id);
            List<ClientCreditSetting> clientCredit = db.ClientCreditSetting.Where(m => m.CustNo == id).OrderByDescending(m => m.CreatedOn).ToList();

            if (clientCredit.Count>0)
            {
                paymentTerm = clientCredit.FirstOrDefault().PaymentTerms.ToString();
            }

            var query =
                (from i in inv
                 join p in payment on i.InvID equals p.InvID
                 where p.CreatedOn.Year == year && p.CreatedOn.Month == month
                 select new
                 {
                     CustNo = i.CustNo,
                     InvID = i.InvID,
                     SalesPaymentMethodID = p.SalesPaymentMethodID,
                     PaymentMethod = p.PaymentMethod,
                     Amount = p.Amount,
                     CreatedOn = p.CreatedOn,
                     InvDate=i.CreatedOn
                 }).Distinct();


            IEnumerable<StatementAccount> statementAccount = from item in query.AsEnumerable()
                                                             select new StatementAccount(item.CustNo, item.InvID, item.SalesPaymentMethodID, item.PaymentMethod, item.Amount, item.CreatedOn, item.InvDate);

            string urls = Request.Url.ToString();
            string[] url = urls.Split('?');
            string finalUrl = url.ElementAt(0).ToString();
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            ViewBag.HeaderURL = string.Format("{0}", finalUrl);
            ViewBag.QueryMonth = string.Format("{0} {1}", monthName, year.ToString());
            ViewBag.PaymentTerm = paymentTerm;

            statementAccountViewModel.Client = client;
            statementAccountViewModel.StatementAccount = statementAccount.ToList();

            return new PdfActionResult(statementAccountViewModel, (writer, document) =>
            {
                Rectangle A4 = new RectangleReadOnly(595, 842);
                document = new Document(A4, 88f, 88f, 0, 0);
                document.SetPageSize(A4);
                document.NewPage();
            });
        }

        public ActionResult KivPrintPreview(int id)
        {
            //PdfWriter writer1 = new PdfWriter();
            //Document document1 = new Document();

            string loginName = User.Identity.Name;
            int custNo = 0;

            Staff staff = db.Staffs.Where(m => m.Email == loginName).FirstOrDefault();
            if (staff != null) ViewBag.PrintedBy = string.Format("{0} {1}", staff.LastName, staff.FirstName);
            //ViewBag.HeaderURL = string.Format("{0}/../../../assets/img/ACHHeader.jpg", Request.Url);

            KivInvoiceViewModel invoiceViewModel = new KivInvoiceViewModel();
            KivOrder inv = db.KivOrders.Find(id);
            Client client = new Client();
            List<KivOrderDet> iNVDETs = db.KivOrderDets.Where(m => m.KorID == id).ToList();
            if (inv != null)
            {
                custNo = inv.CustNo;
                client = db.Clients.Find(custNo);
            }

            ViewBag.HeaderURL = string.Format("{0}", Request.Url);
            ViewBag.INVCount = iNVDETs.Count;
            ViewBag.PageCount = iNVDETs.Count % 15;

            invoiceViewModel.Invoice = inv;
            invoiceViewModel.Client = client;
            invoiceViewModel.INVDETs = iNVDETs;

            return new PdfActionResult(invoiceViewModel, (writer, document) =>
            {
                Rectangle A4 = new RectangleReadOnly(595, 842);
                document = new Document(A4, 88f, 88f, 0, 0);
                document.SetPageSize(A4);
                document.NewPage();
            });
        }

        public ActionResult DeliveryOrder(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptDeliverOrder.rdlc";

            int custNo = 0;
            var client = new List<Client>();
            var inv = new List<INV>();
            var invDets = new List<INVDET>();
            var kivDets = new List<KIVDET>();

            inv = db.INVs.Where(m => m.InvID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.InvID == id).ToList();
            kivDets = db.KIVDETs.Where(m => m.InvID == id).ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet4", kivDets);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/Delivery.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/Delivery.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/Delivery.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult PickingListPrint(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptPickingList.rdlc";

            int custNo = 0;
            int invid = -1;
            var client = new List<Client>();
            var inv = new List<SalesOrder>();
            var invDets = new List<INVDET>();
            var kivDets = new List<KIVDET>();
            var sor = new List<SalesOrder>();

            inv = db.SalesOrders.Where(m => m.SorID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
                invid = inv.FirstOrDefault().SorID;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.SorID == id && m.IsBundle == false).ToList();
            kivDets = db.KIVDETs.Where(m => m.SorID == id && m.SalesType != "Bundle").ToList();

            sor = db.SalesOrders.Where(x => x.SorID == id).ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet4", kivDets);
            ReportDataSource rdsSor = new ReportDataSource("DataSet5", sor);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);
            localReport.DataSources.Add(rdsSor);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/PickingList.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/PickingList.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/PickingList.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult ExchangePrintPreview(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptExchange.rdlc";

            int custNo = 0;
            var client = new List<Client>();
            var inv = new List<ExchangeOrder>();
            var invDets = new List<INVDET>();
            var invDetsIN = new List<INVDET>();
            var invDetsOUT = new List<INVDET>();

            inv = db.ExchangeOrders.Where(m => m.EorID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.EorID == id && m.CnID == 0).ToList();
            invDetsIN = db.INVDETs.Where(m => m.EorID == id && m.CnID == 0 && m.SellType == "RT").ToList();
            invDetsOUT = db.INVDETs.Where(m => m.EorID == id && m.CnID == 0 && m.SellType == "CS").ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsInvDetsIN = new ReportDataSource("DataSet4", invDetsIN);
            ReportDataSource rdsInvDetsOUT = new ReportDataSource("DataSet5", invDetsOUT);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsInvDetsIN);
            localReport.DataSources.Add(rdsInvDetsOUT);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/ExchangeOrder.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/ExchangeOrder.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/ExchangeOrder.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult QuotationPrintPreview(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptQuotation.rdlc";

            localReport.EnableExternalImages = true;

            int custNo = 0;
            var client = new List<Client>();
            var inv = new List<Quotation>();
            var invDets = new List<INVDET>();
            string quoNo = string.Empty;


            inv = db.Quotations.Where(m => m.QuoID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
                quoNo = inv.FirstOrDefault().QuoNo;
            }
            client = db.Clients.Where(m => m.CustNo == custNo).ToList();
            invDets = db.INVDETs.Where(m => m.QuoNo == quoNo && m.CnID == 0).ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/QuotationOrder.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/QuotationOrder.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/QuotationOrder.pdf"));
            return File(bytes, "application/pdf");
        }

        // Purchase Order and Invoice print

        public ActionResult POPrintPreview(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptPurchaseOrder.rdlc";

            int custNo = 0;
            var client = new List<Vendor>();
            var inv = new List<PurchaseOrder>();
            var invDets = new List<PoINVDET>();
            var kivDets = new List<PoKIVDET>();

            inv = db.PurchaseOrders.Where(m => m.SorID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
            }
            client = db.Vendors.Where(m => m.CustNo == custNo).ToList();
            invDets = db.PoINVDETs.Where(m => m.SorID == id && m.CnID == 0).OrderBy(m => m.Position).ToList();
            kivDets = db.PoKIVDETs.Where(m => m.SorID == id && m.KorID == 0 && m.SalesType != "Bundle").OrderBy(m => m.Position).ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet4", kivDets);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/POform.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/POform.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/POform.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult PrintPI(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptPurchaseInvoice.rdlc";

            int custNo = 0;
            var client = new List<Vendor>();
            var inv = new List<PoINV>();
            var invDets = new List<PoINVDET>();
            var kivDets = new List<PoKIVDET>();

            inv = db.PoINVs.Where(m => m.InvID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
            }
            client = db.Vendors.Where(m => m.CustNo == custNo).ToList();
            invDets = db.PoINVDETs.Where(m => m.InvID == id && m.CnID == 0).OrderBy(m => m.Position).ToList();
            kivDets = db.PoKIVDETs.Where(m => m.InvID == id && m.KorID == 0 && m.SalesType != "Bundle").OrderBy(m => m.Position).ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet4", kivDets);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/docPurchaseInvoice.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/docPurchaseInvoice.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/docPurchaseInvoice.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult PrintArrivalNote(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptArrivalNote.rdlc";

            int custNo = 0;
            int invid = -1;
            var client = new List<Vendor>();
            var inv = new List<PoINV>();
            var invDets = new List<PoINVDET>();
            var kivDets = new List<PoKIVDET>();
            var sor = new List<PurchaseOrder>();

            inv = db.PoINVs.Where(m => m.SorID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
                invid = inv.FirstOrDefault().SorID;
            }
            client = db.Vendors.Where(m => m.CustNo == custNo).ToList();
            invDets = db.PoINVDETs.Where(m => m.SorID == id && m.IsBundle == false).OrderBy(m => m.Position).ToList();
            kivDets = db.PoKIVDETs.Where(m => m.SorID == id && m.SalesType != "Bundle").OrderBy(m => m.Position).ToList();

            sor = db.PurchaseOrders.Where(x => x.SorID == id).ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet4", kivDets);
            ReportDataSource rdsSor = new ReportDataSource("DataSet5", sor);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);
            localReport.DataSources.Add(rdsSor);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/ArrivalNote.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/ArrivalNote.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/ArrivalNote.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult PrintPIAndArrivalNote(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptPurchaseInvoice.rdlc";

            int custNo = 0;
            var client = new List<Vendor>();
            var inv = new List<PoINV>();
            var invDets = new List<PoINVDET>();
            var kivDets = new List<PoKIVDET>();

            inv = db.PoINVs.Where(m => m.InvID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
            }
            client = db.Vendors.Where(m => m.CustNo == custNo).ToList();
            invDets = db.PoINVDETs.Where(m => m.InvID == id && m.CnID == 0).OrderBy(m => m.Position).ToList();
            kivDets = db.PoKIVDETs.Where(m => m.InvID == id && m.KorID == 0 && m.SalesType != "Bundle").OrderBy(m => m.Position).ToList();

            ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
            ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
            ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet4", kivDets);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/InvoiceOrg.pdf"), FileMode.Create))
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




            //  generate DO pdf file

            localReport.Dispose();
            localReport.ReportPath = @"ReportTemplate/rptArrivalNote.rdlc";

            custNo = 0;
            var invid = -1;
            client = new List<Vendor>();
            inv = new List<PoINV>();
            invDets = new List<PoINVDET>();
            kivDets = new List<PoKIVDET>();
            var sor = new List<PurchaseOrder>();

            inv = db.PoINVs.Where(m => m.SorID == id).ToList();
            if (inv.Count > 0)
            {
                custNo = inv.FirstOrDefault().CustNo;
                invid = inv.FirstOrDefault().SorID;
            }
            client = db.Vendors.Where(m => m.CustNo == custNo).ToList();
            invDets = db.PoINVDETs.Where(m => m.SorID == id && m.IsBundle == false).OrderBy(m => m.Position).ToList();
            kivDets = db.PoKIVDETs.Where(m => m.SorID == id && m.SalesType != "Bundle").OrderBy(m => m.Position).ToList();

            sor = db.PurchaseOrders.Where(x => x.SorID == id).ToList();

            rdsClient = new ReportDataSource("DataSet1", client);
            rdsInv = new ReportDataSource("DataSet2", inv);
            rdsInvDets = new ReportDataSource("DataSet3", invDets);
            rdsKivDets = new ReportDataSource("DataSet4", kivDets);
            var rdsSor = new ReportDataSource("DataSet5", sor);

            pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsInv);
            localReport.DataSources.Add(rdsInvDets);
            localReport.DataSources.Add(rdsKivDets);
            localReport.DataSources.Add(rdsSor);

            localReport.SetParameters(new ReportParameter[] { pUserName });
            localReport.Refresh();

            renderedBytes = localReport.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            var doc4 = new Document();
            var reader4 = new PdfReader(renderedBytes);

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/ArrivalNote.pdf"), FileMode.Create))
            {
                PdfStamper stamper = new PdfStamper(reader4, fs);
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

            reader4.Close();


            // Merge 2 pdf files 
            MergePDFs(Server.MapPath("~/Reports/PISummary.pdf"), Server.MapPath("~/Reports/InvoiceOrg.pdf"), Server.MapPath("~/Reports/ArrivalNote.pdf"));

            // Display result

            FileStream fss = new FileStream(Server.MapPath("~/Reports/PISummary.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();

            System.IO.File.Delete(Server.MapPath("~/Reports/PISummary.pdf"));
            return File(bytes, "application/pdf");
        }

        public ActionResult PrintGRN(int id)
        {
            string userName = User.Identity.Name;
            LocalReport localReport = new LocalReport();

            localReport.ReportPath = @"ReportTemplate/rptGRN.rdlc";

            int custNo = 0;
            var kivOrder = new List<PoKivOrder>();
            var kivOrderDets = new List<PoKivOrderDet>();
            var kivOrderDetsIN = new List<PoKivOrderDet>();
            var kivOrderDetsOUT = new List<PoKivOrderDet>();
            var kivDets = new List<PoKIVDET>();
            var client = new List<Vendor>();

            kivOrder = db.PoKivOrders.Where(m => m.KorID == id).ToList();
            if (kivOrder.Count > 0)
            {
                custNo = kivOrder.FirstOrDefault().CustNo;
            }
            client = db.Vendors.Where(m => m.CustNo == custNo).ToList();
            kivOrderDets = db.PoKivOrderDets.Where(m => m.KorID == id).ToList();
            kivOrderDetsIN = kivOrderDets.Where(m => m.DeliverOrderQty != 0).ToList();
            bool flag = kivOrderDets.Any(m => m.DeliverOrderQty == 0);
            if (flag)
            {
                kivOrderDetsOUT = kivOrderDets.Where(m => m.DeliverQty > 0).ToList();
                //kivOrderDetsOUT = kivOrderDets.Where(m => (m.DeliverOrderQty == 0) || (m.ChangedQty < 0 && m.DeliverQty > 0)).ToList();
            }

            kivDets = db.PoKIVDETs.Where(m => m.KorID == id).ToList();

            ReportDataSource rdsKivOrder = new ReportDataSource("DataSet1", kivOrder);
            ReportDataSource rdsKivOrderDetsIn = new ReportDataSource("DataSet2", kivOrderDetsIN);
            ReportDataSource rdsKivDets = new ReportDataSource("DataSet3", kivDets);
            ReportDataSource rdsClient = new ReportDataSource("DataSet4", client);
            ReportDataSource rdsKivOrderDetsOut = new ReportDataSource("DataSet5", kivOrderDetsOUT);

            ReportParameter pUserName = new ReportParameter("UserName", userName);

            localReport.DataSources.Add(rdsKivOrder);
            localReport.DataSources.Add(rdsKivOrderDetsIn);
            localReport.DataSources.Add(rdsKivDets);
            localReport.DataSources.Add(rdsClient);
            localReport.DataSources.Add(rdsKivOrderDetsOut);

            localReport.SetParameters(new ReportParameter[] { pUserName });
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

            using (FileStream fs = new FileStream(Server.MapPath("~/Reports/docGRN.pdf"), FileMode.Create))
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

            FileStream fss = new FileStream(Server.MapPath("~/Reports/docGRN.pdf"), FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(Server.MapPath("~/Reports/docGRN.pdf"));
            return File(bytes, "application/pdf");
        }




    }
}
