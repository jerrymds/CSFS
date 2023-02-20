using System;
using System.Data;
using System.Windows.Forms;
using System.Configuration;
using System.Diagnostics;

namespace CTBC.WinExe.CSFS.QueryHTG
{
    public partial class Form1 : Form
    {
       string HTG_Service = ConfigurationManager.AppSettings["HTG_Service"].ToString();

       QueryHTGBIZ PQueryHTGBIZ = new QueryHTGBIZ();

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

           label1.Text = "呼叫HTG程式中！ ....";

           //  取得輸入的ID
            string strID_No = this.txtId.Text.Trim();

            // 根據輸入的ID 寫入CaseCustHTGSend
            PQueryHTGBIZ.InsertCaseCustHTGSend(strID_No);

            timer2.Interval = 10000;
            //timer2.Enabled = true;
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
           timer1.Enabled = false;

           // Check feedback from HTG
           DataTable dt = PQueryHTGBIZ.CheckCaseCustHTGRecv(this.txtId.Text.Trim());

           if (dt != null && dt.Rows.Count > 0)
           {
              label1.Text = "顯示HTG回傳筆數！ ....";

              
              listBox1.Items.Clear();
              listBox1.BeginUpdate();

              foreach(DataRow row in dt.Rows)
              {
                 listBox1.Items.Add(row["name"] + " = " + row["num"].ToString());
              }

              listBox1.EndUpdate();

              label1.Text = "資料已顯示完成！ ....";
           }
           else
           {
              // Enable timer to waiting HTG feedback
              timer1.Enabled = true;
           }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
           timer2.Enabled = false;

           // Call HTG
           Process.Start(HTG_Service);

           // Enable timer to waiting HTG feedback
           timer1.Interval = 30000;
           timer1.Enabled = true;

           label1.Text = "已完成呼叫HTG程式，等待HTG回應中！ ....";

        }

    }
}
