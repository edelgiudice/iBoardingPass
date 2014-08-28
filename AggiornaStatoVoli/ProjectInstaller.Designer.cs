namespace AggiornaStatoVoli
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
            this.serviceProcessInstallerAggVoli = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstallerAggVoli = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstallerAggVoli
            // 
            this.serviceProcessInstallerAggVoli.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceProcessInstallerAggVoli.Password = null;
            this.serviceProcessInstallerAggVoli.Username = null;
            // 
            // serviceInstallerAggVoli
            // 
            this.serviceInstallerAggVoli.ServiceName = "EasyGate Agg. Stato Voli";
            this.serviceInstallerAggVoli.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstallerAggVoli,
            this.serviceInstallerAggVoli});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstallerAggVoli;
        private System.ServiceProcess.ServiceInstaller serviceInstallerAggVoli;
    }
}