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
    }
}
