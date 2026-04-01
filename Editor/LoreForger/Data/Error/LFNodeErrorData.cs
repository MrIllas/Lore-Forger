using System.Collections.Generic;

namespace NovaDot.LoreForger.Data.Error
{
    using Elements;

    public class LFNodeErrorData
    {
        public LFErrorData ErrorData { get; set; }
        public List<LFNode> Nodes { get; set; }

        public LFNodeErrorData()
        {
            ErrorData = new LFErrorData();
            Nodes = new List<LFNode>();
        }
    }
}
