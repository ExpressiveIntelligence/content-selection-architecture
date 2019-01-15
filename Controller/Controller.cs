using System.Collections.Generic;
using System.Diagnostics;
using KnowledgeSources;

namespace Controllers
{
    public abstract class Controller : IController
    {

        // fixme: for now have the KSs live in data structures inside the controller. Consider moving them onto the blackboard. 
        protected readonly ISet<KnowledgeSource> m_ActiveKSs;
        protected readonly ISet<KnowledgeSource> m_Agenda;

        // To update the agenda, execute obviation conditions to remove KSs from the agenda and execute preconditions to add KSs to the agenda.
        protected void UpdateAgenda()
        {
            foreach(KnowledgeSource ks in m_Agenda)
            {
                if (ks.EvaluateObviationCondition())
                {
                    Debug.Assert(ks.Executable);
                    m_Agenda.Remove(ks);
                }
            }

            foreach(KnowledgeSource ks in m_ActiveKSs)
            {
                Debug.Assert(!ks.Executable);
                IEnumerable<KnowledgeSource> executableKSs = ks.Precondition();
                foreach(KnowledgeSource execKS in executableKSs)
                {
                    m_Agenda.Add(execKS);
                }
            }
        }

        public void AddKnowledgeSource(KnowledgeSource ks) => m_ActiveKSs.Add(ks);

        public void RemoveKnowledgeSource(KnowledgeSource ks) => m_ActiveKSs.Remove(ks);

        public ISet<KnowledgeSource> ActiveKSs => new HashSet<KnowledgeSource>(m_ActiveKSs);

        public ISet<KnowledgeSource> Agenda => new HashSet<KnowledgeSource>(m_Agenda);

        protected abstract KnowledgeSource SelectKSForExecution();

        // Any initialization of the controller that needs to happen when it is constructed. 
        // fixme: if I don't find a use case for a controller that needs this, remove it. 
        public abstract void Initialize();

        // fixme: Not sure yet how to handle real-time issues. Don't want this loop to spin endlessly, will need it to yield so other processes
        // can run. For a simple lexeme-based CSA, the presenter could be where the yield lives. Another option is to not have Execute() be a loop.
        // Instead use an event-based architecture where changes to the blackboard trigger the controller. For now, just select a single KS and 
        // execute it. Punt looping to the enclosing application.  
        public void Execute()
        { 
            UpdateAgenda();
            KnowledgeSource selectedKS = SelectKSForExecution();
            Debug.Assert(selectedKS.Executable);
            selectedKS.Execute();

        }

        protected Controller()
        {
            m_ActiveKSs = new HashSet<KnowledgeSource>();
            m_Agenda = new HashSet<KnowledgeSource>();
            Initialize();
        }
    }
}
