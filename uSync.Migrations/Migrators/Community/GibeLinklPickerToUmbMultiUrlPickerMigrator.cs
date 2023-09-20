using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator("Gibe.LinkPicker")]
public class GibeLinklPickerToUmbMultiUrlPickerMigrator : SyncPropertyMigratorBase
{
    private readonly JsonSerializerSettings _serializerSettings;

    public GibeLinklPickerToUmbMultiUrlPickerMigrator()
    {
        _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter> { new StringEnumConverter() },
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.MultiUrlPicker;

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => new MultiUrlPickerConfiguration { MaxNumber = 1 };

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value) == false)
        {
            var source = JsonConvert.DeserializeObject<GibeLinkPickerDto>(contentProperty.Value);
            if (source is not null)
            {
                var isInternal = source.Id > 0 || string.IsNullOrWhiteSpace(source.Udi) == false;

                var link = new Link
                {
                    Name = source.Name,
                    Target = source.Target,
                    Type = isInternal == true ? LinkType.Content : LinkType.External,
                    Udi = string.IsNullOrWhiteSpace(source.Udi) == false && UdiParser.TryParse(source.Udi, out var udi) == true ? udi : default,
                    Url = isInternal == false ? source.Url : null,
                };

                return JsonConvert.SerializeObject(link.AsEnumerableOfOne(), _serializerSettings);
            }
        }

        return default;
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    private class GibeLinkPickerDto
    {
        public int Id { get; set; }

        public string? Udi { get; set; }

        public string? Name { get; set; }

        public string? Url { get; set; }

        public string? Target { get; set; }

        public string? Hashtarget { get; set; }

        public string? Classname { get; set; }
    }
}
