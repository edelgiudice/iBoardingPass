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
    [Table(Name = "IngressiManuali")]
    public class IngressiManualiTable
    {
        [Column(IsPrimaryKey = true)]
        public DateTime CodiceRegostrazione { get; set; }

        [Column(IsPrimaryKey = true)]
        public int CodicePostazione { get; set; }

    }
}

