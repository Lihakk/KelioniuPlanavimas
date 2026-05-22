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
    public async Task<ActionResult<SupplyList>> checkSavedSupplyList(int tripId, CancellationToken cancellationToken)
    {
        var savedSupplyList = await getSavedSupplyList(tripId, cancellationToken);

        if (!checkSavedSupplyList(savedSupplyList))
        {
            return NotFound();
        }

        return savedSupplyList!;
    }

    [HttpPost("trip/{tripId}")]
    public async Task<ActionResult<SupplyList>> createSupplyList(
        int tripId,
        [FromBody] SupplyListRequest? request,
        CancellationToken cancellationToken)
    {
        request ??= new SupplyListRequest();

        var savedSupplyList = await getSavedSupplyList(tripId, cancellationToken);

        if (checkSavedSupplyList(savedSupplyList))
        {
            return savedSupplyList!;
        }

        var trip = await getTripData(tripId, cancellationToken);

        if (trip == null)
        {
            return NotFound();
        }

        if (!validateTripData(trip))
        {
            return BadRequest("Trip data is not valid for supply list.");
        }

        var tripConditions = await determineTripConditions(trip, cancellationToken);

        analyzeTripParameters(tripConditions);

        var supplyList = generateSupplyList(trip, tripConditions, request.HasLaundry);

        await saveNewSupplyList(supplyList, cancellationToken);

        return supplyList;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplyList>> getSupplyList(int id, CancellationToken cancellationToken)
    {
        var supplyList = await _context.SupplyLists
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (supplyList == null)
        {
            return NotFound();
        }

        return supplyList;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> updateSupplyList(int id, [FromBody] SupplyListUpdateRequest request)
    {
        var supplyList = await _context.SupplyLists
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (supplyList == null)
        {
            return NotFound();
        }

        updateItems(supplyList, request.Items);

        await saveSupplyList(supplyList);

        return Ok(new { message = "Updated list saved.", supplyList.Id });
    }

    [HttpPost("{id}/resetCurrentSupplyList")]
    public async Task<ActionResult<SupplyList>> resetCurrentSupplyList(int id, CancellationToken cancellationToken)
    {
        var supplyList = await _context.SupplyLists
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (supplyList == null)
        {
            return NotFound();
        }

        _context.Items.RemoveRange(supplyList.Items);
        resetCurrentSupplyList(supplyList);

        await _context.SaveChangesAsync(cancellationToken);

        return supplyList;
    }

    [HttpPost("{id}/regenerate")]
    public async Task<ActionResult<SupplyList>> createNewSupplyList(
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

        var trip = await getTripData(supplyList.TripId, cancellationToken);

        if (trip == null)
        {
            return NotFound();
        }

        var tripConditions = await determineTripConditions(trip, cancellationToken);

        analyzeTripParameters(tripConditions);

        _context.Items.RemoveRange(supplyList.Items);
        resetCurrentSupplyList(supplyList);

        var newSupplyList = generateSupplyList(trip, tripConditions, request.HasLaundry);

        supplyList.Items = newSupplyList.Items;
        supplyList.WeatherSummary = newSupplyList.WeatherSummary;

        await _context.SaveChangesAsync(cancellationToken);

        return supplyList;
    }

    private async Task<SupplyList?> getSavedSupplyList(int tripId, CancellationToken cancellationToken)
    {
        return await _context.SupplyLists
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.TripId == tripId, cancellationToken);
    }

    private bool checkSavedSupplyList(SupplyList? savedSupplyList)
    {
        return savedSupplyList != null;
    }

    private async Task<Trip?> getTripData(int tripId, CancellationToken cancellationToken)
    {
        return await _context.Trips
            .Include(item => item.Route)
            .ThenInclude(route => route!.RoutePoints)
            .FirstOrDefaultAsync(item => item.Id == tripId, cancellationToken);
    }

    private bool validateTripData(Trip trip)
    {
        return trip.checkTrip();
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

        var weatherData = await requestWeatherData(
            latitude,
            longitude,
            trip.StartDate,
            trip.EndDate,
            cancellationToken
        );

        var tripDays = Math.Max(1, (trip.EndDate.Date - trip.StartDate.Date).Days + 1);

        return new TripConditions(
            tripDays,
            weatherData.AverageTemperatureC,
            weatherData.PrecipitationMm,
            weatherData.Description,
            weatherData.UvIndex
        );
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
                    return new WeatherConditions(18, 0, "Weather data unavailable", 0);
                }

                var weatherData = await requestWeatherBitData(
                    latitude,
                    longitude,
                    weatherBitKey,
                    cancellationToken
                );

                weatherData = evaluateWeatherData(weatherData);

                if (validateWeatherData(weatherData))
                {
                    return weatherData;
                }
            }
            catch
            {
                // Retry until attemptCount reaches 3.
            }

            attemptCount++;
        }

        return new WeatherConditions(18, 0, "Weather data unavailable", 0);
    }

    private WeatherConditions evaluateWeatherData(WeatherConditions weatherData)
    {
        return weatherData with
        {
            Description = weatherData.Description.Trim()
        };
    }

    private bool validateWeatherData(WeatherConditions weatherData)
    {
        return weatherData.AverageTemperatureC > -80 &&
               weatherData.AverageTemperatureC < 70 &&
               weatherData.PrecipitationMm >= 0 &&
               weatherData.UvIndex >= 0;
    }

    private void analyzeTripParameters(TripConditions tripConditions)
    {
        _ = tripConditions.Days;
        _ = tripConditions.AverageTemperatureC;
        _ = tripConditions.PrecipitationMm;
        _ = tripConditions.UvIndex;
    }

    private SupplyList generateSupplyList(Trip trip, TripConditions tripConditions, bool hasLaundry)
    {
        var supplyList = new SupplyList
        {
            TripId = trip.Id,
            WeatherSummary = createWeatherSummary(tripConditions, hasLaundry)
        };

        setItemSelectRules(supplyList, trip, tripConditions, hasLaundry);
        adjustFinalQuantities(supplyList, tripConditions);

        return supplyList;
    }

    private void setItemSelectRules(SupplyList supplyList, Trip trip, TripConditions tripConditions, bool hasLaundry)
    {
        var items = new List<Item>
        {
            createItem("Passport", "Document", 1, "Basic travel document."),
            createItem("Phone charger", "Electronics", 1, "Needed for phone charging during the trip."),
            createItem("Toiletries", "Health", 1, "Basic personal hygiene item.")
        };

        var baseQuantities = calculateBaseClothingQuantities(tripConditions);

        if (hasLaundry)
        {
            baseQuantities = reduceQuantities(baseQuantities);
        }

        items.Add(createItem("Socks", "Clothing", baseQuantities, createClothingReason(tripConditions, hasLaundry)));
        items.Add(createItem("Underwear", "Clothing", baseQuantities, createClothingReason(tripConditions, hasLaundry)));
        items.Add(createItem("Shirts", "Clothing", baseQuantities, createClothingReason(tripConditions, hasLaundry)));

        items.Add(createItem(
            "Pants",
            "Clothing",
            Math.Max(1, (int)Math.Ceiling(baseQuantities / 3.0)),
            "Calculated from trip duration. Usually fewer pants are needed than shirts."
        ));

        items.Add(createItem(
            "Sleepwear",
            "Clothing",
            1,
            "Added as a basic overnight clothing item."
        ));

        addConditionalItems(items, tripConditions, trip);

        supplyList.saveGeneratedSupplyList(items);
    }

    private int calculateBaseClothingQuantities(TripConditions tripConditions)
    {
        return Math.Max(1, tripConditions.Days);
    }

    private int reduceQuantities(int baseQuantities)
    {
        return Math.Max(1, Math.Min(baseQuantities, 4));
    }

    private void addConditionalItems(List<Item> items, TripConditions tripConditions, Trip trip)
    {
        if (tripConditions.PrecipitationMm > 5)
        {
            items.Add(createItem(
                "Rain jacket",
                "Weather",
                1,
                $"Added because expected precipitation is {tripConditions.PrecipitationMm:F1} mm."
            ));
        }

        if (tripConditions.AverageTemperatureC < 10)
        {
            items.Add(createItem(
                "Warm jacket",
                "Clothing",
                1,
                $"Added because average temperature is {tripConditions.AverageTemperatureC:F1}°C."
            ));
        }

        if (tripConditions.AverageTemperatureC > 22)
        {
            items.Add(createItem(
                "Hat",
                "Health",
                1,
                $"Added because average temperature is {tripConditions.AverageTemperatureC:F1}°C."
            ));
        }

        if (tripConditions.UvIndex > 5)
        {
            items.Add(createItem(
                "Sunscreen",
                "Health",
                1,
                $"Added because UV index is {tripConditions.UvIndex:F1}."
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
    }

    private static void adjustFinalQuantities(SupplyList supplyList, TripConditions tripConditions)
    {
        foreach (var item in supplyList.Items.Where(item => item.Type == "Clothing"))
        {
            item.Quantity = Math.Max(1, item.Quantity);
        }

        if (tripConditions.AverageTemperatureC < 5)
        {
            var warmItem = supplyList.Items.FirstOrDefault(item => item.Name == "Warm jacket");

            if (warmItem != null)
            {
                warmItem.Quantity = Math.Max(1, warmItem.Quantity);
            }
        }
    }

    private void updateItems(SupplyList supplyList, List<SupplyListItemUpdateRequest> updatedItems)
    {
        foreach (var updatedItem in updatedItems)
        {
            var existingItem = supplyList.Items.FirstOrDefault(item => item.Id == updatedItem.Id);

            if (existingItem != null)
            {
                existingItem.Name = updatedItem.Name.Trim();
                existingItem.Type = updatedItem.Type.Trim();
                existingItem.Quantity = Math.Max(1, updatedItem.Quantity);
            }
        }
    }

    private async Task saveSupplyList(SupplyList supplyList)
    {
        _context.SupplyLists.Update(supplyList);
        await _context.SaveChangesAsync();
    }

    private async Task saveNewSupplyList(SupplyList supplyList, CancellationToken cancellationToken)
    {
        _context.SupplyLists.Add(supplyList);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private void resetCurrentSupplyList(SupplyList supplyList)
    {
        supplyList.Items.Clear();
        supplyList.WeatherSummary = null;
    }

    private async Task<WeatherConditions> requestWeatherBitData(
        double latitude,
        double longitude,
        string key,
        CancellationToken cancellationToken)
    {
        var weatherBitUrl = _configuration["ExternalApis:WeatherBitUrl"] ?? "https://api.weatherbit.io/v2.0/forecast/daily";

        var url =
            $"{weatherBitUrl}?lat={latitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&lon={longitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&key={Uri.EscapeDataString(key)}";

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

    private string createWeatherSummary(TripConditions tripConditions, bool hasLaundry)
    {
        var laundryText = hasLaundry
            ? "Laundry is available, so clothing quantities are reduced."
            : "Laundry is not available, so clothing quantities are based on full trip duration.";

        return $"Forecast: {tripConditions.Description}. " +
               $"Average temperature: {tripConditions.AverageTemperatureC:F1}°C. " +
               $"Precipitation: {tripConditions.PrecipitationMm:F1} mm. " +
               $"UV index: {tripConditions.UvIndex:F1}. " +
               laundryText;
    }

    private string createClothingReason(TripConditions tripConditions, bool hasLaundry)
    {
        if (hasLaundry)
        {
            return $"Calculated for {tripConditions.Days} trip days, but reduced because laundry is available.";
        }

        return $"Calculated for {tripConditions.Days} trip days without laundry.";
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