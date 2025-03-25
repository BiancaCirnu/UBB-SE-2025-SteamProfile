using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SteamProfile.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace SteamProfile.Views
{
    public sealed partial class CollectionsPage : Page
    {
        private readonly CollectionsViewModel _viewModel;

        public CollectionsPage()
        {
            this.InitializeComponent();
            _viewModel = new CollectionsViewModel(App.CollectionsService);
            this.DataContext = _viewModel;
        }
    }
}
