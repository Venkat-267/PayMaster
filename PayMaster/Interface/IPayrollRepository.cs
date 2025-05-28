using PayMaster.DTO;

namespace PayMaster.Interface
{
    public interface IPayrollRepository
    {
        Task<PayrollDto> GeneratePayrollAsync(int employeeId, int month, int year, int processedBy);
        Task<PayrollDto> GetPayrollByEmployeeAndMonthAsync(int employeeId, int month, int year);
        Task<List<PayrollDto>> GetPayrollHistoryAsync(int employeeId);
    }
}
