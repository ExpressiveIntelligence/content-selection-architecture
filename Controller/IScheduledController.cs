using CSA.KnowledgeSources;

namespace CSA.Controllers
{
    public interface IScheduledController
    {
        /*
         * Executes the Scheduled KSs registered with this controller following the controller's control policy.  
         */
        void Execute();

        // fixme: not sure yet what common methods I'll want on IScheduledController. Commented these out so that I don't
        // have to define them for CFGExpansionController. 
        // void AddKnowledgeSource(IScheduledKnowledgeSource ks);

        // void RemoveKnowledgeSource(IScheduledKnowledgeSource ks);

        // bool RegisteredKnowledgeSource(IScheduledKnowledgeSource ks);
    }
}
