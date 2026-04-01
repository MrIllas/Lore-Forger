using System.Collections.Generic;
using UnityEngine;

namespace NovaDot.LoreForger.SO
{
    public class LFDialogueContainerSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; private set; }

        [field: SerializeField] public SerializableDictionary<LFDialogueGroupSO, List<LFDialogueSO>> DialogueGroups { get; set; }
        [field: SerializeField] public List<LFDialogueSO> UngroupedDialogues { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            DialogueGroups = new SerializableDictionary<LFDialogueGroupSO, List<LFDialogueSO>>();
            UngroupedDialogues = new List<LFDialogueSO>();
        }

        public List<string> GetDialogueGroupNames()
        {
            List<string> dialogueGroupNames = new List<string>();

            foreach(LFDialogueGroupSO dialogueGroup in DialogueGroups.Keys)
            {
                dialogueGroupNames.Add(dialogueGroup.GroupName);
            }

            return dialogueGroupNames;
        }

        public List<string> GetGroupedDialogueNames(LFDialogueGroupSO dialogueGroup, bool startingDialoguesOnly)
        {
            List<LFDialogueSO> groupedDialogues = DialogueGroups[dialogueGroup];
            List<string> groupedDialogueNames = new List<string>();

            foreach(LFDialogueSO groupedDialogue in groupedDialogues)
            {
                if (startingDialoguesOnly && !groupedDialogue.IsStartingDialogue) continue;

                groupedDialogueNames.Add(groupedDialogue.DialogueName);
            }

            return groupedDialogueNames;
        }

        public List<string> GetUngroupedDialogueNames(bool startingDialoguesOnly)
        {
            List<string> ungroupedDialogueNames = new List<string>();

            foreach(LFDialogueSO ungroupedDialogue in UngroupedDialogues)
            {
                if (startingDialoguesOnly && !ungroupedDialogue.IsStartingDialogue) continue;

                ungroupedDialogueNames.Add(ungroupedDialogue.DialogueName);
            }

            return ungroupedDialogueNames;
        }
    }
}
