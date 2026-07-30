using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMAC.Services.DashboardService;

namespace MMAC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service) => _service = service;

        /// <summary>Get summary stats + chart data for the dashboard</summary>
        [HttpGet("GetDashboardData")]
        [Authorize]
        public async Task<IActionResult> GetDashboardData(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var data = await _service.GetDashboardDataAsync(fromDate, toDate);
            return Ok(data);
        }

        /// <summary>Get paginated, filtered traveller/application list</summary>
        [HttpGet("GetTravellers")]
        [Authorize]
        public async Task<IActionResult> GetTravellers(
            [FromQuery] string? status,
            [FromQuery] int? regionId,
            [FromQuery] int? districtId,
            [FromQuery] int? townshipId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _service.GetFilteredTravellersAsync(
                status, regionId, districtId, townshipId, fromDate, toDate, page, pageSize);
            return Ok(result);
        }
    }
}