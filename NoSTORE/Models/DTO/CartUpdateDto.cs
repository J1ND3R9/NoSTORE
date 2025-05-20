using Microsoft.AspNetCore.Mvc;
using NoSTORE.Models;

namespace NoSTORE.Models.DTO
{
    public class CartUpdateDto 
    {
        public string ActionType { get; set; }
        public CartDto Cart { get; set; }
    }
}
