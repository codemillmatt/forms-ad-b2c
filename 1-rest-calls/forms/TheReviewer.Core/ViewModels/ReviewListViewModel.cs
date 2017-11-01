using System;
using System.Net.Http;
using System.Collections.Generic;

using Xamarin.Forms;

using MvvmHelpers;
using Newtonsoft.Json;

namespace TheReviewer.Core
{
    public class ReviewListViewModel : BaseViewModel
    {
        public ReviewListViewModel()
        {
            Title = "All Reviews";
        }

        public ObservableRangeCollection<Review> AllReviews { get; set; } = new ObservableRangeCollection<Review>();

        Command _refreshCommand;
        public Command RefreshCommand => _refreshCommand ??
        (_refreshCommand = new Command(async () =>
        {
            var baseAddr = new Uri("http://localhost:5000");
            var client = new HttpClient { BaseAddress = baseAddr };

            var reviews = await client.GetStringAsync("/api/values");

            var allReviews = JsonConvert.DeserializeObject<List<Review>>(reviews);

            AllReviews.AddRange(allReviews);
        }));
    }
}
