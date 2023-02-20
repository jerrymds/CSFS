using System;
using System.Data;
using System.Windows.Forms;
using System.Configuration;
using System.Diagnostics;

namespace CTBC.WinExe.CSFS.QueryRFDM
{
    public partial class Form1 : Form
    {
       string RFDM_Service = ConfigurationManager.AppSettings["RFDM_Service"].ToString();
       string RFDM_Resolve = ConfigurationManager.AppSettings["RFDM_Resolve"].ToString();

       string VersionNewID = null;

       QueryRFDMBIZ PQueryRFDMBIZ = new QueryRFDMBIZ();

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 拋查按鈕事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

           label1.Text = "呼叫RFDM程式中！ ....";

           //  取得輸入的ID
            string strID_No = this.txtId.Text.Trim();

            // 根據輸入的ID 寫入CaseCustRFDMSend
            VersionNewID = PQueryRFDMBIZ.InsertCaseCustRFDMSend(strID_No);

           // Call RFDM
            Process.Start(RFDM_Service);

           // Enable timer to waiting RFDM feedback
            timer1.Interval = 10000;
            timer1.Enabled = true;

            label1.Text = "已完成呼叫RFDM程式，等待RFDM回應中！ .... (" + VersionNewID + ")";
        }

        /// <summary>
        /// 回文資料按鈕事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
           ShowGrid();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           timer1.Enabled = false;

           // Check feedback from RFMD
           int count = PQueryRFDMBIZ.CheckCaseCustRFDMRecv(this.txtId.Text.Trim(), VersionNewID);
           
           if (count > 0)
           {
              label1.Text = "RFDM回應檔案已產生，請執行〔下載回文資料〕按鈕！ .... (" + VersionNewID + ")";
           }
           else
           {
              // Enable timer to waiting RFDM feedback
              timer1.Enabled = true;
           }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void ShowGrid()
        {
           //  取得輸入的ID
           string strID_No = this.txtId.Text.Trim();

           DataTable dt = PQueryRFDMBIZ.SelectCaseCustRFDMRecv(strID_No);

           if (dt.Rows.Count > 0)
           {
              label1.Text = "解析RFDM回傳資料中！ ....";

              #region 標題顯示中文

              dt.Columns["DATA_DATE"].ColumnName = "資料日期";
              dt.Columns["ACCT_NO"].ColumnName = "帳號";
              dt.Columns["JNRST_DATE"].ColumnName = "交易日";
              dt.Columns["JNRST_TIME"].ColumnName = "交易時間";
              dt.Columns["JNRST_TIME_SEQ"].ColumnName = "交易時間-序號";
              dt.Columns["TRAN_DATE"].ColumnName = "起息日";
              dt.Columns["POST_DATE"].ColumnName = "帳務日";
              dt.Columns["TRANS_CODE"].ColumnName = "交易代號";
              dt.Columns["JRNL_NO"].ColumnName = "交易序號";
              dt.Columns["REVERSE"].ColumnName = "沖正";
              dt.Columns["PROMO_CODE"].ColumnName = "PROMO CODE";
              dt.Columns["REMARK"].ColumnName = "註記";
              dt.Columns["TRAN_AMT"].ColumnName = "交易金額";
              dt.Columns["BALANCE"].ColumnName = "餘額";
              dt.Columns["TRF_BANK"].ColumnName = "轉出入帳號 -銀行別";
              dt.Columns["TRF_ACCT"].ColumnName = "轉出入帳號";
              dt.Columns["NARRATIVE"].ColumnName = "備註";
              dt.Columns["FISC_BANK"].ColumnName = "金資序號-行庫別";
              dt.Columns["FISC_SEQNO"].ColumnName = "金資序號";
              dt.Columns["CHQ_NO"].ColumnName = "票號";
              dt.Columns["ATM_NO"].ColumnName = "ATM 機器編號";
              dt.Columns["TRAN_BRANCH"].ColumnName = "交易分行";
              dt.Columns["TELLER"].ColumnName = "交易櫃員";
              dt.Columns["FILLER"].ColumnName = "PROMO CODE中文或交易說明";
              dt.Columns["TXN_DESC"].ColumnName = "摘要";
              dt.Columns["ACCT_P2"].ColumnName = "帳號的後四碼的前兩碼";
              dt.Columns["FILE_NAME"].ColumnName = "匯入檔案名稱";
              dt.Columns["TYPE"].ColumnName = "TYPE";

           #endregion

              this.dataRecv.DataSource = dt;

              label1.Text = "已有資料顯示於下方，可按〔重讀回文資料〕重新取得RFDM匯入資料！ ....";
           }
           else
           {
              timer2.Enabled = true;

           }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
           timer2.Enabled = false;

           // Show data
           ShowGrid();

        }

        private void button3_Click(object sender, EventArgs e)
        {

           label1.Text = "從MFPT下載檔及匯入中，需要一些時間，請稍後執行〔重讀回文資料〕按鈕顯示資料！ ....";

           // Download files from MFTP and parse data
           Process.Start(RFDM_Resolve);

           timer2.Interval = 2000;
           timer2.Enabled = true;

        }
    }
}
