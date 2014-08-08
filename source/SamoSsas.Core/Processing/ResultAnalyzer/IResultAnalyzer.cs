using Microsoft.AnalysisServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Processing.ResultAnalyzer
{
    public interface IResultAnalyzer
    {
        bool Analyze(XmlaResultCollection results);
    }
}
