using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AnalysisServices;
using SamoSsas.Processing.Monitor;
using SamoSsas.Processing.Object;
using SamoSsas.Processing.ResultAnalyzer;
using System.Diagnostics;

namespace SamoSsas
{
    public class Processor
    {
        private string connectionString = null;
        protected Database database = null;
        protected Server server = null;
        private readonly IEnumerable<IMonitor> monitors;

        private List<string> errors ;
        public IEnumerable<string> Errors 
        {
            get
            {
                return errors;
            }
        }

        public Processor()
        {
            this.monitors = new List<IMonitor>();
        }
       
        public Processor(IEnumerable<IMonitor> monitors)
        {
            this.monitors = monitors;
        }

        protected bool Process(IEnumerable<ProcessableObject> objects, Action<IEnumerable<ProcessableObject>> action)
        {
            if (database == null || server==null)
                throw new InvalidOperationException("You must first connect to a database through the method 'Connect()'.");

            server.CaptureXml = true;

            action.Invoke(objects);

            Debug.WriteLine("Starting monitors ...");
            monitors.ToList().ForEach(m => m.Start(connectionString, objects));
            Debug.WriteLine("All monitors have been started.");
            Debug.WriteLine("Executing process requests ...");
            var processResult = server.ExecuteCaptureLog(true, true);
            Debug.WriteLine("All Process requests executed have been executed or cancelled.");
            Debug.WriteLine("Stopping monitors ...");
            monitors.ToList().ForEach(m => m.Stop());
            Debug.WriteLine("All monitors have been stopped.");

            var analyzer = new AdvancedResultAnalyzer();
            Debug.WriteLine("Analyzing results ...");
            var booleanResult = analyzer.Analyze(processResult);
            Debug.WriteLineIf(booleanResult, "All process requests have been successfully executed.");
            Debug.WriteLineIf(!booleanResult, "At least one error has occured during the processing requests.");
            errors = new List<string>();
            errors.AddRange(analyzer.Errors);

            return booleanResult;
        }


        public bool ProcessDimensions(IEnumerable<string> dimensionNames)
        {
            if (database == null)
                throw new InvalidOperationException("You must first connect to a database through the method 'Connect()'.");

            var dimensions = new List<DimensionObject>();
            foreach (var dimensionName in dimensionNames)
            {
                Debug.WriteLine("Looking for dimension '{0}'", new [] {dimensionName});
                var dimension = database.Dimensions.FindByName(dimensionName);
                if (dimension == null)
                {
                    Debug.WriteLine("Dimension '{0}' has NOT been found", new [] {dimensionName});
                    throw new ArgumentOutOfRangeException("dimensions", dimensions, string.Format("Dimension '{0}' has NOT been found", dimensionName));
                }
                Debug.WriteLine("Dimension '{0}' has been found", new [] {dimensionName});
                var dimObject = new DimensionObject(database, dimensionName);
                dimensions.Add(dimObject);
            }
            return Process(dimensions, this.ProcessDimensions);
        }

        internal bool ProcessDimensions(IEnumerable<DimensionObject> objects)
        {
            return Process(objects, this.ProcessDimensions);
        }

        protected void ProcessDimensions(IEnumerable<ProcessableObject> objects)
        {
            var dimensions = objects.Cast<DimensionObject>();

            foreach (var dimension in dimensions)
            {
                dimension.Refresh();
                switch (dimension.ProcessingState)
                {
                    case ProcessingState.Processed:
                        Debug.WriteLine("Requesting an update process of dimension '{0}' ({1})", dimension.Name, dimension.Id);
                        dimension.Process(ProcessType.ProcessUpdate);
                        break;
                    default:
                        Debug.WriteLine("Requesting a full process of dimension '{0}' ({1})", dimension.Name, dimension.Id);
                        dimension.Process(ProcessType.ProcessFull);
                        break;
                }   
            }
        }

        public bool ProcessMeasureGroups(string cubeName, IEnumerable<string> measureGroupNames)
        {
            if (database == null)
                throw new InvalidOperationException("You must first connect to a database through the method 'Connect()'.");

            var measureGroups = new List<MeasureGroupObject>();
            foreach (var measureGroupName in measureGroupNames)
            {
                Debug.WriteLine("Looking for measure-group '{0}'", new [] {measureGroupName});
                var measureGroup = database.Cubes.FindByName(cubeName).MeasureGroups.FindByName(measureGroupName);
                if (measureGroup == null)
                {
                    Debug.WriteLine("Measure-group '{0}' has NOT been found", new [] {measureGroupName});
                    throw new ArgumentOutOfRangeException("measureGroupNames", measureGroupNames, string.Format("Measure-group '{0}' has NOT been found", measureGroupName));
                }
                Debug.WriteLine("Measure-group '{0}' has been found", new [] {measureGroupName});
                var mgObject = new MeasureGroupObject(database, cubeName, measureGroupName);
                measureGroups.Add(mgObject);
            }
            return Process(measureGroups, this.ProcessMeasureGroups);
        }

