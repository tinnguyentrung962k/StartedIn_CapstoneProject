using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.Constants
{
    public class MessageConstant
    {
        public const string DownLoadError = "Lỗi khi tải tập tin.";
        public const string NotFoundContractError = "Không tìm thấy hợp đồng.";
        public const string RolePermissionError = "Bạn không có vai trò làm việc này.";
        public const string NotFoundProjectError = "Không tìm thấy dự án.";
        public const string NotFoundUserError = "Không tìm thấy người dùng.";
        public const string NotFoundDocumentError = "Không tìm thấy tài liệu.";
        public const string NotFoundCharterError = "Không tìm thấy điều lệ dự án.";
        public const string UserNotInProjectError = "Người dùng không thuộc dự án.";
        public const string UserNotBelongContractError = "Người dùng không thuộc hợp đồng.";
        public const string NotFoundMilestoneError = "Không tìm thấy cột mốc.";
        public const string NotFoundTaskError = "Không tìm thấy công việc.";
        public const string NotFoundParentTaskError = "Không tìm thấy công việc mẹ.";
        public const string AssignParentTaskToSubTaskError = "Không thể gán công việc mẹ cho công việc con.";
        public const string AssignParentTaskToSelfError = "Không thể gán công việc mẹ cho chính nó.";
        public const string NotFoundAssigneeError = "Không tìm thấy người được giao việc.";
        public const string CannotEditContractError = "Không thể sửa hợp đồng này.";
        public const string ContractNumberExistedError = "Mã hợp đồng này đã tồn tại trong dự án.";
        public const string JoinGroupWithLeaderRoleError = "Bạn không thể tham gia với vai trò nhóm trưởng.";
        public const string ContractNotBelongToProjectError = "Hợp đồng này không thuộc dự án được chọn.";
        public const string MilestoneNotBelongToProjectError = "Cột mốc này không thuộc vào dự án.";
        public const string InternalServerError = "Lỗi server.";
        public const string InvalidToken = "Không tìm thấy refresh token.";
        public const string NotFoundInvestorError = "Không tìm thấy nhà đầu tư.";
        public const string UpdateFailed = "Cập nhật thất bại.";
        public const string CreateFailed = "Tạo dữ liệu mới thất bại.";
        public const string UserExistedInProject = "Người dùng đã tồn tại trong dự án.";
        public const string NotFoundDealError = "Không tìm thấy yêu cầu thương lượng.";
        public const string DealNotBelongToProjectError = "Yêu cầu thương lượng này không thuộc vào dự án.";
        public const string CreateMoreProjectError = "Bạn không thể tạo thêm nhóm.";
        public const string DisbursementGreaterThanBuyPriceError = "Số tiền giải ngân không thể vượt quá số tiền mua cổ phần";
        public const string DealPercentageGreaterThanRemainingPercentage = "Tỉ lệ cổ phần thương lượng lớn hơn tỉ lệ cổ phần còn lại của Startup.";
        public const string CharterNotBelongToProjectError = "Điều lệ này không thuộc vào dự án.";
        public const string CharterExistedError = "Điều lệ đã tồn tại cho dự án.";
        public const string DealNotAccepted = "Thỏa thuận chưa được chấp nhận.";
        public const string TotalDistributePercentageGreaterThanRemainingPercentage = "Tỉ lệ cổ phần chia cho các thành viên không thể vượt quá tỉ lệ cổ phần còn lại.";
        public const string ValidShareDistributionContractExisted = "Hợp đồng chia cổ phần hợp lệ của dự án đã tồn tại.";
        public const string CannotCancelContractError = "Bạn không thể huỷ hợp đồng này.";
        public const string CannotInviteToSign = "Tài liệu này không thể được mời ký.";
    }
}
