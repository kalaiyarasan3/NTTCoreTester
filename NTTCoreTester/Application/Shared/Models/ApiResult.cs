using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Shared.Models
{
    public sealed class ApiResult
    {
        public int StatusCode { get; init; }
        public bool IsSuccess => StatusCode == 0;

        public JsonElement Root { get; init; }
        public JsonElement? ResponseData { get; init; }

        public string RawJson { get; init; } = string.Empty;

        public string? Message =>
            Root.TryGetProperty("Message", out var m)
                ? m.GetString()
                : null;
    }

}
