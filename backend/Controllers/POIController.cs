using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

[ApiController]
[Route("api/[controller]")]
public class POIController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public POIController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("KelionesPOI/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(2);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PointOfInterest>>> getPOI()
    {
        return await getAllPOI();
    }

    [HttpGet("getAllPOI")]
    public async Task<ActionResult<IEnumerable<PointOfInterest>>> getAllPOI()
    {
        return await _context.PointsOfInterest.ToListAsync();
    }

    [HttpGet("showPOIList")]
    public async Task<ActionResult<IEnumerable<PointOfInterest>>> showPOIList()
    {
        return await getAllPOI();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PointOfInterest>> getSpecificPOI(int id)
    {
        var poi = await _context.PointsOfInterest.FindAsync(id);
        if (poi == null) return NotFound();
        return poi;
    }

    [HttpPost]
    public async Task<ActionResult<PointOfInterest>> createNewPOI(PointOfInterest poi)
    {
        if (!checkPOI(poi))
            return BadRequest("POI data is not valid.");

        _context.PointsOfInterest.Add(poi);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(getSpecificPOI), new { id = poi.Id }, poi);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> editPOIData(int id, PointOfInterest poi)
    {
        if (id != poi.Id) return BadRequest();
        if (!checkPOIEdit(poi)) return BadRequest("POI edit data is not valid.");

        _context.Entry(poi).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.PointsOfInterest.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return Ok(new { message = "Edit successful" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> deleteSelectedPOI(int id)
    {
        var poi = await _context.PointsOfInterest.FindAsync(id);
        if (poi == null) return NotFound();

        _context.PointsOfInterest.Remove(poi);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successful deletion" });
    }

    [HttpPost("sendRouteData")]
    public async Task<ActionResult<IEnumerable<PointOfInterest>>> sendRouteData([FromBody] RouteSearchRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.City))
        {
            try
            {
                var city = await checkCities(request.City, cancellationToken);
                var osmData = await getOSMData(city, cancellationToken);
                var cleaned = cleanRoadData(osmData);

                if (!string.IsNullOrWhiteSpace(request.Type))
                {
                    cleaned = cleaned
                        .Where(poi => poi.Type.Contains(request.Type, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return cleaned;
            }
            catch (HttpRequestException ex)
            {
                var fallback = getKnownPOI(request.City);
                if (fallback.Count > 0)
                {
                    return fallback;
                }

                return StatusCode(StatusCodes.Status502BadGateway, $"External map API failed: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                return getKnownPOI(request.City);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        return await _context.PointsOfInterest.ToListAsync();
    }

    private bool checkPOI(PointOfInterest poi)
    {
        return poi.checkPOI();
    }

    private bool checkPOIEdit(PointOfInterest poi)
    {
        return poi.checkPOIEdit();
    }

    private async Task<GeoPoint> checkCities(string city, CancellationToken cancellationToken)
    {
        return await geocodeCity(city, cancellationToken);
    }

    private async Task<List<PointOfInterest>> getOSMData(GeoPoint city, CancellationToken cancellationToken)
    {
        var overpassUrl = _configuration["ExternalApis:OverpassUrl"] ?? "https://overpass-api.de/api/interpreter";
        var radius = _configuration.GetValue("ExternalApis:PoiSearchRadiusMeters", 10_000);
        var query = "data=" + Uri.EscapeDataString($"""
            [out:json][timeout:25];
            (
              nwr["tourism"](around:{radius},{city.Latitude.ToString(CultureInfo.InvariantCulture)},{city.Longitude.ToString(CultureInfo.InvariantCulture)});
              nwr["amenity"~"museum|theatre|arts_centre|place_of_worship|restaurant|cafe"](around:{radius},{city.Latitude.ToString(CultureInfo.InvariantCulture)},{city.Longitude.ToString(CultureInfo.InvariantCulture)});
              nwr["historic"](around:{radius},{city.Latitude.ToString(CultureInfo.InvariantCulture)},{city.Longitude.ToString(CultureInfo.InvariantCulture)});
            );
            out center tags 50;
            """);

        try
        {
            using var content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");
            using var response = await _httpClient.PostAsync(overpassUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!json.RootElement.TryGetProperty("elements", out var elements))
            {
                return getKnownPOI(city.Name);
            }

            var pois = elements.EnumerateArray()
                .Select(readOverpassPOI)
                .Where(poi => poi != null)
                .Select(poi => poi!)
                .ToList();

            return pois.Count > 0 ? pois : getKnownPOI(city.Name);
        }
        catch (HttpRequestException)
        {
            return getKnownPOI(city.Name);
        }
        catch (TaskCanceledException)
        {
            return getKnownPOI(city.Name);
        }
    }

    private List<PointOfInterest> cleanRoadData(List<PointOfInterest> pois)
    {
        return pois
            .Where(checkPOI)
            .GroupBy(poi => $"{poi.Name}|{poi.Latitude}|{poi.Longitude}")
            .Select(group => group.First())
            .OrderByDescending(poi => poi.Rating)
            .ThenBy(poi => poi.Name)
            .ToList();
    }

    private async Task<GeoPoint> geocodeCity(string city, CancellationToken cancellationToken)
    {
        var nominatimUrl = _configuration["ExternalApis:NominatimUrl"] ?? "https://nominatim.openstreetmap.org";
        var url = $"{nominatimUrl.TrimEnd('/')}/search?format=jsonv2&limit=1&q={Uri.EscapeDataString(city)}";

        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (json.RootElement.GetArrayLength() == 0)
            {
                return getKnownCity(city);
            }

            var first = json.RootElement[0];
            return new GeoPoint(
                first.GetProperty("display_name").GetString() ?? city,
                double.Parse(first.GetProperty("lat").GetString() ?? "0", CultureInfo.InvariantCulture),
                double.Parse(first.GetProperty("lon").GetString() ?? "0", CultureInfo.InvariantCulture));
        }
        catch (HttpRequestException)
        {
            return getKnownCity(city);
        }
        catch (TaskCanceledException)
        {
            return getKnownCity(city);
        }
    }

    private static PointOfInterest? readOverpassPOI(JsonElement element)
    {
        if (!element.TryGetProperty("tags", out var tags) || !tags.TryGetProperty("name", out var nameElement))
        {
            return null;
        }

        var coordinate = readElementCoordinate(element);
        if (coordinate == null)
        {
            return null;
        }

        var type = readTag(tags, "tourism") ?? readTag(tags, "amenity") ?? readTag(tags, "historic") ?? "OSM";
        return new PointOfInterest
        {
            Name = nameElement.GetString() ?? string.Empty,
            Type = type,
            Address = readTag(tags, "addr:city") ?? readTag(tags, "addr:street") ?? string.Empty,
            HasTicket = type is "museum" or "theme_park" or "zoo",
            WorkingHours = readTag(tags, "opening_hours") ?? string.Empty,
            Rating = 4,
            Latitude = coordinate.Value.Latitude.ToString(CultureInfo.InvariantCulture),
            Longitude = coordinate.Value.Longitude.ToString(CultureInfo.InvariantCulture)
        };
    }

    private static (double Latitude, double Longitude)? readElementCoordinate(JsonElement element)
    {
        if (element.TryGetProperty("lat", out var lat) && element.TryGetProperty("lon", out var lon))
        {
            return (lat.GetDouble(), lon.GetDouble());
        }

        if (element.TryGetProperty("center", out var center))
        {
            return (center.GetProperty("lat").GetDouble(), center.GetProperty("lon").GetDouble());
        }

        return null;
    }

    private static string? readTag(JsonElement tags, string name)
    {
        return tags.TryGetProperty(name, out var value) ? value.GetString() : null;
    }

    private static GeoPoint getKnownCity(string city)
    {
        var knownCities = new Dictionary<string, GeoPoint>(StringComparer.OrdinalIgnoreCase)
        {
            ["Vilnius"] = new("Vilnius", 54.6872, 25.2797),
            ["Kaunas"] = new("Kaunas", 54.8985, 23.9036),
            ["Klaipeda"] = new("Klaipeda", 55.7033, 21.1443),
            ["Klaipėda"] = new("Klaipeda", 55.7033, 21.1443),
            ["Siauliai"] = new("Siauliai", 55.9349, 23.3137),
            ["Šiauliai"] = new("Siauliai", 55.9349, 23.3137),
            ["Panevezys"] = new("Panevezys", 55.7348, 24.3575),
            ["Panevėžys"] = new("Panevezys", 55.7348, 24.3575),
            ["Palanga"] = new("Palanga", 55.9175, 21.0686),
            ["Trakai"] = new("Trakai", 54.6379, 24.9347),
            ["Druskininkai"] = new("Druskininkai", 54.0157, 23.9870)
        };

        if (knownCities.TryGetValue(city.Trim(), out var point))
        {
            return point;
        }

        throw new InvalidOperationException($"City not found in OSM or local fallback: {city}");
    }

    private static List<PointOfInterest> getKnownPOI(string city)
    {
        if (city.Contains("Vilnius", StringComparison.OrdinalIgnoreCase))
        {
            return new List<PointOfInterest>
            {
                createKnownPOI("Vilnius Cathedral", "historic", "Vilnius", 54.6858, 25.2876),
                createKnownPOI("Gediminas Castle Tower", "historic", "Vilnius", 54.6867, 25.2907),
                createKnownPOI("Museum of Occupations and Freedom Fights", "museum", "Vilnius", 54.6870, 25.2708),
                createKnownPOI("Vilnius University", "historic", "Vilnius", 54.6828, 25.2879)
            };
        }

        if (city.Contains("Kaunas", StringComparison.OrdinalIgnoreCase))
        {
            return new List<PointOfInterest>
            {
                createKnownPOI("Kaunas Castle", "historic", "Kaunas", 54.8992, 23.8859),
                createKnownPOI("Devils Museum", "museum", "Kaunas", 54.8990, 23.9114),
                createKnownPOI("Pažaislis Monastery", "historic", "Kaunas", 54.8766, 24.0216)
            };
        }

        if (city.Contains("Trakai", StringComparison.OrdinalIgnoreCase))
        {
            return new List<PointOfInterest>
            {
                createKnownPOI("Trakai Island Castle", "historic", "Trakai", 54.6520, 24.9347)
            };
        }

        return new List<PointOfInterest>();
    }

    private static PointOfInterest createKnownPOI(string name, string type, string address, double latitude, double longitude)
    {
        return new PointOfInterest
        {
            Name = name,
            Type = type,
            Address = address,
            HasTicket = type == "museum",
            WorkingHours = string.Empty,
            Rating = 4.5f,
            Latitude = latitude.ToString(CultureInfo.InvariantCulture),
            Longitude = longitude.ToString(CultureInfo.InvariantCulture)
        };
    }
}

public class RouteSearchRequest
{
    public string City { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
