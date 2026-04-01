using UnityEngine;

namespace NovaDot.LoreForger.SO
{
    public class LFDialogueGroupSO : ScriptableObject
    {
        [field: SerializeField] public string GroupName {  get; set; }

        public void Initialize (string groupName)
        {
            GroupName = groupName;
        }
    }
}
