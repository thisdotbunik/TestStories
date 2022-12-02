using System.Threading.Tasks;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public interface IPartnerReadService
    {
        Task<PartnerDetailViewModel> GetPartnerAsync (int id);
        Task<CollectionModel<PartnerDetailViewModel>> SearchPartner (FilterPartnerViewRequest partnerFilter);
        Task<CollectionModel<PartnerAutoCompleteSerachViewModel>> SearchPartnersAutoComplete ();
        Task<CollectionModel<PartnerDetailViewModel>> GetPartnerDetails (int? id , int page , int pageSize);
        Task<CollectionModel<PartnerModel>> GetShowcasePartners ();
        Task<CollectionModel<PartnerModel>> GetActivePartnersAsync ();
        Task<PartnerMedia> GetDistributionMedia (long mediaId , int partnerId);
        Task<PartnerViewModel> GetPartnerMedia (int id , int PageSize , int Page);
        Task<PartnerDistributionDetailsViewModel> GetPartnerDistribution (FilterPartnerDistributionViewRequest _filter);
        Task<CollectionModel<DistributionAutocompleteViewModel>> DistributionMediaAutoCompleteSearch ();
        Task<PartnerDistributionDetailsViewModel> DistributionMediaSearch (FilterPartnerDistributionViewRequest _filter);
        Task<CollectionModel<PartnerMediaViewModel>> GetPartnerMedia (PartnerMediaFilterRequest request);
        Task<CollectionModel<MediaAutoCompleteModel>> GetPartnerMediaAutoComplete (int partnerId , string filterString);
    }
}
