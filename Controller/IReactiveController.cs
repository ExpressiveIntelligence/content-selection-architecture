using CSA.KnowledgeSources;
using System.Collections.Generic;

namespace CSA.Controllers
{
    // fixme: stub. Fill in once I've figured out more general patterns for controllers.
    public interface IReactiveController
    {
        // Performs any initialization of the controller that needs to happen outside of adding the knowledge sources.
        // fixme: probably doesn't need to be public and can be removed. 
        void Initialize();

        // Starts the controller executing. 
        void Execute();

        void AddKnowledgeSource(IReactiveKnowledgeSource ksToAdd);

        void RemoveKnowledgeSource(IReactiveKnowledgeSource ksToRemove);

        ISet<IReactiveKnowledgeSource> ActiveKSs { get; }

        ISet<IKnowledgeSourceActivation> Agenda { get; }
    }
}
