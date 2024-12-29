using Microsoft.AspNetCore.Http;


namespace StartedIn.CrossCutting.DTOs.RequestDTO.Project
{
    public class ProjectCreateDTO
    {
        public string ProjectName { get; set; }

        public string Description { get; set; }
        public IFormFile LogoFile { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}
