using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace AggiornaStatoVoli
{
    public partial class ServiceAggStatoVoli : ServiceBase
    {
        private System.Diagnostics.EventLog _eventLogAggVoli;

        public ServiceAggStatoVoli(ref System.Diagnostics.EventLog eventLogAggVoli)
        {
            InitializeComponent();
            _eventLogAggVoli = eventLogAggVoli;
          
        }

        protected override void OnStart(string[] args)
        {
            _eventLogAggVoli.WriteEntry("Avvio servizio", EventLogEntryType.Information);
            AggVoliMng.Start(ref _eventLogAggVoli);
        }

        protected override void OnStop()
        {

            _eventLogAggVoli.WriteEntry("Arresto servizio", EventLogEntryType.Information);
        }
    }
}
