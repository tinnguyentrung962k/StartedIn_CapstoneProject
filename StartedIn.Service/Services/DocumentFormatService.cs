using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using StartedIn.CrossCutting.Constants;
using StartedIn.Domain.Entities;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services
{
    public class DocumentFormatService : IDocumentFormatService
    {
        private readonly IAzureBlobService _azureBlobService;
        public DocumentFormatService(IAzureBlobService azureBlobService)
        {
            _azureBlobService = azureBlobService;
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
                CreateTableCell("Tên cột mốc giải ngân", true),
                CreateTableCell("Số tiền", true),
                CreateTableCell("Ngày bắt đầu", true),
                CreateTableCell("Ngày hạn chót", true),
                CreateTableCell("Điều kiện", true)
            );
            table.AppendChild(headerRow);

            // Data rows
            foreach (var d in disbursements)
            {
                TableRow row = new TableRow();
                row.Append(
                    CreateTableCell(d.Title),
                    CreateTableCell(d.Amount.ToString("N3")),
                    CreateTableCell(d.StartDate.ToString("dd-MM-yyyy")),
                    CreateTableCell(d.EndDate.ToString("dd-MM-yyyy")),
                    CreateTableCell(d.Condition)
                );
                table.AppendChild(row);
            }

            return table;
        }

        public TableCell CreateTableCell(string text, bool isHeader = false)
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
