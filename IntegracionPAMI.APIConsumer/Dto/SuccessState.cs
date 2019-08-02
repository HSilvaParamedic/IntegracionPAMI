using System;

namespace IntegracionPAMI.APIConsumer.Dto
{
	public abstract class SuccessState
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public DateTime TimeRequested { get; set; }
	}
}
