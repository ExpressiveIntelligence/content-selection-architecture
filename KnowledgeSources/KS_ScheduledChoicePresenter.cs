using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    public class KS_ScheduledChoicePresenter : KS_ScheduledContentPoolCollector, IChoicePresenter
    {
        public const string DefaultChoicePresenterInputPool = "ContentUnitToDisplay";

        // The delegate for event handling within the Execute() method
        public event EventHandler<PresenterExecuteEventArgs> PresenterExecute;

        // The delegate for event handling within the SelectChoice method
        public event EventHandler<SelectChoiceEventArgs> PresenterSelectChoice;

        // Returns an Enumerable of choices. Choices are Units linked to the filtered Unit by Choice links. 
        // fixme: add support for choices stored not as linked content, but as a query for selecting choices. 
        protected Unit[] GetChoices(Unit selectedUnit)
        {
            Unit originalUnit = FindOriginalUnit(selectedUnit);

            // Gather the choices connected to the originalUnit.
            IEnumerable<Unit> choices = from link in m_blackboard.LookupLinks(originalUnit)
                                        where link.LinkType.Equals(LinkTypes.L_Choice)
                                        select (Unit)link.Node;

            return choices.ToArray();
        }

        // Gathers the choices for the selected unit and calls any registered event handlers. 
        protected override void Execute(object[] boundVars)
        {
            IEnumerable<Unit> selectedUnits = (IEnumerable<Unit>)boundVars[FilteredUnits];

            // fixme: now that we're passing choice info as params should be able to handle multiple selected Units
            Debug.Assert(selectedUnits.Count() == 1);

            Unit selectedUnit = selectedUnits.First();

            string textToDisplay = selectedUnit.GetText();

            Unit[] choices = GetChoices(selectedUnit);

            string[] choicesToDisplay;

            if (choices.Any())
            {
                int choiceCounter = 0;
                choicesToDisplay = new string[choices.Count()];
                foreach (Unit choice in choices)
                {
                    choicesToDisplay[choiceCounter++] = choice.GetText();
                }
            }
            else
            {
                // No choices. Create a 0 length string array so that callers don't have to worry about null checks. 
                choicesToDisplay = new string[0];
            }

            PresenterExecuteEventArgs eventArgs = new PresenterExecuteEventArgs(textToDisplay, choicesToDisplay, choices);
            OnExecute(eventArgs);
        }

        protected virtual void OnExecute(PresenterExecuteEventArgs eventArgs)
        {
            PresenterExecute?.Invoke(this, eventArgs);
        }

        /*
         * Given an array of choices and a 0-indexed choice selection, activates the KC_IDSelectionRequest on the selected choice and
         * call any actions registered on PresenterSelectChoice.         
         */
        public void SelectChoice(Unit[] choices, uint choiceMade)
        {
            if (choiceMade < choices.Length)
            {
                // Activate the KC_IDSelectionRequest associated with the choice. 
                Unit selectedChoice = choices[choiceMade];
                selectedChoice.SetActiveRequest(true);

                // Do any actions that have been registered on PresenterSelectChoice. 
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

        /*
         * Constructor with no input pool specified, use the default input pool.
         */
        public KS_ScheduledChoicePresenter(IBlackboard blackboard) : base(blackboard, DefaultChoicePresenterInputPool)
        {
        }

        /*
         * Constructor with specified inputPool.
         */
        public KS_ScheduledChoicePresenter(IBlackboard blackboard, string inputPool) : base(blackboard, inputPool)
        {
        }

    }
}
