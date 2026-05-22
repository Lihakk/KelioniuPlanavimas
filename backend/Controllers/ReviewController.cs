using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ReviewController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("KelionesRecommendations/1.0");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Review>>> requestTripElementReviews([FromQuery] string? type, [FromQuery] int? elementId)
    {
        var reviews = _context.Reviews.Include(item => item.Trip).AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
        {
            reviews = reviews.Where(item => item.TripElementType == type);
        }

        if (elementId.HasValue)
        {
            reviews = reviews.Where(item => item.TripElementId == elementId.Value);
        }

        return await sortReviews(reviews).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Review>> saveReviewData(Review review)
    {
        var trip = review.TripId.HasValue ? await _context.Trips.FindAsync(review.TripId.Value) : null;
        if (trip != null && !checkTripStatus(trip))
        {
            return BadRequest("Review can be saved only after completed trip.");
        }

        if (!validatePreferences(review))
        {
            return BadRequest("Review data is not valid.");
        }

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(requestTripElementReviews), new { elementId = review.TripElementId }, review);
    }

    [HttpGet("recommendations")]
    public async Task<ActionResult<IEnumerable<TripRecommendation>>> requestRecommendations(CancellationToken cancellationToken)
    {
        var trips = await _context.Trips.Include(item => item.Route).ThenInclude(route => route!.RoutePoints).ToListAsync(cancellationToken);
        var reviews = await _context.Reviews.ToListAsync(cancellationToken);
        return await generateRecommendations(trips, reviews, null, cancellationToken);
    }

    [HttpPost("recommendations")]
    public async Task<ActionResult<IEnumerable<TripRecommendation>>> ReevaluateRecommendations([FromBody] RecommendationPreferences preferences, CancellationToken cancellationToken)
    {
        if (!validatePreferences(preferences))
        {
            return BadRequest("Preference data is not valid.");
        }

        var trips = await _context.Trips.Include(item => item.Route).ThenInclude(route => route!.RoutePoints).ToListAsync(cancellationToken);
        var reviews = await _context.Reviews.ToListAsync(cancellationToken);
        return await ReevaluateRecommendations(trips, reviews, preferences, cancellationToken);
    }

    private IQueryable<Review> sortReviews(IQueryable<Review> reviews)
    {
        return reviews.OrderByDescending(item => item.Date).ThenByDescending(item => item.Rating);
    }

    private bool checkTripStatus(Trip? trip)
    {
        return trip?.TripStatus == TripStatus.Completed;
    }

    private bool validatePreferences(Review review)
    {
        return review.getReviewData();
    }

    private bool validatePreferences(RecommendationPreferences preferences)
    {
        return !string.IsNullOrWhiteSpace(preferences.TravelType) || preferences.Budget > 0;
    }

    private void analyzePreferences(RecommendationPreferences preferences)
    {
        preferences.TravelType = preferences.TravelType.Trim();
        preferences.WeatherPreference = preferences.WeatherPreference.Trim();
    }

    private void evaluateAlternatives(RecommendationPreferences preferences)
    {
        preferences.Budget = Math.Max(preferences.Budget, 0);
    }

    private async Task<List<TripRecommendation>> generateRecommendations(List<Trip> trips, List<Review> reviews, RecommendationPreferences? preferences, CancellationToken cancellationToken = default)
    {
        var result = new List<TripRecommendation>();
        foreach (var trip in trips)
        {
            result.Add(new TripRecommendation
            {
                TripId = trip.Id,
                Name = trip.Name,
                Score = await calculateScoreWeight(trip, reviews, preferences, cancellationToken),
                Reason = trip.Route == null ? "No route assigned" : $"Route: {trip.Route.getRoute()}"
            });
        }

        return result.OrderByDescending(item => item.Score).ToList();
    }

    private async Task<int> calculateScoreWeight(Trip trip, List<Review> reviews, RecommendationPreferences? preferences, CancellationToken cancellationToken = default)
    {
        return await calculateWeatherScore(trip, preferences, cancellationToken)
            + calculateBudgetScore(preferences)
            + calculateLocationScore(trip)
            + calculateRatingScore(trip, reviews)
            + calculateDateScore(trip);
    }

    private async Task<int> calculateWeatherScore(Trip trip, RecommendationPreferences? preferences, CancellationToken cancellationToken = default)
    {
        if (trip.Route == null || string.IsNullOrWhiteSpace(preferences?.WeatherPreference))
        {
            return 5;
        }

        var point = trip.Route.RoutePoints.FirstOrDefault();
        if (point == null || !double.TryParse(point.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) || !double.TryParse(point.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            return 5;
        }

        var weather = await requestWeatherData(latitude, longitude, trip.StartDate, trip.EndDate, cancellationToken);
        return weather.Description.Contains(preferences.WeatherPreference, StringComparison.OrdinalIgnoreCase) ? 20 : weather.PrecipitationMm < 10 ? 15 : 7;
    }

    private int calculateBudgetScore(RecommendationPreferences? preferences)
    {
        return preferences?.Budget > 0 ? 15 : 5;
    }

    private int calculateLocationScore(Trip trip)
    {
        return trip.RouteId.HasValue ? 20 : 5;
    }

    private int calculateRatingScore(Trip trip, List<Review> reviews)
    {
        var tripReviews = getReviewData(trip, reviews);
        if (tripReviews.Count == 0)
        {
            return 5;
        }

        return (int)Math.Round(tripReviews.Average(review => review.Rating) * 4);
    }

    private List<Review> getReviewData(Trip trip, List<Review> reviews)
    {
        return reviews.Where(review => review.TripId == trip.Id || review.TripElementId == trip.Id).ToList();
    }

    private List<Trip> getTripData()
    {
        return _context.Trips.Include(item => item.Route).ThenInclude(route => route!.RoutePoints).ToList();
    }

    private int calculateDateScore(Trip trip)
    {
        return trip.StartDate >= DateTime.UtcNow.Date ? 15 : 3;
    }

    private async Task<List<TripRecommendation>> ReevaluateRecommendations(List<Trip> trips, List<Review> reviews, RecommendationPreferences preferences, CancellationToken cancellationToken = default)
    {
        analyzePreferences(preferences);
        evaluateAlternatives(preferences);
        var recommendations = await generateRecommendations(trips, reviews, preferences, cancellationToken);
        return recommendations.Select(rec => evaluateRecommendation(rec, trips, preferences)).ToList();
    }

    private async Task<WeatherConditions> requestWeatherData(double latitude, double longitude, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var weatherBitKey = _configuration["ExternalApis:WeatherBitKey"];
        if (!string.IsNullOrWhiteSpace(weatherBitKey))
        {
            return await requestWeatherBitData(latitude, longitude, weatherBitKey, cancellationToken);
        }

        return await requestOpenMeteoData(latitude, longitude, startDate, endDate, cancellationToken);
    }

    private async Task<WeatherConditions> requestWeatherBitData(double latitude, double longitude, string key, CancellationToken cancellationToken)
    {
        var weatherBitUrl = _configuration["ExternalApis:WeatherBitUrl"] ?? "https://api.weatherbit.io/v2.0/forecast/daily";
        var url = $"{weatherBitUrl}?lat={latitude.ToString(CultureInfo.InvariantCulture)}&lon={longitude.ToString(CultureInfo.InvariantCulture)}&key={Uri.EscapeDataString(key)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var data = json.RootElement.GetProperty("data").EnumerateArray().ToList();

        return new WeatherConditions(
            data.Average(item => item.GetProperty("temp").GetDouble()),
            data.Sum(item => item.TryGetProperty("precip", out var precip) ? precip.GetDouble() : 0),
            data.FirstOrDefault().TryGetProperty("weather", out var weather) ? weather.GetProperty("description").GetString() ?? "WeatherBit forecast" : "WeatherBit forecast");
    }

    private async Task<WeatherConditions> requestOpenMeteoData(double latitude, double longitude, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var openMeteoUrl = _configuration["ExternalApis:OpenMeteoUrl"] ?? "https://api.open-meteo.com/v1/forecast";
        var url = $"{openMeteoUrl}?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}&daily=temperature_2m_max,temperature_2m_min,precipitation_sum&timezone=auto";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var daily = json.RootElement.GetProperty("daily");
        var max = daily.GetProperty("temperature_2m_max").EnumerateArray().Select(item => item.GetDouble()).ToList();
        var min = daily.GetProperty("temperature_2m_min").EnumerateArray().Select(item => item.GetDouble()).ToList();
        var precipitation = daily.GetProperty("precipitation_sum").EnumerateArray().Sum(item => item.GetDouble());

        return new WeatherConditions((max.Average() + min.Average()) / 2, precipitation, "Open-Meteo forecast");
    }

    // ============== PERSONALIZATION EVALUATION FUNCTIONS ==============

    private TripRecommendation evaluateRecommendation(TripRecommendation recommendation, List<Trip> trips, RecommendationPreferences preferences)
    {
        var trip = trips.FirstOrDefault(t => t.Id == recommendation.TripId);
        if (trip != null && trip.Price > 0)
        {
            var costInfo = PredictCost(trip);
            recommendation.Reason += $" | {costInfo}";
        }

        return recommendation;
    }

    private string PredictCost(Trip trip)
    {
        var travelMonth = trip.StartDate.Month;
        var seasonalFactor = calculateSeasonalFactor(travelMonth);
        var seasonalPrice = trip.Price * seasonalFactor;
        var priceRecommendation = predictPriceTrend(trip);
        
        return $"Current: ${seasonalPrice:F2} ({priceRecommendation})";
    }

    private decimal calculateSeasonalFactor(int month)
    {
        return month switch
        {
            12 or 1 or 7 or 8 => 1.3m,  // High season: holidays & summer
            6 or 9 => 1.15m,             // Shoulder: early summer, early fall
            2 or 3 or 4 or 5 => 0.85m,   // Low season: winter/spring
            10 or 11 => 0.9m,            // Shoulder: fall
            _ => 1m
        };
    }

    private string predictPriceTrend(Trip trip)
    {
        var tripReviews = _context.Reviews
            .Where(r => r.TripId == trip.Id)
            .OrderBy(r => r.Date)
            .ToList();

        if (tripReviews.Count < 2)
        {
            return "BUY_NOW";  // Not enough data
        }

        var recentReviews = tripReviews.TakeLast(3).ToList();
        var oldReviews = tripReviews.SkipLast(3).ToList();

        var recentAvgRating = recentReviews.Any() ? recentReviews.Average(r => r.Rating) : 0;
        var oldAvgRating = oldReviews.Any() ? oldReviews.Average(r => r.Rating) : 0;

        if (recentAvgRating > oldAvgRating + 0.5)
        {
            return "RISING";  // Ratings improving,
        }

        if (recentAvgRating < oldAvgRating - 0.5)
        {
            return "WAIT";    // Ratings declining,
        }

        return "BUY_NOW";     // Stable ratings,
    }
}

public class RecommendationPreferences
{
    public string TravelType { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public string WeatherPreference { get; set; } = string.Empty;
}

public class TripRecommendation
{
    public int TripId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public record WeatherConditions(double AverageTemperatureC, double PrecipitationMm, string Description, double UvIndex = 0);
