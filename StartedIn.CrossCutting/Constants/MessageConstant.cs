﻿using System;
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
        public const string MilestoneFromParentAndFromChildrenError = "Không thể gán cột mốc từ công việc mẹ và từ công việc con.";
        public const string NotFoundAssigneeError = "Không tìm thấy người được giao việc.";
        public const string AssignChildTaskToMilestoneError = "Không thể gán công việc con cho cột mốc. Trước hết phải gỡ gán tác vụ mẹ";
        public const string AssigneeRoleError = "Người được giao việc không hợp lệ.";
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
        public const string DeleteFailed = "Xoá dữ liệu thất bại.";
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
        public const string CannotCancelContractError = "Bạn không thể huỷ ký hợp đồng này.";
        public const string CannotInviteToSign = "Tài liệu này không thể được mời ký.";
        public const string NullOrWhiteSpaceProjectName = "Tên dự án không được để trống.";
        public const string NullOrWhiteSpaceDescription = "Mô tả dự án không được để trống.";
        public const string NullOrEmptyLogoFile = "Logo dự án không được để trống.";
        public const string NullOrEmptyStartDate = "Ngày bắt đầu dự án không được để trống.";
        public const string NegativeNumberError = "Vui lòng không nhập số âm.";
        public const string DisbursementListEmptyInContract = "Vui lòng điền các mốc giải ngân cho hợp đồng.";
        public const string ShareDistributionListEmptyInContract = "Vui lòng điền thông tin phân bổ cổ phần cho hợp đồng.";
        public const string DisbursementNotBelongToProject = "Đợt giải ngân này không thuộc vào dự án được chọn";
        public const string DisbursementNotFound = "Không tìm thấy đợt giải ngân.";
        public const string DisbursementFinished = "Đợt giải ngân đã hoàn thành.";
        public const string PaymentGateWayCustomizeError = "Lỗi thiết lập cổng thanh toán";
        public const string DisbursementNotBelongToInvestor = "Đợt giải ngân này không phải của bạn";
        public const string CannotRejectThisDisbursement = "Bạn không thể từ chối đợt giải ngân này";
        public const string EmptyFileError = "Vui lòng tải tập tin lên";
        public const string NotFoundShareEquityError = "Không tìm thấy cổ phần của dự án";
        public const string NotFoundInvestmentCall = "Không tìm thấy đợt gọi vốn của dự án";
        public const string InvalidEquityShare = "Lượng cổ phần không hợp lệ";
        public const string InvestmentCallEquitySoldOut = "Lượng cổ phần có thể bán của đợt gọi vốn này không còn đủ.";
        public const string ClosedInvestmentCall = "Đợt gọi vốn đã đóng.";
        public const string TransactionNotFound = "Không tìm thấy giao dịch.";
        public const string TransactionNotBelongToProject = "Giao dịch không thuộc dự án.";
        public const string AssetNotFound = "Không tìm thấy tài sản.";
        public const string AssetNotBelongToProject = "Tài sản không thuộc vào dự án.";
        public const string StartDateLaterThanEndDate = "Ngày kết thúc không được sớm hơn ngày bắt đầu";
        public const string NotFoundPhaseError = "Không tìm thấy giai đoạn của dự án";
        public const string NotFoundProjectCharterError = "Không tìm thấy điều lệ của dự án";
        public const string ProjectNotVerifiedError = "Dự án của bạn chưa được duyệt";
        public const string TransactionAmountOverProjectBudget = "Ngân sách dự án không cho phép.";
        public const string TransactionAmountNotMatchTotalPurchaseAssets = "Số tiền không khớp với tổng tiền tài sản.";
        public const string UserHasInvestorSystemRole = "Người dùng có vai trò là nhà đầu tư:";
        public const string NotInvitedError = "Bạn chưa được mời gia nhập dự án.";
        public const string ContractIsNotValid = "Hợp đồng này chưa được kích hoạt/ký kết.";
        public const string AccountAlreadyActivate = "Tài khoản này đã được kích hoạt trước đó.";
        public const string NotFoundTaskAttachmentError = "Không tìm thấy tệp đính kèm tác vụ.";
        public const string CannotAcceptDeal = "Bạn không thể chấp nhận thoả thuận này.";
        public const string InvalidNumberOfMembersInProject = "Số lượng thành viên không hợp lệ";
        public const string FullMembersOfTeam = "Dự án đã đạt số lượng thành viên tối đa.";
    }
}
