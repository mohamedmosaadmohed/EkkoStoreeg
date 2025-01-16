using EkkoSoreeg.DataAccess.Data;
using EkkoSoreeg.DataAccess.Implementation;
using EkkoSoreeg.Entities.Models;
using EkkoSoreeg.Entities.Repositories;

namespace EEkkoSoreeg.DataAccess.Implementation
{
    public class RegionRepository : GenericRepository<Region>, IRegionRepository
    {
        private readonly ApplicationDbContext _context;
        public RegionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public void Update(Region region)
        {
            var regionInDb = _context.TbRegion.FirstOrDefault(x => x.Id == region.Id);
            if (regionInDb != null)
            {
                regionInDb.Name = region.Name;
                regionInDb.ShippingCost = region.ShippingCost;
            }
        }
    }
}
