using Microsoft.AnalysisServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SamoSsas.Processing.Object;
using System.Diagnostics;

namespace SamoSsas.Processing.Monitor
{
    public class ProcessingStateMonitor : IMonitor
    {
        private IEnumerable<ProcessableObject> objects;
        private readonly Dictionary<ProcessableObject, DateTime> dico = new Dictionary<ProcessableObject, DateTime>();
        private Timer timer;
        private bool fullyProcessed = false;

        public event EventHandler<EndProcessEventArgs> EndProcess;

        public ProcessingStateMonitor(TimeSpan occurence)
        {
        }

        public void Start(string connectionString, IEnumerable<ProcessableObject> objects)
        {
            this.objects = objects;

            foreach (var obj in objects)
                dico.Add(obj, obj.LastProcessed);
            this.timer = new Timer(OnTime, null, 50, 50);
        }


        private void OnTime(object state)
        {
            //Go through the collection
            var finished = new Dictionary<ProcessableObject, DateTime>();
            lock (dico)
            {
                if (fullyProcessed)
                    return;

                foreach (var processable in dico.Keys)
                {
                    processable.Refresh();
                    if (dico[processable] != processable.LastProcessed)
                    {
                        Debug.WriteLine("The processable object '{0}' has been processed at {1}.", processable.Name, processable.LastProcessed);
                        finished.Add(processable, processable.LastProcessed);
                    }
                }

                //Remove the dimensions that have been processed
                // because we don't need to refresh them anymore :-)
                foreach (var processable in finished.Keys)
                    dico.Remove(processable);
            }

            foreach (var f in finished.Keys)
                RaiseEvent(f, finished[f]);
        }

        private void RaiseEvent(ProcessableObject processObject, DateTime timing)
        {
            var e = new EndProcessEventArgs(processObject, timing, true);
            EventHandler<EndProcessEventArgs> handler = EndProcess;
            if (handler != null)
                handler(this, e);
            Debug.WriteLine("Object processed: {0} at {1}", e.Object.Name, timing);
        }
        public void Stop()
        {
            timer = null;
        }
    }
}
