using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using GdsSharp.Lib.Library;

namespace Viewer;

public partial class MainWindow : Window
{
    private GdsLibrary? _library;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open GDS File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("GDS Files") { Patterns = new[] { "*.gds", "*.gds2", "*.gdsii" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
            }
        });

        if (files.Count > 0)
        {
            var file = files[0];
            await LoadGdsFile(file.Path.LocalPath);
        }
    }

    private async Task LoadGdsFile(string path)
    {
        try
        {
            // Load on background thread
            _library = await Task.Run(() => GdsLibrary.FromFile(path, opt => opt.BuildBoundingBoxes = true));

            // Update UI
            StructureComboBox.ItemsSource = GdsViewer.GetStructureNames().ToList();
            GdsViewer.LoadLibrary(_library);

            // Select the last structure (often the top-level)
            var names = StructureComboBox.ItemsSource as List<string>;
            if (names != null && names.Count > 0)
            {
                StructureComboBox.SelectedIndex = names.Count - 1;
            }

            Title = $"GDS Viewer - {System.IO.Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            // Show error dialog
            var dialog = new Window
            {
                Title = "Error",
                Content = new TextBlock { Text = $"Failed to load GDS file:\n{ex.Message}", Margin = new Avalonia.Thickness(20) },
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            await dialog.ShowDialog(this);
        }
    }

    private void OnStructureSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (StructureComboBox.SelectedItem is string structureName)
        {
            GdsViewer.SetStructure(structureName);
        }
    }

    private void OnFitToViewClick(object? sender, RoutedEventArgs e)
    {
        if (_library != null && StructureComboBox.SelectedItem is string structureName)
        {
            GdsViewer.SetStructure(structureName);
        }
    }
}