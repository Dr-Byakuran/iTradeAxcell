<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
@{

}
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Invoice Print Preview</title>
        <script runat="server">
            
        void Page_Load(object sender, EventArgs e)
        {
            iAmasco.Models.StarDbContext db = new iAmasco.Models.StarDbContext();
            
            if (!IsPostBack)
            {
                var param = Session["InvID"].ToString();

                int InvID = int.Parse(param);
                int custNo = 0;
                var client = new List<iAmasco.Models.Client>();
                var inv = new List<iAmasco.Models.INV>();
                var invDets = new List<iAmasco.Models.INVDET>();

                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/ReportTemplate/rptInvoice.rdlc");
                ReportViewer1.LocalReport.DataSources.Clear();


                inv = db.INVs.Where(m => m.InvID == InvID).ToList();
                if (inv.Count>0)
                {
                    custNo = inv.FirstOrDefault().CustNo;
                }
                client = db.Clients.Where(m => m.CustNo == custNo).ToList();               
                invDets = db.INVDETs.Where(m => m.InvID == InvID).ToList();
                
                ReportDataSource rdsClient = new ReportDataSource("DataSet1", client);
                ReportDataSource rdsInv = new ReportDataSource("DataSet2", inv);
                ReportDataSource rdsInvDets = new ReportDataSource("DataSet3", invDets); 
                ReportViewer1.LocalReport.DataSources.Add(rdsClient);                
                ReportViewer1.LocalReport.DataSources.Add(rdsInv);
                ReportViewer1.LocalReport.DataSources.Add(rdsInvDets);
                ReportViewer1.LocalReport.Refresh();
            }
        }

      
    </script>
</head>
<body>
    <form id="frmInvoice" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" AsyncRendering="false" SizeToReportContent="true" Height="486px" Width="747px">
        </rsweb:ReportViewer> 
    </div>
    </form>
</body>
</html>