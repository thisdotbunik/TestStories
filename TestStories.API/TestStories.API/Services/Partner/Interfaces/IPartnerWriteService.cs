using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestStories.API.Models.ResponseModels;

namespace TestStories.API.Services
{
    public interface IPartnerWriteService
    {
        Task<PartnerResponseModel> AddPartnerAsync (AddPartnerViewModel entity);

        Task<PartnerResponseModel> EditPartnerAsync (int partnerId , EditPartnerViewModel model);

        Task RemovePartnerAsync(int partnerId);

        Task ArchivePartnerAsync (int id);
        Task ExipreDistributionPartner(List<int> partnerId);

        Task updateEndDateOfDistributionPartner(int partnerId, DateTime endDate);

        Task UnarchivePartner(int partnerId);

    }
}
