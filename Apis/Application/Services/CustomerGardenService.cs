using Application.Commons;
using Application.Interfaces;
using Application.Utils;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.CustomerGardenViewModels;
using AutoMapper;
using Domain.Entities;
using Firebase.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CustomerGardenService : ICustomerGardenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly FirebaseService _fireBaseService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IdUtil _idUtil;

        public CustomerGardenService(IUnitOfWork unitOfWork, IMapper mapper, FirebaseService fireBaseService, UserManager<ApplicationUser> userManager, IdUtil idUtil)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fireBaseService = fireBaseService;
            _userManager = userManager;
            _idUtil = idUtil;
        }

        public async Task AddCustomerGarden(CustomerGardenModel customerGardenModel, Guid id)
        {
            if (customerGardenModel == null)
            {
                throw new Exception("Vui lòng điền đầy đủ thông tin");
            }
            var customerGarden = _mapper.Map<CustomerGarden>(customerGardenModel);
            var customer = await _idUtil.GetCustomerAsync(id);
            customerGarden.CustomerId = customer.Id;
            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork.CustomerGardenRepository.AddAsync(customerGarden);
                if (customerGardenModel.Image != null)
                {
                    foreach (var singleImage in customerGardenModel.Image.Select((image, index) => (image, index)))
                    {
                        string newImageName = customerGarden.Id + "_i" + singleImage.index;
                        string folderName = $"customerGarden/{customerGarden.Id}/Image";
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

                        CustomerGardenImage customerGardenImage = new CustomerGardenImage()
                        {
                            CustomerGardenId = customerGarden.Id,
                            Image = url
                        };

                        await _unitOfWork.CustomerGardenImageRepository.AddAsync(customerGardenImage);
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
        public async Task<Pagination<CustomerGarden>> GetByCustomerId(int pageIndex, int pageSize, Guid id)
        {
            var customer = await _idUtil.GetCustomerAsync(id);
            var customerGardenQuery = _unitOfWork.CustomerGardenRepository
                .GetAllQueryable()
                .Where(x => x.CustomerId == customer.Id)
                .Include(x => x.CustomerGardenImages.Where(y => !y.IsDeleted))
                .Skip(pageIndex * pageSize)
                .Take(pageSize);
            var count = _unitOfWork.CustomerGardenRepository
               .GetAllQueryable()
               .Where(x => x.CustomerId == customer.Id)
               .Include(x => x.CustomerGardenImages.Where(y => !y.IsDeleted)).Count();

            var customerGardens = await customerGardenQuery.ToListAsync();

            foreach (CustomerGarden _garden in customerGardens)
            {
                // Load CustomerBonsais with Bonsai and BonsaiImages
                var bonsais = await _unitOfWork.CustomerBonsaiRepository
                    .GetAllQueryable()
                    .Where(cb => cb.CustomerGardenId == _garden.Id)
                    .Include(bonsai => bonsai.Bonsai)
                    .ThenInclude(x => x.BonsaiImages)
                    .ToListAsync();

                _garden.CustomerBonsais = bonsais;
            }
            Pagination<CustomerGarden> garden = new Pagination<CustomerGarden>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                Items = customerGardens,
                TotalItemsCount = count
            };
            return garden;
        }
        public async Task<Pagination<CustomerGarden>> GetAllByCustomerId(Guid id)
        {
            var customer = await _idUtil.GetCustomerAsync(id);
            var customerGardenQuery = _unitOfWork.CustomerGardenRepository
                .GetAllQueryable()
                .Where(x => x.CustomerId == customer.Id)
                .Include(x => x.CustomerGardenImages.Where(y => !y.IsDeleted));

            var customerGardens = await customerGardenQuery.ToListAsync();
            Pagination<CustomerGarden> garden = new Pagination<CustomerGarden>()
            {
                PageIndex = 0,
                PageSize = customerGardens.Count,
                Items = customerGardens,
                TotalItemsCount = customerGardens.Count
            };
            return garden;
        }
        public async Task Delete(Guid id)
        {
            var customerGarden = await _unitOfWork.CustomerGardenRepository.GetByIdAsync(id);
            if (customerGarden == null)
            {
                throw new Exception("Không tìm thấy");
            }
            _unitOfWork.CustomerGardenRepository.SoftRemove(customerGarden);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<Pagination<CustomerGarden>> GetPaginationForManager(int pageIndex, int pageSize)
        {
            List<Expression<Func<CustomerGarden, object>>> includes = new List<Expression<Func<CustomerGarden, object>>>{
                                 x => x.Customer.ApplicationUser,
                                 x => x.CustomerGardenImages
                                    };
            var customerGardens = await _unitOfWork.CustomerGardenRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted, includes: includes);
            return customerGardens;
        }

        public async Task UpdateCustomerGarden(Guid customerGardenId, CustomerGardenModel customerGardenModel, Guid customerId)
        {
            if (customerGardenModel == null)
            {
                throw new Exception("Vui lòng điền đầy đủ thông tin");
            }
            var customerGarden = _mapper.Map<CustomerGarden>(customerGardenModel);
            var customer = await _idUtil.GetCustomerAsync(customerId);
            customerGarden.CustomerId = customer.Id;
            customerGarden.Id = customerGardenId;
            try
            {
                _unitOfWork.BeginTransaction();
                 _unitOfWork.CustomerGardenRepository.Update(customerGarden);
                if (customerGardenModel.Image != null)
                {
                    var images = await _unitOfWork.CustomerGardenImageRepository.GetAsync(isTakeAll: true, expression: x => x.CustomerGardenId == customerGardenId && !x.IsDeleted, isDisableTracking: true);
                    if (customerGardenModel.OldImage != null)
                    {
                        foreach (CustomerGardenImage image in images.Items.ToList())
                        {
                            if (customerGardenModel.OldImage.Contains(image.Image))
                            {
                                //Bỏ những cái có trong danh sách cũ truyền về -> không xóa
                                images.Items.Remove(image);
                            }
                        }

                    }
                    _unitOfWork.CustomerGardenImageRepository.SoftRemoveRange(images.Items);
                    foreach (var singleImage in customerGardenModel.Image.Select((image, index) => (image, index)))
                    {
                        string newImageName = customerGarden.Id + "_i" + singleImage.index;
                        string folderName = $"customerGarden/{customerGarden.Id}/Image";
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

                        CustomerGardenImage customerGardenImage = new CustomerGardenImage()
                        {
                            CustomerGardenId = customerGarden.Id,
                            Image = url
                        };

                        await _unitOfWork.CustomerGardenImageRepository.AddAsync(customerGardenImage);
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
        public async Task<CustomerGarden> GetById(Guid customerGardenId, bool isCustomer, Guid userId)
        {   
            List<Expression<Func<CustomerGarden, object>>> includes = new List<Expression<Func<CustomerGarden, object>>>{
                                 x => x.Customer.ApplicationUser,
                                 x => x.CustomerGardenImages
                                    };
            Pagination<CustomerGarden> customerGarden;
            if (isCustomer)
            {
                var customer = await _idUtil.GetCustomerAsync(userId);
                customerGarden = await _unitOfWork.CustomerGardenRepository.GetAsync(isTakeAll: true, expression: x => x.Id == customerGardenId && x.CustomerId == customer.Id && !x.IsDeleted, includes: includes);
            }
            else
            {
                customerGarden = await _unitOfWork.CustomerGardenRepository.GetAsync(isTakeAll: true, expression: x => x.Id == customerGardenId && !x.IsDeleted, includes: includes);
            }
            return customerGarden.Items[0];
        }
    }
}
