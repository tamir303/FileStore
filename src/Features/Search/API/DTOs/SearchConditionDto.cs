using FileStoreService.Shared.Enum;

namespace FileStoreService.Features.Search.API.DTOs;

public class SearchConditionDto
{
    public string Field { get; set; } = string.Empty;
    public SearchOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
}