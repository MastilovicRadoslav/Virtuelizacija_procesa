using Common;
using Data_Base;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SearchOption = System.IO.SearchOption;

#region Globalne promenljive
public static class Globals
{
	private static int impfileCounterDB = 1;
	private static int impfileCounterInMemory = 1;
	private static List<Load> loads = new List<Load>();
	private static List<ImportedFile> importedFiles = new List<ImportedFile>();
	private static List<Audit> errorAudits = new List<Audit>();

	public static int ImpfileCounterDB { get => impfileCounterDB; set => impfileCounterDB = value; }
	public static int ImpfileCounterInMemory { get => impfileCounterInMemory; set => impfileCounterInMemory = value; }
	public static List<Load> Loads { get => loads; set => loads = value; }
	public static List<ImportedFile> ImportedFiles { get => importedFiles; set => importedFiles = value; }
	public static List<Audit> ErrorAudits { get => errorAudits; set => errorAudits = value; }
}
#endregion

namespace Server
{
	public class Connection : IConnection, IDisposable
	{
		private readonly string databaseType = ConfigurationManager.AppSettings["DatabaseType"];
		private readonly string deviationFormulaType = ConfigurationManager.AppSettings["DeviationFormulaType"];
		private MemoryStream memoryStream;
		private readonly InMemoryDB inMemory = new InMemoryDB();
		private readonly MyDB db = new MyDB();

		public bool DataProcessing(MemoryStream compressedFolders)
		{
			memoryStream = compressedFolders;
			string folderName = "Projekat_Zadatak1";
			string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			string[] directories = Directory.GetDirectories(desktopPath, folderName, SearchOption.AllDirectories);
			string path = directories[1].Replace(".vs\\Projekat_Zadatak1", "Server");
			string[] paths = ExtractZipAndReturnFolderPaths(compressedFolders, path);

			inMemory.RegisterEventHandler(); // Inicijalizacija Eventa za upis u In-Memory bazu
			db.RegisterEventHandler(); // Inicijalizacija Eventa za upis u XML bazu
			ReadCsvFileForecast(paths[0]); // Funkcija koja obrađuje prognozirane podatke iz CSV fajlova
			ReadCsvFileMeasured(paths[1]); // Funkcija koja obrađuje izmerene podatke iz CSV fajlova
			ValueCheck(Globals.Loads); // Funkcija koja izbacuje nevalidne vrednosti
			CalculateDeviations(Globals.Loads); // Funkcija koja računa absolutno i kvadratno odstupanje
			CreateXmlSubset(); // Funkcija koja deli glavnu bazu na pod baze sa svaki Load objekat po fajlu
			inMemory.RegisterEventHandler(); // Brisanje Eventa za In-Memory bazu
			db.RemoveEventHandlers(); // Brisanje Eventa za XML bazu
			DisposeMemoryStream(compressedFolders); // Dispose pattern
			return true;
		}

		#region Funkcija koja kreira CSV folder na Serveru
		private string[] ExtractZipAndReturnFolderPaths(MemoryStream memoryStream, string destinationPath)
		{
			var folderPaths = new List<string>();

			using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
			{
				foreach (ZipArchiveEntry entry in archive.Entries)
				{
					if (entry.FullName.EndsWith("/"))
					{
						string folderPath = Path.Combine(destinationPath, entry.FullName.TrimEnd('/'));
						folderPaths.Add(folderPath);
						Directory.CreateDirectory(folderPath);
					}
					else
					{
						string filePath = Path.Combine(destinationPath, entry.FullName);
						entry.ExtractToFile(filePath, true);
					}
				}
			}
			return folderPaths.ToArray();
		}
		#endregion

