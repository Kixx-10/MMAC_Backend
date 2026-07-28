using Microsoft.EntityFrameworkCore;
using MMAC.Data;
using MMAC.DTOS;

namespace MMAC.Services.UtilityService
{
    public class UtilityService : IUtilityService
    {
        private readonly AppDbContext _context;
        public UtilityService(AppDbContext context)
        {
            _context = context;
        }

        #region Location 
        public async Task<IEnumerable<LocationDTO>> GetLocations()
        {
            try
            {
                var locations = await _context.StateRegion
                   .AsNoTracking()
                   .Select(sr => new LocationDTO
                   {
                       state = sr,
                       districts = _context.District
                       .Where(d => d.SRId == sr.Id && d.SystemUse == "Y")
                           .Where(d => d.SRId == sr.Id)
                           .Select(d => new DistrictDTO
                           {
                               district = d,
                               townships = _context.Township
                                   .Where(t => t.DistrictId == d.DistrictId)
                                   .ToList()
                           }).ToList()
                   })
                   .ToListAsync();

                return locations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching locations: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region NRC

        public async Task<IEnumerable<NrcDTO>> GetNrcFormat()
        {
            try
            {

                var nrcs = await _context.NRC_StateRegion.AsNoTracking().Select(sr => new NrcDTO
                {
                    nrcState = sr,
                    nrcTownships = _context.NRC_Township
                                        .Where(t => t.NRC_SRId == sr.Id)
                                        .ToList()
                })
                    .ToListAsync();

                return nrcs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching NRCs: {ex.Message}");
                throw;
            }
        }

        #endregion


    }
}
