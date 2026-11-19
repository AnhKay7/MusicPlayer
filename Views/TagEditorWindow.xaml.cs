using Microsoft.Win32;
using MusicPlayer.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MusicPlayer.Views
{
    public partial class TagEditorWindow : Window
    {
        private TagEditorViewModel tagEditorViewModel;
        public TagEditorWindow(TagEditorViewModel viewModel)
        {
            InitializeComponent();

            viewModel.SaveCompleted += SaveHandler;

            viewModel.SaveFailed += SaveFailedHandler;

            tagEditorViewModel = viewModel;

            DataContext = viewModel;
        }
        private void SaveHandler(object? sender, EventArgs e)
        {
            DialogResult = true;
        }

        private void SaveFailedHandler(object? sender, string message)
        {
            MessageBox.Show(message, "Tag Editor Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnChangeCover_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select New Cover Art";
            dialog.Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
            dialog.Multiselect = false;
            bool? result = dialog.ShowDialog();
            if (result != true)
            {
                Debug.Print("User cancel choosing new cover art");
                return;
            }
            string filePath = dialog.FileName;

            try
            {
                tagEditorViewModel.CoverData = File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Changing new cover art error, message: {ex.Message}", "Tag Editor Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
