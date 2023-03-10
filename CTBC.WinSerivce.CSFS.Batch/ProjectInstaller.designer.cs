using System.Configuration.Install;
using System.ServiceProcess;

namespace CTBC.WinSerivce.CSFS.Batch
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
      this.serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
      this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();
      // 
      // serviceProcessInstaller
      // 
      this.serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
      this.serviceProcessInstaller.Password = null;
      this.serviceProcessInstaller.Username = null;
      // 
      // serviceInstaller
      // 
      this.serviceInstaller.Description = "新外來文系統外部批次執行排程程式";
      this.serviceInstaller.ServiceName = "CTBC.WinService.CSFS.Batch";
      this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
      // 
      // ProjectInstaller
      // 
      this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller,
            this.serviceInstaller});
      this.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.ServiceInstaller_AfterInstall);

    }

    void ServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
    {
      using (ServiceController sc = new ServiceController(this.serviceInstaller.ServiceName))
      {
        sc.Start();
      }
    }

    #endregion

    private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
    private System.ServiceProcess.ServiceInstaller serviceInstaller;
  }
}