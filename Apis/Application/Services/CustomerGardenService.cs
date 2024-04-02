using Application.Commons;
using Application.Interfaces;
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

        public CustomerGardenService(IUnitOfWork unitOfWork, IMapper mapper, FirebaseService fireBaseService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fireBaseService = fireBaseService;
            _userManager = userManager;
        }

        public async Task AddCustomerGarden(CustomerGardenModel customerGardenModel, Guid id)
        {
            if (customerGardenModel == null)
            {
                throw new Exception("Không có thông tin vườn");
            }
            var customerGarden = _mapper.Map<CustomerGarden>(customerGardenModel);
            var customer = await GetCustomerAsync(id);
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
        private async Task<Customer> GetCustomerAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            if (!isCustomer)
                throw new Exception("Bạn không có quyền để thực hiện hành động này!");
            var customer = await _unitOfWork.CustomerRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.UserId.ToLower().Equals(user.Id.ToLower()));
            if (customer == null)
                throw new Exception("Không tìm thấy thông tin người dùng");
            return customer;
        }
        public async Task<Pagination<CustomerGarden>> GetByCustomerId(int pageIndex, int pageSize, Guid id)
        {
            var customer = await GetCustomerAsync(id);
            var customerGardenQuery = _unitOfWork.CustomerGardenRepository
                .GetAllQueryable()
                .Include(x => x.CustomerGardenImages.Where(y => !y.IsDeleted))
                .Skip(pageIndex * pageSize)
                .Take(pageSize);
            var count = _unitOfWork.CustomerGardenRepository
               .GetAllQueryable()
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
            var customer = await GetCustomerAsync(id);
            var customerGardenQuery = _unitOfWork.CustomerGardenRepository
                .GetAllQueryable()
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

        public async Task<Pagination<CustomerGarden>> GetPaginationForAdmin(int pageIndex, int pageSize)
        {
            List<Expression<Func<CustomerGarden, object>>> includes = new List<Expression<Func<CustomerGarden, object>>>{
                                 x => x.Customer.ApplicationUser,
                                 x => x.CustomerGardenImages
                                    };
            var customerGardens = await _unitOfWork.CustomerGardenRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted, includes: includes);
            return customerGardens;
        }
    }
}
