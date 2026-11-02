using Microsoft.Win32;
using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for ManageFoldersWindow.xaml
    /// </summary>
    public partial class ManageFoldersWindow : Window
    {
        private MusicLibrary musicLibrary;
        private ObservableCollection<ScanFolder> scanFolders = new ObservableCollection<ScanFolder>();
        public ManageFoldersWindow(MusicLibrary appMusicLibrary)
        {
            InitializeComponent();

            musicLibrary = appMusicLibrary;

            foreach (ScanFolder scanFolder in musicLibrary.ScanFolders)
            {
                scanFolders.Add(scanFolder);
            }

            lbScanFolders.DataContext = scanFolders;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbScanFolders.SelectedItem is not ScanFolder requestFolder)
            {
                return;
            }

            scanFolders.Remove(requestFolder);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                List<string> errorList = musicLibrary.UpdateScanFolders(scanFolders);
                if (errorList.Count > 0)
                {
                    string allErrorMessage = string.Join("\n", errorList);
                    MessageBox.Show(
                        $"Some folders could not be updated:\n{allErrorMessage}",
                        "Partial Update Errors",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                        );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fail to save new scan folders list. Message: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.DialogResult = true;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Select Music Folder";
            bool? result = dialog.ShowDialog();
            if (result != true)
            {
                Debug.Print("User cancel choosing folder");
                return;
            }

            try
            {
                string chosenFolderPath = dialog.FolderName;
                switch (musicLibrary.ValidateNewFolderPath(chosenFolderPath, scanFolders, out string normalizePath))
                {
                    case MusicLibrary.AddFolderResult.Success:
                        scanFolders.Add(new ScanFolder(0, normalizePath, DateTime.Now));
                        break;
                    case MusicLibrary.AddFolderResult.Duplicate:
                        MessageBox.Show("This folder is already in your library or your pending queue.");
                        break;
                    case MusicLibrary.AddFolderResult.NotFound:
                        MessageBox.Show("Fail: This is not a valid folder.");
                        break;
                    case MusicLibrary.AddFolderResult.SubFolder:
                        MessageBox.Show("Fail: The parent folder of this folder is already in your library or your pending queue.");
                        break;
                    case MusicLibrary.AddFolderResult.ParentOfExisting:
                        MessageBox.Show("Fail: This folder contains another folder that is already in your library or your pending queue");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
