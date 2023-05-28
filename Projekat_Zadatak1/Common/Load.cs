using System;
using System.Runtime.Serialization;

namespace Common
{
	[DataContract]
	public class Load
	{
		#region Polja
		private int id;
		private DateTime timestamp;
		private double forecastValue;
		private double measuredValue;
		private double absolutePercentageDeviation;
		private double squaredDeviation;
		private string forecastFileId;
		private string measuredFileId;
		#endregion

		#region Konstruktori
		public Load(int id, DateTime timestamp, double forecastValue, double measuredValue, double absolutePercentageDeviation, double squaredDeviation, string forecastFileId, string measuredFileId)
		{
			this.id = id;
			this.timestamp = timestamp;
			this.forecastValue = forecastValue;
			this.measuredValue = measuredValue;
			this.absolutePercentageDeviation = absolutePercentageDeviation;
			this.squaredDeviation = squaredDeviation;
			this.forecastFileId = forecastFileId;
			this.measuredFileId = measuredFileId;
		}

		public Load()
		{
		}
		#endregion

		#region Svojstva
		[DataMember]
		public int Id { get => id; set => id = value; }
		[DataMember]
		public DateTime Timestamp { get => timestamp; set => timestamp = value; }
		[DataMember]
		public double ForecastValue { get => forecastValue; set => forecastValue = value; }
		[DataMember]
		public double MeasuredValue { get => measuredValue; set => measuredValue = value; }
		[DataMember]
		public double AbsolutePercentageDeviation { get => absolutePercentageDeviation; set => absolutePercentageDeviation = value; }
		[DataMember]
		public double SquaredDeviation { get => squaredDeviation; set => squaredDeviation = value; }
		[DataMember]
		public string ForecastFileId { get => forecastFileId; set => forecastFileId = value; }
		[DataMember]
		public string MeasuredFileId { get => measuredFileId; set => measuredFileId = value; }
		#endregion

		#region Ispis
		public override string ToString()
		{
			return ($"{Id} {Timestamp} {ForecastValue} {MeasuredValue} {AbsolutePercentageDeviation} {SquaredDeviation} {ForecastFileId} {MeasuredFileId}\n");
		}
		#endregion
	}
}