		#region Funkcija koja određuje absolutno i kvadratno odstupanje
		private void CalculateDeviations(List<Load> loads)
		{
			for (int i = 0; i < loads.Count; i++)
			{
				if (deviationFormulaType == "Apsolutno")
				{
					loads[i].AbsolutePercentageDeviation = CalculateAbsolutePercentageDeviation(loads[i].ForecastValue, loads[i].MeasuredValue);
				}
				if (deviationFormulaType == "Kvadratno")
				{
					loads[i].SquaredDeviation = CalculateSquaredDeviation(loads[i].ForecastValue, loads[i].MeasuredValue);
				}
			}
			if (databaseType == "XML")
			{
				db.TriggerEvent(loads, "Load.xml");
			}
			else if (databaseType == "InMemory")
			{
				for (int i = 0; i < loads.Count; i++)
				{
					inMemory.TriggerEvent(loads[i]);
				}
			}
		}
		#endregion

		#region Funkcija koja računa absolutno odstupanje
		private double CalculateAbsolutePercentageDeviation(double forecastValue, double measuredValue)
		{
			return ((Math.Abs(measuredValue - forecastValue)) / measuredValue) * 100;
		}
		#endregion

		#region Funkcija koja računa kvadratno odstupanje
		private double CalculateSquaredDeviation(double forecastValue, double measuredValue)
		{
			return Math.Pow((measuredValue - forecastValue) / measuredValue, 2);
		}
		#endregion

		#region Funkcija koja računa broj linija u CSV fajlu
		public static int CountLinesInCsvFile(string filePath)
		{
			int lineCount = 0;
			using (var reader = new StreamReader(filePath))
			{
				while (reader.ReadLine() != null)
				{
					lineCount++;
				}
			}
			return lineCount;
		}
		#endregion

		#region Funkcija za obradu prognoziranih podataka
		public void ReadCsvFileForecast(string filePath)
		{
			string[] newPath = Directory.GetFiles(filePath);
			List<string> importedFiles = new List<string>();
			List<Load> writeLoad = new List<Load>();
			foreach (string file in newPath)
			{
				if (Validate(file))
				{
					importedFiles.Add(file);
					using (TextFieldParser parser = new TextFieldParser(file))
					{
						parser.TextFieldType = FieldType.Delimited;
						parser.SetDelimiters(",");
						List<string> newS = ParsingFromCSVFile(parser);
						List<double> doubles = GetForecastValue(newS);
						List<DateTime> dt = GetDateTime(newS);
						List<string> forecastFileNames = GetForecastFileNames(file);
						Load load = null;
						for (int i = 0; i < doubles.Count; i++)
						{
							load = new Load(i + 1, dt[i], doubles[i], 0, 0, 0, forecastFileNames[i], null);
							writeLoad.Add(load);
						}
						if (databaseType == "XML")
						{
							AddToDBLoad(writeLoad, load);
							ProcessedFileDB(importedFiles);
						}
					}
				}
				else
				{
					CreatingAuditObjects(file);
				}
			}
			for (int i = 0; i < writeLoad.Count; i++)
			{
				Globals.Loads.Add(writeLoad[i]);
			}
			Globals.ImportedFiles = ProcessedFileInMemory(importedFiles);
		}
		#endregion

