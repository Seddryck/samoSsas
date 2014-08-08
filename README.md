samoSsas
========

SamoSsas is an API over the API of the AMO objects. It's specifically designed for an effective way to handle the processing of cubes and monitor it.

![project status](http://stillmaintained.com/Seddryck/samoSsas.png)

## Continuous Integration ##
A continuous integration service is available on AppVeyor at https://ci.appveyor.com/project/CdricLCharlier/samossas/ 
Note that all the tests are not executed on this environment due to limitations in the availability of some components (SSAS).

[![Build status](https://ci.appveyor.com/api/projects/status/vq2itc724iasnfdy)](https://ci.appveyor.com/project/CdricLCharlier/samossas)

## HelloWorld sample ##

````csharp
public class SamossasSaysHelloWorld
{    
    private static string dbName = "Adventure Works DW 2012";
    private static string connString = @"Provider=MSOLAP.4;Data Source=(local)\SQL2014;Initial Catalog='Adventure Works DW 2012'";

    static void Main(string[] args)
    {
        var dimensions = new[] { "Customer", "Date" };

        var monitor = new TraceMonitor();
        monitor.BeginProcess += delegate (object sender, BeginProcessEventArgs e)
        {
            Console.WriteLine("Start process of '" + e.Object.Name + "' at " + e.Timing.ToString("hh:mm:ss.ffff"));
        };
        
        monitor.EndProcess += delegate(object sender, EndProcessEventArgs e)
        {
            Console.WriteLine("{0} process of '{1}' at {2}"
                , e.Success ? "Successful" : "Failed"
                , e.Object.Name
                , e.Timing.ToString("hh:mm:ss.ffff"));
        };

        var processor = new Processor(new[] { monitor });
        processor.Connect(connString, dbName);
        processor.ProcessDimensions(dimensions);
    }
}
````
