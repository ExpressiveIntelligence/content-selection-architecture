using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using CSA.Core;
using CSA.KnowledgeUnits;
using static CSA.KnowledgeUnits.CUSlots;

namespace CSA.KnowledgeSources
{
    /* fixme: currently making this a parent of KS_ScheduledFilterSelector. But this is just to inherit all the mechanism related to handling input 
     * pools in the precondition. Better will be to have a hierarchy with two siblings: Filter (which copies some subset from input to output pools), 
     * and Process (which does some manipulation of ContentUnits passing some filter criterion, including input pool). The PrologEval unit still copies
     * CUs because it's modifying meta-data, while Display doesn't do meta-data modification. So not clear if the are both subclasses of Process or not. 
     */

    public class KS_ScheduledChoicePresenter : KS_ScheduledFilterSelector
    {
        public const string DefaultChoicePresenterInputPool = "ContentUnitToDisplay";

        // fixme: storing the choice information on fields on the knowledge source assumes that there will always only be one binding for KS_ChoicePresenter that is executed. If multiple 
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

        /*
         * Recursively searches back through L_SelectedContentUnit links until it finds the original content unit.
         */
        protected ContentUnit FindOriginalContentUnit(ContentUnit cu)
        {
            var linkToPreviousCUInFilterChain = from link in m_blackboard.LookupLinks(cu)
                                                where link.LinkType.Equals(LinkTypes.L_SelectedContentUnit)
                                                where link.Direction.Equals(LinkDirection.Start)
                                                select link;

            int count = linkToPreviousCUInFilterChain.Count();

            // There should be 0 (if we've reached the original) or 1 (if we're still crawling back up the filter chain) links.
            Debug.Assert(count == 0 || count == 1);

            if (count == 0)
            {
                return cu; // The passed in CU was the parent of a chain.
            }
            else
            {
                // Recursively search back up the filter chain
                (IUnit previousCUInChain, _, _) = linkToPreviousCUInFilterChain.ElementAt(0);
                return FindOriginalContentUnit((ContentUnit)previousCUInChain);
            }
        }

        // Returns an Enumerable of choices. Choices are ContentUnits linked to this ContentUnit by Choice links. 
        // fixme: add support for choices stored not as linked content, but as a query for selecting choices. 
        protected IEnumerable<ContentUnit> GetChoices(ContentUnit selectedCU)
        {
            ContentUnit originalCU = FindOriginalContentUnit(selectedCU);

            // Gather the choices connected to the originalCU.
            IEnumerable<ContentUnit> choices = from link in m_blackboard.LookupLinks(originalCU)
                                               where link.LinkType.Equals(LinkTypes.L_Choice)
                                               select (ContentUnit)link.Node;

            return choices;

        }

        // Gathers the choices for the selected content unit, stores them on fields provided on this KS, and calls any calls any registered event handlers. 
        // fixme: should't store display info on KS but rather should pass it as args through to the event handler. 
        protected override void Execute(IDictionary<string, object> boundVars)
        {
            var selectedCUs = ContentUnitsFilteredByPrecondition(boundVars);
 
            Debug.Assert(selectedCUs.Count() == 1); // fixme: With choice information stored on the KS, can only manage one selected content unit to display.

            ContentUnit selectedCU = selectedCUs.First();

            m_textToDisplay = (string)selectedCU.Content[Text];

            m_choices = GetChoices(selectedCU);

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

        /*
         * Constructor with no input pool specified, use the default input pool. The output pool is null since it won't be used by Execute().
         */
        public KS_ScheduledChoicePresenter(IBlackboard blackboard) : base(blackboard, DefaultChoicePresenterInputPool, (string)null)
        {
        }

        /*
         * If called with a specified inputPool, use the input pool. The output pool is null since it won't be used by Execute().
         */
        public KS_ScheduledChoicePresenter(IBlackboard blackboard, string inputPool) : base(blackboard, inputPool, (string)null)
        {
        }
    }
}
