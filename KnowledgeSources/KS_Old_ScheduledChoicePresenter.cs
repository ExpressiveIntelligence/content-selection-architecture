using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using CSA.Core;
using CSA.KnowledgeUnits;

#pragma warning disable CS0618 // Type or member is obsolete
using static CSA.KnowledgeUnits.CUSlots;
#pragma warning restore CS0618 // Type or member is obsolete

namespace CSA.KnowledgeSources
{
    /* fixme: currently making this a parent of KS_ScheduledFilterSelector. But this is just to inherit all the mechanism related to handling input 
     * pools in the precondition. Better will be to have a hierarchy with two siblings: Filter (which copies some subset from input to output pools), 
     * and Process (which does some manipulation of ContentUnits passing some filter criterion, including input pool). The PrologEval unit still copies
     * CUs because it's modifying meta-data, while Display doesn't do meta-data modification. So not clear if the are both subclasses of Process or not. 
     */
    [Obsolete("Use KnowledgeComponent-based version of this controller.")]
    public class KS_Old_ScheduledChoicePresenter : KS_Old_ScheduledFilterSelector, IChoicePresenter_Old
    {
        public const string DefaultChoicePresenterInputPool = "ContentUnitToDisplay";

        // The delegate for event handling within the Execute() method
        public event EventHandler<PresenterExecuteEventArgs> PresenterExecute;

        // The delegate for event handling within the SelectChoice method
        public event EventHandler<SelectChoiceEventArgs> PresenterSelectChoice;

        /*
         * Recursively searches back through L_SelectedContentUnit links until it finds the original content unit.
         */
         // fixme: remove this when this class is converted to KS_KC_ScheduledChoicePresenter - it will be inherited from ContentPoolCollector.
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
        protected ContentUnit[] GetChoices(ContentUnit selectedCU)
        {
            ContentUnit originalCU = FindOriginalContentUnit(selectedCU);

            // Gather the choices connected to the originalCU.
            IEnumerable<ContentUnit> choices = from link in m_blackboard.LookupLinks(originalCU)
                                               where link.LinkType.Equals(LinkTypes.L_Choice)
                                               select (ContentUnit)link.Node;

            return choices.ToArray();
        }

        // Gathers the choices for the selected content unit and calls any registered event handlers. 
        protected override void Execute(IDictionary<string, object> boundVars)
        {
            var selectedCUs = ContentUnitsFilteredByPrecondition(boundVars);

            // fixme: now that we're passing choice info as params should be able to handle multiple selected CUs
            Debug.Assert(selectedCUs.Count() == 1); 

            ContentUnit selectedCU = selectedCUs.First();

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

            PresenterExecuteEventArgs eventArgs = new PresenterExecuteEventArgs(textToDisplay, choicesToDisplay, choices);
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

        /*
         * Constructor with no input pool specified, use the default input pool. The output pool is null since it won't be used by Execute().
         */
        public KS_Old_ScheduledChoicePresenter(IBlackboard blackboard) : base(blackboard, DefaultChoicePresenterInputPool, (string)null)
        {
        }

        /*
         * If called with a specified inputPool, use the input pool. The output pool is null since it won't be used by Execute().
         */
        public KS_Old_ScheduledChoicePresenter(IBlackboard blackboard, string inputPool) : base(blackboard, inputPool, (string)null)
        {
        }

    }
}
