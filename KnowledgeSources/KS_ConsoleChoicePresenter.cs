using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSACore;
using KnowledgeUnits;

namespace KnowledgeSources
{
    public class KS_ConsoleChoicePresenter : KnowledgeSource
    {
        // Name of the bound context variable
        private const string SelectedContentUnit = "SelectedContentUnit";

        protected override void EvaluatePrecondition()
        {
            // Use LINQ to create a collection of the selected content units on the blackkboard
            var cuS = from cu in m_blackboard.LookupUnits(ContentUnit.TypeName)
                      where ((ContentUnit)cu).HasMetadataSlot(CU_SlotNames.SelectedContentUnit)
                      where
                        (!cu.Properties.ContainsKey(U_PropertyNames.KSPreconditionMatched)) ||
                        (!((ISet<KnowledgeSource>)cu.Properties[U_PropertyNames.KSPreconditionMatched]).Contains(this))
                      select cu;

            // Iterate through each of the selected content units, creating context entries
            foreach (var cu in cuS)
            {
                var boundVars = new Dictionary<string, object>
                {
                    [SelectedContentUnit] = cu
                };
                m_contexts.Add(boundVars);

                // fixme: this bit of boilerplate code for marking a knowledge unit as having already participated in a matched precondition 
                // should be baked into the infrastructure somewhere so knowledge source implementers don't always have to do this. 
                if (cu.Properties.ContainsKey(U_PropertyNames.KSPreconditionMatched))
                {
                    ((HashSet<KnowledgeSource>)cu.Properties[U_PropertyNames.KSPreconditionMatched]).Add(this);
                }
                else
                {
                    cu.Properties[U_PropertyNames.KSPreconditionMatched] = new HashSet<KnowledgeSource> { this };
                }
            }

        }

        public override bool EvaluateObviationCondition()
        {
            Debug.Assert(Executable);
            return !m_blackboard.ContainsUnit((IUnit)m_boundVars[SelectedContentUnit]);
        }

        // Returns an Enumerable of choices. Choices are ContentUnits linked to this ContentUnit by Choice links.  
        protected IEnumerable<ContentUnit> GetChoices()
        {
            Debug.Assert(Executable);

            // Gather the links of type L_SelectedContentUnit that have the SelectedContentUnit as one of the endpoints. 
            ContentUnit selectedCU = (ContentUnit)m_boundVars[SelectedContentUnit];
            var linkToOrigCU = from link in m_blackboard.LookupLinks(selectedCU)
                               where link.LinkType.Equals(LinkTypes.L_SelectedContentUnit)
                               select link;

            // There should be only one such link (ie. a SelectedContentUnit should only point to one originating unit). 
            Debug.Assert(linkToOrigCU.Count() == 1);

            // Get the original content unit (originalCU)
            (IUnit originalCU, string LinkType) = linkToOrigCU.ElementAt(0);

            // Gather the choices connected to the originalCU.
            IEnumerable<ContentUnit> choices = from link in m_blackboard.LookupLinks(originalCU)
                                               where link.LinkType.Equals(LinkTypes.L_Choice)
                                               select (ContentUnit)link.Node;

            return choices;

        }

        // fixme: the first version of this blocks the thread while it waits for input. Much of the functionality in this class needs to be factored out
        // into a more abstract class. Will figure this out when I get it working within Unity. 
        public override void Execute()
        {
            Debug.Assert(Executable);
            ContentUnit selectedCU = (ContentUnit)m_boundVars[SelectedContentUnit];

            string textToDisplay = (string)selectedCU.Content[CU_SlotNames.Text];

            Console.WriteLine(textToDisplay);

            var choices = GetChoices();

            if (choices.Any())
            {
                int choiceCounter = 0;
                foreach (ContentUnit choice in choices)
                {
                    Console.Write("{0}. ", choiceCounter);
                    Console.WriteLine(choice.Content[CU_SlotNames.Text]);
                    choiceCounter++;
                }

                Debug.Assert(choiceCounter < 10);

                ConsoleKeyInfo keyInfo;
                do
                {
                    keyInfo = Console.ReadKey(true);
                }
                while (!char.IsDigit(keyInfo.KeyChar));

                // Add a U_IDQuery to blackboard for the target content unit associated with the choice. 
                int choiceMade = int.Parse(keyInfo.KeyChar.ToString());
                ContentUnit selectedChoice = (ContentUnit)choices.ElementAt(choiceMade);
                m_blackboard.AddUnit(new U_IDQuery((string)selectedChoice.Metadata[CU_SlotNames.TargetContentUnitID]));
            }

            // Remove the displayed SelectedContentUnit from the blackboard.
            m_blackboard.RemoveUnit((ContentUnit)m_boundVars[SelectedContentUnit]);
            m_boundVars = null; // Set m_boundVars to null, marking this KS as not executable. 
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

        public KS_ConsoleChoicePresenter(IBlackboard blackboard) : base(blackboard)
        {
        }

    }
}
