namespace electromaster.core.models.system.cart
{
    public class UpdateCartDto
    {
        public Guid OrderId { get; set; } = Guid.Empty;
        public string ProductReference { get; set; }
        public string? ProductVariantReference { get; set; } = null;
        public string ProductName { get; set; }
        public int ProductCount { get; set; }
    }
}
