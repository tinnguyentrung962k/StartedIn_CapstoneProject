using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IDocumentFormatService
    {
        Task<MemoryStream> ReplacePlaceHolderForInvestmentDocumentAsync(Contract contract, User investor, User leader, Project project, ShareEquity shareEquity, List<Disbursement> disbursements, decimal? buyPrice);
        Table CreateDisbursementTable(List<Disbursement> disbursements);
        TableCell CreateTableHeaderCell(string text, bool isHeader = false);
        Table CreateShareholdersInfoTable(List<UserContract> shareholders);
        Task<Table> CreateShareDistributionTable(List<UserContract> usersInContract, string projectId, string contractId);
        Task<MemoryStream> ReplacePlaceHolderForStartUpShareDistributionDocumentAsync(Contract contract, User leader, Project project, List<ShareEquity> shareEquities, List<UserContract> usersInContract);
        TableCell CreateBoldCell(string text, bool isBold = false);

    }
}
