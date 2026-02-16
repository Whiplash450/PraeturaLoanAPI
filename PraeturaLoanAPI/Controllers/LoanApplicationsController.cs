using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PraeturaLoanAPI.Data;
using PraeturaLoanAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace PraeturaLoanAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LoanApplicationsController : ControllerBase
{
    private readonly LoanDbContext _context;
    //
    public LoanApplicationsController(LoanDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Route("/loan-applications")]
    public async Task<IActionResult> CreateApplication([FromBody] CreateLoanApplication request)
    {
        try
        {
            var errors = new List<string>();
            //
            if(string.IsNullOrWhiteSpace(request.Name)) errors.Add("Name is required.");
            if(string.IsNullOrWhiteSpace(request.Email)) errors.Add("Email is required.");
            var emailValidator = new EmailAddressAttribute();
            if(!emailValidator.IsValid(request.Email)) errors.Add("Email is not valid.");
            if (request.MonthlyIncome <= 0) errors.Add("Monthly income must be greater than zero.");
            if(request.RequestedAmount <= 0) errors.Add("Requested amount must be greater than zero.");
            if(request.TermMonths <= 0) errors.Add("Term months must be greater than zero.");
            //
            if (errors.Any())
            {
                return BadRequest(errors);
            }

            //Idempotency check
            var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
            if(!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existingApplication = await _context.LoanApplications.FirstOrDefaultAsync(a => a.IdempotencyKey == idempotencyKey);
                if(existingApplication != null)
                {
                    return Ok(new CreatedApplicationResponse() { Id = existingApplication.Id, Status = existingApplication.Status, CreatedAt = existingApplication.CreatedAt });
                }
            }

            // Create new loan application
            var application = new LoanApplication
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                MonthlyIncome = request.MonthlyIncome,
                RequestedAmount = request.RequestedAmount,
                TermMonths = request.TermMonths,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                IdempotencyKey = idempotencyKey
            };
            //
            _context.LoanApplications.Add(application);
            await _context.SaveChangesAsync();
            //
            return Created("/loan-applications/" + application.Id, new CreatedApplicationResponse() { Id = application.Id, Status = application.Status, CreatedAt = application.CreatedAt });
        }
        catch (Exception ex)
        {
            //In production would properly log here to appinsights/db
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpGet]
    [Route("/loan-applications/{id}")]
    public async Task<IActionResult> GetApplication(Guid id)
    {
        try
        {
            var application = await _context.LoanApplications
                .Include(a => a.DecisionLogs)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (application == null)
            {
                return NotFound();
            }
            return Ok(application);
        }
        catch (Exception ex)
        {
            //In production would properly log here to appinsights/db
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}
