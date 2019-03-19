using CSA.KnowledgeSources;

namespace CSA.Controllers
{
    public interface IScheduledController
    {
        /*
         * Executes the Scheduled KSs registered with this controller following the controller's control policy.  
         */
        void Execute();

        void AddKnowledgeSource(IScheduledKnowledgeSource ks);

        void RemoveKnowledgeSource(IScheduledKnowledgeSource ks);

        bool RegisteredKnowledgeSource(IScheduledKnowledgeSource ks);
    }
}
