using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AnalysisServices;

namespace SamoSsas.Core.Processing.ResultAnalyzer
{
    public class BasicResultAnalyzer
    {
        public virtual bool Analyze(XmlaResultCollection processResult)
        {
            foreach (XmlaResult xmlaResult in processResult)
                foreach (var xmlaMessage in xmlaResult.Messages)
                    if (xmlaMessage is XmlaError)
                        return false;
            return true;
        }
    }
}
