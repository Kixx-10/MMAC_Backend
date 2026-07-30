namespace MMAC.DTOS
{
    public class ArrivalSubmitResponseDTO
    {
        public Guid ApplicationNo { get; set; }
        public string ReferenceNo { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        //Extra Id
        public Guid TravellerId { get; set; }
    }
}
