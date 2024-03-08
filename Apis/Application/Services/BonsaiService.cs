using Application.Commons;
using Application.Interfaces;
using Application.Utils;
using Application.Validations.Bonsai;
using Application.ViewModels.BonsaiViewModel;
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
            Pagination<Bonsai> bonsais;
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.BonsaiImages.Where(y => !y.IsDeleted),
                                 x => x.Category,
                                    };
            if (isAdmin)
            {
                bonsais = await _unitOfWork.BonsaiRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted, includes: includes);
            }
            else
            {
                bonsais = await _unitOfWork.BonsaiRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted && !x.isDisable, includes: includes);
            }
            return bonsais;
        }
        public async Task<Pagination<Bonsai>> GetAll(bool isAdmin = false)
        {
            Pagination<Bonsai> bonsais;
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.BonsaiImages.Where(y => !y.IsDeleted),
                                 x => x.Category,
                                    };
            if (isAdmin)
            {
                bonsais = await _unitOfWork.BonsaiRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted,
                isDisableTracking: true, includes: includes);
            }
            else
            {
                bonsais = await _unitOfWork.BonsaiRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && !x.isDisable && x.isSold != null && !x.isSold.Value,
                isDisableTracking: true, includes: includes);
            }

            return bonsais;
        }
        public async Task<Pagination<Bonsai>> GetByFilter(int pageIndex, int pageSize, FilterBonsaiModel filterBonsaiModel, bool isAdmin = false)
        {
            var filter = new List<Expression<Func<Bonsai, bool>>>();
            filter.Add(x => !x.IsDeleted);
            if (!isAdmin)
                filter.Add(x => !x.isDisable && x.isSold != null && !x.isSold.Value);

            if (filterBonsaiModel.Keyword != null)
            {
                string keywordLower = filterBonsaiModel.Keyword.ToLower();
                filter.Add(x => x.Name.ToLower().Contains(keywordLower) || x.NameUnsign.ToLower().Contains(keywordLower));
            }
            if (filterBonsaiModel.Category != null)
            {
                filter.Add(x => x.CategoryId == filterBonsaiModel.Category);
            }
            if (filterBonsaiModel.Style != null)
            {
                filter.Add(x => x.StyleId == filterBonsaiModel.Style);
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
                                    };
            var bonsais = await _unitOfWork.BonsaiRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: finalFilter,
                isDisableTracking: true, includes: includes);
            return bonsais;
        }

        public async Task<Bonsai?> GetById(Guid id, bool isAdmin = false)
        {
            Pagination<Bonsai> bonsais;
            List<Expression<Func<Bonsai, object>>> includes = new List<Expression<Func<Bonsai, object>>>{
                                 x => x.BonsaiImages.Where(y => !y.IsDeleted),
                                 x => x.Category,
                                    };
            if (isAdmin)
            {
                bonsais = await _unitOfWork.BonsaiRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.Id == id,
                isDisableTracking: true, includes: includes);
            }
            else
            {
                bonsais = await _unitOfWork.BonsaiRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && !x.isDisable && x.Id == id && x.isSold != null && !x.isSold.Value,
                isDisableTracking: true, includes: includes);
            }
            return bonsais.Items[0];
        }

        public async Task AddAsync(BonsaiModel bonsaiModel, bool isAdmin)
        {

            if (bonsaiModel == null)
                throw new ArgumentNullException(nameof(bonsaiModel), "Vui lòng nhập thêm thông tin sản phẩm!");

            var validationRules = new BonsaiModelValidator();
            var resultBonsaiInfo = await validationRules.ValidateAsync(bonsaiModel);
            if (!resultBonsaiInfo.IsValid)
            {
                var errors = resultBonsaiInfo.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            var bonsai = _mapper.Map<Bonsai>(bonsaiModel);
            if (isAdmin)
            {
                bonsai.isSold = false;
            }
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
                            throw new Exception("Lỗi khi đăng ảnh lên firebase!");

                        BonsaiImage bonsaiImage = new BonsaiImage()
                        {
                            BonsaiId = bonsai.Id,
                            ImageUrl = url
                        };

                        await _unitOfWork.BonsaiImageRepository.AddAsync(bonsaiImage);
                    }
                    await _unitOfWork.CommitTransactionAsync();
                }
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }
        public async Task Update(Guid id, BonsaiModel bonsaiModel)
        {
            if (bonsaiModel == null)
                throw new ArgumentNullException(nameof(bonsaiModel), "Vui lòng nhập thêm thông tin sản phẩm!");
            var validationRules = new BonsaiModelValidator();
            var resultOrderInfo = await validationRules.ValidateAsync(bonsaiModel);
            if (!resultOrderInfo.IsValid)
            {
                var errors = resultOrderInfo.Errors.Select(x => x.ErrorMessage);
                throw new ValidationException("Xác thực không thành công cho mẫu sản phẩm.", (Exception?)errors);
            }
            var bonsai = _mapper.Map<Bonsai>(bonsaiModel);
            bonsai.Id = id;
            var result = await _unitOfWork.BonsaiRepository.GetByIdAsync(bonsai.Id);
            if (result == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            try
            {
                _unitOfWork.BeginTransaction();
                _unitOfWork.BonsaiRepository.Update(bonsai);
                if (bonsaiModel.Image != null)
                {
                    var pictures = await _unitOfWork.BonsaiImageRepository.GetAsync(isTakeAll: true, expression: x => x.BonsaiId == id && x.IsDeleted == false, isDisableTracking: true);
                    foreach (BonsaiImage image in pictures.Items)
                    {
                        image.IsDeleted = true;
                    }
                    _unitOfWork.BonsaiImageRepository.UpdateRange(pictures.Items);
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task Delete(Guid id)
        {
            var result = await _unitOfWork.BonsaiRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            try
            {
                _unitOfWork.BonsaiRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa sản phẩm. Vui lòng thử lại!");
            }
        }

    }
}
