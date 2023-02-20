<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CaseMasterReport.aspx.cs" Inherits="CTBC.CSFS.Reports.Pages.CaseMasterReport" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
     <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>            
                <asp:UpdatePanel id="updataPanel1" runat="server">
                   <ContentTemplate>
                    <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt"  Width="100%" Height="100%" OnReportRefresh="ReportViewer1_ReportRefresh" ShowExportControls="False" ShowFindControls="False" ZoomMode="PageWidth" SizeToReportContent="True" ShowBackButton="False" ShowRefreshButton="False">
                        <LocalReport ReportPath="Report\Report1.rdlc">
                        </LocalReport>
                    </rsweb:ReportViewer>
                      
                   </ContentTemplate>
                </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>

