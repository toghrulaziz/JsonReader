using JsonReader.ViewModels;
using JsonReader.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace JsonReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public new MainView? MainWindow { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow = new MainView
            {
                DataContext = new MainViewModel()
            };

            MainWindow.Show();

        }

    }
}
