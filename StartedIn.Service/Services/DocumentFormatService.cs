using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.DocumentFormat;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System.Text.RegularExpressions;
using System.Web;
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

        public Table CreateSignatureFieldTable(List<UserContract> userContracts)
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
                ),
                new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct } // Scale table width to fit
            );
            table.AppendChild(tblProperties);

            // Add header row
            TableRow headerRow = new TableRow();
            headerRow.Append(
                CreateTableForSignatureFieldCell("Họ và tên", true),
                CreateTableForSignatureFieldCell("Chữ ký", true)
            );
            table.AppendChild(headerRow);

            // Add rows for each user contract
            foreach (var userContract in userContracts)
            {
                TableRow row = new TableRow();

                // Set row height for larger signature space
                TableRowProperties rowProps = new TableRowProperties(
                    new TableRowHeight { Val = 500 } // Height in twips (1/20 of a point)
                );
                row.Append(rowProps);

                row.Append(
                    CreateTableForSignatureFieldCell(userContract.User.FullName),
                    CreateTableForSignatureFieldCell("") // Signature placeholder with spacing
                );

                table.AppendChild(row);
            }

            return table;
        }

        private TableCell CreateTableForSignatureFieldCell(string text, bool isBold = false)
        {
            TableCell cell = new TableCell();

            // Add cell margin to make it larger
            TableCellProperties cellProperties = new TableCellProperties(
                new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = "2400" }, // Set cell width
                new TableCellMargin(
                    new TopMargin { Width = "200", Type = TableWidthUnitValues.Dxa },
                    new BottomMargin { Width = "200", Type = TableWidthUnitValues.Dxa }
                )
            );

            cell.Append(cellProperties);

            Paragraph para = new Paragraph(new Run(new Text(text)));
            if (isBold)
            {
                para = new Paragraph(new Run(new RunProperties(new Bold()), new Text(text)));
            }
            cell.Append(para);

            return cell;
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
                CreateTableHeaderCell("Số tiền (đồng)", true),
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
                    CreateTableHeaderCell(d.Amount.ToString("N0", new System.Globalization.CultureInfo("vi-VN"))),
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
                { "LEADER", leader.FullName},
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
                    if (usersInContract.Count > 0)
                    {
                        // Insert shareholder info table if usersInContract list is not empty
                        Table shareHolderInfoTable = CreateShareholdersInfoTable(usersInContract);
                        placeholderShareHoldersInfoParagraph.InsertAfterSelf(shareHolderInfoTable);
                    }
                    else
                    {
                        // Replace with whitespace if usersInContract list is empty
                        placeholderShareHoldersInfoParagraph.AppendChild(new Run(new Text(" ")));
                    }
                    placeholderShareHoldersInfoParagraph.Remove();
                }

                var placeholderShareHoldersDistributionParagraph = body.Elements<Paragraph>().FirstOrDefault(p => p.InnerText.Contains("TABLE"));
                if (placeholderShareHoldersDistributionParagraph != null)
                {
                    if (shareEquities.Count > 0)
                    {
                        Table shareHolderTable = await CreateShareDistributionTable(usersInContract, project.Id, contract.Id);
                        placeholderShareHoldersDistributionParagraph.InsertAfterSelf(shareHolderTable);
                    }
                    else
                    {
                        placeholderShareHoldersDistributionParagraph.AppendChild(new Run(new Text(" ")));
                    }
                    placeholderShareHoldersDistributionParagraph.Remove();
                }

                var placeholderShareHoldersSignatureParagraph = body.Elements<Paragraph>().FirstOrDefault(p => p.InnerText.Contains("SIGNATURES"));
                if (placeholderShareHoldersSignatureParagraph != null)
                {
                    if (usersInContract.Count > 0)
                    {
                        // Insert shareholder info table if usersInContract list is not empty
                        Table shareHolderSignatureTable = CreateSignatureFieldTable(usersInContract);
                        placeholderShareHoldersSignatureParagraph.InsertAfterSelf(shareHolderSignatureTable);
                    }
                    else
                    {
                        // Replace with whitespace if usersInContract list is empty
                        placeholderShareHoldersSignatureParagraph.AppendChild(new Run(new Text(" ")));
                    }
                    placeholderShareHoldersSignatureParagraph.Remove();
                }

                var placeholderContractPolicyParagraph = body.Elements<Paragraph>().FirstOrDefault(p => p.InnerText.Contains("CONTRACTPOLICY"));
                if (placeholderContractPolicyParagraph != null)
                {
                    // Contract policy text containing HTML content (WYSIWYG)
                    string contractPolicyText = contract.ContractPolicy;  // This is the HTML WYSIWYG content

                    // Convert the HTML content into plain text (you can apply bold and other formatting later)
                    string plainText = ConvertHtmlToPlainText(contractPolicyText);

                    // Debugging: Print out the plain text before further processing
                    Console.WriteLine("Plain Text: " + plainText);

                    // Split the contract policy text into individual lines by \n
                    var policyLines = plainText.Split(new[] { "\n" }, StringSplitOptions.None);
                    var contractPolicyParagraph = new Paragraph();
                    // Debugging: Print out the policy lines before processing
                    foreach (var line in policyLines)
                    {
                        Console.WriteLine("Policy Line: " + line);
                    }

                    // Create a new paragraph to hold the policy lines
                    foreach (var line in policyLines)
                    {
                        // Split the line into parts for both bold and italic markers
                        var parts = SplitByBoldAndItalicMarkers(line); // Now we process both bold and italic together

                        // Now process each part and apply the correct formatting
                        foreach (var part in parts)
                        {
                            var run = new Run();
                            RunProperties runProperties = new RunProperties
                            {
                                RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                                FontSize = new FontSize { Val = "26" }
                            };

                            // If the part is bold, apply bold formatting
                            if (part.IsBold)
                            {
                                runProperties.Bold = new Bold(); // Apply bold formatting
                            }

                            // If the part is italic, apply italic formatting
                            if (part.IsItalic)
                            {
                                runProperties.Italic = new Italic(); // Apply italic formatting
                            }

                            // Append the run properties to the run
                            run.AppendChild(runProperties);

                            // Append the text part to the run
                            run.AppendChild(new Text(part.Text));

                            // Add the run to the paragraph
                            contractPolicyParagraph.AppendChild(run);
                        }

                        // Add a line break after processing each line
                        contractPolicyParagraph.AppendChild(new Break());
                    }
                    // Insert the paragraph after the placeholder paragraph
                    body.InsertAfter(contractPolicyParagraph, placeholderContractPolicyParagraph);
                    // Remove the placeholder paragraph after inserting the content
                    placeholderContractPolicyParagraph.Remove();
                }


                doc.MainDocumentPart.Document.Save();
            }

            modifiedMemoryStream.Position = 0;

            return modifiedMemoryStream;
        }

        public async Task<MemoryStream> ReplacePlaceHolderForInvestmentDocumentAsync(
            Contract contract, User investor, User leader, Project project,
            ShareEquity shareEquity, List<Disbursement> disbursements)
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
                { "SDTCDA", leader.PhoneNumber },
                { "CHUDUAN", leader.FullName },
                { "CMNDCDA", leader.IdCardNumber },
                { "MAIL", leader.Email },
                { "DCCDU", leader.Address },
                { "PHANTRAMCOPHAN", shareEquity.Percentage.ToString() },
                { "GIAMUA", shareEquity.SharePrice.ToString("N0", new System.Globalization.CultureInfo("vi-VN")) }
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
                var placeholderParagraph = body.Elements<Paragraph>().FirstOrDefault(p => p.InnerText.Contains("CACMOCGIAINGAN"));
                if (placeholderParagraph != null)
                {
                    if (disbursements.Count > 0)
                    {
                        // Insert disbursement table if there are items in the list
                        Table disbursementTable = CreateDisbursementTable(disbursements);
                        placeholderParagraph.InsertAfterSelf(disbursementTable);
                    }
                    else
                    {
                        // Replace with a whitespace paragraph if the list is empty
                        placeholderParagraph.AppendChild(new Run(new Text(" ")));  // Single whitespace
                    }

                    placeholderParagraph.Remove();
                }
                var placeholderContractPolicyParagraph = body.Elements<Paragraph>()
     .FirstOrDefault(p => p.InnerText.Contains("DIEUKHOANDUAN"));

                if (placeholderContractPolicyParagraph != null)
                {
                    // Contract policy text containing HTML content (WYSIWYG)
                    string contractPolicyText = contract.ContractPolicy;  // This is the HTML WYSIWYG content

                    // Convert the HTML content into plain text (you can apply bold and other formatting later)
                    string plainText = ConvertHtmlToPlainText(contractPolicyText);

                    // Debugging: Print out the plain text before further processing
                    Console.WriteLine("Plain Text: " + plainText);

                    // Split the contract policy text into individual lines by \n
                    var policyLines = plainText.Split(new[] { "\n" }, StringSplitOptions.None);
                    var contractPolicyParagraph = new Paragraph();
                    // Debugging: Print out the policy lines before processing
                    foreach (var line in policyLines)
                    {
                        Console.WriteLine("Policy Line: " + line);
                    }

                    // Create a new paragraph to hold the policy lines
                    foreach (var line in policyLines)
                    {
                        // Split the line into parts for both bold and italic markers
                        var parts = SplitByBoldAndItalicMarkers(line); // Now we process both bold and italic together

                        // Now process each part and apply the correct formatting
                        foreach (var part in parts)
                        {
                            var run = new Run();
                            RunProperties runProperties = new RunProperties
                            {
                                RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                                FontSize = new FontSize { Val = "26" }
                            };

                            // If the part is bold, apply bold formatting
                            if (part.IsBold)
                            {
                                runProperties.Bold = new Bold(); // Apply bold formatting
                            }

                            // If the part is italic, apply italic formatting
                            if (part.IsItalic)
                            {
                                runProperties.Italic = new Italic(); // Apply italic formatting
                            }

                            // Append the run properties to the run
                            run.AppendChild(runProperties);

                            // Append the text part to the run
                            run.AppendChild(new Text(part.Text));

                            // Add the run to the paragraph
                            contractPolicyParagraph.AppendChild(run);
                        }

                        // Add a line break after processing each line
                        contractPolicyParagraph.AppendChild(new Break());
                    }
                    // Insert the paragraph after the placeholder paragraph
                    body.InsertAfter(contractPolicyParagraph, placeholderContractPolicyParagraph);
                    // Remove the placeholder paragraph after inserting the content
                    placeholderContractPolicyParagraph.Remove();
                }

                // Lưu thay đổi vào MemoryStream
                doc.MainDocumentPart.Document.Save();
            }

            // Đặt lại vị trí của MemoryStream để sẵn sàng cho upload
            modifiedMemoryStream.Position = 0;

            return modifiedMemoryStream;
        }

        private List<TextPart> SplitByBoldAndItalicMarkers(string line)
        {
            var parts = new List<TextPart>();
            bool isBold = false;
            bool isItalic = false;
            string currentText = "";

            for (int i = 0; i < line.Length; i++)
            {
                // Check for double asterisks '**' for bold
                if (i + 1 < line.Length && line[i] == '*' && line[i + 1] == '*')
                {
                    // Add the current text before the marker
                    if (!string.IsNullOrEmpty(currentText))
                    {
                        parts.Add(new TextPart { Text = currentText, IsBold = isBold, IsItalic = isItalic });
                        currentText = "";
                    }

                    // Toggle bold state
                    isBold = !isBold;
                    i++; // Skip over the second '*' in '**'
                }
                // Check for single asterisk '*' for italic
                else if (line[i] == '*' && (i + 1 >= line.Length || line[i + 1] != '*'))
                {
                    // Add the current text before the marker
                    if (!string.IsNullOrEmpty(currentText))
                    {
                        parts.Add(new TextPart { Text = currentText, IsBold = isBold, IsItalic = isItalic });
                        currentText = "";
                    }

                    // Toggle italic state
                    isItalic = !isItalic;
                }
                else
                {
                    // Add normal character to current text
                    currentText += line[i];
                }
            }

            // Add the last collected text
            if (!string.IsNullOrEmpty(currentText))
            {
                parts.Add(new TextPart { Text = currentText, IsBold = isBold, IsItalic = isItalic });
            }

            return parts;
        }

        public string ConvertHtmlToPlainText(string htmlContent)
        {
            // Tải HTML vào một HtmlDocument
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent); // Tải HTML vào HtmlDocument


            // Replace <strong> tags with ** or * (representing bold in plain text)
            var strongNodes = htmlDoc.DocumentNode.SelectNodes("//strong");
            if (strongNodes != null)
            {
                foreach (var strongNode in strongNodes)
                {
                    strongNode.InnerHtml = "**" + strongNode.InnerHtml + "**";
                }
            }

            var emNodes = htmlDoc.DocumentNode.SelectNodes("//em");
            if (emNodes != null)
            {
                foreach (var emNode in emNodes)
                {
                    emNode.InnerHtml = "*" + emNode.InnerHtml + "*";
                }
            }

            var ulNodes = htmlDoc.DocumentNode.SelectNodes("//ul");
            if (ulNodes != null)
            {
                foreach (var ulNode in ulNodes)
                {
                    ulNode.InnerHtml = "\n" + string.Join("\n", ulNode.SelectNodes("li").Select(li => "-" + li.InnerText));
                }
            }

            var olNodes = htmlDoc.DocumentNode.SelectNodes("//ol");
            if (olNodes != null)
            {
                foreach (var olNode in olNodes)
                {
                    olNode.InnerHtml = "\n" + string.Join("\n", olNode.SelectNodes("li").Select((li, index) => $"{index + 1}. {li.InnerText}"));
                }
            }

            // Lấy văn bản thuần từ HTML
            string plainText = htmlDoc.DocumentNode.InnerText;

            // Giải mã các ký tự HTML đặc biệt như &lt;, &gt;, &ecirc; thành ký tự thật
            plainText = HttpUtility.HtmlDecode(plainText);

            // Giữ lại các ký tự xuống dòng (\n)
            plainText = plainText.Replace("\n", "\n").Trim();

            return plainText;
        }

    }
}
