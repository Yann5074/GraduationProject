namespace ApiProject.DTOs
{
    public class ResCategoryDTO
    {
        public int CategoryId { get; init; }
        public string Name { get; init; } = "";
        public int ParentCategoryId { get; init; }
        public bool IsActive { get; init; }     // tCategory.fIsActive
        public int SortOrder { get; init; }     // tCategory.fSortOrder
    }
}
