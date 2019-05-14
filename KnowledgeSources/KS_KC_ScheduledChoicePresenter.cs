using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using CSA.Core;
using CSA.KnowledgeUnits;
using static CSA.KnowledgeUnits.CUSlots;

namespace CSA.KnowledgeSources
{
    public class KS_KC_ScheduledChoicePresenter : KS_KC_ScheduledFilterSelector, IChoicePresenter
    {
        public const string DefaultChoicePresenterInputPool = "ContentUnitToDisplay";

        // The delegate for event handling within the Execute() method
        public event EventHandler<PresenterExecuteEventArgs> PresenterExecute;

        // The delegate for event handling within the SelectChoice method
        public event EventHandler<SelectChoiceEventArgs> PresenterSelectChoice;

        // Returns an Enumerable of choices. Choices are Units linked to the filtered Unit by Choice links. 
        // fixme: add support for choices stored not as linked content, but as a query for selecting choices. 
        protected Unit[] GetChoices(Unit selectedCU)
        {
            Unit originalCU = FindOriginalUnit(selectedCU);

            // Gather the choices connected to the originalCU.
            IEnumerable<Unit> choices = from link in m_blackboard.LookupLinks(originalCU)
                                               where link.LinkType.Equals(LinkTypes.L_Choice)
                                               select (Unit)link.Node;

            return choices.ToArray();
        }

        // Gathers the choices for the selected unit and calls any registered event handlers. 
        protected override void Execute(IDictionary<string, object> boundVars)
        {
            var selectedUnits = UnitsFilteredByPrecondition(boundVars);

            // fixme: now that we're passing choice info as params should be able to handle multiple selected CUs
            Debug.Assert(selectedUnits.Count() == 1);

            Unit selectedUnit = selectedUnits.First();

            string textToDisplay = (string)selectedUnit.GetText();

            Unit[] choices = GetChoices(selectedUnit);

            string[] choicesToDisplay;

            if (choices.Any())
            {
                int choiceCounter = 0;
                choicesToDisplay = new string[choices.Count()];
                foreach (Unit choice in choices)
                {
                    choicesToDisplay[choiceCounter++] = (string)choice.Content[Text];
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

        public void SelectChoice(ContentUnit[] choices, uint choiceMade)
        {
            throw new NotImplementedException();
        }

        public KS_KC_ScheduledChoicePresenter()
        {
        }

    }
}
