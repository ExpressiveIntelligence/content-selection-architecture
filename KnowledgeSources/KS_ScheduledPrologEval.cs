using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{

    public class KS_ScheduledPrologEval : KS_ScheduledFilterSelector
    {
        // Binding index for the prolog knowledge base. 
        public const int PrologKB = 1;

        public const string DefaultOutputPoolName = "PrologQueryEvaluted";

        // Name of the prolog expression evaluated by this instance of KS_ScheduledPrologEval.
        public string PrologExpressionName { get; }

        /*
         * Convenience utility for generating a filter method which filters by the prolog evaluation being true or false (== to the param result). 
         * Can be used by other KSs that want to filter these results.         
         */
        public static FilterCondition FilterByPrologResult(string prologExpName, bool result)
        {
            return (Unit unit) => unit.HasComponent<KC_EvaluatablePrologExpression>() 
                && unit.GetPrologExpName<KC_EvaluatablePrologExpression>().Equals(prologExpName) &&
                unit.GetPrologExpEvaluated() && (unit.GetPrologExpEvalResult() == result);
        }

        /*
         * Given an expression name, returns a FilterCondition that filters Units based on whether they have a KC_PrologExpression of the appropriate name. 
         */
        protected static FilterCondition GeneratePrologExpFilter(string expName, IBlackboard blackboard)
        {
            // For non-null expression names, test the name, otherwise ignore the name. 
            if (expName != null)
            {
                return (Unit unit) =>
                    unit.HasComponent<KC_PrologExpression>() &&
                    unit.GetPrologExpName<KC_PrologExpression>().Equals(expName) &&
                    !HasLinkToEvaluatedExpression(unit, blackboard);
            }
            return GenerateHasComponent<KC_PrologExpression>();
        }

        /*
         * Returns true if the KC_PrologExpression on the unit has already been evaluated, false otherwise.        
         * Given a unit containing a KC_PrologExpression and a blackboard, returns true if there is a 
         * L_SelectedUnitLink to a Unit with a KC_EvaluatablePrologExpression matching the KC_PrologExpression. 
         */
        protected static bool HasLinkToEvaluatedExpression(Unit unit, IBlackboard blackboard)
        {
            ISet <(IUnit, string, LinkDirection)> links = blackboard.LookupLinks(unit);
            foreach((IUnit linkedIUnit, string type, LinkDirection dir) in links)
            {
                /*
                 * There should only be up to 1 outgoing L_SelectedUnit link, but since I'm not enforcing this on the blackboard, 
                 * for now testing all L_SelectedUnit links until I find a matching KC_EvaluatablePrologExpression                
                 */                
                if (type.Equals(LinkTypes.L_SelectedUnit) && dir == LinkDirection.End)
                {
                    Unit linkedUnit = linkedIUnit as Unit;

                    // If the linkedUnit contains a matching, evaluated, KC_EvaluatablePrologExpression, return true.
                    if (linkedUnit.HasComponent<KC_EvaluatablePrologExpression>())
                    {
                        KC_EvaluatablePrologExpression evaledPrologExp = linkedUnit.GetComponent<KC_EvaluatablePrologExpression>();
                        if (evaledPrologExp.PrologExpName.Equals(unit.GetPrologExpName<KC_PrologExpression>()) && evaledPrologExp.Evaluated) {
                            Debug.Assert(evaledPrologExp.PrologExp.Equals(unit.GetPrologExp()));
                            return true;
                        }
                    }
                }
            }
            return false; // No linked, matching KC_EvaluatablePrologExpression was found. 
        }

        /*
         * The precondition binds the units with an appropriately named prolog expression in the input pool and the 
         * global prolog knowledge base.         
         */
        protected override object[][] Precondition()
        {
            var prologExpBinding = base.Precondition();
            if (prologExpBinding.Any())
            {
                // Units with the appropriately named prolog expression were found in the input pool.

                // There should be exactly one set of units. 
                Debug.Assert(prologExpBinding.Length == 1);

                // Lookup the prolog database. 
                var prologKBQuery = from unit in m_blackboard.LookupUnits<Unit>()
                                    where unit.HasComponent<KC_PrologKB>()
                                    select unit;

                // Continue making bindings if there is a prologKB - otherwise return the empty binding.
                if (prologKBQuery.Any())
                {

                    // There should be one prolog KB (currently only handling a global KB)
                    Debug.Assert(prologKBQuery.Count() == 1);

                    object[][] newBindings = new object[1][];

                    // Add the prologKB to the prologExp binding
                    newBindings[0] = new object[] { prologExpBinding[0][FilteredUnits], prologKBQuery.First() }; 

                    return newBindings;
                }
            }
            // Either no Units with appropriately named prolog exp found or no prolog KB found
            return m_emptyBindings;
        }

        /*
         * Copy all the units bound by the precondition into the OutputPool adding a KC_EvaluatablePrologExpression and evaluate the expression.
         */
        protected override void Execute(object[] boundVars)
        {
            // Get the units and the prolog KB from the precondition bindings.
            var units = (IEnumerable<Unit>)boundVars[FilteredUnits];
            Unit prologKB = (Unit)boundVars[PrologKB];

            // Copy each of the units to the output pool, adding a KC_EvaluatablePrologExpression and evaluating it. 
            foreach(var unit in units)
            {
                Unit unitCopy = CopyUnitToOutputPool(unit);

                // fixme: ignoring the name. When the Unit component infrastructure is updated to handle multiple named components, change the call here. 
                KC_PrologExpression expToEvaluate = unitCopy.GetComponent<KC_PrologExpression>();

                // Remove the KC_PrologExpression
                unitCopy.RemoveComponent(expToEvaluate);

                // Create a KC_EvaluatablePrologExpression based on expToEvaluate.
                KC_EvaluatablePrologExpression evaluatablePrologExp = new KC_EvaluatablePrologExpression(expToEvaluate);

                // Add the KC_EvaluatablePrologExpression to the Unit
                unitCopy.AddComponent(evaluatablePrologExp);

                /*
                 * Evaluate the prolog expression.
                 * The original unit now being linked (via CopyUnitToOutputPool) to a unit with a matching and evaled KC_EvaluatablePrologExpression
                 * is what will keep this unit from repeatedly matching in the precondition. The precondition is testing !HasLinkToEvaluatedExpression
                 * as a filter condition. 
                 */                
                evaluatablePrologExp.Evaluate(prologKB.GetComponent<KC_PrologKB>());
            }
        }

        public KS_ScheduledPrologEval(IBlackboard blackboard, string prologExpName) : 
            base(blackboard, DefaultOutputPoolName, GeneratePrologExpFilter(prologExpName, blackboard))
        {
            PrologExpressionName = prologExpName;
        }

        public KS_ScheduledPrologEval(IBlackboard blackboard, string outputPool, string prologExpName) : 
            base(blackboard, outputPool, GeneratePrologExpFilter(prologExpName, blackboard))
        {
            PrologExpressionName = prologExpName;
        }

        public KS_ScheduledPrologEval(IBlackboard blackboard, string inputPool, string outputPool, string prologExpName) : 
            base(blackboard, inputPool, outputPool, (Unit u) => SelectFromPool(u, inputPool) && GeneratePrologExpFilter(prologExpName, blackboard)(u))
        {
            PrologExpressionName = prologExpName;
        }

        public KS_ScheduledPrologEval(IBlackboard blackboard, string outputPool, FilterCondition filter, string prologExpName) : 
            base(blackboard, outputPool, (Unit u) => filter(u) && GeneratePrologExpFilter(prologExpName, blackboard)(u))
        {
            PrologExpressionName = prologExpName;
        }
    }
}
