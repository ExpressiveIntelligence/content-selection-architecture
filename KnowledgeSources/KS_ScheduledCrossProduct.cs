using System;
using System.Linq;
using System.Collections.Generic;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    public class KS_ScheduledCrossProduct : ScheduledKnowledgeSource
    {
        // Indices for source pool 1 and 2 bindings 
        protected const int SourcePool1Units = 0;
        protected const int SourcePool2Units = 1;

        /*
         * First pool for the cross product. 
         */
        public string SourcePool1 { get; }

        /*
         * Second pool for the cross product.
         */
        public string SourcePool2 { get; }

        /*
         * Output pool that results from the cross product of the two sources.
         */
        public string OutputPool { get; }


        protected override object[][] Precondition()
        {
            /*
             * First LINQ to grab all the Units in SourcePool1
             */
            var unitsFromPool1 = from unit in m_blackboard.LookupUnits<Unit>()
                                 where SelectFromPool(unit, SourcePool1)
                                 select unit;

            /*
             * Second LINQ to grab all the Units in SourcePool2
             */
            var unitsFromPool2 = from unit in m_blackboard.LookupUnits<Unit>()
                                 where SelectFromPool(unit, SourcePool2)
                                 select unit;

            if (unitsFromPool1.Any() && unitsFromPool2.Any())
            {
                var bindings = new object[1][];
                var binding = new object[2];

                binding[SourcePool1Units] = unitsFromPool1;
                binding[SourcePool2Units] = unitsFromPool2;

                bindings[0] = binding;

                return bindings;
            }
            else
            {
                return m_emptyBindings;
            }
        }

        protected override void Execute(object[] boundVars)
        {
            foreach (Unit u1 in (IEnumerable<Unit>)boundVars[SourcePool1Units])
            {
                foreach (Unit u2 in (IEnumerable<Unit>)boundVars[SourcePool2Units])
                {
                    Unit combiner = new Unit();
                    combiner.AddComponent(new KC_UnitReference(SourcePool1, true, u1));
                    combiner.AddComponent(new KC_UnitReference(SourcePool2, true, u2));
                    combiner.AddComponent(new KC_ContentPool(OutputPool, true));
                    m_blackboard.AddUnit(combiner);
                }
            }
        }

        public KS_ScheduledCrossProduct(IBlackboard blackboard, string sourcePool1, string sourcepPool2, string outputPool) : base(blackboard)
        {
            SourcePool1 = sourcePool1;
            SourcePool2 = sourcepPool2;
            OutputPool = outputPool;
        }
    }

}
