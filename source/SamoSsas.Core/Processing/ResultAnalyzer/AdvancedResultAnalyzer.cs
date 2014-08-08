using Microsoft.AnalysisServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Core.Processing.ResultAnalyzer
{
    public class AdvancedResultAnalyzer : BasicResultAnalyzer
    {
        private IList<string> errors = new List<string>();
        public IEnumerable<string> Errors
        {
            get
            {
                return errors;
            }
        }

        public event EventHandler<ErrorReceivedEventArgs> ErrorReceived;
        public override bool Analyze(XmlaResultCollection processResult)
        {
            var isError = false;
            foreach (XmlaResult xmlaResult in processResult)
            {
                var message= string.Empty;
                foreach (var xmlaMessage in xmlaResult.Messages)
                    if (xmlaMessage is XmlaError)
                        message = Concatenate(message, xmlaMessage as XmlaError);
                if (!string.IsNullOrEmpty(message))
                {
                    errors.Add(message);
                    RaiseErrorReceived(message);
                    isError = true;
                }
            }
            return !isError;
        }

        private void RaiseErrorReceived(string message)
        {
            var e = new ErrorReceivedEventArgs(message);
            EventHandler<ErrorReceivedEventArgs> handler = ErrorReceived;
            if (handler != null)
                handler(this, e);
        }

        private string Concatenate(string message, XmlaError error)
        {
            if (!string.IsNullOrEmpty(message))
                return message + "\r\n" + error.Description;
            else
                return error.Description;
        }
    }
}
