using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Payloads;

internal class PayloadJsonConverter : JsonConverter<IPayload>
{
    public override IPayload? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("JSON reading started not from the very beginning.");
        }

        Utf8JsonReader copyReader = reader;
        IPayload payload = null;
        while (reader.Read())
        {
            
        }

        return payload;
    }

    public override void Write(Utf8JsonWriter writer, IPayload value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
