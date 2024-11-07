namespace StartedIn.CrossCutting.DTOs.BaseDTO
{
    public class SearchResponseDTO<T>
    {
        public List<T> ResponseList { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecord { get; set; }
        public int TotalPage { get; set; }
    }
}
