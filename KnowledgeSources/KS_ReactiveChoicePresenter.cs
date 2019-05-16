using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using CSA.Core;
using CSA.KnowledgeUnits;
#pragma warning disable CS0618 // Type or member is obsolete
using static CSA.KnowledgeUnits.CUSlots;
#pragma warning restore CS0618 // Type or member is obsolete
using static CSA.KnowledgeUnits.KUProps;

namespace CSA.KnowledgeSources
{
    [Obsolete("Used ScheduledChoicePresenter until a reactive version of this has been implemented.")]
    public class KS_ReactiveChoicePresenter : ReactiveKnowledgeSource, IChoicePresenter
    {
        // Name of the bound activation variable
        private const string SelectedContentUnit = "SelectedContentUnit";

        // The delegate for event handling within the Execute() method
        public event EventHandler<PresenterExecuteEventArgs> PresenterExecute;

        // The delegate for event handling within the SelectChoice method
        public event EventHandler<SelectChoiceEventArgs> PresenterSelectChoice;

        public override IKnowledgeSourceActivation[] Precondition()
        {
            // Use LINQ to create a collection of the selected content units on the blackkboard
            var CUs = from ContentUnit cu in m_blackboard.LookupUnits<ContentUnit>()
                      where cu.HasMetadataSlot(SelectedContentUnit) // look for a selected content unit
                      where // that has not been previously matched by this precondition
                        (!cu.Slots.ContainsKey(KSPreconditionMatched)) ||
                        (!((ISet<ReactiveKnowledgeSource>)cu.Slots[KSPreconditionMatched]).Contains(this))
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
                if (cu.Slots.ContainsKey(KSPreconditionMatched))
                {
                    ((HashSet<ReactiveKnowledgeSource>)cu.Slots[KSPreconditionMatched]).Add(this);
                }
                else
                {
                    cu.Slots[KSPreconditionMatched] = new HashSet<ReactiveKnowledgeSource> { this };
                }
            }

            return activations;

        }

        internal override bool EvaluateObviationCondition(IDictionary<string, object> boundVars)
        {
            return !m_blackboard.ContainsUnit((IUnit)boundVars[SelectedContentUnit]);
        }

        /*
         * fixme: this doesn't work if there are multiple filter pools (see KS_ScheduledChoicePresenter for how to handle this.
         * Fix this when I've decided more definitively how I'm handling reactive KSs.         
         */
        // Returns an Enumerable of choices. Choices are ContentUnits linked to this ContentUnit by Choice links.  
        protected ContentUnit[] GetChoices(ContentUnit selectedCU)
        {
            // Gather the links of type L_SelectedContentUnit that have the SelectedContentUnit as one of the endpoints. 
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

            return choices.ToArray();

        }

        // Gathers the choices for the selected content unit, stores them on fields provided on this KS, and calls any calls any registered event handlers. 
        internal override void Execute(IDictionary<string, object> boundVars)
        {
            ContentUnit selectedCU = (ContentUnit)boundVars[SelectedContentUnit];

            string textToDisplay = (string)selectedCU.Content[Text];

            ContentUnit[] choices = GetChoices(selectedCU);

            string[] choicesToDisplay;

            if (choices.Any())
            {
                int choiceCounter = 0;
                choicesToDisplay = new string[choices.Count()];
                foreach (ContentUnit choice in choices)
                {
                    choicesToDisplay[choiceCounter++] = (string)choice.Content[Text];
                }
            }
            else
            {
                // No choices. Create a 0 length string array so that callers don't have to worry about null checks. 
                choicesToDisplay = new string[0];
            }

            // Remove the displayed SelectedContentUnit from the blackboard. Do this to indicate that we have processed the selectedCU. 
            m_blackboard.RemoveUnit(selectedCU);

            // Construct event args and call the event handler. 
            var eventArgs = new PresenterExecuteEventArgs(textToDisplay, choicesToDisplay, choices);
            OnExecute(eventArgs);
        }

        protected virtual void OnExecute(PresenterExecuteEventArgs eventArgs)
        {
            PresenterExecute?.Invoke(this, eventArgs);
        }

        /*
         * Given an array of choices and a 0-indexed choice selection, adds the appropriate query to the blackboard. 
         */
        public void SelectChoice(ContentUnit[] choices, uint choiceMade)
        {
            if (choiceMade < choices.Length)
            {
                // Add a U_IDQuery to blackboard for the target content unit associated with the choice. 
                ContentUnit selectedChoice = choices[choiceMade];
                m_blackboard.AddUnit(new U_IDSelectRequest((string)selectedChoice.Metadata[TargetContentUnitID]));
                SelectChoiceEventArgs eventArgs = new SelectChoiceEventArgs(selectedChoice, m_blackboard);
                OnSelectChoice(eventArgs);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(choiceMade), choiceMade, $"choiceMade must be between 0 and the number of choices - 1 {choices.Length - 1}");
            }
        }

        protected virtual void OnSelectChoice(SelectChoiceEventArgs eventArgs)
        {
            PresenterSelectChoice?.Invoke(this, eventArgs);
        }

        public KS_ReactiveChoicePresenter(IBlackboard blackboard) : base(blackboard)
        {

        }


    }
}
