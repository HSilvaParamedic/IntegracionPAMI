using System;

namespace IntegracionPAMI.APIConsumer.Dto
{
	public class OngoingServiceDto
	{
		public string Id { get; set; }
		public string ResponseAgent { get; set; }
		public DateTime TimeAssigned { get; set; }
		public string Classification { get; set; }
	}
}
