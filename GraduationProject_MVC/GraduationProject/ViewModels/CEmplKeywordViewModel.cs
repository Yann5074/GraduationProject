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
        public int Page { get; set; } = 1;     // 第幾頁
        public int Size { get; set; } = 10;    // 每頁筆數
    }
}
