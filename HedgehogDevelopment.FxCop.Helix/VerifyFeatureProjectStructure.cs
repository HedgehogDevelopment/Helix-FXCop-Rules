using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgehogDevelopment.FxCop.Helix
{
    public class VerifyFeatureProjectStructure : BaseParentTest
    {
        public VerifyFeatureProjectStructure() : base("VerifyFeatureProjectStructure")
        {
        }

        protected override string BaseNamespaceMoniker
        {
            get
            {
                return "Feature";
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
