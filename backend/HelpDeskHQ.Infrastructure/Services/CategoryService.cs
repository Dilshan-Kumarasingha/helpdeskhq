using HelpDeskHQ.Core.DTOs.Admin;
using HelpDeskHQ.Core.Entities;
using HelpDeskHQ.Core.Interfaces;
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

            return categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                TeamId = c.TeamId,
                TeamName = c.Team.Name
            }).ToList();
        }

        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto request)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == request.TeamId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found.");
            }

            var nameExists = await _context.TicketCategories.AnyAsync(c => c.Name == request.Name);
            if (nameExists)
            {
                throw new InvalidOperationException("A category with this name already exists.");
            }

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
            var category = await _context.TicketCategories.FirstOrDefaultAsync(c => c.Id == categoryId);
            if (category == null)
            {
                throw new InvalidOperationException("Category not found.");
            }

            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == request.TeamId);
            if (team == null)
            {
                throw new InvalidOperationException("Team not found.");
            }

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
            var category = await _context.TicketCategories.FirstOrDefaultAsync(c => c.Id == categoryId);
            if (category == null)
            {
                throw new InvalidOperationException("Category not found.");
            }

            var hasTickets = await _context.Tickets.AnyAsync(t => t.TicketCategoryId == categoryId);
            if (hasTickets)
            {
                throw new InvalidOperationException("Cannot delete a category that has tickets assigned to it.");
            }

            _context.TicketCategories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}