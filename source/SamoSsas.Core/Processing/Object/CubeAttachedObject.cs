using Microsoft.AnalysisServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Processing.Object
{
    abstract class CubeAttachedObject : ProcessableObject
    {
        public string CubeName { get; private set; }

        protected CubeAttachedObject(Database database, string cubeName, string name)
            : base(database, name)
        {
            this.CubeName = cubeName;
        }
    }
}
