namespace StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO
{
    public class SignInviteFreeFormResponseDTO
    {
        public List<SignInviteData> Data { get; set; }
        public SignInviteMeta Meta { get; set; }
    }

    public class SignInviteData
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public long Created { get; set; } // Consider using DateTimeOffset if you need to work with time zones
        public string Email { get; set; }
    }

    public class SignInviteMeta
    {
        public SignInvitePagination Pagination { get; set; }
    }

    public class SignInvitePagination
    {
        public int Total { get; set; }
        public int Count { get; set; }
        public int PerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<string> Links { get; set; }
    }
}
