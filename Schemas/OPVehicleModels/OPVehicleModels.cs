using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BPMSoft.Configuration.Models
{

    public class VehicleBaseResponse<T>
    {
        [JsonProperty("data")]
        public List<T> Data { get; set; }
    }

    public class VehicleBrandDto
    {
        [JsonProperty("id")]
        public string ExternalId { get; set; }

        [JsonProperty("numeric_id")]
        public string ExternalNumericId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("models")]
        public List<VehicleModelDto> Models { get; set; }
    }

    public class VehicleModelDto
    {
        [JsonProperty("id")]
        public string ExternalId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("year_from")]
        public int YearFrom { get; set; }

        [JsonProperty("year_to")]
        public int? YearTo { get; set; }

        [JsonProperty("class")]
        public string VehicleClass { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class VehicleConfigurationDto
    {
        [JsonProperty("id")]
        public string ExternalId { get; set; }

        [JsonProperty("model_id")]
        public string ModelExternalId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("doors_count")]
        public int DoorsCount { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class VehicleGenerationDto
    {
        [JsonProperty("id")]
        public string ExternalId { get; set; }

        [JsonProperty("model_id")]
        public string ModelExternalId { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("year_from")]
        public int YearFrom { get; set; }

        [JsonProperty("year_to")]
        public int YearTo { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

    }
}