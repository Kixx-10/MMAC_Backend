using Microsoft.EntityFrameworkCore;
using MMAC.Data;
using MMAC.Models.Cores;

namespace MMAC.Repositories
{
    public class CompleteArrivalRepository : ICompleteArrivalRepository
    {
        private readonly AppDbContext _context;

        public CompleteArrivalRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> SubmitArrivalApplicationAsync(Traveller traveller, ArrivalApplication application)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var isNewTraveller = traveller.TravellerId == Guid.Empty;

                if (isNewTraveller)
                {
                    // if new generate new id
                    traveller.TravellerId = Guid.NewGuid();
                    await _context.Traveller.AddAsync(traveller);
                }
                else
                {
                    var trackedTraveller = _context.Traveller.Local.FirstOrDefault(t => t.TravellerId == traveller.TravellerId);

                    if (trackedTraveller != null)
                    {
                        _context.Entry(trackedTraveller).CurrentValues.SetValues(traveller);
                    }
                    else
                    {
                        _context.Traveller.Attach(traveller);
                        _context.Entry(traveller).State = EntityState.Modified;
                    }
                }

                application.TravellerId = traveller.TravellerId;
                await _context.ArrivalApplication.AddAsync(application);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return application.AppNo;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"💡 Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return Guid.Empty;
            }
        }

        //validate 24 hr 
        public async Task<bool> IsDuplicateSubmissionWithin24HoursAsync(string fullName, string passportNo, string issuedCountryCode, DateTime dob)
        {
            var twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);

            return await _context.Traveller
                .AnyAsync(t => t.FullName.ToLower() == fullName.ToLower()
                            && t.PassportNo.ToLower() == passportNo.ToLower()
                            && t.IssuedCountryCode.ToLower() == issuedCountryCode.ToLower()
                            && t.DOB.Date == dob.Date
                            && t.ArrivalApplications.Any(a => a.CreatedDate >= twentyFourHoursAgo));
        }

        public async Task<ArrivalApplication?> GetArrivalApplicationDetailsAsync(Guid appNo)
        {
            return await _context.ArrivalApplication
                .Include(x => x.Traveller)
                .ThenInclude(t => t!.Nationality)
                .Include(x => x.selectedModeOfTravel)
                .Include(x => x.selectedPortOfArrival)
                .Include(x => x.Township)
                    .ThenInclude(t => t!.District)
                        .ThenInclude(d => d!.StateRegion)
                .FirstOrDefaultAsync(a => a.AppNo == appNo);
        }

        public async Task<ArrivalApplication?> GetActiveApplicationByReferenceNoAsync(string referenceNo)
        {
            return await _context.ArrivalApplication
                .Include(a => a.Traveller)
                .FirstOrDefaultAsync(a => a.ReferenceNo == referenceNo && a.AppStatus == "Submitted");

        }
        public async Task<bool> IsReferenceNoExistsAsync(string referenceNo)
        {
            return await _context.ArrivalApplication.AnyAsync(a => a.ReferenceNo == referenceNo);
        }
        public async Task<bool> ApproveApplicationAsync(Guid appNo, string appStatus, string approveUser)
        {
            var application = await _context.ArrivalApplication.FirstOrDefaultAsync(a => a.AppNo == appNo);

            if (application == null)
            {
                return false;
            }

            application.AppStatus = appStatus;
            application.UpdatedDate = DateTime.UtcNow;

            if (appStatus.Equals("Arrived", StringComparison.OrdinalIgnoreCase))
            {
                application.ApprovedDate = DateTime.UtcNow;
                application.ApprovedUser = approveUser;
            }
            //else if (appStatus.Equals("Departed", StringComparison.OrdinalIgnoreCase))
            //{
            //    application.ApprovedDate = null;
            //    application.ApprovedUser = null;
            //}
            else
            {
                application.ApprovedDate = null;
                application.ApprovedUser = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}