using System;

namespace IntegracionPAMI.APIConsumer.Dto
{
	public class NotificationDto 
	{
		public string NotificationType { get; set; }
		public DateTime TimeSent { get; set; }
		public string ServiceID { get; set; }
		public int Order { get; set; }
		public string ResponseAgent { get; set; }
		public string Clasification { get; set; }
	}
}
