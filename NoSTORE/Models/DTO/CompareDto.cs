namespace NoSTORE.Models.DTO
{
    public class CompareDto
    {
        public Dictionary<string, List<Product>> Compares { get; set; } = new();
    }
}
