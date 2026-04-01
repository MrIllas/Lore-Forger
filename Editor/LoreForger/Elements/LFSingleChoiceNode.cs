using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace NovaDot.LoreForger.Elements   
{
    using Enumerations;
    using Data.Save;
    using Windows;
    using Utilities;

    public class LFSingleChoiceNode : LFNode
    {
        public override void Initialize(string nodeName, LFGraphView graphView, Vector2 position)
        {
            base.Initialize(nodeName, graphView, position);

            DialogueType = LFDialogueType.SingleChoice;

            //LFChoiceSaveData choiceData = new LFChoiceSaveData() { Text = "Next Dialogue" };

            Choices.Add(new LFChoiceSaveData() 
            { 
                Text = "Next Dialogue" 
            });
        }

        public override void Draw()
        {
            base.Draw();

            foreach (LFChoiceSaveData choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);
                choicePort.userData = choice;
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}