        internal bool ProcessMeasureGroups(IEnumerable<MeasureGroupObject> objects)
        {
            return Process(objects, this.ProcessMeasureGroups);
        }

        protected void ProcessMeasureGroups(IEnumerable<ProcessableObject> objects)
        {
            var measureGroups = objects.Cast<MeasureGroupObject>();

            foreach (var measureGroup in measureGroups)
            {
                measureGroup.Process(ProcessType.ProcessFull);
                Debug.WriteLine( "Requesting an update process of measure-group '{0}' ({1})", measureGroup.Name, measureGroup.Id);
            }
        }

        public bool ProcessPartitions(string cubeName, IEnumerable<string> partitionNames)
        {
            if (database == null)
                throw new InvalidOperationException("You must first connect to a database through the method 'Connect()'.");

            Debug.WriteLine("Looking for the cube '{0}'", new [] {cubeName});
            var cube = database.Cubes.FindByName(cubeName);
            if (cube == null)
            {
                Debug.WriteLine("The cube named '{0}' has NOT been found", new [] {cubeName});
                throw new ArgumentOutOfRangeException("cubeName", cubeName, string.Format("The cube named '{0}' has NOT been found", new [] {cubeName}));
            }
            Debug.WriteLine("The cube named '{0}' has been found", new [] {cubeName});

            var partitions = new List<PartitionObject>();
            foreach (MeasureGroup measureGroup in cube.MeasureGroups)
            {
                var foundPartitions = measureGroup.Partitions.Cast<Partition>().Where(p => partitionNames.Contains(p.Name));
                foreach (var foundPartition in foundPartitions)
                {
                    var partitionObject = new PartitionObject(database, cubeName, foundPartition.Name);
                    partitions.Add(partitionObject);
                    Debug.WriteLine("The partition named '{0}' has been found", new [] {foundPartition.Name});
                }
            }

            var notFoundPartitions = partitionNames.Except(partitions.Select(name => name.Name));
            if (notFoundPartitions.Count() > 0)
            {
                Debug.WriteLine("The following partitions have not been found: {0}", new [] {string.Join(", ", notFoundPartitions)});
                throw new ArgumentOutOfRangeException("partitionNames", partitionNames
                    , string.Format("The following partitions have not been found: {0}", string.Join(", ", notFoundPartitions))
                    );
            }

            return ProcessPartitions(partitions);
        }

        internal bool ProcessPartitions(IEnumerable<PartitionObject> objects)
        {
            return Process(objects, this.ProcessPartitions);
        }
        protected void ProcessPartitions(IEnumerable<ProcessableObject> objects)
        {
            var partitions = objects.Cast<PartitionObject>();
        
            foreach (var cubeName in partitions.Select(p => p.CubeName).Distinct())
            {
                foreach (MeasureGroup measureGroup in database.Cubes.FindByName(cubeName).MeasureGroups)
                {
                    foreach (Partition partition in measureGroup.Partitions)
                    {
                        var target = partitions.FirstOrDefault(p => p.CubeName==cubeName && p.Name == partition.Name);
                        if (target!=null)
                        {
                            target.Process(ProcessType.ProcessFull);
                            Debug.WriteLine( "Requesting update process of partition {0} from measure-group '{1}'.", target.Name, measureGroup.Name);
                        }
                    }
                }
            }
        }

        public virtual void Connect(string connectionString, string databaseName)
        {
            this.connectionString = connectionString;
            server = new Server();
            server.Connect(connectionString);
            database = server.Databases.FindByName(databaseName);
            if (database == null)
            {
                Debug.WriteLine("Database '{0}' has NOT been found.", new[] { databaseName });
                throw new ArgumentOutOfRangeException("databaseName", databaseName, string.Format("Database '{0}' has NOT been found", databaseName));
            }
            Debug.WriteLine("Database '{0}' has been found.", new [] {databaseName});
        }
    }
}
