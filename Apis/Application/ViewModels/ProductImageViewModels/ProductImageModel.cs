using Microsoft.AspNetCore.Http;

namespace Application.ViewModels.ProductImageViewModels
{
    public class ProductImageModel
    {
        public List<IFormFile> Image { get; set; } = default!;
    }
}
