using Microsoft.AnalysisServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Core.Processing.Object
{
    class DimensionObject : ProcessableObject
    {
        public DimensionObject(Database database, string name)
            : base(database, name)
        { }

        protected override void Initialize()
        {
            ItSelf = Database.Dimensions.FindByName(Name);
        }

        public ProcessingState ProcessingState
        {
            get
            {
                if (ItSelf == null)
                    Initialize();
                return (ItSelf as Dimension).ProcessingState;
            }
        }

        
    }
}
