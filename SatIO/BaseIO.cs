using BaseComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SatIO
{
    [Serializable]
    public abstract class BaseIO
    {
        public string Path;

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="path">ファイル</param>
        public void Save(string path)
        {
            using (FileStream file = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(file, this);
            }
        }

        /// <summary>
        /// Xmlで保存する
        /// </summary>
        /// <param name="path"></param>
        public void SaveAsXml(string path)
        {
            using (FileStream file = new FileStream(path, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(this.GetType());
                serializer.Serialize(file, this);
            }
        }

        /// <summary>
        /// ロードする
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">パス</param>
        /// <returns></returns>
        static public T Load<T>(string path) where T : BaseIO
        {
            try
            {
                BinaryFormatter serializer = new BinaryFormatter();
                T data;
                using (var stream = IO.GetStream(path))
                {
                    data = (T)serializer.Deserialize(stream);
                }
                data.Path = System.IO.Path.GetDirectoryName(path);
                return data;
            }
            catch
            {
                return LoadFromXml<T>(path);
            }
        }

        /// <summary>
        /// XMLファイルからロード
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">パス</param>
        /// <returns></returns>
        static public T LoadFromXml<T>(string path) where T : BaseIO
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T data;
            using (var stream = IO.GetStream(path))
            {
                data = (T)serializer.Deserialize(stream);
            }
            data.Path = System.IO.Path.GetDirectoryName(path);
            return data;
        }
    }
}
