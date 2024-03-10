using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.StyleViewModels;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class StyleService : IStyleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StyleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Pagination<Style>> GetStyles()
        {
            var styles = await _unitOfWork.StyleRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true);
            return styles;
        }
        public async Task AddStyle(StyleModel styleModel)
        {
            var checkStyle = await _unitOfWork.StyleRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(styleModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            if (checkStyle.Items.Count > 0)
                throw new Exception("Dáng cây này đã tồn tại!");
            var style = _mapper.Map<Style>(styleModel);
            try
            {
                await _unitOfWork.StyleRepository.AddAsync(style);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình tạo mới. Vui lòng thử lại!");
            }
        }
        public async Task<Style?> GetStyleById(Guid id)
        {
            var style = await _unitOfWork.StyleRepository.GetByIdAsync(id);
            return style;
        }
        public async Task UpdateStyle(Guid id, StyleModel styleModel)
        {
            var checkStyle = await _unitOfWork.StyleRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(styleModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            if (checkStyle.Items.Count > 0)
                throw new Exception("Dáng cây này đã tồn tại!");
            var style = _mapper.Map<Style>(styleModel);
            style.Id = id;
            var result = await _unitOfWork.StyleRepository.GetByIdAsync(style.Id);
            if (result == null)
                throw new Exception("Không tìm thấy dáng cây!");
            try
            {
                _unitOfWork.StyleRepository.Update(style);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }
        public async Task DeleteStyle(Guid id)
        {
            var result = await _unitOfWork.StyleRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy dáng cây!");
            try
            {
                _unitOfWork.StyleRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa dáng cây. Vui lòng thử lại!");
            }
        }
    }
}
