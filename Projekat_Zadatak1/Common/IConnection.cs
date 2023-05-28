using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IConnection
    {
        [OperationContract]
        bool DataProcessing(MemoryStream filePath);

        [OperationContract]
        List<Load> PrintLoad();

        [OperationContract]
        List<ImportedFile> PrintImportedFile();

        [OperationContract]
        List<Audit> PrintAudit();
    }
}