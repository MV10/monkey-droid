using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using monkeydroid.Models;

namespace monkeydroid.Services;

public class DataStore
{
    public static DataStore Instance { get; } = new();

    public AppData Data { get; private set; } = new();
    public string? SelectedServerName { get; set; }
    public bool LoadFailed { get; private set; }

    private static readonly string DataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "monkeydroid");

    private static readonly string DataFile = Path.Combine(DataDir, "data.json");

    private DataStore() { }

    public void Load()
    {
        LoadFailed = false;

        if (!File.Exists(DataFile))
        {
            Data = new AppData();
            return;
        }

        var json = File.ReadAllText(DataFile);
        if (string.IsNullOrWhiteSpace(json))
        {
            Data = new AppData();
            return;
        }

        try
        {
            Data = JsonSerializer.Deserialize(json, AppDataJsonContext.Default.AppData) ?? new AppData();
        }
        catch
        {
            Data = new AppData();
            File.WriteAllText(DataFile, "");
            LoadFailed = true;
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(DataDir);
        Data.Servers = Data.Servers.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase).ToList();
        var json = JsonSerializer.Serialize(Data, AppDataJsonContext.Default.AppData);
        File.WriteAllText(DataFile, json);
    }

    public Server? GetSelectedServer()
    {
        if (SelectedServerName is null) return null;
        return Data.Servers.FirstOrDefault(s =>
            s.Name.Equals(SelectedServerName, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasServers => Data.Servers.Count > 0;

    public bool FileExists => File.Exists(DataFile);

    public void Reset()
    {
        Data = new AppData();
        SelectedServerName = null;
        if (File.Exists(DataFile))
            File.WriteAllText(DataFile, "");
    }
}
