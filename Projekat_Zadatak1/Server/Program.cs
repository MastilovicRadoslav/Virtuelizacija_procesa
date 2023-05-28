using System;
using System.ServiceModel;

namespace Server
{
	public class Program
	{
		static void Main()
		{
			using (ServiceHost host = new ServiceHost(typeof(Connection)))
			{
				host.Open();
				Console.WriteLine("The service has been successfully started!!!");
				Console.ReadKey();
				host.Close();
			}
		}
	}
}