using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatIO.MapEventIO
{
    [Serializable()]
    public class TalkComponentIO : MapEventComponentIO
    {
        public List<BaseTalkElementIO> TalkElements { get; set; }

        public TalkComponentIO()
        {
            TalkElements = new List<BaseTalkElementIO>();
        }

        [Serializable()]
        public abstract class BaseTalkElementIO
        {
            public string CharacterName { get; set; }
        }

        [Serializable()]
        public class ShowCharacterElementIO : BaseTalkElementIO
        {
            public int Index { get; set; }
        }

        [Serializable()]
        public class TalkElementIO : BaseTalkElementIO
        {
            public string Text { get; set; }
        }

        [Serializable()]
        public class ChangeDiffElementIO : BaseTalkElementIO
        {
            public string DiffImage { get; set; }
        }

        [Serializable()]
        public class HideCharacterElementIO : BaseTalkElementIO
        {
        }
    }
}
