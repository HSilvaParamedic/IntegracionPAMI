using System;

namespace IntegracionPAMI.APIConsumer.Dto
{
	public class OngoingSerivce
	{
		public string Id { get; set; }
		public int ResponseAgent { get; set; }
		public DateTime TimeAssigned { get; set; }
		public string Clasification { get; set; }
	}
}
