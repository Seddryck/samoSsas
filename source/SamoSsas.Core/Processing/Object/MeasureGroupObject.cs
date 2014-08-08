using Microsoft.AnalysisServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Processing.Object
{
    class MeasureGroupObject : CubeAttachedObject
    {
        public MeasureGroupObject(Database database,  string cubeName, string name)
            : base(database, cubeName, name)
        {

        }
        protected override void Initialize()
        {
            ItSelf = Database.Cubes.FindByName(CubeName).MeasureGroups.FindByName(Name);
        }
    }
}
