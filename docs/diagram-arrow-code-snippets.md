# Diagramu rodykles su tikromis kodo eilutemis

Sita lentele papildo `docs/diagram-arrow-code-map.md`: cia prie kiekvienos sekos diagramos rodykles pridedama konkreti kodo eilute arba trumpas snippet. Jei rodykle yra UI lango parodymas, snippet yra JSX/modal state eilute. Jei rodykle yra isorinis aktorius, snippet rodo adapteri arba OSM/HTTP kvietima.

## Sukurti marsruta

| Nr. | Rodykle diagramoje | Tikra kodo eilute / snippet |
|---:|---|---|
| 1 | `openRouteCreate()` | `onClick={openRouteCreate}` ir `const openRouteCreate = async () => { ... }` |
| 2 | `openRouteCreation` | `await api.post('/Route/openRouteCreation').catch(() => undefined);` |
| 3 | `open()` | `setIsCreateOpen(true);` ir `aria-label="RouteCreate"` |
| 4 | `InitialCities()` | `<input value={startCity} ... />` ir `<input value={endCity} ... />` |
| 5 | `sendCities()` | `const response = await api.post<TravelRoute>('/Route/preview', buildRoutePayload());` |
| 6 | `checkCities()` | `var checkedCities = await checkCities(route, cancellationToken);` |
| 7 | `getNearbyPOI()` | `const response = await api.post<PointOfInterest[]>('/Route/roadPOI', { startingCity: startCity, endCity });` |
| 8 | `getNearbyPOI()` entity | `var osmData = await getOSMData(cities, cancellationToken);` |
| 9 | `Nearby POI data` | `return roadData;` |
| 10 | `nearby POI` | `setObjects(response.data);` |
| 11 | `POI data` | `{objects.map(object => { ... className={\`object-card ...\`} ... })}` |
| 12 | `ref Rasti lankytinus objektus` | `var routePlaces = await sendRoutePlaces(routeParts, cancellationToken);` |
| 13 | `POI data` | `<Marker ... eventHandlers={{ click: () => onToggleObject(pin.object.name) }}>` |
| 14 | `selectPOI()` | `const toggleObject = (name: string) => { setSelectedNames(...) }` |
| 15 | `selectedPOI()` | `const routePoints: RoutePoint[] = selectedObjects.map((object, index) => ({ ... }))` |
| 16 | `checkPOI()` | `private bool checkPOI(PointOfInterest poi) { return poi.checkPOI(); }` |
| 17 | `getOSMData()` | `using var response = await _httpClient.PostAsync(overpassUrl, content, cancellationToken);` |
| 18 | `cleanRoadData()` | `return pois.Where(checkPOI).OrderByDescending(poi => poi.Rating)...ToList();` |
| 19 | `evaluateRoads()` | `if (evaluateRoads(graph, swapped) < evaluateRoads(graph, road))` |
| 20 | `createGraph()` | `private List<List<double>> createGraph(RouteMatrix matrix) { return matrix.Durations; }` |
| 21 | `createLengthMatrix()` | `var roadGraph = await readOSMFile(points, cancellationToken);` |
| 22 | `selectObjects()` | `return road.Select(index => matrix.Points[index]).ToList();` |
| 23 | `calculateHeuristic()` | `var next = calculateHeuristic(graph[current], visited);` |
| 24 | `selectRoad()` | `var selectedRoad = selectRoad(matrix, road);` |
| 25 | `saveToLengthMatrix()` | `saveToLengthMatrix(durations);` |
| 26 | `createInitialRoute()` | `var initialRoad = createInitialRoute(graph);` |
| 27 | `shuffleObjectOrder()` | `var road = shuffleObjectOrder(graph, initialRoad);` |
| 28 | `Route data` | `return new RoutePlanningResult(route, selectedPois, fastestRoad);` |
| 29 | `Route` | `<RouteMap ... createdRoute={previewRoute} ... />` |
| 30 | `acceptRoute()` | `onClick={confirmAndSaveRoute}` |
| 31 | `saveRoute()` | `await api.post<TravelRoute>('/Route', routeToSave);` |
| 32 | `saveRouteData()` | `saveRouteData(route, fastestRoad, fastestRoad.Points);` |
| 33 | `route save status` | `await _context.SaveChangesAsync(cancellationToken);` |
| 34 | `route save message` | `setMessage('Route saved successfully.');` |
| 35 | `save success message` | `<h2>Route saved</h2>` |