		#region Funkcija za obradu izmerenih podataka
		public void ReadCsvFileMeasured(string filePath)
		{
			string[] newPath = Directory.GetFiles(filePath);
			int counter = Globals.ErrorAudits.Count;
			List<string> importedFiles = new List<string>();
			List<Load> writeLoad = new List<Load>();

			foreach (string file in newPath)
			{
				bool fileValidation = Validate(file);
				if (fileValidation)
				{
					importedFiles.Add(file);
					using (TextFieldParser parser = new TextFieldParser(file))
					{
						parser.TextFieldType = FieldType.Delimited;
						parser.SetDelimiters(",");
						List<string> newS = ParsingFromCSVFile(parser);
						List<double> doubles = GetMeasuredValue(newS);
						List<string> measuredFileNames = GetMeasuredFileNames(file);
						Load load = null;
						for (int i = 0; i < doubles.Count; i++)
						{
							load = new Load(i + 1, new DateTime(), 0, doubles[i], 0, 0, null, measuredFileNames[i]);
							writeLoad.Add(load);
						}
						for (int i = 0; i < writeLoad.Count; i++)
						{
							Globals.Loads[i].MeasuredValue = writeLoad[i].MeasuredValue;
							Globals.Loads[i].MeasuredFileId = writeLoad[i].MeasuredFileId;
						}
						if (databaseType == "XML")
						{
							AddToDBLoad(Globals.Loads, load);
							ProcessedFileDB(importedFiles);
						}
					}
				}
				else
				{
					CreatingAuditObjects(file);
				}
			}
			if (databaseType == "XML")
			{
				List<ImportedFile> temp = db.DeSerializeObject<List<ImportedFile>>("ImportedFile.xml");
				Globals.ImportedFiles = Globals.ImportedFiles.Concat(temp).ToList();
				db.TriggerEvent(Globals.ImportedFiles, "ImportedFile.xml");
			}
			else if (databaseType == "InMemory")
			{
				for (int i = 0; i < Globals.Loads.Count; i++)
				{
					inMemory.AddObject(Globals.Loads[i]);
				}

				Globals.ImportedFiles = Globals.ImportedFiles.Concat(ProcessedFileInMemory(importedFiles)).ToList();
				foreach (ImportedFile l in Globals.ImportedFiles)
				{
					inMemory.TriggerEvent(l);
				}
			}
		}
		#endregion

		#region Funkcija za parsiranje podatak iz CSV fajla
		private List<string> ParsingFromCSVFile(TextFieldParser parser)
		{
			List<string> retV = new List<string>();
			while (!parser.EndOfData)
			{
				string[] fields = parser.ReadFields();
				foreach (string field in fields)
				{
					retV.Add(field + " ");
				}
			}
			return retV;
		}
		#endregion

		#region Funkcija koja preuzima ForecastValue iz CSV fajlova
		public List<double> GetForecastValue(List<string> value)
		{
			List<double> fValues = new List<double>();
			for (int i = 2; i < value.Count; i += 3)
			{
				var result = double.TryParse(value[i + 2].Replace(".", ","), out double fValue);
				if (result && fValue > 0)
				{
					fValues.Add(Double.Parse(value[i + 2].Replace(".", ",")));
				}
				else
				{
					fValues.Add(1);
				}
			}
			return fValues;
		}
		#endregion

		#region Funkcija koja preuzima DateTime iz CSV fajla
		public List<DateTime> GetDateTime(List<string> value)
		{
			List<DateTime> dateTimes = new List<DateTime>();
			for (int i = 2; i < value.Count; i += 3)
			{
				string time = value[i] + value[i + 1];
				DateTime dateTime = DateTime.Parse(time);
				dateTimes.Add(dateTime);
			}
			return dateTimes;
		}
		#endregion

		#region Funkcija koja proverava da li je fajl validan ili nije
		private bool Validate(string file)
		{
			bool retBoolV = true;
			int numberOfLines = CountLinesInCsvFile(file);

			if (numberOfLines == 0) // Prazan CSV fajl
			{
				retBoolV = false;
			}
			else if (numberOfLines > 25) // Ako ima više sati nego 24 + 1 zboh naslova
			{
				retBoolV = false;
			}
			else if (numberOfLines < 24) // Ako ima manje redova od 24 pošto ima toliko sati u danu 
			{
				retBoolV = false;
			}
			return retBoolV;
		}
		#endregion

