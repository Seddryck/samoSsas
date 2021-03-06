﻿using SamoSsas.Processing.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamoSsas.Processing.Monitor
{
    public interface IMonitor
    {
        void Start(string connectionString, IEnumerable<ProcessableObject> objects);
        void Stop();
    }
}
