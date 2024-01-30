using Application.Interfaces;
using Application.ViewModels.ProductImageViewModels;
using Application.ViewModels.ProductTagViewModels;
using Application.ViewModels.ProductViewModels;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IProductImageService _productImageService;
        private readonly IProductTagService _productTagService;
        private readonly IFirebaseService _firebaseService;
        private readonly IClaimsService _claims;

        public ProductController(IProductService productService,
            IProductImageService productImageService,
            IProductTagService productTagService,
            IFirebaseService firebaseService,
            IClaimsService claimsService)
        {
            _productService = productService;
            _productImageService = productImageService;
            _productTagService = productTagService;
            _firebaseService = firebaseService;
            _claims = claimsService;
        }
        [HttpGet("Pagination")]
        public async Task<IActionResult> GetPagination([FromQuery] int pageIndex, int pageSize)
        {
            try
            {
                var products = await _productService.GetPagination(pageIndex, pageSize, _claims.GetIsAdmin);
                if (products.Items.Count == 0)
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
                var products = await _productService.GetProducts(_claims.GetIsAdmin);
                if (products.Items.Count == 0)
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
        [HttpPost("Filter")]
        public async Task<IActionResult> Post([FromQuery] int pageIndex, int pageSize, [FromBody] FilterProductModel? filterProductModel)
        {
            try
            {
                var products = await _productService.GetProductsByFilter(pageIndex, pageSize, filterProductModel, _claims.GetIsAdmin);
                if (products.Items.Count == 0)
                {
                    return NotFound("Không tìm thấy");
                }
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Post([FromForm] ProductModel productModel)
        {
            try
            {
                var id = await _productService.AddAsync(productModel);
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
                if (productModel.Tag != null)
                {
                    foreach (var tagid in productModel.Tag)
                    {
                        await _productTagService.AddAsync(new ProductTagModel()
                        {
                            ProductId = id,
                            TagId = tagid
                        });
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
            var product = await _productService.GetProductById(id, _claims.GetIsAdmin);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
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
        [HttpPut("ChangeImage/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ChangeImage([FromRoute] Guid id, [FromForm] ProductImageModel productImageModel)
        {
            try
            {
                if (productImageModel.Image != null)
                {
                    await _productImageService.DeleteProductImagesByProductId(id);
                    foreach (var singleImage in productImageModel.Image.Select((image, index) => (image, index)))
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
        [HttpPut("UpdateAvalability/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateProductAvailability([FromRoute] Guid id)
        {
            try
            {
                await _productService.UpdateProductAvailability(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
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
        [HttpGet("TreeShapeList")]
        public async Task<IActionResult> GetTreeShapeList()
        {
            var treeShapes = await _productService.GetTreeShapeList();
            if (treeShapes == null)
            {
                return NotFound();
            }
            return Ok(treeShapes);
        }
    }
}