		#region Funkcija koja ažurira i upisuje Load listu u XML bazu podataka
		private void AddToDBLoad(List<Load> loads, Load load)
		{
			for (int i = 0; i < loads.Count; i++)
			{
				if (loads[i].Timestamp == load.Timestamp)
				{
					loads[i].Id = load.Id;
					loads[i].Timestamp = load.Timestamp;
					loads[i].ForecastValue = load.ForecastValue;
					loads[i].MeasuredValue = load.MeasuredValue;
					loads[i].AbsolutePercentageDeviation = load.AbsolutePercentageDeviation;
					loads[i].SquaredDeviation = load.SquaredDeviation;
					loads[i].ForecastFileId = load.ForecastFileId;
					loads[i].MeasuredFileId = load.MeasuredFileId;
				}
			}
			db.TriggerEvent(loads, "Load.xml");
		}
		#endregion

		#region Funkcija koja upisuje ImportedFile u XML bazu podataka
		private void ProcessedFileDB(List<string> file)
		{
			List<ImportedFile> impFiles = new List<ImportedFile>();
			for (int i = 0; i < file.Count; i++)
			{
				string[] parsedString = file[i].Split('\\');
				string fileName = parsedString[parsedString.Length - 1];
				ImportedFile ImpFile = new ImportedFile(Globals.ImpfileCounterDB, fileName);
				impFiles.Add(ImpFile);
				Globals.ImpfileCounterDB++;
			}
			db.TriggerEvent(Globals.ImportedFiles, "ImportedFile.xml");
			Globals.ImpfileCounterDB = 1;
		}
		#endregion

		#region Funkcija koja kreira listu ImportedFile za InMemory bazu
		private List<ImportedFile> ProcessedFileInMemory(List<string> file)
		{
			List<ImportedFile> impFiles = new List<ImportedFile>();
			for (int i = 0; i < file.Count; i++)
			{
				string[] parsedString = file[i].Split('\\');
				string fileName = parsedString[parsedString.Length - 1];
				ImportedFile ImpFile = new ImportedFile(Globals.ImpfileCounterInMemory, fileName);
				impFiles.Add(ImpFile);
				Globals.ImpfileCounterInMemory++;
			}
			Globals.ImpfileCounterInMemory = 1;
			return impFiles;
		}
		#endregion

		#region Funkcija koja nalazi naziv za ForecastFile 
		private List<string> GetForecastFileNames(string file)
		{
			List<string> findNames = new List<string>();

			for (int i = 0; i < file.Length; i++)
			{
				string[] parsedString = file.Split('\\');
				string fileName = parsedString[parsedString.Length - 1].Replace(".csv", "");
				findNames.Add(fileName);
			}
			return findNames;
		}
		#endregion

		#region Funkcija koja nalazi naziv za MeasuredFile
		private List<string> GetMeasuredFileNames(string file)
		{
			List<string> findNames = new List<string>();

			for (int i = 0; i < file.Length; i++)
			{
				string[] parsedString = file.Split('\\');
				string fileName = parsedString[parsedString.Length - 1].Replace(".csv", "");
				findNames.Add(fileName);
			}
			return findNames;
		}
		#endregion

		#region Funkcija koja preuzima MeasuredValue iz CSV fajlova
		private List<double> GetMeasuredValue(List<string> value)
		{
			List<double> mValues = new List<double>();
			for (int i = 3; i < value.Count; i += 2)
			{
				int a = 0;
				if (value[i].GetType() == a.GetType())
				{
					mValues.Add(Double.Parse(value[i].Replace(".", ",")));
				}
				else
				{
					var result = double.TryParse(value[i].Replace(".", ","), out double mValue);
					if (result && mValue > 0)
					{
						mValues.Add(Double.Parse(value[i].Replace(".", ",")));
					}
					else
					{
						mValues.Add(1);
					}
				}
			}
			return mValues;
		}
		#endregion

		#region Funkcija koja deli XML po datumima
		private void CreateXmlSubset()
		{
			if (File.Exists("Load.xml"))
			{
				List<Load> loadsFromXML = db.DeSerializeObject<List<Load>>("Load.xml");
				_ = new List<Load>();
				int j = 17;

				for (int i = 0; i < loadsFromXML.Count; i += 24)
				{
					List<Load> temp = loadsFromXML.GetRange(i, 24);
					db.TriggerEventSigle(temp, "Load" + j + ".xml", "LoadsByDate");
					j++;
					temp.Clear();
				}
			}
		}
		#endregion