## Rasti lankytinus objektus

| Nr. | Rodykle diagramoje | Tikra kodo eilute / snippet |
|---:|---|---|
| 1 | `separateParts()` | `var routeParts = await separateParts(request, cancellationToken);` |
| 2 | `sendRouteData()` | `[HttpPost("sendRouteData")] public async Task<ActionResult<IEnumerable<PointOfInterest>>> sendRouteData(...)` |
| 3 | `SendRoutePlaces` | `var routePlaces = await sendRoutePlaces(routeParts, cancellationToken);` |
| 4 | `Points of Interest nearby` | `routePlaces.AddRange(await getOSMData(routeParts.Start, cancellationToken));` |
| 5 | `filterPOI` | `var filtered = filterPOI(routePlaces, request.Type).Take(5).ToList();` |
| 6 | `savePOI()` | `await savePOI(filtered, cancellationToken);` |
| 7 | `POI save status` | `await _context.SaveChangesAsync(cancellationToken);` |
| 8 | `POI data` | `return filtered;` |

## Ivertinti keliones trukme

| Nr. | Rodykle diagramoje | Tikra kodo eilute / snippet |
|---:|---|---|
| 1 | `separateParts()` | `var routeObjects = separateParts(route, checkedCities);` |
| 2 | `sendRoute` | `var fastestRoad = await sendRoute(poiRoads, cancellationToken);` |
| 3 | `getRouteTime` | `var routeTime = getRouteTime(fastestRoad);` |
| 4 | `saveRouteTime()` | `saveRouteTime(route, routeTime);` |
| 5 | `route time saved` | `route.TravelTime = routeTime.ToString(@"hh\:mm", CultureInfo.InvariantCulture);` |

## Pasirinkti marsruta

| Nr. | Rodykle diagramoje | Tikra kodo eilute / snippet |
|---:|---|---|
| 1 | `openRouteSelection()` | `<select value={routeId} onChange={e => { setRouteId(e.target.value); ... }}>` |
| 2 | `openRoutes()` | `const getRoutes = () => api.get<TravelRoute[]>('/Route/openRoutes')` |
| 3 | `open()` | `<option value="">Select route</option>` ir route select renderis |
| 4 | `GetRoutes()` | `api.get<TravelRoute[]>('/Route/openRoutes')` |
| 5 | `getAllRoutes()` | `public async Task<ActionResult<IEnumerable<TripRoute>>> getAllRoutes()` |
| 6 | `routes` | `return await _context.Routes.Include(...).ToListAsync();` |
| 7 | `AllRoutes` | `.then(res => { setRoutes(res.data); ... })` |
| 8 | `selectRoute()` | `setRouteId(e.target.value);` |
| 9 | `getRoute()` | `const selectedRoute = useMemo(() => routes.find(route => String(route.id) === routeId) ?? null, ...)` |
| 10 | `getSpecificRoute()` | `return await getSpecificRoute(id);` |
| 11 | `route` | `FirstOrDefaultAsync(item => item.Id == id);` |
| 12 | `route data` | `setViewRoute(response.data);` |
| 13 | `selected route` | `{selectedRoute && (<div className="route-choice-card"> ... )}` |
| ref | `Sukurti marsruta` | `await api.post('/Route/openRouteCreation').catch(() => undefined);` |

## Perziureti marsrutus

| Nr. | Rodykle diagramoje | Tikra kodo eilute / snippet |
|---:|---|---|
| 1 | `showRoutes()` | `<NavLink to="/routes" className={navClass}>Routes</NavLink>` |
| 2 | `showRouteList` | `<Route path="/routes" element={<RoutesPage />} />` |
| 3 | `openRouteList()` | `<h2>Saved routes</h2>` |
| 4 | `GetRoutes()` | `const getRoutes = () => api.get<TravelRoute[]>('/Route/openRoutes')` |
| 5 | `getAllRoutes()` | `return await getAllRoutes();` |
| 6 | `Routes` | `return await _context.Routes.Include(...).ToListAsync();` |
| 7 | `Route list` | `.then(res => setRoutes(res.data))` |
| 8 | `selectRoute()` | `<button ... onClick={() => viewSelectedRoute(route)}>View</button>` |
| 9 | `getRoute()` | `const response = await api.get<TravelRoute>(\`/Route/openRouteView/${route.id}\`);` |
| 10 | `openRouteView` | `aria-label="RouteView"` |
| 11 | `getRoute()` | `[HttpGet("openRouteView/{id}")] public async Task<ActionResult<TripRoute>> openRouteView(int id)` |
| 12 | `get()` | `return await getSpecificRoute(id);` |
| 13 | `route` | `var route = await _context.Routes.Include(...).FirstOrDefaultAsync(item => item.Id == id);` |
| 14 | `route` | `setViewRoute(response.data);` |
| ref | `Redaguoti marsruta` | `<button className="btn btn-primary" onClick={() => editRoute(viewRoute)}>Edit route</button>` |

