using Microsoft.AnalysisServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Processing.Object
{
    public abstract class ProcessableObject
    {
        protected ProcessableMajorObject ItSelf { get; set; }
        public Database Database { get; private set; }
        public string Name { get; private set; }
        public string Id
        {
            get
            {
                if (ItSelf == null)
                    Initialize();
                return ItSelf.ID;
            }
            
        }

        protected ProcessableObject(Database database, string name)
        {
            this.Database = database;
            this.Name = name;
        }

        protected abstract void Initialize();

        public void Refresh()
        {
            if (ItSelf == null)
                Initialize();

            ItSelf.Refresh();
        }

        public DateTime LastProcessed
        {
            get
            {
                if (ItSelf == null)
                    Initialize();

                return ItSelf.LastProcessed;
            }
            
        }

        public void Process(ProcessType processType)
        {
            if (ItSelf == null)
                Initialize();
            ItSelf.Process(processType);
        }

    }
}
