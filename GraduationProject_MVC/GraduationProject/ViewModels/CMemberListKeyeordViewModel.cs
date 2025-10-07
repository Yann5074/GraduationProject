namespace GraduationProject.ViewModels
{
    public class CMemberListKeyeordViewModel
    {
        private string? _keyword;
        public string? Keyword
        {
            get => _keyword;
            set => _keyword = value?.Trim();
        }

        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
