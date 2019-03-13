using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CSA.Core;
using static CSA.KnowledgeUnits.CUSlots;

namespace CSA.KnowledgeSources
{
    public abstract class KS_FilterSelector : KnowledgeSource
    {

        // Name of the bound activation variable
        private const string FilteredContentUnits = "FilteredContentUnits";

        // Input and output pools for 
        public string InputPool { get; }
        public string OutputPool { get; }

        public override IKnowledgeSourceActivation[] Precondition()
        {
            var CUs = from ContentUnit cu in m_blackboard.LookupUnits(ContentUnit.TypeName)
                      where cu.HasMetadataSlot(ContentPool)
                      where cu.Metadata[ContentPool] == (object)InputPool
                      where FilterCondition(cu)
                      select cu;

            if (CUs.Count() > 0)
            {
                // There is at least one CU in the InputPool 

                /*
                 * Test to see if this collection of CUs has been previously matched by this knowledge source. If so, don't match again and 
                 * return the empty activation array (length 0).             
                 */
                Debug.Assert((m_previousMatchSets.Count == 0) || (m_previousMatchSets.Count == 1)); // There should never be more than one previous match set for Selectors
                foreach (HashSet<IUnit> set in m_previousMatchSets)
                {
                    if (set.SetEquals(CUs))
                    {
                        return m_emptyActivations;
                    }
                }

                // If it gets this far, then a unique collection of CUs has been matched. Create an activation and return it. 

                // The activation contains the enumeration of all the CUs that pass the filter condition. 
                // Execute will copy all the ContentUnits in the enumeration into the output pool. 
                IKnowledgeSourceActivation[] activations = new KnowledgeSourceActivation[1];

                var boundVars = new Dictionary<string, object>
                {
                    [FilteredContentUnits] = CUs
                };

                activations[0] = new KnowledgeSourceActivation(this, boundVars);

                // Add a match to the previousMatchSets to prevent rematching. 
                m_previousMatchSets.Add(new HashSet<IUnit>(CUs));

                return activations;
            }
            else
            {
                // No CUs in the InputPool - return empty activations (length 0). 
                return m_emptyActivations;
            }
        }

        /*
         * The abstract FilterSelector obviates an activation if the filtered set of CUs in the input pool changes from the matched set. 
         */
        internal override bool EvaluateObviationCondition(IDictionary<string, object> boundVars)
        {
            var CUs = from ContentUnit cu in m_blackboard.LookupUnits(ContentUnit.TypeName)
                      where cu.HasMetadataSlot(ContentPool)
                      where cu.Metadata[ContentPool] == (object)InputPool
                      where FilterCondition(cu)
                      select cu;

            // Construct a HashSet out of the IEnumerable so that we can do a SetEquals() test. 
            HashSet<ContentUnit> currentMatchSet = new HashSet<ContentUnit>(CUs);
            IEnumerable<ContentUnit> previousMatchSet = (IEnumerable<ContentUnit>)boundVars[FilteredContentUnits];

            /*
             * fixme: this actually doesn't cut the mustard. If all the CUs are the same, but the *metadata* of one of them has been changed, 
             * then then this activation should be obviated. In this case it could be fixed by, after testing set equality, retesting the filter
             * condition on each CU in the match. Verify that this breaks in the test rig, then fix it here. 
             */
            if (!currentMatchSet.SetEquals(previousMatchSet))
            {
                /* The InputPool has changed! Clear m_previousMatchSets and return true. Clearing is OK because there will never be more than one
                 * previous match set. 
                 */
                m_previousMatchSets.Clear();
                return true;
            }
            else
            {
                return false; 
            }
        }

        /*
         * On Execute(), the abstract FilterSelector copys the filtered CUs from the input pool to the output pool.  
         */
        internal override void Execute(IDictionary<string, object> boundVars)
        {
            IEnumerable<ContentUnit> CUs = (IEnumerable<ContentUnit>)boundVars[FilteredContentUnits];
            foreach(ContentUnit cu in CUs)
            {
                ContentUnit cuCopy = new ContentUnit(cu);
                cuCopy.Metadata[ContentPool] = OutputPool;
                m_blackboard.AddUnit(cuCopy);
            }
        }

        /*
         * FilterCondition() is defined on subclasses to provide a ContentUnit test. Returns true if the ContentUnit satisfies the test, false otherwise.
         */
        protected abstract bool FilterCondition(ContentUnit cu);

        public KS_FilterSelector(IBlackboard blackboard, string inputPool, string outputPool) : base(blackboard)
        {
            InputPool = inputPool;
            OutputPool = outputPool;
        }

     }
}
