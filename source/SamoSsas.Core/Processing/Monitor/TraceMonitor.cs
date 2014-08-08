using Microsoft.AnalysisServices;
using SamoSsas.Processing.Object;
using System;
using System.Collections.Generic;
using Diag = System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Processing.Monitor
{
    public class TraceMonitor : IMonitor
    {
        protected string connectionString;
        protected IEnumerable<ProcessableObject> objects;
        protected Trace trace;
        protected DateTime lastTraceEvent;
        protected object syncLock = new object();
        protected ProcessEventArgsFactory factory;

        TraceEventHandler onTraceEvent;
        TraceStoppedEventHandler onTraceStopped;

        public event EventHandler<BeginProcessEventArgs> BeginProcess;
        public event EventHandler<EndProcessEventArgs> EndProcess;
        public event EventHandler<ProgressProcessEventArgs> ProgressProcess;

        public TraceMonitor()
        {
        }


        public void Start(string connectionString, IEnumerable<ProcessableObject> objects)
        {
            this.connectionString = connectionString;
            this.objects = objects; 
            
            factory = new ProcessEventArgsFactory(objects);

            var server = new Server();
            server.Connect(connectionString);
            trace = server.Traces.Add();
            var beginEvent = DefineEvent(TraceEventClass.ProgressReportBegin);
            var endEvent = DefineEvent(TraceEventClass.ProgressReportEnd);

            // Save the newly created Trace to the server
            trace.Update();

            // Subscribe to the newly create trace.
            onTraceEvent = new TraceEventHandler(OnTraceEvent);
            onTraceStopped = new TraceStoppedEventHandler(OnTraceStopped);

            trace.OnEvent += new TraceEventHandler(OnTraceEvent);
            trace.Stopped += new TraceStoppedEventHandler(OnTraceStopped);
            // this method is not blocking, it starts a separate thread to listen for events from server
            trace.Start(); 
        }

        private TraceEvent DefineEvent(TraceEventClass traceEventClass)
        {
            var newEvent = trace.Events.Add(traceEventClass);

            var columns = new List<TraceColumn>();

            columns.AddRange( new[] {
                TraceColumn.EventClass,
                TraceColumn.EventSubclass,
                TraceColumn.StartTime,
                TraceColumn.ObjectType,
                TraceColumn.ObjectName,
                TraceColumn.TextData,
                TraceColumn.SessionID
            });

            if (traceEventClass==TraceEventClass.ProgressReportEnd)
                columns.AddRange( new[] {
                    TraceColumn.EndTime,
                    TraceColumn.Duration,
                    TraceColumn.CpuTime,
                    TraceColumn.Success
                });

            if (traceEventClass==TraceEventClass.ProgressReportCurrent)
                columns.AddRange( new[] {
                    TraceColumn.ProgressTotal,
                    TraceColumn.IntegerData,
                });

            columns.ToList().ForEach(c => newEvent.Columns.Add(c));
            return newEvent;
        }

        private void OnTraceStopped(ITrace sender, TraceStoppedEventArgs e)
        {
            Diag.Debug.WriteLine("Trace Stopped: Cause = {0}, Exception = {1}", e.StopCause, e.Exception);
            trace.OnEvent -= onTraceEvent;
            trace.Drop();
        }

        private void OnTraceEvent(object sender, TraceEventArgs e)
        {
            lock(syncLock)
            {
                lastTraceEvent = DateTime.Now;
            }
            var processEvent = factory.Build(e);

            if (processEvent is BeginProcessEventArgs)
            {
                EventHandler<BeginProcessEventArgs> handler = BeginProcess;
                if (handler != null)
                    handler(this, processEvent as BeginProcessEventArgs);
                Diag.Debug.WriteLine("Object begin processing: {0}", new [] {processEvent.Object.Name});
            }
            else if (processEvent is EndProcessEventArgs)
            {
                EventHandler<EndProcessEventArgs> handler = EndProcess;
                if (handler != null)
                    handler(this, processEvent as EndProcessEventArgs);
                Diag.Debug.WriteLine("Object end processing: {0}",  new [] {processEvent.Object.Name});
            }
            else if (processEvent is ProgressProcessEventArgs)
            {
                EventHandler<ProgressProcessEventArgs> handler = ProgressProcess;
                if (handler != null)
                    handler(this, processEvent as ProgressProcessEventArgs);
                Diag.Debug.WriteLine("Object currently processing: {0} with {1} rows"
                    ,  new object [] {
                            processEvent.Object.Name
                            , (processEvent as ProgressProcessEventArgs).RowCount
                    }
                );
            }
        }

        public void Stop()
        {
            //If we haven't received an event we should wait a little bit.
            if (lastTraceEvent==DateTime.MinValue)
                lastTraceEvent = DateTime.Now;

            //Waiting at least 5 seconds after the reception of the last event
            while (DateTime.Now.Subtract(lastTraceEvent).TotalSeconds < 5)
            {
                System.Threading.Thread.Sleep(1000);
            }
                
            trace.Stop();
            trace.OnEvent -= onTraceEvent;
            trace.Stopped -= onTraceStopped;
        }

        
    }
}
