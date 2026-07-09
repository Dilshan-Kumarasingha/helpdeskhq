using HelpDeskHQ.Core.DTOs.Admin;
using HelpDeskHQ.Core.Entities;
using HelpDeskHQ.Core.Enums;
using HelpDeskHQ.Core.Interfaces;
using HelpDeskHQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.Infrastructure.Services
{
    public class SlaPolicyService : ISlaPolicyService
    {
        private readonly HelpDeskHQDbContext _context;

        public SlaPolicyService(HelpDeskHQDbContext context)
        {
            _context = context;
        }

        public async Task<List<SlaPolicyResponseDto>> GetAllAsync()
        {
            var policies = await _context.SlaPolicies
                .Include(p => p.TicketCategory)
                .ToListAsync();

            return policies.Select(p => new SlaPolicyResponseDto
            {
                Id = p.Id,
                TicketCategoryId = p.TicketCategoryId,
                CategoryName = p.TicketCategory.Name,
                Priority = p.Priority.ToString(),
                ResponseTargetMinutes = p.ResponseTargetMinutes,
                ResolutionTargetMinutes = p.ResolutionTargetMinutes
            }).ToList();
        }

        public async Task<SlaPolicyResponseDto> CreateAsync(CreateSlaPolicyDto request)
        {
            var category = await _context.TicketCategories.FirstOrDefaultAsync(c => c.Id == request.TicketCategoryId);
            if (category == null)
            {
                throw new InvalidOperationException("Ticket category not found.");
            }

            if (!Enum.IsDefined(typeof(TicketPriority), request.Priority))
            {
                throw new InvalidOperationException("Invalid priority value.");
            }

            var priority = (TicketPriority)request.Priority;

            var duplicateExists = await _context.SlaPolicies
                .AnyAsync(p => p.TicketCategoryId == request.TicketCategoryId && p.Priority == priority);
            if (duplicateExists)
            {
                throw new InvalidOperationException("An SLA policy for this category and priority already exists.");
            }

            if (request.ResponseTargetMinutes <= 0 || request.ResolutionTargetMinutes <= 0)
            {
                throw new InvalidOperationException("Target minutes must be greater than zero.");
            }

            if (request.ResponseTargetMinutes > request.ResolutionTargetMinutes)
            {
                throw new InvalidOperationException("Response target cannot be greater than resolution target.");
            }

            var policy = new SlaPolicy
            {
                TicketCategoryId = request.TicketCategoryId,
                Priority = priority,
                ResponseTargetMinutes = request.ResponseTargetMinutes,
                ResolutionTargetMinutes = request.ResolutionTargetMinutes
            };

            _context.SlaPolicies.Add(policy);
            await _context.SaveChangesAsync();

            return new SlaPolicyResponseDto
            {
                Id = policy.Id,
                TicketCategoryId = policy.TicketCategoryId,
                CategoryName = category.Name,
                Priority = policy.Priority.ToString(),
                ResponseTargetMinutes = policy.ResponseTargetMinutes,
                ResolutionTargetMinutes = policy.ResolutionTargetMinutes
            };
        }

        public async Task<SlaPolicyResponseDto> UpdateAsync(int policyId, CreateSlaPolicyDto request)
        {
            var policy = await _context.SlaPolicies
                .Include(p => p.TicketCategory)
                .FirstOrDefaultAsync(p => p.Id == policyId);

            if (policy == null)
            {
                throw new InvalidOperationException("SLA policy not found.");
            }

            if (!Enum.IsDefined(typeof(TicketPriority), request.Priority))
            {
                throw new InvalidOperationException("Invalid priority value.");
            }

            if (request.ResponseTargetMinutes <= 0 || request.ResolutionTargetMinutes <= 0)
            {
                throw new InvalidOperationException("Target minutes must be greater than zero.");
            }

            if (request.ResponseTargetMinutes > request.ResolutionTargetMinutes)
            {
                throw new InvalidOperationException("Response target cannot be greater than resolution target.");
            }

            policy.Priority = (TicketPriority)request.Priority;
            policy.ResponseTargetMinutes = request.ResponseTargetMinutes;
            policy.ResolutionTargetMinutes = request.ResolutionTargetMinutes;

            await _context.SaveChangesAsync();

            return new SlaPolicyResponseDto
            {
                Id = policy.Id,
                TicketCategoryId = policy.TicketCategoryId,
                CategoryName = policy.TicketCategory.Name,
                Priority = policy.Priority.ToString(),
                ResponseTargetMinutes = policy.ResponseTargetMinutes,
                ResolutionTargetMinutes = policy.ResolutionTargetMinutes
            };
        }

        public async Task DeleteAsync(int policyId)
        {
            var policy = await _context.SlaPolicies.FirstOrDefaultAsync(p => p.Id == policyId);
            if (policy == null)
            {
                throw new InvalidOperationException("SLA policy not found.");
            }

            _context.SlaPolicies.Remove(policy);
            await _context.SaveChangesAsync();
        }
    }
}