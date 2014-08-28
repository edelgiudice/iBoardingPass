using System;
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
    }
}
