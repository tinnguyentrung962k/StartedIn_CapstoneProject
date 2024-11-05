using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.Constants
{
    public class MessageConstant
    {
        public const string DownLoadError = "Lỗi khi tải tập tin";
        public const string NotFoundContractError = "Không tìm thấy hợp đồng";
        public const string RolePermissionError = "Bạn không có vai trò làm việc này";
        public const string NotFoundProjectError = "Không tìm thấy dự án";
        public const string NotFoundUserError = "Không tìm thấy người dùng";
        public const string NotFoundDocumentError = "Không tìm thấy tài liệu";
        public const string NotFoundCharterError = "Không tìm thấy điều lệ dự án";
        public const string UserNotInProjectError = "Người dùng không thuộc dự án";
        public const string UserNotBelongContractError = "Người dùng không thuộc hợp đồng";
        public const string NotFoundMilestoneError = "Không tìm thấy cột mốc";
        public const string NotFoundTaskError = "Không tìm thấy công việc";
        public const string CannotUpdateContractError = "Không thể sửa hợp đồng này";
        public const string ContractNumberExistedError = "Mã hợp đồng này đã tồn tại trong dự án";
        public const string JoinGroupWithLeaderRoleError = "Bạn không thể tham gia với vai trò nhóm trưởng";
        public const string ContractNotBelongToProjectError = "Hợp đồng này không thuộc dự án được chọn";
    }
}
