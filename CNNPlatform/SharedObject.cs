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
        public double Error { get; set; }
        public bool ExitApplication { get; set; }

        [Serializable]
        public class WeightData
        {
            public int DataCount { get; private set; }
            public Components.RNdObject[] Data { get; set; }
            public Components.Real[] Difference { get; set; }
            public WeightData(int count)
            {
                DataCount = count;
                Data = new Components.RNdObject[count];
                Difference = new Components.Real[count];
            }
        }
        public List<WeightData> Weignt { get; set; } = new List<WeightData>();
    }

}