		#region Funkcija koja kreira audit objekte i upisuje u bazu
		private void CreatingAuditObjects(string file)
		{
			int counter = 1;
			List<Audit> invalidFiles = new List<Audit>();
			string[] dateTimeString = file.Split('_');
			string dateTimeS = dateTimeString[2] + "-" + dateTimeString[3] + "-" + dateTimeString[4].Replace(".csv", "");
			DateTime dateTime = DateTime.Parse(dateTimeS);
			Audit audit = new Audit(counter, dateTime, "Greska! Nije prosla validacija!", MessageType.Error);
			invalidFiles.Add(audit);
			Globals.ErrorAudits.Add(audit);
			if (databaseType == "XML")
			{
				counter++;
				db.TriggerEvent(Globals.ErrorAudits, "Audit.xml");
			}
			else if (databaseType == "InMemory")
			{
				counter++;
				for (int i = 0; i < invalidFiles.Count; i++)
				{
					inMemory.TriggerEvent(Globals.ErrorAudits[i]);
				}
			}
		}
		#endregion

		#region Funkcija koja proverava da li su vrednosti ispravne
		private void ValueCheck(List<Load> loads)
		{
			Load temp = new Load();
			int counter = 0;
			for (int i = 0; i < loads.Count; i++)
			{
				if (loads[i].ForecastValue == 1 || loads[i].MeasuredValue == 1)
				{
					counter++;
					temp = loads[i];
					loads.Remove(loads[i]);
				}
			}
			if (counter != 0)
			{
				CreatingAuditObjects(temp.ForecastFileId);
			}
		}
		#endregion

		#region Dispose pattern
		private void DisposeMemoryStream(MemoryStream memoryStream)
		{
			memoryStream.Dispose();
		}

		private bool disposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					memoryStream.Dispose();
				}
				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~Connection()// Finalizer
		{
			Dispose(false);
		}
		#endregion

		#region Funkcija koja šalje Load objekte iz baze klijentu
		public List<Load> PrintLoad()
		{
			List<Load> listLoad = new List<Load>();
			if (databaseType == "InMemory")
			{
				foreach (Load load in inMemory.dictLoad.Values)
				{
					listLoad.Add(load);
				}
			}
			if (databaseType == "XML")
			{
				listLoad = db.DeSerializeObject<List<Load>>("Load.xml");
			}
			return listLoad;
		}
		#endregion

		#region Funkcija koja šalje ImportedFile objekte iz baze klijentu
		public List<ImportedFile> PrintImportedFile()
		{
			List<ImportedFile> listImported = new List<ImportedFile>();
			if (databaseType == "InMemory")
			{
				foreach (ImportedFile f in inMemory.dictImportedFile.Values)
				{
					listImported.Add(f);
				}
			}
			if (databaseType == "XML")
			{
				listImported = db.DeSerializeObject<List<ImportedFile>>("ImportedFile.xml");
			}
			return listImported;
		}
		#endregion

		#region Funkcija koja šalje Audit objekte iz baze klijentu
		public List<Audit> PrintAudit()
		{
			List<Audit> listAudits = new List<Audit>();
			if (databaseType == "InMemory")
			{
				foreach (Audit a in inMemory.dictAudit.Values)
				{
					listAudits.Add(a);
				}
			}
			if (databaseType == "XML")
			{
				string binDebugPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Debug");
				binDebugPath += "\\Audit.xml";

				if (!File.Exists(binDebugPath))
				{
					listAudits = null;
				}
				else
				{
					listAudits = db.DeSerializeObject<List<Audit>>("Audit.xml");
				}
			}
			return listAudits;
		}
		#endregion
	}
}