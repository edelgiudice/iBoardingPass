using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.PointOfService;
using System.IO.Ports;
using System.Windows.Threading;

namespace iBoardingPass
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DateTime _lastKeystroke = new DateTime(0);
        List<byte> _barcode = new List<byte>(500);
        serialBarcodeReader serialBarCodeReader = null;

        System.Diagnostics.EventLog eventLogBoardingPass;


        //Parametri per le statistiche 
        int NumLetture = 0;         //Numero letture effettuate dall'ultimo reset delle statistiche
        int NumIngressi = 0;        //Numero Ingressi effettuate dall'ultimo reset delle statistiche
        int NumRespinti = 0;        //Numero Respinti effettuate dall'ultimo reset delle statistiche
        DateTime StartStat;         // Istante di riferimento per l'eacquisizione dei dato statistici

        //Timer per la gestione dell'applicativo
        DispatcherTimer LedTimer;           //Timer per la persistenza dei "LED" virtuali
        DispatcherTimer StatsTimer;         //Timer di aggiornamento statistiche
        DispatcherTimer ClockTimer;         //Timer per aggiornamento orologio interno
        DispatcherTimer GetFocusTimer;      //Timer per la gestione del fuoco dovuto al lettrore barcode
        DispatcherTimer BarCodeTimer;       //Timer per la gestione del flusso di caratteri del Barcode


        BoardingPassData LastBpD = null;    // Strittura di appoggio in cui è memorizzata l'ultima lettura corretta del BoardingPass.

        string easyGateDBConnStr;           // Stringa di connesione al DB
        int codicePostazione;               // Codice postazione di lettura

      
        // Risorse wave per la segnalazione del risultato
        Uri SoundOk = new Uri(@"pack://siteoforigin:,,,/Resources/beepOK.wav");   //Suono per OK
        Uri SoundKO = new Uri(@"pack://siteoforigin:,,,/Resources/beepKO.wav");   //Suono per KO

        int FinestraOreaccettazione;

        public MainWindow()
        {
            serialBarCodeReader = new serialBarcodeReader("COM1", 9600, Parity.None, 8);
            serialBarCodeReader.AddParser(new byte[] { 0x02, 0x36 }, new byte[] { 0x0D, 0x03 });
            serialBarCodeReader.AddParser(new byte[] { 0x02, 0x38 }, new byte[] { 0x0D, 0x03 });
            serialBarCodeReader.OnMessageReceived += serialBarCodeReader_OnMessageReceived;
            serialBarCodeReader.Open();

            eventLogBoardingPass = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("EasyGate_LettoreBoardigCard"))
            {
                System.Diagnostics.EventLog.CreateEventSource("EasyGate_LettoreBoardigCard", "BoardingCardLog");
            }
            eventLogBoardingPass.Source = "EasyGate_LettoreBoardigCard";
            eventLogBoardingPass.Log = "BoardingCardLog";
            eventLogBoardingPass.WriteEntry("START PROGRAM", EventLogEntryType.Information);

            try
            {

               
                InitializeComponent();

                // Inizializzazione strutture interne e UI
                bnManualEnter.Visibility = Visibility.Hidden;

                LedTimer = new DispatcherTimer();
                LedTimer.Tick += LedTimer_Tick;
                LedTimer.Interval = TimeSpan.FromMilliseconds(500); ;
                LedTimer.IsEnabled = false;
                LedTimer.Stop();

                StartStat = DateTime.Now;
                StatsTimer = new DispatcherTimer();
                StatsTimer.Tick += StatsTimer_Tick;
                StatsTimer.Interval = TimeSpan.FromMinutes(1);
                StatsTimer.IsEnabled = true;
                StatsTimer.Start();


                ClockTimer = new DispatcherTimer();
                ClockTimer.Tick += ClockTimer_Tick;
                ClockTimer.Interval = TimeSpan.FromSeconds(1);
                ClockTimer.IsEnabled = true;
                ClockTimer.Start();

                GetFocusTimer = new DispatcherTimer();
                GetFocusTimer.Tick += GetFocusTimer_Tick;
                GetFocusTimer.Interval = TimeSpan.FromMilliseconds(300);
                GetFocusTimer.IsEnabled = false;
                GetFocusTimer.Start();

                BarCodeTimer = new DispatcherTimer();
                BarCodeTimer.Tick += BarCodeTimer_Tick;
                BarCodeTimer.Interval = TimeSpan.FromMilliseconds(100);
                BarCodeTimer.IsEnabled = false;
                BarCodeTimer.Start();


                this.dummyBox.TextChanged += tb_TextChanged;
                this.dummyBox.Text = "";
                this.dummyBox.Focus();

                cbStazionePrincipale.IsChecked = iBoardingPass.Properties.Settings.Default.IngressoCoda;
                cbFastTrack.IsChecked = iBoardingPass.Properties.Settings.Default.FastTrackAlarm;

                bnManualEnter.Background = new SolidColorBrush(Color.FromRgb(205, 205, 205));
                bnManualEnter.Content = string.Empty;

                // dummyBox è il controllo nascosto che raccoglie il testo del lettore BARCODE
                //this.dummyBox.Visibility = Visibility.Hidden;

                easyGateDBConnStr = iBoardingPass.Properties.Settings.Default.easyGateDBConnStr;
                codicePostazione = iBoardingPass.Properties.Settings.Default.CodicePostazione;

                FinestraOreaccettazione = iBoardingPass.Properties.Settings.Default.FinestraOreAccettazione;

                ResetBoardingData();
            }
            catch (System.Exception ex)
            {
                eventLogBoardingPass.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }

        }

        void serialBarCodeReader_OnMessageReceived(string Message)
        {
            this.tLed.Background = new SolidColorBrush(Colors.Green);
            this.tLed.Foreground = new SolidColorBrush(Colors.White);
            this.tLed.Text = "OK";
            try
            {
                BoardingPassData _BpD = BoardingPassData.Parse(Message);

                if (_BpD != null)
                {
                    NumLetture++;
                    tbLetture.Text = NumLetture.ToString();

                    tbFlightNum.Text = _BpD.FlightNumber;
                    tbFlightDate.Text = BPHelper.GetDateFromJulian(Convert.ToInt32(_BpD.DateOfFlight)).ToShortDateString();
                    tbCodFromCity.Text = _BpD.FromCity;
                    tbCodToCity.Text = _BpD.ToCity;
                    tbAirCmpany.Text = _BpD.OperatingCarrier;
                    tbPassengerName.Text = _BpD.PassengerName;

                    LastBpD = _BpD;

                    VerifyBoardingPass(LastBpD);
                    bnManualEnter.Visibility = Visibility.Visible;
                }
            }
            catch (System.Exception ex)
            {
                eventLogBoardingPass.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
        }

      

        void BarCodeTimer_Tick(object sender, EventArgs e)
        {
            BarCodeTimer.IsEnabled = false;
            BarCodeTimer.Stop();
            //Debug.WriteLine(this.dummyBox.Text);
            this.tLed.Background = new SolidColorBrush(Colors.Green);
            this.tLed.Foreground = new SolidColorBrush(Colors.White);
            this.tLed.Text = "OK";
            try
            {
                BoardingPassData _BpD = BoardingPassData.Parse(this.dummyBox.Text);

                if (_BpD != null)
                {
                    NumLetture++;
                    tbLetture.Text = NumLetture.ToString();

                    tbFlightNum.Text = _BpD.FlightNumber;
                    tbFlightDate.Text = BPHelper.GetDateFromJulian(Convert.ToInt32(_BpD.DateOfFlight)).ToShortDateString();
                    tbCodFromCity.Text = _BpD.FromCity;
                    tbCodToCity.Text = _BpD.ToCity;
                    tbAirCmpany.Text = _BpD.OperatingCarrier;
                    tbPassengerName.Text = _BpD.PassengerName;

                    LastBpD = _BpD;

                    VerifyBoardingPass(LastBpD);
                    bnManualEnter.Visibility = Visibility.Visible;
                }
            }
            catch (System.Exception ex)
            {
                eventLogBoardingPass.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
            //Cancellazione del contenuto di dummyBox per una nuova lettura
            //L'operazione di cancellazione avviene dopo aver inibito i meccanismi che tracciano la variazione del contenuto della casella
            
            this.dummyBox.TextChanged -= tb_TextChanged;
            this.dummyBox.Text = string.Empty;
            this.dummyBox.TextChanged += tb_TextChanged;
        }

        void GetFocusTimer_Tick(object sender, EventArgs e)
        {
            this.dummyBox.Focus();
            GetFocusTimer.IsEnabled = false;
        }

        void StatsTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan DeltaTime = DateTime.Now.Subtract(StartStat);
            this.tbMinuti.Text = Convert.ToInt32(DeltaTime.TotalMinutes).ToString();
        }

        void ClockTimer_Tick(object sender, EventArgs e)
        {
            DateTime Now = DateTime.Now;
            tbData.Text = Now.ToShortDateString();
            tbOra.Text = Now.ToLongTimeString();
        }

        void LedTimer_Tick(object sender, EventArgs e)
        {
            this.LedTimer.IsEnabled = false;
            this.tLed.Background = new SolidColorBrush(Colors.Gray);
            this.tLed.Foreground = new SolidColorBrush(Colors.White);
            this.tLed.Text = "PRONTO";
            LedTimer.Stop();
        }

        private void ResetBoardingData()
        {
            tbFlightNum.Text = "---------"; ledFlightNumb.Fill = new SolidColorBrush(Colors.LightGray);
            tbFlightDate.Text = "--/--/----"; ledFlightDate.Fill = new SolidColorBrush(Colors.LightGray);
            tbCodFromCity.Text = "---"; ledAeroportoPartenza.Fill = new SolidColorBrush(Colors.LightGray);
            tbCodToCity.Text = "---";
            tbAirCmpany.Text = "---";
            tbPassengerName.Text = "--------------------";
            TBOrarioPrevisto.Text = "--:--";
            TBOrarioEffettivo.Text = "--:--"; ledFlightTime.Fill = new SolidColorBrush(Colors.LightGray);
            TBStatoVolo.Text = "------------------------"; ledFlightStatus.Fill = new SolidColorBrush(Colors.LightGray);

            bnManualEnter.Background=new SolidColorBrush(Color.FromRgb(205,205,205));
            bnManualEnter.Content=string.Empty;

            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri("pack://siteoforigin:,,,/Resources/FT_OFF.png");
            img.EndInit();
            imgFastTrack.Source = img;

            bnManualEnter.Visibility = Visibility.Hidden;
        }


        /// <summary>
        /// Forza il ritorno del fuoco sul componente destinato a catturare l'output dello scanner barcode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dummyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            GetFocusTimer.IsEnabled = true;
        }
      


        private void  VerifyBoardingPass(BoardingPassData _BpD)
        {
            bool GoNoGo = true;
            string resultCheck=string.Empty;
            string StatoVolo=string.Empty;
            string OraPrevistaVolo=string.Empty;
            string OraEffettivaVolo=string.Empty;

            ledFlightNumb.Fill = new SolidColorBrush(Colors.LightGreen);
            
            //Inserisce il dato letto nel DB e verifica la carta di imbarco
            try
            {
                Exception exResult = null;
                resultCheck = BPHelper.CheckBoardingPass(this.easyGateDBConnStr, 
                    iBoardingPass.Properties.Settings.Default.CodAeroporto, 
                    _BpD, 
                    this.codicePostazione, 
                    FinestraOreaccettazione,
                    iBoardingPass.Properties.Settings.Default.FastTrackAlarm, 
                    out StatoVolo,
                    out OraPrevistaVolo,
                    out OraEffettivaVolo,
                    out exResult);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
           
            //Verifica la data del volo
            if (resultCheck[0] == 'X')
            {
                ledFlightDate.Fill = new SolidColorBrush(Colors.Red);
                if (GoNoGo)
                    GoNoGo = false;
            }
            else
            {
                if(resultCheck[0] == 'O')
                    ledFlightDate.Fill = new SolidColorBrush(Colors.LightGreen);
            }

            //Verifica Aeroporto di Partenza
            if (resultCheck[1] == 'X')
            {
                ledAeroportoPartenza.Fill = new SolidColorBrush(Colors.Red);
                if (GoNoGo)
                    GoNoGo = false;
            }
            else
            {
                if (resultCheck[1] == 'O')
                    ledAeroportoPartenza.Fill = new SolidColorBrush(Colors.LightGreen); 
            }

            //Verifica Numero Volo
            if (resultCheck[2] == 'X')
            {
                ledFlightNumb.Fill = new SolidColorBrush(Colors.Red);
                if (GoNoGo)
                    GoNoGo = false;
            }
            else
            {
                if (resultCheck[2] == 'O')
                    ledFlightNumb.Fill = new SolidColorBrush(Colors.LightGreen);
            }

            //Verifica Stato Volo
            if (resultCheck[3] == 'X')
            {
                ledFlightStatus.Fill = new SolidColorBrush(Colors.Red);
                if (GoNoGo)
                    GoNoGo = false;
            }
            else
            {
                if (resultCheck[3] == 'X')
                    ledFlightStatus.Fill = new SolidColorBrush(Colors.LightGreen);
            }

            //Verifica Orario Volo
            if (resultCheck[4] == 'X')
            {
                ledFlightTime.Fill = new SolidColorBrush(Colors.Red);
                if (GoNoGo)
                    GoNoGo = false;
            }
            else
            {
                if (resultCheck[4] == 'X')
                    ledFlightTime.Fill = new SolidColorBrush(Colors.LightGreen);
            }
            
            //Verifica Fast Track
            if (resultCheck[5] == 'X')
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri("pack://siteoforigin:,,,/Resources/FT_KO.png");
                img.EndInit();
                imgFastTrack.Source = img;
                if (GoNoGo)
                    GoNoGo = false;
            }
            else
            {
                if (resultCheck[5] == 'O')
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri("pack://siteoforigin:,,,/Resources/FT_OK.png");
                    img.EndInit();
                    imgFastTrack.Source = img;
                }
            }

           

            if (GoNoGo)
            {
                bnManualEnter.Background = new SolidColorBrush(Color.FromRgb(46,207,119));
                bnManualEnter.Foreground = new SolidColorBrush(Color.FromRgb(49, 106, 162));
                bnManualEnter.Content = "AVANTI";
                NumIngressi++;



                mediaElementObj.Source = this.SoundOk;
                mediaElementObj.Position = new TimeSpan(0, 0, 0, 0, 0);
                mediaElementObj.Play();

            }
            else
            {
                bnManualEnter.Background = new SolidColorBrush(Color.FromRgb(216, 20, 29));
                bnManualEnter.Foreground = new SolidColorBrush(Color.FromRgb(233,243, 253));
                bnManualEnter.Content = "STOP";
                NumRespinti++;



                mediaElementObj.Source = this.SoundKO;
                mediaElementObj.Position = new TimeSpan(0, 0, 0, 0, 0);
                mediaElementObj.Play();
            }

            if (StatoVolo != string.Empty)
            {
                TBStatoVolo.Text = StatoVolo;
            }

            if(OraPrevistaVolo!=string.Empty)
            {
                TBOrarioPrevisto.Text = OraPrevistaVolo.Substring(0, 2) + ":" + OraPrevistaVolo.Substring(2,2);
            }

            if (OraEffettivaVolo != string.Empty)
            {
                TBOrarioEffettivo.Text = OraEffettivaVolo.Substring(0, 2) + ":" + OraEffettivaVolo.Substring(2,2);
            }
            UpDateStats();

        }

        private void UpDateStats()
        {

            tbNumIngressi.Text = NumIngressi.ToString();
            tbNumRespinti.Text = NumRespinti.ToString();
        }

     
        

        //private void dummyBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
           
        //}

        void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
           
            TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
            if (elapsed.TotalMilliseconds > 100)
            {
                _barcode.Clear();
                BarCodeTimer.IsEnabled = true;
                
            }
            BarCodeTimer.Stop();
            BarCodeTimer.Start();
            
        }

        private void MainWindow1_KeyDown_1(object sender, KeyEventArgs e)
        {
            this.tLed.Background = new SolidColorBrush(Colors.Yellow);
            this.tLed.Foreground = new SolidColorBrush(Colors.Black);
            this.tLed.Text = "IN LETTURA";
            LedTimer.IsEnabled = true;
            LedTimer.Stop();
            LedTimer.Start();
            ResetBoardingData();
        }

        private void bnManualEnter_Click(object sender, RoutedEventArgs e)
        {
            mediaElementObj.Source = this.SoundOk;
            mediaElementObj.Position = new TimeSpan(0, 0, 0, 0, 0);
            mediaElementObj.Play();
                     

            bnManualEnter.Background = new SolidColorBrush(Color.FromRgb(46, 207, 119));
            bnManualEnter.Foreground = new SolidColorBrush(Color.FromRgb(49, 106, 162));
            bnManualEnter.Content = "AVANTI (Manuale)";

            NumRespinti--;
            NumIngressi++;
            if (LastBpD != null)
            {
                string statoVolo, OraPrevista, OraEffettiva;
                Exception exResult = null;
                BPHelper.CheckBoardingPass(this.easyGateDBConnStr, iBoardingPass.Properties.Settings.Default.CodAeroporto, LastBpD, this.codicePostazione, FinestraOreaccettazione, iBoardingPass.Properties.Settings.Default.FastTrackAlarm, out statoVolo, out OraPrevista, out OraEffettiva,out exResult, true);
                UpDateStats();
                LastBpD = null;

            }


           

            ResetBoardingData();
        }

        void mediaElementObj_BufferingEnded(object sender, RoutedEventArgs e)
        {
            mediaElementObj.Close();
        }

        private void bnManualEnter_MouseEnter(object sender, MouseEventArgs e)
        {
            bnManualEnter.Background = new SolidColorBrush(Color.FromRgb(46, 207, 119));
            bnManualEnter.Foreground = new SolidColorBrush(Color.FromRgb(49, 106, 162));
            bnManualEnter.Content = "AVANTI (Manuale)";
        }

        private void bnManualEnter_MouseLeave(object sender, MouseEventArgs e)
        {
            bnManualEnter.Background = new SolidColorBrush(Color.FromRgb(205, 205, 205));
            bnManualEnter.Content = string.Empty;
        }

        private void bnAzzeraStat_Click(object sender, RoutedEventArgs e)
        {
            NumLetture = 0;
            NumIngressi = 0;
            NumRespinti = 0;
            StartStat = DateTime.Now;

            tbLetture.Text = NumLetture.ToString();
            tbNumIngressi.Text = NumIngressi.ToString();
            tbNumRespinti.Text = NumRespinti.ToString();
            this.tbMinuti.Text ="0";
        }

        private void MainWindow1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            eventLogBoardingPass.WriteEntry("STOP PROGRAM", EventLogEntryType.Information);
        }

      
    }
}
