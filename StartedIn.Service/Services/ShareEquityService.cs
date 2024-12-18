using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ShareEquity;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services
{
    public class ShareEquityService : IShareEquityService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IShareEquityRepository _shareEquityRepository;
        private readonly UserManager<User> _userManager;
        private readonly IContractRepository _contractRepository;
        private readonly IUserService _userService;

        public ShareEquityService(
            IContractRepository contractRepository, 
            IProjectRepository projectRepository, 
            IShareEquityRepository shareEquityRepository,
            UserManager<User> userManager,
            IUserService userService
        )
        {
            _contractRepository = contractRepository;
            _projectRepository = projectRepository;
            _userManager = userManager;
            _shareEquityRepository = shareEquityRepository;
            _userService = userService;
        }
        public async Task<List<ShareEquitySummaryDTO>> GetShareEquityOfAllMembersInAProject(string userId, string projectId, EquityShareFilterDTO equityShareFilterDTO)
        {
            // Kiểm tra nếu người dùng có trong dự án
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            // Bước 1: Lấy tất cả hợp đồng trong dự án
            var contracts = await _contractRepository.GetContractByProjectId(projectId);

            // Bước 2: Lọc các hợp đồng còn hiệu lực (hoặc hết hạn nhưng có phân bổ cổ phần trước ngày tính)
            var allContracts = contracts.Where(c =>
                (c.ContractStatus == CrossCutting.Enum.ContractStatusEnum.COMPLETED) ||  // Hợp đồng còn hiệu lực
                (c.ContractStatus == CrossCutting.Enum.ContractStatusEnum.EXPIRED && c.ExpiredDate > equityShareFilterDTO.ToDate) ||  // Hợp đồng hết hạn nhưng vẫn còn hiệu lực trong ngày tính
                (c.ContractStatus == CrossCutting.Enum.ContractStatusEnum.WAITINGFORLIQUIDATION)
            ).ToList();

            // Bước 3: Tính tổng cổ phần cho từng thành viên
            var shareEquitySummary = new List<ShareEquitySummaryDTO>();

            // Lấy ngày tính cổ phần (nếu không có, sử dụng ngày hiện tại)
            var targetDate = equityShareFilterDTO.ToDate ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);

            foreach (var contract in allContracts)
            {
                foreach (var shareEquity in contract.ShareEquities)
                {
                    // Kiểm tra nếu cổ phần đã được phân bổ vào hoặc trước ngày tính cổ phần
                    if (shareEquity.DateAssigned <= targetDate)
                    {
                        // Kiểm tra xem người dùng này đã có cổ phần trong danh sách chưa
                        var existingEntry = shareEquitySummary.FirstOrDefault(s => s.UserId == shareEquity.UserId);

                        if (existingEntry == null)
                        {
                            // Nếu chưa có, tạo mới và thêm vào danh sách
                            shareEquitySummary.Add(new ShareEquitySummaryDTO
                            {
                                UserId = shareEquity.UserId,
                                UserFullName = shareEquity.User.FullName,  // Lấy tên đầy đủ từ bảng User
                                TotalPercentage = shareEquity.Percentage ?? 0,
                                LatestShareDate = shareEquity.DateAssigned.Value,
                                StakeHolderType = shareEquity.StakeHolderType
                            });
                        }
                        else
                        {
                            // Nếu đã có, cộng dồn cổ phần
                            existingEntry.TotalPercentage += shareEquity.Percentage ?? 0;
                            if (shareEquity.DateAssigned > existingEntry.LatestShareDate)
                            {
                                existingEntry.LatestShareDate = shareEquity.DateAssigned.Value;
                            }
                        }
                    }
                }
            }

            // Bước 4: Trả về kết quả
            return shareEquitySummary.OrderByDescending(x=>x.TotalPercentage).ToList();
        }
        public async Task<decimal?> GetShareEquityOfAUserInAProject(string userId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var contracts = await _contractRepository.GetContractByProjectId(projectId);
            var contractsOfUser = contracts.Where(c => (c.ContractStatus == CrossCutting.Enum.ContractStatusEnum.COMPLETED 
            || c.ContractStatus == CrossCutting.Enum.ContractStatusEnum.WAITINGFORLIQUIDATION) 
            && c.ShareEquities.Any(se => se.UserId.Equals(userId))).ToList();
            decimal totalUserEquity = 0;
            foreach (var contract in contractsOfUser)
            {
                var equityOfUserInContract = contract.ShareEquities.FirstOrDefault(x => x.UserId.Equals(userId)).Percentage;
                totalUserEquity += (decimal)equityOfUserInContract;
            }
            return totalUserEquity;
        }
    }
}
