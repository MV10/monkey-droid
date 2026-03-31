using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace monkeydroid.Models;

public class AppData
{
    public string AutoSelectServer { get; set; } = "";
    public List<Server> Servers { get; set; } = new();
}

public class Server
{
    public string Name { get; set; } = "";
    public int Port { get; set; } = 50001;
    public int? AlternatePort { get; set; } = 50002;
    public List<VisualizerInfo> Visualizers { get; set; } = new();
    public List<FxInfo> Fx { get; set; } = new();
    public List<PlaylistInfo> Playlists { get; set; } = new();
    public DateTime? VisualizersTimestamp { get; set; }
    public DateTime? FxTimestamp { get; set; }
    public DateTime? PlaylistsTimestamp { get; set; }

    [JsonIgnore]
    public string DisplayName { get; set; } = "";

    [JsonIgnore]
    public string Subtitle { get; set; } = "";
}

public class VisualizerInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Audio { get; set; }

    [JsonIgnore]
    public string AudioIcon => Audio ? "\u266A" : "\u3030";
}

public class FxInfo
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Audio { get; set; }

    [JsonIgnore]
    public string AudioIcon => Audio ? "\u266A" : "\u3030";
}

public class PlaylistInfo
{
    public string Name { get; set; } = "";
}
