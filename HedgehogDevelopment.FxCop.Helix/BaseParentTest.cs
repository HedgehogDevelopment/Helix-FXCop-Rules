using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FxCop.Sdk;

namespace HedgehogDevelopment.FxCop.Helix
{
    public abstract class BaseParentTest : BaseHelixRule
    {
        protected abstract string ParentNamespaceMoniker { get; }

        public BaseParentTest(string name) : base(name)
        {
        }

        internal override void TestObjectValue(UniqueProblemCollection problems, SourceContext? sourceContext, string baseNamespace, object value, string messageFormat)
        {
            if (sourceContext.HasValue && !sourceContext.Value.IsValid)
            {
                return;
            }

            //Determine the type of value
            TestObjectNamespaces(value, (instructionTypeName) =>
            {
                string instructionBaseNamespace = GetNamespaceFromTypeName(instructionTypeName, ParentNamespaceMoniker);

                if (instructionBaseNamespace != null)
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
