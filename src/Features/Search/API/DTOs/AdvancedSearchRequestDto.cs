using FileStoreService.Shared.Enum;

namespace FileStoreService.Features.Search.API.DTOs;

public class AdvancedSearchRequestDto
{
    public Dictionary<string, string>? CustomPropertyFilters { get; set; }
        
    public List<SearchConditionDto>? Conditions { get; set; }
        
    public LogicalOperator ConditionOperator { get; set; } = LogicalOperator.And;
}