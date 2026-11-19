using MusicPlayer.ViewModels;
using System.Windows;

namespace MusicPlayer.Views
{
    public partial class TagEditorWindow : Window
    {
        public TagEditorWindow(TagEditorViewModel viewModel)
        {
            InitializeComponent();

            viewModel.SaveCompleted += SaveHandler;

            viewModel.SaveFailed += SaveFailedHandler;

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
    }
}
