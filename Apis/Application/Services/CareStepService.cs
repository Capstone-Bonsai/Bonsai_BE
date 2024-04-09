using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.CareStepViewModels;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.DeliveryFeeViewModels;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CareStepService : ICareStepService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CareStepService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task AddCareSteps(CareStepModel careStepModel)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var result = await _unitOfWork.CareStepRepository.GetAsync(isTakeAll: true, expression: x => x.CategoryId == careStepModel.CategoryId && !x.IsDeleted);
                _unitOfWork.CareStepRepository.SoftRemoveRange(result.Items);
                List<CareStep> careSteps = new List<CareStep>();
                for (int i = 1; i < 1 + careStepModel.CareSteps.Count; i++)
                {
                    careSteps.Add(new CareStep()
                    {
                        CategoryId = careStepModel.CategoryId,
                        OrderStep = i,
                        Step = careStepModel.CareSteps[i - 1]
                    });
                }
                await _unitOfWork.CareStepRepository.AddRangeAsync(careSteps);
                await _unitOfWork.CommitTransactionAsync();
            } catch(Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }
        public async Task<Pagination<CareStep>> GetCareStepsByCategoryId(Guid categoryId)
        {
            try
            {
                var careSteps = await _unitOfWork.CareStepRepository.GetAsync(isTakeAll: true, expression: x => x.CategoryId == categoryId && !x.IsDeleted, orderBy: query => query.OrderBy(x => x.OrderStep), isDisableTracking: true);
                return careSteps;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task UpdateCareStep(Guid id, CareStepUpdateModel careStepUpdateModel)
        {
            if (careStepUpdateModel == null)
            {
                throw new Exception("Vui lòng nhập lại!");
            }
            var careStep = await _unitOfWork.CareStepRepository.GetByIdAsync(id);
            if (careStep == null)
            {
                throw new Exception("Không tìm thấy bước chăm sóc!");
            }
            careStep.Step = careStepUpdateModel.CareStep;
            try
            {
                _unitOfWork.CareStepRepository.Update(careStep);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }
        public async Task DeleteCareStep(Guid id)
        {
            var result = await _unitOfWork.CareStepRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy bước chăm sóc!");
            try
            {
                _unitOfWork.CareStepRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa bước chăm sóc. Vui lòng thử lại!");
            }
        }
    }
}
