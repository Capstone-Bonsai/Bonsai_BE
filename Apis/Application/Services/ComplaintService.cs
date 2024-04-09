using Application.Interfaces;
using Application.ViewModels.ComplaintModels;
using Domain.Entities;
using Firebase.Auth.Requests;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly IFirebaseService _firebaseService;

        public ComplaintService(IUnitOfWork unitOfWork, IClaimsService claimsService, IFirebaseService firebaseService)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _firebaseService = firebaseService;
        }
        public async Task CreateComplaint(string userId, ComplaintModel model)
        {
            var contract = await _unitOfWork.ContractRepository.GetAllQueryable().AsNoTracking().Include(x => x.ServiceGarden.CustomerGarden.Customer).FirstOrDefaultAsync(x => x.Id == model.ContractId);
            if (contract == null)throw new Exception("Không tìm thấy hợp đồng bạn yêu cầu");
            if (!contract.ServiceGarden.CustomerGarden.Customer.UserId.ToLower().Equals(userId.ToLower()))throw new Exception("Bạn không có quyền truy cập vào hợp đồng này");
            if(contract.ContractStatus < Domain.Enums.ContractStatus.Completed)throw new Exception("Hợp đồng này chưa hoàn thành nên không thể gửi khiếu nại.");
            if(contract.EndDate.AddDays(5) < DateTime.Now) throw new Exception("Hợp đồng này đã quá thời gian khiếu nại");
            _unitOfWork.BeginTransaction();
            if (model.ListImage == null || model.ListImage.Count == 0)
                throw new Exception("Vui lòng thêm hình ảnh khiếu nại.");
            try
            {
                var complaint = new Complaint { Detail = model.Detail, ContractId = model.ContractId };
                try
                {
                    await _unitOfWork.ComplaintRepository.AddAsync(complaint);
                    contract.ContractStatus = Domain.Enums.ContractStatus.Complained;
                     _unitOfWork.ContractRepository.Update(contract);
                    await _unitOfWork.SaveChangeAsync();
                }
                catch (Exception)
                {
                    throw new Exception("Đã xảy ra lỗi trong khởi tạo khiếu nại.");
                }
                List<ComplaintImage> complaintImages = new List<ComplaintImage>();
                foreach (var item in model.ListImage)
                {
                    string url = "";
                    try
                    {
                        url = await AddImageToFireBase(item, complaint);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Đã xảy ra lỗi trong quá trình tải ảnh lên FireBase.");
                    }

                    if (url.IsNullOrEmpty())
                        throw new Exception("Đã xảy ra lỗi trong quá trình tải ảnh lên firebase.");
                    var complaintImage = new ComplaintImage { ComplaintId = complaint.Id, Image = url };
                    complaintImages.Add(complaintImage);
                }
                try
                {
                    await _unitOfWork.ComplaintImageRepository.AddRangeAsync(complaintImages);
                    await _unitOfWork.SaveChangeAsync();
                }
                catch (Exception)
                {
                    throw new Exception("Đã xảy ra lỗi trong khởi tạo ảnh");
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> AddImageToFireBase(FormFile file, Complaint complaint)
        {
            Random random = new Random();
            var number = random.Next(1, 1000000);
            string newImageName = complaint.Id + "_i" + number;
            string folderName = $"complaint/{complaint.Id}/Image";
            string imageExtension = Path.GetExtension(file.FileName);
            //Kiểm tra xem có phải là file ảnh không.
            string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            const long maxFileSize = 20 * 1024 * 1024;
            if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || file.Length > maxFileSize)
            {
                throw new Exception("Có chứa file không phải ảnh hoặc quá dung lượng tối đa(>20MB)!");
            }
            var url = await _firebaseService.UploadFileToFirebaseStorage(file, newImageName, folderName);
            return url;
        }
        public async Task ReplyComplaint(ComplaintUpdateModel model)
        {
            try
            {
                var complaint = await _unitOfWork.ComplaintRepository.GetAllQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.ComplaintId);
                if (complaint == null) throw new Exception("Không tìm thấy phản ánh mà bạn yêu cầu");
                if (model.ComplaintStatus == Domain.Enums.ComplaintStatus.Request) throw new Exception("Trạng thái phản hồi không hợp lệ.");
                if (complaint.ComplaintStatus != Domain.Enums.ComplaintStatus.Request) throw new Exception("Phản hồi này đã được xử lý");
                var contract = await _unitOfWork.ContractRepository.GetAllQueryable().AsNoTracking().Include(x => x.BonsaiCareSteps).Include(x => x.GardenCareTasks).FirstOrDefaultAsync(x => x.Id == complaint.ContractId);
                if (model.ComplaintStatus == Domain.Enums.ComplaintStatus.Canceled && model.CancelReason == null) throw new Exception("Để hủy phản hồi cần phải có lý do hủy.");
                else
                {
                    complaint.CancelReason = model.CancelReason;
                    contract.ContractStatus = Domain.Enums.ContractStatus.Completed;
                }
                complaint.ComplaintStatus = model.ComplaintStatus;

                _unitOfWork.BeginTransaction();
                if (model.ComplaintStatus == Domain.Enums.ComplaintStatus.Processing)
                {
                    contract.ContractStatus = Domain.Enums.ContractStatus.ProcessingComplaint;
                    _unitOfWork.ContractRepository.Update(contract);
                    if (contract.CustomerBonsaiId == null)
                    {
                        foreach (var item in contract.GardenCareTasks)
                            item.CompletedTime = null;
                        _unitOfWork.ClearTrack();
                        _unitOfWork.GardenCareTaskRepository.UpdateRange(contract.GardenCareTasks.ToList());
                    }
                    else
                    {
                        foreach (var item in contract.BonsaiCareSteps)
                            item.CompletedTime = null;
                        _unitOfWork.ClearTrack();
                        _unitOfWork.BonsaiCareStepRepository.UpdateRange(contract.BonsaiCareSteps.ToList());
                    }
                }
                else
                {
                    contract.ContractStatus = Domain.Enums.ContractStatus.ProcessedComplaint;
                    _unitOfWork.ContractRepository.Update(contract);
                    if (contract.CustomerBonsaiId == null)
                    {
                        foreach (var item in contract.GardenCareTasks)
                        {
                            if (item.CompletedTime == null) throw new InvalidOperationException("Không thể cập nhật trạng thái hoàn thành khiếu nại khi các nhiệm vụ của hợp đồng này chưa được hoàn thành");
                        }
                    }
                    else
                    {
                        foreach (var item in contract.GardenCareTasks)
                        {
                            if (item.CompletedTime == null) throw new InvalidOperationException("Không thể cập nhật trạng thái hoàn thành khiếu nại khi các nhiệm vụ của hợp đồng này chưa được hoàn thành");
                        }
                    }

                }
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
            
        }
    }
}
