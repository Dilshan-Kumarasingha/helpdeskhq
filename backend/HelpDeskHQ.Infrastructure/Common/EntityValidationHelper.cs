using HelpDeskHQ.Core.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskHQ.Infrastructure.Common
{
    /// <summary>
    /// Small shared helpers for the common "find or throw" / "check duplicate"
    /// patterns that were previously copy-pasted across TeamService,
    /// CategoryService, and SlaPolicyService.
    /// </summary>
    public static class EntityValidationHelper
    {
        /// <summary>
        /// Finds an entity by a predicate, or throws NotFoundException with
        /// the given message if it doesn't exist.
        /// </summary>
        public static async Task<T> GetOrThrowAsync<T>(
            IQueryable<T> query,
            System.Linq.Expressions.Expression<Func<T, bool>> predicate,
            string notFoundMessage) where T : class
        {
            var entity = await query.FirstOrDefaultAsync(predicate);
            if (entity == null)
            {
                throw new NotFoundException(notFoundMessage);
            }
            return entity;
        }

        /// <summary>
        /// Throws ConflictException if any entity matches the predicate.
        /// Use for uniqueness checks (e.g. name, email already exists).
        /// </summary>
        public static async Task ThrowIfExistsAsync<T>(
            IQueryable<T> query,
            System.Linq.Expressions.Expression<Func<T, bool>> predicate,
            string conflictMessage) where T : class
        {
            var exists = await query.AnyAsync(predicate);
            if (exists)
            {
                throw new ConflictException(conflictMessage);
            }
        }
    }
}