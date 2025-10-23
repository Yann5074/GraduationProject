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
            if (page <= 0) page = 1;//最小頁碼1
            if (pageSize <= 0) pageSize = 10;//一頁最多10筆

            var total = await source.CountAsync(ct);//總比數
            var items = await source.Skip((page - 1) * pageSize)//跳過前頁資訊, pagesize一樣10筆
                                    .Take(pageSize)//取本頁資料數
                                    .ToListAsync(ct);//實際送到DB

            return new PagedList<T>//裝進泛型容器
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
}
