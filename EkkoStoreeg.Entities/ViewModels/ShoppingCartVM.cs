﻿using EkkoSoreeg.Entities.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EkkoSoreeg.Entities.ViewModels
{
    public class ShoppingCartVM
    {
        [ValidateNever]
        public IEnumerable<ShoppingCart> shoppingCarts { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public decimal totalCarts { get; set; }
        public decimal totalCartsWithShipping { get; set; }
	}
}
