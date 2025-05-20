using NoSTORE.Models.DTO;
using System.Globalization;
using System.Xml.Linq;

namespace NoSTORE.Models.ViewModels
{
    public class CartViewModel
    {
        public ProductDto Product { get; set; }
        public int Quantity { get; set; }
        public bool IsSelected { get; set; }
        public int TotalPrice => Product.FinalPrice * Quantity;
        public string CorrectPrice(int price)
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            return price.ToString("#,0 ₽", nfi);
        }
        public string GetFolder()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos", "products", Product.Name);
            if (Directory.Exists(path))
                return path;
            return "";
        }

        public bool ImageExist()
        {
            string path = GetFolder();
            if (path != "")
            {
                path += "/" + Product.Image;
                if (File.Exists(path))
                    return true;
            }
            else
                CreateFolder();
            return false;
        }

        public void CreateFolder()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "photos", "products", Product.Name);
            Directory.CreateDirectory(path);
        }
    }
}
