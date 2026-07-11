using HelpDeskHQ.Core.Common.Exceptions;
using HelpDeskHQ.Core.DTOs.Admin;
using HelpDeskHQ.Core.Entities;
using HelpDeskHQ.Core.Interfaces;
using HelpDeskHQ.Infrastructure.Common;
using HelpDeskHQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HelpDeskHQDbContext _context;

        public CategoryService(HelpDeskHQDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryResponseDto>> GetAllAsync()
        {
            var categories = await _context.TicketCategories
                .Include(c => c.Team)
                .ToListAsync();

            return categories.Select(MapToDto).ToList();
        }

        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto request)
        {
            var team = await EntityValidationHelper.GetOrThrowAsync(
                _context.Teams,
                t => t.Id == request.TeamId,
                "Team not found.");

            await EntityValidationHelper.ThrowIfExistsAsync(
                _context.TicketCategories,
                c => c.Name == request.Name,
                "A category with this name already exists.");

            var category = new TicketCategory
            {
                Name = request.Name,
                TeamId = request.TeamId
            };

            _context.TicketCategories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                TeamId = team.Id,
                TeamName = team.Name
            };
        }

        public async Task<CategoryResponseDto> UpdateAsync(int categoryId, CreateCategoryDto request)
        {
            var category = await EntityValidationHelper.GetOrThrowAsync(
                _context.TicketCategories,
                c => c.Id == categoryId,
                "Category not found.");

            var team = await EntityValidationHelper.GetOrThrowAsync(
                _context.Teams,
                t => t.Id == request.TeamId,
                "Team not found.");

            category.Name = request.Name;
            category.TeamId = request.TeamId;
            await _context.SaveChangesAsync();

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                TeamId = team.Id,
                TeamName = team.Name
            };
        }

        public async Task DeleteAsync(int categoryId)
        {
            var category = await EntityValidationHelper.GetOrThrowAsync(
                _context.TicketCategories,
                c => c.Id == categoryId,
                "Category not found.");

            var hasTickets = await _context.Tickets.AnyAsync(t => t.TicketCategoryId == categoryId);
            if (hasTickets)
            {
                throw new ValidationException("Cannot delete a category that has tickets assigned to it.");
            }

            _context.TicketCategories.Remove(category);
            await _context.SaveChangesAsync();
        }

        private static CategoryResponseDto MapToDto(TicketCategory category)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                TeamId = category.TeamId,
                TeamName = category.Team.Name
            };
        }
    }
}