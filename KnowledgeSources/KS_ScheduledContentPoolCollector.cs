using System;
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
    public abstract class KS_ScheduledContentPoolCollector : ScheduledKnowledgeSource
    {
        // Index for FilteredUnits binding.
        protected const int FilteredUnits = 0;

        // Input pool for this filter 
        public string InputPool { get; }

        // Define the type for the FilterCondition delegate
        public delegate bool FilterCondition(Unit unit);

        // Define the field that stores a FilterCondition delegate
        protected FilterCondition FilterConditionDel;

        // The default filter condition does no filtering. 
        public static bool DefaultFilterCondition(Unit _) => true;
 
        private bool SelectFromPool(Unit unit)
        {
            return SelectFromPool(unit, InputPool);
        }

        public static FilterCondition GenerateHasComponent<T>() where T : KnowledgeComponent
        {
            return (Unit unit) => unit.HasComponent<T>();
        }

        protected override object[][] Precondition()
        {
            var units = from unit in m_blackboard.LookupUnits<Unit>()
                        where FilterConditionDel(unit)
                        select unit;

            if (units.Any())
            {
                // There is at least one unit passing the conditions 


                // The binding contains the enumeration of all the units that pass the filter condition. 
                object[][] bindings = new object[1][];
                bindings[0] = new object[] { units };

                return bindings;
            }
            else
            {
                // No units matching the conditions in the InputPool - return empty bindings (length 0). 
                return m_emptyBindings;
            }
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

        protected KS_ScheduledContentPoolCollector(IBlackboard blackboard) : base(blackboard)
        {
            FilterConditionDel = DefaultFilterCondition;
        }

        protected KS_ScheduledContentPoolCollector(IBlackboard blackboard, string inputPool) : base(blackboard)
        {
            if (inputPool != null)
            {
                InputPool = inputPool;
                FilterConditionDel = SelectFromPool;
            }
            else
            {
                FilterConditionDel = DefaultFilterCondition;
            }
        }

        protected KS_ScheduledContentPoolCollector(IBlackboard blackboard, FilterCondition filter) : base(blackboard)
        {
            FilterConditionDel = filter ?? DefaultFilterCondition;
        }

        protected KS_ScheduledContentPoolCollector(IBlackboard blackboard, string inputPool, FilterCondition filter) : base(blackboard)
        {
            if (inputPool != null)
            {
                InputPool = inputPool;
                FilterConditionDel = filter != null ? ((Unit unit) => SelectFromPool(unit) && filter(unit)) : filter;
            }
            else
            {
                FilterConditionDel = filter ?? DefaultFilterCondition;
            }
        }
    }
}
