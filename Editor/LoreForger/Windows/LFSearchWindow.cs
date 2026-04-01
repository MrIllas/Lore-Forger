using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


namespace NovaDot.LoreForger.Windows
{
    using Enumerations;
    using Elements;

    public class LFSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private LFGraphView _graphView;
        private Texture2D identationIcon;

        public void Initialize(LFGraphView lFGraphView)
        {
            _graphView = lFGraphView;

            identationIcon = new Texture2D(1, 1);
            identationIcon.SetPixel(0, 0, Color.clear);
            identationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice", identationIcon))
                {
                    level = 2,
                    userData = LFDialogueType.SingleChoice
                },
                new SearchTreeEntry(new GUIContent("Multiple Choice", identationIcon))
                {
                    level = 2,
                    userData = LFDialogueType.MultipleChoice
                },
                new SearchTreeGroupEntry(new GUIContent("Dialogue Group"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", identationIcon))
                {
                    level = 2,
                    userData = new Group()
                }
            };

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = _graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData)
            {
                case LFDialogueType.SingleChoice:
                {
                    LFSingleChoiceNode node = (LFSingleChoiceNode) _graphView.CreateNode("DialogueName", LFDialogueType.SingleChoice, localMousePosition);
                    _graphView.AddElement(node);

                    return true;
                }   
                case LFDialogueType.MultipleChoice:
                {
                    LFMultipleChoiceNode node = (LFMultipleChoiceNode) _graphView.CreateNode("DialogueName", LFDialogueType.MultipleChoice, localMousePosition);
                    _graphView.AddElement(node);

                    return true; 
                }
                case Group _:
                {
                    _graphView.CreateGroup("DialogueGroup", localMousePosition);

                    return true;
                }
                default:
                {
                    return false;
                }
            }
        }
    }

}