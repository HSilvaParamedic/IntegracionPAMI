﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegracionPAMI.APIConsumer.Dto
{
public	class OngoingSerivcesList
	{
		public OngoingSerivce[] SummaryMyProperty { get; set; }
		public DateTime TimeRequested { get; set; }
		public bool IsSeccess { get; set; }
		public string Message { get; set; }
	}
}
