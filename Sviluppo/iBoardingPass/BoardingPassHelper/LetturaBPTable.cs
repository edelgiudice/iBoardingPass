using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Collections;
using System.Data.Linq;

namespace iBoardingPass
{
    [Table(Name = "LetturaBP")]
    public class LetturaBPTable
    {
        public LetturaBPTable()
        {
            ForzatoManualmente = 0;
        }
        public LetturaBPTable(BoardingPassData bpD, int Postazione, string statusString,string DataVolo, string OraPartenza, bool Forzato=false)
        {
            NomePasseggero = bpD.PassengerName;
            NumVolo = bpD.FlightNumber;
            this.DataVolo = DataVolo;
            CodiceCompagnia = bpD.OperatingCarrier;
            PostazioneLettura = Postazione;
            OraLettura = DateTime.Now.ToString("HHmm");
            DataLettura = DateTime.Now.ToString("yyyyMMdd");
            if (statusString != string.Empty)
            {
                Risultato = 1;
                RisultatoStr = statusString;
            }
            else
            {
                Risultato = 0;
                RisultatoStr = string.Empty;
            }
            DeltaPartenza = BPHelper.DetaPartenza(DataVolo, OraPartenza);
            if (Forzato)
            {
                Risultato = 1;
                ForzatoManualmente = 1;
            }
            else
            {
                ForzatoManualmente = 0;
            }
        }


        public void Update( string statusString, string DataVolo, string OraPartenza, bool Forzato=false)
        {
          
            OraLettura = DateTime.Now.ToString("HHmm");
            DataLettura = DateTime.Now.ToString("yyyyMMdd");
            if (statusString != string.Empty)
            {
                Risultato = 1;
                RisultatoStr = statusString;
            }
            else
            {
                Risultato = 0;
                RisultatoStr = string.Empty;
            }
            DeltaPartenza = BPHelper.DetaPartenza(DataVolo, OraPartenza);
            if (Forzato)
            {
                Risultato = 0;
                ForzatoManualmente = 1;
            }
            else
            {
                ForzatoManualmente = 0;
            }
        }


        [Column(IsPrimaryKey = true)]
        public string NomePasseggero { get; set; }

        [Column(IsPrimaryKey = true)]
        public string NumVolo { get; set; }

        [Column(IsPrimaryKey = true)]
        public string DataVolo { get; set; }

        [Column(IsPrimaryKey = true)]
        public string CodiceCompagnia { get; set; }

        [Column(IsPrimaryKey = true)]
        public int PostazioneLettura { get; set; }

        [Column(CanBeNull = false)]
        public string OraLettura { get; set; }

        [Column(CanBeNull = false)]
        public string DataLettura { get; set; }

        [Column(CanBeNull = false)]
        public Int16 Risultato { get; set; }

        [Column(CanBeNull = false)]
        public Single DeltaPartenza { get; set; }

        [Column(CanBeNull = true)]
        public string RisultatoStr { get; set; }

        [Column(CanBeNull = true)]
        public Int16? ForzatoManualmente { get; set; }

    }
}

