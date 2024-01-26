using Microsoft.AspNetCore.Http;

namespace Application.ViewModels.ProductImageViewModels
{
    public class ProductImageModel
    {
        public Guid? ProductId { get; set; }
        public List<IFormFile> Image { get; set; } = default!;
    }
}
