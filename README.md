samoSsas
========

SamoSsas is an API over the API of the AMO objects. It's specifically designed for an effective way to handle the processing of cubes and monitor it.

## Continuous Integration ##
A continuous integration service is available on AppVeyor at https://ci.appveyor.com/project/CdricLCharlier/samossas/ 
Note that all the tests are not executed on this environment due to limitations in the availability of some components (SSAS).

[![Build status](https://ci.appveyor.com/api/projects/status/vq2itc724iasnfdy)](https://ci.appveyor.com/project/CdricLCharlier/samossas)

## HelloWorld sample ##

````csharp
public class SamossasSaysHelloWorld
{
    private string dbName = "Adventure Works DW 2012";
    private string connString = @"Provider=MSOLAP.4;Data Source=(local)\SQL2014;Initial Catalog='Adventure Works DW 2012'";
        
    public void ProcessTwoDimensions()
    {
        var dimensions = new[] { "Customer", "Date" };

        var monitor = new TraceMonitor();
        monitor.BeginProcess += delegate (object sender, BeginProcessEventArgs e)
        {
            Console.WriteLine("Start:" + e.Object.Name);
        };
        monitor.EndProcess += delegate(object sender, EndProcessEventArgs e)
        {
            Console.WriteLine("End:" + e.Object.Name);
        };

        var processor = new Processor(new[] { monitor });
        processor.Connect(connString, dbName);
        processor.ProcessDimensions(dimensions);
    }
}
````
