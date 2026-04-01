using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace NovaDot.LoreForger.Elements
{
    using Enumerations;
    using Data.Save;
    using Windows;
    using Utilities;

    public class LFMultipleChoiceNode : LFNode
    {
        public override void Initialize(string nodeName, LFGraphView graphView, Vector2 position)
        {
            base.Initialize(nodeName, graphView, position);

            DialogueType = LFDialogueType.MultipleChoice;

            LFChoiceSaveData choiceData = new LFChoiceSaveData()
            {
                Text = "New Choice"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            /*Main Container*/
            Button addChoiceButton = LFElementUtility.CreateButton("Add Choice", () =>
            {
                LFChoiceSaveData choiceData = new LFChoiceSaveData()
                {
                    Text = "New Choice"
                };

                Choices.Add(choiceData);
                //Port choicePort = CreateChoicePort(choiceData);
                outputContainer.Add(CreateChoicePort(choiceData));
            });

            addChoiceButton.AddToClassList("lf-node__button");
            mainContainer.Insert(1, addChoiceButton);

            /*Output Container*/
            foreach (LFChoiceSaveData choice in Choices)
            {
                //Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(CreateChoicePort(choice));
            }

            RefreshPorts();
            RefreshExpandedState();
        }

        #region Element Creation
        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();
            choicePort.userData = userData;

            LFChoiceSaveData choiceData = (LFChoiceSaveData) userData;

            Button deleteChoiceButton = LFElementUtility.CreateButton("X", () => 
            {
                // At least there needs to be 1 choice
                if (Choices.Count == 1) return;

                if (choicePort.connected) _graphView.DeleteElements(choicePort.connections);

                Choices.Remove(choiceData);
                _graphView.RemoveElement(choicePort);

            });

            deleteChoiceButton.AddToClassList("lf-node__button");

            TextField choiceTextField = LFElementUtility.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });

            choiceTextField.AddClasses(
                "lf-node__text-field",
                "lf-node__choice-text-field",
                "lf-node__text-field__hidden"
                );

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);
            return choicePort;
        }
        #endregion
    }
}