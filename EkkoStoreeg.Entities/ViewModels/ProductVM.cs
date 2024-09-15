﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using EkkoSoreeg.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkkoSoreeg.Entities.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> CatagoryList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> ColorList { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> SizeList { get; set; }
        [ValidateNever]
        public List<ProductImage> ImageList { get; set; }
        public List<int> SelectedColors { get; set; }
        public List<int> SelectedSizes { get; set; }
    }
}
