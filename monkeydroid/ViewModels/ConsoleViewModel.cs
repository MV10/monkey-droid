using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using monkeydroid.Services;

namespace monkeydroid.ViewModels;

public partial class ConsoleViewModel : ViewModelBase
{
    public string Title => "Console";

    [ObservableProperty] private string _inputText = "";

    public ObservableCollection<string> OutputLines { get; } = new();

    private readonly List<string> _history = new();
    private int _historyIndex = -1;
    private const int MaxHistory = 20;
    private const int MaxOutputLines = 999;

    [RelayCommand]
    private async Task Send()
    {
        var input = InputText.Trim();
        if (string.IsNullOrEmpty(input)) return;

        AddToHistory(input);
        AddOutputLine($"> {input}");
        InputText = "";

        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0 && !parts[0].StartsWith("--"))
            parts[0] = "--" + parts[0];

        var server = DataStore.Instance.GetSelectedServer();
        if (server is null)
        {
            AddOutputLine("No server selected.");
            return;
        }

        if (await CommsService.SendCommand(server, parts))
        {
            var response = CommsService.GetResponse();
            if (!string.IsNullOrEmpty(response))
                AddOutputLine(response);
        }
    }

    [RelayCommand]
    private void HistoryUp()
    {
        if (_history.Count == 0) return;
        if (_historyIndex < _history.Count - 1)
            _historyIndex++;
        InputText = _history[_history.Count - 1 - _historyIndex];
    }

    [RelayCommand]
    private void HistoryDown()
    {
        if (_historyIndex <= 0)
        {
            _historyIndex = -1;
            InputText = "";
            return;
        }
        _historyIndex--;
        InputText = _history[_history.Count - 1 - _historyIndex];
    }

    private void AddToHistory(string command)
    {
        _history.Add(command);
        if (_history.Count > MaxHistory)
            _history.RemoveAt(0);
        _historyIndex = -1;
    }

    private void AddOutputLine(string line)
    {
        OutputLines.Add(line);
        while (OutputLines.Count > MaxOutputLines)
            OutputLines.RemoveAt(0);
    }
}
