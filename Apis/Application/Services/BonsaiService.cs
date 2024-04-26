using Application.Commons;
using Application.Interfaces;
using Application.Utils;
using Application.Validations.Bonsai;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.DeliveryFeeViewModels;
using AutoMapper;
using Domain.Entities;
using Firebase.Auth;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Export.ToCollection;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class BonsaiService : IBonsaiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FirebaseService _fireBaseService;
        private readonly IMapper _mapper;
        private readonly IdUtil _idUtil;

        public BonsaiService(IUnitOfWork unitOfWork, IMapper mapper, FirebaseService fireBaseService, IdUtil idUtil)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fireBaseService = fireBaseService;
            _idUtil = idUtil;
        }

        public async Task<Pagination<Bonsai>> GetPagination(int pageIndex, int pageSize, bool isAdmin = false)
        {
            Pagination<Bonsai> bonsais;
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.BonsaiImages.Where(y => !y.IsDeleted),
                                 x => x.Category,
                                 x => x.Style,
                                    };
            if (isAdmin)
            {
                bonsais = await _unitOfWork.BonsaiRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted && !x.Code.Contains("KHACHHANG"), includes: includes);
            }
            else
            {
                bonsais = await _unitOfWork.BonsaiRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted && !x.Code.Contains("KHACHHANG") && !x.isDisable && x.isSold != null && !x.isSold.Value, includes: includes);
            }
            return bonsais;
        }
        public async Task<Pagination<Bonsai>> GetAll(bool isAdmin = false)
        {
            Pagination<Bonsai> bonsais;
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.BonsaiImages.Where(y => !y.IsDeleted),
                                 x => x.Category,
                                 x => x.Style,
                                    };
            if (isAdmin)
            {
                bonsais = await _unitOfWork.BonsaiRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && !x.Code.Contains("KHACHHANG"),
                isDisableTracking: true, includes: includes);
            }
            else
            {
                bonsais = await _unitOfWork.BonsaiRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && !x.Code.Contains("KHACHHANG") && !x.isDisable && x.isSold != null && !x.isSold.Value,
                isDisableTracking: true, includes: includes);
            }

            return bonsais;
        }
        public async Task<Pagination<Bonsai>?> GetByFilter(int pageIndex, int pageSize, FilterBonsaiModel filterBonsaiModel, bool isAdmin = false)
        {
            if(filterBonsaiModel.Keyword != null && filterBonsaiModel.Keyword.Length > 50)
            {
                throw new Exception("Từ khóa phải dưới 50 kí tự");
            }
            var filter = new List<Expression<Func<Bonsai, bool>>>();
            filter.Add(x => !x.IsDeleted && !x.Code.Contains("KHACHHANG"));
            if (!isAdmin)
                filter.Add(x => !x.isDisable && x.isSold != null && !x.isSold.Value);

            if (filterBonsaiModel.Keyword != null)
            {
                string keywordLower = filterBonsaiModel.Keyword.ToLower();
                filter.Add(x => x.Name.ToLower().Contains(keywordLower) || x.NameUnsign.ToLower().Contains(keywordLower));
            }
            try
            {
                if (filterBonsaiModel.Category != null && filterBonsaiModel.Category != "")
                {
                    filter.Add(x => x.CategoryId == Guid.Parse(filterBonsaiModel.Category));
                }
                if (filterBonsaiModel.Style != null && filterBonsaiModel.Style != "")
                {
                    filter.Add(x => x.StyleId == Guid.Parse(filterBonsaiModel.Style));
                }
            }
            catch (Exception)
            {
                throw new Exception("Xảy ra lỗi trong quá trình nhập bộ lọc!");
            }

            if (filterBonsaiModel.MinPrice != null)
            {
                filter.Add(x => x.Price >= filterBonsaiModel.MinPrice);
            }
            if (filterBonsaiModel.MaxPrice != null)
            {
                filter.Add(x => x.Price <= filterBonsaiModel.MaxPrice);
            }
            var finalFilter = filter.Aggregate((current, next) => current.AndAlso(next));
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.BonsaiImages.Where(y => !y.IsDeleted),
                                 x => x.Category,
                                 x => x.Style,
                                    };
            var bonsais = await _unitOfWork.BonsaiRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: finalFilter,
                isDisableTracking: true, includes: includes, orderBy: x => x.OrderBy(bonsai => bonsai.isSold).ThenByDescending(bonsai => bonsai.CreationDate));
            return bonsais;
        }

        public async Task<Bonsai?> GetById(Guid id, bool isAdmin = false)
        {
            Pagination<Bonsai> bonsais;
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.BonsaiImages.Where(y => !y.IsDeleted),
                                 x => x.Category,
                                 x => x.Style,
                                    };
            if (isAdmin)
            {
                bonsais = await _unitOfWork.BonsaiRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.Id == id && !x.Code.Contains("KHACHHANG"),
                isDisableTracking: true, includes: includes);
            }
            else
            {
                bonsais = await _unitOfWork.BonsaiRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && !x.isDisable && x.Id == id && x.isSold != null && !x.isSold.Value && !x.Code.Contains("KHACHHANG"),
                isDisableTracking: true, includes: includes);
            }
            return bonsais.Items[0];
        }

        public async Task AddAsync(BonsaiModel bonsaiModel)
        {

            if (bonsaiModel == null)
                throw new ArgumentNullException(nameof(bonsaiModel), "Vui lòng điền đầy đủ thông tin!");

            var validationRules = new BonsaiModelValidator();
            var resultBonsaiInfo = await validationRules.ValidateAsync(bonsaiModel);
            if (!resultBonsaiInfo.IsValid)
            {
                var errors = resultBonsaiInfo.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            if (bonsaiModel.Price == null)
            {
                throw new Exception("Giá không được để trống.");
            }
            if (bonsaiModel.Image == null || bonsaiModel.Image.Count == 0)
                throw new Exception("Vui lòng thêm hình ảnh");
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(bonsaiModel.CategoryId);
            if (category == null)
                throw new Exception("Không tìm thấy danh mục!");
            var style = await _unitOfWork.StyleRepository.GetByIdAsync(bonsaiModel.StyleId);
            if (style == null)
                throw new Exception("Không tìm thấy kiểu dáng!");
            var bonsai = _mapper.Map<Bonsai>(bonsaiModel);
            bonsai.isDisable = false;
            bonsai.Code = await generateCode(bonsaiModel.CategoryId);
            bonsai.DeliverySize = bonsaiModel.DeliverySize;
            bonsai.isSold = false;
            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork.BonsaiRepository.AddAsync(bonsai);
                if (bonsaiModel.Image != null)
                {
                    foreach (var singleImage in bonsaiModel.Image.Select((image, index) => (image, index)))
                    {
                        string newImageName = bonsai.Id + "_i" + singleImage.index;
                        string folderName = $"bonsai/{bonsai.Id}/Image";
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
                            throw new Exception("Lỗi khi đăng ảnh lên Firebase!");

                        BonsaiImage bonsaiImage = new BonsaiImage()
                        {
                            BonsaiId = bonsai.Id,
                            ImageUrl = url
                        };

                        await _unitOfWork.BonsaiImageRepository.AddAsync(bonsaiImage);
                    }
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }
        public async Task Update(Guid id, BonsaiModel bonsaiModel)
        {
            var result = await _unitOfWork.BonsaiRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy bonsai!");
            if ((result.isSold != null && result.isSold.Value == true) || result.Code.Contains("KHACHHANG"))
            {
                throw new Exception("Cây này thuộc về khách hàng, không thể cập nhật!");
            }
            if (bonsaiModel == null)
                throw new ArgumentNullException(nameof(bonsaiModel), "Vui lòng điền đầy đủ thông tin!");
            var validationRules = new BonsaiModelValidator();
            var resultBonsaiInfo = await validationRules.ValidateAsync(bonsaiModel);
            if (!resultBonsaiInfo.IsValid)
            {
                var errors = resultBonsaiInfo.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            if ((bonsaiModel.Image == null || bonsaiModel.Image.Count == 0) && (bonsaiModel.OldImage == null || bonsaiModel.OldImage.Count == 0))
                throw new Exception("Vui lòng thêm hình ảnh");
            var bonsai = _mapper.Map<Bonsai>(bonsaiModel);
            bonsai.Id = id;
            bonsai.DeliverySize = bonsaiModel.DeliverySize;
            bonsai.Code = result.Code;
            bonsai.isDisable = false;
            bonsai.isSold = false;
            try
            {
                _unitOfWork.BeginTransaction();
                _unitOfWork.BonsaiRepository.Update(bonsai);
                if (bonsaiModel.Image != null)
                {
                    var images = await _unitOfWork.BonsaiImageRepository.GetAsync(isTakeAll: true, expression: x => x.BonsaiId == id && !x.IsDeleted, isDisableTracking: true);
                    if (bonsaiModel.OldImage != null)
                    {
                        foreach (BonsaiImage image in images.Items.ToList())
                        {
                            if (bonsaiModel.OldImage.Contains(image.ImageUrl))
                            {
                                //Bỏ những cái có trong danh sách cũ truyền về -> không xóa
                                images.Items.Remove(image);
                            }
                        }
                        
                    }
                    _unitOfWork.BonsaiImageRepository.SoftRemoveRange(images.Items);
                    foreach (var singleImage in bonsaiModel.Image.Select((image, index) => (image, index)))
                    {
                        string newImageName = bonsai.Id + "_i" + singleImage.index;
                        string folderName = $"bonsai/{bonsai.Id}/Image";
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

                        BonsaiImage bonsaiImage = new BonsaiImage()
                        {
                            BonsaiId = bonsai.Id,
                            ImageUrl = url
                        };

                        await _unitOfWork.BonsaiImageRepository.AddAsync(bonsaiImage);
                    }
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task Delete(Guid id)
        {
            var result = await _unitOfWork.BonsaiRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy!");
            if ((result.isSold != null && result.isSold.Value == true )|| result.Code.Contains("KHACHHANG"))
            {
                throw new Exception("Cây này thuộc về khách hàng, không thể xóa");
            }
            var orderDetails = await _unitOfWork.OrderDetailRepository.GetAsync(pageIndex: 0, pageSize: 1, expression: x => x.BonsaiId == id && !x.IsDeleted);
            if (orderDetails.TotalItemsCount > 0)
            {
                throw new Exception("Tồn tại đơn hàng thuộc về cây này, không thể xóa!");
            }
            try
            {
                _unitOfWork.BonsaiRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa bonsai. Vui lòng thử lại!");
            }
        }
        public async Task<Pagination<Bonsai>> GetBoughtBonsai(Guid id)
        {
            var customer = await _idUtil.GetCustomerAsync(id);
            var orderDetails = await _unitOfWork.OrderDetailRepository.GetAsync(isTakeAll: true, expression: x => x.Order.CustomerId == customer.Id && x.Order.OrderStatus == Domain.Enums.OrderStatus.Delivered);
            if (orderDetails.Items.Count == 0)
            {
                throw new Exception("Bạn chưa có đơn hàng hoàn thành nào");
            }
            List<Guid> orderDetailsId = new List<Guid>();
            foreach (OrderDetail orderDetail in orderDetails.Items)
            {
                orderDetailsId.Add(orderDetail.Id);
            }
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.BonsaiImages.Where(y => !y.IsDeleted)
                                    };
            var bonsais = await _unitOfWork.BonsaiRepository.GetAsync(isTakeAll: true, expression: x => x.OrderDetails.Any(y => orderDetailsId.Contains(y.Id)) && x.CustomerBonsai == null && !x.Code.Contains("KHACHHANG"), includes: includes);
            return bonsais;
        }
        private async Task<string> generateCode(Guid categoryId)
        {
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.Category,
                                    };
            var lastCodeBonsai = await _unitOfWork.BonsaiRepository.GetAsync(pageIndex: 0, pageSize: 1, expression: x => x.CategoryId == categoryId && !x.Code.Contains("KHACHHANG"), orderBy: query => query.OrderByDescending(x => x.Code), includes: includes);
            if (lastCodeBonsai.Items.Count > 0)
            {
                var lastCodeNumericPart = Regex.Match(lastCodeBonsai.Items[0].Code, @"\d+").Value;
                if (lastCodeNumericPart == "")
                {
                    var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
                    return $"{StringUtils.RemoveDiacritics(category.Name)}00001";
                }
                var newCodeNumericPart = (int.Parse(lastCodeNumericPart) + 1).ToString().PadLeft(lastCodeNumericPart.Length, '0');
                return $"{StringUtils.RemoveDiacritics((lastCodeBonsai.Items[0].Category.Name))}{newCodeNumericPart}";

            }
            else
            {
                var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
                return $"{StringUtils.RemoveDiacritics(category.Name)}00001";
            }
        }
        public async Task<Pagination<Bonsai>> GetByCategory(int pageIndex, int pageSize, Guid categoryId)
        {
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.BonsaiImages.Where(y => !y.IsDeleted),
                                 x => x.Category,
                                 x => x.Style,
                                    };
            var bonsais = await _unitOfWork.BonsaiRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted && !x.isDisable && x.isSold == false && x.CategoryId == categoryId && !x.isSold.Value && !x.Code.Contains("KHACHHANG"),
                isDisableTracking: true, includes: includes);
            return bonsais;
        }
        public async Task DisableBonsai(Guid id)
        {
            var bonsai = await _unitOfWork.BonsaiRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.Id == id && !x.Code.Contains("KHACHHANG"),
                isDisableTracking: true);
            if (!bonsai.Items[0].isDisable)
                bonsai.Items[0].isDisable = true;
            else
                bonsai.Items[0].isDisable = false;
            _unitOfWork.BonsaiRepository.Update(bonsai.Items[0]);
            await _unitOfWork.SaveChangeAsync();
        }
        public async Task<List<Bonsai>> getCurrentCart(List<Guid> bonsaiId)
        {
            List<Bonsai> bonsais = new List<Bonsai>();
            foreach(Guid id in bonsaiId)
            {
                var bonsai = await _unitOfWork.BonsaiRepository
                    .GetAllQueryable()
                    .Include(x => x.BonsaiImages)
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync();
                if (bonsai == null)
                {
                    throw new Exception("Không tìm thấy bonsai " + id.ToString());
                }
                bonsais.Add(bonsai);
            }
            if(bonsaiId.Count != bonsais.Count)
            {
                throw new Exception("Số lượng cây trong cart khác với số lượng cây");
            }
            return bonsais;
        }
    }
}
