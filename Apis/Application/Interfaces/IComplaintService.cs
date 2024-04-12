using Application.ViewModels.ComplaintModels;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IComplaintService
    {
        public Task CreateComplaint(string userId, ComplaintModel model);
        public Task ReplyComplaint(ComplaintUpdateModel model);
        public Task<IList<Complaint>> GetList();
    }
}
