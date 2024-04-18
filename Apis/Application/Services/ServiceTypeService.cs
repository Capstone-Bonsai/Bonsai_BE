using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.ServiceTypeViewModels;
using Application.ViewModels.ServiceViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Pkcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ServiceTypeService : IServiceTypeService
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFirebaseService _firebaseService;

        public ServiceTypeService(IUnitOfWork unit, IMapper mapper, UserManager<ApplicationUser> userManager, IFirebaseService firebaseService)
        {
            _unit = unit;
            _mapper = mapper;
            _userManager = userManager;
            _firebaseService = firebaseService;
        }
        public async Task<Pagination<ServiceType>> Get()
        {
            var serviceType = await _unit.ServiceTypeRepository.GetAsync(isTakeAll: true);
            return serviceType;
        }
        public async Task Update(Guid serviceTypeId, ServiceTypeModel serviceTypeModel)
        {
            if (serviceTypeModel.Description == null && serviceTypeModel.Image == null)
            {
                throw new Exception("Không có thông tin cập nhật");
            }
            var serviceType = await _unit.ServiceTypeRepository.GetByIdAsync(serviceTypeId);
            if (serviceType == null)
            {
                throw new Exception("Không tìm thấy dịch vụ");
            }
            if (serviceTypeModel.Description != null && serviceTypeModel.Description != "")
            {
                if (serviceTypeModel.Description.Length > 1000)
                {
                    throw new Exception("Ghi chú không được quá 1000 kí tự");
                }
                serviceType.Description = serviceTypeModel.Description;
            }

            if (serviceTypeModel.Image != null)
            {
                string newImageName = serviceType.Id + "_i" + serviceTypeModel.Image;
                string folderName = $"serviceType/{serviceType.Id}/Image";
                string imageExtension = Path.GetExtension(serviceTypeModel.Image.FileName);
                string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                const long maxFileSize = 20 * 1024 * 1024;
                if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || serviceTypeModel.Image.Length > maxFileSize)
                {
                    throw new Exception("Có chứa file không phải ảnh hoặc quá dung lượng tối đa(>20MB)!");
                }
                var url = await _firebaseService.UploadFileToFirebaseStorage(serviceTypeModel.Image, newImageName, folderName);
                if (url == null)
                    throw new Exception("Lỗi khi đăng ảnh lên firebase!");
                serviceType.Image = url;
            }
            _unit.ServiceTypeRepository.Update(serviceType);
            await _unit.SaveChangeAsync();
        }

    }
}
