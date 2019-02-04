using System.Collections.Generic;

namespace CSA.KnowledgeSources
{
    public interface IKnowledgeSource
    {

        IDictionary<string, object> Properties { get; }

        // Returns activations of the knowledge source.
        IKnowledgeSourceActivation[] Precondition();

    }
}
