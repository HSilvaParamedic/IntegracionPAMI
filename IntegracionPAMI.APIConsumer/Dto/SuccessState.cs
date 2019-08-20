using System;

namespace IntegracionPAMI.APIConsumer.Dto
{
	public class SuccessState
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public DateTime TimeRequested { get; set; }
	}
}
