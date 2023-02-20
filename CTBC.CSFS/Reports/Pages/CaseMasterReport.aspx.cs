using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CTBC.CSFS.BussinessLogic;
using CTBC.CSFS.Models;
using Microsoft.Reporting.WebForms;

namespace CTBC.CSFS.Reports.Pages
{
    public partial class CaseMasterReport : System.Web.UI.Page
    {
        List<ReportDataSource> subDataSource = new List<ReportDataSource>();
        public CaseMasterBIZ caseMaster = new CaseMasterBIZ();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ReportView_Binding();
            }
        }

        public void ReportView_Binding()
        {
            List<ReportParameter> listParm = new List<ReportParameter>();

            PARMCode codeItem = caseMaster.GetCodeData("REPORT_SETTING", "Address").FirstOrDefault();
            listParm.Add(new ReportParameter("CtbcAddr", codeItem == null ? "" : codeItem.CodeDesc));
            codeItem = caseMaster.GetCodeData("REPORT_SETTING", "Tel").FirstOrDefault();
            listParm.Add(new ReportParameter("CtbcTel", codeItem == null ? "" : codeItem.CodeDesc));
            string ctbcTel = codeItem == null ? "" : codeItem.CodeDesc;
            codeItem = caseMaster.GetCodeData("REPORT_SETTING", "Fax").FirstOrDefault();
            listParm.Add(new ReportParameter("CtbcFax", codeItem == null ? "" : codeItem.CodeDesc));
            
            string strCaseIdList = Request["caseIdList"] ?? "";
            List<string> caseIdList = strCaseIdList.Split(',').ToList();
            DataTable dtMaster = caseMaster.GetCaseMasterByCaseIdList(caseIdList);
            DataTable dtSendSetting = new CaseSendSettingBIZ().GetSendSettingByCaseIdList(caseIdList);

            this.ReportViewer1.Reset();
            this.ReportViewer1.LocalReport.Dispose();
            this.ReportViewer1.LocalReport.DataSources.Clear();
            this.ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Reports/Rdlc/CaseMaster.rdlc");
            this.ReportViewer1.LocalReport.SetParameters(listParm); //*添加參數
            this.ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dtMaster));   //* 添加數據源,可以多個
            this.ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("SendSetting", dtSendSetting));   //* 添加數據源,可以多個

            this.ReportViewer1.LocalReport.SubreportProcessing += new SubreportProcessingEventHandler(SubreportProcessingEventHandler);

            subDataSource.Add(new ReportDataSource("SendSetting", dtSendSetting));
            this.ReportViewer1.LocalReport.Refresh();
        }

        protected void ReportViewer1_ReportRefresh(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ReportView_Binding();
        }

        void SubreportProcessingEventHandler(object sender, SubreportProcessingEventArgs e)
        {
            foreach (var reportDataSource in subDataSource)
            {
                e.DataSources.Add(reportDataSource);
            }
        }
    }
}