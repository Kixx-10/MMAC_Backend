namespace MMAC.DTOS
{
    public class ResponseCompleteArrivalDTO
    {
        public Guid TravellerId { get; set; } // to fix foreign key audit log error
        public string ReferenceNo { get; set; } = string.Empty;
        public string AppStatus { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime DOB { get; set; }

        //public string CountryOfBirthCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string VisaNo { get; set; } = string.Empty;
        public string? NRC { get; set; }
        public string? FatherName { get; set; }
        public string? UID { get; set; }
        public string PassportNo { get; set; } = string.Empty;
        public string IssuedCountryCode { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        // Arrival Info
        public DateTime ArrivalDate { get; set; }
        public string? VehicleNumber { get; set; }
        public string? Accommodation { get; set; }
        public string? AddressInMyanmar { get; set; }
        public string MobileNumberMM { get; set; } = string.Empty;
        public string PurposeOfVisit { get; set; } = string.Empty;
        public string? PreviousCity { get; set; }
        public string? HealthDeclaration { get; set; }
        public string? HealthRecordUrl { get; set; }
        public string? DigitalDeclarations { get; set; }

        public string ModeOfTravelName { get; set; } = string.Empty;
        public string PortOfArrivalName { get; set; } = string.Empty;
        public string StateRegionName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string TownshipName { get; set; } = string.Empty;


    }
}