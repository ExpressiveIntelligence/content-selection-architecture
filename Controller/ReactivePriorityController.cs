﻿using System;
using CSA.KnowledgeSources;
using static CSA.KnowledgeSources.KSProps;
using System.Diagnostics;

namespace CSA.Controllers
{
    [Obsolete("Have stopped developing reactive controller and KS infrastructure. Need to come up with new design for reactive controllers.")]
    public class ReactivePriorityController : ReactiveController
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
                Debug.Assert(KSA.Properties.ContainsKey(Priority));
                int curPriority = (int)KSA.Properties[Priority];
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
