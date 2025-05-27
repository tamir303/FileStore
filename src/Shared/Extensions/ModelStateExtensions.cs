using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FileStoreService.Shared.Extensions;

public static class ModelStateExtensions
{
    /// <summary>
    /// Returns a flat list of all error messages in the model‐state.
    /// </summary>
    public static List<string?> GetErrors(this ModelStateDictionary? modelState)
    {
        if (modelState == null) return [];

        return modelState
            .Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                ? e.Exception?.Message
                : e.ErrorMessage)
            .Where(msg => !string.IsNullOrWhiteSpace(msg))
            .ToList();
    }
}