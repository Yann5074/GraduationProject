using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Extensions
{
    public static class PagingExtensions
    {
        public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> source,
        int page,
        int pageSize,
        CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var total = await source.CountAsync(ct);
            var items = await source.Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync(ct);

            return new PagedList<T>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
}
