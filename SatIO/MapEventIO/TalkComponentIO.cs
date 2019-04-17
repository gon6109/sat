using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SatIO.MapEventIO
{
    [Serializable()]
    public class TalkComponentIO : MapEventComponentIO
    {
        [XmlArrayItem(typeof(ShowCharacterElementIO))]
        [XmlArrayItem(typeof(TalkElementIO))]
        [XmlArrayItem(typeof(ChangeDiffElementIO))]
        [XmlArrayItem(typeof(HideCharacterElementIO))]
        public List<BaseTalkElementIO> TalkElements;

        public TalkComponentIO()
        {
            TalkElements = new List<BaseTalkElementIO>();
        }

        [Serializable()]
        public abstract class BaseTalkElementIO
        {
            public string CharacterName;
        }

        [Serializable()]
        public class ShowCharacterElementIO : BaseTalkElementIO
        {
            public int Index;
        }

        [Serializable()]
        public class TalkElementIO : BaseTalkElementIO
        {
            public string Text;
        }

        [Serializable()]
        public class ChangeDiffElementIO : BaseTalkElementIO
        {
            public string DiffImage;
        }

        [Serializable()]
        public class HideCharacterElementIO : BaseTalkElementIO
        {
        }
    }
}
