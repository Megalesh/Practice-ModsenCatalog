namespace ModsenCatalog.BusinessLogic.DTOs;

public record ProductSearchParameters(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    double? MinRating = null,
    string SortBy = "date",
    bool IsDescending = false
);