## Redaguoti marsruta

| Nr. | Rodykle diagramoje | Tikra kodo eilute / snippet |
|---:|---|---|
| 1 | `openRouteEdit()` | `onClick={() => editRoute(viewRoute)}` |
| 2 | `openRoutePage()` | `const response = route.id ? await api.get<TravelRoute>(\`/Route/openRouteEdit/${route.id}\`) : { data: route };` |
| 3 | `open()` | `setIsCreateOpen(true);` |
| 4 | `getRoute()` | `api.get<TravelRoute>(\`/Route/openRouteEdit/${route.id}\`)` |
| 5 | `getSpecificRoute()` | `[HttpGet("openRouteEdit/{id}")] public async Task<ActionResult<TripRoute>> openRouteEdit(int id)` |
| 6 | `specific Route` | `return await getSpecificRoute(id);` |
| 7 | `specific Route data` | `const routeForEdit = response.data;` |
| 8 | `getRoutePOI()` | `[HttpGet("{id}/getRoutePOI")] public async Task<ActionResult<IEnumerable<RoutePoint>>> getRoutePOI(int id)` |
| 9 | `getRoutePOI()` entity | `return route.RoutePoints.OrderBy(point => point.Order).ToList();` |
| 10 | `route POI data` | `route.routePoints?.map(point => ({ name: point.name, ... }))` |
| 11 | `route POI information` | `const routePointObjects = routePointsToObjects(routeForEdit);` |
| 12 | `useKnownPOI()` | `setSelectedNames(routeForEdit.routePoints?.map(point => point.name) ?? []);` |
| 13 | `getPOI()` | `const poiResponse = await api.post<PointOfInterest[]>('/Route/roadPOI', { startingCity: routeForEdit.startingCity, endCity: routeForEdit.endCity });` |
| 14 | `getNearbyPOI()` | `public async Task<ActionResult<IEnumerable<PointOfInterest>>> getRoadPOI(...)` |
| 15 | `POI` | `var osmData = await getOSMData(cities, cancellationToken);` |
| 16 | `POI data` | `setObjects([...routePointObjects, ...poiResponse.data].filter(...));` |
| 17 | `POI information` | `{objects.map(object => (<button ... className={\`object-card ...\`}> ... ))}` |
| 18 | `requestNewPOI()` | `<button ... onClick={findObjects} ...>Reload POIs</button>` |
| 19 | `getNewPOI()` | `const response = await api.post<PointOfInterest[]>('/Route/roadPOI', { startingCity: startCity, endCity });` |
| 20 | `POI data` | `setObjects(response.data);` |
| 21 | `POI information` | `<RouteMap ... objects={objects} ... />` |
| 22 | `sendUpdatedRoute()` | `<form onSubmit={previewCalculatedRoute} ...>` |
| 23 | `sendRouteData()` | `const response = await api.post<TravelRoute>('/Route/preview', buildRoutePayload());` |
| 24 | `updateRoute()` | `var fastestRoad = await sendRoute(poiRoads, cancellationToken);` |
| 25 | `routeData` | `var roadGraph = await readOSMFile(points, cancellationToken);` |
| 26 | `Updated route` | `var geoJson = $$"""{"type":"LineString","coordinates":[{{string.Join(",", coordinates)}}]}""";` |
| 27 | `updated Route information` | `return Task.FromResult(new RouteRoad(selectedRoad, distance, distance / 60_000d * 3600d, geoJson));` |
| 28 | `createPolyline()` | `private Task<RouteRoad> createPolyLine(...)` |
| 29 | `Updated Route` | `setPreviewRoute(response.data);` |
| 30 | `Show updated route` | `aria-label="Route preview"` |
| 31 | `confirmEdit()` | `onClick={confirmAndSaveRoute}` |
| 32 | `saveRoute()` | `await api.put(\`/Route/${editingRouteId}\`, routeToSave)` |
| 33 | `saveRouteData()` | `existingRoute.RoutePoints = route.RoutePoints.Select((point, index) => new RoutePoint { ... }).ToList();` |
| 34 | `Route save status` | `await _context.SaveChangesAsync(cancellationToken);` |
| 35 | `Save status` | `return Ok(new { message = "Route save status", route.Id });` |
| 36 | `Save success` | `setShowSuccess(true);` |

