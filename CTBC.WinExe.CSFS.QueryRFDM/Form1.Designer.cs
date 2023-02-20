namespace CTBC.WinExe.CSFS.QueryRFDM
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
         this.components = new System.ComponentModel.Container();
         this.txtId = new System.Windows.Forms.TextBox();
         this.button1 = new System.Windows.Forms.Button();
         this.button2 = new System.Windows.Forms.Button();
         this.label2 = new System.Windows.Forms.Label();
         this.dataRecv = new System.Windows.Forms.DataGridView();
         this.timer1 = new System.Windows.Forms.Timer(this.components);
         this.label1 = new System.Windows.Forms.Label();
         this.timer2 = new System.Windows.Forms.Timer(this.components);
         this.button3 = new System.Windows.Forms.Button();
         ((System.ComponentModel.ISupportInitialize)(this.dataRecv)).BeginInit();
         this.SuspendLayout();
         // 
         // txtId
         // 
         this.txtId.Location = new System.Drawing.Point(101, 51);
         this.txtId.Name = "txtId";
         this.txtId.Size = new System.Drawing.Size(117, 22);
         this.txtId.TabIndex = 0;
         // 
         // button1
         // 
         this.button1.Location = new System.Drawing.Point(262, 42);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(75, 40);
         this.button1.TabIndex = 2;
         this.button1.Text = "拋查";
         this.button1.UseVisualStyleBackColor = true;
         this.button1.Click += new System.EventHandler(this.button1_Click);
         // 
         // button2
         // 
         this.button2.Location = new System.Drawing.Point(532, 41);
         this.button2.Name = "button2";
         this.button2.Size = new System.Drawing.Size(100, 40);
         this.button2.TabIndex = 3;
         this.button2.Text = "重讀回文資料";
         this.button2.UseVisualStyleBackColor = true;
         this.button2.Click += new System.EventHandler(this.button2_Click);
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(52, 55);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(41, 12);
         this.label2.TabIndex = 4;
         this.label2.Text = "客戶ID";
         // 
         // dataRecv
         // 
         this.dataRecv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
         this.dataRecv.Location = new System.Drawing.Point(7, 96);
         this.dataRecv.Name = "dataRecv";
         this.dataRecv.RowTemplate.Height = 24;
         this.dataRecv.Size = new System.Drawing.Size(956, 399);
         this.dataRecv.TabIndex = 5;
         // 
         // timer1
         // 
         this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
         // 
         // label1
         // 
         this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         this.label1.Dock = System.Windows.Forms.DockStyle.Top;
         this.label1.ForeColor = System.Drawing.Color.Red;
         this.label1.Location = new System.Drawing.Point(0, 0);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(967, 32);
         this.label1.TabIndex = 6;
         this.label1.Click += new System.EventHandler(this.label1_Click);
         // 
         // timer2
         // 
         this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
         // 
         // button3
         // 
         this.button3.Location = new System.Drawing.Point(384, 40);
         this.button3.Name = "button3";
         this.button3.Size = new System.Drawing.Size(100, 40);
         this.button3.TabIndex = 7;
         this.button3.Text = "下載回文資料";
         this.button3.UseVisualStyleBackColor = true;
         this.button3.Click += new System.EventHandler(this.button3_Click);
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(967, 507);
         this.Controls.Add(this.button3);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.dataRecv);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.button2);
         this.Controls.Add(this.button1);
         this.Controls.Add(this.txtId);
         this.Name = "Form1";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Text = "Form1";
         ((System.ComponentModel.ISupportInitialize)(this.dataRecv)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dataRecv;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Button button3;
    }
}

