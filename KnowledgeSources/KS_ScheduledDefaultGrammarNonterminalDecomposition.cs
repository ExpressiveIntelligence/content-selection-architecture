using System.Diagnostics;
using System.Linq;
using CSA.Core;
using static CSA.Core.KCNames;

namespace CSA.KnowledgeSources
{
    public class KS_ScheduledDefaultGrammarNonterminalDecomposition : KS_ScheduledContentPoolCollector
    {
        protected const int TargetIDForDefault = 0;

        protected override object[][] Precondition()
        {
            var decompBindings = base.Precondition();
            if (decompBindings == m_emptyBindings)
            {
                // No decompositions were found. We now want to create a default decomposition so we want to return non-empty bindings (so that Execute() will run).

                /*
                 * Grab the target id of the unit (non-terminal) we're currently expanding.
                 */
                var targetIDSet = from unit in m_blackboard.LookupUnits<Unit>()
                                  where unit.HasComponent<KC_UnitReference>() && unit.ReferenceNameEquals(CurrentTreeNodeExpansion)
                                  where unit.GetUnitReference().HasComponent<KC_IDSelectionRequest>()
                                  select unit.GetUnitReference().GetTargetUnitID();

                /*
                 * If there is a current target id, pass it in the bindings so Execute() can use it to create a default decomposition. 
                 * Otherwise fall through and return empty bindings.                 
                 */
                if (targetIDSet.Any())
                {
                    // There is a target ID

                    Debug.Assert(targetIDSet.Count() == 1); // There should be exactly one target ID.

                    object[][] bindings = new object[1][];

                    bindings[0] = new object[] { targetIDSet.First() };

                    return bindings;
                }

            }
            // Decompositions were found - return empty bindings as there is no need to generate a default decomposition.
            return m_emptyBindings;
        }

        protected override void Execute(object[] boundVars)
        {
            string targetID = (string)boundVars[TargetIDForDefault];
            Unit pseudoRule = new Unit();
            pseudoRule.AddComponent(new KC_UnitID(targetID, true)); // Create a pseudo-rule with the targetID
            Unit[] decomp = new Unit[1]; // The decomposition of the pseudo-rule will consists of a single node. 
            decomp[0] = new Unit();
            decomp[0].AddComponent(new KC_Text("#" + targetID + "#", true)); // The decomposition is a terminal with a tracery-style default exapansion
            pseudoRule.AddComponent(new KC_Decomposition(decomp, true)); // Add the decomposition to the rule. 

            // Add the default decomposition to the inputPool. 
            pseudoRule.AddComponent(new KC_ContentPool(InputPool, true));
            m_blackboard.AddUnit(pseudoRule);
        }

        protected KS_ScheduledDefaultGrammarNonterminalDecomposition(IBlackboard blackboard) : base(blackboard)
        {
            FilterConditionDel = DefaultFilterCondition;
        }

        /*
         * Is the inputPool used at all in DefaultGrammarNonterminalDecomposition?
         */
        public KS_ScheduledDefaultGrammarNonterminalDecomposition(IBlackboard blackboard, string inputPool) : base(blackboard, inputPool)
        {
        }

        /*
         * fixme: is the filter used at all in DefaultGrammarNonterminalDecomposition?
         */
        protected KS_ScheduledDefaultGrammarNonterminalDecomposition(IBlackboard blackboard, FilterCondition filter) : base(blackboard, filter)
        {
        }

        /*
         * fixme: Are the input pool and filter used at all in DefaultGrammarNonterminalDecomposition?
         */
        protected KS_ScheduledDefaultGrammarNonterminalDecomposition(IBlackboard blackboard, string inputPool, FilterCondition filter) : base(blackboard, inputPool, filter)
        {
        }
    }
}
