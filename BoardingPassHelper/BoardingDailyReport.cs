using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SpreadsheetLight;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;

namespace iBoardingPass
{
    public class BoardingDailyReport
    {
        private DateTime _reportDate;
        private string _reportFileName;
        private string _connectionString;
        private List<FlightCompany> FlightData;
        private int _openGateInterval = 180;
        private int _boardingInterval = 30;

        public BoardingDailyReport(DateTime dayReport, string reportFolder, string connectionString)
        {

            _connectionString = connectionString;
            _reportDate = dayReport;
            if(!reportFolder.EndsWith(@"\"))
            {
                reportFolder += @"\";
            }
            _reportFileName = reportFolder+string.Format("{0:0000}{1:00}{2:00}.xlsx", _reportDate.Year, _reportDate.Month, _reportDate.Day);

            FlightData = DbFlightData.GetFlightData(_connectionString, dayReport, 5);
            if ((FlightData == null) || (FlightData.Count()==0))
            {
                throw new ApplicationException("Non sono presenti dati nel DB per la data specificata");
            }
            List<StatisticData> GlobalStats;
            DbFlightData.QueryData(_connectionString, dayReport, FlightData, 5,out GlobalStats);

            using (SLDocument sl = new SLDocument())
            {
                sl.FreezePanes(4, 4);
                Write_DateHeader(sl, _reportDate);
                Write_LeftHeader(sl);
                GlobalDetail(sl, GlobalStats);
                if (FlightData != null)
                {
                    Write_DetailedFlightData(sl, FlightData, GlobalStats);
                }
                try
                {
                    sl.SaveAs(_reportFileName);
                }
                catch (SystemException ex)
                {
                    //TODO Log Exception on file log
                }
                
            }
        }



        private void Write_DetailedFlightData(SLDocument sl, List<FlightCompany> flightData, List<StatisticData> GlobalStats)
        {
            foreach (FlightCompany fc in flightData)
            {
                CompanyHeader(sl, fc);


               
                foreach (Flight flight in fc.Flights)
                {
                    SLStyle style = sl.CreateStyle();
                    FlightHeader(sl, flight);

                   

                    int startRow = 5;
                    int TotalPassenger=0;
                    foreach (StatisticData sd in flight.TimeStats)
                    {
                        double DeltaScheduled=0;
                        double DeltaDeparture=0;

                        style = sl.CreateStyle();
                        style.Font.FontName = "Calibri";
                        style.Font.FontSize = 11;
                        style.Font.FontColor = System.Drawing.Color.Black;
                        style.Font.Bold = false;
                        style.Font.Italic = false;
                        style.Font.Strike = false;

                        DeltaScheduled =flight.ScheduledTime.Subtract(sd.Time).TotalMinutes;
                        DeltaDeparture = flight.DepartureTime.Subtract(sd.Time).TotalMinutes;

                        bool grayZone = false;

                        if ((Math.Abs(DeltaScheduled) >= _openGateInterval) || (DeltaDeparture<0))
                        {
                            style.Fill.SetPatternType(PatternValues.Solid);
                            style.Fill.SetPatternForegroundColor(System.Drawing.Color.LightGray);
                            grayZone = true;
                        }
                        else
                        {
                            if ((DeltaScheduled < 0) && DeltaDeparture >= 0)
                            {
                                style.Fill.SetPatternType(PatternValues.Solid);
                                style.Fill.SetPatternForegroundColor(System.Drawing.Color.Red);
                            }
                            else
                            {
                                if ((DeltaScheduled >= 0) && (DeltaScheduled <= _boardingInterval))
                                {
                                    style.Fill.SetPatternType(PatternValues.Solid);
                                    style.Fill.SetPatternForegroundColor(System.Drawing.Color.Orange);
                                }
                                else
                                {
                                    style.Fill.SetPatternType(PatternValues.Solid);
                                    style.Fill.SetPatternForegroundColor(System.Drawing.Color.White);
                                }
                            }
                            
                        }

                        style.FormatCode = "#00";
                        sl.SetCellStyle(startRow, flight.StartColumn, startRow, flight.StartColumn, style);

                        var gS=GlobalStats.Where(w => w.Time == sd.Time).FirstOrDefault();
                        if (gS != null)
                        {
                            if ((gS.NumPassengers > 0) && (sd.NumPassengers>0))
                            {
                                sd.Percentage = ((float)sd.NumPassengers / (float)gS.NumPassengers);
                            }
                        }
                        TotalPassenger+=sd.NumPassengers;
                        if(!(grayZone && (sd.NumPassengers==0)))
                        {
                            sl.SetCellValue(startRow, flight.StartColumn, sd.NumPassengers);
                            sl.SetCellValue(startRow, flight.StartColumn + 1, sd.Percentage);

                        }
                        style.FormatCode = "#0.0%";
                        sl.SetCellStyle(startRow, flight.StartColumn + 1, startRow, flight.StartColumn + 1, style);
                        startRow++;
                    }
                    style = sl.CreateStyle();
                    style.Border.Outline = true;

                    style.Border.RightBorder.BorderStyle = BorderStyleValues.Medium;
                    
                    sl.SetCellStyle(5, flight.StartColumn+1 , startRow-1, flight.StartColumn + 1, style);
                    style.Border.RemoveAllBorders();
                    style.Border.LeftBorder.BorderStyle = BorderStyleValues.Medium;
                    sl.SetCellStyle(5, flight.StartColumn , startRow - 1, flight.StartColumn , style);

                    style.Fill.SetPatternType(PatternValues.Solid);
                    style.Fill.SetPatternForegroundColor(System.Drawing.Color.Yellow);
                    style.FormatCode = "##0";
                    style.Font.Bold = true;
                    sl.SetCellStyle(startRow , flight.StartColumn, startRow , flight.StartColumn, style);
                    sl.SetCellValue(startRow , flight.StartColumn, TotalPassenger);
                    
                }
            }
        }

        private static void GlobalDetail(SLDocument sl, List<StatisticData> GlobalStats)
        {
            SLStyle style = sl.CreateStyle();
            style = sl.CreateStyle();
            style.Font.FontName = "Calibri";
            style.Font.FontSize = 11;
            style.Font.FontColor = System.Drawing.Color.Black;
            style.Font.Bold = false;
            style.Font.Italic = false;
            style.Font.Strike = false;
            style.FormatCode = "##0";
            int startRow = 5;
            int TotalPassenger = 0;
            
            foreach (StatisticData sd in GlobalStats)
            {
                sl.SetCellStyle(startRow, 2, startRow, 2, style);
                sl.SetCellValue(startRow, 2, sd.NumPassengers);
                startRow++;
                TotalPassenger += sd.NumPassengers;
            }
            style.Fill.SetPatternType(PatternValues.Solid);
            style.Fill.SetPatternForegroundColor(System.Drawing.Color.Yellow);
            
            style.Font.Bold = true;
            sl.SetCellStyle(startRow, 2, startRow, 2, style);
            sl.SetCellValue(startRow, 2, TotalPassenger);

        }

        private static void FlightHeader(SLDocument sl, Flight flight)
        {
            int companyRangeCell;
            SLStyle style = sl.CreateStyle();
            style.Font.FontName = "Calibri";
            style.Font.FontSize = 11;
            style.Font.FontColor = System.Drawing.Color.Black;
            style.Font.Bold = true;
            style.Font.Italic = false;
            style.Font.Strike = false;

            style.Alignment.Horizontal = HorizontalAlignmentValues.Center;

            companyRangeCell = flight.StartColumn + 1;
            sl.MergeWorksheetCells(2, flight.StartColumn, 2, companyRangeCell);
            sl.MergeWorksheetCells(3, flight.StartColumn, 3, companyRangeCell);

            style.Border.LeftBorder.BorderStyle = BorderStyleValues.Thin;
            style.Border.RightBorder.BorderStyle = BorderStyleValues.Thin;
            style.Border.BottomBorder.BorderStyle = BorderStyleValues.Thin;
            style.Border.TopBorder.BorderStyle = BorderStyleValues.Thin;
            style.Border.Outline = true;
            sl.SetCellStyle(2, flight.StartColumn, 3, companyRangeCell, style);

            sl.SetCellValue(2, flight.StartColumn, flight.FlightNumber);
            sl.SetCellValue(3, flight.StartColumn, flight.FlightFinalStateDesc);

            sl.SetCellValue(4, flight.StartColumn, "# Pass"); sl.SetCellStyle(4, flight.StartColumn, 4, flight.StartColumn, style);
            sl.SetCellValue(4, flight.StartColumn + 1, "% Pass"); sl.SetCellStyle(4, flight.StartColumn + 1, 4, flight.StartColumn + 1, style);
        }

        private static void CompanyHeader(SLDocument sl, FlightCompany fc)
        {
            int companyRangeCell;
            SLStyle style = sl.CreateStyle();
            style.Font.FontName = "Calibri";
            style.Font.FontSize = 11;
            style.Font.FontColor = System.Drawing.Color.Black;
            style.Font.Bold = true;
            style.Font.Italic = false;
            style.Font.Strike = false;

            style.Alignment.Horizontal = HorizontalAlignmentValues.Center;

            companyRangeCell = fc.StartColumn + 2 * fc.Flights.Count - 1;
            sl.MergeWorksheetCells(1, fc.StartColumn, 1, companyRangeCell);
            style.Border.LeftBorder.BorderStyle = BorderStyleValues.Medium;
            style.Border.RightBorder.BorderStyle = BorderStyleValues.Medium;
            style.Border.BottomBorder.BorderStyle = BorderStyleValues.Medium;
            style.Border.TopBorder.BorderStyle = BorderStyleValues.Medium;
            style.Border.Outline = true;

            sl.SetCellStyle(1, fc.StartColumn, 1, companyRangeCell, style);

            sl.SetCellValue(1, fc.StartColumn, fc.FlightCompanyDesc);
        }

        private void Write_LeftHeader(SLDocument sl)
        {
            sl.SetColumnWidth("B", 11.5);

            SLStyle style = sl.CreateStyle();
            style.Font.FontName = "Calibri";
            style.Font.FontSize = 11;
            style.Font.FontColor = System.Drawing.Color.Black;
            style.Font.Bold = true;
            style.Font.Italic = false;
            style.Font.Strike = false;

           

            sl.SetCellValue("a4", "Orario");
            sl.SetCellValue("b4", "#Passeggeri"); 
            sl.SetCellValue("c4", "T.Medio Coda");
            sl.SetCellValue("d4", "Fast Track");

            style.Border.LeftBorder.BorderStyle = BorderStyleValues.Medium;
            style.Border.RightBorder.BorderStyle = BorderStyleValues.Medium;
            style.Border.BottomBorder.BorderStyle = BorderStyleValues.Medium;
            style.Border.TopBorder.BorderStyle = BorderStyleValues.Medium;
            

            style.SetWrapText(true);
            sl.SetCellStyle("a4", "d4", style);

            TimeSpan ts = TimeSpan.FromMinutes(5);
            int rowIndex=5;
            DateTime dt=DateTime.Today;
            do
            {
                string cellRef="A"+rowIndex.ToString();
                style = sl.CreateStyle();
                style.FormatCode = "hh:mm";
                sl.SetCellStyle(cellRef, style);
                sl.SetCellValue(cellRef, dt);
                dt=dt.Add(ts);
                rowIndex++;
            }while (dt<DateTime.Today.Add(TimeSpan.FromDays(1)));

        }

        private void Write_DateHeader(SLDocument sl, DateTime reportDate)
        {
           
            string text = reportDate.ToShortDateString();

            sl.MergeWorksheetCells("A1", "D3");
            sl.SetCellValue("A1", text);

            SLStyle style = sl.CreateStyle();
            style.Font.FontName = "Calibri";
            style.Font.FontSize = 16;
            style.Font.FontColor = System.Drawing.Color.Wheat;
            
            style.Font.Bold = true;
            style.Font.Italic = false;
            style.Font.Strike = false;

            style.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.Purple, System.Drawing.Color.Blue);

            style.Alignment.Horizontal = HorizontalAlignmentValues.Center;
            style.Alignment.Vertical = VerticalAlignmentValues.Center;   

            sl.SetCellStyle("A1", style);

        }
    }

