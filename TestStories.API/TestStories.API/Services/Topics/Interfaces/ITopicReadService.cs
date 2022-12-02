using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface ITopicReadService
    {
        Task<CollectionModel<TopicViewModel>> GetTopicViewCollectionAsync (FilterTopicViewRequest filter);
        Task<CollectionModel<TopicAutoCompleteModel>> TopicAutoCompleteSearch ();
        Task<CollectionModel<TopicModel>> GetTopicsAsync (int? parentId);
        Task<TopicModel> GetTopicAsync (int id);
        Task<CollectionModel<ShortTopicModel>> GetShortTopicsAsync ();
    }
}
