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
            var styles = await _unitOfWork.StyleRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true, orderBy: x => x.OrderBy(style => style.Name.Contains("Khác")));
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
                throw new Exception("Không tìm thấy!");
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
                throw new Exception("Không tìm thấy");
            var bonsais = await _unitOfWork.BonsaiRepository.GetAsync(pageIndex: 0, pageSize: 1, expression: x => x.StyleId == id && !x.IsDeleted);
            if (bonsais.TotalItemsCount > 0)
            {
                throw new Exception("Còn tồn tại cây thuộc về phân loại này, không thể xóa!");
            }
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
        public async Task<List<StyleCountViewModel>> GetStyleCount()
        {
            var styles = await _unitOfWork.StyleRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true);
            List<StyleCountViewModel> styleCountViewModels = new List<StyleCountViewModel>();
            foreach (Style style in styles.Items)
            {
                var numberOfBonsai = await _unitOfWork.BonsaiRepository.GetAsync(isTakeAll: true, expression: x => x.StyleId == style.Id && !x.IsDeleted && !x.isDisable && x.isSold == false);
                styleCountViewModels.Add(new StyleCountViewModel()
                {
                    Id = style.Id,
                    name = style.Name,
                    count = numberOfBonsai.Items.Count
                });
            }
            return styleCountViewModels;
        }
    }
}
