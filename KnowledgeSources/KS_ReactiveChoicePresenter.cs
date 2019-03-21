using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using CSA.Core;
using CSA.KnowledgeUnits;
using static CSA.KnowledgeUnits.CUSlots;
using static CSA.KnowledgeUnits.KUProps;

namespace CSA.KnowledgeSources
{
    public class KS_ReactiveChoicePresenter : ReactiveKnowledgeSource
    {
        // Name of the bound activation variable
        private const string SelectedContentUnit = "SelectedContentUnit";

        // fixme: storing the choice information on fields on the knowledge source assumes that there will always only be one activation for KS_ChoicePresenter that is executed. If multiple 
        // activations ever occured, an event handler that was accessing this data through a reference to the KS would have stale data. 
        // Internal fields for storing the text to display, array of Enumerable of choices, and string array of text choices. 
        protected string m_textToDisplay;
        protected IEnumerable<ContentUnit> m_choices; // This is used by the SelectChoice() method. 
        protected string[] m_choicesToDisplay;

        // fixme: see fixme above 
        // Public accessors for text and choices. 
        public string TextToDisplay => m_textToDisplay ?? "";
        public string[] ChoicesToDisplay => m_choicesToDisplay ?? new string[0];

        // The delegate for event handling within the Execute() method
        public event EventHandler PresenterExecute; 

        public override IKnowledgeSourceActivation[] Precondition()
        {
            // Use LINQ to create a collection of the selected content units on the blackkboard
            var CUs = from ContentUnit cu in m_blackboard.LookupUnits(ContentUnit.TypeName)
                      where cu.HasMetadataSlot(SelectedContentUnit) // look for a selected content unit
                      where // that has not been previously matched by this precondition
                        (!cu.Properties.ContainsKey(KSPreconditionMatched)) ||
                        (!((ISet<ReactiveKnowledgeSource>)cu.Properties[KSPreconditionMatched]).Contains(this))
                      select cu;

            // Iterate through each of the selected content units, creating KnowledgeSourceActivations
            IKnowledgeSourceActivation[] activations = new KnowledgeSourceActivation[CUs.Count()];

            int i = 0;
            foreach (var cu in CUs)
            {
                var boundVars = new Dictionary<string, object>
                {
                    [SelectedContentUnit] = cu
                };

                activations[i++] = new KnowledgeSourceActivation(this, boundVars);

                // fixme: this bit of boilerplate code for marking a knowledge unit as having already participated in a matched precondition 
                // should be baked into the infrastructure somewhere so knowledge source implementers don't always have to do this. 
                // A good place to add this would be on Unit (and declare a method on IUnit).
                if (cu.Properties.ContainsKey(KSPreconditionMatched))
                {
                    ((HashSet<ReactiveKnowledgeSource>)cu.Properties[KSPreconditionMatched]).Add(this);
                }
                else
                {
                    cu.Properties[KSPreconditionMatched] = new HashSet<ReactiveKnowledgeSource> { this };
                }
            }

            return activations;

        }

        internal override bool EvaluateObviationCondition(IDictionary<string, object> boundVars)
        {
            return !m_blackboard.ContainsUnit((IUnit)boundVars[SelectedContentUnit]);
        }

        // Returns an Enumerable of choices. Choices are ContentUnits linked to this ContentUnit by Choice links.  
        protected IEnumerable<ContentUnit> GetChoices(IDictionary<string, object> boundVars)
        {
            // Gather the links of type L_SelectedContentUnit that have the SelectedContentUnit as one of the endpoints. 
            ContentUnit selectedCU = (ContentUnit)boundVars[SelectedContentUnit];
            var linkToOrigCU = from link in m_blackboard.LookupLinks(selectedCU)
                               where link.LinkType.Equals(LinkTypes.L_SelectedContentUnit)
                               select link;

            // There should be only one such link (ie. a SelectedContentUnit should only point to one originating unit). 
            Debug.Assert(linkToOrigCU.Count() == 1);

            // Get the original content unit (originalCU)
            (IUnit originalCU, _, _) = linkToOrigCU.ElementAt(0);

            // Gather the choices connected to the originalCU.
            IEnumerable<ContentUnit> choices = from link in m_blackboard.LookupLinks(originalCU)
                                               where link.LinkType.Equals(LinkTypes.L_Choice)
                                               select (ContentUnit)link.Node;

            return choices;

        }

        // Gathers the choices for the selected content unit, stores them on fields provided on this KS, and calls any calls any registered event handlers. 
        // fixme: should't store display info on KS but rather should pass it as args through to the event handler. 
        internal override void Execute(IDictionary<string, object> boundVars)
        {
            ContentUnit selectedCU = (ContentUnit)boundVars[SelectedContentUnit];

            m_textToDisplay = (string)selectedCU.Content[Text];

            m_choices = GetChoices(boundVars);

            if (m_choices.Any())
            {
                int choiceCounter = 0;
                m_choicesToDisplay = new string[m_choices.Count()];
                foreach (ContentUnit choice in m_choices)
                {
                    m_choicesToDisplay[choiceCounter++] = (string)choice.Content[Text];
                }
            }
            else
            {
                // No choices. Create a 0 length string array so that callers don't have to worry about null checks. 
                m_choicesToDisplay = new string[0];        
            }

            // Remove the displayed SelectedContentUnit from the blackboard.
            m_blackboard.RemoveUnit((IUnit)boundVars[SelectedContentUnit]);

            OnExecute(EventArgs.Empty);
        }

        protected virtual void OnExecute(EventArgs e)
        {
            PresenterExecute?.Invoke(this, EventArgs.Empty);
        }

        // Given a 0-indexed choice selection, sets the appropriate query on the blackboard.
        // fixme: This should be removed when the choice info is not longer stored on the KS. 
        public void SelectChoice(int choiceMade)
        {
            if (choiceMade >= 0 && choiceMade < m_choicesToDisplay.Length)
            {
                // Add a U_IDQuery to blackboard for the target content unit associated with the choice. 
                ContentUnit selectedChoice = m_choices.ElementAt(choiceMade);
                m_blackboard.AddUnit(new U_IDSelectRequest((string)selectedChoice.Metadata[TargetContentUnitID]));
             }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(choiceMade), choiceMade, $"choiceMade must be between 0 and the number of choices - 1 {m_choicesToDisplay.Length - 1}");
            }
        }

        public KS_ReactiveChoicePresenter(IBlackboard blackboard) : base(blackboard)
        {

        }


    }
}
