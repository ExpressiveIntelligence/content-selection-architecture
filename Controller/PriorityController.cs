using CSA.KnowledgeSources;
using System.Diagnostics;

namespace CSA.Controllers
{
    public class PriorityController : Controller
    {
 
        // fixme: selects the highest priority KSA for execution. More generally, would want to select KSs probabilistically based on priority.
        // Consider implementing controllers with KSAs on their own blackboard. There's a regress where the KSs that implement a controller need 
        // their own meta-controller to decide what to do. This regress can be broken by having "eager" KSs that execute immediately when their 
        // preconditions are satisfied (like a forward-chaining rule). 
        protected override IKnowledgeSourceActivation SelectKSForExecution()
        {
            IKnowledgeSourceActivation highestPriorityKSA = null;
            int highestPriority = int.MinValue;
            foreach(IKnowledgeSourceActivation KSA in m_Agenda)
            { 
                Debug.Assert(KSA.Properties.ContainsKey(KS_PropertyNames.Priority));
                int curPriority = (int)KSA.Properties[KS_PropertyNames.Priority];
                if (curPriority > highestPriority)
                {
                    highestPriorityKSA = KSA;
                    highestPriority = curPriority;
                }
            }
            return highestPriorityKSA; // returns null if there are no KSs in the agenda
        }

    }
}
