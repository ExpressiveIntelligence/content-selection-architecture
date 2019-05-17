using System;
using System.Collections.Generic;

namespace CSA.KnowledgeSources
{
    public class KS_KC_ScheduledExecute : IScheduledKnowledgeSource
    {
        /* 
         * Define delegates for the precondition and execution functions. The execution function will be executed if
         * the precondition function returns true.       
         */
        public delegate bool PreconditionFun();
        public delegate void ExecuteFun();

        /*
         * Private fields storing the precondition and execution functions. 
         */
        private readonly PreconditionFun m_precondition;
        private readonly ExecuteFun m_codeToExecute;

        public IDictionary<string, object> Properties { get; }

        /*
         * If the precondition is true, execute m_codeToExecute.
         * fixme: may eventually want to pass bindings from the precondition to m_codeToExecute().        
         */
        public void Execute()
        {
            if (m_precondition())
            {
                m_codeToExecute();
            }
        }

        /* 
         * This constructor defines a default precondition which always returns true. 
         */
        public KS_KC_ScheduledExecute(ExecuteFun codeToExecute)
        {
            m_precondition = () => true; 
            m_codeToExecute = codeToExecute;
            Properties = new Dictionary<string, object>();
        }

        /*
         * This constructor defines an explicit precondition. 
         */
        public KS_KC_ScheduledExecute(ExecuteFun codeToExecute, PreconditionFun precondition)
        {
            m_precondition = precondition;
            m_codeToExecute = codeToExecute;
            Properties = new Dictionary<string, object>();
        }
    }
}
