// fixme: just putting this here for now so that I can store the code in the repository. But it only builds within unity because of the mysterious UnityEngine.UI.dll problem. 
// Have to comment it out so that I can build the solution. 

using System;
using System.Collections.Generic;
using CSA.Demo;
using CSA.KnowledgeSources;
using UnityEngine;
// using UnityEngine.UI; // UnityEngine.UI.dll is mysteriously not available from the unity folder. 

/* namespace CSA.UnitySupport
{

    // fixme: generalize this so that it has a general button container with a layouy policy that you set (by dragging) in the inspector. 
    public class TestCSA : MonoBehaviour
    {
        // fixme: change this to Demo1_Reactive or Demo1_Scheduled depending on which demo you want. 
        // private readonly Demo1_Scheduled m_demo = new Demo1_Scheduled();
        private readonly Demo1_Reactive m_demo = new Demo1_Reactive();

        private readonly List<Button> buttons = new List<Button>();
        private GameObject m_textDisplay;
        public Button ButtonPrefab;

        void Start()
        {
            // Set the handler on m_demo to DisplayUnityChoice. This handler will be fired by an event generated when ChoiceDisplay.Execute() is called. 
            // fixme: change this to DisplayUnityChoice_Reactive or DisplayUnityChoice_Scheduled depending on which demo you want. 
            // m_demo.AddChoicePresenterHandler(DisplayUnityChoiceScheduled);
            m_demo.AddChoicePresenterHandler(DisplayUnityChoiceReactive);

            // Find and store the text display so we can change the text later. 
            m_textDisplay = GameObject.Find("TextDisplay");
        }

        // Update is called once per frame
        public void Update()
        {
            if (m_demo.Blackboard.Changed)
            {
                m_demo.Blackboard.ResetChanged();
                m_demo.Controller.Execute();
            }
        }

        /*
         * fixme: next two methods have repeated logic. Refactor once ChoicePresenters sending data as args. Could also be fixed with an IChoicePresenter interface.
         */
         /*
        public void DisplayUnityChoiceScheduled(object sender, EventArgs e)
        {
            // Get the ChoicePresenter knowledge source that fired this event. Currently the choice info is stored on the knowledge source.
            // fixme: remove the choice info from the knowledge source and pass it as args. 
            KS_ScheduledChoicePresenter cp = (KS_ScheduledChoicePresenter)sender;

            if (buttons.Count > 0)
            {
                // Some buttons have already been created. Destroy them.
                Debug.Log("Clearing buttons: Button count: " + buttons.Count);
                foreach (Button button in buttons)
                {
                    Destroy(button);
                }
                buttons.Clear();
            }

            // Change the text displayed
            Text textToChange = m_textDisplay.GetComponent<Text>();
            textToChange.text = cp.TextToDisplay;

            // Create the buttons for choices
            if (cp.ChoicesToDisplay.Length > 0)
            {
                Debug.Assert(cp.ChoicesToDisplay.Length < 5);

                // Iterate through the number of choices, creating a button for each choice. 
                Debug.Log($"Number of choices = {cp.ChoicesToDisplay.Length}");
                for (int i = 0; i < cp.ChoicesToDisplay.Length; i++)
                {
                    // Create buttons with appropriate handlers
                    Debug.Log("Creating button with transform: " + transform);
                    Button button = Instantiate(ButtonPrefab);
                    Text textChild = button.GetComponentInChildren<Text>();
                    textChild.text = cp.ChoicesToDisplay[i];
                    RectTransform rect = (RectTransform)button.transform;
                    rect.SetParent(transform);
                    rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 10 + i * 40, 30);
                    rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 600, 150);


                    int choice = i; // Create a new variable with a copy of i so that the closure below closes a unique variable per choice 
                                    // register a handler with the button that calls cp.SelectChoice() with the appropriate choice.
                    button.onClick.AddListener(() => ButtonCallbackScheduled(choice, cp));

                    buttons.Add(button);
                }
            }
        }

        public void DisplayUnityChoiceReactive(object sender, EventArgs e)
        {
            // Get the ChoicePresenter knowledge source that fired this event. Currently the choice info is stored on the knowledge source.
            // fixme: remove the choice info from the knowledge source and pass it as args. 
            KS_ReactiveChoicePresenter cp = (KS_ReactiveChoicePresenter)sender;

            if (buttons.Count > 0)
            {
                // Some buttons have already been created. Destroy them.
                Debug.Log("Clearing buttons: Button count: " + buttons.Count);
                foreach (Button button in buttons)
                {
                    Destroy(button);
                }
                buttons.Clear();
            }

            // Change the text displayed
            Text textToChange = m_textDisplay.GetComponent<Text>();
            textToChange.text = cp.TextToDisplay;

            // Create the buttons for choices
            if (cp.ChoicesToDisplay.Length > 0)
            {
                Debug.Assert(cp.ChoicesToDisplay.Length < 5);

                // Iterate through the number of choices, creating a button for each choice. 
                Debug.Log($"Number of choices = {cp.ChoicesToDisplay.Length}");
                for (int i = 0; i < cp.ChoicesToDisplay.Length; i++)
                {
                    // Create buttons with appropriate handlers
                    Debug.Log("Creating button with transform: " + transform);
                    Button button = Instantiate(ButtonPrefab);
                    Text textChild = button.GetComponentInChildren<Text>();
                    textChild.text = cp.ChoicesToDisplay[i];
                    RectTransform rect = (RectTransform)button.transform;
                    rect.SetParent(transform);
                    rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 10 + i * 40, 30);
                    rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 600, 150);


                    int choice = i; // Create a new variable with a copy of i so that the closure below closes a unique variable per choice 
                                    // register a handler with the button that calls cp.SelectChoice() with the appropriate choice.
                    button.onClick.AddListener(() => ButtonCallbackReactive(choice, cp));

                    buttons.Add(button);
                }
            }
        }


        void ButtonCallbackReactive(int selection, KS_ReactiveChoicePresenter cp)
        {
            cp.SelectChoice(selection);
        }

        void ButtonCallbackScheduled(int selection, KS_ScheduledChoicePresenter cp)
        {
            cp.SelectChoice(selection);
        }

    }
} */