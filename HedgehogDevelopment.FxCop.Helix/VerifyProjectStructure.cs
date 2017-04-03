using Microsoft.FxCop.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace HedgehogDevelopment.FxCop.Helix
{
    public class VerifyProjectStructure : BaseSiblingTest
    {
        public VerifyProjectStructure() : 
            base("VerifyProjectStructure")
        {
        }

        protected override string BaseNamespaceMoniker
        {
            get
            {
                return "Project";
            }
        }
    }
}
