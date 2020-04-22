using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace Components.Locker
{
    public enum FileType
    {
        Binary,
        XML,
    }
    class FileConverter<T>
    {
        public static void Save(object obj, string path, FileType type)
        {
            switch (type)
            {
                case FileType.Binary:
                    SaveToBinaryFile(obj, path);
                    break;
                case FileType.XML:
                    SaveToXMLFile(obj, path);
                    break;
                default:
                    break;
            }
        }

        public static T Load(string path, FileType type)
        {
            T result = default(T);
            switch (type)
            {
                case FileType.Binary:
                    result = LoadFromBinaryFile(path);
                    break;
                case FileType.XML:
                    result = LoadFromXMLFile(path);
                    break;
                default:
                    break;
            }
            return result;
        }

        #region Binary
        /// <summary>
        /// オブジェクトの内容をファイルから読み込み復元する
        /// </summary>
        /// <param name="path">読み込むファイル名</param>
        /// <returns>復元されたオブジェクト</returns>
        private static T LoadFromBinaryFile(string path)
        {
            FileStream fs = new FileStream(path,
                FileMode.Open,
                FileAccess.Read);
            BinaryFormatter f = new BinaryFormatter();
            //読み込んで逆シリアル化する
            object obj = f.Deserialize(fs);
            fs.Close();

            return (T)obj;
        }

        /// <summary>
        /// オブジェクトの内容をファイルに保存する
        /// </summary>
        /// <param name="obj">保存するオブジェクト</param>
        /// <param name="path">保存先のファイル名</param>
        private static void SaveToBinaryFile(object obj, string path)
        {
            FileStream fs = new FileStream(path,
                FileMode.Create,
                FileAccess.Write);
            BinaryFormatter bf = new BinaryFormatter();
            //シリアル化して書き込む
            bf.Serialize(fs, obj);
            fs.Close();
        }
        #endregion

        #region XML
        private static List<Tuple<Type, XmlSerializer>> XMLSerializer_Instance { get; set; } = new List<Tuple<Type, XmlSerializer>>();

        internal static void AddXMLSerializerType(Type t)
        {
            GetInstance(t);
        }

        private static XmlSerializer GetInstance(Type t)
        {
            var instance = XMLSerializer_Instance.Find(x => x.Item1 == t);
            if (instance == null)
            {
                var serializer = new XmlSerializer(t);
                instance = new Tuple<Type, XmlSerializer>(t, serializer);
                XMLSerializer_Instance.Add(instance);
            }
            return instance.Item2;
        }

        private static T LoadFromXMLFile(string path)
        {
            object result;
            var instance = GetInstance(typeof(T));
            var xmlSettings = new System.Xml.XmlReaderSettings() { CheckCharacters = false };
            using (var streamReader = new StreamReader(path, new System.Text.UTF8Encoding(false)))
            {
                result = instance.Deserialize(streamReader);
            }

            return (T)result;
        }

        private static void SaveToXMLFile(object obj, string path)
        {
            var instance = GetInstance(obj.GetType());
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                instance.Serialize(sw, (T)obj);
                sw.Flush();
            }
        }
        #endregion
    }
}
