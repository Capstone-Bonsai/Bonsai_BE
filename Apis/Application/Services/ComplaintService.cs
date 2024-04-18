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
            var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetAllQueryable().AsNoTracking().Include(x => x.CustomerGarden.Customer).Include(x=>x.Complaints).FirstOrDefaultAsync(x => x.Id == model.ServiceOrderId);
            if (serviceOrder == null)throw new Exception("Không tìm thấy đơn đặt hàng dịch vụ của bạn");
            if(serviceOrder.ServiceOrderStatus == Domain.Enums.ServiceOrderStatus.Completed) throw new Exception("Đơn đặt hàng dịch vụ này đã hoàn thành nên không thể khiếu nại.");
            if (!serviceOrder.CustomerGarden.Customer.UserId.ToLower().Equals(userId.ToLower()))throw new Exception("Bạn không có quyền truy cập vào đơn đặt hàng dịch vụ này");
            if (serviceOrder.ServiceOrderStatus == Domain.Enums.ServiceOrderStatus.TaskFinished || serviceOrder.ServiceOrderStatus == Domain.Enums.ServiceOrderStatus.DoneTaskComplaint) { }else throw new Exception("Đơn đặt hàng dịch vụ này đang được xử lý nên không thể gửi khiếu nại.");
            if(serviceOrder.EndDate.AddDays(3) < DateTime.Now) throw new Exception("Đơn đặt hàng dịch vụ này đã quá thời gian khiếu nại");
            _unitOfWork.BeginTransaction();
            if (model.ListImage == null || model.ListImage.Count == 0)
                throw new Exception("Vui lòng thêm hình ảnh khiếu nại.");
             
            if (serviceOrder.Complaints!=null && serviceOrder.Complaints.Count >= 0)
            {
                foreach (var item in serviceOrder.Complaints)
                {
                    if(item.ComplaintStatus == Domain.Enums.ComplaintStatus.Request|| item.ComplaintStatus == Domain.Enums.ComplaintStatus.Processing) throw new Exception("Đơn đặt hàng dịch vụ này hiện đâng có khiếu nại đang xử lý.");
                }
            }
            
            try
            {
                var complaint = new Complaint{ Detail = model.Detail, ServiceOrderId = model.ServiceOrderId };
                try
                {
                    await _unitOfWork.ComplaintRepository.AddAsync(complaint);
                    serviceOrder.ServiceOrderStatus = Domain.Enums.ServiceOrderStatus.Complained;
                     _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
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

        public async Task<string> AddImageToFireBase(IFormFile file, Complaint complaint)
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
                _unitOfWork.BeginTransaction();
                var complaint = await _unitOfWork.ComplaintRepository.GetAllQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.ComplaintId);
                if (complaint == null) throw new Exception("Không tìm thấy khiếu nại mà bạn yêu cầu");
                if (model.ComplaintStatus == Domain.Enums.ComplaintStatus.Request) throw new Exception("Trạng thái khiếu nại không hợp lệ.");
                if (complaint.ComplaintStatus == Domain.Enums.ComplaintStatus.Canceled) throw new Exception("Khiếu nại này đã bị hủy nên không thể cập nhật trạng thái mới.");
                if (complaint.ComplaintStatus == Domain.Enums.ComplaintStatus.Completed) throw new Exception("Khiếu nại này đã hoàn thành nên không thể cập nhật trạng thái mới.");

                if (complaint.ComplaintStatus != Domain.Enums.ComplaintStatus.Request && model.ComplaintStatus != Domain.Enums.ComplaintStatus.Completed) throw new Exception("Khiếu nại này đã được xử lý");
                var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetAllQueryable().AsNoTracking().Include(x => x.BonsaiCareSteps).Include(x => x.GardenCareTasks).FirstOrDefaultAsync(x => x.Id == complaint.ServiceOrderId);
                if (model.ComplaintStatus == Domain.Enums.ComplaintStatus.Canceled && model.CancelReason == null) throw new Exception("Để hủy khiếu nại cần phải có lý do hủy.");
                if(model.ComplaintStatus == Domain.Enums.ComplaintStatus.Canceled)
                {
                    complaint.CancelReason = model.CancelReason;
                    serviceOrder.ServiceOrderStatus = Domain.Enums.ServiceOrderStatus.Completed;
                    _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
                }
                else if (model.ComplaintStatus == Domain.Enums.ComplaintStatus.Processing)
                {
                    if(complaint.ComplaintStatus != Domain.Enums.ComplaintStatus.Request) throw new Exception("Trạng thái khiếu nại không hợp lệ.");
                    serviceOrder.ServiceOrderStatus = Domain.Enums.ServiceOrderStatus.ProcessingComplaint;
                    _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
                    await _unitOfWork.SaveChangeAsync();
                    if (serviceOrder.CustomerBonsaiId!=null)
                    {
                        var tasks = await _unitOfWork.BonsaiCareStepRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == serviceOrder.Id);
                        foreach (BonsaiCareStep bonsaiCareStep in tasks.Items)
                        {
                            bonsaiCareStep.CompletedTime = null;
                        }
                        _unitOfWork.ClearTrack();
                        _unitOfWork.BonsaiCareStepRepository.UpdateRange(tasks.Items);
                        
                    }
                    else
                    {
                        var tasks = await _unitOfWork.GardenCareTaskRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == serviceOrder.Id);
                        foreach (GardenCareTask gardenCareTask in tasks.Items)
                        {
                            gardenCareTask.CompletedTime = null;
                        }
                        _unitOfWork.ClearTrack();
                        _unitOfWork.GardenCareTaskRepository.UpdateRange(tasks.Items);
                        
                    }
                }
                else if(model.ComplaintStatus == Domain.Enums.ComplaintStatus.Completed)
                {
                    if (complaint.ComplaintStatus != Domain.Enums.ComplaintStatus.Processing) throw new Exception("Trạng thái khiếu nại không hợp lệ.");
                    serviceOrder.ServiceOrderStatus = Domain.Enums.ServiceOrderStatus.DoneTaskComplaint;
                    _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
                    await _unitOfWork.SaveChangeAsync();
                    if (serviceOrder.CustomerBonsaiId == null)
                    {
                        foreach (var item in serviceOrder.GardenCareTasks)
                        {
                            if (item.CompletedTime == null) throw new InvalidOperationException("Không thể cập nhật trạng thái hoàn thành khiếu nại khi các nhiệm vụ của đơn đặt hàng dịch vụ này chưa được hoàn thành");
                        }
                    }
                    else
                    {
                        foreach (var item in serviceOrder.BonsaiCareSteps)
                        {
                            if (item.CompletedTime == null) throw new InvalidOperationException("Không thể cập nhật trạng thái hoàn thành khiếu nại khi các nhiệm vụ của đơn đặt hàng dịch vụ này chưa được hoàn thành");
                        }
                    }

                }
                complaint.ComplaintStatus = model.ComplaintStatus;
                _unitOfWork.ComplaintRepository.Update(complaint);
                await _unitOfWork.SaveChangeAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
            
        }
        public async Task<IList<Complaint>> GetList()
        {
            var complaints = await _unitOfWork.ComplaintRepository.GetAllQueryable().OrderBy(x=>x.ComplaintStatus).ToListAsync();
            return complaints;
        }
    }
}
