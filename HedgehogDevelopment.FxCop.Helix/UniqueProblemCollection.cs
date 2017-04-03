using Microsoft.FxCop.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HedgehogDevelopment.FxCop.Helix
{
    /// <summary>
    /// Maintains a list of unique problems
    /// </summary>
    class UniqueProblemCollection
    {
        /// <summary>
        /// Compares two problems to see if they are at the same location
        /// </summary>
        class ProblemSourceLocationComparer : IEqualityComparer<Problem>
        {
            public string File { get; set; }
            public int Line { get; set; }

            public bool Equals(Problem x, Problem y)
            {
                if (x.SourceFile == null || y.SourceFile == null)
                {
                    return false;
                }

                return x.SourceFile == y.SourceFile && x.SourceLine == y.SourceLine && x.Resolution.Format == y.Resolution.Format;
            }

            public int GetHashCode(Problem obj)
            {
                if (obj.SourceFile == null)
                {
                    return obj.GetHashCode();
                }

                return obj.SourceLine.GetHashCode() ^ obj.SourceFile.GetHashCode() ^ obj.Resolution.Format.GetHashCode();
            }
        }

        public ProblemCollection Problems { get; private set; }
        HashSet<Problem> _uniqueSourceLocations = new HashSet<Problem>(new ProblemSourceLocationComparer());

        public UniqueProblemCollection()
        {
            Problems = new ProblemCollection();
        }

        public void Add(Problem problem)
        {
            if (!_uniqueSourceLocations.Contains(problem))
            {
                _uniqueSourceLocations.Add(problem);
                Problems.Add(problem);
            }
        }
    }
}
