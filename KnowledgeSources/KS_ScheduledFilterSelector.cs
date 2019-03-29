using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CSA.Core;
using static CSA.KnowledgeUnits.CUSlots;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    public class KS_ScheduledFilterSelector : ScheduledKnowledgeSource
    {

        // Name of the bound activation variable
        protected const string FilteredContentUnits = "FilteredContentUnits";

        // Input and output pools for this filter. 
        public string InputPool { get; }
        public string OutputPool { get; }

        // Define the type for the FilterCondition delegate
        public delegate bool FilterCondition(ContentUnit cu);

        // Define the field that stores a FilterCondition delegate
        protected FilterCondition FilterConditionDel;

        // The default filter condition does no filtering. 
        public static bool DefaultFilterCondition(ContentUnit contentUnit)
        {
            return true;
        }

        public static bool SelectFromPool(ContentUnit contentUnit, string inputPool)
        {
            return contentUnit.HasMetadataSlot(ContentPool) && contentUnit.Metadata[ContentPool].Equals(inputPool);
        }

        private bool SelectFromPool(ContentUnit contentUnit)
        {
            return SelectFromPool(contentUnit, InputPool);
        }

        protected override IDictionary<string, object>[] Precondition()
        {
            var contentUnits = from contentUnit in m_blackboard.LookupUnits<ContentUnit>()
                               where FilterConditionDel(contentUnit)
                               select contentUnit;

            if (contentUnits.Any())
            {
                // There is at least one CU passing the conditions 


                // The binding contains the enumeration of all the CUs that pass the filter condition. 
                // fixme: Execute will copy all the ContentUnits in the enumeration into the output pool. 
                var bindings = new IDictionary<string, object>[1];

                bindings[0] = new Dictionary<string, object>
                {
                    [FilteredContentUnits] = contentUnits
                };
  
                return bindings;
            }
            else
            {
                // No CUs matching the conditions in the InputPool - return empty bindings (length 0). 
                return m_emptyBindings;
            }
        }

        protected ContentUnit CopyCUToOutputPool(ContentUnit contentUnit)
        {
            ContentUnit newUnit = new ContentUnit(contentUnit);
            newUnit.Metadata[ContentPool] = OutputPool;
            m_blackboard.AddUnit(newUnit);
            m_blackboard.AddLink(contentUnit, newUnit, LinkTypes.L_SelectedContentUnit, true); // fixme: need a more general link type for copies between pools
            return newUnit;
        }

        protected IEnumerable<ContentUnit> ContentUnitsFilteredByPrecondition(IDictionary<string, object> boundVars)
        {
            return (IEnumerable<ContentUnit>)boundVars[FilteredContentUnits];
        }

        /*
         * On Execute(), the abstract FilterSelector copys the filtered CUs from the input pool to the output pool.  
         */
        protected override void Execute(IDictionary<string, object> boundVars)
        {
            var contentUnits = ContentUnitsFilteredByPrecondition(boundVars);
            foreach(ContentUnit contentUnit in contentUnits)
            {
                CopyCUToOutputPool(contentUnit);
            }
        }

        public KS_ScheduledFilterSelector(IBlackboard blackboard, string outputPool) : base(blackboard)
        {
            OutputPool = outputPool;
            FilterConditionDel = DefaultFilterCondition;
        }

        public KS_ScheduledFilterSelector(IBlackboard blackboard, string inputPool, string outputPool) : base(blackboard)
        {
            InputPool = inputPool;
            OutputPool = outputPool;
            FilterConditionDel = SelectFromPool;
        }

        public KS_ScheduledFilterSelector(IBlackboard blackboard, string outputPool, FilterCondition filter) : base(blackboard)
        {
            Debug.Assert(filter != null);

            OutputPool = outputPool;
            FilterConditionDel = filter;
        }

         /*
         * ScheduledFilterSelector constructed with both an input pool and a filter specified using the conjunction of SelectFromPool and filter 
         * as the FilterConditionDel.         
         */
        public KS_ScheduledFilterSelector(IBlackboard blackboard, string inputPool, string outputPool, FilterCondition filter) : base(blackboard)
        {
            Debug.Assert(filter != null);
            Debug.Assert(inputPool != null);

            InputPool = inputPool;
            OutputPool = outputPool;
            FilterConditionDel = (ContentUnit cu) => SelectFromPool(cu) && filter(cu);
        }
    }
}
