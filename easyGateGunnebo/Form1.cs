using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using iBoardingPass;
using System.IO.Ports;
using log4net;
using log4net;
using log4net.Config;
using System.Reflection;

namespace easyGateGunnebo
{
    public partial class easyGate : Form
    {
        private Gunnebo.MFLAFLGate GateObj;
        private serialBarcodeReader reader;
        private SerialPort port;


        private string cartaImbarcoITA = "Carta d'Imbarco";
        private string cartaImbarcoENG = "Boarding Pass";
        
        private string nonValidoBP_ITA = "NON VALIDO";
        private string nonValidoBP_ENG = "NOT VALID";
        private string nonValidoDATE_ITA = "GIORNO ERRATO";
        private string nonValidoDATE_ENG = "WRONG DAY";
        private string nonValidoAIRPORT_ITA = "AEROPORTO ERRATO";
        private string nonValidoAIRPORT_ENG = "WRONG AIRPORT";


        private Color greenBckColor = Color.Lime;
        private Color errorBckColor = Color.OrangeRed;
        bool ErrorBarcodeReader
        {
            set
            {
                if(value)
                {
                    panelErrorBarcodeReader.BackColor = errorBckColor;
                }
                else
                {
                    panelErrorBarcodeReader.BackColor = greenBckColor;
                }
            }

        }
        string ErrorBarcodeReaderMsg;
        bool ErrorGunnabo
        {
            set
            {
                if (value)
                {
                    panelErrorGunnabo.BackColor = errorBckColor;
                }
                else
                {
                    panelErrorGunnabo.BackColor = greenBckColor;
                }
            }

        }
        string ErrorGunnaboMsg;

        private bool _busy
        {
            get
            {
                return __busy;
            }
            set
            {
                if(value)
                {
                    labelBusy.Text = "B";
                }
                else
                {
                    labelBusy.Text = "";
                }
                __busy = value;
            }
        }
        private bool __busy;
        private void ResetError()
        {
            ErrorGunnaboMsg = string.Empty;
            ErrorGunnabo = false;
            ErrorBarcodeReaderMsg = string.Empty;
            ErrorBarcodeReader = false;
            Reset();

        }

        private System.Threading.Timer _blockTimer;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public easyGate()
        {
            
            Log.Info("EasyGate Startup");
            InitializeComponent();
            _blockTimer = new System.Threading.Timer(new System.Threading.TimerCallback(OnBlockTimer));
            _blockTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            InitSerialBarcodeReader();
            InitGunnabo();
            labelITA.Text = cartaImbarcoITA;
            labelENG.Text = cartaImbarcoENG;
        }

        private void Reset()
        {
            Log.Info("Reset del Varco");
            labelITA.Text = cartaImbarcoITA;
            labelENG.Text = cartaImbarcoENG;
            ResetError();
           
            try
            { 
            InitSerialBarcodeReader();
                }
            catch(SystemException ex)
            {
                Log.Error("Errore nel resettare il lettore barcode " + ex.ToString());
            }
            try
            {
                GateObj.Dispose();
                InitGunnabo();
            }
            catch(SystemException ex)
            {
                Log.Error("Errore nel resettare il controller del tornello " + ex.ToString());
                ErrorGunnaboMsg = ex.ToString();
                ErrorGunnabo = true;
            }
            
        }

        private void InitGunnabo()
        {
            Log.Info("Inizializzazione Tornello Gunnabo");
            Log.Debug("Parametri Serial Port >>>>>>>>>>>>>>");
            Log.Debug("Port Name " + easyGateGunnebo.Properties.Settings.Default.GunneboSerialPort);
            Log.Debug("BaudRate " + easyGateGunnebo.Properties.Settings.Default.GunneboBaudRate);
            Log.Debug("Parity " + Parity.None);
            Log.Debug("DataBits " + easyGateGunnebo.Properties.Settings.Default.GunneboDataBits);
            Log.Debug("Parametri Serial Port <<<<<<<<<<<<<<<");
            port = new SerialPort(
                easyGateGunnebo.Properties.Settings.Default.GunneboSerialPort,
                easyGateGunnebo.Properties.Settings.Default.GunneboBaudRate,
                Parity.None,
                easyGateGunnebo.Properties.Settings.Default.GunneboDataBits);
            try
            {
                if (port.IsOpen)
                    port.Close();
                port.Open();
            }
            catch(SystemException ex)
            {
                Log.Error("Errore nell'apertura della porta COM del controller del Tornello " + ex.ToString());
                ErrorGunnaboMsg = ex.ToString();
                ErrorGunnabo = true;
            }
            
            GateObj = new Gunnebo.MFLAFLGate((byte)easyGateGunnebo.Properties.Settings.Default.GunneboSlaveId, port);
            GateObj.onError += GateObj_onError;
        }

