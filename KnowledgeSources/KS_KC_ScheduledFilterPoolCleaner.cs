using System.Collections.Generic;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    public class KS_KC_ScheduledFilterPoolCleaner : KS_KC_ScheduledFilterSelector
    {
        private static bool SelectFromInputPools(string[] inputPools, Unit unit)
        {
            if (unit.HasComponent<KC_ContentPool>())
            {
                foreach (string pool in inputPools)
                {
                    if (unit.ContentPoolEquals(pool))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void Execute(IDictionary<string, object> boundVars)
        {
            IEnumerable<ContentUnit> units = UnitsFilteredByPrecondition(boundVars);
            foreach (var unit in units)
            {
                m_blackboard.RemoveUnit(unit);
            }
        }

        public KS_KC_ScheduledFilterPoolCleaner(IBlackboard blackboard, string[] inputPools) :
            base(blackboard, null, (unit) => SelectFromInputPools(inputPools, unit))
        {
        }
    }
}
