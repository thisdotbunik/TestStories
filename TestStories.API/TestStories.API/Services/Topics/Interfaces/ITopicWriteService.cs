using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface ITopicWriteService
    {
        Task DeleteTopicByName(string name);

        Task<TopicModel> AddTopicAsync (AddTopicModel model);

        Task<TopicModel> EditTopicAsync (EditTopicModel model);

        Task RemoveTopicAsync(int id);

        Task UpdateCloudTopics ();

        Task MigrateDbTopicsToCloud ();
    }
}
