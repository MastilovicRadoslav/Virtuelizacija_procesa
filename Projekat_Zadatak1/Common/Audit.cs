using System;
using System.Runtime.Serialization;

namespace Common
{
    [Serializable]
    public enum MessageType { [EnumMember] Info, [EnumMember] Warning, [EnumMember] Error }

    [DataContract]
    public class Audit
    {
        #region Polja
        private int id;
        private DateTime timestamp;
        private string message;
        private MessageType type;
        #endregion

        #region Konstruktori
        public Audit(int id, DateTime timestamp, string message, MessageType type)
        {
            this.id = id;
            this.timestamp = timestamp;
            this.message = message;
            this.type = type;
        }

        public Audit()
        {
        }
        #endregion

        #region Svojstva
        [DataMember]
        public int Id { get => id; set => id = value; }
        [DataMember]
        public DateTime Timestamp { get => timestamp; set => timestamp = value; }
        [DataMember]
        public string Message { get => message; set => message = value; }
        [DataMember]
        public MessageType Type { get => type; set => type = value; }
        #endregion

        #region Ispis
        public override string ToString()
        {
            return Id + " " + Timestamp + " " + Message + " " + Type;
        }
        #endregion
    }
}