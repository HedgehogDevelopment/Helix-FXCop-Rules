using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgehogDevelopment.FxCop.Helix
{
    public class VerifyFoundationProjectStructure : BaseParentTest
    {
        public VerifyFoundationProjectStructure() : base("VerifyFoundationProjectStructure")
        {
        }

        protected override string BaseNamespaceMoniker
        {
            get
            {
                return "Foundation";
            }
        }

        protected override string ParentNamespaceMoniker
        {
            get
            {
                return "Project";
            }
        }
    }
}
