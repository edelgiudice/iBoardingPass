using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Collections;
using System.Data.Linq;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace AggiornaStatoVoli
{
    public class AggVoliMng
    {
        private static int POLLING_INTERVAL =60*1000; // Intervallo di ricerca tra
        private static Timer PollingTimer;
        private static System.Diagnostics.EventLog _eventLogAggVoli;
        private static UInt64 prevFileHashCode;
        public static void Start(ref System.Diagnostics.EventLog eventLogAggVoli)
        {
            _eventLogAggVoli = eventLogAggVoli;
            Console.WriteLine("Servizio Started");
            DateTime LastTime = DateTime.Now;
            PollingTimer = new Timer(new TimerCallback(PollingTimerHandler));
            PollingTimer.Change(0, POLLING_INTERVAL);
            
            Console.ReadLine();
        }
        public static void Start(object o)
        {
            System.Diagnostics.EventLog eventLogAggVoli = (System.Diagnostics.EventLog)o;
            Start(ref eventLogAggVoli);
        }
        private static void PollingTimerHandler(object sender)
        {
            try
            {
                UInt64 ActualHashVal = 0;

                Uri ftpUri = new Uri(AggiornaStatoVoli.Properties.Settings.Default.FtpServerUri +@"/"+ AggiornaStatoVoli.Properties.Settings.Default.FtpFolder+ @"/" +
                           AggiornaStatoVoli.Properties.Settings.Default.InfoVoliFileName);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUri);
                request.Timeout = POLLING_INTERVAL/2;
                request.UseBinary = false;
                request.Method = WebRequestMethods.Ftp.DownloadFile;
               
                request.Credentials = new NetworkCredential(AggiornaStatoVoli.Properties.Settings.Default.FtpUser, AggiornaStatoVoli.Properties.Settings.Default.FtpPassword);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string ActualHashStr;
                string infoVolo = reader.ReadLine();
                ActualHashStr = infoVolo;
                while (infoVolo != null)
                {
                    if (infoVolo != string.Empty)
                    {
                        ParseInfoVoli(infoVolo);
                    }
                    infoVolo = reader.ReadLine();
                    ActualHashStr += infoVolo;
                }
                ActualHashVal = CalculateHash(ActualHashStr);
                if (ActualHashVal != prevFileHashCode)
                {
                    try
                    {
                        DataContext dc = new DataContext(AggiornaStatoVoli.Properties.Settings.Default.DbConnectionString);
                        DateTime Now = DateTime.Now;
                        AggiornamentoInfoVoli aiv = new AggiornamentoInfoVoli() { RilevamentoAggiornamento = Now, DataAggiornamento = Now.ToString("yyyyMMdd"), OraAggiornamento = Now.ToString("hhmm") };
                        AggiornamentoInfoVoli.InsertAggiornamentoInfoVoli(dc, aiv);
                        dc.SubmitChanges();
                    }
                    catch (System.Exception ex)
                    {

                        _eventLogAggVoli.WriteEntry(ex.ToString(), EventLogEntryType.Error);

                    }
                    prevFileHashCode = ActualHashVal;
                }
                reader.Close();
                response.Close();
                _eventLogAggVoli.WriteEntry("Aggiornamento FtpStatusCode InfoVoli", EventLogEntryType.Information);
            }
            catch (System.Exception ex)
            {

                _eventLogAggVoli.WriteEntry(ex.ToString(), EventLogEntryType.Error);
                
            }
            
        }


        private static UInt64 CalculateHash(string read)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
        private static void ParseInfoVoli(string ParseInfoVoli)
        {
            string[] Tokens = ParseInfoVoli.Split(new char[] { ';' });

           //string Partenza Arrivo
            string FlagPartenzaArrivo = Tokens[1].Substring(0,1);
           
            if (FlagPartenzaArrivo == "P") // Partenza
            {
                DataContext dc = new DataContext(AggiornaStatoVoli.Properties.Settings.Default.DbConnectionString);
                string NumVolo = Tokens[2];
                string DataVolo = Tokens[8];
                string CodCompagnia = Tokens[2].Substring(0, 3).Trim();
                string DescrizioneCompagnia = Tokens[3];
                string CodiceDestinazione = Tokens[4];
                string DescrizioneDestinazione = Tokens[5];
                string OraPrevista = Tokens[9];
                string OraEffettiva = Tokens[10];
                string CodiceStatoVolo = Tokens[0];
                string DescStatoVolo = Tokens[13];

                InfoVoli UpdatedInfoVolo = new InfoVoli();
                UpdatedInfoVolo.NumVolo = NumVolo;
                UpdatedInfoVolo.DataVolo = DataVolo;
                UpdatedInfoVolo.CodCompagnia = CodCompagnia;
                UpdatedInfoVolo.DescrizioneCompagnia = DescrizioneCompagnia;
                UpdatedInfoVolo.CodiceDestinazione = CodiceDestinazione;
                UpdatedInfoVolo.DescrizioneDestinazione = DescrizioneDestinazione;
                UpdatedInfoVolo.OraPrevista = OraPrevista;
                UpdatedInfoVolo.OraEffettiva = OraEffettiva;
                UpdatedInfoVolo.CodStatoVolo = CodiceStatoVolo;
                UpdatedInfoVolo.DescStatoVolo = DescStatoVolo;

                //Aggiornamento del DB              
                InfoVoli.InsertorUpdate(dc, UpdatedInfoVolo);
              
                dc.SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
            else
            {
                //Skip line
                return;

            }
            


        }


        [Table(Name="InfoVoli")]
        public class InfoVoli
        {
            [Column(IsPrimaryKey=true)]
            public string NumVolo{get;set;}

            [Column(IsPrimaryKey=true)]
            public string DataVolo { get; set; }

            [Column(CanBeNull=false)]
            public string CodCompagnia { get; set; }

            [Column(CanBeNull = true)]
            public string DescrizioneCompagnia { get; set; }

            [Column(CanBeNull = false)]
            public string CodiceDestinazione { get; set; }

            [Column(CanBeNull = true)]
            public string DescrizioneDestinazione { get; set; }

            [Column(CanBeNull = false)]
            public string OraPrevista { get; set; }

            [Column(CanBeNull = false)]
            public string OraEffettiva { get; set; }

            [Column(CanBeNull = false)]
            public string CodStatoVolo { get; set; }

            [Column(CanBeNull = true)]
            public string DescStatoVolo { get; set; }


            public static Table<InfoVoli> GetInfoVoli(DataContext dx)
            {
                return dx.GetTable<InfoVoli>();
            }

            public static bool DeleteInfoVoli(DataContext dx, InfoVoli data2Delete)
            {
                var element = dx.GetTable<InfoVoli>()
                    .Where(w => w.NumVolo == data2Delete.NumVolo)
                    .Where(w => w.DataVolo == data2Delete.DataVolo)
                    .SingleOrDefault<InfoVoli>();

                if (element != null)
                {
                    dx.GetTable<InfoVoli>().DeleteOnSubmit(element);
                    dx.SubmitChanges();
                    return true;
                }
                return false;
            }

            public static bool InsertInfoVoli(DataContext dx, InfoVoli data2Insert)
            {
                try
                {
                    dx.GetTable<InfoVoli>().InsertOnSubmit(data2Insert);
                    dx.SubmitChanges();
                    return true;
                }
                catch(System.Exception ex)
                {
                    return false;
                }
            }

            public static void InsertorUpdate(DataContext dx, InfoVoli data2Insert)
            {
                var element = dx.GetTable<InfoVoli>()
                   .Where(w => w.NumVolo == data2Insert.NumVolo)
                   .Where(w => w.DataVolo == data2Insert.DataVolo)
                   .SingleOrDefault<InfoVoli>();

                if (element == null)
                {
                    dx.GetTable<InfoVoli>().InsertOnSubmit(data2Insert);

                }
                else
                {

                    element.CodCompagnia = data2Insert.CodCompagnia;
                    element.DescrizioneCompagnia = data2Insert.DescrizioneCompagnia;
                    element.CodiceDestinazione = data2Insert.CodiceDestinazione;
                    element.DescrizioneDestinazione = data2Insert.DescrizioneDestinazione;
                    element.OraPrevista = data2Insert.OraPrevista;
                    element.OraEffettiva = data2Insert.OraEffettiva;
                    element.CodStatoVolo = data2Insert.CodStatoVolo;
                    element.DescStatoVolo = data2Insert.DescStatoVolo;
                }
                
            }

        }

        [Table(Name = "AggiornamentoInfoVoli")]
        public class AggiornamentoInfoVoli
        {
            [Column(IsPrimaryKey = true)]
            public DateTime RilevamentoAggiornamento { get; set; }  
            [Column(CanBeNull = false)]
            public string DataAggiornamento { get; set; }
            [Column(CanBeNull = false)]
            public string OraAggiornamento { get; set; }

            public static void InsertAggiornamentoInfoVoli(DataContext dx, AggiornamentoInfoVoli elmnt)
            {
                dx.GetTable<AggiornamentoInfoVoli>().InsertOnSubmit(elmnt);
            }
        }
    }
}