        void GateObj_onError(ApplicationException ax)
        {
            ErrorGunnaboMsg = ax.ToString();
            ErrorGunnabo = true;
        }

        private void InitSerialBarcodeReader()
        {
            Log.Info("Inizializzazione Barcode Seriale");
            reader = new serialBarcodeReader(
                easyGateGunnebo.Properties.Settings.Default.BarcodeReaderSerial,
                easyGateGunnebo.Properties.Settings.Default.BarcodeBaudRate,
                System.IO.Ports.Parity.None,
                easyGateGunnebo.Properties.Settings.Default.BarcodeDataBits);
            reader.AddParser(new byte[] { 0x02, 0x36 }, new byte[] { 0x0D, 0x03 });  // PDF 915
            reader.AddParser(new byte[] { 0x02, 0x38 }, new byte[] { 0x0D, 0x03 });  // QR Code
            reader.AddParser(new byte[] { 0x02, 0x34 }, new byte[] { 0x0D, 0x03 });  // Alitalia
        
            reader.OnMessageReceived += reader_OnMessageReceived;
            try
            {
                reader.Open();
            }
            catch(SystemException ex) {
                Log.Error("Errore nell'apertura della porta COM del lettore BarCode "+ex.ToString());
                ErrorBarcodeReaderMsg = ex.ToString();
                ErrorBarcodeReader = true;
            }
            
        }

