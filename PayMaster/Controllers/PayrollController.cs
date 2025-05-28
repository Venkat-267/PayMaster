using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayMaster.DTO;
using PayMaster.Interface;
using PayMaster.Models;

namespace PayMaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollRepository _payrollRepo;
        private readonly IAdminRepository _adminRepo;
        private readonly PayMasterDbContext _context;

        public PayrollController(IPayrollRepository payrollRepo, IAdminRepository adminRepo, PayMasterDbContext context)
        {
            _payrollRepo = payrollRepo;
            _adminRepo = adminRepo;
            _context = context;
        }

        [Authorize(Roles = "Admin,Payroll-Processor")]
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromQuery] int employeeId, [FromQuery] int month, [FromQuery] int year, [FromQuery] int processedBy)
        {
            try
            {
                var result = await _payrollRepo.GeneratePayrollAsync(employeeId, month, year, processedBy);
                await _adminRepo.GenerateAuditLogAsync(new AuditLogDto
                {
                    UserId = processedBy,
                    Action = "Generate Payroll",
                    Description = $"Payroll generated for EmployeeId={employeeId}, Month={month}, Year={year}"
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Payroll-Processor, Manager")]
        [HttpPost("verify/{payrollId}")]
        public async Task<IActionResult> VerifyPayroll(int payrollId, [FromQuery] int userId)
        {
            var payroll = await _context.Payrolls.FindAsync(payrollId);
            if (payroll == null) return NotFound();

            payroll.IsVerified = true;
            payroll.VerifiedBy = userId;
            payroll.VerifiedDate = DateTime.Now;

            _context.Payrolls.Update(payroll);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Payroll verified." });
        }

        [Authorize(Roles = "Admin,Payroll-Processor")]
        [HttpPost("mark-paid/{payrollId}")]
        public async Task<IActionResult> MarkPayrollAsPaid(int payrollId, [FromQuery] string mode)
        {
            var payroll = await _context.Payrolls.FindAsync(payrollId);
            if (payroll == null || !payroll.IsVerified)
                return BadRequest("Payroll not found or not verified.");

            payroll.IsPaid = true;
            payroll.PaidDate = DateTime.Now;
            payroll.PaymentMode = mode;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Payment processed." });
        }
        [Authorize(Roles = "Admin,Payroll-Processor,Employee,Manager")]
        [HttpGet("{employeeId}/{month}/{year}")]
        public async Task<IActionResult> GetByMonth(int employeeId, int month, int year)
        {
            var result = await _payrollRepo.GetPayrollByEmployeeAndMonthAsync(employeeId, month, year);
            return result != null ? Ok(result) : NotFound(new { Error = "Payroll not found" });
        }

        [Authorize(Roles = "Admin,Payroll-Processor,Employee,Manager")]
        [HttpGet("history/{employeeId}")]
        public async Task<IActionResult> GetHistory(int employeeId)
        {
            var result = await _payrollRepo.GetPayrollHistoryAsync(employeeId);
            return Ok(result);
        }
    }
}
