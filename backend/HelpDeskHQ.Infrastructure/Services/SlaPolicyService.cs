using HelpDeskHQ.Core.Common.Exceptions;
using HelpDeskHQ.Core.DTOs.Admin;
using HelpDeskHQ.Core.Entities;
using HelpDeskHQ.Core.Enums;
using HelpDeskHQ.Core.Interfaces;
using HelpDeskHQ.Infrastructure.Common;
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

            return policies.Select(MapToDto).ToList();
        }

        public async Task<SlaPolicyResponseDto> CreateAsync(CreateSlaPolicyDto request)
        {
            var category = await EntityValidationHelper.GetOrThrowAsync(
                _context.TicketCategories,
                c => c.Id == request.TicketCategoryId,
                "Ticket category not found.");

            var priority = ValidateAndParsePriority(request.Priority);
            ValidateTargetMinutes(request.ResponseTargetMinutes, request.ResolutionTargetMinutes);

            await EntityValidationHelper.ThrowIfExistsAsync(
                _context.SlaPolicies,
                p => p.TicketCategoryId == request.TicketCategoryId && p.Priority == priority,
                "An SLA policy for this category and priority already exists.");

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
            var policy = await EntityValidationHelper.GetOrThrowAsync(
                _context.SlaPolicies.Include(p => p.TicketCategory),
                p => p.Id == policyId,
                "SLA policy not found.");

            var priority = ValidateAndParsePriority(request.Priority);
            ValidateTargetMinutes(request.ResponseTargetMinutes, request.ResolutionTargetMinutes);

            policy.Priority = priority;
            policy.ResponseTargetMinutes = request.ResponseTargetMinutes;
            policy.ResolutionTargetMinutes = request.ResolutionTargetMinutes;

            await _context.SaveChangesAsync();

            return MapToDto(policy);
        }

        public async Task DeleteAsync(int policyId)
        {
            var policy = await EntityValidationHelper.GetOrThrowAsync(
                _context.SlaPolicies,
                p => p.Id == policyId,
                "SLA policy not found.");

            _context.SlaPolicies.Remove(policy);
            await _context.SaveChangesAsync();
        }

        private static TicketPriority ValidateAndParsePriority(int priorityValue)
        {
            if (!Enum.IsDefined(typeof(TicketPriority), priorityValue))
            {
                throw new ValidationException("Invalid priority value.");
            }
            return (TicketPriority)priorityValue;
        }

        private static void ValidateTargetMinutes(int responseTarget, int resolutionTarget)
        {
            if (responseTarget <= 0 || resolutionTarget <= 0)
            {
                throw new ValidationException("Target minutes must be greater than zero.");
            }

            if (responseTarget > resolutionTarget)
            {
                throw new ValidationException("Response target cannot be greater than resolution target.");
            }
        }

        private static SlaPolicyResponseDto MapToDto(SlaPolicy policy)
        {
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
    }
}