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

            DataContext = viewModel;
        }
        private void SaveHandler(object? sender, EventArgs e)
        {
            DialogResult = true;
        }
    }
}
