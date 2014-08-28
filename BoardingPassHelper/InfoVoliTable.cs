using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Collections;
using System.Data.Linq;


namespace iBoardingPass
{
    [Table(Name = "InfoVoli")]
    public class InfoVoliTable
    {
        [Column(IsPrimaryKey = true)]
        public string NumVolo { get; set; }

        [Column(IsPrimaryKey = true)]
        public string DataVolo { get; set; }

        [Column(CanBeNull = false)]
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


        public static Table<InfoVoliTable> GetInfoVoli(DataContext dx)
        {
            return dx.GetTable<InfoVoliTable>();
        }

        

    }
}
