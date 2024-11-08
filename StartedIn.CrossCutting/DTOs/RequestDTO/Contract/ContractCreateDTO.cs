using System.ComponentModel.DataAnnotations;
namespace StartedIn.CrossCutting.DTOs.RequestDTO.Contract
{
    public class ContractCreateDTO
    {
        [Required(ErrorMessage = "Vui lòng điền tên hợp đồng")]
        [StringLength(50, ErrorMessage = "Tên hợp đồng không được quá 50 ký tự")]
        public string ContractName { get; set; }

        //[Required(ErrorMessage = "Vui lòng điền các điều khoản khác")]
        public string ContractPolicy { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Vui lòng điền số hợp đồng")]
        [StringLength(50, ErrorMessage = "Số hợp đồng không được quá 50 ký tự")]
        public string ContractIdNumber { get; set; } = string.Empty;
    }
}
