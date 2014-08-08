using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AnalysisServices;
using SamoSsas.Core.Processing.Object;

namespace SamoSsas.Core.Processing.Monitor
{
    public class ProcessEventArgsFactory
    {
        private readonly IEnumerable<ProcessableObject> trackedObject;

        public ProcessEventArgsFactory(IEnumerable<ProcessableObject> trackedObject)
        {
            this.trackedObject = trackedObject;
        }

        public ProcessEventArgs Build(TraceEventArgs e)
        {
            var objectTypes = new[] { "Dimension", "MeasureGroup", "Partition" };
            if (e.ObjectType==null || !objectTypes.Contains(e.ObjectType.Name))
                return null;

            if (string.IsNullOrEmpty(e.ObjectName) || trackedObject.Count(o => o.Name == e.ObjectName) == 0)
                return null;

            if (e.EventClass == TraceEventClass.ProgressReportBegin && e.EventSubclass == TraceEventSubclass.Process)
                return new BeginProcessEventArgs(FindProcessableObjectByName(e.ObjectName), e.StartTime);

            if (e.EventClass == TraceEventClass.ProgressReportEnd && e.EventSubclass == TraceEventSubclass.Process)
                return new EndProcessEventArgs(FindProcessableObjectByName(e.ObjectName), e.EndTime, Convert.ToBoolean(e.Success));

            if (e.EventClass == TraceEventClass.ProgressReportCurrent && e.EventSubclass == TraceEventSubclass.Process)
                return new ProgressProcessEventArgs(FindProcessableObjectByName(e.ObjectName), e.CurrentTime, e.IntegerData);

            return null;
        }

        protected ProcessableObject FindProcessableObjectByName(string name)
        {
            return trackedObject.First(o => o.Name == name);
        }
    }
}
