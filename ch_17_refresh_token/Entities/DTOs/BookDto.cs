namespace Entities.DTOs;

public record BookDto : BookDtoBase
{
    public int Id { get; init; }
    public Category Category { get; init; }

}
