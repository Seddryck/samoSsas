using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Core.Processing.ResultAnalyzer
{
    public class ErrorReceivedEventArgs
    {
        public string Message { get; private set; }

        internal ErrorReceivedEventArgs(string message)
        {
            Message = message;
        }
    }
}
