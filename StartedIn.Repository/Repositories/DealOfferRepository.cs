using Microsoft.EntityFrameworkCore;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories
{
    public class DealOfferRepository : GenericRepository<DealOffer, string>, IDealOfferRepository
    {
        private readonly AppDbContext _appDbContext;
        public DealOfferRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public async Task<DealOffer> GetDealOfferById(string dealId)
        {
            var dealOffer = await _appDbContext.DealOffers.Where(x=>x.Id.Equals(dealId))
                .Include(x=>x.Project)
                .ThenInclude(x=>x.UserProjects)
                .ThenInclude(x => x.User)
                .Include(x => x.Investor)
                .Where(x => x.DeletedTime == null)
                .FirstOrDefaultAsync();
            return dealOffer;
        }

    }
}
