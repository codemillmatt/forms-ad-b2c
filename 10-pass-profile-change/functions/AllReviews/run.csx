#load ".\review.csx"
#r "Newtonsoft.Json"

using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;

public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("All reviews started.");

    var allReviews = new List<Review>();
    allReviews.Add(new Review { Text = "Good" });
    allReviews.Add(new Review { Text = "Bad" });

    var json = JsonConvert.SerializeObject(allReviews);

    var res = req.CreateResponse(HttpStatusCode.OK, json);

    return res;
}
