using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestStories.API.Models.ResponseModels
{
    public class CommonApis
    {
        public List<SeriesLookup> Series { get; set; }
        public List<SeriesTypeLookup> SeriesType { get; set; }
        public List<TopicLookup> Topics { get; set; }
        public List<TagLookup> Tags { get; set; }
        public List<SourceLookup> Sources { get; set; }
        public List<UserTypes> UserType { get; set; }
        public List<Status> UserStatus { get; set; }
        public List<MediaTypes> MediaTypes { get; set; }
        public List<Media_Status> MediaStatus { get; set; }
        public List<Editors> Editor { get; set; }
        public List<Publisher> Publishers { get; set; }
        [JsonProperty(propertyName: "distributionPartners")]
        public List<SourceLookup> DistributionPartners { get; set; }

        [JsonProperty(propertyName: "activeContentPartners")]
        public List<SourceLookup> ActiveContentPartners { get; set; }

        [JsonProperty(propertyName: "experimentStatus")]
        public List<ExperimentStatus> ExperimentStatus { get; set; }

    }

    public class SeriesLookup
    {

        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }

    public class SeriesTypeLookup
    {

        [JsonProperty(propertyName: "id")]
        public byte Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }

    public class TopicLookup
    {

        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }

    public class TagLookup
    {

        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }


    public class SourceLookup
    {

        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }


    public class UserTypes
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }

    public class Status
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }

    public class MediaTypes
    {

        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }

    public class Media_Status
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }

    public class Editors
    {

        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }

    public class Publisher
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }

    public class ExperimentStatus
    {
        [JsonProperty(propertyName: "id")]
        public int Id { get; set; }
        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }
    }

    public class LookupType
    {
        [JsonProperty(propertyName: "topic")]
        public bool Topic { get; set; }

        [JsonProperty(propertyName: "series")]
        public bool Series { get; set; }

        [JsonProperty(propertyName: "seriesType")]
        public bool SeriesType { get; set; }

        [JsonProperty(propertyName: "tags")]
        public bool Tags { get; set; }

        [JsonProperty(propertyName: "source")]
        public bool Source { get; set; }

        [JsonProperty(propertyName: "distributionPartner")]
        public bool DistributionPartner { get; set; }

        [JsonProperty(propertyName: "userType")]
        public bool UserType { get; set; }

        [JsonProperty(propertyName: "userStatus")]
        public bool UserStatus { get; set; }

        [JsonProperty(propertyName: "mediaType")]
        public bool MediaType { get; set; }

        [JsonProperty(propertyName: "mediaStatus")]
        public bool MediaStatus { get; set; }

        [JsonProperty(propertyName: "editors")]
        public bool Editors { get; set; }

        [JsonProperty(propertyName: "publishers")]
        public bool Publishers { get; set; }

        [JsonProperty(propertyName: "activeContentPartners")]
        public bool ActiveContentPartners { get; set; }

        [JsonProperty(propertyName: "experimentStatus")]
        public bool ExperimentStatus { get; set; }
    }
}