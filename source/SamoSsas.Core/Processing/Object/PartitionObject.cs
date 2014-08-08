using Microsoft.AnalysisServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Core.Processing.Object
{
    class PartitionObject : CubeAttachedObject
    {
        public PartitionObject(Database database, string cubeName, string name)
            : base(database, cubeName, name)
        {
        }

        protected override void Initialize()
        {
            var cube = Database.Cubes.FindByName(CubeName);
            int i = 0;
            while (ItSelf == null)
            {
                ItSelf = cube.MeasureGroups[i].Partitions.FindByName(Name);
                i++;
            }
        }
    }
}
