using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class ImportedFile
    {
        #region Polja
        private int id;
        private string fileName;
        #endregion

        #region Konstruktori
        public ImportedFile(int id, string fileName)
        {
            this.id = id;
            this.fileName = fileName;
        }

        public ImportedFile()
        {
        }
        #endregion

        #region Svojstva
        [DataMember]
        public int Id { get => id; set => id = value; }
        [DataMember]
        public string FileName { get => fileName; set => fileName = value; }
        #endregion

        #region Ispis
        public override string ToString()
        {
            return Id + " " + FileName;
        }
        #endregion
    }
}