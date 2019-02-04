using System;
using System.Collections.Generic;
using System.Diagnostics;
using CSA.Core;

namespace CSA.KnowledgeSources
{
    // fixme: remove
    public abstract class KS_ConsoleChoicePresenter : KS_ChoicePresenter
    {
        /* 
        public override void Execute()
        {
            base.Execute();
            Console.WriteLine(m_textToDisplay);

            if(m_choicesToDisplay.Length > 0)
            {
                Debug.Assert(m_choicesToDisplay.Length < 10);

                int choiceCounter = 0; 
                foreach(string choice in m_choicesToDisplay)
                {
                    Console.Write("{0}. ", choiceCounter);
                    Console.WriteLine(choice);
                    choiceCounter++;
                }

                ConsoleKeyInfo keyInfo;
                do
                {
                    keyInfo = Console.ReadKey(true);
                }
                while (!char.IsDigit(keyInfo.KeyChar));

                // Add a U_IDQuery to blackboard for the target content unit associated with the choice. 
                int choiceMade = int.Parse(keyInfo.KeyChar.ToString());
                SelectChoice(choiceMade);
            }
        }

        // fixme: the factory and constructor stuff for knowledge sources (which supports constructing knowledge sources with bound variables)
        // is a bit confusing. See if there's a cleaner way to represent active KSs vs. KSs on the agenda, perhaps with an ActivatedKS class which wraps the KS and its bound variables.
        protected override KnowledgeSource Factory(IBlackboard blackboard, IDictionary<string, object> boundVars, KnowledgeSource ks)
        {
            return new KS_ConsoleChoicePresenter(blackboard, boundVars, ks);
        }

        protected KS_ConsoleChoicePresenter(IBlackboard blackboard, IDictionary<string, object> boundVars, KnowledgeSource ks) : base(blackboard, boundVars, ks)
        {
        }
        */
        public KS_ConsoleChoicePresenter(IBlackboard blackboard) : base(blackboard)
        {
        } 



    }
}
