using Application.ViewModels.ComplaintModels;

namespace Application.Interfaces
{
    public interface IComplaintService
    {
        public Task CreateComplaint(string userId, ComplaintModel model);
        public Task ReplyComplaint(ComplaintUpdateModel model);
    }
}
