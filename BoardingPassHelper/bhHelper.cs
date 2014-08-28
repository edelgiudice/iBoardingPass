using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;

namespace iBoardingPass
{
    public class BPHelper
    {
        public static int GetCurrentJulianDate()
        {
            DateTime Now = DateTime.Now;
            return GetJulianDate(Now);
        }

        public static int GetJulianDate(DateTime Date2Convert)
        {
            int YYYY = Date2Convert.Year;

            DateTime FirstDay = new DateTime(YYYY, 1, 1);

            int DeltaDay = Date2Convert.Subtract(FirstDay).Days+1;

            return DeltaDay;
        }

        public static DateTime GetDateFromJulian(int JulianD)
        {
            DateTime FirstDay = new DateTime(DateTime.Now.Year, 1, 1);
            JulianD--;
            DateTime ReturnDate=FirstDay.Add(TimeSpan.FromDays(JulianD));

            return ReturnDate;
        }

        public static bool InsertUpdateBPonDB(string connectionString, BoardingPassData BPObj)
        {
            DataContext dx = new DataContext(connectionString);
            string convertedDate = GetDateFromJulian(Convert.ToInt32(BPObj.DateOfFlight)).ToString("yyyyMMdd");
            var element = dx.GetTable<BoardingPassTable>()
                    .Where(w => w.NomePasseggero == BPObj.PassengerName)
                    .Where(w => w.NumVolo == BPObj.FlightNumber)
                    .Where(w => w.DataVolo == convertedDate)
                    .Where(w => w.CodiceCompagnia == BPObj.OperatingCarrier)
                    .FirstOrDefault();
            if (element == null)
            {
               
                BoardingPassTable newBoardingPass = new BoardingPassTable(BPObj);
                Table<BoardingPassTable> T = dx.GetTable<BoardingPassTable>();
                T.InsertOnSubmit(newBoardingPass);
                dx.SubmitChanges();
                
                return true;
            }
            else
            {
                // Il dato è già presente NOP su questa tabella
                return false;
            }

        }

        public static Single DetaPartenza(string dataPrevista,string OraPrevista)
        {
            try
            {
                DateTime OrarioSchedulato = new DateTime(
                    Convert.ToInt32(dataPrevista.Substring(0, 4)),
                    Convert.ToInt32(dataPrevista.Substring(4, 2)),
                    Convert.ToInt32(dataPrevista.Substring(6, 2))).AddHours(Convert.ToInt32(OraPrevista.Substring(0, 2))).AddMinutes(Convert.ToInt32(OraPrevista.Substring(2, 2)));

                TimeSpan Delta = DateTime.Now.Subtract(OrarioSchedulato);

                return (Single)Delta.TotalMinutes;
            }
            catch
            {
                return -9999;
            }
        }

