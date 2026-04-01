using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace NovaDot.LoreForger.Elements
{
    using Enumerations;
    using Utilities;
    using Windows;
    using Data.Save;
    
    public class LFNode : Node
    {
        public string ID;

        public string DialogueName { get; set; }
        public List<LFChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public LFDialogueType DialogueType { get; set; }
        public LFGroup Group { get; set; }

        private Color defaultBackgroundColor;

        protected LFGraphView _graphView;

        public virtual void Initialize(string nodeName, LFGraphView graphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();

            _graphView = graphView;

            DialogueName = nodeName;
            Choices = new List<LFChoiceSaveData>();
            Text = "Dialogue Text.";

            defaultBackgroundColor = new Color(29.0f / 255.0f, 29.0f / 255.0f, 30 / 255.0f);

            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("lf-node__main-container");
            extensionContainer.AddToClassList("lf-node__extension-container");
        }

        public virtual void Draw()
        {
            /*Title Container*/
            TextField dialogueNameTextField = LFElementUtility.CreateTextField(DialogueName, null, callback =>
            {
                TextField target = (TextField) callback.target;
                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(DialogueName)) ++_graphView.NameErrorsAmount;
                }
                else
                {
                    if (string.IsNullOrEmpty(DialogueName)) --_graphView.NameErrorsAmount;
                }

                if (Group == null)
                {
                    _graphView.RemoveUngroupedNodes(this);

                    DialogueName = target.value;

                    _graphView.AddUngroupedNode(this);

                    return;
                }

                LFGroup currentGroup = Group;

                _graphView.RemoveGroupedNode(this, Group);

                DialogueName = target.value;

                _graphView.AddGroupedNode(this, currentGroup);
            });

            dialogueNameTextField.AddClasses(
                "lf-node__text-field",
                "lf-node__filename-text-field",
                "lf-node__text-field__hidden"
                );

            titleContainer.Insert(0, dialogueNameTextField);

            /*Input Container*/
            //Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi));

            /*Extensions Container*/

            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("lf-node__custom-data-container");

            Foldout textFoldout = LFElementUtility.CreateFoldout("Dialogue Text");

            TextField textTextField = LFElementUtility.CreateTextArea(Text, null, callback =>
            {
                Text = callback.newValue;
            });

            textTextField.AddClasses(
                "lf-node__text-field",
                "lf-node__quote-text-field"
                );

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
        }

        #region Overrided Methods

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }

        #endregion

        #region Utility Methods
        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach(Port port in container.Children())
            {
                if (!port.connected) continue;

                _graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port) inputContainer.Children().First();

            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }

        #endregion
    }
}