using System.Collections.Generic;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    /*
     * Computes the cosine similarity between a specified query vector and each Unit in the input pool that has a KC_FloatVector. Each of the units copied into the
     * output pool has a cosine similarity KC_Double.
     */
    // fixme: conside making a KS_ScheduledQuery class of which KS_ScheduledCosineSimilarity is a subtype.
    public class KS_ScheduledCosineSimilarity : KS_ScheduledFilterSelector
    {
        /*
         * Initializing the enumerator of unique output pool names (static) and the initialization of the DefaultOutputPoolName (instance).
         */
        private static readonly IEnumerator<string> m_OutputPoolNameEnumerator = OutputPoolNameEnumerator("CosineSimilarityComputed");
        public override string DefaultOutputPoolName { get; } = GenDefaultOutputPoolName(m_OutputPoolNameEnumerator);

        /*
         * Precondition: similar to KS_ScheduledPrologEval, call the base precondition to get a collection of all the Units with a KC_FloatVector (with a
         * given name?), then get an active Vector request (maybe handle this similarly to the way KS_ScheduledIDSelector handles it?). It is more like KS_ScheduledIDSelector
         * in that some query information is needed, vs. expression evaluatio where it's just the presence of the expression that makes the difference.
         * Perhaps refactor ActiveRequest field in KC_IDSelectionRequest into a separate KnowledgeComponent, then group them on a unit. Then you'd look for queries by finding
         * a conjunction (has an ActiveRequest flag and contains a KC_FloatVector, or something like that). Ack, for now just create a QueryVectorRequest and call it good. 
         */

        /*
         * Execute: Call cosine similarity for each Unit filtered by the precondition and store the result in a KC_Similarity double in the output pool. 
         */

        public KS_ScheduledCosineSimilarity(IBlackboard blackboard) : base(blackboard)
        {
        }
    }
}