    public class FlightCompany
    {
        public string FlightCompanyName { get; set; }
        public string FlightCompanyDesc { get; set; }
        public int StartColumn { get; set; }
        public List<Flight> Flights { get; set; }

        public FlightCompany(
            string flightCompany,
            string flightCompanyDesc,
            int startColumn
            )
        {
            FlightCompanyName = flightCompany;
            FlightCompanyDesc = flightCompanyDesc;
            StartColumn = startColumn;
            Flights = new List<Flight>();
        }

        public void AddFlight(
                string flightNumber,
                DateTime scheduledTime,
                DateTime departureTime,
                string flightFinalState,
                string flightFinalStateDesc,
                int interval,
                DateTime refDate
            )
        {
            int flightsCount = Flights.Count;
            int flightStartPos = StartColumn + flightsCount * 2;
            Flight newFlight = new Flight(
                    flightNumber,
                    flightStartPos,
                    scheduledTime,
                    departureTime,
                    flightFinalState,
                    flightFinalStateDesc,
                    interval,
                    refDate
                );
            Flights.Add(newFlight);
        }
    }

    public class Flight
    {
        public string FlightNumber { get; set; }
        public int StartColumn { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public string FlightFinalState { get; set; }
        public string FlightFinalStateDesc { get; set; }
        public List<StatisticData> TimeStats { get; set; }

        public Flight(
            string flightNumber,
            int startColumn,
            DateTime scheduledTime,
            DateTime departureTime,
            string flightFinalState,
            string flightFinalStateDesc,
            int interval,
            DateTime refDate)
        {
            TimeStats = new List<StatisticData>();
            FlightNumber = flightNumber;
            StartColumn=startColumn;
            ScheduledTime = scheduledTime;
            DepartureTime = departureTime;
            FlightFinalState = flightFinalState;
            FlightFinalStateDesc = flightFinalStateDesc;
            DateTime S = refDate;
            for (int i = 0; i < (24f * 60f) ; i+=interval)
            {
                StatisticData sd = new StatisticData();
                sd.Time = S.AddMinutes(i);
                sd.NumPassengers = 0;
                sd.Percentage = 0;
                sd.NumFastTrack = 0;
                TimeStats.Add(sd);
            }

        }
    }

