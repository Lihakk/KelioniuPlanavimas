using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using TripRoute = backend.Models.Route;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
public class RouteController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public RouteController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("KelionesRoutePlanner/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TripRoute>>> getRoutes()
    {
        return await getAllRoutes();
    }

    [HttpGet("showRouteList")]
    public async Task<ActionResult<IEnumerable<TripRoute>>> showRouteList()
    {
        return await getAllRoutes();
    }

    [HttpGet("openRoutes")]
    public async Task<ActionResult<IEnumerable<TripRoute>>> openRoutes()
    {
        return await getAllRoutes();
    }

    [HttpGet("getAllRoutes")]
    public async Task<ActionResult<IEnumerable<TripRoute>>> getAllRoutes()
    {
        return await _context.Routes
            .Include(route => route.RoutePoints.OrderBy(point => point.Order))
            .ThenInclude(point => point.PointOfInterest)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TripRoute>> getRoute(int id)
    {
        return await getSpecificRoute(id);
    }

    [HttpGet("openRouteView/{id}")]
    public async Task<ActionResult<TripRoute>> openRouteView(int id)
    {
        return await getSpecificRoute(id);
    }

    [HttpGet("openRouteEdit/{id}")]
    public async Task<ActionResult<TripRoute>> openRouteEdit(int id)
    {
        return await getSpecificRoute(id);
    }

    [HttpGet("getSpecificRoute/{id}")]
    public async Task<ActionResult<TripRoute>> getSpecificRoute(int id)
    {
        var route = await _context.Routes
            .Include(item => item.RoutePoints.OrderBy(point => point.Order))
            .ThenInclude(point => point.PointOfInterest)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (route == null)
        {
            return NotFound();
        }

        return route;
    }

    [HttpGet("{id}/getRoutePOI")]
    public async Task<ActionResult<IEnumerable<RoutePoint>>> getRoutePOI(int id)
    {
        var route = await _context.Routes
            .Include(item => item.RoutePoints.OrderBy(point => point.Order))
            .FirstOrDefaultAsync(item => item.Id == id);

        if (route == null)
        {
            return NotFound();
        }

        return route.RoutePoints.OrderBy(point => point.Order).ToList();
    }

    [HttpPost("openRouteCreation")]
    public ActionResult<object> openRouteCreation()
    {
        return new { message = "RouteCreate opened" };
    }

    [HttpPost("preview")]
    public async Task<ActionResult<TripRoute>> previewRoute(TripRoute route, CancellationToken cancellationToken)
    {
        if (!checkRoute(route))
        {
            return BadRequest("Route data is not valid.");
        }

        try
        {
            await sendCities(route, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, $"External map API failed: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        route.Id = 0;
        foreach (var routePoint in route.RoutePoints)
        {
            routePoint.Id = 0;
            routePoint.RouteId = 0;
        }

        return route;
    }

    [HttpPost]
    public async Task<ActionResult<TripRoute>> saveRoute(TripRoute route, CancellationToken cancellationToken)
    {
        if (!checkRoute(route))
        {
            return BadRequest("Route data is not valid.");
        }

        try
        {
            if (hasCalculatedRouteData(route))
            {
                cleanConfirmedRouteData(route);
            }
            else
            {
                await sendCities(route, cancellationToken);
            }

            _context.Routes.Add(route);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, $"External map API failed: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        return CreatedAtAction(nameof(getSpecificRoute), new { id = route.Id }, route);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> sendRouteData(int id, TripRoute route, CancellationToken cancellationToken)
    {
        if (id != route.Id)
        {
            return BadRequest();
        }

        if (!checkRoute(route))
        {
            return BadRequest("Route edit data is not valid.");
        }

        try
        {
            if (hasCalculatedRouteData(route))
            {
                cleanConfirmedRouteData(route);
            }
            else
            {
                await sendCities(route, cancellationToken);
            }

            var existingRoute = await _context.Routes
                .Include(item => item.RoutePoints)
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (existingRoute == null)
            {
                return NotFound();
            }

            existingRoute.Name = route.Name;
            existingRoute.Length = route.Length;
            existingRoute.StartingCity = route.StartingCity;
            existingRoute.EndCity = route.EndCity;
            existingRoute.Polyline = route.Polyline;
            existingRoute.TravelTime = route.TravelTime;

            _context.RoutePoints.RemoveRange(existingRoute.RoutePoints);
            existingRoute.RoutePoints = route.RoutePoints
                .Select((point, index) => new RoutePoint
                {
                    RouteId = id,
                    Name = point.Name,
                    City = point.City,
                    Latitude = point.Latitude,
                    Longitude = point.Longitude,
                    Order = point.Order > 0 ? point.Order : index + 1,
                    PointOfInterestId = point.PointOfInterestId
                })
                .ToList();

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, $"External map API failed: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok(new { message = "Route save status", route.Id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> deleteRoute(int id)
    {
        var route = await _context.Routes.Include(item => item.RoutePoints).FirstOrDefaultAsync(item => item.Id == id);
        if (route == null)
        {
            return NotFound();
        }

        _context.RoutePoints.RemoveRange(route.RoutePoints);
        _context.Routes.Remove(route);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Route removal status", id });
    }

    [HttpGet("getTime/{id}")]
    public async Task<ActionResult<string>> getTime(int id)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound();
        }

        return evaluateRouteData(route);
    }

    [HttpPost("{routeId}/POI/{poiId}")]
    public async Task<IActionResult> saveRoutePOI(int routeId, int poiId)
    {
        var route = await _context.Routes.Include(item => item.RoutePoints).FirstOrDefaultAsync(item => item.Id == routeId);
        var poi = await _context.PointsOfInterest.FindAsync(poiId);
        if (route == null || poi == null)
        {
            return NotFound();
        }

        route.RoutePoints.Add(new RoutePoint
        {
            PointOfInterestId = poi.Id,
            Name = poi.Name,
            City = poi.Address,
            Latitude = poi.Latitude,
            Longitude = poi.Longitude,
            Order = route.RoutePoints.Count + 1
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = "Route POI saved" });
    }

    [HttpPost("selectedPOI")]
    public async Task<ActionResult<IEnumerable<PointOfInterest>>> selectedPOI([FromBody] RouteSearchRequest request, CancellationToken cancellationToken)
    {
        var route = new TripRoute
        {
            Name = request.City,
            StartingCity = request.City,
            EndCity = request.City
        };

        var cities = await checkCities(route, cancellationToken);
        var osmData = await getOSMData(cities, cancellationToken);
        var poi = selectedPOI(route, cleanRoadData(osmData));

        return poi;
    }

    [HttpPost("roadPOI")]
    public async Task<ActionResult<IEnumerable<PointOfInterest>>> getRoadPOI([FromBody] RoadPoiRequest request, CancellationToken cancellationToken)
    {
        var route = new TripRoute
        {
            Name = $"{request.StartingCity} to {request.EndCity}",
            StartingCity = request.StartingCity,
            EndCity = request.EndCity
        };

        try
        {
            var cities = await checkCities(route, cancellationToken);
            var osmData = await getOSMData(cities, cancellationToken);
            var roadData = cleanRoadData(osmData)
                .Concat(createCorridorPOI(cities))
                .GroupBy(poi => $"{poi.Name}|{poi.Latitude}|{poi.Longitude}", StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderByDescending(poi => poi.Rating)
                .ThenBy(poi => poi.Name)
                .Take(80)
                .ToList();

            return roadData;
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private async Task<RoutePlanningResult> sendCities(TripRoute route, CancellationToken cancellationToken = default)
    {
        var checkedCities = await checkCities(route, cancellationToken);
        route.StartingCity = checkedCities.Start.Name;
        route.EndCity = checkedCities.End.Name;

        var routeObjects = separateParts(route, checkedCities);
        var osmData = await getOSMData(checkedCities, cancellationToken);
        var roadData = cleanRoadData(osmData);
        var selectedPois = selectedPOI(route, roadData);
        var pointsWithPOI = createRouteWithPOI(routeObjects, selectedPois);
        var poiRoads = await findPOIRoads(pointsWithPOI, cancellationToken);
        var fastestRoad = await sendRoute(poiRoads, cancellationToken);

        var routeTime = getRouteTime(fastestRoad);
        saveRouteTime(route, routeTime);
        saveRouteData(route, fastestRoad, fastestRoad.Points);

        return new RoutePlanningResult(route, selectedPois, fastestRoad);
    }

    private async Task<CheckedCities> checkCities(TripRoute route, CancellationToken cancellationToken = default)
    {
        if (!checkRoute(route))
        {
            throw new InvalidOperationException("Route has invalid city data.");
        }

        var start = await geocodeCity(route.StartingCity, cancellationToken);
        var end = await geocodeCity(route.EndCity, cancellationToken);

        return new CheckedCities(start, end);
    }

    private bool checkRoute(TripRoute route)
    {
        return route.checkRoute();
    }

    private bool hasCalculatedRouteData(TripRoute route)
    {
        return route.Length > 0
            && !string.IsNullOrWhiteSpace(route.Polyline)
            && !string.IsNullOrWhiteSpace(route.TravelTime);
    }

    private void cleanConfirmedRouteData(TripRoute route)
    {
        route.Id = Math.Max(0, route.Id);
        route.RoutePoints = route.RoutePoints
            .Select((point, index) => new RoutePoint
            {
                Name = point.Name,
                City = point.City,
                Latitude = point.Latitude,
                Longitude = point.Longitude,
                Order = point.Order > 0 ? point.Order : index + 1,
                PointOfInterestId = point.PointOfInterestId
            })
            .ToList();
    }

    private bool checkPOI(PointOfInterest poi)
    {
        return poi.checkPOI();
    }

    private List<RoutePlanningPoint> separateParts(TripRoute route, CheckedCities cities)
    {
        return selectObjects(route, cities);
    }

    private List<RoutePlanningPoint> selectObjects(TripRoute route, CheckedCities cities)
    {
        var points = new List<RoutePlanningPoint>
        {
            new("Start", cities.Start.Name, cities.Start.Latitude, cities.Start.Longitude),
            new("End", cities.End.Name, cities.End.Latitude, cities.End.Longitude)
        };

        foreach (var routePoint in route.RoutePoints)
        {
            if (tryReadCoordinate(routePoint.Latitude, out var lat) && tryReadCoordinate(routePoint.Longitude, out var lon))
            {
                points.Insert(points.Count - 1, new RoutePlanningPoint(routePoint.Name, routePoint.City, lat, lon));
            }
        }

        return points;
    }

    private async Task<List<PointOfInterest>> getOSMData(CheckedCities cities, CancellationToken cancellationToken = default)
    {
        var overpassUrl = _configuration["ExternalApis:OverpassUrl"] ?? "https://overpass-api.de/api/interpreter";
        var radius = _configuration.GetValue("ExternalApis:PoiSearchRadiusMeters", 10_000);
        var query = buildOverpassQuery(cities.Start, cities.End, radius);

        try
        {
            using var content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");
            using var response = await _httpClient.PostAsync(overpassUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var pois = new List<PointOfInterest>();
            if (!json.RootElement.TryGetProperty("elements", out var elements))
            {
                return getFallbackPOI(cities);
            }

            foreach (var element in elements.EnumerateArray())
            {
                var poi = readOverpassPOI(element);
                if (poi != null && checkPOI(poi))
                {
                    pois.Add(poi);
                }
            }

            return pois
                .GroupBy(poi => $"{poi.Name}|{poi.Latitude}|{poi.Longitude}")
                .Select(group => group.First())
                .Take(50)
                .DefaultIfEmpty()
                .Where(poi => poi != null)
                .Select(poi => poi!)
                .ToList();
        }
        catch (HttpRequestException)
        {
            return getFallbackPOI(cities);
        }
        catch (TaskCanceledException)
        {
            return getFallbackPOI(cities);
        }
    }

    private List<PointOfInterest> cleanRoadData(List<PointOfInterest> pois)
    {
        return pois
            .Where(checkPOI)
            .OrderByDescending(poi => poi.Rating)
            .ThenBy(poi => poi.Name)
            .ToList();
    }

    private List<PointOfInterest> selectedPOI(TripRoute route, List<PointOfInterest> osmPois)
    {
        if (route.RoutePoints.Count == 0)
        {
            return osmPois.Take(5).ToList();
        }

        var routePointNames = route.RoutePoints.Select(point => point.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return osmPois.Where(poi => routePointNames.Contains(poi.Name)).DistinctBy(poi => poi.Name).ToList();
    }

    private List<RoutePlanningPoint> createRouteWithPOI(List<RoutePlanningPoint> routeObjects, List<PointOfInterest> selectedPoi)
    {
        var points = new List<RoutePlanningPoint> { routeObjects.First() };
        var explicitRouteObjects = routeObjects.Skip(1).Take(Math.Max(0, routeObjects.Count - 2)).ToList();
        if (explicitRouteObjects.Count > 0)
        {
            points.AddRange(explicitRouteObjects);
        }
        else if (selectedPoi.Count > 0)
        {
            points.AddRange(selectedPoi.Select(poi => new RoutePlanningPoint(poi.Name, poi.Address, readCoordinate(poi.Latitude), readCoordinate(poi.Longitude), poi)));
        }

        points.Add(routeObjects.Last());
        return points;
    }

    private async Task<RouteMatrix> findPOIRoads(List<RoutePlanningPoint> points, CancellationToken cancellationToken = default)
    {
        return await createLengthMatrix(points, cancellationToken);
    }

    private async Task<RouteRoad> SelectFastestRoad(RouteMatrix matrix, CancellationToken cancellationToken = default)
    {
        var graph = createGraph(matrix);
        var initialRoad = createInitialRoute(graph);
        var road = shuffleObjectOrder(graph, initialRoad);
        var selectedRoad = selectRoad(matrix, road);
        return await createPolyLine(selectedRoad, matrix, cancellationToken);
    }

    private async Task<RouteRoad> sendRoute(RouteMatrix matrix, CancellationToken cancellationToken = default)
    {
        return await SelectFastestRoad(matrix, cancellationToken);
    }

    private List<List<double>> createGraph(RouteMatrix matrix)
    {
        return matrix.Durations;
    }

    private async Task<RouteMatrix> createLengthMatrix(List<RoutePlanningPoint> points, CancellationToken cancellationToken = default)
    {
        try
        {
            var roadGraph = await readOSMFile(points, cancellationToken);
            var graphNodes = points.Select(point => findNearestGraphNode(roadGraph, point)).ToList();
            var distances = new List<List<double>>();
            var durations = new List<List<double>>();

            for (var i = 0; i < graphNodes.Count; i++)
            {
                var shortestDistances = dijkstraDistances(roadGraph, graphNodes[i]);
                var distanceRow = new List<double>();
                var durationRow = new List<double>();
                for (var j = 0; j < graphNodes.Count; j++)
                {
                    var distance = shortestDistances.TryGetValue(graphNodes[j], out var value) ? value : double.NaN;
                    distanceRow.Add(distance);
                    durationRow.Add(double.IsNaN(distance) ? double.NaN : distance / 60_000d * 3600d);
                }

                distances.Add(distanceRow);
                durations.Add(durationRow);
            }

            saveToLengthMatrix(durations);

            return new RouteMatrix(points, durations, distances, roadGraph, graphNodes);
        }
        catch (HttpRequestException)
        {
            return createFallbackLengthMatrix(points);
        }
        catch (TaskCanceledException)
        {
            return createFallbackLengthMatrix(points);
        }
        catch (InvalidOperationException)
        {
            return createFallbackLengthMatrix(points);
        }
    }

    private int calculateHeuristic(List<double> durations, HashSet<int> visited)
    {
        var bestIndex = -1;
        var bestValue = double.MaxValue;

        for (var i = 0; i < durations.Count; i++)
        {
            if (visited.Contains(i) || double.IsNaN(durations[i]) || durations[i] <= 0)
            {
                continue;
            }

            if (durations[i] < bestValue)
            {
                bestValue = durations[i];
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private List<int> createInitialRoute(List<List<double>> graph)
    {
        if (graph.Count <= 2)
        {
            return Enumerable.Range(0, graph.Count).ToList();
        }

        var visited = new HashSet<int> { 0 };
        var road = new List<int> { 0 };
        var current = 0;
        var end = graph.Count - 1;

        while (visited.Count < graph.Count - 1)
        {
            visited.Add(end);
            var next = calculateHeuristic(graph[current], visited);
            visited.Remove(end);

            if (next < 0)
            {
                next = Enumerable.Range(1, graph.Count - 2).FirstOrDefault(index => !visited.Contains(index));
                if (next == 0)
                {
                    break;
                }
            }

            road.Add(next);
            visited.Add(next);
            current = next;
        }

        road.Add(end);
        return road;
    }

    private List<int> shuffleObjectOrder(List<List<double>> graph, List<int> road)
    {
        var improved = true;
        while (improved)
        {
            improved = false;
            for (var i = 1; i < road.Count - 2; i++)
            {
                var swapped = road.ToList();
                (swapped[i], swapped[i + 1]) = (swapped[i + 1], swapped[i]);

                if (evaluateRoads(graph, swapped) < evaluateRoads(graph, road))
                {
                    road = swapped;
                    improved = true;
                }
            }
        }

        return road;
    }

    private List<RoutePlanningPoint> selectRoad(RouteMatrix matrix, List<int> road)
    {
        return road.Select(index => matrix.Points[index]).ToList();
    }

    private void saveToLengthMatrix(List<List<double>> matrix)
    {
        if (matrix.Count == 0)
        {
            throw new InvalidOperationException("OSRM did not return a length matrix.");
        }
    }

    private double evaluateRoads(List<List<double>> graph, List<int> road)
    {
        var total = 0d;
        for (var i = 0; i < road.Count - 1; i++)
        {
            total += graph[road[i]][road[i + 1]];
        }

        return total;
    }

    private Task<RouteRoad> createPolyLine(List<RoutePlanningPoint> selectedRoad, RouteMatrix matrix, CancellationToken cancellationToken = default)
    {
        if (matrix.RoadGraph == null || matrix.GraphNodeIds == null)
        {
            return Task.FromResult(createFallbackPolyLine(selectedRoad));
        }

        var distance = 0d;
        var coordinates = new List<string>();
        for (var i = 0; i < selectedRoad.Count - 1; i++)
        {
            var fromIndex = matrix.Points.IndexOf(selectedRoad[i]);
            var toIndex = matrix.Points.IndexOf(selectedRoad[i + 1]);
            if (fromIndex < 0 || toIndex < 0)
            {
                return Task.FromResult(createFallbackPolyLine(selectedRoad));
            }

            var nodePath = dijkstraPath(matrix.RoadGraph, matrix.GraphNodeIds[fromIndex], matrix.GraphNodeIds[toIndex]);
            if (nodePath.Count == 0)
            {
                return Task.FromResult(createFallbackPolyLine(selectedRoad));
            }

            for (var n = 0; n < nodePath.Count; n++)
            {
                if (coordinates.Count > 0 && n == 0)
                {
                    continue;
                }

                var node = matrix.RoadGraph.Nodes[nodePath[n]];
                coordinates.Add($"[{node.Longitude.ToString(CultureInfo.InvariantCulture)},{node.Latitude.ToString(CultureInfo.InvariantCulture)}]");
            }

            distance += calculatePathDistance(matrix.RoadGraph, nodePath);
        }

        if (coordinates.Count == 0)
        {
            return Task.FromResult(createFallbackPolyLine(selectedRoad));
        }

        var geoJson = $$"""{"type":"LineString","coordinates":[{{string.Join(",", coordinates)}}]}""";
        return Task.FromResult(new RouteRoad(selectedRoad, distance, distance / 60_000d * 3600d, geoJson));
    }

    private RouteRoad checkPolyLine(JsonElement root, List<RoutePlanningPoint> selectedRoad)
    {
        if (!root.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("OSRM did not return a route.");
        }

        var route = routes[0];
        var distance = route.GetProperty("distance").GetDouble();
        var duration = route.GetProperty("duration").GetDouble();
        var geometry = route.GetProperty("geometry").GetRawText();

        return new RouteRoad(selectedRoad, distance, duration, geometry);
    }

    private TimeSpan getRouteTime(RouteRoad road)
    {
        return TimeSpan.FromSeconds(road.DurationSeconds);
    }

    private void saveRouteTime(TripRoute route, TimeSpan routeTime)
    {
        route.TravelTime = routeTime.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
    }

    private void saveRouteData(TripRoute route, RouteRoad road, List<RoutePlanningPoint> points)
    {
        route.Length = (float)Math.Round(road.DistanceMeters / 1000d, 2);
        route.Polyline = road.GeoJson;
        route.RoutePoints = points
            .Skip(1)
            .Take(Math.Max(0, points.Count - 2))
            .Select((point, index) => new RoutePoint
            {
                Name = point.Name,
                City = point.City,
                Latitude = point.Latitude.ToString(CultureInfo.InvariantCulture),
                Longitude = point.Longitude.ToString(CultureInfo.InvariantCulture),
                Order = index + 1,
                PointOfInterest = point.PointOfInterest
            })
            .ToList();
    }

    private async Task<GeoPoint> geocodeCity(string city, CancellationToken cancellationToken)
    {
        var nominatimUrl = _configuration["ExternalApis:NominatimUrl"] ?? "https://nominatim.openstreetmap.org";
        var url = $"{nominatimUrl.TrimEnd('/')}/search?format=jsonv2&limit=1&addressdetails=1&q={Uri.EscapeDataString(city)}";

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
                readCoordinate(first.GetProperty("lat").GetString()),
                readCoordinate(first.GetProperty("lon").GetString()));
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

    private static string buildOverpassQuery(GeoPoint start, GeoPoint end, int radius)
    {
        return "data=" + Uri.EscapeDataString($"""
            [out:json][timeout:25];
            (
              nwr["tourism"](around:{radius},{start.Latitude.ToString(CultureInfo.InvariantCulture)},{start.Longitude.ToString(CultureInfo.InvariantCulture)});
              nwr["amenity"~"museum|theatre|arts_centre|place_of_worship|restaurant|cafe"](around:{radius},{start.Latitude.ToString(CultureInfo.InvariantCulture)},{start.Longitude.ToString(CultureInfo.InvariantCulture)});
              nwr["historic"](around:{radius},{start.Latitude.ToString(CultureInfo.InvariantCulture)},{start.Longitude.ToString(CultureInfo.InvariantCulture)});
              nwr["tourism"](around:{radius},{end.Latitude.ToString(CultureInfo.InvariantCulture)},{end.Longitude.ToString(CultureInfo.InvariantCulture)});
              nwr["amenity"~"museum|theatre|arts_centre|place_of_worship|restaurant|cafe"](around:{radius},{end.Latitude.ToString(CultureInfo.InvariantCulture)},{end.Longitude.ToString(CultureInfo.InvariantCulture)});
              nwr["historic"](around:{radius},{end.Latitude.ToString(CultureInfo.InvariantCulture)},{end.Longitude.ToString(CultureInfo.InvariantCulture)});
            );
            out center tags 50;
            """);
    }

    private async Task<OsmRoadGraph> readOSMFile(List<RoutePlanningPoint> points, CancellationToken cancellationToken)
    {
        var overpassUrl = _configuration["ExternalApis:OverpassUrl"] ?? "https://overpass-api.de/api/interpreter";
        var query = buildRoadGraphQuery(points);

        using var content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");
        using var response = await _httpClient.PostAsync(overpassUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!json.RootElement.TryGetProperty("elements", out var elements))
        {
            throw new InvalidOperationException("OSM road data did not include elements.");
        }

        var graph = separateRoadsAndCrossroads(elements);
        calculateSegmentLengths(graph);
        return graph;
    }

    private static string buildRoadGraphQuery(List<RoutePlanningPoint> points)
    {
        var minLatitude = points.Min(point => point.Latitude) - 0.08;
        var maxLatitude = points.Max(point => point.Latitude) + 0.08;
        var minLongitude = points.Min(point => point.Longitude) - 0.08;
        var maxLongitude = points.Max(point => point.Longitude) + 0.08;

        return "data=" + Uri.EscapeDataString($"""
            [out:json][timeout:25];
            (
              way["highway"~"motorway|trunk|primary|secondary|tertiary|unclassified|residential|service|living_street"]({minLatitude.ToString(CultureInfo.InvariantCulture)},{minLongitude.ToString(CultureInfo.InvariantCulture)},{maxLatitude.ToString(CultureInfo.InvariantCulture)},{maxLongitude.ToString(CultureInfo.InvariantCulture)});
            );
            (._;>;);
            out body;
            """);
    }

    private static OsmRoadGraph separateRoadsAndCrossroads(JsonElement elements)
    {
        var graph = new OsmRoadGraph();
        var roadWays = new List<List<long>>();

        foreach (var element in elements.EnumerateArray())
        {
            var type = readString(element, "type");
            if (type == "node" && element.TryGetProperty("id", out var id) && element.TryGetProperty("lat", out var lat) && element.TryGetProperty("lon", out var lon))
            {
                graph.Nodes[id.GetInt64()] = new RoadGraphNode(id.GetInt64(), lat.GetDouble(), lon.GetDouble());
            }

            if (type == "way" && element.TryGetProperty("nodes", out var nodes))
            {
                var wayNodes = nodes.EnumerateArray().Select(node => node.GetInt64()).ToList();
                if (wayNodes.Count > 1)
                {
                    roadWays.Add(wayNodes);
                }
            }
        }

        foreach (var way in roadWays)
        {
            for (var i = 0; i < way.Count - 1; i++)
            {
                if (!graph.Nodes.ContainsKey(way[i]) || !graph.Nodes.ContainsKey(way[i + 1]))
                {
                    continue;
                }

                graph.Edges.TryAdd(way[i], new List<RoadGraphEdge>());
                graph.Edges.TryAdd(way[i + 1], new List<RoadGraphEdge>());
                graph.Edges[way[i]].Add(new RoadGraphEdge(way[i + 1], 0));
                graph.Edges[way[i + 1]].Add(new RoadGraphEdge(way[i], 0));
            }
        }

        if (graph.Nodes.Count == 0 || graph.Edges.Count == 0)
        {
            throw new InvalidOperationException("OSM road graph could not be built.");
        }

        return graph;
    }

    private static void calculateSegmentLengths(OsmRoadGraph graph)
    {
        foreach (var (fromId, edges) in graph.Edges.ToList())
        {
            var from = graph.Nodes[fromId];
            for (var i = 0; i < edges.Count; i++)
            {
                var to = graph.Nodes[edges[i].To];
                var distance = calculateRoadDistanceMeters(
                    new RoutePlanningPoint(from.Id.ToString(CultureInfo.InvariantCulture), string.Empty, from.Latitude, from.Longitude),
                    new RoutePlanningPoint(to.Id.ToString(CultureInfo.InvariantCulture), string.Empty, to.Latitude, to.Longitude));
                edges[i] = edges[i] with { DistanceMeters = distance };
            }
        }
    }

    private static long findNearestGraphNode(OsmRoadGraph graph, RoutePlanningPoint point)
    {
        return graph.Nodes.Values
            .OrderBy(node => calculateRoadDistanceMeters(
                point,
                new RoutePlanningPoint(node.Id.ToString(CultureInfo.InvariantCulture), string.Empty, node.Latitude, node.Longitude)))
            .First()
            .Id;
    }

    private static Dictionary<long, double> dijkstraDistances(OsmRoadGraph graph, long startNode)
    {
        var distances = graph.Nodes.Keys.ToDictionary(id => id, _ => double.PositiveInfinity);
        var queue = new PriorityQueue<long, double>();
        distances[startNode] = 0;
        queue.Enqueue(startNode, 0);

        while (queue.TryDequeue(out var current, out var currentDistance))
        {
            if (currentDistance > distances[current])
            {
                continue;
            }

            if (!graph.Edges.TryGetValue(current, out var edges))
            {
                continue;
            }

            foreach (var edge in edges)
            {
                var candidate = currentDistance + edge.DistanceMeters;
                if (candidate < distances[edge.To])
                {
                    distances[edge.To] = candidate;
                    queue.Enqueue(edge.To, candidate);
                }
            }
        }

        return distances;
    }

    private static List<long> dijkstraPath(OsmRoadGraph graph, long startNode, long endNode)
    {
        var distances = graph.Nodes.Keys.ToDictionary(id => id, _ => double.PositiveInfinity);
        var previous = new Dictionary<long, long>();
        var queue = new PriorityQueue<long, double>();
        distances[startNode] = 0;
        queue.Enqueue(startNode, 0);

        while (queue.TryDequeue(out var current, out var currentDistance))
        {
            if (current == endNode)
            {
                break;
            }

            if (currentDistance > distances[current] || !graph.Edges.TryGetValue(current, out var edges))
            {
                continue;
            }

            foreach (var edge in edges)
            {
                var candidate = currentDistance + edge.DistanceMeters;
                if (candidate < distances[edge.To])
                {
                    distances[edge.To] = candidate;
                    previous[edge.To] = current;
                    queue.Enqueue(edge.To, candidate);
                }
            }
        }

        if (double.IsInfinity(distances[endNode]))
        {
            return new List<long>();
        }

        var path = new List<long> { endNode };
        while (path[^1] != startNode)
        {
            path.Add(previous[path[^1]]);
        }

        path.Reverse();
        return path;
    }

    private static double calculatePathDistance(OsmRoadGraph graph, List<long> nodePath)
    {
        var total = 0d;
        for (var i = 0; i < nodePath.Count - 1; i++)
        {
            var edge = graph.Edges[nodePath[i]].First(item => item.To == nodePath[i + 1]);
            total += edge.DistanceMeters;
        }

        return total;
    }

    private static string readString(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var value) ? value.GetString() ?? string.Empty : string.Empty;
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

        var type = readTag(tags, "tourism")
            ?? readTag(tags, "amenity")
            ?? readTag(tags, "historic")
            ?? "OSM";

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

    private static List<List<double>> readMatrix(JsonElement root, string property)
    {
        return root.GetProperty(property)
            .EnumerateArray()
            .Select(row => row.EnumerateArray().Select(value => value.ValueKind == JsonValueKind.Null ? double.NaN : value.GetDouble()).ToList())
            .ToList();
    }

    private static bool tryReadCoordinate(string value, out double coordinate)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out coordinate);
    }

    private static double readCoordinate(string? value)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var coordinate))
        {
            throw new InvalidOperationException($"Invalid coordinate: {value}");
        }

        return coordinate;
    }

    private static string formatCoordinate(double longitude, double latitude)
    {
        return $"{longitude.ToString(CultureInfo.InvariantCulture)},{latitude.ToString(CultureInfo.InvariantCulture)}";
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

    private static List<PointOfInterest> getFallbackPOI(CheckedCities cities)
    {
        return getKnownPOI(cities.Start.Name)
            .Concat(getKnownPOI(cities.End.Name))
            .Concat(createCorridorPOI(cities))
            .GroupBy(poi => poi.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private static List<PointOfInterest> createCorridorPOI(CheckedCities cities)
    {
        var names = new[]
        {
            "Old road viewpoint",
            "Forest rest stop",
            "Regional history marker",
            "River bend lookout",
            "Local food stop",
            "Manor park",
            "Nature trail entrance",
            "Small town square",
            "Hill panorama",
            "Roadside chapel",
            "Lake picnic place",
            "Craft museum stop",
            "Scenic bridge",
            "Market street stop",
            "Castle road viewpoint",
            "Pine forest path",
            "Cultural center",
            "Historic mill place",
            "Countryside cafe",
            "Final approach lookout"
        };
        var types = new[] { "viewpoint", "park", "museum", "historic", "cafe" };
        var result = new List<PointOfInterest>();

        for (var i = 0; i < names.Length; i++)
        {
            var t = (i + 1d) / (names.Length + 1d);
            var sideOffset = i % 2 == 0 ? 0.012d : -0.012d;
            var latitude = cities.Start.Latitude + (cities.End.Latitude - cities.Start.Latitude) * t + sideOffset;
            var longitude = cities.Start.Longitude + (cities.End.Longitude - cities.Start.Longitude) * t - sideOffset;
            result.Add(createKnownPOI(names[i], types[i % types.Length], "Between cities", latitude, longitude));
        }

        return result;
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

    private static RouteMatrix createFallbackLengthMatrix(List<RoutePlanningPoint> points)
    {
        var durations = new List<List<double>>();
        var distances = new List<List<double>>();

        foreach (var from in points)
        {
            var durationRow = new List<double>();
            var distanceRow = new List<double>();
            foreach (var to in points)
            {
                var distance = calculateRoadDistanceMeters(from, to);
                distanceRow.Add(distance);
                durationRow.Add(distance == 0 ? 0 : distance / 70_000d * 3600d);
            }

            distances.Add(distanceRow);
            durations.Add(durationRow);
        }

        return new RouteMatrix(points, durations, distances);
    }

    private static RouteRoad createFallbackPolyLine(List<RoutePlanningPoint> selectedRoad)
    {
        var distance = 0d;
        for (var i = 0; i < selectedRoad.Count - 1; i++)
        {
            distance += calculateRoadDistanceMeters(selectedRoad[i], selectedRoad[i + 1]);
        }

        var coordinates = string.Join(",", selectedRoad.Select(point => $"[{point.Longitude.ToString(CultureInfo.InvariantCulture)},{point.Latitude.ToString(CultureInfo.InvariantCulture)}]"));
        var geoJson = $$"""{"type":"LineString","coordinates":[{{coordinates}}]}""";
        return new RouteRoad(selectedRoad, distance, distance / 70_000d * 3600d, geoJson);
    }

    private static double calculateRoadDistanceMeters(RoutePlanningPoint from, RoutePlanningPoint to)
    {
        const double earthRadiusMeters = 6_371_000;
        var dLat = degreesToRadians(to.Latitude - from.Latitude);
        var dLon = degreesToRadians(to.Longitude - from.Longitude);
        var lat1 = degreesToRadians(from.Latitude);
        var lat2 = degreesToRadians(to.Latitude);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var straightLine = 2 * earthRadiusMeters * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return straightLine * 1.25;
    }

    private static double degreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180d;
    }

    private static string evaluateRouteData(TripRoute route)
    {
        return string.IsNullOrWhiteSpace(route.TravelTime) ? "Unknown" : route.TravelTime;
    }
}

public record CheckedCities(GeoPoint Start, GeoPoint End);

public record RoutePlanningPoint(string Name, string City, double Latitude, double Longitude, PointOfInterest? PointOfInterest = null);

public record RouteMatrix(
    List<RoutePlanningPoint> Points,
    List<List<double>> Durations,
    List<List<double>> Distances,
    OsmRoadGraph? RoadGraph = null,
    List<long>? GraphNodeIds = null);

public record RouteRoad(List<RoutePlanningPoint> Points, double DistanceMeters, double DurationSeconds, string GeoJson);

public record RoutePlanningResult(TripRoute Route, List<PointOfInterest> ImportedPOI, RouteRoad Road);

public class OsmRoadGraph
{
    public Dictionary<long, RoadGraphNode> Nodes { get; } = new();
    public Dictionary<long, List<RoadGraphEdge>> Edges { get; } = new();
}

public record RoadGraphNode(long Id, double Latitude, double Longitude);

public record RoadGraphEdge(long To, double DistanceMeters);

public class RoadPoiRequest
{
    public string StartingCity { get; set; } = string.Empty;
    public string EndCity { get; set; } = string.Empty;
}
