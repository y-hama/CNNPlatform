using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components.Store
{
    public class Storage : Locker.ObjectLocker
    {
        private Storage() { }
        public static Storage Instance { get; } = new Storage();

        private List<Package> Packages = new List<Package>();


        public Package CreatePackage(int contentsCount)
        {
            var pkg = new Package(contentsCount);
            Packages.Add(pkg);
            return pkg;
        }

        public Package Search(string hash, string hashpass = "FORCE")
        {
            LockWait(hashpass);
            return Packages.Find(x => x.Hash == hash);
        }

    }
}
