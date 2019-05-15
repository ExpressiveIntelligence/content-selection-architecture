using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    /*
     * KS_ContentPoolCollector is an abstract class that provides functionality for collecting the Units in a content pool, potentially with additional filtering logic applied. 
     * KS_ScheduledFilterSelector is a subclass which adds additional functionality for copying the collected Units to an output pool. In general, filter knowledge sources will
     * inherit from KS_ScheduledFilterSelector while knowledge sources that perform some kind of non-copying processing (such as KS_ChoicePresenter or KS_ProcessTreeNode) will
     * inherit from KS_ContentPoolCollector.     
     */
    public abstract class KS_KC_ScheduledContentPoolCollector : ScheduledKnowledgeSource
    {
        // Name of the bound activation variable
        protected const string FilteredUnits = "FilteredUnits";

        // Input pool for this filter 
        public string InputPool { get; }

        // Define the type for the FilterCondition delegate
        public delegate bool FilterCondition(Unit unit);

        // Define the field that stores a FilterCondition delegate
        protected FilterCondition FilterConditionDel;

        // The default filter condition does no filtering. 
        public static bool DefaultFilterCondition(Unit _)
        {
            return true;
        }

        public static bool SelectFromPool(Unit unit, string inputPool)
        {
            return unit.HasComponent<KC_ContentPool>() && unit.ContentPoolEquals(inputPool);
        }

        private bool SelectFromPool(Unit unit)
        {
            return SelectFromPool(unit, InputPool);
        }

        public static FilterCondition GenerateHasComponent<T>() where T : KnowledgeComponent
        {
            return (Unit unit) => unit.HasComponent<T>();
        }

        protected override IDictionary<string, object>[] Precondition()
        {
            var units = from unit in m_blackboard.LookupUnits<Unit>()
                        where FilterConditionDel(unit)
                        select unit;

            if (units.Any())
            {
                // There is at least one unit passing the conditions 


                // The binding contains the enumeration of all the units that pass the filter condition. 
                // fixme: Execute will copy all the units in the enumeration into the output pool. 
                var bindings = new IDictionary<string, object>[1];

                bindings[0] = new Dictionary<string, object>
                {
                    [FilteredUnits] = units
                };

                return bindings;
            }
            else
            {
                // No units matching the conditions in the InputPool - return empty bindings (length 0). 
                return m_emptyBindings;
            }
        }

        // Return an enumerable containing the units bound by the precondition.
        // fixme: remove when the Dictionary has been replaced with an array. 
        protected IEnumerable<Unit> UnitsFilteredByPrecondition(IDictionary<string, object> boundVars)
        {
            return (IEnumerable<Unit>)boundVars[FilteredUnits];
        }

        protected Unit FindOriginalUnit(Unit unit)
        {
            return FindOriginalUnit(unit, m_blackboard);
        }

        public static Unit FindOriginalUnit(Unit unit, IBlackboard blackboard)
        {
            var linkToPreviousUnitsInFilterChain = from link in blackboard.LookupLinks(unit)
                                                   where link.LinkType.Equals(LinkTypes.L_SelectedUnit)
                                                   where link.Direction.Equals(LinkDirection.Start)
                                                   select link;

            int count = linkToPreviousUnitsInFilterChain.Count();

            // There should be 0 (if we've reached the original) or 1 (if we're still crawling back up the filter chain) links.
            Debug.Assert(count == 0 || count == 1);

            if (count == 0)
            {
                return unit; // The passed in CU was the parent of a chain.
            }
            else
            {
                // Recursively search back up the filter chain
                (IUnit previousUnitInChain, _, _) = linkToPreviousUnitsInFilterChain.ElementAt(0);
                return FindOriginalUnit((Unit)previousUnitInChain, blackboard);
            }
        }

        protected KS_KC_ScheduledContentPoolCollector(IBlackboard blackboard) : base(blackboard)
        {
            FilterConditionDel = DefaultFilterCondition;
        }

        protected KS_KC_ScheduledContentPoolCollector(IBlackboard blackboard, string inputPool) : base(blackboard)
        {
            InputPool = inputPool ?? throw new ArgumentException("Null inputPool passed to constructor for KS_ContentPoolCollector");
            FilterConditionDel = SelectFromPool;
        }

        protected KS_KC_ScheduledContentPoolCollector(IBlackboard blackboard, FilterCondition filter) : base(blackboard)
        {
            FilterConditionDel = filter ?? throw new ArgumentException("Null filter passed to constructor for KS_ContentPoolCollector");
        }

        protected KS_KC_ScheduledContentPoolCollector(IBlackboard blackboard, string inputPool, FilterCondition filter) : base(blackboard)
        {
            if (filter == null)
            {
                throw new ArgumentException("Null filter passed to constructor for KS_ContentPoolCollector");
            }

            InputPool = inputPool ?? throw new ArgumentException("Null inputPool passed to constructor for KS_ContentPoolCollector");
            FilterConditionDel = (Unit unit) => SelectFromPool(unit) && filter(unit);
        }
    }
}
