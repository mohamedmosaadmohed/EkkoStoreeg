using EkkoSoreeg.Entities.Models;

namespace EkkoSoreeg.Entities.Repositories
{
    public interface IRegionRepository : IGenericRepository<Region>
    {
        void Update(Region region);
    }
}
