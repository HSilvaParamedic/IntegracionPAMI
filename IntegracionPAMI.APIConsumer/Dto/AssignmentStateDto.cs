﻿using System;

namespace IntegracionPAMI.APIConsumer.Dto
{
	public class AssignmentStateDto
	{
		public int Order { get; set; }
		public string Description { get; set; }
		public DateTime? TimeReported { get; set; }
	}
}
