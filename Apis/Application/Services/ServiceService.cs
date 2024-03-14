using Application.Commons;
using Application.Interfaces;
using Application.Validations.Order;
using Application.Validations.Services;
using Application.ViewModels.OrderViewModels;
using Application.ViewModels.ServiceViewModels;
using AutoMapper;
using Domain.Entities;
using Firebase.Auth;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFirebaseService _firebaseService;

        public ServiceService(IUnitOfWork unit, IMapper mapper, UserManager<ApplicationUser> userManager, IFirebaseService firebaseService)
        {
            _unit = unit;
            _mapper = mapper;
            _userManager = userManager;
            _firebaseService = firebaseService;
        }
        public async Task<IList<string>> ValidateServiceModel(ServiceModel model)
        {
            if (model == null)
            {
                throw new Exception("Vui lòng điền đầy đủ thông tin.");
            }
            var validator = new ServiceModelValidator();
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                var errors = new List<string>();
                errors.AddRange(result.Errors.Select(x => x.ErrorMessage));
                return errors;
            }
            if (model.ServiceType == Domain.Enums.ServiceType.GardenCare) {
                if (model.ServiceBaseTaskId == null || model.ServiceBaseTaskId.Count == 0)
                {
                    throw new Exception("Vui lòng điền danh sách công việc đi kèm dịch vụ.");
                }
            }
            
            return null;
        }
        public async Task<IList<string>> AddService(ServiceModel model)
        {
            var errs = await ValidateServiceModel(model);
            if (errs != null)
            {
                return errs;
            }
            if (model.Image == null)
            {
                throw new Exception("Không để trống hình ảnh");
            }
            else
            {
                try
                {
                    _unit.BeginTransaction();
                    
                    //thêm ảnh
                    IList<string> list = await AddImage(model.Image, null);
                    // thêm service
                    Guid serviceId = Guid.Parse(list[0]);
                    await CreateService(model, list[1], serviceId);
                    if (model.ServiceType == Domain.Enums.ServiceType.GardenCare)
                    {
                        // Add service task
                        await CreateServiceBaseTask(model.ServiceBaseTaskId, serviceId);
                    }
                    await _unit.CommitTransactionAsync();
                    return null;
                }
                catch (Exception ex)
                {
                    _unit.RollbackTransaction();
                    throw new Exception(ex.Message);
                }

            }

        }
        public async Task<IList<string>> AddImage(IFormFile Image, Guid? id)
        {
            var serviceId = Guid.NewGuid();
            if (id != null)
                serviceId = id.Value;
            Random random = new Random();
            int number = random.Next(1000, 10000);
            string newImageName = serviceId + "_i" + number;
            string folderName = $"bonsai/{serviceId}/Image";
            string imageExtension = Path.GetExtension(Image.FileName);
            //Kiểm tra xem có phải là file ảnh không.
            string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            const long maxFileSize = 20 * 1024 * 1024;
            if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || Image.Length > maxFileSize)
            {
                throw new Exception("Có chứa file không phải ảnh hoặc quá dung lượng tối đa(>20MB)!");
            }
            var url = await _firebaseService.UploadFileToFirebaseStorage(Image, newImageName, folderName);
            if (url == null)
                throw new Exception("Lỗi khi đăng ảnh lên firebase!");
            List<string> list = new List<string>();
            list.Add(serviceId.ToString().ToLower());
            list.Add(url);
            return list;

        }

        public async Task CreateService(ServiceModel model, string image, Guid serviceId)
        {
            var service = await _unit.ServiceRepository.GetAllQueryable().Include(x => x.ServiceBaseTasks).FirstOrDefaultAsync(x => x.Name.ToLower().Equals(model.Name.ToLower()));
            if (service != null)
                throw new Exception("Tên dịch vụ này đã tồn tại.");
            var newService = _mapper.Map<Service>(model);
            newService.IsDisable = false;
            newService.Image = image;
            newService.Id = serviceId;
            await _unit.ServiceRepository.AddAsync(newService);
        }
        public async Task CreateServiceBaseTask(IList<Guid> listBaseTask, Guid serviceId)
        {
            foreach (var item in listBaseTask.Distinct())
            {

                var baseTask = await _unit.BaseTaskRepository.GetByIdAsync(item);
                if (baseTask == null)
                    throw new Exception("Không tìm thấy nhiệm vụ");
                try
                {
                    var serviceBaseTask = new ServiceBaseTask();
                    serviceBaseTask.BaseTaskId = baseTask.Id;
                    serviceBaseTask.ServiceId = serviceId;
                    await _unit.ServiceBaseTaskRepository.AddAsync(serviceBaseTask);
                }
                catch (Exception ex)
                {

                    var er = ex;
                    throw new Exception("Tạo nhiệm vụ cho dịch vụ đã xảy ra lỗi");
                }
            }
        }

        public async Task<ServiceViewModel> GetServiceById(Guid id, string? userId)
        {
            Service services = new Service();
            if (userId == null || userId.Equals("00000000-0000-0000-0000-000000000000"))
            {
                services = await _unit.ServiceRepository.GetAllQueryable()
                       .AsNoTracking()
                       .Include(x => x.ServiceBaseTasks)
                       .ThenInclude(x => x.BaseTask)
                       .FirstOrDefaultAsync(x => x.IsDeleted == false && !x.IsDisable && x.Id == id);
                      
            }
            else
            {
                var user = await _userManager.FindByIdAsync(userId);
                var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
                var isAdmin = await _userManager.IsInRoleAsync(user, "Manager");
                var isStaff = await _userManager.IsInRoleAsync(user, "Staff");

                if (isCustomer && !isAdmin && !isStaff)
                    services = await _unit.ServiceRepository.GetAllQueryable()
                        .AsNoTracking()
                        .Include(x => x.ServiceBaseTasks)
                        .ThenInclude(x => x.BaseTask).FirstOrDefaultAsync(x => x.IsDeleted == false && x.IsDisable == false && x.Id == id);
                else if (isAdmin || isStaff)
                    services = await _unit.ServiceRepository.GetAllQueryable()
                        .AsNoTracking()
                        .Include(x => x.ServiceBaseTasks)
                        .ThenInclude(x => x.BaseTask).FirstOrDefaultAsync(x => x.IsDeleted == false && x.Id == id);
            }
            if (services == null)
                throw new Exception("Không tìm thấy");
            var result = _mapper.Map<ServiceViewModel>(services);
            return result;
        }

        public async Task<Pagination<ServiceViewModel>> GetServicePagination(string? userId, int pageIndex = 0, int pageSize = 10)
        {
            IList<Service> services = new List<Service>();
            if (userId == null || userId.Equals("00000000-0000-0000-0000-000000000000"))
            {
                services = await _unit.ServiceRepository.GetAllQueryable()
                       .AsNoTracking()
                       .Include(x => x.ServiceBaseTasks)
                       .ThenInclude(x => x.BaseTask)
                       .Where(x => x.IsDeleted == false && !x.IsDisable)
                       .OrderByDescending(x => x.CreationDate).ToListAsync();
            }
            else
            {
                var user = await _userManager.FindByIdAsync(userId);
                var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
                var isAdmin = await _userManager.IsInRoleAsync(user, "Manager");
                var isStaff = await _userManager.IsInRoleAsync(user, "Staff");

                if (isCustomer && !isAdmin && !isStaff)
                    services = await _unit.ServiceRepository.GetAllQueryable()
                        .AsNoTracking()
                        .Include(x => x.ServiceBaseTasks)
                        .ThenInclude(x => x.BaseTask).Where(x => x.IsDeleted == false && x.IsDisable == false).OrderByDescending(x => x.CreationDate).ToListAsync();
                else if (isAdmin || isStaff)
                    services = await _unit.ServiceRepository.GetAllQueryable()
                        .AsNoTracking()
                        .Include(x => x.ServiceBaseTasks)
                        .ThenInclude(x => x.BaseTask).Where(x => x.IsDeleted == false).OrderByDescending(x => x.CreationDate).ToListAsync();
            }
            var itemCount = services.Count();
            var items = services.OrderByDescending(x => x.CreationDate)
                                    .Skip(pageIndex * pageSize)
                                    .Take(pageSize)
                                    .ToList();

            var res = new Pagination<Service>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItemsCount = itemCount,
                Items = items,
            };

            var result = _mapper.Map<Pagination<ServiceViewModel>>(res);
            return result;
        }

        public async Task<IList<string>> UpdateService(Guid id, ServiceModel model)
        {
            var service = await _unit.ServiceRepository
                .GetAllQueryable()
                .Include(x => x.ServiceBaseTasks)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
            if (service == null)
                throw new Exception("Không tìm thấy dịch vụ bạn muốn tìm");
            var errs = await ValidateServiceModel(model);
            if (errs != null)
            {
                return errs;
            }
            else
            {
                IList<string> list = new List<string>();
                try
                {
                    _unit.BeginTransaction();
                    //thêm ảnh
                    if (model.Image != null)
                        list = await AddImage(model.Image, service.Id);

                    // sửa  service
                    if (model.Image != null)
                        Update(service, model, list[1]);
                    else
                        Update(service, model, null);

                    //xóa serviceBaseTAsk
                    _unit.ServiceBaseTaskRepository.HardDeleteRange(service.ServiceBaseTasks.ToList());
                    // Add service task
                    await CreateServiceBaseTask(model.ServiceBaseTaskId, service.Id);
                    await _unit.CommitTransactionAsync();
                    return null;
                }
                catch (Exception ex)
                {
                    _unit.RollbackTransaction();
                    throw new Exception(ex.Message);
                }

            }
        }
        public void Update(Service service, ServiceModel model, string Image)
        {
            service.Name = model.Name;
            service.Description = model.Description;
            service.StandardPrice = model.StandardPrice;
            if (Image != null)
                service.Image = Image;
            service.IsDisable = model.IsDisable;
            _unit.ServiceRepository.Update(service);

        }

        public async Task DeleteService(Guid id)
        {
            var service = await _unit.ServiceRepository
               .GetAllQueryable().Include(x=>x.ServiceBaseTasks)
               .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
            if (service == null)
                throw new Exception("Không tìm thấy dịch vụ bạn muốn tìm");
            try
            {
                _unit.BeginTransaction();
                //xóa service
                _unit.ServiceRepository.SoftRemove(service);

                //xóa serviceBaseTAsk
                _unit.ServiceBaseTaskRepository.SoftRemoveRange(service.ServiceBaseTasks.ToList());
                await _unit.CommitTransactionAsync();
                
            }
            catch (Exception ex)
            {
                _unit.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }
    }
}
