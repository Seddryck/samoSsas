using Microsoft.AnalysisServices;
using NUnit.Framework;
using SamoSsas;
using SamoSsas.Processing;
using SamoSsas.Processing.Monitor;
using SamoSsas.Processing.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Testing
{
    public class ProcessorTest
    {

        private string dbName = "Adventure Works DW 2012";
        private string connString = @"Provider=MSOLAP.4;Data Source=(local)\SQL2014;Initial Catalog='Adventure Works DW 2012'";
        private string dimCustomerName = "Customer";
        private string dimDateName = "Date";

        [Test]
        public void Process_DimensionCustomerLockMonitor_LastProcessedIsUpdated()
        {
            var server = new Server();
            server.Connect(connString);
            
            var db = server.Databases.FindByName(dbName);
            var dimCustomer = db.Dimensions.FindByName(dimCustomerName);
            var processBeforeTest = dimCustomer.LastProcessed;

            //var processObjects = new ProcessableObject[] { new DimensionObject(db, dimCustomerName) };
            var processor = new Processor(new IMonitor[]
                {
                    new LockMonitor(new TimeSpan(0,0,0,0,500))
                });
            processor.Connect(connString, dbName);
            var result = processor.ProcessDimensions(new List<string>() { dimCustomerName });
            Assert.That(result, Is.True);

            dimCustomer.Refresh();
            Assert.That(processBeforeTest, Is.LessThan(dimCustomer.LastProcessed));
        }

        [Test]
        public void Process_DimensionCustomerProcessingStateMonitor_LastProcessedIsUpdated()
        {
            var server = new Server();
            server.Connect(connString);

            var db = server.Databases.FindByName(dbName);
            var dimCustomer = db.Dimensions.FindByName(dimCustomerName);
            var processBeforeTest = dimCustomer.LastProcessed;

            var processor = new Processor(new IMonitor[]
                {
                    new ProcessingStateMonitor(new TimeSpan(0,0,0,0,500))
                });
            processor.Connect(connString, dbName);
            var result = processor.ProcessDimensions(new List<string>() { dimCustomerName });
            Assert.That(result, Is.True);

            dimCustomer.Refresh();
            Assert.That(processBeforeTest, Is.LessThan(dimCustomer.LastProcessed));
        }

        [Test]
        public void Process_DimensionCustomerTraceMonitor_LastProcessedIsUpdated()
        {
            var server = new Server();
            server.Connect(connString);

            var db = server.Databases.FindByName(dbName);
            var dimCustomer = db.Dimensions.FindByName(dimCustomerName);
            var processBeforeTest = dimCustomer.LastProcessed;

            var processor = new Processor(new IMonitor[]
                {
                    new TraceMonitor()
                });
            processor.Connect(connString, dbName);
            var result = processor.ProcessDimensions(new List<string>() { dimCustomerName });
            Assert.That(result, Is.True);

            dimCustomer.Refresh();
            Assert.That(processBeforeTest, Is.LessThan(dimCustomer.LastProcessed));
        }

        [Test]
        public void Process_DimensionCustomerAndDate_LastProcessedIsUpdated()
        {
            var server = new Server();
            server.Connect(connString);

            var db = server.Databases.FindByName(dbName);
            var processBeforeTest = db.Dimensions.FindByName(dimCustomerName).LastProcessed;
            var processBeforeTest2 = db.Dimensions.FindByName(dimDateName).LastProcessed;

            var processor = new Processor(new IMonitor[]
                {
                    new LockMonitor(new TimeSpan(0,0,0,0,500))
                    , new ProcessingStateMonitor(new TimeSpan(0,0,0,0,500))
                    ,new TraceMonitor()
                });
            processor.Connect(connString, dbName);
            processor.ProcessDimensions(new List<string>() { dimCustomerName, dimDateName });

            server.Databases.FindByName(dbName).Dimensions.FindByName(dimCustomerName).Refresh();
            server.Databases.FindByName(dbName).Dimensions.FindByName(dimDateName).Refresh();
            Assert.That(processBeforeTest, Is.LessThan(server.Databases.FindByName(dbName).Dimensions.FindByName(dimCustomerName).LastProcessed));
            Assert.That(processBeforeTest, Is.LessThan(server.Databases.FindByName(dbName).Dimensions.FindByName(dimDateName).LastProcessed));
        }

        [Test]
        public void Process_AllDimension_LastProcessedIsUpdated()
        {
            var server = new Server();
            server.Connect(connString);

            var processBeforeTest = DateTime.MinValue;
            var dimensionNames = new List<string>();
            var db = server.Databases.FindByName(dbName);
            processBeforeTest = db.Dimensions.Cast<Dimension>().Max<Dimension, DateTime>(x => x.LastProcessed);
            foreach (Dimension dimension in db.Dimensions)
            {
                if (!((new[] { "Clustered Customers", "Subcategory Basket Analysis" }).Contains(dimension.Name)))
                {
                    dimensionNames.Add(dimension.Name);
                }
            }

            var processor = new Processor(new IMonitor[]
                {
                    new LockMonitor(new TimeSpan(0,0,0,0,500))
                    , new ProcessingStateMonitor(new TimeSpan(0,0,0,0,500))
                    , new TraceMonitor()
                });
            processor.Connect(connString, dbName);
            processor.ProcessDimensions(dimensionNames);

            foreach (var dimensionName in dimensionNames)
            {
                var dimension = db.Dimensions.FindByName(dimensionName);
                dimension.Refresh();
                Assert.That(processBeforeTest, Is.LessThan(dimension.LastProcessed), dimension.Name);
            }
        }

        [Test]
        public void Process_AllMeasureGroups_LastProcessedIsUpdated()
        {
            var server = new Server();
            server.Connect(connString);

            var processBeforeTest = DateTime.MinValue;
            var measureGroupNames = new List<string>();
            var db = server.Databases.FindByName(dbName);
            processBeforeTest = db.Cubes.FindByName("Adventure Works").MeasureGroups.Cast<MeasureGroup>().Max<MeasureGroup, DateTime>(x => x.LastProcessed);
            foreach (MeasureGroup measureGroup in db.Cubes.FindByName("Adventure Works").MeasureGroups)
            {
                measureGroupNames.Add(measureGroup.Name);
            }

            var processor = new Processor(new IMonitor[]
                {
                     new TraceMonitor()
                });
            processor.Connect(connString, dbName);
            processor.ProcessMeasureGroups("Adventure Works", measureGroupNames);

            foreach (var measureGroupName in measureGroupNames)
            {
                var measureGroup = db.Cubes.FindByName("Adventure Works").MeasureGroups.FindByName(measureGroupName);
                measureGroup.Refresh();
                Assert.That(processBeforeTest, Is.LessThan(measureGroup.LastProcessed), measureGroup.Name);
            }
        }

        [Test]
        public void Process_AllPartitionsOfMeasureGroup_LastProcessedIsUpdated()
        {
            var server = new Server();
            server.Connect(connString);

            var processBeforeTest = DateTime.MinValue;
            var partitions = new List<string>();
            var db = server.Databases.FindByName(dbName);
            var specificMeasureGroup = db.Cubes.FindByName("Adventure Works").MeasureGroups.FindByName("Internet Sales");
            processBeforeTest = specificMeasureGroup.Partitions.Cast<Partition>().Max<Partition, DateTime>(x => x.LastProcessed);
            foreach (Partition partition in specificMeasureGroup.Partitions)
            {
                partitions.Add(partition.Name);
            }

            var processor = new Processor(new IMonitor[]
                {
                    new LockMonitor(new TimeSpan(0,0,0,0,500))
                    , new ProcessingStateMonitor(new TimeSpan(0,0,0,0,500))
                    , new TraceMonitor()
                });
            processor.Connect(connString, dbName);
            processor.ProcessPartitions("Adventure Works", partitions);

            foreach (var partitionName in partitions)
            {
                var partition = specificMeasureGroup.Partitions.FindByName(partitionName);
                partition.Refresh();
                Assert.That(processBeforeTest, Is.LessThan(partition.LastProcessed), partition.Name);
            }
        }

        [Test]
        public void Process_DimensionCustomerTraceMonitor_BeginEventRaised()
        {
            var monitor = new TraceMonitor();
            BeginProcessEventArgs beginEvent=null;
            var processor = new Processor(new IMonitor[]
                {
                    monitor
                });
            processor.Connect(connString, dbName);

            monitor.BeginProcess += delegate(object sender, BeginProcessEventArgs e)
            {
                if (beginEvent == null)
                    beginEvent = e;
                else
                    Assert.Fail("Begin event has been received more than once.");
            };
            
            var result = processor.ProcessDimensions(new List<string>() { dimCustomerName });
            var maxTime = DateTime.Now;

            Assert.That(result, Is.True);
            Assert.That(beginEvent, Is.Not.Null);
            Assert.That(beginEvent.Object.Name, Is.EqualTo(dimCustomerName));
            Assert.That(beginEvent.Timing, Is.LessThan(maxTime));
        }

        [Test]
        public void Process_DimensionCustomerTraceMonitor_EndEventRaised()
        {
            var monitor = new TraceMonitor();
            EndProcessEventArgs endEvent = null;
            var processor = new Processor(new IMonitor[]
                {
                    monitor
                });
            processor.Connect(connString, dbName);

            monitor.EndProcess += delegate(object sender, EndProcessEventArgs e)
            {
                if (endEvent == null)
                    endEvent = e;
                else
                    Assert.Fail("End event has been received more than once.");
            };

            var result = processor.ProcessDimensions(new List<string>() { dimCustomerName });
            var maxTime = DateTime.Now;

            Assert.That(result, Is.True);
            Assert.That(endEvent, Is.Not.Null);
            Assert.That(endEvent.Object.Name, Is.EqualTo(dimCustomerName));
            Assert.That(endEvent.Timing, Is.LessThan(maxTime));
            Assert.That(endEvent.Success, Is.True);
        }

        [Test]
        public void Process_DimensionCustomerAndDateTraceMonitor_EventsArePaired()
        {
            var monitor = new TraceMonitor();
            var processor = new Processor(new IMonitor[]
                {
                    monitor
                });
            processor.Connect(connString, dbName);

            var events = new Dictionary<string, bool>();

            monitor.BeginProcess += delegate(object sender, BeginProcessEventArgs e)
            {
                lock(events)
                {
                    Assert.That(events.Keys.Contains(e.Object.Name), Is.False);
                    events.Add(e.Object.Name, false);
                }
                
            };

            monitor.EndProcess += delegate(object sender, EndProcessEventArgs e)
            {
                lock (events)
                {
                    Assert.That(events.Keys.Contains(e.Object.Name), Is.True);
                    Assert.That(events[e.Object.Name], Is.False);
                    events[e.Object.Name] = true;
                }
            };

            var result = processor.ProcessDimensions(new List<string>() { dimCustomerName, dimDateName });
            
            Assert.That(events, Has.Count.EqualTo(2));
            Assert.That(events.Values, Has.All.True);
        }

    }
}
