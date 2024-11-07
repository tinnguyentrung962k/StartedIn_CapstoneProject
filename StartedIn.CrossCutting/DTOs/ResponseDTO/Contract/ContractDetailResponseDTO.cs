using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract
{
    public class ContractDetailResponseDTO : ContractResponseDTO
    {
        public List<DisbursementInContractResponseDTO> Disbursements { get; set; }
    }
}
