using BarqTMS.API.DTOs;

namespace BarqTMS.API.Services
{
    public interface ISearchService
    {
        Task<SearchResultsDto> SearchAsync(string query, string? type = null);
    }

    public class SearchService : ISearchService
    {
        public Task<SearchResultsDto> SearchAsync(string query, string? type = null) => throw new NotImplementedException();
    }
}
