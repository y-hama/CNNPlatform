using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNNPlatform.Utility.Shared
{
    [Serializable]
    public class ProcessParameter : Components.Locker.ObjectLocker.Exclusive
    {
        public const string ChannelName = "Process";
        public const string ObjectName = "Controller";

        public bool ExitApplication { get; set; }

        public int Generagion { get; set; }
        public Components.RNdMatrix Result { get; set; }
    }
}
