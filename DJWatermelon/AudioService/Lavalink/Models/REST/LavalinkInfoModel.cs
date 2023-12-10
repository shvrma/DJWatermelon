using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink.Models.REST;

public sealed record LavalinkInfoModel(
    [property: JsonRequired]
    [property: JsonPropertyName("version")]
    LavalinkVersionModel Version,

    [property: JsonRequired]
    [property: JsonPropertyName("buildTime")]
    int BuildTime,

    [property: JsonRequired]
    [property: JsonPropertyName("git")]
    LavalinkGitBuildInfoModel GitBuildInfo,

    [property: JsonRequired]
    [property: JsonPropertyName("jvm")]
    string JVMVersion,

    [property: JsonRequired]
    [property: JsonPropertyName("lavaplayer")]
    string LavaplayerVersion,

    [property: JsonRequired]
    [property: JsonPropertyName("sourceManagers")]
    IEnumerable<string> SourceManagers,

    [property: JsonRequired]
    [property: JsonPropertyName("filters")]
    IEnumerable<string> EnabledFilters,

    [property: JsonRequired]
    [property: JsonPropertyName("plugins")]
    IEnumerable<LavalinkPluginModel> EnabledPlugins);

public record LavalinkVersionModel(
    [property: JsonRequired]
    [property: JsonPropertyName("semver")]
    string SemanticVersion,

    [property: JsonRequired]
    [property: JsonPropertyName("major")]
    int Major,

    [property: JsonRequired]
    [property: JsonPropertyName("minor")]
    int Minor,

    [property: JsonRequired]
    [property: JsonPropertyName("patch")]
    int Patch,

    [property: JsonRequired]
    [property: JsonPropertyName("preRelease")]
    string? PreReleaseVersion,

    [property: JsonRequired]
    [property: JsonPropertyName("build")]
    string? BuildMetadata);

public record LavalinkGitBuildInfoModel(
    [property: JsonRequired]
    [property: JsonPropertyName("branch")]
    string Branch,

    [property: JsonRequired]
    [property: JsonPropertyName("commit")]
    string Commit,

    [property: JsonRequired]
    [property: JsonPropertyName("commitTime")]
    ulong CommitTime);

public record LavalinkPluginModel(
    [property: JsonRequired]
    [property: JsonPropertyName("name")]
    string Name,

    [property: JsonRequired]
    [property: JsonPropertyName("version")]
    string Version);
