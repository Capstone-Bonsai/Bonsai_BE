using Application.Commons;
using Application.Interfaces;
using Application.Validations.Bonsai;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.CustomerBonsaiViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CustomerBonsaiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBonsaiService _bonsaiService;

        public CustomerBonsaiService(IUnitOfWork unitOfWork, IMapper mapper, IBonsaiService bonsaiService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _bonsaiService = bonsaiService;
        }
        public async Task AddBonsaiForCustomer(CustomerBonsaiModel customerBonsaiModel, Guid customerId)
        {
            if (customerBonsaiModel == null)
            {
                throw new Exception("Add sai data bonsai/vườn");
            }
            var bonsai = await _unitOfWork.BonsaiRepository.GetByIdAsync(customerBonsaiModel.BonsaiId);
            if (bonsai == null)
            {
                throw new Exception("Không tìm thấy thông tin bonsai");
            }
            var listBoughtBonsai = await _bonsaiService.GetBoughtBonsai(customerId);
            if (!listBoughtBonsai.Items.Contains(bonsai))
            {
                throw new Exception("Cây này không tồn tại trong danh sách đã mua");
            }
            var garden = await _unitOfWork.CustomerGardenRepository.GetByIdAsync(customerBonsaiModel.CustomerGardenId);
            if (garden == null)
            {
                throw new Exception("Không tìm thấy thông tin bonsai");
            }
            var customerBonsai = _mapper.Map<CustomerBonsai>(customerBonsaiModel);
            await _unitOfWork.CustomerBonsaiRepository.AddAsync(customerBonsai);
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
            bonsai.isDisable = false;
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
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }
    }
}
