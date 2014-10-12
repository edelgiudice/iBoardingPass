﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using iBoardingPass;

namespace BoardingPassTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GetCurrentJuliandate()
        {
            int day=BPHelper.GetCurrentJulianDate();
            int day2=BPHelper.GetJulianDate(new DateTime(2013,8,14));
        }
        [TestMethod]
        public void ParseBoardingPass()
        {
            string BPCode = @"M1DEL GIUDICE/RICACRDO S8B8NR CUFCAGFR 4816 283Y000 77   10C           B@";
            BoardingPassData bpd = BoardingPassData.Parse(BPCode);
        }

        [TestMethod]
        public void CheckBoardingPass_01()
        {
            Exception exResult = null;
            string BPCode = @"M1DEL GIUDICE/RICACRDO S8B8NR CUFCAGFR 4816 283Y000 77   10C           B@";
            BoardingPassData bpd = BoardingPassData.Parse(BPCode);
            string connectionString = @"Data Source=192.168.0.47\EXPRESSR2;Initial Catalog=EasyGate;Persist Security Info=True;User ID=easyGate;Password=easyG2013";
            string statoVolo=string.Empty;
            string oraVoloEffettiva=string.Empty;
            string oraVoloPrevista=string.Empty;
            
            string result=BPHelper.CheckBoardingPass(
                connectionString
                ,"CAG"
                ,bpd
                ,0
                ,3
                ,false
                ,1
                , out statoVolo
                , out oraVoloPrevista
                ,out  oraVoloEffettiva
                ,false
                );
            Console.WriteLine(result);
                
        }

        [TestMethod]
        public void TestReportExcel()
        {
            DateTime FlightDate = new DateTime(2013, 10, 14);
            BoardingDailyReport bdr = new BoardingDailyReport(FlightDate, @"L:\temp\TestExcel", BoardingPassTests.Properties.Settings.Default.connectionString);

        }
    }
}
