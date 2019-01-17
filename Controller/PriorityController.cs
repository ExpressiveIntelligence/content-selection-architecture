using KnowledgeSources;
using System.Diagnostics;

namespace Controllers
{
    public class PriorityController : Controller
    {
 
        // fixme: selects the highest priority KS for execution. More generally, would want to select KSs probabilistically based on priority.
        // Consider implementing controllers with KSs on their own blackboard. There's a regress where the KSs that implement a controller need 
        // their own meta-controller to decide what to do. This regress can be broken by having "eager" KSs that execute immediately when their 
        // preconditions are satisfied (like a forward-chaining rule). 
        protected override KnowledgeSource SelectKSForExecution()
        {
            KnowledgeSource highestPriorityKS = null;
            int highestPriority = int.MinValue;
            foreach(KnowledgeSource ks in m_Agenda)
            {
                Debug.Assert(ks.Properties.ContainsKey(KS_PropertyNames.Priority));
                int curPriority = (int)ks.Properties[KS_PropertyNames.Priority];
                if (curPriority > highestPriority)
                {
                    highestPriorityKS = ks;
                    highestPriority = curPriority;
                }
            }
            return highestPriorityKS; // returns null if there are no KSs in the agenda
        }

    }
}
