using System;
using System.Collections.Generic;

namespace CSA.KnowledgeSources
{
    [Obsolete("Need a new implemention of reactive knowledge sources. Use ScheduledKnowledgeSources for now.")]
    public interface IKnowledgeSourceActivation
    {
        // Properties of the knowledge source associated with this knowledge source activation
        IDictionary<string, object> Properties { get; }

        // Execute the activated knowledge source.
        void Execute();

        // Evaluate the activated obviation condition. 
        bool EvaluateObviationCondition();
    }
}
