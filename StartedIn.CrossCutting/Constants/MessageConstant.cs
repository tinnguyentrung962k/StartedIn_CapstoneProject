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
        public const string CannotUpdateTaskInfoWhenStarted = "Không thể cập nhật thông tin công việc khi công việc đã bắt đầu.";
        public const string NotFoundParentTaskError = "Không tìm thấy công việc mẹ.";
        public const string AssignParentTaskToSubTaskError = "Không thể gán công việc mẹ cho công việc con.";
        public const string AssignParentTaskToSelfError = "Không thể gán công việc mẹ cho chính nó.";
        public const string MilestoneFromParentAndFromChildrenError = "Không thể gán cột mốc từ công việc mẹ và từ công việc con.";
        public const string NotFoundAssigneeError = "Không tìm thấy người được giao việc.";
        public const string CannotUpdateManHourWhenNotInProgress = "Không thể cập nhật giờ làm khi công việc chưa bắt đầu.";
        public const string AssignChildTaskToMilestoneError = "Không thể gán công việc con cho cột mốc. Trước hết phải gỡ gán tác vụ mẹ";
        public const string AssigneeRoleError = "Người được giao việc không hợp lệ.";
        public const string CannotDeleteTaskWhenStarted = "Không thể xoá công việc khi công việc đã bắt đầu.";
        public const string CannotEditContractError = "Không thể sửa hợp đồng này.";
        public const string JoinGroupWithLeaderRoleError = "Bạn không thể tham gia với vai trò nhóm trưởng.";
        public const string ContractNotBelongToProjectError = "Hợp đồng này không thuộc dự án được chọn.";
        public const string MilestoneNotBelongToProjectError = "Cột mốc này không thuộc vào dự án.";
        public const string CannotUpdateManHourOfParentTask = "Không thể cập nhật giờ làm của công việc mẹ.";
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
        public const string AssetBelongToTransaction = "Tài sản này nằm trong mã giao dịch: ";
        public const string UserInOtherProjectError = "Người dùng đã tham gia vào dự án khác: ";
        public const string RemainingAmountOfAssetNotGreaterThanInitial = "Số lượng tài sản ban đầu không thể lớn hơn số còn lại";
        public const string AssetQuantityCannotSmallerThanZero = "Số lượng tài sản ban đầu không được bé hơn hoặc bằng 0";
        public const string ValidContractsStillExisted = "Hợp đồng được gửi/kích hoạt vẫn còn tồn tại trong dự án";
        public const string DisbursementIssueExisted = "Bạn còn các đợt giải ngân cần được giải quyết";
        public const string InvalidRoleForInvitation = "Vai trò mời vào nhóm không hợp lệ";
        public const string NotValidImage = "Tệp tin đính kèm không phải dạng hình ảnh.";
        public const string NotFoundRecruitmentPost = "Không tìm thấy bài đăng tuyển dụng";
        public const string YouHaveSentInvitationForUser = "Bạn đã gửi lời mời cho người dùng: ";
        public const string MeetingNotFound = "Cuộc họp không tìm thấy";
        public const string GreaterThan2MentorError = "Dự án của bạn chỉ được phép có 2 cố vấn/hướng dẫn";
        public const string UnsoldAssetsError = "Dự án của bạn vẫn còn tài sản chưa được thanh lý.";
        public const string SellGreaterThanRemainError = "Số lượng thanh lý không thể lớn hơn số lượng tài sản còn lại cảu dự án";
        public const string NotFoundLeavingRequest = "Không tìm thấy yêu cầu rời nhóm.";
        public const string PendingLeavingRequestExisted = "Bạn còn yêu cầu cũ chưa được duyệt.";
        public const string UserBelongToActiveContracts = "Người dùng này còn hợp đồng đang có hiệu lực";
        public const string LeaderCannotLeaveGroup = "Bạn không thể rời nhóm với vị trí trưởng nhóm.";
        public const string NotFoundRecruitmentImg = "Không tìm thấy ảnh tuyển dụng";
        public const string RecruitmentPostExist = "Bài đăng tuyển dụng đã tồn tại";
        public const string YouHaveAppliedForRecruitment = "Bạn đã ứng tuyển cho bài đăng tuyển dụng này";
        public const string NotFoundRecruitmentApplication = "Không tìm thấy ứng tuyển cho bài đăng tuyển dụng";
        public const string ApplicantAlreadyInProject = "Bạn đã tham gia dự án";
        public const string InvalidEquityChosenDate = "Không thể chọn ngày trong tương lai.";
        public const string YouCannotRequestToTerminateThisContract = "Bạn không thể yêu cầu huỷ hợp đồng này.";
        public const string NotFoundTerminateRequest = "Không tìm thấy yêu cầu chấm dứt hợp đồng.";
        public const string YouCannotAcceptOrRejectTermination = "Bạn không thể chấp nhận/từ chối huỷ hợp đồng này.";
        public const string YouAreNotBelongToThisRequest = "Ban không phải người nhận/gửi yêu cầu này.";
        public const string NotFoundMeetingNote = "Không tìm thấy biên bản cuộc họp.";
        public const string ExistingProcessingLiquidationError = "Hợp đồng này còn biên bản thanh lý chưa được xác nhận";
        public const string InvalidInformationOfUser = "Thông tin người dùng chưa hợp lệ";
        public const string ThisContractHasBeenCancelled = "Hợp đồng này đã bị huỷ ký";
        public const string CannotUploadMeetingNote = "Không thể tải lên biên bản cuộc họp khi chưa họp xong.";
        public const string NotFoundAppointment = "Không tìm thấy cuộc họp";
        public const string CompleteAppointment = "Đã hoàn thành cuộc họp";
        public const string CancelAppointment = "Đã huỷ cuộc họp";
        public const string InvalidLink = "Hãy nhập một đường dẫn Google Meet hợp lệ.";
        public const string MeetingIsNotFinished = "Cuộc họp chưa hoàn thành";
        public const string ThisContractIsNotInLiquidatedState = "Hợp đồng này không ở trong trạng thái chờ thanh lý";
        public const string OldRemainingQuantityMustBeGreaterThanNewRemainQuantity = "Số lượng tồn kho sau khi cập nhật phải nhỏ hơn hoặc bằng số lượng ban đầu";
        public const string NoTransferRequestWasFound = "Không có yêu cầu chuyển nhóm trưởng nào được gửi.";
        public const string ExistingTransferLeaderRequestWasFound = "Bạn có yêu cầu chuyển nhóm trưởng chưa được xử lý.";
        public const string InternalContractExisted = "Hợp đồng sáng lập đã có trong dự án";
        public const string CannotTransferLeaderRoleForNotMember = "Bạn không thể quyền nhóm trưởng cho cố vấn hoặc nhà đầu tư";
        public const string NotFoundUserTask = "Không tìm thấy tác vụ liên quan tới người dùng";
        public const string NotFoundProjectApprovalRequest = "Không tìm thấy yêu cầu duyệt dự án";
        public const string NotFoundTransferOrTerminatedRequest = "Không tìm thấy yêu cầu huỷ hợp đồng hay chuyển nhóm trưởng";
        public const string ThisDealNotBelongToYou = "Yêu cầu thương lượng này không phải của bạn";
        public const string CannotDeleteDeal = "Không thể xoá yêu cầu thương lượng này";
        public const string NoMoreThanOnePendingApproval = "Bạn có yêu cầu dự án đang chờ xử lý, vui lòng đợi quản trị viên duyệt.";
        public const string AcceptedApprovalExisted = "Dự án của bạn đã được duyệt, bạn không thể gửi thêm yêu cầu.";
        public const string AppointmentEndTimeError =
            "Ngày kết thúc cuộc họp phải trùng với ngày họp, và thời gian kết thúc cuộc họp phải trễ hơn so với giờ bắt đầu.";
        public const string NotFoundLeaderTransfer = "Cuộc bổ nhiệm nhóm trưởng mới không tồn tại";
        public const string ExistProjectName = "Tên dự án đã tồn tại.";
        public const string CannotOpenTask = "Bạn không thể mở lại task khi chưa hoàn thành.";
        public const string ShareDistributionMustBeFrom2Member = "Hợp đồng phải có từ 2 bên tham gia trở lên";
        public const string CannotCreateParentTask = "Bạn không thể tạo tác vụ lớn khi không phải là nhóm trưởng";
        public const string CannotAssignChildrenTaskAsParent = "Bạn không thể gán một tác vụ con vào một tác vụ con khác";
        public const string CannotCompleteTaskWithoutManHour = "Bạn không thể hoàn thành tác vụ khi chưa cập nhật giờ làm";
        public const string CannotChangeStatusTaskWrongAssignee = "Bạn không phải là người thực hiện tác vụ nên không thể thay đổi trạng thái tác vụ";
        public const string NoMoreThanOneAssignee = "Không thể có nhiều hơn một người thực hiện tác vụ";
    }
}
