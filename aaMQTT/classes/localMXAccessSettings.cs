using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aaMQTT
{
    public class localMXAccessSettings
    {
        public string roottopic { get; set; }
        public List<publish> publishtags { get; set; }
        public List<subscription> subscribetags { get; set; }
    }

    public class publish
    {
        public string tag { get; set; }
        public byte qoslevel { get; set; }
        public bool retain { get; set; }
    }

    public class subscription
    {
        public string topic { get; set; }
        public string writetag { get; set; }
        public byte qoslevel { get; set; }
        public int hitem { get; set; }
    }
}
