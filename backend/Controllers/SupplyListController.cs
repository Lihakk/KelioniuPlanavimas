using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

[ApiController]
[Route("api/[controller]")]
public class SupplyListController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SupplyListController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("KelionesSupplyList/1.0");
    }

    [HttpGet("trip/{tripId}")]
    public async Task<ActionResult<SupplyList>> requestNewSupplyList(int tripId, CancellationToken cancellationToken)
    {
        var saved = await _context.SupplyLists.Include(item => item.Items).FirstOrDefaultAsync(item => item.TripId == tripId, cancellationToken);
        if (checkSavedSupplyList(saved))
        {
            return saved!;
        }

        return await createSupplyListByConditions(tripId, cancellationToken);
    }

    [HttpPost("trip/{tripId}")]
    public async Task<ActionResult<SupplyList>> createSupplyListByConditions(int tripId, CancellationToken cancellationToken)
    {
        var trip = await _context.Trips.Include(item => item.Route).ThenInclude(route => route!.RoutePoints).FirstOrDefaultAsync(item => item.Id == tripId, cancellationToken);
        if (trip == null)
        {
            return NotFound();
        }

        analyzeTripParameters(trip);
        if (!validateTripData(trip))
        {
            return BadRequest("Trip data is not valid for supply list.");
        }

        var supplyList = await requestNewSupplyList(trip, cancellationToken);
        _context.SupplyLists.Add(supplyList);
        await _context.SaveChangesAsync(cancellationToken);

        return supplyList;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplyList>> getSavedSupplyList(int id)
    {
        var supplyList = await _context.SupplyLists.Include(item => item.Items).FirstOrDefaultAsync(item => item.Id == id);
        if (supplyList == null)
        {
            return NotFound();
        }

        return supplyList;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> updateSupplyList(int id, SupplyList supplyList)
    {
        if (id != supplyList.Id)
        {
            return BadRequest();
        }

        var trip = await _context.Trips.FindAsync(supplyList.TripId);
        if (trip == null)
        {
            return NotFound();
        }

        var conditions = await determineTripConditions(trip);
        updateSupplyList(supplyList, trip, conditions);
        _context.Entry(supplyList).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(new { message = "display updated list", supplyList.Id });
    }

    [HttpPost("{id}/resetCurrentSupplyList")]
    public async Task<ActionResult<SupplyList>> resetCurrentSupplyList(int id)
    {
        var supplyList = await _context.SupplyLists.Include(item => item.Items).FirstOrDefaultAsync(item => item.Id == id);
        if (supplyList == null)
        {
            return NotFound();
        }

        _context.Items.RemoveRange(supplyList.Items);
        resetCurrentSupplyList(supplyList);
        await _context.SaveChangesAsync();

        return supplyList;
    }

    private bool checkSavedSupplyList(SupplyList? supplyList)
    {
        return supplyList != null;
    }

    private async Task<SupplyList> requestNewSupplyList(Trip trip, CancellationToken cancellationToken = default)
    {
        var conditions = await determineTripConditions(trip, cancellationToken);
        var items = createSupplyListByConditions(conditions);
        var supplyList = new SupplyList { TripId = trip.Id };
        supplyList.saveGeneratedSupplyList(items);
        updateSupplyList(supplyList, trip, conditions);
        return supplyList;
    }

    private async Task<TripConditions> determineTripConditions(Trip trip, CancellationToken cancellationToken = default)
    {
        var point = trip.Route?.RoutePoints.FirstOrDefault();
        if (point == null || !double.TryParse(point.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) || !double.TryParse(point.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            return new TripConditions(trip.determineTripConditions(), 18, 0, "No weather coordinates");
        }

        var weather = await requestWeatherData(latitude, longitude, trip.StartDate, trip.EndDate, cancellationToken);
        return determineTripConditions(trip.StartDate, trip.EndDate, weather);
    }

    private TripConditions determineTripConditions(DateTime startDate, DateTime endDate, WeatherConditions weather)
    {
        var days = Math.Max(1, (endDate.Date - startDate.Date).Days + 1);
        return new TripConditions(days, weather.AverageTemperatureC, weather.PrecipitationMm, weather.Description);
    }

    private void analyzeTripParameters(Trip trip)
    {
        trip.Name = trip.Name.Trim();
    }

    private List<Item> createSupplyListByConditions(TripConditions conditions)
    {
        var items = new List<Item>
        {
            createItem("Passport", "Document", 1),
            createItem("Clothes", "Clothing", calculateBaseClothingQuantities(conditions)),
            createItem("Phone charger", "Electronics", 1),
            createItem("Toiletries", "Health", 1)
        };

        if (conditions.PrecipitationMm > 5)
        {
            items.Add(createItem("Rain jacket", "Weather", 1));
        }

        if (conditions.AverageTemperatureC < 10)
        {
            items.Add(createItem("Warm jacket", "Clothing", 1));
        }

        if (conditions.AverageTemperatureC > 22)
        {
            items.Add(createItem("Sunscreen", "Health", 1));
        }

        return items;
    }

    private int calculateRecommendedQuantities(TripConditions conditions)
    {
        return Math.Max(1, conditions.Days);
    }

    private int calculateBaseClothingQuantities(TripConditions conditions)
    {
        return calculateRecommendedQuantities(conditions);
    }

    private void reduceQuantities(SupplyList supplyList)
    {
        foreach (var item in supplyList.Items.Where(item => item.Type == "Clothing"))
        {
            item.Quantity = Math.Max(1, item.Quantity - 1);
        }
    }

    private void updateSupplyList(SupplyList supplyList, Trip trip, TripConditions conditions)
    {
        validateTripData(trip);
        setItemSelectRules(supplyList);
        adjustFinalQuantities(supplyList, conditions);
    }

    private bool validateTripData(Trip trip)
    {
        return trip.checkTrip();
    }

    private void setItemSelectRules(SupplyList supplyList)
    {
        foreach (var item in supplyList.Items)
        {
            item.Quantity = Math.Max(1, item.Quantity);
        }
    }

    private void resetCurrentSupplyList(SupplyList supplyList)
    {
        supplyList.Items.Clear();
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

    private static Item createItem(string name, string type, int quantity)
    {
        return new Item { Name = name, Type = type, Quantity = quantity };
    }

    private static void adjustFinalQuantities(SupplyList supplyList, TripConditions conditions)
    {
        foreach (var item in supplyList.Items.Where(item => item.Type == "Clothing"))
        {
            item.Quantity = Math.Max(item.Quantity, conditions.Days);
        }
    }
}

public record TripConditions(int Days, double AverageTemperatureC, double PrecipitationMm, string Description);
