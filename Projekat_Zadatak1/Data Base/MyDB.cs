using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Data_Base
{
    public class MyDB
    {
        public delegate void XMLDBWriteEventHandler<T>(T serializableObject, string fileName);
        public static event XMLDBWriteEventHandler<object> XMLWriteEvent;

        public delegate void XMLDBWriteSingleEventHandler<T>(T serializableObject, string fileName, string folderName);
        public static event XMLDBWriteSingleEventHandler<object> XMLWriteSingleEvent;

        #region Upis u XML fajl
        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion

        #region Upis u XML za Loads po datumima
        public void SerializeSingleObject<T>(T serializableObject, string fileName, string folderName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(Path.Combine(folderName, fileName));
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        #endregion

        #region Čitanje iz XML fajla
        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default; }

            T objectOut = default;

            try
            {
                string attributeXml = string.Empty;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return objectOut;
        }
        #endregion

        #region Funkcije za Event
        public void RegisterEventHandler()
        {
            XMLWriteEvent += SerializeObject;
            XMLWriteSingleEvent += SerializeSingleObject;
        }

        public void TriggerEvent<T>(T serializableObject, string fileName)
        {
            if (XMLWriteEvent != null)
            {
                XMLWriteEvent(serializableObject, fileName);
            }
        }

        public void TriggerEventSigle<T>(T serializableObject, string fileName, string folderName)
        {
            if (XMLWriteEvent != null)
            {
                XMLWriteSingleEvent(serializableObject, fileName, folderName);
            }
        }

        public void RemoveEventHandlers()
        {
            XMLWriteEvent -= SerializeObject;
            XMLWriteSingleEvent -= SerializeSingleObject;
        }
        #endregion
    }
}