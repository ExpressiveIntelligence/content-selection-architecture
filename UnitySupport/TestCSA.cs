﻿using System.Collections.Generic;
using CSA.Demo;
using CSA.KnowledgeSources;
using CSA.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CSA.UnitySupport
{
    /*
     * fixme: Unable to test in Unity 2019. Get an "unloading broken assembly" error for dlls. The first thing to try is to see if this still works in Unity 2018. But
     * the Simple Text Choices project got updated to Unity 2019 and won't import back to 2018. So I'll need to rebuild Simple Text Choices in Unity 2018 to test this.     
     */
    public class TestCSA : MonoBehaviour
    {
        private readonly Demo1_Scheduled m_demo = new Demo1_Scheduled();

        private readonly List<Button> buttons = new List<Button>();
        private GameObject m_textDisplay;
        public Button ButtonPrefab;

        void Start()
        {
            // Set the handler on m_demo to DisplayUnityChoice. This handler will be fired by an event generated when ChoiceDisplay.Execute() is called. 
            m_demo.AddChoicePresenterHandler(DisplayUnityChoice);

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
         * fixme: Generalize this so that it has a general button container with a layouy policy that you set (by dragging) in the inspector. 
         */
        public void DisplayUnityChoice(object sender, PresenterExecuteEventArgs eventArgs)
        {
            // Get the ChoicePresenter knowledge source that fired this event. Currently the choice info is stored on the knowledge source.
            IChoicePresenter cp = (IChoicePresenter)sender;

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
            textToChange.text = eventArgs.TextToDisplay;

            // Create the buttons for choices
            if (eventArgs.ChoicesToDisplay.Length > 0)
            {
                Debug.Assert(eventArgs.ChoicesToDisplay.Length < 5);

                // Iterate through the number of choices, creating a button for each choice. 
                Debug.Log($"Number of choices = {eventArgs.ChoicesToDisplay.Length}");
                for (uint i = 0; i < eventArgs.ChoicesToDisplay.Length; i++)
                {
                    // Create buttons with appropriate handlers
                    Debug.Log("Creating button with transform: " + transform);
                    Button button = Instantiate(ButtonPrefab);
                    Text textChild = button.GetComponentInChildren<Text>();
                    textChild.text = eventArgs.ChoicesToDisplay[i];
                    RectTransform rect = (RectTransform)button.transform;
                    rect.SetParent(transform);
                    rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 10 + i * 40, 30);
                    rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 600, 150);


                    uint choice = i; // Create a new variable with a copy of i so that the closure below closes a unique variable per choice 
                                     // register a handler with the button that calls cp.SelectChoice() with the appropriate choice.
                    button.onClick.AddListener(() => ButtonCallback(eventArgs.Choices, choice, cp));

                    buttons.Add(button);
                }
            }
        }

        void ButtonCallback(Unit[] choices, uint selection, IChoicePresenter cp)
        {
            cp.SelectChoice(choices, selection);
        }

    }
}