using System.Collections.Generic;
using CSA.KnowledgeSources;

namespace CSA.Controllers
{
    public class ScheduledSequenceController : IScheduledController
    {

        protected readonly IList<IScheduledKnowledgeSource> knowledgeSources;
                
        public void Execute()
        {
            foreach(IScheduledKnowledgeSource ks in knowledgeSources)
            {
                ks.Execute();
            }
        }

        public void AddKnowledgeSource(IScheduledKnowledgeSource ks)
        {
            knowledgeSources.Add(ks);
        }

        public void RemoveKnowledgeSource(IScheduledKnowledgeSource ks)
        {
            knowledgeSources.Remove(ks);
        }

        public bool RegisteredKnowledgeSource(IScheduledKnowledgeSource ks)
        {
            return knowledgeSources.Contains(ks);
        }

        public ScheduledSequenceController()
        {
            knowledgeSources = new List<IScheduledKnowledgeSource>();
        }
    }
}
