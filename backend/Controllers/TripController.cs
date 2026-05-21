using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

[ApiController]
[Route("api/[controller]")]
public class TripController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public TripController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("KelionesTravelOffers/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(2);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Trip>>> getTrips()
    {
        return await getTripsData();
    }

    [HttpGet("getTripsData")]
    public async Task<ActionResult<IEnumerable<Trip>>> getTripsData()
    {
        return await _context.Trips.Include(trip => trip.Route).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Trip>> getTrip(int id)
    {
        return await getTripData(id);
    }

    [HttpGet("getTripData/{id}")]
    public async Task<ActionResult<Trip>> getTripData(int id)
    {
        var trip = await _context.Trips.Include(item => item.Route).FirstOrDefaultAsync(item => item.Id == id);
        if (trip == null)
        {
            return NotFound();
        }

        return trip;
    }

    [HttpPost("openTripCreate")]
    public ActionResult<object> openTripCreate()
    {
        return new { message = "TripCreate opened" };
    }

    [HttpPost]
    public async Task<ActionResult<Trip>> saveTripInformation(Trip trip)
    {
        if (!checkTrip(trip))
        {
            return BadRequest("Trip data is not valid.");
        }

        trip.TripStatus = TripStatus.Planned;
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(getTripData), new { id = trip.Id }, trip);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> updateTrip(int id, Trip trip)
    {
        if (id != trip.Id)
        {
            return BadRequest();
        }

        if (!checkTrip(trip))
        {
            return BadRequest("Trip edit data is not valid.");
        }

        _context.Entry(trip).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(new { message = "save information", trip.Id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> deleteTripData(int id)
    {
        var trip = await _context.Trips.FindAsync(id);
        if (trip == null)
        {
            return NotFound();
        }

        removeTrip(trip);
        await _context.SaveChangesAsync();

        return Ok(new { message = "removal status information", id });
    }

    [HttpPost("{tripId}/route/{routeId}")]
    public async Task<IActionResult> assignRouteToTrip(int tripId, int routeId)
    {
        var trip = await _context.Trips.FindAsync(tripId);
        var route = await _context.Routes.FindAsync(routeId);
        if (trip == null || route == null)
        {
            return NotFound();
        }

        trip.RouteId = route.Id;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Route assigned to trip" });
    }

    [HttpPost("accommodation/list")]
    public async Task<ActionResult<IEnumerable<TravelOffer>>> requestAccommodationList(TravelOfferRequest request, CancellationToken cancellationToken)
    {
        return await requestAccommodationListFromExternalActor(request, cancellationToken);
    }

    [HttpPost("flight/list")]
    public async Task<ActionResult<IEnumerable<TravelOffer>>> requestFlightList(TravelOfferRequest request, CancellationToken cancellationToken)
    {
        return await requestFlightListFromExternalActor(request, cancellationToken);
    }

    [HttpPost("car/list")]
    public async Task<ActionResult<IEnumerable<TravelOffer>>> requestCarList(TravelOfferRequest request, CancellationToken cancellationToken)
    {
        return await requestCarListFromExternalActor(request, cancellationToken);
    }

    [HttpPost("{tripId}/accommodation")]
    public async Task<IActionResult> assignAccommodationToTrip(int tripId, [FromBody] TravelOffer offer)
    {
        return await saveSelectedValue(tripId, trip =>
        {
            var reservation = selectAccommodation(trip, offer);
            _context.Reservations.Add(reservation);
        });
    }

    [HttpPost("{tripId}/flight")]
    public async Task<IActionResult> assignFlightToTrip(int tripId, [FromBody] TravelOffer offer)
    {
        return await saveSelectedValue(tripId, trip =>
        {
            var reservation = selectFlight(trip, offer);
            _context.Reservations.Add(reservation);
        });
    }

    [HttpPost("{tripId}/car")]
    public async Task<IActionResult> assignCarToTrip(int tripId, [FromBody] TravelOffer offer)
    {
        return await saveSelectedValue(tripId, trip =>
        {
            var reservation = selectCar(trip, offer);
            _context.Reservations.Add(reservation);
        });
    }

    private bool checkTrip(Trip trip)
    {
        return trip.checkTrip();
    }

    private void removeTrip(Trip trip)
    {
        _context.Trips.Remove(trip);
    }

    private async Task<IActionResult> saveSelectedValue(int tripId, Action<Trip> assign)
    {
        var trip = await _context.Trips.FindAsync(tripId);
        if (trip == null)
        {
            return NotFound();
        }

        assign(trip);
        await _context.SaveChangesAsync();
        return Ok(new { message = "save status information", trip.Id });
    }

    private async Task<List<TravelOffer>> requestAccommodationListFromExternalActor(TravelOfferRequest request, CancellationToken cancellationToken)
    {
        var bookingOffers = await requestProviderOffers("Booking", "ExternalApis:BookingUrl", request, cancellationToken);
        if (bookingOffers.Count > 0)
        {
            return bookingOffers;
        }

        return createFallbackOffers("Booking/local", request.DestinationCity, "Hotel", 85m);
    }

    private async Task<List<TravelOffer>> requestFlightListFromExternalActor(TravelOfferRequest request, CancellationToken cancellationToken)
    {
        var skyscannerOffers = await requestProviderOffers("Skyscanner", "ExternalApis:SkyscannerUrl", request, cancellationToken);
        if (skyscannerOffers.Count > 0)
        {
            return skyscannerOffers;
        }

        var from = await geocodeCity(request.StartCity, cancellationToken);
        var to = await geocodeCity(request.DestinationCity, cancellationToken);
        return createFallbackFlightOffers(from, to, request);
    }

    private async Task<List<TravelOffer>> requestCarListFromExternalActor(TravelOfferRequest request, CancellationToken cancellationToken)
    {
        var discoverCarsOffers = await requestProviderOffers("Discovercars", "ExternalApis:DiscovercarsUrl", request, cancellationToken);
        if (discoverCarsOffers.Count > 0)
        {
            return discoverCarsOffers;
        }

        return createFallbackOffers("Discovercars/local", request.DestinationCity, "Car rental", 35m);
    }

    private Reservation selectAccommodation(Trip trip, TravelOffer offer)
    {
        trip.SelectedAccommodation = offer.Name;
        return saveSelectedAccommodation(trip, offer);
    }

    private Reservation selectFlight(Trip trip, TravelOffer offer)
    {
        trip.SelectedFlight = offer.Name;
        return saveSelectedFlight(trip, offer);
    }

    private Reservation selectCar(Trip trip, TravelOffer offer)
    {
        trip.SelectedCar = offer.Name;
        return saveSelectedCar(trip, offer);
    }

    private Reservation saveSelectedAccommodation(Trip trip, TravelOffer offer)
    {
        return createReservation(trip, offer, ReservationType.Accommodation);
    }

    private Reservation saveSelectedFlight(Trip trip, TravelOffer offer)
    {
        return createReservation(trip, offer, ReservationType.Flight);
    }

    private Reservation saveSelectedCar(Trip trip, TravelOffer offer)
    {
        return createReservation(trip, offer, ReservationType.Car);
    }

    private async Task<List<TravelOffer>> requestProviderOffers(string providerName, string configKey, TravelOfferRequest request, CancellationToken cancellationToken)
    {
        var providerUrl = _configuration[configKey];
        if (string.IsNullOrWhiteSpace(providerUrl))
        {
            return new List<TravelOffer>();
        }

        try
        {
            var url = $"{providerUrl.TrimEnd('/')}?from={Uri.EscapeDataString(request.StartCity)}&to={Uri.EscapeDataString(request.DestinationCity)}&startDate={request.StartDate:yyyy-MM-dd}&endDate={request.EndDate:yyyy-MM-dd}";
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!json.RootElement.TryGetProperty("offers", out var offers))
            {
                return new List<TravelOffer>();
            }

            return offers.EnumerateArray()
                .Select(item => new TravelOffer
                {
                    Provider = providerName,
                    Name = readString(item, "name", providerName),
                    Description = readString(item, "description", string.Empty),
                    Price = readDecimal(item, "price", 0),
                    Latitude = readString(item, "latitude", string.Empty),
                    Longitude = readString(item, "longitude", string.Empty)
                })
                .ToList();
        }
        catch
        {
            return new List<TravelOffer>();
        }
    }

    private async Task<List<TravelOffer>> requestOsmAccommodationList(GeoPoint city, CancellationToken cancellationToken)
    {
        var elements = await requestOverpass(city, """nwr["tourism"~"hotel|hostel|guest_house|apartment"]""", cancellationToken);
        return elements.Select(element => readOsmOffer(element, "Booking/OSM", 85m)).Where(offer => offer != null).Select(offer => offer!).Take(20).ToList();
    }

    private async Task<List<TravelOffer>> requestOsmFlightList(GeoPoint from, GeoPoint to, TravelOfferRequest request, CancellationToken cancellationToken)
    {
        var airportsFrom = await requestOverpass(from, """nwr["aeroway"="aerodrome"]""", cancellationToken);
        var airportsTo = await requestOverpass(to, """nwr["aeroway"="aerodrome"]""", cancellationToken);

        var fromAirport = airportsFrom.Select(element => readOsmOffer(element, "Skyscanner/OSM airport", 0)).FirstOrDefault();
        var toAirport = airportsTo.Select(element => readOsmOffer(element, "Skyscanner/OSM airport", 0)).FirstOrDefault();

        if (fromAirport == null || toAirport == null)
        {
            return new List<TravelOffer>();
        }

        var days = Math.Max(1, (request.EndDate.Date - request.StartDate.Date).Days);
        return new List<TravelOffer>
        {
            new()
            {
                Provider = "Skyscanner/OSM airport",
                Name = $"{fromAirport.Name} -> {toAirport.Name}",
                Description = $"Airport pair found from OSM data for {request.StartDate:yyyy-MM-dd}",
                Price = 80 + days * 25,
                Latitude = toAirport.Latitude,
                Longitude = toAirport.Longitude
            }
        };
    }

    private async Task<List<TravelOffer>> requestOsmCarList(GeoPoint city, CancellationToken cancellationToken)
    {
        var elements = await requestOverpass(city, """nwr["amenity"="car_rental"]""", cancellationToken);
        return elements.Select(element => readOsmOffer(element, "Discovercars/OSM", 35m)).Where(offer => offer != null).Select(offer => offer!).Take(20).ToList();
    }

    private async Task<List<JsonElement>> requestOverpass(GeoPoint city, string selector, CancellationToken cancellationToken)
    {
        try
        {
            var overpassUrl = _configuration["ExternalApis:OverpassUrl"] ?? "https://overpass-api.de/api/interpreter";
            var radius = _configuration.GetValue("ExternalApis:OfferSearchRadiusMeters", 25_000);
            var query = "data=" + Uri.EscapeDataString($"""
                [out:json][timeout:25];
                (
                  {selector}(around:{radius},{city.Latitude.ToString(CultureInfo.InvariantCulture)},{city.Longitude.ToString(CultureInfo.InvariantCulture)});
                );
                out center tags 40;
                """);

            using var content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");
            using var response = await _httpClient.PostAsync(overpassUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!json.RootElement.TryGetProperty("elements", out var elements))
            {
                return new List<JsonElement>();
            }

            return elements.EnumerateArray().Select(element => element.Clone()).ToList();
        }
        catch
        {
            return new List<JsonElement>();
        }
    }

    private async Task<GeoPoint> geocodeCity(string city, CancellationToken cancellationToken)
    {
        if (isKnownCity(city))
        {
            return createFallbackGeoPoint(city);
        }

        try
        {
            var nominatimUrl = _configuration["ExternalApis:NominatimUrl"] ?? "https://nominatim.openstreetmap.org";
            var url = $"{nominatimUrl.TrimEnd('/')}/search?format=jsonv2&limit=1&q={Uri.EscapeDataString(city)}";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (json.RootElement.GetArrayLength() == 0)
            {
                return createFallbackGeoPoint(city);
            }

            var first = json.RootElement[0];
            return new GeoPoint(
                first.GetProperty("display_name").GetString() ?? city,
                readDouble(first.GetProperty("lat").GetString()),
                readDouble(first.GetProperty("lon").GetString()));
        }
        catch
        {
            return createFallbackGeoPoint(city);
        }
    }

    private static Reservation createReservation(Trip trip, TravelOffer offer, ReservationType type)
    {
        return new Reservation
        {
            TripId = trip.Id,
            ReservationDate = DateTime.UtcNow,
            ReservationStatus = ReservationStatus.WaitingForPayment,
            ReservationType = type,
            Provider = offer.Provider,
            Description = $"{offer.Name} - {offer.Description}".Trim(' ', '-'),
            Price = offer.Price
        };
    }

    private static TravelOffer? readOsmOffer(JsonElement element, string provider, decimal basePrice)
    {
        if (!element.TryGetProperty("tags", out var tags) || !tags.TryGetProperty("name", out var name))
        {
            return null;
        }

        var location = readElementCoordinate(element);
        if (location == null)
        {
            return null;
        }

        return new TravelOffer
        {
            Provider = provider,
            Name = name.GetString() ?? provider,
            Description = readTag(tags, "brand") ?? readTag(tags, "operator") ?? readTag(tags, "tourism") ?? readTag(tags, "amenity") ?? string.Empty,
            Price = basePrice,
            Latitude = location.Value.Latitude.ToString(CultureInfo.InvariantCulture),
            Longitude = location.Value.Longitude.ToString(CultureInfo.InvariantCulture)
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

    private static string readString(JsonElement item, string property, string fallback)
    {
        return item.TryGetProperty(property, out var value) ? value.ToString() : fallback;
    }

    private static decimal readDecimal(JsonElement item, string property, decimal fallback)
    {
        return item.TryGetProperty(property, out var value) && value.TryGetDecimal(out var number) ? number : fallback;
    }

    private static double readDouble(string? value)
    {
        return double.Parse(value ?? "0", CultureInfo.InvariantCulture);
    }

    private static GeoPoint createFallbackGeoPoint(string city)
    {
        return city.Trim().ToLowerInvariant() switch
        {
            "kaunas" => new GeoPoint("Kaunas", 54.8985, 23.9036),
            "vilnius" => new GeoPoint("Vilnius", 54.6872, 25.2797),
            "klaipeda" or "klaipėda" => new GeoPoint("Klaipeda", 55.7033, 21.1443),
            "trakai" => new GeoPoint("Trakai", 54.6378, 24.9343),
            _ => new GeoPoint(city, 54.6872, 25.2797)
        };
    }

    private static bool isKnownCity(string city)
    {
        var value = city.Trim().ToLowerInvariant();
        return value is "kaunas" or "vilnius" or "klaipeda" or "klaipėda" or "trakai";
    }

    private static List<TravelOffer> createFallbackOffers(string provider, string city, string category, decimal basePrice)
    {
        var place = createFallbackGeoPoint(city);
        return new List<TravelOffer>
        {
            new()
            {
                Provider = provider,
                Name = $"{city} {category} option",
                Description = $"Local fallback {category.ToLowerInvariant()} offer for {city}",
                Price = basePrice,
                Latitude = place.Latitude.ToString(CultureInfo.InvariantCulture),
                Longitude = place.Longitude.ToString(CultureInfo.InvariantCulture)
            },
            new()
            {
                Provider = provider,
                Name = $"{city} central {category.ToLowerInvariant()}",
                Description = $"Central {category.ToLowerInvariant()} option when external service is unavailable",
                Price = basePrice + 20,
                Latitude = (place.Latitude + 0.01).ToString(CultureInfo.InvariantCulture),
                Longitude = (place.Longitude + 0.01).ToString(CultureInfo.InvariantCulture)
            }
        };
    }

    private static List<TravelOffer> createFallbackFlightOffers(GeoPoint from, GeoPoint to, TravelOfferRequest request)
    {
        var days = Math.Max(1, (request.EndDate.Date - request.StartDate.Date).Days);
        return new List<TravelOffer>
        {
            new()
            {
                Provider = "Skyscanner/local",
                Name = $"{from.Name} -> {to.Name}",
                Description = $"Local fallback flight for {request.StartDate:yyyy-MM-dd}",
                Price = 80 + days * 25,
                Latitude = to.Latitude.ToString(CultureInfo.InvariantCulture),
                Longitude = to.Longitude.ToString(CultureInfo.InvariantCulture)
            }
        };
    }
}

public class TripSelectionRequest
{
    public string Value { get; set; } = string.Empty;
}

public class TravelOfferRequest
{
    public string StartCity { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class TravelOffer
{
    public string Provider { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
}

public record GeoPoint(string Name, double Latitude, double Longitude);