## Valdyti keliones

| Nr. | Rodykle diagramoje | Tikra kodo eilute / snippet |
|---:|---|---|
| 1 | `openTripList()` | `<NavLink to="/trips" className={navClass}>Trips</NavLink>` |
| 2 | `openTripListPage()` | `openTripList(); openTripListPage();` |
| 3 | `open()` | `<section className="planner-hero trip-selection-hero">` |
| 4 | `getTrips()` | `const getTrips = () => api.get<Trip[]>('/Trip')` |
| 5 | `getTripData()` | `return await _context.Trips.Include(trip => trip.Route).ToListAsync();` |
| 6 | `Trips` | `return await getTripsData();` |
| 7 | `Trip information` | `setTrips(res.data);` |
| 8 | `openTrip()` | `<button ... onClick={() => openTripView(trip)}>View</button>` |
| 9 | `openTripPage()` | `const response = await api.post<Trip>(\`/Trip/${trip.id}/openTripPage\`);` |
| 10 | `open()` | `aria-label="TripView"` |
| 11 | `getTrip()` | `[HttpPost("{id}/openTripPage")] public async Task<ActionResult<Trip>> openTripPage(int id)` |
| 12 | `getTripData()` | `return await getTripData(id);` |
| 13 | `trip data` | `var trip = await _context.Trips.Include(item => item.Route).FirstOrDefaultAsync(item => item.Id == id);` |
| 14 | `trip information` | `setViewTrip(response.data);` |
| 15 | `openTripEdit()` | `<button ... onClick={() => openTripEdit(trip)}>Edit</button>` |
| 16 | `openTripEditPage()` | `api.post<Trip>(\`/Trip/${trip.id}/openTripEditPage\`)` |
| 17 | `open()` | `aria-label="TripEdit"` |
| 18 | `getTrip()` | `[HttpPost("{id}/openTripEditPage")] public async Task<ActionResult<Trip>> openTripEditPage(int id)` |
| 19 | `getTripData()` | `return await getTripData(id);` |
| 20 | `trip data` | `FirstOrDefaultAsync(item => item.Id == id);` |
| 21 | `trip information` | `setEditingTrip(response.data);` |
| 22 | `editTrip()` | `<form onSubmit={saveTripEdit}>` |
| 23 | `updateTrip()` | `await api.put(\`/Trip/${editingTrip.id}\`, payload);` |
| 24 | `checkTrip()` | `if (!checkTrip(trip)) { return BadRequest("Trip edit data is not valid."); }` |
| 25 | `saveTrip()` | `_context.Entry(trip).State = EntityState.Modified;` |
| 26 | `save status` | `await _context.SaveChangesAsync();` |
| 27 | `save information` | `return Ok(new { message = "save information", trip.Id });` |
| 28 | `edit success message` | `setMessage('Trip edit saved.');` |
| 29 | `deleteTrip()` | `<button ... onClick={() => requestDeleteTrip(trip)}>Delete</button>` |
| 30 | `delete confirmation modal` | `setDeleteCandidate(trip);` |
| 31 | `confirmDelete()` | `<button ... onClick={confirmDeleteTrip}>Delete</button>` |
| 32 | `deleteTripData()` | `await api.delete(\`/Trip/${deleteCandidate.id}\`);` |
| 33 | `removeTrip()` | `removeTrip(trip);` |
| 34 | `removal status` | `await _context.SaveChangesAsync();` |
| 35 | `removal status information` | `return Ok(new { message = "removal status information", id });` |
| 36 | `removal success message` | `await getTrips();` |
| ref | `Valdyti keliones rezervacijas` | `api.get<Reservation[]>(\`/Payment/trip/${selectedTripId}/reservations\`)` |
| 37 | `openTripCreate()` | `const openTripCreate = () => api.post('/Trip/openTripCreate').catch(() => undefined);` |
| 38 | `openTripCreatePage()` | `await openTripCreatePage();` |
| 39 | `open()` | `<form className="trip-selection-panel" onSubmit={createTripFromSelection}>` |
| ref | `Pasirinkti marsruta` | `<select value={routeId} ...>` |
| ref | `Pasirinkti apgyvendinima is saraso` | `api.post<TravelOffer[]>('/Trip/accommodation/list', payload)` |
| ref | `Pasirinkti automobili is saraso` | `api.post<TravelOffer[]>('/Trip/car/list', payload)` |
| ref | `Pasirinkti skrydi is saraso` | `api.post<TravelOffer[]>('/Trip/flight/list', payload)` |
| 40 | `saveTrip()` | `<button ... type="submit">Create trip from selection</button>` |
| 41 | `saveTripInformation()` | `const tripResponse = await api.post<Trip>('/Trip', { ... });` |
| 42 | `checkTrip()` | `if (!checkTrip(trip)) { return BadRequest("Trip data is not valid."); }` |
| 43 | `saveTripData()` | `_context.Trips.Add(trip);` |
| 44 | `save status` | `await _context.SaveChangesAsync();` |
| 45 | `save status information` | `return CreatedAtAction(nameof(getTripData), new { id = trip.Id }, trip);` |
| 46 | `successful save message` | `setMessage('Trip created from selected route.');` |

