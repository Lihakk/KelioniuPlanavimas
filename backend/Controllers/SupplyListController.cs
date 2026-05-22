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
    public async Task<ActionResult<SupplyList>> getSupplyListByTrip(int tripId, CancellationToken cancellationToken)
    {
        var saved = await _context.SupplyLists
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.TripId == tripId, cancellationToken);

        if (saved == null)
        {
            return NotFound();
        }

        return saved;
    }

    [HttpPost("trip/{tripId}")]
    public async Task<ActionResult<SupplyList>> createSupplyListByConditions(
        int tripId,
        [FromBody] SupplyListRequest? request,
        CancellationToken cancellationToken)
    {
        request ??= new SupplyListRequest();
        var existing = await _context.SupplyLists
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.TripId == tripId, cancellationToken);

        if (existing != null)
        {
            return existing;
        }
        var trip = await _context.Trips
            .Include(item => item.Route)
            .ThenInclude(route => route!.RoutePoints)
            .FirstOrDefaultAsync(item => item.Id == tripId, cancellationToken);

        if (trip == null)
        {
            return NotFound();
        }

        analyzeTripParameters(trip);

        if (!validateTripData(trip))
        {
            return BadRequest("Trip data is not valid for supply list.");
        }

        var supplyList = await requestNewSupplyList(trip, request.HasLaundry, cancellationToken);

        _context.SupplyLists.Add(supplyList);
        await _context.SaveChangesAsync(cancellationToken);

        return supplyList;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplyList>> getSavedSupplyList(int id)
    {
        var supplyList = await _context.SupplyLists
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (supplyList == null)
        {
            return NotFound();
        }

        return supplyList;
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> updateSavedSupplyList(int id, [FromBody] SupplyListUpdateRequest request)
    {
        var supplyList = await _context.SupplyLists
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (supplyList == null)
        {
            return NotFound();
        }

        foreach (var updatedItem in request.Items)
        {
            var existingItem = supplyList.Items.FirstOrDefault(item => item.Id == updatedItem.Id);

            if (existingItem != null)
            {
                existingItem.Name = updatedItem.Name.Trim();
                existingItem.Type = updatedItem.Type.Trim();
                existingItem.Quantity = Math.Max(1, updatedItem.Quantity);
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Supply list saved.", supplyList.Id });
    }

    [HttpPost("{id}/resetCurrentSupplyList")]
    public async Task<ActionResult<SupplyList>> resetCurrentSupplyList(int id)
    {
        var supplyList = await _context.SupplyLists
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (supplyList == null)
        {
            return NotFound();
        }

        _context.Items.RemoveRange(supplyList.Items);
        resetCurrentSupplyList(supplyList);
        await _context.SaveChangesAsync();

        return supplyList;
    }
    [HttpPost("{id}/regenerate")]
    public async Task<ActionResult<SupplyList>> regenerateSupplyList(
        int id,
        [FromBody] SupplyListRequest? request,
        CancellationToken cancellationToken)
    {
        request ??= new SupplyListRequest();

        var supplyList = await _context.SupplyLists
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (supplyList == null)
        {
            return NotFound();
        }

        var trip = await _context.Trips
            .Include(item => item.Route)
            .ThenInclude(route => route!.RoutePoints)
            .FirstOrDefaultAsync(item => item.Id == supplyList.TripId, cancellationToken);

        if (trip == null)
        {
            return NotFound();
        }

        var conditions = await determineTripConditions(trip, cancellationToken);
        var newItems = createSupplyListByConditions(conditions, trip, request.HasLaundry);

        _context.Items.RemoveRange(supplyList.Items);

        supplyList.Items = newItems;

        updateSupplyListByRules(supplyList, trip, conditions);

        await _context.SaveChangesAsync(cancellationToken);

        supplyList.WeatherSummary = createWeatherSummary(conditions, request.HasLaundry);

        return supplyList;
    }
    private bool checkSavedSupplyList(SupplyList? supplyList)
    {
        return supplyList != null;
    }

    private async Task<SupplyList> requestNewSupplyList(
        Trip trip,
        bool hasLaundry,
        CancellationToken cancellationToken = default)
    {
        var conditions = await determineTripConditions(trip, cancellationToken);
        var items = createSupplyListByConditions(conditions, trip, hasLaundry);

        var supplyList = new SupplyList
        {
            TripId = trip.Id,
            WeatherSummary = createWeatherSummary(conditions, hasLaundry)
        };

        supplyList.saveGeneratedSupplyList(items);

        updateSupplyListByRules(supplyList, trip, conditions);

        return supplyList;
    }

    private async Task<TripConditions> determineTripConditions(Trip trip, CancellationToken cancellationToken = default)
    {
        var point = trip.Route?.RoutePoints.FirstOrDefault();

        if (point == null ||
            !double.TryParse(point.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
            !double.TryParse(point.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            return new TripConditions(trip.determineTripConditions(), 18, 0, "No weather coordinates", 0);
        }

        var weather = await requestWeatherData(latitude, longitude, trip.StartDate, trip.EndDate, cancellationToken);
        return determineTripConditions(trip.StartDate, trip.EndDate, weather);
    }

    private TripConditions determineTripConditions(DateTime startDate, DateTime endDate, WeatherConditions weather)
    {
        var days = Math.Max(1, (endDate.Date - startDate.Date).Days + 1);

        return new TripConditions(
            days,
            weather.AverageTemperatureC,
            weather.PrecipitationMm,
            weather.Description,
            weather.UvIndex
        );
    }

    private void analyzeTripParameters(Trip trip)
    {
        trip.Name = trip.Name.Trim();
    }

    private List<Item> createSupplyListByConditions(TripConditions conditions, Trip trip, bool hasLaundry)
    {
        var items = new List<Item>
        {
            createItem("Passport", "Document", 1, "Basic travel document."),
            createItem("Phone charger", "Electronics", 1, "Needed for phone charging during the trip."),
            createItem("Toiletries", "Health", 1, "Basic personal hygiene item."),

            createItem("Socks", "Clothing", calculateSocks(conditions, hasLaundry),
                createClothingReason(conditions, hasLaundry)),

            createItem("Underwear", "Clothing", calculateUnderwear(conditions, hasLaundry),
                createClothingReason(conditions, hasLaundry)),

            createItem("Shirts", "Clothing", calculateShirts(conditions, hasLaundry),
                createClothingReason(conditions, hasLaundry)),

            createItem("Pants", "Clothing", calculatePants(conditions, hasLaundry),
                "Calculated from trip duration. Usually fewer pants are needed than shirts."),

            createItem("Sleepwear", "Clothing", 1,
                "Added as a basic overnight clothing item.")
        };

        if (conditions.PrecipitationMm > 5)
        {
            items.Add(createItem(
                "Rain jacket",
                "Weather",
                1,
                $"Added because expected precipitation is {conditions.PrecipitationMm:F1} mm."
            ));
        }

        if (conditions.AverageTemperatureC < 10)
        {
            items.Add(createItem(
                "Warm jacket",
                "Clothing",
                1,
                $"Added because average temperature is {conditions.AverageTemperatureC:F1}°C."
            ));
        }

        if (conditions.AverageTemperatureC > 22)
        {
            items.Add(createItem(
                "Hat",
                "Health",
                1,
                $"Added because average temperature is {conditions.AverageTemperatureC:F1}°C."
            ));
        }

        if (conditions.UvIndex > 5)
        {
            items.Add(createItem(
                "Sunscreen",
                "Health",
                1,
                $"Added because UV index is {conditions.UvIndex:F1}."
            ));
        }

        var routeText = string.Join(" ", trip.Route?.RoutePoints.Select(point =>
            $"{point.Name} {point.City}") ?? []);

        var hasWaterPoi =
            routeText.Contains("lake", StringComparison.OrdinalIgnoreCase) ||
            routeText.Contains("sea", StringComparison.OrdinalIgnoreCase) ||
            routeText.Contains("beach", StringComparison.OrdinalIgnoreCase) ||
            routeText.Contains("river", StringComparison.OrdinalIgnoreCase) ||
            routeText.Contains("ežer", StringComparison.OrdinalIgnoreCase) ||
            routeText.Contains("jūr", StringComparison.OrdinalIgnoreCase) ||
            routeText.Contains("paplūdim", StringComparison.OrdinalIgnoreCase);

        var hasNaturePoi =
            routeText.Contains("park", StringComparison.OrdinalIgnoreCase) ||
            routeText.Contains("forest", StringComparison.OrdinalIgnoreCase) ||
            routeText.Contains("trail", StringComparison.OrdinalIgnoreCase) ||
            routeText.Contains("mišk", StringComparison.OrdinalIgnoreCase) ||
            routeText.Contains("gamt", StringComparison.OrdinalIgnoreCase) ||
            routeText.Contains("pažint", StringComparison.OrdinalIgnoreCase);

        if (hasWaterPoi)
        {
            items.Add(createItem(
                "Swimsuit",
                "Activity",
                1,
                "Added because the route includes a water-related place."
            ));

            items.Add(createItem(
                "Towel",
                "Activity",
                1,
                "Added because the route includes a water-related place."
            ));
        }

        if (hasNaturePoi)
        {
            items.Add(createItem(
                "Insect repellent",
                "Health",
                1,
                "Added because the route includes a nature-related place."
            ));
        }

        return items;
    }

    private int calculateClothingDays(int days, bool hasLaundry)
    {
        if (!hasLaundry)
        {
            return days;
        }

        return Math.Min(days, 4);
    }

    private int calculateSocks(TripConditions conditions, bool hasLaundry)
    {
        return calculateClothingDays(conditions.Days, hasLaundry);
    }

    private int calculateUnderwear(TripConditions conditions, bool hasLaundry)
    {
        return calculateClothingDays(conditions.Days, hasLaundry);
    }

    private int calculateShirts(TripConditions conditions, bool hasLaundry)
    {
        return Math.Max(1, calculateClothingDays(conditions.Days, hasLaundry));
    }

    private int calculatePants(TripConditions conditions, bool hasLaundry)
    {
        return Math.Max(1, (int)Math.Ceiling(calculateClothingDays(conditions.Days, hasLaundry) / 3.0));
    }

    private string createClothingReason(TripConditions conditions, bool hasLaundry)
    {
        if (hasLaundry)
        {
            return $"Calculated for {conditions.Days} trip days, but reduced because laundry is available.";
        }

        return $"Calculated for {conditions.Days} trip days without laundry.";
    }

    private string createWeatherSummary(TripConditions conditions, bool hasLaundry)
    {
        var laundryText = hasLaundry
            ? "Laundry is available, so clothing quantities are reduced."
            : "Laundry is not available, so clothing quantities are based on full trip duration.";

        return $"Forecast: {conditions.Description}. " +
               $"Average temperature: {conditions.AverageTemperatureC:F1}°C. " +
               $"Precipitation: {conditions.PrecipitationMm:F1} mm. " +
               $"UV index: {conditions.UvIndex:F1}. " +
               laundryText;
    }

    private void reduceQuantities(SupplyList supplyList)
    {
        foreach (var item in supplyList.Items.Where(item => item.Type == "Clothing"))
        {
            item.Quantity = Math.Max(1, item.Quantity - 1);
        }
    }

    private void updateSupplyListByRules(SupplyList supplyList, Trip trip, TripConditions conditions)
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

    private async Task<WeatherConditions> requestWeatherData(
        double latitude,
        double longitude,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var attemptCount = 0;

        while (attemptCount < 3)
        {
            try
            {
                var weatherBitKey = _configuration["ExternalApis:WeatherBitKey"];

                if (string.IsNullOrWhiteSpace(weatherBitKey))
                {
                    return new WeatherConditions(18, 0, "WeatherBit key is missing", 0);
                }

                var weather = await requestWeatherBitData(
                    latitude,
                    longitude,
                    weatherBitKey,
                    cancellationToken
                );

                weather = evaluateWeatherData(weather);

                if (validateWeatherData(weather))
                {
                    return weather;
                }
            }
            catch
            {
                // Retry until attemptCount reaches 3.
            }

            attemptCount++;
        }

        return new WeatherConditions(18, 0, "WeatherBit weather unavailable", 0);
    }

    private WeatherConditions evaluateWeatherData(WeatherConditions weather)
    {
        return weather with
        {
            Description = weather.Description.Trim()
        };
    }
    private bool validateWeatherData(WeatherConditions weather)
    {
        return weather.AverageTemperatureC > -80 &&
               weather.AverageTemperatureC < 70 &&
               weather.PrecipitationMm >= 0 &&
               weather.UvIndex >= 0;
    }

    private async Task<WeatherConditions> requestWeatherBitData(
        double latitude,
        double longitude,
        string key,
        CancellationToken cancellationToken)
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
            data.FirstOrDefault().TryGetProperty("weather", out var weather)
                ? weather.GetProperty("description").GetString() ?? "WeatherBit forecast"
                : "WeatherBit forecast",
            data.Average(item => item.TryGetProperty("uv", out var uv) ? uv.GetDouble() : 0)
        );
    }

    private static Item createItem(string name, string type, int quantity, string? reason = null)
    {
        return new Item
        {
            Name = name,
            Type = type,
            Quantity = quantity,
            Reason = reason
        };
    }

    private static void adjustFinalQuantities(SupplyList supplyList, TripConditions conditions)
    {
        foreach (var item in supplyList.Items.Where(item => item.Type == "Clothing"))
        {
            item.Quantity = Math.Max(1, item.Quantity);
        }
    }
}

public class SupplyListRequest
{
    public bool HasLaundry { get; set; }
}

public class SupplyListUpdateRequest
{
    public List<SupplyListItemUpdateRequest> Items { get; set; } = new();
}

public class SupplyListItemUpdateRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public int Quantity { get; set; }
}

public record TripConditions(
    int Days,
    double AverageTemperatureC,
    double PrecipitationMm,
    string Description,
    double UvIndex
);