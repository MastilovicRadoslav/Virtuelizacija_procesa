using Common;
using System.Collections.Generic;

namespace Data_Base
{
	public class InMemoryDB
	{
		public Dictionary<int, Load> dictLoad = new Dictionary<int, Load>(); // In-Memory baza za Load
		public Dictionary<int, Audit> dictAudit = new Dictionary<int, Audit>(); // In-Memory baza za Audit
		public Dictionary<int, ImportedFile> dictImportedFile = new Dictionary<int, ImportedFile>(); // In-Memory baza za ImportedFile

		private static int loadCounter = 1; // Brojač za Load objekte - koristi se kao ID za dictLoad
		private static int auditCounter = 1; // Brojač za Audit objekte - koristi se kao ID za dictAudit
		private static int importedFileCounter = 1; // Brojač za ImportedFile objekte - koristi se kao ID za dictImportedFile

		public delegate void InMemoryEventHandler(object o); //Delegat za In-Memory bazu
		public static event InMemoryEventHandler InMemoryEvent; // Event za iIn-Memory bazu

		#region Funkcija za dodavanje u In-Memory bazu
		public void AddObject(object o)
		{
			Load load = new Load();
			if (o.GetType() == load.GetType())
			{
				_ = new Load();
				Load newObject = (Load)o;
				dictLoad.Add(loadCounter, newObject);
				loadCounter++;
			}

			Audit audit = new Audit();
			if (o.GetType() == audit.GetType())
			{
				_ = new Audit();
				Audit newObject = (Audit)o;
				dictAudit.Add(auditCounter, newObject);
				auditCounter++;
			}

			ImportedFile file = new ImportedFile();
			if (o.GetType() == file.GetType())
			{
				_ = new ImportedFile();
				ImportedFile newObject = (ImportedFile)o;
				dictImportedFile.Add(importedFileCounter, newObject);
				importedFileCounter++;
			}
		}
		#endregion

		#region Funkcije za Event
		public void RegisterEventHandler()
		{
			InMemoryEvent += AddObject;
		}

		public void TriggerEvent(object o)
		{
			InMemoryEvent(o);
		}

		public void RemoveEventHandler()
		{
			InMemoryEvent -= AddObject;
		}
		#endregion
	}
}