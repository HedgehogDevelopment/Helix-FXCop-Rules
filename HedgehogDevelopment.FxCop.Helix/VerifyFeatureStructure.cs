using Microsoft.FxCop.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgehogDevelopment.FxCop.Helix
{
    public class VerifyFeatureStructure : BaseSiblingTest
    {
        public VerifyFeatureStructure() : 
            base("VerifyFeatureStructure")
        {
        }

        protected override string BaseNamespaceMoniker
        {
            get
            {
                return "Feature";
            }
        }
    }
}
