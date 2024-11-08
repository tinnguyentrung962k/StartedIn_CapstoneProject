﻿using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
namespace StartedIn.Service.Services
{
    public class DocumentFormatService : IDocumentFormatService
    {
        private readonly IAzureBlobService _azureBlobService;
        private readonly IProjectRepository _projectRepository;
        public DocumentFormatService(IAzureBlobService azureBlobService, IProjectRepository projectRepository)
        {
            _azureBlobService = azureBlobService;
            _projectRepository = projectRepository;
        }

        public Table CreateDisbursementTable(List<Disbursement> disbursements)
        {
            Table table = new Table();

            // Define table properties
            TableProperties tblProperties = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                ));
            table.AppendChild(tblProperties);

            // Header row
            TableRow headerRow = new TableRow();
            headerRow.Append(
                CreateTableHeaderCell("Tên cột mốc giải ngân", true),
                CreateTableHeaderCell("Số tiền", true),
                CreateTableHeaderCell("Ngày bắt đầu", true),
                CreateTableHeaderCell("Ngày hạn chót", true),
                CreateTableHeaderCell("Điều kiện", true)
            );
            table.AppendChild(headerRow);

            // Data rows
            foreach (var d in disbursements)
            {
                TableRow row = new TableRow();
                row.Append(
                    CreateTableHeaderCell(d.Title),
                    CreateTableHeaderCell(d.Amount.ToString("N3")),
                    CreateTableHeaderCell(d.StartDate.ToString("dd-MM-yyyy")),
                    CreateTableHeaderCell(d.EndDate.ToString("dd-MM-yyyy")),
                    CreateTableHeaderCell(d.Condition)
                );
                table.AppendChild(row);
            }

