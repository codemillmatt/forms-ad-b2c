using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace TheReviewer.Core
{
    public partial class ReviewListPage : ContentPage
    {
        ReviewListViewModel vm;

        public ReviewListPage()
        {
            InitializeComponent();

            vm = new ReviewListViewModel();
            BindingContext = vm;

            //vm.SilentSignIn();
        }
    }
}
