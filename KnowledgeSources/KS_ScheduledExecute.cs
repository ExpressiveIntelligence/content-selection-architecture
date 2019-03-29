using System;
using System.Collections.Generic;

namespace CSA.KnowledgeSources
{
    public class KS_ScheduledExecute : IScheduledKnowledgeSource
    {
        public delegate void ExecuteFun();

        private readonly ExecuteFun m_codeToExecute;

        public IDictionary<string, object> Properties { get; }

        public void Execute()
        {
            m_codeToExecute();
        }

        public KS_ScheduledExecute(ExecuteFun codeToExecute)
        {
            m_codeToExecute = codeToExecute;
            Properties = new Dictionary<string, object>();
        }

    }
}
