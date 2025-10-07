namespace GraduationProject.ViewModels
{
    public class CEmplKeywordViewModel
    {
        private string? _keyword;
        public string? Keyword
        {
            get => _keyword;
            set => _keyword = value?.Trim();
        }
    }
}
