using Application.Commons;
using Application.ViewModels.TagViewModels;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ITagService
    {
        Task<Pagination<Tag>> GetTags();
        Task AddTag(TagModel tagModel);
        Task UpdateTag(Guid id, TagModel tagModel);
        Task DeleteTag(Guid id);
    }
}
