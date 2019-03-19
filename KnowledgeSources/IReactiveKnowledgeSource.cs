using System.Collections.Generic;

namespace CSA.KnowledgeSources
{
    public interface IReactiveKnowledgeSource
    {

        IDictionary<string, object> Properties { get; }

        // Returns activations of the knowledge source.
        IKnowledgeSourceActivation[] Precondition();

    }
}
