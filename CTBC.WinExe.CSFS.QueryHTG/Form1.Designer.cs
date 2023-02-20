namespace CTBC.WinExe.CSFS.QueryHTG
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
         this.label2 = new System.Windows.Forms.Label();
         this.timer1 = new System.Windows.Forms.Timer(this.components);
         this.label1 = new System.Windows.Forms.Label();
         this.listBox1 = new System.Windows.Forms.ListBox();
         this.timer2 = new System.Windows.Forms.Timer(this.components);
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
         this.button1.Location = new System.Drawing.Point(262, 51);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(75, 23);
         this.button1.TabIndex = 2;
         this.button1.Text = "拋查";
         this.button1.UseVisualStyleBackColor = true;
         this.button1.Click += new System.EventHandler(this.button1_Click);
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
         this.label1.Size = new System.Drawing.Size(469, 32);
         this.label1.TabIndex = 6;
         this.label1.Click += new System.EventHandler(this.label1_Click);
         // 
         // listBox1
         // 
         this.listBox1.FormattingEnabled = true;
         this.listBox1.ItemHeight = 12;
         this.listBox1.Location = new System.Drawing.Point(54, 91);
         this.listBox1.Name = "listBox1";
         this.listBox1.Size = new System.Drawing.Size(378, 376);
         this.listBox1.TabIndex = 7;
         // 
         // timer2
         // 
         this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(469, 507);
         this.Controls.Add(this.listBox1);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.button1);
         this.Controls.Add(this.txtId);
         this.Name = "Form1";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
         this.Text = "Form1";
         this.ResumeLayout(false);
         this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Timer timer2;
    }
}

