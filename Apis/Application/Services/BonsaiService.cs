/*using Application.Commons;
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
    public class BonsaiService : IBonsaiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FirebaseService _fireBaseService;
        private readonly IMapper _mapper;

        public BonsaiService(IUnitOfWork unitOfWork, IMapper mapper, FirebaseService fireBaseService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fireBaseService = fireBaseService;
        }

        public async Task<Pagination<Bonsai>> GetPagination(int pageIndex, int pageSize, bool isAdmin = false)
        {
            Pagination<Bonsai> products;
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.ProductImages
                                    };
            if (isAdmin)
            {
                products = await _unitOfWork.ProductRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted, includes: includes);
            }
            else
            {
                products = await _unitOfWork.ProductRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted && !x.isDisable, includes: includes);
            }
            return products;
        }
        public async Task<Pagination<Bonsai>> GetProducts(bool isAdmin = false)
        {
            Pagination<Bonsai> products;
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.ProductImages.Where(y => !y.IsDeleted)
                                    };
            if (isAdmin)
            {
                products = await _unitOfWork.ProductRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted,
                isDisableTracking: true, includes: includes);
            }
            else
            {
                products = await _unitOfWork.ProductRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && !x.isDisable,
                isDisableTracking: true, includes: includes);
            }

            return products;
        }
        public async Task<Pagination<Bonsai>> GetProductsByFilter(int pageIndex, int pageSize, FilterBonsaiModel filterProductModel, bool isAdmin = false)
        {
            var filter = new List<Expression<Func<Bonsai, bool>>>();
            filter.Add(x => !x.IsDeleted);
            if (!isAdmin)
                filter.Add(x => !x.isDisable);
         
            if (filterProductModel.keyword != null)
            {
                string keywordLower = filterProductModel.keyword.ToLower();
                filter.Add(x => x.Name.ToLower().Contains(keywordLower) || x.NameUnsign.ToLower().Contains(keywordLower));
            }
            if (filterProductModel.minPrice != null)
            {
                filter.Add(x => x.Price >= filterProductModel.minPrice);
            }
            if (filterProductModel.maxPrice != null)
            {
                filter.Add(x => x.Price <= filterProductModel.maxPrice);
            }
            var finalFilter = filter.Aggregate((current, next) => current.AndAlso(next));
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.ProductImages.Where(y => !y.IsDeleted),
                                 x => x.Category
                                    };
            var products = await _unitOfWork.ProductRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: finalFilter,
                isDisableTracking: true, includes: includes);
            return products;
        }

        public async Task<Bonsai?> GetProductById(Guid id, bool isAdmin = false)
        {
            Pagination<Bonsai> products;
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.ProductImages.Where(y => !y.IsDeleted)
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

        public async Task AddAsync(BonsaiModel productModel)
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
            var product = _mapper.Map<Bonsai>(productModel);
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

                        BonsaiImage productImage = new BonsaiImage()
                        {
                            BonsaiId = product.Id,
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
        public async Task UpdateProduct(Guid id, BonsaiModel productModel)
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
            var product = _mapper.Map<Bonsai>(productModel);
            product.Id = id;
            var result = await _unitOfWork.ProductRepository.GetByIdAsync(product.Id);
            if (result == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            try
            {
                _unitOfWork.BeginTransaction();
                _unitOfWork.ProductRepository.Update(product);
                if (productModel.Image != null)
                {
                    var pictures = await _unitOfWork.ProductImageRepository.GetAsync(isTakeAll: true, expression: x => x.ProductId == id && x.IsDeleted == false, isDisableTracking: true);
                    foreach (BonsaiImage image in pictures.Items)
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

                        BonsaiImage productImage = new BonsaiImage()
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
*/