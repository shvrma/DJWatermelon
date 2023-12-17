﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public record RESTResponceModel
{
    [JsonExtensionData]
    protected JsonObject? OtherProperties { get; init; }

    public ErrorModel? Error 
        => JsonSerializer.Deserialize(
            node: OtherProperties, 
            jsonTypeInfo: LavalinkModelsSourceGenerationContext.Default.ErrorModel);
}