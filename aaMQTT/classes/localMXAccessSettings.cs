using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aaMQTT
{
    public class localMXAccessSettings
    {
        public string publishregexmatch { get; set; }
        public string publishregexreplace { get; set; }
        public string subscriberegexmatch { get; set; }
        public string subscriptregexreplace { get; set; }
        public string roottopic { get; set; }
        public List<string> publishtags { get; set; }
        public List<subscription> subscribetags { get; set; }
    }
    public class subscription
    {
        public string topic { get; set; }
        public string writetag { get; set; }
        public int hitem { get; set; }
    }
}
