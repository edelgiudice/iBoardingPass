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
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace easyGateGunnebo
{
    public partial class easyGate : Form
    {
        private Gunnebo.MFLAFLGate GateObj;
        private serialBarcodeReader reader;
        private SerialPort port;
        private string versione = "v1.1.2";

        private string cartaImbarcoITA = "Carta d'Imbarco";
        private string cartaImbarcoENG = "Boarding Pass";
        
        private string nonValidoBP_ITA = "NON VALIDO";
        private string nonValidoBP_ENG = "NOT VALID";
        private string nonValidoDATE_ITA = "GIORNO ERRATO";
        private string nonValidoDATE_ENG = "WRONG DAY";
        private string nonValidoAIRPORT_ITA = "AEROPORTO ERRATO";
        private string nonValidoAIRPORT_ENG = "WRONG AIRPORT";
        private string nonValidoWFLIGHT_ITA = "VOLO ERRATO";
        private string nonValidoWFLIGHT_ENG = "WRONG FLIGHT";
        private string nonValidoFLIGHTNOMORE_ITA = "VOLO NON PIU DISP.";
        private string nonValidoFLIGHTNOMORE_ENG = "FLIGHT NO AVAILABLE";
        private string nonValidoTIMEWINDOW_ITA = "GATE CHIUSO";
        private string nonValidoTIMEWINDOW_ENG = "GATE CLOSED";

        private Table<ICAO2IATA> icao2IataTable;

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
                    if (this.InvokeRequired)
                    {
                        blockTimerThSafe d = new blockTimerThSafe(OnChangeBusy);
                        this.Invoke(d, new object[] { value });
                    }
                    else
                    {
                        labelBusy.Text = "B";
                    }
                }
                else
                {
                    if (this.InvokeRequired)
                    {
                        blockTimerThSafe d = new blockTimerThSafe(OnChangeBusy);
                        this.Invoke(d, new object[] { value });
                    }
                    else
                    {
                        labelBusy.Text = "";
                    }
                }
                __busy = value;

            }
        }

        private void OnChangeBusy(object state)
        {
            if ((bool)state)
            {
                labelBusy.Text = "B";
            }
            else
            {
                labelBusy.Text = "";
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
        private int _runningMode;

        private System.Threading.Timer _blockTimer;
        private System.Threading.Timer _serverTimer;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool passepartoutEnabled;
        public easyGate()
        {

            Label.CheckForIllegalCrossThreadCalls = false;
            
            Log.Info("EasyGate Startup");

            InitializeComponent();

            this.version.Text = versione;

            _blockTimer = new System.Threading.Timer(new System.Threading.TimerCallback(OnBlockTimer));
            _blockTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            _serverTimer=new System.Threading.Timer(new System.Threading.TimerCallback(OnLookForServer));
            _serverTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            InitSerialBarcodeReader();
            InitGunnabo();
            labelITA.Text = cartaImbarcoITA;
            labelENG.Text = cartaImbarcoENG;

            _runningMode = easyGateGunnebo.Properties.Settings.Default.RunningMode;

            LoadIcao2Iata();

            passepartoutEnabled = easyGateGunnebo.Properties.Settings.Default.passepartoutEnabled;
        }

        private void LoadIcao2Iata()
        {
            if (_runningMode == 1)
            {
                try
                {

                    DataContext dx = new DataContext(easyGateGunnebo.Properties.Settings.Default.ConnectionString);

                    icao2IataTable = dx.GetTable<ICAO2IATA>();
                }
                catch
                {

                }
            }
        }

        private void OnLookForServer(object state)
        {
            _serverTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            Log.Error("Imposto nuovamente il tornello in running mode 1");
            _runningMode=easyGateGunnebo.Properties.Settings.Default.RunningMode;
            LoadIcao2Iata();
            panelNODB.BackColor = greenBckColor;

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

        int retryCounter = 3;
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



                    //label3.Text = Message;
                    if (_BpD != null)
                    {
                        if (_BpD.ManualForced && passepartoutEnabled)
                        {
                            //Log.Debug("TTTT:01");
                            OpenGate();
                            return;
                        }

                        //Console.WriteLine("Flight N." + _BpD.FlightNumber);
                        //Console.WriteLine("Date: "+BPHelper.GetDateFromJulian(Convert.ToInt32(_BpD.DateOfFlight)).ToShortDateString());
                        //Console.WriteLine("From City: "+_BpD.FromCity);
                        //Console.WriteLine("To City: "+_BpD.ToCity);
                        //Console.WriteLine("Carrier: "+_BpD.OperatingCarrier);
                        //Console.WriteLine("Passenger Name: "+ _BpD.PassengerName);

                        if (_runningMode == 1 && icao2IataTable != null && icao2IataTable.Count() > 0)
                        {
                            //Log.Debug("TTTT:02" + _BpD.OperatingCarrier.Trim());
                            if (_BpD.OperatingCarrier.Trim().Length > 2)
                            {
                                var v = icao2IataTable.Where(w => w.ICAO == _BpD.OperatingCarrier).FirstOrDefault();
                                if ((v != null) && (v.IATA.Trim() != string.Empty))
                                {
                                    _BpD.OperatingCarrier = v.IATA;
                                    //Log.Debug("TTTT:03" + v.IATA);
                                }
                            }
                        }
                        else
                        {
                            _runningMode = 0;
                        }
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
                                _runningMode,
                                out StatoVolo,
                                out OraPrevistaVolo,
                                out OraEffettivaVolo);

                            retryCounter = 3;

                            if (resultCheck.Trim().IndexOf('X') == -1)
                            {
                                OpenGate();
                            }
                            else
                            {

                                BlockPassage(resultCheck);
                            }
                        }
                        catch (BoardingPassException ex)
                        {
                            Log.Error(ex.Message);
                            ErrorGunnaboMsg = ex.ToString();
                            ErrorGunnabo = true;
                            _busy = false;
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
                catch (System.Data.SqlClient.SqlException ex)
                {
                    if (retryCounter == 3)
                    {
                       
                        double interval = (double)easyGateGunnebo.Properties.Settings.Default.ResetRunnigMode;
                        double intervalMs = TimeSpan.FromMinutes(interval).TotalMilliseconds;
                        _serverTimer.Change(
                            (long)intervalMs
                            , System.Threading.Timeout.Infinite);
                    }
                    Log.Error("Database non raggiungibile");
                    _runningMode = 0;
                    panelNODB.BackColor = errorBckColor;
                    if (retryCounter-- > 0)
                    {
                        _busy = false;
                        reader_OnMessageReceived(Message);
                    }
                    else
                    {
                        BlockPassage(string.Empty);
                        retryCounter = 3;
                    }
                    

                }
                catch (Exception ex)
                {
                    Log.Error("Errore durante il parsing della carta di imbarco");
                    BlockPassage(string.Empty);
                }
                finally
                {
                    _busy = false;
                }

            }
            else
            {
                Log.Debug("Messaggio non gestito Sistema Occupato");
            }
        }

        private void OpenGate()
        {
            labelITA.Text = cartaImbarcoITA;
            labelENG.Text = cartaImbarcoENG;
            try
            {
                GateObj.SetBuzz(200);
                GateObj.OpenADir_SingleTransit();
                GateObj.SetResetLight();
            }
            catch (SystemException ex)
            {
                Log.Error("Errore durante il comando di apertura");
                ErrorGunnaboMsg = ex.ToString();
                ErrorGunnabo = true;

            }
            _busy = false;
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

                if (blockMsg != null && blockMsg.Length >= 2 && blockMsg[2] == 'X')
                {
                    labelITA.Text = nonValidoWFLIGHT_ITA;
                    labelENG.Text = nonValidoWFLIGHT_ENG;
                }

          

                if (blockMsg != null && blockMsg.Length >= 3 && blockMsg[3] == 'X')
                {
                    labelITA.Text = nonValidoFLIGHTNOMORE_ITA;
                    labelENG.Text = nonValidoFLIGHTNOMORE_ENG;
                }

                if (blockMsg != null && blockMsg.Length >= 4 && blockMsg[4] == 'X')
                {
                    labelITA.Text = nonValidoTIMEWINDOW_ITA;
                    labelENG.Text = nonValidoTIMEWINDOW_ENG;
                }

                

                Color bkOld = this.BackColor;
                
                panelErrorBarcodeReader.BackColor = errorBckColor;
                panelErrorGunnabo.BackColor = errorBckColor;
                panelNODB.Visible = false;

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
                    panelNODB.Visible = true;
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

    [Table]
    public class ICAO2IATA
    {

        [Column]
        public int ID { get; set; }
        [Column]
        public string ICAO { get; set; }
        [Column]
        public string IATA { get; set; }

    }
}
