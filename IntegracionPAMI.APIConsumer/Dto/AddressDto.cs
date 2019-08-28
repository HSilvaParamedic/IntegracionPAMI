namespace IntegracionPAMI.APIConsumer.Dto
{
	public class AddressDto
	{
		public string StreetName { get; set; }
		public string HouseNumber { get; set; }
		public string BetweenStreet1 { get; set; }
		public string BetweenStreet2 { get; set; }
		public string FloorApt { get; set; }
		public string AdditionalData { get; set; }
		public string Province { get; set; }
		public string Deppartment { get; set; }
		public string City { get; set; }
		public string Neighborhood { get; set; }
		public LatLngDto LatLng { get; set; }
		public string PointOfReference { get; set; }
	}
}
