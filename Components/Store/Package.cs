using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components.Store
{
    public class Package
    {
        private const int HASHLEN = 16;
        public string Hash { get; private set; }

        public int Count { get; private set; }

        private object[] Contents { get; set; }

        public object this[int i]
        {
            get { return Contents[i]; }
            set { Contents[i] = value; }
        }

        internal Package(int count)
        {
            Hash = Locker.HashCreator.GetString(HASHLEN);
            Count = count;
            Contents = new object[count];
        }
    }
}
