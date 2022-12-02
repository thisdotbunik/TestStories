using TestStories.Common;

namespace TestStories.API.Services
{
    public interface IImageMigration
    {
        void ProcessImages(EntityType entityType);
    }
}
