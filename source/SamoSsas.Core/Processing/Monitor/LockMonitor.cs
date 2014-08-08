using Microsoft.AnalysisServices.AdomdClient;
using SamoSsas.Core.Processing.Object;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;


namespace SamoSsas.Core.Processing.Monitor
{
    public class LockMonitor : IMonitor
    {
        private string connectionString;
        private IEnumerable<ProcessableObject> objects;
        private readonly TimeSpan occurence;
        private Timer timer;

        public event EventHandler<LockingEventArgs> ObjectLocked;

        public LockMonitor(TimeSpan occurence)
        {
            this.occurence = occurence;
        }

        public void Start(string connectionString, IEnumerable<ProcessableObject> objects)
        {
            this.connectionString = connectionString;
            this.objects = objects;
            this.timer = new Timer(OnTime, null, 50, 50);
        }

        
        protected virtual void OnTime(object state)
        {
            var locks = GetLocks(connectionString);
            if (locks.Count==0)
                Debug.WriteLine("No Lock found!");
            foreach (var loc in locks)
                RaiseLockedEvent(loc.Key, loc.Value);
        }

        protected void RaiseLockedEvent(ProcessableObject processObject, DateTime timing)
        {
            var e = new LockingEventArgs(processObject, timing);
            EventHandler<LockingEventArgs> handler = ObjectLocked;
            if (handler != null)
                handler(this, e);
            Debug.WriteLine("Object Locked: {0}", new [] {e.Object.Name});
        }

        protected Dictionary<ProcessableObject, DateTime> GetLocks(string connectionString)
        {
            var ds = new DataSet();
            using (var conn = new AdomdConnection(connectionString))
            {
                using (var cmd = new AdomdCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "select LOCK_OBJECT_ID, LOCK_GRANT_TIME from $system.discover_locks where LOCK_STATUS=1 and LOCK_TYPE=4";
                    var da = new AdomdDataAdapter(cmd);
                    da.Fill(ds);
                }
            }

            var locks = new Dictionary<ProcessableObject, DateTime>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                var objectLocked = ParseLockObjectId((string)dr["LOCK_OBJECT_ID"]);
                if (objectLocked != null)
                    locks.Add(objectLocked, (DateTime)dr["LOCK_GRANT_TIME"]);
            }

            return locks;
        }

        protected ProcessableObject ParseLockObjectId(string xmlValue)
        {
            var xml = new XmlDocument();
            xml.LoadXml(xmlValue);

            var xmlNode = xml.ChildNodes[0].ChildNodes[1];

            if (xmlNode.Name=="DimensionID")
            {
                var id = xmlNode.InnerText;

                return objects.FirstOrDefault(o => o.Id == id);
            }
            else if (xmlNode.Name=="CubeID")
            {
                var cubeName = xmlNode.InnerText;

                if (xml.ChildNodes[0].ChildNodes.Count==3 && xml.ChildNodes[0].ChildNodes[2].Value=="MeasureGroupID")
                {
                    var id = xml.ChildNodes[0].ChildNodes[2].InnerText;
                    return objects.FirstOrDefault(o => o.Id == id);
                }
                else if (xml.ChildNodes[0].ChildNodes.Count==4 && xml.ChildNodes[0].ChildNodes[3].Value == "PartitionID")
                {
                    var id = xml.ChildNodes[0].ChildNodes[3].InnerText;
                    return objects.FirstOrDefault(o => o.Id == id);
                }
            }
            return null;    

        }

        public void Stop()
        {
            this.timer.Dispose();
        }
    }
}
