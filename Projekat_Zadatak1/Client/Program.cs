using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace Client
{
    public class Program : IDisposable
    {
        private static MemoryStream stream = new MemoryStream();
        static void Main()
        {
            bool fileExists;
            string path;
            do
            {
                Console.WriteLine("Enter the path to the 'csv.zip' folder :");
                path = Console.ReadLine();

                fileExists = CheckCsvZipFileExists(path);
                path += "\\csv.zip";
            }
            while (!fileExists);

            byte[] fileBytes = File.ReadAllBytes(path);
            stream = new MemoryStream(fileBytes);

            ChannelFactory<IConnection> channel = new ChannelFactory<IConnection>("ServiceName");
            IConnection proxy = channel.CreateChannel();

            bool check = proxy.DataProcessing(stream);
            if (check == true)
            {
                Console.WriteLine("\n\nThe data processing is successful!!!\n\n");
                List<Load> loads = proxy.PrintLoad();
                List<ImportedFile> imported = proxy.PrintImportedFile();
                List<Audit> audits = proxy.PrintAudit();

                DisplayLoads(loads);
                Console.Write("\n\n");
                DisplayImported(imported);
                Console.Write("\n\n");
                DisplayAudits(audits);
            }
            else
            {
                Console.WriteLine("\n\nThe data processing is unsuccessful!!!");
            }
            Console.ReadLine();
        }

        #region Funkcija za ispis Audit objekata
        private static void DisplayAudits(List<Audit> audits)
        {
            if (audits == null || audits.Count == 0 )
            {
                Console.Write("The database is empty for Audit objects!!!");
            }
            else
            {
                foreach (Audit a in audits)
                {
                    Console.WriteLine(a);
                }
            }
        }
        #endregion

        #region Funkcija za ispis ImportedFile objekata
        private static void DisplayImported(List<ImportedFile> imported)
        {
            if (imported.Count == 0)
            {
                Console.Write("\n\nThe database is empty for ImportedFile objects!!!");
            }
            else
            {
                foreach (ImportedFile i in imported)
                {
                    Console.WriteLine(i);
                }
            }
        }
        #endregion

        #region Funkcija za ispis Load objekata
        private static void DisplayLoads(List<Load> loads)
        {
            if (loads.Count == 0)
            {
                Console.Write("\n\nThe database is empty for Load objects!!!");
            }
            else
            {
                foreach (Load l in loads)
                {
                    Console.Write(l);
                }
            }
        }
        #endregion

        #region Funkcija koja proverava da li je putanja dobra
        public static bool CheckCsvZipFileExists(string filePath)
        {
            string csvZipPath = Path.Combine(filePath, "csv.zip");
            return File.Exists(csvZipPath);
        }
        #endregion

        #region Dispose pattern
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    stream.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Program() // Finalizer
        {
            Dispose(false);
        }
        #endregion
    }
}