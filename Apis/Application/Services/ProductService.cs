using Application.Commons;
using Application.Interfaces;
using Application.Utils;
using Application.Validations.Product;
using Application.ViewModels.ProductViewModels;
using Application.ViewModels.ServiceModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using System.Linq.Expressions;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FirebaseService _fireBaseService;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, FirebaseService fireBaseService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fireBaseService = fireBaseService;
        }

        public async Task<Pagination<Product>> GetPagination(int pageIndex, int pageSize, bool isAdmin = false)
        {
            Pagination<Product> products;
            List<Expression<Func<Product, object>>> includes = new List<Expression<Func<Product, object>>>{
                                 x => x.ProductImages
                                    };
            if (isAdmin)
            {
                products = await _unitOfWork.ProductRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted, includes: includes);
            }
            else
            {
                products = await _unitOfWork.ProductRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted && !x.isDisable && x.Quantity > 0, includes: includes);
            }
            return products;
        }
        public async Task<Pagination<Product>> GetProducts(bool isAdmin = false)
        {
            Pagination<Product> products;
            List<Expression<Func<Product, object>>> includes = new List<Expression<Func<Product, object>>>{
                                 x => x.ProductImages
                                    };
            if (isAdmin)
            {
                products = await _unitOfWork.ProductRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted,
                isDisableTracking: true, includes: includes);
            }
            else
            {
                products = await _unitOfWork.ProductRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && !x.isDisable && x.Quantity > 0, 
                isDisableTracking: true, includes: includes);
            }

            return products;
        }
        public async Task<Pagination<Product>> GetProductsByFilter(int pageIndex, int pageSize, FilterProductModel filterProductModel, bool isAdmin = false)
        {
            var filter = new List<Expression<Func<Product, bool>>>();
            filter.Add(x => !x.IsDeleted);
            if (!isAdmin)
                filter.Add(x => !x.isDisable && x.Quantity > 0);
            if (filterProductModel.subCategory != null)
            {
                foreach (var subCategoryId in filterProductModel.subCategory)
                {

                    filter.Add(x => filterProductModel.subCategory.Contains(x.SubCategoryId));
                }
            }
            if (filterProductModel.tag != null)
            {
                var productTags = await _unitOfWork.ProductTagRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && filterProductModel.tag.Contains(x.TagId),
                isDisableTracking: true);
                List<Guid> productIdList = productTags.Items.Select(productTags => productTags.ProductId).ToList();
                foreach (var tagId in filterProductModel.tag)
                {
                    filter.Add(x => productIdList.Contains(x.Id));
                }
            }
            if (filterProductModel.keyword != null)
            {
                string keywordLower = filterProductModel.keyword.ToLower();
                filter.Add(x => x.Name.ToLower().Contains(keywordLower) || x.NameUnsign.ToLower().Contains(keywordLower));
            }
            if (filterProductModel.minPrice != null)
            {
                filter.Add(x => x.UnitPrice >= filterProductModel.minPrice);
            }
            if (filterProductModel.maxPrice != null)
            {
                filter.Add(x => x.UnitPrice <= filterProductModel.maxPrice);
            }
            var finalFilter = filter.Aggregate((current, next) => current.AndAlso(next));
            List<Expression<Func<Product, object>>> includes = new List<Expression<Func<Product, object>>>{
                                 x => x.ProductImages
                                    };
            var products = await _unitOfWork.ProductRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: finalFilter,
                isDisableTracking: true, includes: includes);
            return products;
        }

        public async Task<Product?> GetProductById(Guid id, bool isAdmin = false)
        {
            Pagination<Product> products;
            List<Expression<Func<Product, object>>> includes = new List<Expression<Func<Product, object>>>{
                                 x => x.ProductImages
                                    };
            if (isAdmin)
            {
                products = await _unitOfWork.ProductRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.Id == id,
                isDisableTracking: true, includes: includes);
            }
            else
            {
                products = await _unitOfWork.ProductRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && !x.isDisable && x.Id == id,
                isDisableTracking: true, includes: includes);
            }
            return products.Items[0];
        }

        public async Task AddAsync(ProductModel productModel)
        {
            bool operationSuccessful = false;

            if (productModel == null)
                throw new ArgumentNullException(nameof(productModel), "Vui lòng nhập thêm thông tin sản phẩm!");

            var validationRules = new ProductModelValidator();
            var resultProductInfo = await validationRules.ValidateAsync(productModel);
            if (!resultProductInfo.IsValid)
            {
                var errors = resultProductInfo.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            var product = _mapper.Map<Product>(productModel);
            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork.ProductRepository.AddAsync(product);
                if (productModel.Image != null)
                {
                    foreach (var singleImage in productModel.Image.Select((image, index) => (image, index)))
                    {
                        string newImageName = product.Id + "_i" + singleImage.index;
                        string folderName = $"product/{product.Id}/Image";
                        string imageExtension = Path.GetExtension(singleImage.image.FileName);
                        //Kiểm tra xem có phải là file ảnh không.
                        string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        const long maxFileSize = 20 * 1024 * 1024;
                        if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || singleImage.image.Length > maxFileSize)
                        {
                            throw new Exception("Có chứa file không phải ảnh hoặc quá dung lượng tối đa(>20MB)!");
                        }
                        var url = await _fireBaseService.UploadFileToFirebaseStorage(singleImage.image, newImageName, folderName);
                        if (url == null)
                            throw new Exception("Lỗi khi đăng ảnh lên firebase!");

                        ProductImage productImage = new ProductImage()
                        {
                            ProductId = product.Id,
                            ImageUrl = url
                        };

                        await _unitOfWork.ProductImageRepository.AddAsync(productImage);
                    }
                }

                if (productModel.TagId != null && productModel.TagId.Count > 0)
                {
                    foreach (Guid id in productModel.TagId)
                    {
                        if (await _unitOfWork.TagRepository.GetByIdAsync(id) == null)
                        {
                            throw new Exception();
                        }
                        await _unitOfWork.ProductTagRepository.AddAsync(new ProductTag()
                        {
                            ProductId = product.Id,
                            TagId = id
                        });
                    }

                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                if (operationSuccessful)
                {
                    foreach (var singleImage in productModel.Image.Select((image, index) => (image, index)))
                    {
                        string newImageName = product.Id + "_i" + singleImage.index;
                        string folderName = $"product/{product.Id}/Image";
                        await _fireBaseService.DeleteFileInFirebaseStorage(newImageName, folderName);
                    }
                }
                throw;
            }
        }
        public async Task UpdateProduct(Guid id, ProductModel productModel)
        {
            if (productModel == null)
                throw new ArgumentNullException(nameof(productModel), "Vui lòng nhập thêm thông tin sản phẩm!");
            var validationRules = new ProductModelValidator();
            var resultOrderInfo = await validationRules.ValidateAsync(productModel);
            if (!resultOrderInfo.IsValid)
            {
                var errors = resultOrderInfo.Errors.Select(x => x.ErrorMessage);
                throw new ValidationException("Xác thực không thành công cho mẫu sản phẩm.", (Exception?)errors);
            }
            var product = _mapper.Map<Product>(productModel);
            product.Id = id;
            var result = await _unitOfWork.ProductRepository.GetByIdAsync(product.Id);
            if (result == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork.ProductRepository.AddAsync(product);
                if (productModel.Image != null)
                {
                    var pictures = await _unitOfWork.ProductImageRepository.GetAsync(expression: x => x.ProductId == id && !x.IsDeleted);
                    foreach(ProductImage image in pictures.Items)
                    {
                        image.IsDeleted = true;
                    }
                    _unitOfWork.ProductImageRepository.UpdateRange(pictures.Items);
                    foreach (var singleImage in productModel.Image.Select((image, index) => (image, index)))
                    {
                        string newImageName = product.Id + "_i" + singleImage.index;
                        string folderName = $"product/{product.Id}/Image";
                        string imageExtension = Path.GetExtension(singleImage.image.FileName);
                        string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        const long maxFileSize = 20 * 1024 * 1024;
                        if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || singleImage.image.Length > maxFileSize)
                        {
                            throw new Exception("Có chứa file không phải ảnh hoặc quá dung lượng tối đa(>20MB)!");
                        }
                        var url = await _fireBaseService.UploadFileToFirebaseStorage(singleImage.image, newImageName, folderName);
                        if (url == null)
                            throw new Exception("Lỗi khi đăng ảnh lên firebase!");

                        ProductImage productImage = new ProductImage()
                        {
                            ProductId = product.Id,
                            ImageUrl = url
                        };

                        await _unitOfWork.ProductImageRepository.AddAsync(productImage);
                    }
                }

                if (productModel.TagId != null && productModel.TagId.Count > 0)
                {
                    var tags = await _unitOfWork.ProductTagRepository.GetAsync(expression: x => x.ProductId == id && !x.IsDeleted);
                    foreach (ProductTag productTag in tags.Items)
                    {
                        productTag.IsDeleted = true;
                    }
                    _unitOfWork.ProductTagRepository.UpdateRange(tags.Items);
                    foreach (Guid guid in productModel.TagId)
                    {
                        if (await _unitOfWork.TagRepository.GetByIdAsync(id) == null)
                        {
                            throw new Exception();
                        }
                        await _unitOfWork.ProductTagRepository.AddAsync(new ProductTag()
                        {
                            ProductId = product.Id,
                            TagId = id
                        });
                    }

                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }
        public async Task DeleteProduct(Guid id)
        {
            var result = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            try
            {
                _unitOfWork.ProductRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa sản phẩm. Vui lòng thử lại!");
            }
        }
        public async Task UpdateProductAvailability(Guid id)
        {
            var result = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            try
            {
                result.isDisable = !result.isDisable;
                _unitOfWork.ProductRepository.Update(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }

        public Task<List<String?>> GetTreeShapeList()
            => _unitOfWork.ProductRepository.GetTreeShapeList();
    }
}
