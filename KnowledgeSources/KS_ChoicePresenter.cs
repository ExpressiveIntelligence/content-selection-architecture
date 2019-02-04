using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using CSA.Core;
using CSA.KnowledgeUnits;

namespace CSA.KnowledgeSources
{
    public class KS_ChoicePresenter : KnowledgeSource
    {
        // Name of the bound context variable
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

        public int i;

        public override IKnowledgeSourceActivation[] Precondition()
        {
            // Use LINQ to create a collection of the selected content units on the blackkboard
            var cuS = from cu in m_blackboard.LookupUnits(ContentUnit.TypeName)
                      where ((ContentUnit)cu).HasMetadataSlot(CU_SlotNames.SelectedContentUnit) // look for a selected content unit
                      where // that has not been previously matched by this precondition
                        (!cu.Properties.ContainsKey(U_PropertyNames.KSPreconditionMatched)) ||
                        (!((ISet<KnowledgeSource>)cu.Properties[U_PropertyNames.KSPreconditionMatched]).Contains(this))
                      select cu;

            // Iterate through each of the selected content units, creating KnowledgeSourceActivations
            IKnowledgeSourceActivation[] activations = new KnowledgeSourceActivation[cuS.Count()];

            int i = 0;
            foreach (var cu in cuS)
            {
                var boundVars = new Dictionary<string, object>
                {
                    [SelectedContentUnit] = cu
                };

                activations[i++] = new KnowledgeSourceActivation(this, boundVars);

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

            return activations;

        }

        // fixme: remove
        /* protected override void EvaluatePrecondition()
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

        } */

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
            (IUnit originalCU, string LinkType) = linkToOrigCU.ElementAt(0);

            // Gather the choices connected to the originalCU.
            IEnumerable<ContentUnit> choices = from link in m_blackboard.LookupLinks(originalCU)
                                               where link.LinkType.Equals(LinkTypes.L_Choice)
                                               select (ContentUnit)link.Node;

            return choices;

        }

        /* fixme: storing choice data on the KS_ChoicePresenter doesn't work for Unity. It works for the Console app because display is happening within the Execute method of the same Controller instance,
         * since the display logic is in a subclass of KS_ChoicePresenter. But for Unity, the KS_ChoicePresenter was going to be stored within a MonoBehaviour. But the KS_ChoicePresenter that is registered
         * with a controller is *not* the same one that executes, since copies of the knowledge source are made for each activation context. Even if I move away from representing activation contexts as 
         * copies of the controller, and instead have a single controller that is bound in (potentially multiple) activation contexts, the execute method will stomp on the state stored on the presenter (KS) 
         * as Execute() is called in different contexts. I need a general mechanism for the Execute method of a presenter (or potentially any controller) to store state during an Execute(). The most 
         * natural place is on the blackboard, to be retrieved by the application. What if the Execute() method below stored TextToDisplay, ChoicesToDisplay and the Choices content units in a special 
         * KnowledgeUnit, say ChoicePresenterContentToDisplay? The downside is that the calling application how to do a LINQ query to retrieve it, and is responsible for deleting it in order to prevent 
         * multipe ChoicePresenterContentToDisplay units from piling up (which then creates ambiguity of which one to display). Another option to to support event registration with Presenter KSs. When 
         * Execute() is called on the KS, it calls any registered events with TextToDisplay and Choices. This code could do the Unity UI wrangling (assuming it bound the appropriate UI elements). This 
         * latter approach is good, but it doen't know what query to add when a button is pressed. So in adding the UI elements, this latter approach would also need to register an appropriate listener on 
         * the buttons which adds the correct element to the blackboard. 
        */
        internal override void Execute(IDictionary<string, object> boundVars)
        {
            ContentUnit selectedCU = (ContentUnit)boundVars[SelectedContentUnit];

            m_textToDisplay = (string)selectedCU.Content[CU_SlotNames.Text];

            m_choices = GetChoices(boundVars);

            if (m_choices.Any())
            {
                int choiceCounter = 0;
                m_choicesToDisplay = new string[m_choices.Count()];
                foreach (ContentUnit choice in m_choices)
                {
                    m_choicesToDisplay[choiceCounter++] = (string)choice.Content[CU_SlotNames.Text];
                }
            }
            else
            {
                // No choices. Create a 0 length string array so that callers don't have to worry about null checks. 
                m_choicesToDisplay = new string[0];        
            }

            // Remove the displayed SelectedContentUnit from the blackboard.
            m_blackboard.RemoveUnit((IUnit)boundVars[SelectedContentUnit]);

            PresenterExecute?.Invoke(this, EventArgs.Empty);

        }

        // Given a 0-indexed choice selection, sets the appropriate query on the blackboard.
         public void SelectChoice(int choiceMade)
        {
            if (choiceMade >= 0 && choiceMade < m_choicesToDisplay.Length)
            {
                // Add a U_IDQuery to blackboard for the target content unit associated with the choice. 
                ContentUnit selectedChoice = m_choices.ElementAt(choiceMade);
                m_blackboard.AddUnit(new U_IDQuery((string)selectedChoice.Metadata[CU_SlotNames.TargetContentUnitID]));
             }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(choiceMade), choiceMade, $"choiceMade must be between 0 and the number of choices - 1 {m_choicesToDisplay.Length - 1}");
            }
        }

        // fixme: remove
        // fixme: the factory and constructor stuff for knowledge sources (which supports constructing knowledge sources with bound variables)
        // is a bit confusing. See if there's a cleaner way to represent active KSs vs. KSs on the agenda, perhaps with an ActivatedKS class which wraps the KS and its bound variables.
        /* protected override KnowledgeSource Factory(IBlackboard blackboard, IDictionary<string, object> boundVars, KnowledgeSource ks)
        {
            return new KS_ChoicePresenter(blackboard, boundVars, ks);
        }

        protected KS_ChoicePresenter(IBlackboard blackboard, IDictionary<string, object> boundVars, KnowledgeSource ks) : base(blackboard, boundVars, ks)
        {
        }

        public KS_ChoicePresenter(IBlackboard blackboard) : base(blackboard)
        {
        } */

        public KS_ChoicePresenter(IBlackboard blackboard) : base(blackboard)
        {

        }


    }
}
