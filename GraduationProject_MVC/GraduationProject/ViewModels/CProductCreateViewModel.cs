using GraduationProject.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GraduationProject.ViewModels
{
    public class CProductCreateViewModel
    {
  
        public CProductCreateDTO Product { get; set; } = new();

        public IEnumerable<SelectListItem> CategoryOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> PStatusOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> ColorOptions { get; set; } = new List<SelectListItem>();
    }
}
