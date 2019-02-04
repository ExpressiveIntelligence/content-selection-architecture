using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSA.KnowledgeSources;

namespace CSA.Controllers
{
    public abstract class Controller : IController
    {

        // fixme: for now have the KSs live in data structures inside the controller. Consider moving them onto the blackboard. 
        protected readonly ISet<IKnowledgeSource> m_ActiveKSs;
        protected readonly ISet<IKnowledgeSourceActivation> m_Agenda;

        // Return a copy of the ActiveKSs so that the caller can't directly modify the set. 
        public ISet<IKnowledgeSource> ActiveKSs => new HashSet<IKnowledgeSource>(m_ActiveKSs);

        // Return a copy of the agenda so that the caller can't directly modify the set.
        public ISet<IKnowledgeSourceActivation> Agenda => new HashSet<IKnowledgeSourceActivation>(m_Agenda);

        // To update the agenda, execute obviation conditions to remove KSs from the agenda and execute preconditions to add KSs to the agenda.
        protected void UpdateAgenda()
        {
            IEnumerable<IKnowledgeSourceActivation> invalidKSAs = m_Agenda.Where(ksa => ksa.EvaluateObviationCondition());

            // In the event that m_Agenda has the same elements as invalidKSAs (ie. all the obviation conditions evaluate to true), it looks like Where() is doing an optimization where it just 
            // aliases m_Agenda. But then this causes an InvalidOperationException because you're modifying a set while you enumerate over it. So need to make a copy, which is what new List() is doing. 
            m_Agenda.ExceptWith(new List<IKnowledgeSourceActivation>(invalidKSAs));

            // fixme: remove
            /* ISet<IKnowledgeSourceActivation> ksToRemoveFromAgenda = new HashSet<IKnowledgeSourceActivation>(); 
            foreach(IKnowledgeSourceActivation ksa in m_Agenda)
            {
                if (ksa.EvaluateObviationCondition())
                {
                    ksToRemoveFromAgenda.Add(ksa);
                }
            } */

            // fixme: remove 
            /*
            foreach (var ksa in invalidKSAs)
            {
                m_Agenda.Remove(ksa);
            }
            */

            foreach (IKnowledgeSource ks in m_ActiveKSs)
            {
                IEnumerable<IKnowledgeSourceActivation> KSAs = ks.Precondition();
                foreach(IKnowledgeSourceActivation ksa in KSAs)
                {
                    m_Agenda.Add(ksa);
                }
            }
        }

        public void AddKnowledgeSource(IKnowledgeSource ks) => m_ActiveKSs.Add(ks);

        public void RemoveKnowledgeSource(IKnowledgeSource ks) => m_ActiveKSs.Remove(ks);

        protected abstract IKnowledgeSourceActivation SelectKSForExecution();

        // Any initialization of the controller that needs to happen when it is constructed. 
        // fixme: if I don't find a use case for a controller that needs this, remove it. 
        // fixme: don't think that this needs to be public 
        public virtual void Initialize() { }

        // fixme: Not sure yet how to handle real-time issues. Don't want this loop to spin endlessly, will need it to yield so other processes
        // can run. For a simple lexeme-based CSA, the presenter could be where the yield lives. Another option is to not have Execute() be a loop.
        // Instead use an event-based architecture where changes to the blackboard trigger the controller. For now, just select a single KS and 
        // execute it. Punt looping to the enclosing application.  
        public void Execute()
        { 
            UpdateAgenda();
            IKnowledgeSourceActivation selectedKSA = SelectKSForExecution();
            if (selectedKSA != null)
            {
                selectedKSA.Execute();
                m_Agenda.Remove(selectedKSA);
            }
        }

        protected Controller()
        {
            m_ActiveKSs = new HashSet<IKnowledgeSource>();
            m_Agenda = new HashSet<IKnowledgeSourceActivation>();
            Initialize();
        }
    }
}
