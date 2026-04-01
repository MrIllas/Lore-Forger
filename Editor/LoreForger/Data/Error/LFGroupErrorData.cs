using NovaDot.LoreForger.Elements;
using System.Collections.Generic;

namespace NovaDot.LoreForger.Data.Error
{
    public class LFGroupErrorData
    {
        public LFErrorData ErrorData { get; set; }
        public List<LFGroup> Groups { get; set; }

        public LFGroupErrorData()
        {
            ErrorData = new LFErrorData();
            Groups = new List<LFGroup>();
        }
    }
}
