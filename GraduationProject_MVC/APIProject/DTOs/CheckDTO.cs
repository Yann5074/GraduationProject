using ApiProject.Models;

namespace ApiProject.DTOs
{
    public class CheckDTO
    {
        public bool Ok { get; set; }

        public int Code { get; set; }

        public object Message { get; set; }

        public TMember Member { get; set; }
    }
}
