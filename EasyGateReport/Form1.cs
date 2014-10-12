using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyGateReport
{
    public partial class EasyGateReportForm : Form
    {
        public EasyGateReportForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string folderName=EasyGateReport.Properties.Settings.Default.DataFolder;
            if(!Directory.Exists(folderName))
            {
                try
                {
                    Directory.CreateDirectory(folderName);
                }
                catch(SystemException ex)
                {
                    MessageBox.Show(ex.ToString(),"Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            if(!folderName.EndsWith(@"\"))
            {
                folderName+=@"\";
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                iBoardingPass.BoardingDailyReport bdr = new iBoardingPass.BoardingDailyReport(
                    dateTimePickerReortDate.Value
                    , folderName
                    , EasyGateReport.Properties.Settings.Default.DbConnectionString);
                Process.Start(folderName);
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message
                    , "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            catch (SystemException ex)
            {
                MessageBox.Show(ex.ToString(), "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

       
    }
}
