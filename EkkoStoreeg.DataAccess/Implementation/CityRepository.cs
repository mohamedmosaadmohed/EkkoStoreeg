using EkkoSoreeg.DataAccess.Data;
using EkkoSoreeg.Entities.Models;
using EkkoSoreeg.Entities.Repositories;

namespace EkkoSoreeg.DataAccess.Implementation
{
    public class CityRepository : GenericRepository<City>, ICityRepository
    {
        private readonly ApplicationDbContext _context;
        public CityRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public void Update(City city)
        {
            var cityInDb = _context.TbCity.FirstOrDefault(x => x.Id == city.Id);
            if (cityInDb != null)
            {
                cityInDb.Name = city.Name;
            }
        }
    }
}
