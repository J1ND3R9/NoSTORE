﻿using Microsoft.AspNetCore.Mvc;
using NoSTORE.Models;
using NoSTORE.Services;
using static System.Collections.Specialized.BitVector32;
using System.Security.Claims;
using MongoDB.Bson.Serialization.IdGenerators;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Tls;
using NoSTORE.Models.DTO;
using NoSTORE.Models.ViewModels;

namespace NoSTORE.Controllers
{
    public class CartController : Controller
    {
        private readonly UserService _userService;
        private readonly ProductService _productService;

        public CartController(UserService userSerive, ProductService productService)
        {
            _userService = userSerive;
            _productService = productService;
        }

        [Route("~/cart")]
        public async Task<IActionResult> Index()
        {
            string userId = "";
            if (User.Identity.IsAuthenticated)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                if (Request.Cookies.TryGetValue("GuestId", out var guestId))
                {
                    userId = guestId;
                }
                else
                {
                    // Гость без куки — крайне редкая ситуация
                    userId = null;
                }
            }
            var user = await _userService.GetUserById(userId);
            
            var basket = user == null ? null : user.Basket;
            if (basket == null || !basket.Any())
                return View(new List<CartViewModel>());

            var productIds = basket.Select(b => b.ProductId).ToList();
            var products = await _productService.GetByIdsAsync(productIds);

            var cartItems = basket.Select(b => new CartViewModel
            {
                Product = new ProductDto(products.FirstOrDefault(p => p.Id == b.ProductId)),
                Quantity = b.Quantity,
                IsSelected = b.IsSelected ?? true
            })
                .Where(item => item.Product != null)
                .ToList();

            int cartCost = cartItems.Where(i => i.IsSelected).Sum(i => i.TotalPrice);
            int selectedCount = cartItems.Count(i => i.IsSelected);

            ViewBag.CartCost = cartCost;
            ViewBag.SelectedCount = selectedCount;

            cartItems.Reverse();

            return View(cartItems);
        }

        [HttpGet]
        public async Task<IActionResult> GetCartPartial()
        {
            string userId = "";
            if (User.Identity.IsAuthenticated)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                if (Request.Cookies.TryGetValue("GuestId", out var guestId))
                {
                    userId = guestId;
                }
                else
                {
                    // Гость без куки — крайне редкая ситуация
                    userId = null;
                }
            }
            var user = await _userService.GetUserById(userId);
            var basket = user.Basket;
            if (basket == null || !basket.Any())
                return View(new List<CartViewModel>());

            var productIds = basket.Select(b => b.ProductId).ToList();
            var products = await _productService.GetByIdsAsync(productIds);

            var cartItems = basket.Select(b => new CartViewModel
            {
                Product = new ProductDto(products.FirstOrDefault(p => p.Id == b.ProductId)),
                Quantity = b.Quantity,
                IsSelected = b.IsSelected ?? true
            })
                .Where(item => item.Product != null)
                .ToList();

            int cartCost = cartItems.Where(i => i.IsSelected).Sum(i => i.TotalPrice);
            int selectedCount = cartItems.Count(i => i.IsSelected);

            ViewBag.CartCost = cartCost;
            ViewBag.SelectedCount = selectedCount;

            cartItems.Reverse();

            return PartialView("_CartPartial", cartItems);
        }
    }
}
