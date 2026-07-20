using Microsoft.AspNetCore.Mvc;
using ZKTecoAttendanceService.DTO.Dto;
using ZKTecoAttendanceService.Postgre.Services;

namespace ZKTecoAttendanceService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceProcessPostgresqlController : ControllerBase
    {
        private readonly ProcessService _processService;
        public AttendanceProcessPostgresqlController()
        {
            this._processService = new ProcessService();
        }

        // GET: api/AttendanceProcessPostgresql
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> loadAllPunches()
        {
            await _processService.loadAllPunchesAsync();

            return Ok(ApiResponse<string>.SuccessResponse("Completed", "Loaded all punches in the database successfully."));
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> loadCurrentAndPreviousYearPunches()
        {
            await _processService.loadCurrentAndPreviousYearPunchesAsync();

            return Ok(ApiResponse<string>.SuccessResponse("Completed", "Loaded current and previous year punches in the database successfully."));
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> loadCurrentYearPunches()
        {
            await _processService.loadCurrentYearPunchesAsync();

            return Ok(ApiResponse<string>.SuccessResponse("Completed", "Loaded current year punches in the database successfully."));
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> loadCurrentMonthPunches()
        {
            await _processService.loadCurrentMonthPunchesAsync();

            return Ok(ApiResponse<string>.SuccessResponse("Completed", "Loaded current month punches in the database successfully."));
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> loadPunchesByDateRange(DateOnly startDate, DateOnly endDate)
        {
            await _processService.loadPunchesByDateRangeAsync(startDate, endDate);

            return Ok(ApiResponse<string>.SuccessResponse("Completed", $"Loaded punches from {startDate.ToShortDateString()} to {endDate.ToShortDateString()} in the database successfully."));
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> loadAllPunchesByEmployee(string employeeCode)
        {
            await _processService.loadAllPunchesByEmployeeAsync(employeeCode);

            return Ok(ApiResponse<string>.SuccessResponse("Completed", $"Loaded all punches for the employee {employeeCode} in the database successfully."));
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> loadCurrentMonthPunchesByEmployee(string employeeCode)
        {
            await _processService.loadCurrentMonthPunchesByEmployeeAsync(employeeCode);

            return Ok(ApiResponse<string>.SuccessResponse("Completed", $"Loaded current month punches for the employee {employeeCode} in the database successfully."));
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> loadEmployeePunchesByDateRange(string employeeCode, DateOnly startDate, DateOnly endDate)
        {
            await _processService.loadEmployeePunchesByDateRangeAsync(employeeCode, startDate, endDate);

            return Ok(ApiResponse<string>.SuccessResponse("Completed", $"Loaded punches for the employee {employeeCode} from {startDate.ToShortDateString()} to {endDate.ToShortDateString()} in the database successfully."));
        }
    }
}
