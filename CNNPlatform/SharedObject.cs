using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform
{
    public class SharedObject : Components.Locker.ObjectLocker.Exclusive
    {
        public const string ChannelName = "Model";
        public const string ObjectName = "Controller";

        public bool Initialized { get; set; }


        public int Generation { get; set; }
        public bool ExitApplication { get; set; }

        [Serializable]
        public class WeigntData
        {
            public Components.RNdObject[] Data { get; set; }
        }
        public List<WeigntData> Weignt { get; set; } = new List<WeigntData>();
    }

}
