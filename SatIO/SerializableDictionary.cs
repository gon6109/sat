using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SatIO
{
    /// <summary>
    /// シリアル化可能な Dictionary クラス
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SerializableDictionary<Tkey, TValue> : Dictionary<Tkey, TValue>, IXmlSerializable
    {
        public SerializableDictionary(Dictionary<Tkey, TValue> dictionary)
        {
            foreach (var item in dictionary)
            {
                Add(item.Key, item.Value);
            }
        }
        public SerializableDictionary()
        {
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(KeyValue));

            reader.Read();
            if (reader.IsEmptyElement)
                return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                KeyValue kv = serializer.Deserialize(reader) as KeyValue;
                if (kv != null)
                    Add(kv.Key, kv.Value);
            }
            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(KeyValue));
            foreach (var key in Keys)
            {
                serializer.Serialize(writer, new KeyValue(key, this[key]));
            }
        }

        public class KeyValue
        {
            public KeyValue(Tkey key, TValue value)
            {
                Key = key;
                Value = value;
            }

            public KeyValue()
            {
            }

            public Tkey Key { get; set; }
            public TValue Value { get; set; }
        }
    }
}
