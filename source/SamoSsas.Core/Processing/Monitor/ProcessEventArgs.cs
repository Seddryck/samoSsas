using SamoSsas.Core.Processing.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Core.Processing.Monitor
{
    public class ProcessEventArgs : EventArgs
    {
        public ProcessableObject Object { get; private set; }
        public DateTime Timing { get; private set; }

        protected ProcessEventArgs(ProcessableObject obj, DateTime timing)
	    {
            Object = obj;
            Timing = timing;
	    }
    }

    public class BeginProcessEventArgs : ProcessEventArgs
    {
        internal BeginProcessEventArgs(ProcessableObject obj, DateTime timing)
            : base(obj, timing)
        { }
    }

    public class EndProcessEventArgs : ProcessEventArgs
    {
        public bool Success { get; private set; }
        internal EndProcessEventArgs(ProcessableObject obj, DateTime timing, bool success)
            : base(obj, timing)
        {
            Success = success;
        }
    }

    public class ProgressProcessEventArgs : ProcessEventArgs
    {
        public long RowCount { get; private set; }
        internal ProgressProcessEventArgs(ProcessableObject obj, DateTime timing, long rowCount)
            : base(obj, timing)
        {
            RowCount = rowCount;
        }
    }
}
