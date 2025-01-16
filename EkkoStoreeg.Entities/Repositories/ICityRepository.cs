using EkkoSoreeg.Entities.Models;

namespace EkkoSoreeg.Entities.Repositories
{
    public interface ICityRepository : IGenericRepository<City>
    {
        void Update(City city);
    }
}
