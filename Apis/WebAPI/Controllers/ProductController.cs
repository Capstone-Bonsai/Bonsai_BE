using Application.Interfaces;
using Application.ViewModels.ProductViewModels;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IProductImageService _productImageService;
        private readonly IFirebaseService _firebaseService;
        private readonly IClaimsService _claims;

        public ProductController(IProductService productService,
            IProductImageService productImageService,
            IFirebaseService firebaseService,
            IClaimsService claimsService)
        {
            _productService = productService;
            _productImageService = productImageService;
            _firebaseService = firebaseService;
            _claims = claimsService;
        }
        [HttpGet("Pagination")]
        public async Task<IActionResult> GetPagination([FromQuery] int pageIndex, int pageSize)
        {
            try
            {
                var products = await _productService.GetPagination(pageIndex, pageSize);
                if (products == null)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(products);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _productService.GetProducts();
                if (products == null)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(products);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] ProductModel productModel)
        {
            try
            {
                var id = await _productService.AddAsyncGetId(productModel);
                if (productModel.Image != null)
                {
                    foreach (var singleImage in productModel.Image.Select((image, index) => (image, index)))
                    {
                        string newImageName = id + "_i" + singleImage.index;
                        string folderName = $"product/{id}/Image";
                        string imageExtension = Path.GetExtension(singleImage.image.FileName);
                        //Kiểm tra xem có phải là file ảnh không.
                        string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

                        if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1)
                        {
                            throw new Exception("Invalid image file format");
                        }
                        var url = await _firebaseService.UploadFileToFirebaseStorage(singleImage.image, newImageName, folderName);
                        if (url == null)
                            throw new Exception("Lỗi khi đăng ảnh lên firebase!");

                        ProductImage productImage = new ProductImage()
                        {
                            ProductId = id,
                            ImageUrl = url
                        };

                        await _productImageService.AddProductImages(productImage);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] ProductModel productModel)
        {
            try
            {
                await _productService.UpdateProduct(id, productModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                await _productService.DeleteProduct(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
