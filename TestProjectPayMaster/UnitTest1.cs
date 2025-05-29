using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayMaster.Controllers;
using PayMaster.DTO;
using PayMaster.Interface;
using PayMaster.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
namespace TestProjectPayMaster
{
    public class Tests
    {
        private Mock<IBenefitRepository> _benefitRepo;
        private Mock<IEmployeeRepository> _employeeRepo;
        private Mock<ILeaveRequestRepository> _leaveRepo;
        private Mock<IPayrollRepository> _payrollRepo;
        private Mock<IPayrollPolicyRepository> _policyRepo;
        private Mock<IAdminRepository> _adminRepo;
        private Mock<ISalaryStructureRepository> _salaryRepo;
        private Mock<ITimeSheetRepository> _timeSheetRepo;
        private Mock<PayMasterDbContext> _dbContext;

        [SetUp]
        public void Setup()
        {
            _benefitRepo = new Mock<IBenefitRepository>();
            _employeeRepo = new Mock<IEmployeeRepository>();
            _leaveRepo = new Mock<ILeaveRequestRepository>();
            _payrollRepo = new Mock<IPayrollRepository>();
            _policyRepo = new Mock<IPayrollPolicyRepository>();
            _adminRepo = new Mock<IAdminRepository>();
            _salaryRepo = new Mock<ISalaryStructureRepository>();
            _timeSheetRepo = new Mock<ITimeSheetRepository>();
            _dbContext = new Mock<PayMasterDbContext>();
        }

        [Test]
        public async Task AddBenefit_ReturnsOk()
        {
            var controller = new BenefitController(_benefitRepo.Object);
            var dto = new BenefitDto { EmployeeId = 1, Amount = 500, BenefitType = "Medical" };
            _benefitRepo.Setup(x => x.AddBenefitAsync(dto)).ReturnsAsync(1);

            var result = await controller.Add(dto);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task AddEmployee_ReturnsOk()
        {
            var controller = new EmployeeController(_employeeRepo.Object, _adminRepo.Object);
            var dto = new EmployeeDto { FirstName = "John", LastName = "Doe", UserId = 4 };
            _employeeRepo.Setup(x => x.AddEmployee(dto)).ReturnsAsync(1);
            _adminRepo.Setup(x => x.GenerateAuditLogAsync(It.IsAny<AuditLogDto>())).ReturnsAsync(1);
            // Mock User with Claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "1"), // Simulate logged-in user with ID 1
        new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.Add(dto);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task SubmitLeaveRequest_ReturnsOk()
        {
            var controller = new LeaveRequestController(_leaveRepo.Object, _adminRepo.Object);
            var dto = new LeaveRequestDto { EmployeeId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2) };
            _leaveRepo.Setup(x => x.SubmitLeaveRequest(dto)).ReturnsAsync(1);

            var result = await controller.Submit(dto);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GeneratePayroll_ReturnsOk()
        {
            var controller = new PayrollController(_payrollRepo.Object, _adminRepo.Object);
            _payrollRepo.Setup(x => x.GeneratePayrollAsync(1, 5, 2025, 10)).ReturnsAsync(new PayrollDto { EmployeeId = 1 });

            var result = await controller.Generate(1, 5, 2025, 10);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task SetPayrollPolicy_ReturnsOk()
        {
            var controller = new PayrollPolicyController(_policyRepo.Object);
            var policy = new PayrollPolicy { DefaultPFPercent = 12, OvertimeRatePerHour = 100, EffectiveFrom = DateTime.Now };
            _policyRepo.Setup(x => x.SetPolicyAsync(It.IsAny<PayrollPolicy>())).ReturnsAsync(1);

            var result = await controller.SetPolicy(policy);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetTaxStatements_ReturnsOk()
        {
            var controller = new ReportsController(_adminRepo.Object);
            _adminRepo.Setup(x => x.GetTaxStatementsAsync(2025)).ReturnsAsync(new List<TaxStatementDto>());

            var result = await controller.GetTaxStatements(2025);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task AssignSalaryStructure_ReturnsOk()
        {
            var controller = new SalaryStructureController(_salaryRepo.Object);
            var dto = new SalaryStructureDto { EmployeeId = 1, BasicPay = 30000 };
            _salaryRepo.Setup(x => x.AssignSalaryStructure(dto)).ReturnsAsync(1);

            var result = await controller.Assign(dto);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task SubmitTimeSheet_ReturnsOk()
        {
            var controller = new TimeSheetController(_timeSheetRepo.Object, _adminRepo.Object);
            var dto = new TimeSheetDto { EmployeeId = 1, WorkDate = DateTime.Today, HoursWorked = 8 };
            _timeSheetRepo.Setup(x => x.SubmitTimeSheetAsync(dto)).ReturnsAsync(1);

            var result = await controller.Submit(dto);

            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}