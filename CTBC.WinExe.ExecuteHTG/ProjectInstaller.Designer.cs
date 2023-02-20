namespace ExcuteESB
{
    partial class ProjectInstaller
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ExcuteESBProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.ExcuteESBInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // ExcuteESBProcessInstaller1
            // 
            this.ExcuteESBProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ExcuteESBProcessInstaller1.Password = null;
            this.ExcuteESBProcessInstaller1.Username = null;
            // 
            // ExcuteESBInstaller1
            // 
            this.ExcuteESBInstaller1.ServiceName = "ExcuteHTG";
            this.ExcuteESBInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ExcuteESBProcessInstaller1,
            this.ExcuteESBInstaller1});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ExcuteESBProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller ExcuteESBInstaller1;
    }
}