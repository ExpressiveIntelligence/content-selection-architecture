using System;
using System.Collections.Generic;

using CSA.Core;

namespace CSA.KnowledgeSources
{
    /* fixme: currently making this a parent of KS_ScheduledFilterSelector. But this is just to inherit all the mechanism related to handling input 
     * pools in the precondition. Better will be to have a hierarchy with two siblings: Filter (which copies some subset from input to output pools), 
     * and Process (which does some manipulation of ContentUnits passing some filter criterion, including input pool). The PrologEval unit still copies
     * CUs because it's modifying meta-data, while Display doesn't do meta-data modification. So not clear if the are both subclasses of Process or not. 
     */

    public class KS_ScheduledChoicePresenter : KS_ScheduledFilterSelector
    {

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

        public int i;

        protected override void Execute(IDictionary<string, object> boundVars)
        {
            
        }

        public KS_ScheduledChoicePresenter(IBlackboard blackboard, string outputPool) : base(blackboard, outputPool)
        {
        }
    }
}
