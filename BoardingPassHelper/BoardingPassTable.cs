using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Collections;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Collections;
using System.Data.Linq;


namespace iBoardingPass
{
    [Table(Name = "BoardingPass")]
    public class BoardingPassTable
    {

        public BoardingPassTable()
        {
        }
        public BoardingPassTable(BoardingPassData bpD)
        {
            NomePasseggero = bpD.PassengerName;
            NumVolo = bpD.FlightNumber;
            DataVolo = BPHelper.GetDateFromJulian(Convert.ToInt32(bpD.DateOfFlight)).ToString("yyyyMMdd");
            CodiceCompagnia = bpD.OperatingCarrier;
            PartenzaDa = bpD.FromCity;
            ArrivoA = bpD.ToCity;
            FastTrackFlag = Convert.ToInt16(bpD.FastTrackFlag);
        }

        [Column(IsPrimaryKey = true)]
        public string NomePasseggero { get; set; }

        [Column(IsPrimaryKey = true)]
        public string NumVolo { get; set; }

        [Column(IsPrimaryKey = true)]
        public string DataVolo { get; set; }

        [Column(IsPrimaryKey = true)]
        public string CodiceCompagnia { get; set; }

        [Column(CanBeNull = false)]
        public string PartenzaDa { get; set; }

        [Column(CanBeNull = false)]
        public string ArrivoA { get; set; }

        [Column(CanBeNull = false)]
        public Int16 FastTrackFlag { get; set; }

    }
}
