namespace FileStoreService.Features.Search.API.DTOs;

public class SearchSuggestionsDto
{
    public List<string> QuerySuggestions { get; set; } = new();
    public List<string> FileNameSuggestions { get; set; } = new();
    public List<string> CategorySuggestions { get; set; } = new();
    public List<string> TagSuggestions { get; set; } = new();
}