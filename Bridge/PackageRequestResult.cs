using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.CLI
{
    public class PackageRequestResult
    {
        public bool Success
        {
            get;
            set;
        }

        public bool Exists
        {
            get;
            set;
        }

        public string Folder
        {
            get;
            set;
        }
    }
}