## Apmoketi kelione

| Nr. | Rodykle diagramoje | Tikra kodo eilute / snippet |
|---:|---|---|
| 1 | `openTripPayment()` | `const openTripPayment = () => api.post('/Payment/openTripPayment').catch(() => undefined);` |
| 2 | `openTripPayments()` | `const openTripPayments = () => api.post('/Payment/openTripPayments').catch(() => undefined);` |
| 3 | `open()` | `<Route path="/payments" element={<PaymentsPage />} />` |
| 4 | `getTripReservations()` | `api.get<Reservation[]>(\`/Payment/trip/${selectedTripId}/reservations\`)` |
| 5 | `getReservations()` | `return await _context.Reservations.Where(item => item.TripId == tripId).ToListAsync();` |
| 6 | `reservations` | `.then(res => setReservations(res.data))` |
| 7 | `reservation data` | `{reservations.map(reservation => (<article ...> ... ))}` |
| 8 | `payReservation()` | `<button ... onClick={() => processPayment(payment.id)}>Pay</button>` |
| 9 | `requestPayment()` | `const pageResponse = await api.post<PaymentPageResponse>(\`/Payment/${id}/requestPaymentPage\`);` |
| 10 | `requestPaymentPage()` | `[HttpPost("{id}/requestPaymentPage")] public async Task<ActionResult<PaymentPageResponse>> requestPaymentPage(...)` |
| 11 | `getPaymentPage` | `return getPaymentPage(payment, cancellationToken);` |
| 12 | `payment_page` | `var paymentPage = string.IsNullOrWhiteSpace(bankUrl) ? "local-banking-adapter://payment" : ...;` |
| 13 | `payment_page` | `return Task.FromResult(new PaymentPageResponse(...));` |
| 14 | `payment page` | `setPaymentPage(pageResponse.data);` |
| 15 | `processPayment()` | `await api.post(\`/Payment/${id}/processPayment\`);` |
| 16 | `processPayment()` backend | `var result = await processPayment(payment, "payment", cancellationToken);` |
| 17 | `paymentData` | `var payload = JsonSerializer.Serialize(new { payment.Id, payment.Amount, ... });` |
| 18 | `payment result` | `return new PaymentProcessResult(true, $"Banking service accepted {operation}");` |
| 19 | `payment result` | `return new PaymentProcessResult(true, $"Local banking adapter accepted {operation}");` |
| 20 | `payment result` | `return Ok(new { message = result.Message, payment.PaymentStatus });` |
| 21 | `savePayment()` | `await api.post('/Payment', { tripId: Number(tripId), reservationId: reservation.data.id, ... });` |
| 22 | `savePaymentData()` | `savePaymentData(payment);` |
| 23 | `payment save status` | `_context.Payments.Add(payment); await _context.SaveChangesAsync();` |
| 24 | `Payment result` | `return CreatedAtAction(nameof(getPayment), new { id = payment.Id }, payment);` |
| 25 | `Payment result message` | `setMessage(\`${pageResponse.data.message}. Payment completed.\`);` |

