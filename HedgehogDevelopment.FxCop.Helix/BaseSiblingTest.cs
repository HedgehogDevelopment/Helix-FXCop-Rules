using Microsoft.FxCop.Sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgehogDevelopment.FxCop.Helix
{
    public abstract class BaseSiblingTest : BaseHelixRule
    {
        public BaseSiblingTest(string name) : base(name)
        {
#if DEBUG
            Debugger.Launch();
#endif
        }


        /// <summary>
        /// Checks an object to see if it uses the wrong namespace
        /// </summary>
        /// <param name="problems"></param>
        /// <param name="sourceContext"></param>
        /// <param name="baseNamespace"></param>
        /// <param name="value"></param>
        internal override void TestObjectValue(UniqueProblemCollection problems, SourceContext? sourceContext, string baseNamespace, object value, string messageFormat)
        {
            if (sourceContext.HasValue && !sourceContext.Value.IsValid)
            {
                return;
            }

            //Determine the type of value
            TestObjectNamespaces(value, (instructionTypeName) =>
            {
                string instructionBaseNamespace = GetNamespaceFromTypeName(instructionTypeName, BaseNamespaceMoniker);

                if (instructionBaseNamespace != null && instructionBaseNamespace != baseNamespace)
                {
                    Resolution res = new Resolution("Using invalid namespace", messageFormat, baseNamespace, instructionBaseNamespace);

                    if (sourceContext.HasValue)
                    {
                        problems.Add(new Problem(res, sourceContext.Value));
                    }
                    else
                    {
                        problems.Add(new Problem(res));
                    }
                }
            });
        }
    }
}