            return table;
        }
        public TableCell CreateTableHeaderCell(string text, bool isHeader = false)
        {
            TableCell cell = new TableCell();
            var paragraph = new Paragraph(new Run(new Text(text)));
            if (isHeader)
            {
                RunProperties runProperties = new RunProperties(new Bold());
                paragraph.Descendants<Run>().First().PrependChild(runProperties);
            }

            cell.Append(paragraph);
            return cell;
        }

        public TableCell CreateBoldCell(string text, bool isBold = false)
        {
            RunProperties runProperties = new RunProperties(
                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });

            if (isBold)
            {
                runProperties.Bold = new Bold();
            }
            Run run = new Run(runProperties, new Text(text));
            Paragraph paragraph = new Paragraph(run);
            return new TableCell(paragraph);
        }


        public Table CreateShareholdersInfoTable(List<UserContract> shareholders)
        {
            Table table = new Table();

            // Define table properties with no borders
            TableProperties tblProperties = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.None },
                    new BottomBorder { Val = BorderValues.None },
                    new LeftBorder { Val = BorderValues.None },
                    new RightBorder { Val = BorderValues.None },
                    new InsideHorizontalBorder { Val = BorderValues.None },
                    new InsideVerticalBorder { Val = BorderValues.None }));
            table.AppendChild(tblProperties);

            // Add rows for each shareholder with label-value pairs
            foreach (var shareholder in shareholders)
            {
                TableRow nameRow = new TableRow();
                nameRow.Append(
                    CreateBoldCell("Họ và tên:", true),  // Bold title
                    CreateBoldCell(shareholder.User.FullName));
                table.AppendChild(nameRow);

                TableRow permanentAddressRow = new TableRow();
                permanentAddressRow.Append(
                    CreateBoldCell("Địa chỉ:", true),    // Bold title
                    CreateBoldCell(shareholder.User.Address));
                table.AppendChild(permanentAddressRow);

                TableRow idCardRow = new TableRow();
                idCardRow.Append(
                    CreateBoldCell("CMND/CCCD:", true),  // Bold title
                    CreateBoldCell(shareholder.User.IdCardNumber));
                table.AppendChild(idCardRow);

                TableRow phoneRow = new TableRow();
                phoneRow.Append(
                    CreateBoldCell("SĐT:", true),        // Bold title
                    CreateBoldCell(shareholder.User.PhoneNumber));
                table.AppendChild(phoneRow);

                // Add an empty row to separate shareholders
                TableRow emptyRow = new TableRow(new TableCell(new Paragraph(new Run(new Text("")))));
                table.AppendChild(emptyRow);
            }

            return table;
        }

        public async Task<Table> CreateShareDistributionTable(List<UserContract> usersInContract, string projectId, string contractId)
        {
            Table table = new Table();

            // Define table properties with borders
            TableProperties tblProperties = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 6 },
                    new BottomBorder { Val = BorderValues.Single, Size = 6 },
                    new LeftBorder { Val = BorderValues.Single, Size = 6 },
                    new RightBorder { Val = BorderValues.Single, Size = 6 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 6 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Size = 6 }
                ));
            table.AppendChild(tblProperties);

            // Helper function to create a table cell with Times New Roman font
            TableCell CreateTableCell(string text, bool isBold = false)
            {
                RunProperties runProperties = new RunProperties(
                    new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });

                if (isBold)
                {
                    runProperties.Bold = new Bold();
                }

                Run run = new Run(runProperties, new Text(text));
                Paragraph paragraph = new Paragraph(run);
                return new TableCell(paragraph);
            }

            // Header row
            TableRow headerRow = new TableRow();
            headerRow.Append(
                CreateTableCell("Tên thành viên", true),
                CreateTableCell("Chức vụ", true),
                CreateTableCell("Số cổ phần được hưởng thụ", true)
            );
            table.AppendChild(headerRow);

            // Data rows
            foreach (var shareholder in usersInContract)
            {
                var userRole = await _projectRepository.GetUserRoleInProject(shareholder.UserId, projectId);
                var sharePercentage = shareholder.Contract.ShareEquities
                    .FirstOrDefault(x => x.ContractId.Equals(contractId) && x.UserId.Equals(shareholder.UserId))?.Percentage.ToString() + "%" ?? "0%";

                TableRow row = new TableRow();
                row.Append(
                    CreateTableCell(shareholder.User.FullName),
                    CreateTableCell(GetRoleInTeamInString(userRole)),
                    CreateTableCell(sharePercentage)
                );
                table.AppendChild(row);
            }

            return table;
        }
        private string GetRoleInTeamInString(RoleInTeam roleInTeam)
        {
            switch (roleInTeam)
            {
                case RoleInTeam.Investor:
                    return RoleInTeamConstant.Investor;
                case RoleInTeam.Mentor:
                    return RoleInTeamConstant.Mentor;
                case RoleInTeam.Leader:
                    return RoleInTeamConstant.Leader;
                case RoleInTeam.Member:
                    return RoleInTeamConstant.Member;
                default:
                    throw new ArgumentOutOfRangeException(nameof(roleInTeam), roleInTeam, "Invalid role in team.");
            }
        }

        public async Task<MemoryStream> ReplacePlaceHolderForStartUpShareDistributionDocumentAsync(Contract contract, User leader, Project project, List<ShareEquity> shareEquities, List<UserContract> usersInContract)
        {
            string blobName = BlobServiceConstant.StartupAllMemberShareContractTemplate;

            // Bước 1: Tải mẫu hợp đồng từ Azure Blob Storage
            using var originalMemoryStream = await _azureBlobService.DownloadDocumentToMemoryStreamAsync(blobName);

            // Tạo một bản sao của MemoryStream để chỉnh sửa
            var modifiedMemoryStream = new MemoryStream();
            originalMemoryStream.CopyTo(modifiedMemoryStream);
            modifiedMemoryStream.Position = 0;

            // Tạo dictionary cho các placeholders và giá trị thay thế
            var replacements = new Dictionary<string, string>
            {
                { "SOHOPDONG", contract.ContractIdNumber },
                { "CREATEDDATE", DateOnly.FromDateTime(DateTime.Now).ToString("dd-MM-yyyy") },
                { "PROJECT", project.ProjectName },
                { "CONTRACTPOLICY", contract.ContractPolicy },
                { "LEADER", leader.FullName}
            };

            // Bước 2: Mở file Word để thay thế placeholders
            using (WordprocessingDocument doc = WordprocessingDocument.Open(modifiedMemoryStream, true))
            {
                var body = doc.MainDocumentPart.Document.Body;

                // Thay thế các placeholders trong văn bản
                foreach (var paragraph in body.Descendants<Paragraph>())
                {
                    foreach (var run in paragraph.Descendants<Run>())
                    {
                        foreach (var text in run.Descendants<Text>())
                        {
                            foreach (var placeholder in replacements)
                            {
                                if (text.Text.Contains(placeholder.Key))
                                {
                                    text.Text = text.Text.Replace(placeholder.Key, placeholder.Value);
                                }
                            }
                        }
                    }
                }

                var placeholderShareHoldersInfoParagraph = body.Elements<Paragraph>().FirstOrDefault(p => p.InnerText.Contains("HOLDERS"));
                if (placeholderShareHoldersInfoParagraph != null)
                {
                    Table shareHolderInfoTable = CreateShareholdersInfoTable(usersInContract);
                    placeholderShareHoldersInfoParagraph.InsertAfterSelf(shareHolderInfoTable);
                    placeholderShareHoldersInfoParagraph.Remove();
                }

                var placeholderShareHoldersDistributionParagraph = body.Elements<Paragraph>().FirstOrDefault(p => p.InnerText.Contains("HOLDERS"));
                if (placeholderShareHoldersDistributionParagraph != null)
                {
                    Table shareHolderTable = await CreateShareDistributionTable(usersInContract, project.Id, contract.Id);
                    placeholderShareHoldersDistributionParagraph.InsertAfterSelf(shareHolderTable);
                    placeholderShareHoldersDistributionParagraph.Remove();
                }

                doc.MainDocumentPart.Document.Save();
            }

            modifiedMemoryStream.Position = 0;

            return modifiedMemoryStream;
        }

        public async Task<MemoryStream> ReplacePlaceHolderForInvestmentDocumentAsync(
            Contract contract, User investor, User leader, Project project,
            ShareEquity shareEquity, List<Disbursement> disbursements, decimal? buyPrice)
        {
            string blobName = BlobServiceConstant.InvesmentContractTemplate;

            // Bước 1: Tải mẫu hợp đồng từ Azure Blob Storage
            using var originalMemoryStream = await _azureBlobService.DownloadDocumentToMemoryStreamAsync(blobName);

            // Tạo một bản sao của MemoryStream để chỉnh sửa
            var modifiedMemoryStream = new MemoryStream();
            originalMemoryStream.CopyTo(modifiedMemoryStream);
            modifiedMemoryStream.Position = 0;

            // Tạo dictionary cho các placeholders và giá trị thay thế
            var replacements = new Dictionary<string, string>
            {
                { "SOHOPDONG", contract.ContractIdNumber },
                { "CREATEDDATE", DateOnly.FromDateTime(DateTime.Now).ToString("dd-MM-yyyy") },
                { "NHADAUTU", investor.FullName },
                { "EMAIL", investor.Email },
                { "SDTNDT", investor.PhoneNumber },
                { "CMNDNDT", investor.IdCardNumber },
                { "DCNDT", investor.Address },
                { "TENDUAN", project.ProjectName },
                { "MASOTHUE", project.CompanyIdNumber },
                { "SDTCDA", leader.PhoneNumber },
                { "CHUDUAN", leader.FullName },
                { "CMNDCDA", leader.IdCardNumber },
                { "MAIL", leader.Email },
                { "DCCDU", leader.Address },
                { "PHANTRAMCOPHAN", shareEquity.Percentage.ToString() },
                { "GIAMUA", buyPrice?.ToString() },
                { "DIEUKHOANDUAN", contract.ContractPolicy },
            };

            // Bước 2: Mở file Word để thay thế placeholders
            using (WordprocessingDocument doc = WordprocessingDocument.Open(modifiedMemoryStream, true))
            {
                var body = doc.MainDocumentPart.Document.Body;

                // Thay thế các placeholders trong văn bản
                foreach (var paragraph in body.Descendants<Paragraph>())
                {
                    foreach (var run in paragraph.Descendants<Run>())
                    {
                        foreach (var text in run.Descendants<Text>())
                        {
                            foreach (var placeholder in replacements)
                            {
                                if (text.Text.Contains(placeholder.Key))
                                {
                                    text.Text = text.Text.Replace(placeholder.Key, placeholder.Value);
                                }
                            }
                        }
                    }
                }

                // Tìm và thay thế "CACMOCGIAINGAN" với bảng giải ngân
                var placeholderParagraph = body.Elements<Paragraph>().FirstOrDefault(p => p.InnerText.Contains("CACMOCGIAINGAN"));
                if (placeholderParagraph != null)
                {
                    Table disbursementTable = CreateDisbursementTable(disbursements);
                    placeholderParagraph.InsertAfterSelf(disbursementTable);
                    placeholderParagraph.Remove();
                }

                // Lưu thay đổi vào MemoryStream
                doc.MainDocumentPart.Document.Save();
            }

            // Đặt lại vị trí của MemoryStream để sẵn sàng cho upload
            modifiedMemoryStream.Position = 0;

            return modifiedMemoryStream;
        }

    }
}
