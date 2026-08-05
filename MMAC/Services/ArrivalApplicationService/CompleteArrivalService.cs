using AutoMapper;
using Hangfire;
using MMAC.DTOS;
using MMAC.Models.Cores;
using MMAC.Repositories;
using MMAC.Services.AuditLogService;
using MMAC.Services.TokenService;

namespace MMAC.Services.ArrivalInterface
{
    public class CompleteArrivalService : ICompleteArrivalService
    {
        private readonly ICompleteArrivalRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly ITokenService _tokenService;
        public CompleteArrivalService(
            ICompleteArrivalRepository repository,
            IMapper mapper, IAuditLogService auditLogService,
            ITokenService tokenService
            )
        {
            _repository = repository;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _tokenService = tokenService;
        }

        public async Task<ArrivalSubmitResponseDTO> SubmitAsync(CompleteArrivalDTO dto)
        {
            try
            {
                if (dto.NationalityCode == "MMR")
                {
                    if (string.IsNullOrWhiteSpace(dto.NRC) || string.IsNullOrWhiteSpace(dto.FatherName))
                    {
                        throw new ArgumentException("Need NRC and FatherName for Myanmar Citizens");
                    }
                }

                string finalReferenceNo = string.Empty;
                Guid currentTravellerId = Guid.Empty;
                bool isUpdateFlow = !string.IsNullOrWhiteSpace(dto.ReferenceNo);

                var traveller = _mapper.Map<Traveller>(dto);
                var arrivalApplication = _mapper.Map<ArrivalApplication>(dto);

                if (isUpdateFlow)
                {

                    //UPDATE APPLICATION FLOW
                    var oldApp = await _repository.GetActiveApplicationByReferenceNoAsync(dto.ReferenceNo!);
                    if (oldApp == null)
                    {
                        throw new KeyNotFoundException($"Active application with reference number '{dto.ReferenceNo}' was not found.");
                    }
                    oldApp.AppStatus = "Invalid";
                    oldApp.UpdatedDate = DateTime.UtcNow;
                    currentTravellerId = oldApp.TravellerId;
                    traveller.TravellerId = currentTravellerId;
                    finalReferenceNo = oldApp.ReferenceNo;
                }
                else
                {

                    // NEW APPLICATION FLOW

                    bool isDuplicateApplication = await _repository.IsDuplicateSubmissionWithin24HoursAsync(
                        dto.FullName, dto.PassportNo, dto.IssuedCountryCode, dto.DOB);

                    if (isDuplicateApplication)
                    {
                        throw new InvalidOperationException("You have already submitted an application within the last 24 hours. Please try again after 24 hours.");
                    }

                    currentTravellerId = Guid.Empty;
                    traveller.TravellerId = Guid.Empty;

                    // new generate Reference No 
                    string currentYear = DateTime.UtcNow.Year.ToString();
                    bool isDuplicate = true;

                    while (isDuplicate)
                    {
                        int randomNumber = Random.Shared.Next(10000000, 99999999);
                        finalReferenceNo = $"MMR-{currentYear}-{randomNumber}";
                        isDuplicate = await _repository.IsReferenceNoExistsAsync(finalReferenceNo);
                    }
                }
                arrivalApplication.AppNo = Guid.NewGuid();
                arrivalApplication.ReferenceNo = finalReferenceNo;
                arrivalApplication.AppStatus = "Submitted";
                arrivalApplication.CreatedDate = DateTime.UtcNow;
                Guid savedAppNo = await _repository.SubmitArrivalApplicationAsync(traveller, arrivalApplication);
                //for arrivaldate checking expire
                var expiryTime = dto.ArrivalDate.AddDays(1).AddHours(1);
                var delay = expiryTime - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    BackgroundJob.Schedule<ICompleteArrivalService>(
                        service => service.AutoExpireApplicationAsync(savedAppNo),
                        delay
                    );
                }


                if (!isUpdateFlow)
                {
                    currentTravellerId = traveller.TravellerId;
                }

                await _auditLogService.LogAsync(isUpdateFlow ? "UPDATE_ARRIVAL_FORM" : "CREATE_ARRIVAL_FORM", new { ReferenceNo = finalReferenceNo, AppNo = savedAppNo }, currentTravellerId);//old currentTravellerId

                return new ArrivalSubmitResponseDTO
                {
                    ApplicationNo = savedAppNo,
                    ReferenceNo = arrivalApplication.ReferenceNo,
                    TravellerId = currentTravellerId    //TravellerId to fix fk key audit log
                };


            }
            catch (ArgumentException) { throw; }
            catch (KeyNotFoundException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SubmitAsync: {ex.Message}");
                throw;
                //var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                //throw new Exception($"Database Error: {innerMessage}");
            }
        }

        public async Task<ResponseCompleteArrivalDTO?> GetScanAsync(Guid AppNo)
        {
            try
            {
                var app = await _repository.GetArrivalApplicationDetailsAsync(AppNo);

                if (app == null)
                {
                    return null;
                }

                if (app.AppStatus?.Equals("Invalid", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return null;
                }
                if (app.AppStatus?.Equals("Expired", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new InvalidOperationException("This Arrival card is Expired.");
                }

                //if (app.AppStatus?.Equals("Departed", StringComparison.OrdinalIgnoreCase) == true)
                //{
                //    throw new InvalidOperationException("This Traveller has already been Departed.");
                //}

                if (app.AppStatus?.Equals("Arrived", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new InvalidOperationException("This Traveller already Arrived.");
                }


                //Success
                var result = _mapper.Map<ResponseCompleteArrivalDTO>(app);

                if (app.Traveller != null) _mapper.Map(app.Traveller, result);

                // Re-apply ArrivalApplication-level fields that may be overwritten by the Traveller mapping above
                result.HealthRecordUrl = app.HealthRecordUrl;
                result.GoodsRecordUrl = app.GoodsRecordUrl;

                if (app.Traveller != null) _mapper.Map(app.Traveller, result);
                if (app.selectedModeOfTravel != null) result.ModeOfTravelName = app.selectedModeOfTravel.ModeOfTravelName;
                if (app.selectedPortOfArrival != null) result.PortOfArrivalName = app.selectedPortOfArrival.PortOfArrivalName;

                if (app.Traveller != null && app.Traveller.Nationality != null)
                {
                    result.Name = app.Traveller.Nationality.Name;
                }

                if (app.Township != null)
                {
                    result.TownshipName = app.Township.Name;
                    if (app.Township.District != null)
                    {
                        result.DistrictName = app.Township.District.Name;
                        if (app.Township.District.StateRegion != null)
                        {
                            result.StateRegionName = app.Township.District.StateRegion.Name;
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not find or map {AppNo}. Exception: {ex.Message}");
                throw;
            }
        }
        //To check hangfire Expire date
        public async Task AutoExpireApplicationAsync(Guid appNo)
        {
            var app = await _repository.GetArrivalApplicationDetailsAsync(appNo);

            if (app != null && app.AppStatus == "Submitted" && app.ArrivalDate.Date < DateTime.UtcNow.Date)
            {
                await _repository.ApproveApplicationAsync(appNo, "Expired", "System-Auto-Expire");
            }
        }
        //
        public async Task<bool> ApproveApplication(Guid AppNo, string AppStatus, string ApproveUser)
        {
            try
            {
                return await _repository.ApproveApplicationAsync(AppNo, AppStatus, ApproveUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ApproveApplication: {ex.Message}");
                throw;
            }
        }
    }
}
