using System.Collections.Generic;

namespace CSA.KnowledgeSources
{
    public interface IKnowledgeSource
    {
        // fixme: remove
        // bool Executable { get; }

        IDictionary<string, object> Properties { get; }

        // Returns activations of the knowledge source.
        IKnowledgeSourceActivation[] Precondition();

        // fixme: delete once we've figured out that we don't need access to these outside of this project 
        // bool EvaluateObviationCondition();

        // void Execute();
    }
}