    public class StatisticData
    {
        public DateTime Time { get; set; }
        public int NumPassengers { get; set; }
        public float Percentage { get; set; }
        public int NumFastTrack { get; set; }  // Not used

    }



    public class DbFlightData
    {
        public static List<FlightCompany> GetFlightData(string connectionString, DateTime RefDate, int startColumn)
        {
            List<FlightCompany> flightData = new List<FlightCompany>();
            string extractionDataQuery = @"
                    
                    declare @datavolo as varchar(8)

                    select 
	                    CodCompagnia
	                    ,DescrizioneCompagnia
	                    ,NumVolo
	                    ,OraPrevista
	                    ,OraEffettiva
	                    ,CodStatoVolo
	                    ,DescStatoVolo
                    from InfoVoli where DataVolo={0}
                    order by DescrizioneCompagnia,NumVolo
                ";
            DataContext dc = new DataContext(connectionString);
            string refDataStr = RefDate.ToString("yyyyMMdd");
            var FlightList = dc.ExecuteQuery <DbFlightData>( extractionDataQuery, new object[] { refDataStr });
            if (FlightList!=null )
            {
                string CurrentFlightCompany = string.Empty;
                int StartColumn=startColumn;
                
                FlightCompany currentFlightCompany=null;
                foreach (DbFlightData dbFD in FlightList)
                {
                    if (dbFD.CodCompagnia != CurrentFlightCompany)
                    {
                        FlightCompany fc = new FlightCompany(dbFD.CodCompagnia, dbFD.DescrizioneCompagnia, StartColumn);
                        currentFlightCompany = fc;
                        CurrentFlightCompany = dbFD.CodCompagnia;
                        flightData.Add(fc);
                    }
                    currentFlightCompany.AddFlight(
                            dbFD.NumVolo
                            , RefDate.Add(new TimeSpan(int.Parse(dbFD.OraPrevista.Substring(0, 2))
                                                     , int.Parse(dbFD.OraPrevista.Substring(2, 2)),0))
                            , RefDate.Add(new TimeSpan(int.Parse(dbFD.OraEffettiva.Substring(0, 2))
                                                     , int.Parse(dbFD.OraEffettiva.Substring(2, 2)),0))
                            , dbFD.CodStatoVolo
                            , dbFD.DescStatoVolo
                            ,5
                            ,RefDate
                        );
                    StartColumn +=  2;
                }
            }
            

            return flightData;
        }

