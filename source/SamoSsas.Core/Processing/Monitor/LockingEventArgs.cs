using SamoSsas.Core.Processing.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Core.Processing.Monitor
{
    public class LockingEventArgs : EventArgs
    {
        public ProcessableObject Object { get; private set; }
        public DateTime Timing { get; private set; }

        internal LockingEventArgs(ProcessableObject @object, DateTime timing)
        {
            this.Object = @object;
            this.Timing = timing;
        }
    }
}
