using CSA.Core;

namespace CSA.KnowledgeSources
{
    /*
     * KS_CopySelector is a KS_FilterSelector that just copies the InputPool to the OutputPool.
     */
    public class KS_CopySelector : KS_FilterSelector
    {
        /*
         * FilterCondition always returns true, so every ContentUnit in the input pool meets the condition. 
         */
        protected override bool FilterCondition(ContentUnit cu)
        {
            return true; 
        }

        public KS_CopySelector(IBlackboard blackboard, string inputPool, string outputPool) : base(blackboard, inputPool, outputPool)
        {
        }
    }
}
