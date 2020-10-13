using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace EDennis.Samples.ColorApp.Blazor {
    public class WeatherApiClient {
        private readonly HttpClient _client;
        public WeatherApiClient(HttpClient client) {
            _client = client;
        }
        public async Task<Pages.FetchData.WeatherForecast[]> GetForecasts() {
            var forecasts = new Pages.FetchData.WeatherForecast[0];
            try {
                forecasts= await _client.GetFromJsonAsync<Pages.FetchData.WeatherForecast[]>("sample-data/weather.json");
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return forecasts;
        }
    }
}
