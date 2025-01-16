using EkkoSoreeg.Entities.Models;
using EkkoSoreeg.Entities.Repositories;
using Microsoft.AspNetCore.Mvc;


namespace EkkoSoreeg.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index(int? id)
        {
            TempData["RegionId"] = id;
            return View();
        }
        public async Task<IActionResult> GetData(int regionId)
        {
            var cities = _unitOfWork.City.GetAll(o => o.RegionId == regionId);
            return Json(new { data = cities });
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(City city)
        {
            _unitOfWork.City.Add(city);
            _unitOfWork.Complete();

            return Json(new { success = true,
                message = "City has been created successfully."
            });
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteCity(int? Id)
        {
            var item = _unitOfWork.City.GetFirstorDefault(x => x.Id == Id);
            if (item == null)
                return NotFound();

            _unitOfWork.City.Remove(item);
            _unitOfWork.Complete();
            return Json(new { success = true, message ="City Has been Deleted Successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> GetCityById(int id)
        {
            var city = _unitOfWork.City.GetFirstorDefault(r => r.Id == id);
            if (city == null) return NotFound();

            var cityDto = new CityDto
            {
                Id = city.Id,
                Name = city.Name,
            };
            return Ok(cityDto);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Update(City city)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.City.Update(city);
                _unitOfWork.Complete();
                return Json(new { success = true, 
                    message ="City has been updated successfully" });
            }
            return Json(new { success = false, 
                message ="An error occurred" });
        }
        public class CityDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int RegionId { get; set; }
        }
    }
}
