﻿using Newtonsoft.Json;

namespace CheckoutOrderService.Models
{
    /// <summary>
    /// Representation of a sku in the persistence layer
    /// </summary>
    public class SkuModel
    {
        public SkuModel(string id, string name)
        {
            Id = id;
            DisplayName = name;
        }

        public string Id { get; }
        [JsonProperty]
        public string DisplayName { get; private set; }
    }
}
