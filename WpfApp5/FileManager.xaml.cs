using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp5
{
    /// <summary>
    /// Логика взаимодействия для FileManager.xaml
    /// </summary>
    public partial class FileManager : Window
    {
        private readonly UserViewModel userViewModel = new();
        public FileManager()
        {
            InitializeComponent();
            DataContext = userViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var thread = new Thread(WriteTo);
            thread.Start();
        }

        public void WriteTo()
        {
            using var fileStream = new FileStream(@"C:\test.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            for (int i = 0; i < 1_000_000_000; i++)
            {
                byte[] buffer = Encoding.Default.GetBytes(userViewModel.ToString());
                fileStream.Write(buffer, 0, buffer.Length);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();

            Close();
        }
    }
}