        public static bool InsertUpdateLetturaBOonDB(string connectionString, BoardingPassData BPObj, int Postazione, string OraPartenza, string statusString, bool Force=false)
        {
            DataContext dx = new DataContext(connectionString);
            string convertedDate = GetDateFromJulian(Convert.ToInt32(BPObj.DateOfFlight)).ToString("yyyyMMdd");
            LetturaBPTable element = dx.GetTable<LetturaBPTable>()
                   .Where(w => w.NomePasseggero == BPObj.PassengerName)
                   .Where(w => w.NumVolo == BPObj.FlightNumber)
                   .Where(w => w.DataVolo == convertedDate)
                   .Where(w => w.CodiceCompagnia == BPObj.OperatingCarrier)
                   .Where(w=> w.PostazioneLettura==Postazione)
                   .FirstOrDefault();

            if (element == null)
            {
                LetturaBPTable newLetturaBP = new LetturaBPTable(BPObj, Postazione, statusString, convertedDate, OraPartenza,Force);
                Table<LetturaBPTable> T = dx.GetTable<LetturaBPTable>();
                T.InsertOnSubmit(newLetturaBP);
                dx.SubmitChanges();
                return true;
            }
            else
            {
                element.Update(statusString, convertedDate, OraPartenza, Force);
                dx.SubmitChanges();
                return false;
            }
         
        }
        public static string CheckBoardingPass(string connectionString, string CodAeroportoPartenza, BoardingPassData BPObj, int Postazione, int FinestraOreaccettazione, bool checkFastTrack, int runnigMode, out string StatoVolo, out string OraPrevistaVolo, out string OraEffettivaVolo, bool Force = false)
        {
            StatoVolo = string.Empty;
            OraPrevistaVolo = string.Empty;
            OraEffettivaVolo = string.Empty;

            DateTime OraPassaggio = DateTime.Now;
            char[] resultArray = new char[]{
                ' ', //Data errata
                ' ', //Aeroporto di partenza
                ' ', //Volo non trovato
                ' ', //Stato Volo
                ' ', //Orario Volo
                ' '  //Fast Track
            };
            //string OraEffettivaVolo = "";

            //Verfica della data
            DateTime Now = DateTime.Now;
            DateTime FlightDate = BPHelper.GetDateFromJulian(Convert.ToInt32(BPObj.DateOfFlight));
            if (!(
                (Now.Year == FlightDate.Year) &&
                (Now.Month == FlightDate.Month) &&
                (Now.Day == FlightDate.Day)))
            {
                resultArray[0] = 'X';
            }
            else
            {
                resultArray[0] = 'O';
            }

            //Verifica Aeroporto di Partenza
            if (BPObj.FromCity != CodAeroportoPartenza)
            {
                resultArray[1] = 'X';
            }
            else
            {
                resultArray[1] = 'O';
            }

            if (runnigMode == 0)
                return new string(resultArray); 

            DataContext dx = new DataContext(connectionString);
            InfoVoliTable VoloSchedulato = dx.GetTable<InfoVoliTable>()
                .Where(w => w.NumVolo == BPObj.OperatingCarrier + " " + BPObj.FlightNumber)
                .Where(w => w.DataVolo == FlightDate.ToString("yyyyMMdd"))
                .FirstOrDefault();

            //InfoVoliTable VoloSchedulato = dx.GetTable<InfoVoliTable>()
            //    .Where(w => w.NumVolo == "AP 05497")
            //    .Where(w => w.DataVolo == "20131023")
            //    .FirstOrDefault();

            if (VoloSchedulato == null)
            {
                resultArray[2] = 'X';
            }
            else
            {
                StatoVolo = VoloSchedulato.DescStatoVolo;
                OraPrevistaVolo = VoloSchedulato.OraPrevista;
                OraEffettivaVolo = VoloSchedulato.OraEffettiva;
                resultArray[2] = 'O';

                switch (VoloSchedulato.CodStatoVolo)
                {
                    case "E": resultArray[3] = 'X'; break;  // Volo Cancellato
                    case "4": resultArray[3] = 'X'; break;  // Volo Decollato
                    case "3": resultArray[3] = 'X'; break;  // Volo Chiuso
                    default: resultArray[3] = 'O'; break;
                }



                DateTime LimiteInferiore = DateTime.Today.AddHours(Convert.ToInt32(VoloSchedulato.OraEffettiva.Substring(0, 2)) - FinestraOreaccettazione).AddMinutes(Convert.ToInt32(VoloSchedulato.OraEffettiva.Substring(2, 2)));
                DateTime LimiteSuperiore = DateTime.Today.AddHours(Convert.ToInt32(VoloSchedulato.OraEffettiva.Substring(0, 2))).AddMinutes(Convert.ToInt32(VoloSchedulato.OraEffettiva.Substring(2, 2)));
                if (OraPassaggio.CompareTo(LimiteSuperiore) > 0)
                {
                    //Il volo è già partito
                    resultArray[4] = 'X';
                }
                else
                {
                    if (OraPassaggio.CompareTo(LimiteInferiore) < 0)
                    {
                        //Il volo parte tra oltre 3 ore
                        resultArray[4] = 'X';
                    }
                    else
                    {
                        resultArray[4] = 'O';
                    }
                }
                if (checkFastTrack && !BPObj.FastTrackFlag)
                {
                    // Il biglietto non è FastTrack
                    resultArray[5] = 'X';
                }
                else
                {
                    if (checkFastTrack)
                        resultArray[5] = 'O';
                }



            }
            InsertUpdateBPonDB(connectionString, BPObj);
            InsertUpdateLetturaBOonDB(connectionString, BPObj, Postazione, OraEffettivaVolo, new string(resultArray), Force);


            return new string(resultArray);
        }
        public static string CheckBoardingPass(string connectionString, string CodAeroportoPartenza,BoardingPassData BPObj, int Postazione,int FinestraOreaccettazione, bool checkFastTrack,  out string StatoVolo, out string OraPrevistaVolo,out string OraEffettivaVolo, bool Force=false)
        {
            return CheckBoardingPass(connectionString,
                CodAeroportoPartenza,
                BPObj,
                Postazione,
                FinestraOreaccettazione,
                checkFastTrack,
                0,
                out StatoVolo,
                out OraPrevistaVolo,
                out OraEffettivaVolo,
                Force
                );
        }
    }

    public class BoardingPassData
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
                (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string FormatCode { get; set; }
        public string NumberLegs { get; set; }
        public string PassengerName { get; set; }
        public string ElectronicTicketFlag { get; set; }
        public string PRNCode { get; set; }
        public string FromCity { get; set; }
        public string ToCity { get; set; }
        public string OperatingCarrier { get; set; }
        public string FlightNumber { get; set; }
        public string DateOfFlight { get; set; }
        public string CompartmentCode { get; set; }
        public string SeatNumber { get; set; }
        public string CheckInSeq { get; set; }
        public bool FastTrackFlag { get; set; }
        public static BoardingPassData Parse(string BPCode)
        {
            BoardingPassData bpd= new BoardingPassData();

            try
            {
                bpd.FormatCode = BPCode.Substring(0, 1).Trim();
                bpd.NumberLegs = BPCode.Substring(1, 1).Trim();
                bpd.PassengerName = BPCode.Substring(2, 20).Trim();
                bpd.ElectronicTicketFlag = BPCode.Substring(22, 1).Trim();
                bpd.PRNCode = BPCode.Substring(23, 7).Trim();
                bpd.FromCity = BPCode.Substring(30, 3).Trim();
                bpd.ToCity = BPCode.Substring(33, 3).Trim();
                bpd.OperatingCarrier = BPCode.Substring(36, 3).Trim();
                bpd.FlightNumber = BPCode.Substring(39, 5).Trim();
                bpd.DateOfFlight = BPCode.Substring(44, 3).Trim();
                bpd.CompartmentCode = BPCode.Substring(47, 1).Trim();
                bpd.SeatNumber = BPCode.Substring(48, 4).Trim();
                bpd.CheckInSeq = BPCode.Substring(52, 5).Trim();

                bpd.FastTrackFlag = false;
            }
            catch (SystemException ex)
            {
                Log.Warn("Errore nella lettura della carta di imbarco " + BPCode);
                Log.Debug("Errore nella lettura della carta di imbarco " + ex.ToString());
                return null;
            }

            return bpd;
        }
    }
}
