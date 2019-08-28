namespace IntegracionPAMI.APIConsumer.Dto
{
	public class ServiceDto : SuccessState
	{
		public string Id { get; set; }
		public string ResponseAgent { get; set; }
		public string Classification { get; set; }
		public string ResponseAgentClass { get; set; }
		public AddressDto Address { get; set; }
		public string CustomerName { get; set; }
		public string BeneficiaryID { get; set; }
		public string BeneficiaryName { get; set; }
		public string Gender { get; set; }
		public int Age { get; set; }
		public string AgeUnit { get; set; }
		public string OriginComments { get; set; }
		public string AssignmentComments { get; set; }
		public TriageDto[] Triage { get; set; }
		public AssignmentStateDto[] AssignmentStates { get; set; }
		public AttributeDto[] Attributes { get; set; }
		public string DiagnosticCode { get; set; }
		public string DiagnosticDescription { get; set; }
		public string UrgencyDegreeCode { get; set; }
		public string UrgencyDegreeDescription { get; set; }
		public string FinalDestinationCode { get; set; }
		public string FinalDestinationDescription { get; set; }
		public string HeathcareCenter { get; set; }
	}
}