        [Column] 
        public string CodCompagnia { get; set; }
        [Column]
        public string DescrizioneCompagnia { get; set; }
        [Column] 
        public string NumVolo { get; set; }
        [Column]
        public string OraPrevista { get; set; }
        [Column]
        public string OraEffettiva { get; set; }
        [Column]
        public string CodStatoVolo { get; set; }
        [Column]
        public string DescStatoVolo { get; set; }

        internal static void QueryData(string _connectionString, DateTime dayReport, List<FlightCompany> FlightData, int interval, out  List<StatisticData> GlobalStats)
        {
            GlobalStats = new List<StatisticData>();
            string extractionDataQuery = @"
                    
                    select
	                    CodiceCompagnia
	                    ,NumVolo
	                    ,COUNT(*) nPassage
                    from [EasyGate].[dbo].[LetturaBP]
                    where 
	                    DataLettura={0}
	                    and OraLettura>={1}
	                    and OraLettura< {2}
                    group by CodiceCompagnia
	                    ,NumVolo
                ";
            DataContext dc = new DataContext(_connectionString);
            
            string dataStr = dayReport.ToString("yyyyMMdd");

            for (int i = 0; i < (24f * 60f); i += interval)
            {
                int totalPassenger = 0;
                DateTime T = dayReport.AddMinutes(i);
                StatisticData sd = new StatisticData();
                sd.Time =T;
                
                GlobalStats.Add(sd);

                string startTime = T.ToString("HHmm");
                string endTime = T.AddMinutes(interval).ToString("HHmm");
                var baordingDetailList = dc.ExecuteQuery<boardingPassData>(extractionDataQuery, new object[] { dataStr, startTime, endTime });
                if (baordingDetailList != null)
                {
                    foreach (boardingPassData bpd in baordingDetailList)
                    {
                        FlightCompany fc = FlightData.Where(w => w.FlightCompanyName == bpd.CodiceCompagnia).FirstOrDefault();
                        if (fc != null)
                        {
                            var flight = fc.Flights.Where(w => w.FlightNumber == fc.FlightCompanyName+" "+bpd.NumVolo).FirstOrDefault();
                            if (flight != null)
                            {
                                
                               
                                var statisticData = flight.TimeStats.Where(w => w.Time == T).FirstOrDefault();
                                statisticData.NumPassengers = bpd.nPassage;
                                totalPassenger += bpd.nPassage;
                            }
                        }
                            
                    }
                }
                sd.NumPassengers = totalPassenger;
            }
           

        }

        public class boardingPassData
        {
            [Column]
            public string CodiceCompagnia { get; set; }
            [Column]
            public string NumVolo { get; set; }
            [Column]
            public int nPassage { get; set; }
        }
    }
}
