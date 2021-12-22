using System;
using System.Collections.Generic;
using Serilog;

namespace Microsoft.Extensions.Hosting
{
    public class SerilogOptions
    {
        public string ApplicationName { get; set; }
        public bool LogToStandardOutput{ get; set; }
        public List<string> LogCategories { get; } = new();
        public string LocalOutputTemplate { get; set; }
        public Action<LoggerConfiguration> OnConfigure { get; set; }
    }
}