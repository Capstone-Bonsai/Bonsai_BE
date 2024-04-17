using Application.Commons;
using Application.Interfaces;
using Application.Validations.Bonsai;
using Application.ViewModels.CustomerBonsaiViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Application.Utils;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class CustomerBonsaiService : ICustomerBonsaiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBonsaiService _bonsaiService;
        private readonly FirebaseService _fireBaseService;
        private readonly IdUtil _idUtil;
  

        public CustomerBonsaiService(IUnitOfWork unitOfWork, IMapper mapper, IBonsaiService bonsaiService, FirebaseService fireBaseService, IdUtil idUtil)

        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _bonsaiService = bonsaiService;
            _fireBaseService = fireBaseService;
            _idUtil = idUtil;

        }
        public async Task AddBonsaiForCustomer(CustomerBonsaiModel customerBonsaiModel, Guid customerId)
        {
            if (customerBonsaiModel == null)
            {
                throw new Exception("Vui lòng điền đầy đủ thông tin");
            }
            var bonsai = await _unitOfWork.BonsaiRepository.GetByIdAsync(customerBonsaiModel.BonsaiId);
            if (bonsai == null)
            {
                throw new Exception("Không tìm thấy thông tin bonsai");
            }
            var listBoughtBonsai = await _bonsaiService.GetBoughtBonsai(customerId);
            if (!listBoughtBonsai.Items.Any(x => x.Id == bonsai.Id))
            {
                throw new Exception("Cây này không tồn tại trong danh sách đã mua");
            }
            var garden = await _unitOfWork.CustomerGardenRepository.GetByIdAsync(customerBonsaiModel.CustomerGardenId);
            if (garden == null)
            {
                throw new Exception("Không tìm thấy thông tin vườn");
            }
            var customerBonsai = _mapper.Map<CustomerBonsai>(customerBonsaiModel);
            await _unitOfWork.CustomerBonsaiRepository.AddAsync(customerBonsai);
            await _unitOfWork.SaveChangeAsync();
        }
        public async Task CreateBonsai(Guid gardenId,BonsaiModelForCustomer bonsaiModelForCustomer)
        {

            if (bonsaiModelForCustomer == null)
                throw new ArgumentNullException(nameof(bonsaiModelForCustomer), "Vui lòng điền đầy đủ thông tin!");

            var validationRules = new BonsaiModelForCustomerValidator();
            var resultBonsaiInfo = await validationRules.ValidateAsync(bonsaiModelForCustomer);
            if (!resultBonsaiInfo.IsValid)
            {
                var errors = resultBonsaiInfo.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            var bonsai = _mapper.Map<Bonsai>(bonsaiModelForCustomer);
            bonsai.Price = 0;
            bonsai.isDisable = false;
            bonsai.Code = await generateCode();
            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork.BonsaiRepository.AddAsync(bonsai);
                if (bonsaiModelForCustomer.Image != null)
                {
                    foreach (var singleImage in bonsaiModelForCustomer.Image.Select((image, index) => (image, index)))
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
                await _unitOfWork.CustomerBonsaiRepository.AddAsync(new CustomerBonsai()
                {
                    BonsaiId = bonsai.Id,
                    CustomerGardenId = gardenId
                });
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }
        private async Task<string> generateCode()
        {
            var lastCodeBonsai = await _unitOfWork.BonsaiRepository.GetAsync(pageIndex: 0, pageSize: 1, expression: x => x.Code.Contains("KHACHHANG"), orderBy: query => query.OrderByDescending(x => x.Code));
            if (lastCodeBonsai.Items.Count > 0)
            {
                var lastCodeNumericPart = Regex.Match(lastCodeBonsai.Items[0].Code, @"\d+").Value;
                if (lastCodeNumericPart == "")
                {
                    return $"KHACHHANG00001";
                }
                var newCodeNumericPart = (int.Parse(lastCodeNumericPart) + 1).ToString().PadLeft(lastCodeNumericPart.Length, '0');
                return $"KHACHHANG{newCodeNumericPart}";

            }
            else
            {
                return $"KHACHHANG00001";
            }
        }
        public async Task<Pagination<CustomerBonsai>> GetBonsaiOfGarden(Guid gardenId)
        {
            List<Expression<Func<CustomerBonsai, object>>> includes = new List<Expression<Func<CustomerBonsai, object>>>{
                                 x => x.Bonsai.BonsaiImages
                                    };
            var bonsais = await _unitOfWork.CustomerBonsaiRepository.GetAsync(isTakeAll: true, expression: x => x.CustomerGardenId == gardenId && !x.IsDeleted, includes: includes);
            return bonsais;
        }
        public async Task<CustomerBonsai> GetCustomerBonsaiById(Guid customerBonsaiId, Guid userId, bool isCustomer)
        {
            List<Expression<Func<CustomerBonsai, object>>> includes = new List<Expression<Func<CustomerBonsai, object>>>{
                                 x => x.Bonsai.BonsaiImages,
                                 x => x.CustomerGarden
                                    };
            var bonsais = await _unitOfWork.CustomerBonsaiRepository.GetAsync(isTakeAll: true, expression: x => x.Id == customerBonsaiId && !x.IsDeleted, includes: includes);
            if (isCustomer == true)
            {
                var customer = await _idUtil.GetCustomerAsync(userId);
                if (bonsais.Items[0].CustomerGarden.CustomerId != customer.Id)
                {
                    throw new Exception("Bạn không có quyền truy cập vào bonsai này");
                }
            }
            return bonsais.Items[0];
        }
    }
}
