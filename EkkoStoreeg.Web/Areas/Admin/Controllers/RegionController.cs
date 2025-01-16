using EkkoSoreeg.Entities.Models;
using EkkoSoreeg.Entities.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EkkoSoreeg.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RegionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public RegionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetData()
        {
            var regions = _unitOfWork.Region.GetAll();
            return Json(new { data = regions });
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(Region region)
        {
            _unitOfWork.Region.Add(region);
            _unitOfWork.Complete();

            var message = "Region has been created successfully.";
            return Json(new { success = true, message });
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteRegion(int? Id)
        {
            var item = _unitOfWork.Region.GetFirstorDefault(x => x.Id == Id);
            if (item == null)
                return NotFound();

            _unitOfWork.Region.Remove(item);
            _unitOfWork.Complete();
            return Json(new { success = true, message = "Category Has been Deleted Successfully" });
        }
        [HttpGet]
        public async Task<IActionResult> GetRegionById(int id)
        {
            var region = _unitOfWork.Region.GetFirstorDefault(r => r.Id == id);
            if (region == null) return NotFound();

            var regionDto = new RegionDto
            {
                Id = region.Id,
                Name = region.Name,
                ShippingCost = region.ShippingCost,
            };

            return Ok(regionDto);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Update(Region region)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Region.Update(region);
                _unitOfWork.Complete();
                string successMessage = "Region has been updated successfully";
                return Json(new { success = true, message = successMessage });
            }
            string errorMessage = "An error occurred";
            return Json(new { success = false, message = errorMessage });
        }
        public class RegionDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal ShippingCost { get; set; }
        }


    }
}