        void reader_OnMessageReceived(string Message)
        {
            Log.Debug("Lettura Barcode: " + Message);
            //label3.Text = Message;
            if (!_busy)
            {
                _busy = true;
                try
                { 
                    BoardingPassData _BpD = BoardingPassData.Parse(Message);

                    if (_BpD != null)
                    {
                        //Console.WriteLine("Flight N." + _BpD.FlightNumber);
                        //Console.WriteLine("Date: "+BPHelper.GetDateFromJulian(Convert.ToInt32(_BpD.DateOfFlight)).ToShortDateString());
                        //Console.WriteLine("From City: "+_BpD.FromCity);
                        //Console.WriteLine("To City: "+_BpD.ToCity);
                        //Console.WriteLine("Carrier: "+_BpD.OperatingCarrier);
                        //Console.WriteLine("Passenger Name: "+ _BpD.PassengerName);


                        bool GoNoGo = true;
                        string resultCheck = string.Empty;
                        string StatoVolo = string.Empty;
                        string OraPrevistaVolo = string.Empty;
                        string OraEffettivaVolo = string.Empty;



                        //Inserisce il dato letto nel DB e verifica la carta di imbarco
                        try
                        {
                            resultCheck = BPHelper.CheckBoardingPass(
                                 easyGateGunnebo.Properties.Settings.Default.ConnectionString,
                                easyGateGunnebo.Properties.Settings.Default.CodAeroporto,
                                _BpD,
                                easyGateGunnebo.Properties.Settings.Default.CodicePostazione,
                                easyGateGunnebo.Properties.Settings.Default.FinestraOreAccettazione,
                                easyGateGunnebo.Properties.Settings.Default.FastTrack,
                                easyGateGunnebo.Properties.Settings.Default.RunningMode,
                                out StatoVolo,
                                out OraPrevistaVolo,
                                out OraEffettivaVolo);
                       
                            if (resultCheck.Trim().IndexOf('X') == -1)
                            {
                                labelITA.Text = cartaImbarcoITA;
                                labelENG.Text = cartaImbarcoENG;
                                try
                                {
                                    GateObj.SetBuzz(200);
                                    GateObj.OpenADir_SingleTransit();
                                    GateObj.SetResetLight();
                                }catch(SystemException ex)
                                {
                                    Log.Error("Errore durante il comando di apertura");
                                    ErrorGunnaboMsg = ex.ToString();
                                    ErrorGunnabo = true;
                                
                                }
                                _busy = false;
                            }
                            else
                            {

                                BlockPassage(resultCheck);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Log.Error("Errore durante la verifica della carta di imbarco");
                            ErrorGunnaboMsg = ex.ToString();
                            ErrorGunnabo = true;
                            _busy = false;
                        }
                    }
               
                    else
                    {
                        BlockPassage(string.Empty);
                    }
                }
                catch(Exception ex)
                {
                    Log.Error("Errore durante il parsing della carta di imbarco");
                    BlockPassage(string.Empty);
                }

            }
            else
            {
                Log.Debug("Messaggio non gestito Sistema Occupato");
            }
        }

        private void BlockPassage(string blockMsg)
        {
            try
            {
                if (blockMsg==string.Empty)
                {
                    labelITA.Text = nonValidoBP_ITA;
                    labelENG.Text = nonValidoBP_ENG;
                }
                if(blockMsg!=null && blockMsg.Length>=1 && blockMsg[0]=='X')
                {
                    labelITA.Text = nonValidoDATE_ITA;
                    labelENG.Text = nonValidoDATE_ENG;
                }
                if (blockMsg != null && blockMsg.Length >= 1 && blockMsg[1] == 'X')
                {
                    labelITA.Text = nonValidoAIRPORT_ITA;
                    labelENG.Text = nonValidoAIRPORT_ENG;
                }
               
                Color bkOld = this.BackColor;
                
                panelErrorBarcodeReader.BackColor = errorBckColor;
                panelErrorGunnabo.BackColor = errorBckColor;
                panelReset.BackColor = errorBckColor;
                panelClose.BackColor = errorBckColor;

                this.BackColor = errorBckColor;

                GateObj.SetBuzz(1000);
                GateObj.SetFlafhRedLight();
                _blockTimer.Change(2500, System.Threading.Timeout.Infinite);
            }
            catch(SystemException ex)
            {
                Log.Error("Errore durante il blocco passaggio");
                ErrorGunnaboMsg = ex.ToString();
                ErrorGunnabo = true;
                _busy = false;
            }
           
            
        }

        private delegate void blockTimerThSafe(object state);
        private void OnBlockTimer(object state)
        {
            try
            { 
                if (this.InvokeRequired)
                {
                    blockTimerThSafe d = new blockTimerThSafe(OnBlockTimer);
                    this.Invoke(d, new object[] { state });
                }
                else
                {
                    labelITA.Text = cartaImbarcoITA;
                    labelENG.Text = cartaImbarcoENG;
                    GateObj.SetResetLight();
                    this.BackColor = this.greenBckColor;
                    panelErrorBarcodeReader.BackColor = greenBckColor;
                    panelErrorGunnabo.BackColor = greenBckColor;
                    panelReset.BackColor = greenBckColor;
                    panelClose.BackColor = greenBckColor;
                }
                
            }
            catch(SystemException ex)
            {
                Log.Error("Errore durante la fase finale del blocco passaggio");
                ErrorGunnaboMsg = ex.ToString();
                ErrorGunnabo = true;
                
            }
            _busy = false;
        }

        private void easyGate_DoubleClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void easyGate_FormClosed(object sender, FormClosedEventArgs e)
        {
            reader.Close();
            port.Close();
            Log.Info("EasyGate Closed");
        }

        private void panelErrorBarcodeReader_Click(object sender, EventArgs e)
        {
            Panel p=(Panel) sender;
            if(p.BackColor==errorBckColor)
            {
                MessageBox.Show(ErrorBarcodeReaderMsg, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panelErrorGunnabo_Click(object sender, EventArgs e)
        {
            Log.Debug("panelErrorGunnabo_Click");
            Panel p = (Panel)sender;
            if (p.BackColor == errorBckColor)
            {
                MessageBox.Show(ErrorGunnaboMsg, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panelClose_Click(object sender, EventArgs e)
        {
            Log.Debug("panelClose_Click");
            this.Close();
        }

        private void panelReset_Click(object sender, EventArgs e)
        {
            Log.Debug("panelReset_Click");
            ResetError();
        }
    }
}
