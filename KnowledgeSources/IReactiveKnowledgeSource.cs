using System;
using System.Collections.Generic;

namespace CSA.KnowledgeSources
{
    [Obsolete("Need a new implementation of reactive knowledge sources. Use ScheduledKnowledgeSources for now.")]
    public interface IReactiveKnowledgeSource
    {

        IDictionary<string, object> Properties { get; }

        // Returns activations of the knowledge source.
        IKnowledgeSourceActivation[] Precondition();

    }
}
