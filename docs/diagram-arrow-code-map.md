# Diagramu rodykliu ir kodo lentele

Lentele sudaryta pagal pateiktas sekos diagramas. Jei diagramoje nurodytas `Google maps`, kode tai igyvendinta per OSM/Overpass/Nominatim/Leaflet arba lokalus fallback, nes projekte pagrindinis marsruto skaiciavimas remiasi OSM grafu.

Konkrecios kodo eilutes/snippet'ai kiekvienai rodyklei yra faile `docs/diagram-arrow-code-snippets.md`.

## Sukurti marsruta

| Nr. | Diagramoje rodoma rodykle | Kodo vieta | Atitiktis |
|---:|---|---|---|
| 1 | `User -> RouteList: openRouteCreate()` | `frontend/src/Views/Travel/RoutesPage.tsx:187` `openRouteCreate` | Tikslu |
| 2 | `RouteList -> ReactController: openRouteCreation` | `RoutesPage.tsx:188`, `backend/Controllers/RouteController.cs:103` `openRouteCreation` | Tikslu |
| 3 | `ReactController -> RouteCreate: open()` | `RoutesPage.tsx:190`, modalas `RouteCreate` `RoutesPage.tsx:246` | UI modalas |
| 4 | `User -> RouteCreate: InitialCities()` | `RoutesPage.tsx:256` start/end city inputai | UI forma |
| 5 | `RouteCreate -> RouteController: sendCities()` | `RoutesPage.tsx:93`, `RouteController.previewRoute` `RouteController.cs:110` -> `sendCities` `RouteController.cs:342` | Tikslu |
| 6 | `RouteController: checkCities()` | `RouteController.cs:363` `checkCities` | Tikslu |
| 7 | `RouteController -> POIController: getNearbyPOI()` | `RoutesPage.findObjects` `RoutesPage.tsx:46`, `RouteController.getRoadPOI` `RouteController.cs:312` | Pavadinimas kitoks, funkcija ta pati |
| 8 | `POIController -> PointOfInterest: getNearbyPOI()` | `POIController.sendRouteData` `backend/Controllers/POIController.cs:97`, `sendRoutePlaces` `POIController.cs:172` | Papildomas diagraminis endpointas |
| 9 | `PointOfInterest -> POIController: Nearby POI data` | `POIController.filterPOI` `POIController.cs:187`, `savePOI` `POIController.cs:200` | Tikslu |
| 10 | `POIController -> ReactController: nearby POI` | `RouteController.getRoadPOI` grazina POI `RouteController.cs:312`, frontend gauna `RoutesPage.tsx:49` | Tikslu |
| 11 | `ReactController -> User: POI data` | POI korteles ir zemelapio markeriai `RoutesPage.tsx:283`, `RouteMap.tsx:112` | Tikslu |
| 12 | `ref Rasti lankytinus objektus` | `docs/diagram-arrow-code-map.md` skyrius `Rasti lankytinus objektus` | Istrauktas i atskira lentele |
| 13 | `RouteCreate -> User: POI data` | `RoutesPage.tsx:283` objektu sarasas, `RouteMap.tsx:112` markeriai | Tikslu |
| 14 | `User -> RouteCreate: selectPOI()` | `RoutesPage.toggleObject` `RoutesPage.tsx:58`, `RouteMap` marker click `RouteMap.tsx:119` | Tikslu |
| 15 | `RouteCreate -> RouteController: selectedPOI()` | Payload `routePoints` `RoutesPage.tsx:72`, `previewCalculatedRoute` `RoutesPage.tsx:93` | Tikslu |
| 16 | `RouteController: checkPOI()` | `RouteController.cs:406` `checkPOI` | Tikslu |
| 17 | `RouteController: getOSMData()` | `RouteController.cs:433` `getOSMData` | Tikslu |
| 18 | `RouteController: cleanRoadData()` | `RouteController.cs:482` `cleanRoadData` | Tikslu |
| 19 | `RouteController: evaluateRoads()` | `RouteController.cs:586` `calculateHeuristic`, `RouteController.cs:644` `shuffleObjectOrder` | Artimiausia realizacija |
| 20 | `RouteController: createGraph()` | `RouteController.cs:526`, `RouteController.cs:538` | Tikslu |
| 21 | `RouteController: createLengthMatrix()` | `RouteController.cs:543` | Tikslu |
| 22 | `RouteController: selectObjects()` | `RouteController.cs:413` `selectObjects`, `RouteController.cs:409` `separateParts` | Tikslu |
| 23 | `RouteController: calculateHeuristic()` | `RouteController.cs:586` | Tikslu |
| 24 | `RouteController: selectRoad()` | `RouteController.cs:657` `selectRoad` | Tikslu |
| 25 | `RouteController: saveToLengthMatrix()` | `RouteController.cs:662` `saveToLengthMatrix` | Tikslu |
| 26 | `RouteController: createInitialRoute()` | `RouteController.cs:608` | Tikslu |
| 27 | `RouteController: shuffleObjectOrder()` | `RouteController.cs:644` | Tikslu |
| 28 | `RouteController -> ReactController: Route data` | `previewRoute` atsakymas `RoutesPage.tsx:97` | Tikslu |
| 29 | `ReactController -> User: Route` | Preview modalas `RoutesPage.tsx:355`, `RouteMap.tsx:96` | Tikslu |
| 30 | `User -> RouteCreate: acceptRoute()` | `confirmAndSaveRoute` `RoutesPage.tsx:111`, mygtukas `RoutesPage.tsx:374` | Tikslu |
| 31 | `RouteCreate -> RouteController: saveRoute()` | `RoutesPage.tsx:122`, `RouteController.saveRoute` `RouteController.cs:132` | Tikslu |
| 32 | `RouteController -> Route: saveRouteData()` | `RouteController.cs:762` `saveRouteData` arba `cleanConfirmedRouteData` `RouteController.cs:395` | Tikslu |
| 33 | `Route -> RouteController: route save status` | EF `SaveChangesAsync` `RouteController.cs:164`, response `CreatedAtAction` `RouteController.cs:171` | Tikslu |
| 34 | `RouteController -> RouteCreate: route save message` | `RoutesPage.tsx:129` message | Tikslu |
| 35 | `RouteCreate -> User: save success message` | Success modalas `RoutesPage.tsx:386` | Tikslu |

## Rasti lankytinus objektus

| Nr. | Diagramoje rodoma rodykle | Kodo vieta | Atitiktis |
|---:|---|---|---|
| 1 | `RouteController: separateParts()` | `RouteController.cs:409`, `POIController.cs:165` | Tikslu |
| 2 | `RouteController -> POIController: sendRouteData()` | `POIController.sendRouteData` `POIController.cs:97` | Tikslu |
| 3 | `POIController -> Google maps: SendRoutePlaces` | `POIController.sendRoutePlaces` `POIController.cs:172`, `getOSMData` `POIController.cs:234` | Google Maps pakeista OSM |
| 4 | `Google maps -> POIController: Points of Interest nearby` | `getOSMData` grazina POI `POIController.cs:234` | OSM atsakymas |
| 5 | `POIController: filterPOI` | `POIController.cs:187` | Tikslu |
| 6 | `POIController -> PointOfInterest: savePOI()` | `POIController.cs:200` | Tikslu |
| 7 | `PointOfInterest -> POIController: POI save status` | `SaveChangesAsync` `POIController.cs:217` | Tikslu |
| 8 | `POIController -> RouteController: POI data` | `sendRouteData` return `POIController.cs:109` | Tikslu |

## Ivertinti keliones trukme

| Nr. | Diagramoje rodoma rodykle | Kodo vieta | Atitiktis |
|---:|---|---|---|
| 1 | `RouteController: separateParts()` | `RouteController.cs:409` | Tikslu |
| 2 | `RouteController -> Google maps: sendRoute` | `RouteController.sendRoute` `RouteController.cs:533`, OSM grafas `readOSMFile` `RouteController.cs:831` | Google Maps pakeista OSM grafu |
| 3 | `Google maps -> RouteController: getRouteTime` | `RouteController.getRouteTime` `RouteController.cs:752`, grafas grazina `DurationSeconds` | Tikslu, saltinis OSM |
| 4 | `RouteController -> Route: saveRouteTime()` | `RouteController.cs:757` | Tikslu |
| 5 | `Route -> RouteController: route time saved` | `route.TravelTime` nustatomas `RouteController.cs:759`, saugoma `saveRouteData`/EF | Tikslu |

## Pasirinkti marsruta

| Nr. | Diagramoje rodoma rodykle | Kodo vieta | Atitiktis |
|---:|---|---|---|
| 1 | `User -> TripCreate: openRouteSelection()` | `TripsPage` route select UI `frontend/src/Views/Travel/TripsPage.tsx:211` | UI dalis TripCreate puslapyje |
| 2 | `TripCreate -> ReactController: openRoutes()` | `TripsPage.getRoutes` `TripsPage.tsx:50`, `/Route/openRoutes` | Tikslu |
| 3 | `ReactController -> RouteSelect: open()` | `TripsPage` route select render `TripsPage.tsx:214` | UI select vietoj atskiro failo |
| 4 | `RouteSelect -> RouteController: GetRoutes()` | `api.get('/Route/openRoutes')` `TripsPage.tsx:50` | Tikslu |
| 5 | `RouteController -> Route: getAllRoutes()` | `RouteController.openRoutes` `RouteController.cs:39`, `getAllRoutes` `RouteController.cs:45` | Tikslu |
| 6 | `Route -> RouteController: routes` | EF query `RouteController.cs:47` | Tikslu |
| 7 | `RouteController -> RouteSelect: AllRoutes` | `TripsPage.tsx:51` setRoutes | Tikslu |
| 8 | `User -> RouteSelect: selectRoute()` | `<select>` `TripsPage.tsx:214` | Tikslu |
| 9 | `RouteSelect -> RouteController: getRoute()` | Pasirinktas route naudojamas `selectedRoute` `TripsPage.tsx:21`; detaliam view naudojama `/Route/openRouteView` `RoutesPage.tsx:152` | Dalinai, pasirinkimui uztenka listo |
| 10 | `RouteController -> Route: getSpecificRoute()` | `RouteController.getSpecificRoute` `RouteController.cs:72` | Tikslu |
| 11 | `Route -> RouteController: route` | EF query `RouteController.cs:74` | Tikslu |
| 12 | `RouteController -> RouteSelect: route data` | `RoutesPage.viewSelectedRoute` `RoutesPage.tsx:150` | Tikslu route view sraute |
| 13 | `RouteSelect -> User: selected route` | `selectedRoute` card `TripsPage.tsx:226` | Tikslu |
| ref | `Sukurti marsruta` | `RoutesPage.openRouteCreate` `RoutesPage.tsx:187` | Tikslu |

## Perziureti marsrutus

| Nr. | Diagramoje rodoma rodykle | Kodo vieta | Atitiktis |
|---:|---|---|---|
| 1 | `User -> Main: showRoutes()` | Navigacija i `/routes` `frontend/src/App.tsx:26` | Tikslu |
| 2 | `Main -> ReactController: showRouteList` | `RoutesPage` uzkraunamas route list | UI routing |
| 3 | `ReactController -> RouteList: openRouteList()` | `RoutesPage` route list render `RoutesPage.tsx:209` | UI |
| 4 | `RouteList -> RouteController: GetRoutes()` | `RoutesPage.getRoutes` `RoutesPage.tsx:39` | Tikslu |
| 5 | `RouteController -> Route: getAllRoutes()` | `RouteController.cs:45` | Tikslu |
| 6 | `Route -> RouteController: Routes` | EF query `RouteController.cs:47` | Tikslu |
| 7 | `RouteController -> RouteList: Route list` | `setRoutes` `RoutesPage.tsx:40` | Tikslu |
| 8 | `User -> RouteList: selectRoute()` | View mygtukas `RoutesPage.tsx:234` | Tikslu |
| 9 | `RouteList -> RouteController: getRoute()` | `viewSelectedRoute` `RoutesPage.tsx:150` | Tikslu |
| 10 | `ReactController -> RouteView: openRouteView` | RouteView modalas `RoutesPage.tsx:319` | Tikslu |
| 11 | `RouteView -> RouteController: getRoute()` | `api.get('/Route/openRouteView/{id}')` `RoutesPage.tsx:152` | Tikslu |
| 12 | `RouteController -> Route: get()` | `getSpecificRoute` `RouteController.cs:72` | Tikslu |
| 13 | `Route -> RouteController: route` | EF query `RouteController.cs:74` | Tikslu |
| 14 | `RouteController -> RouteView: route` | `setViewRoute` `RoutesPage.tsx:153`, modal `RoutesPage.tsx:319` | Tikslu |
| ref | `Redaguoti marsruta` | `editRoute` `RoutesPage.tsx:157` | Tikslu |

## Redaguoti marsruta

| Nr. | Diagramoje rodoma rodykle | Kodo vieta | Atitiktis |
|---:|---|---|---|
| 1 | `User -> RouteView: openRouteEdit()` | Edit mygtukas `RoutesPage.tsx:343` | Tikslu |
| 2 | `RouteView -> ReactController: openRoutePage()` | `editRoute` `RoutesPage.tsx:157` | Tikslu |
| 3 | `ReactController -> RouteEdit: open()` | `setIsCreateOpen(true)` `RoutesPage.tsx:162`, RouteCreate/Edit modalas `RoutesPage.tsx:246` | UI modalas |
| 4 | `RouteEdit -> RouteController: getRoute()` | `/Route/openRouteEdit/{id}` `RoutesPage.tsx:159` | Tikslu |
| 5 | `RouteController -> Route: getSpecificRoute()` | `RouteController.cs:66`, `RouteController.cs:72` | Tikslu |
| 6 | `Route -> RouteController: specific Route` | EF query `RouteController.cs:74` | Tikslu |
| 7 | `RouteController -> RouteEdit: specific Route data` | `routeForEdit` `RoutesPage.tsx:161` | Tikslu |
| 8 | `RouteEdit -> POIController: getRoutePOI()` | `RouteController.getRoutePOI` `RouteController.cs:88` yra endpointas; UI naudoja route.routePoints | Dalinai, duomenys jau ateina su route |
| 9 | `POIController -> PointOfInterest: getRoutePOI()` | `RouteController.getRoutePOI` grazina `RoutePoint`, ne POI entity | Skirtumas del modelio |
| 10 | `PointOfInterest -> POIController: route POI data` | `route.routePoints` `RouteController.cs:76` | Tikslu pagal duomenis |
| 11 | `POIController -> RouteEdit: route POI information` | `routePointObjects` `RoutesPage.tsx:175` | Tikslu |
| 12 | `User -> RouteEdit: useKnownPOI()` | Esami `route.routePoints` pazymimi `RoutesPage.tsx:170` | Tikslu |
| 13 | `RouteEdit -> POIController: getPOI()` | `api.post('/Route/roadPOI')` `RoutesPage.tsx:174` | Per RouteController |
| 14 | `POIController -> PointOfInterest: getNearbyPOI()` | `RouteController.getRoadPOI` `RouteController.cs:312` | Per RouteController |
| 15 | `PointOfInterest -> POIController: POI` | `getOSMData` `RouteController.cs:433` | OSM duomenys |
| 16 | `POIController -> RouteEdit: POI data` | `poiResponse.data` `RoutesPage.tsx:175` | Tikslu |
| 17 | `RouteEdit -> User: POI information` | POI korteles `RoutesPage.tsx:283` | Tikslu |
| 18 | `User -> RouteEdit: requestNewPOI()` | Reload POIs mygtukas `RoutesPage.tsx:266` | Tikslu |
| 19 | `RouteEdit -> RouteController: getNewPOI()` | `findObjects` `RoutesPage.tsx:46` | Tikslu |
| 20 | `RouteController -> RouteEdit: POI data` | `setObjects` `RoutesPage.tsx:50` | Tikslu |
| 21 | `RouteEdit -> User: POI information` | POI sarasas/zemelapis `RoutesPage.tsx:283`, `RouteMap.tsx:112` | Tikslu |
| 22 | `User -> RouteEdit: sendUpdatedRoute()` | `previewCalculatedRoute` `RoutesPage.tsx:93` | Tikslu |
| 23 | `RouteEdit -> RouteController: sendRouteData()` | `api.post('/Route/preview')` `RoutesPage.tsx:98` | Preview pries save |
| 24 | `RouteController -> GoogleMapsBoundary: updateRoute()` | `sendRoute` `RouteController.cs:533`, OSM grafas `RouteController.cs:831` | Google Maps pakeista OSM |
| 25 | `GoogleMapsBoundary -> Google maps: routeData` | `readOSMFile` `RouteController.cs:831` | OSM/Overpass |
| 26 | `Google maps -> GoogleMapsBoundary: Updated route` | `createPolyLine` `RouteController.cs:691` | OSM polyline |
| 27 | `GoogleMapsBoundary -> RouteController: updated Route information` | `RouteRoad` grazinamas is `sendRoute` `RouteController.cs:533` | Tikslu |
| 28 | `RouteController: createPolyline()` | `RouteController.cs:691` `createPolyLine` | Tikslu |
| 29 | `RouteController -> RouteEdit: Updated Route` | `setPreviewRoute` `RoutesPage.tsx:100` | Tikslu |
| 30 | `RouteEdit -> User: Show updated route` | Preview modalas `RoutesPage.tsx:355` | Tikslu |
| 31 | `User -> RouteEdit: confirmEdit()` | Confirm route mygtukas `RoutesPage.tsx:374` | Tikslu |
| 32 | `RouteEdit -> RouteController: saveRoute()` | PUT `/Route/{id}` `RoutesPage.tsx:122`, `RouteController.sendRouteData` `RouteController.cs:176` | Tikslu |
| 33 | `RouteController -> Route: saveRouteData()` | `cleanConfirmedRouteData` `RouteController.cs:395`, routePoints replace `RouteController.cs:215` | Tikslu |
| 34 | `Route -> RouteController: Route save status` | `SaveChangesAsync` `RouteController.cs:229` | Tikslu |
| 35 | `RouteController -> RouteEdit: Save status` | response OK `RouteController.cs:241` | Tikslu |
| 36 | `RouteEdit -> User: Save success` | Success modalas/message `RoutesPage.tsx:126` ir `RoutesPage.tsx:386` | Tikslu |

## Valdyti keliones

| Nr. | Diagramoje rodoma rodykle | Kodo vieta | Atitiktis |
|---:|---|---|---|
| 1 | `User -> Main: openTripList()` | Navigacija `/trips` `frontend/src/App.tsx:25` | Tikslu |
| 2 | `Main -> ReactController: openTripListPage()` | `TripsPage.useEffect` `TripsPage.tsx:184` | Tikslu |
| 3 | `ReactController -> TripList: open()` | Trips page render `TripsPage.tsx:198` | UI |
| 4 | `TripList -> TripController: getTrips()` | `getTrips` `TripsPage.tsx:42` | Tikslu |
| 5 | `TripController -> Trip: getTripData()` | `TripController.getTripsData` `TripController.cs:44` | Tikslu |
| 6 | `Trip -> TripController: Trips` | EF query `TripController.cs:46` | Tikslu |
| 7 | `TripController -> TripList: Trip information` | `setTrips` `TripsPage.tsx:44` | Tikslu |
| 8 | `User -> TripList: openTrip()` | View mygtukas `TripsPage.tsx:314` | Tikslu |
| 9 | `TripList -> ReactController: openTripPage()` | `openTripView` `TripsPage.tsx:137` | Tikslu |
| 10 | `ReactController -> TripView: open()` | TripView modalas `TripsPage.tsx:326` | UI modalas |
| 11 | `TripView -> TripController: getTrip()` | `/Trip/{id}/openTripPage` `TripsPage.tsx:139` | Tikslu |
| 12 | `TripController -> Trip: getTripData()` | `TripController.cs:62`, `TripController.cs:80` | Tikslu |
| 13 | `Trip -> TripController: trip data` | EF query `TripController.cs:82` | Tikslu |
| 14 | `TripController -> TripView: trip information` | `setViewTrip` `TripsPage.tsx:140` | Tikslu |
| 15 | `User -> TripList: openTripEdit()` | Edit mygtukas `TripsPage.tsx:315` | Tikslu |
| 16 | `TripList -> ReactController: openTripEditPage()` | `openTripEdit` `TripsPage.tsx:143` | Tikslu |
| 17 | `ReactController -> TripEdit: open()` | TripEdit modalas `TripsPage.tsx:355` | UI modalas |
| 18 | `TripEdit -> TripController: getTrip()` | `/Trip/{id}/openTripEditPage` `TripsPage.tsx:145` | Tikslu |
| 19 | `TripController -> Trip: getTripData()` | `TripController.cs:74`, `TripController.cs:80` | Tikslu |
| 20 | `Trip -> TripController: trip data` | EF query `TripController.cs:82` | Tikslu |
| 21 | `TripController -> TripEdit: trip information` | `setEditingTrip` `TripsPage.tsx:146` | Tikslu |
| 22 | `User -> TripEdit: editTrip()` | TripEdit forma `TripsPage.tsx:364` | Tikslu |
| 23 | `TripEdit -> TripController: updateTrip()` | `saveTripEdit` `TripsPage.tsx:149`, `TripController.updateTrip` `TripController.cs:120` | Tikslu |
| 24 | `TripController: checkTrip()` | `TripController.cs:226` `checkTrip` | Tikslu |
| 25 | `TripController -> Trip: saveTrip()` | `_context.Entry(trip).State` `TripController.cs:132` | Tikslu |
| 26 | `Trip -> TripController: save status` | `SaveChangesAsync` `TripController.cs:133` | Tikslu |
| 27 | `TripController -> TripEdit: save information` | response `TripController.cs:135` | Tikslu |
| 28 | `TripEdit -> User: edit success message` | `setMessage('Trip edit saved.')` `TripsPage.tsx:155` | Tikslu |
| 29 | `User -> TripList: deleteTrip()` | Delete mygtukas `TripsPage.tsx:317` | Tikslu |
| 30 | `TripList -> User: delete confirmation modal` | `requestDeleteTrip` `TripsPage.tsx:160`, modal `TripsPage.tsx:395` | Tikslu |
| 31 | `User -> TripList: confirmDelete()` | `confirmDeleteTrip` `TripsPage.tsx:166` | Tikslu |
| 32 | `TripList -> TripController: deleteTripData()` | `api.delete('/Trip/{id}')` `TripsPage.tsx:168`, `TripController.cs:139` | Tikslu |
| 33 | `TripController -> Trip: removeTrip()` | `removeTrip` `TripController.cs:231` | Tikslu |
| 34 | `Trip -> TripController: removal status` | `SaveChangesAsync` `TripController.cs:148` | Tikslu |
| 35 | `TripController -> TripList: removal status information` | response `TripController.cs:150` | Tikslu |
| 36 | `TripList -> User: removal success message` | `getTrips` po delete `TripsPage.tsx:170` | Dalinai, nera atskiro teksto |
| ref | `Valdyti keliones rezervacijas` | Rezervacijos per `PaymentsPage` ir `PaymentController.getTripReservations` `PaymentController.cs:36` | Atskiras payment/reservation ekranas |
| 37 | `User -> TripList: openTripCreate()` | Create forma `TripsPage.tsx:211`, submit `TripsPage.tsx:100` | Tikslu |
| 38 | `TripList -> ReactController: openTripCreatePage()` | `openTripCreate`/`openTripCreatePage` `TripsPage.tsx:36`, `TripsPage.tsx:111` | Tikslu |
| 39 | `ReactController -> TripCreate: open()` | Trip setup forma `TripsPage.tsx:211` | UI forma |
| ref | `Pasirinkti marsruta` | Route select `TripsPage.tsx:214` | Tikslu |
| ref | `Pasirinkti apgyvendinima is saraso` | Hotels tab `TripsPage.tsx:291`, backend `TripController.cs:181` | Tikslu |
| ref | `Pasirinkti automobili is saraso` | Cars tab `TripsPage.tsx:293`, backend `TripController.cs:193` | Tikslu |
| ref | `Pasirinkti skrydi is saraso` | Flights tab `TripsPage.tsx:292`, backend `TripController.cs:187` | Tikslu |
| 40 | `User -> TripCreate: saveTrip()` | `createTripFromSelection` `TripsPage.tsx:100` | Tikslu |
| 41 | `TripCreate -> TripController: saveTripInformation()` | `api.post('/Trip')` `TripsPage.tsx:112`, `TripController.cs:105` | Tikslu |
| 42 | `TripController: checkTrip()` | `TripController.cs:108`, helper `TripController.cs:226` | Tikslu |
| 43 | `TripController -> Trip: saveTripData()` | `_context.Trips.Add` `TripController.cs:113` | Tikslu |
| 44 | `Trip -> TripController: save status` | `SaveChangesAsync` `TripController.cs:114` | Tikslu |
| 45 | `TripController -> TripCreate: save status information` | `CreatedAtAction` `TripController.cs:116` | Tikslu |
| 46 | `TripCreate -> User: successful save message` | `setMessage('Trip created from selected route.')` `TripsPage.tsx:126` | Tikslu |

## Apmoketi kelione

| Nr. | Diagramoje rodoma rodykle | Kodo vieta | Atitiktis |
|---:|---|---|---|
| 1 | `User -> Main: openTripPayment()` | Navigacija `/payments` `frontend/src/App.tsx:26`, `PaymentsPage.openTripPayment` `PaymentsPage.tsx:16` | Tikslu |
| 2 | `Main -> ReactController: openTripPayments()` | `PaymentsPage.openTripPayments` `PaymentsPage.tsx:17`, `useEffect` `PaymentsPage.tsx:96` | Tikslu |
| 3 | `ReactController -> TripPayments: open()` | `PaymentsPage` render `PaymentsPage.tsx:102` | UI |
| 4 | `TripPayments -> PaymentController: getTripReservations()` | `getReservations` `PaymentsPage.tsx:34` | Tikslu |
| 5 | `PaymentController -> Reservation: getReservations()` | `PaymentController.getTripReservations` `PaymentController.cs:36` | Tikslu |
| 6 | `Reservation -> PaymentController: reservations` | EF query `PaymentController.cs:38` | Tikslu |
| 7 | `PaymentController -> TripPayments: reservation data` | `setReservations` `PaymentsPage.tsx:39` | Tikslu |
| 8 | `User -> TripPayments: payReservation()` | Pay mygtukas `PaymentsPage.tsx:124` | Tikslu |
| 9 | `TripPayments -> PaymentController: requestPayment()` | `processPayment` UI sraute `PaymentsPage.tsx:79` | Pavadinimas UI sutrumpintas |
| 10 | `PaymentController -> BankingBoundary: requestPaymentPage()` | `PaymentController.requestPaymentPage` `PaymentController.cs:129` | Tikslu |
| 11 | `BankingBoundary -> Banking: getPaymentPage` | `getPaymentPage` `PaymentController.cs:197` | Lokalus/banko adapteris |
| 12 | `Banking -> BankingBoundary: payment_page` | `PaymentPageResponse` `PaymentController.cs:204` | Tikslu |
| 13 | `BankingBoundary -> PaymentController: payment_page` | `requestPaymentPage` return `PaymentController.cs:138` | Tikslu |
| 14 | `PaymentController -> TripPayments: payment_page` | `setPaymentPage` `PaymentsPage.tsx:82`, preview `PaymentsPage.tsx:154` | Tikslu |
| 15 | `TripPayments -> PaymentController: processPayment()` | `api.post('/Payment/{id}/processPayment')` `PaymentsPage.tsx:83` | Tikslu |
| 16 | `PaymentController -> BankingBoundary: processPayment()` | `processPayment(payment, 'payment')` `PaymentController.cs:150` | Tikslu |
| 17 | `BankingBoundary -> Banking: paymentData` | `PaymentController.processPayment` payload `PaymentController.cs:229` | Tikslu |
| 18 | `Banking -> BankingBoundary: payment result` | `PaymentProcessResult` `PaymentController.cs:244` | Tikslu |
| 19 | `BankingBoundary -> PaymentController: payment result` | `PaymentProcessResult` return `PaymentController.cs:244` | Tikslu |
| 20 | `PaymentController -> TripPayments: payment result` | `Ok(new { message... })` `PaymentController.cs:160` | Tikslu |
| 21 | `TripPayments -> PaymentController: savePayment()` | Payment request forma sukuria payment `PaymentsPage.tsx:46`, `api.post('/Payment')` `PaymentsPage.tsx:60` | Tikslu |
| 22 | `PaymentController -> Payment: savePaymentData()` | `savePaymentData` `PaymentController.cs:187` | Tikslu |
| 23 | `Payment -> PaymentController: payment save status` | `_context.Payments.Add`/`SaveChangesAsync` `PaymentController.cs:79` | Tikslu |
| 24 | `PaymentController -> TripPayments: Payment result` | `CreatedAtAction` `PaymentController.cs:82` | Tikslu |
| 25 | `TripPayments -> User: Payment result message` | `setMessage` `PaymentsPage.tsx:71`, `PaymentsPage.tsx:84` | Tikslu |

## Activity diagramu veiksmo blokai

| Activity diagrama | Veiksmo blokas diagramoje | Kodo vieta | Atitiktis |
|---|---|---|---|
| `Sukurti marsruta` | `Pasirinkti marsruto kurima` | `RoutesPage.openRouteCreate` `frontend/src/Views/Travel/RoutesPage.tsx:187` | Tikslu |
| `Sukurti marsruta` | `Pateikti marsruto kurimo langa` | RouteCreate modalas `RoutesPage.tsx:246` | Tikslu |
| `Sukurti marsruta` | `Pateikti pradine ir galutini miesta` | Start/end inputai `RoutesPage.tsx:256` | Tikslu |
| `Sukurti marsruta` | `Patikrinti duomenis` | `RouteController.checkRoute` `backend/Controllers/RouteController.cs:401`, `checkCities` `RouteController.cs:363` | Tikslu |
| `Sukurti marsruta` | `Rasti lankytinus objektus` | `RoutesPage.findObjects` `RoutesPage.tsx:46`, `RouteController.getRoadPOI` `RouteController.cs:312` | Tikslu |
| `Sukurti marsruta` | `Pateikti lankytinus objektus` | Object grid `RoutesPage.tsx:283`, markers `RouteMap.tsx:112` | Tikslu |
| `Sukurti marsruta` | `Pasirinkti norimus objektus` | `RoutesPage.toggleObject` `RoutesPage.tsx:58` | Tikslu |
| `Sukurti marsruta` | `Nuskaityti OSM faila` | `RouteController.readOSMFile` `RouteController.cs:831` | Tikslu |
| `Sukurti marsruta` | `Isskirti gatves kaip briaunas ir sankryzas kaip virsunes` | `separateRoadsAndCrossroads` `RouteController.cs:870` | Tikslu |
| `Sukurti marsruta` | `Apskaiciuoti segmentu ilgius` | `calculateSegmentLengths` `RouteController.cs:917` | Tikslu |
| `Sukurti marsruta` | `Sudaryti grafa is suskaiciuotu segmentu ilgiu` | `OsmRoadGraph`, `createGraph` `RouteController.cs:538` | Tikslu |
| `Sukurti marsruta` | `Sukurti atstumu tarp objektu matrica` | `createLengthMatrix` `RouteController.cs:543` | Tikslu |
| `Sukurti marsruta` | `Pasirinkti du neapskaiciuotus objektus` | Dijkstra poru skaiciavimas `RouteController.cs:547` | Tikslu |
| `Sukurti marsruta` | `Apskaiciuoti euristine atstumo funkcija` | `calculateHeuristic` `RouteController.cs:586` | Tikslu |
| `Sukurti marsruta` | `Pasirinkti labiausiai tinkama mazga` | `createInitialRoute` `RouteController.cs:608` | Tikslu |
| `Sukurti marsruta` | `Issaugoti marsruta tarp dvieju objektu` | `dijkstraPath` `RouteController.cs:976`, `createPolyLine` `RouteController.cs:691` | Tikslu |
| `Sukurti marsruta` | `Prideti trumpiausia marsruta tarp objektu` | `createPolyLine` koordinaciu sujungimas `RouteController.cs:691` | Tikslu |
| `Sukurti marsruta` | `Apkeisti lankytinu objektu seka` | `shuffleObjectOrder` `RouteController.cs:644` | Tikslu |
| `Sukurti marsruta` | `Pateikti surasto marsruto duomenis` | Preview modalas `RoutesPage.tsx:355` | Tikslu |
| `Sukurti marsruta` | `Pateikti pakitusi marsruta` | `setPreviewRoute` `RoutesPage.tsx:100` | Tikslu |
| `Sukurti marsruta` | `Patvirtinti marsruta` | `confirmAndSaveRoute` `RoutesPage.tsx:111` | Tikslu |
| `Sukurti marsruta` | `Issaugoti marsruta` | `RouteController.saveRoute` `RouteController.cs:132` | Tikslu |
| `Sukurti marsruta` | `Pateikti issaugojimo pranesima` | Success modalas `RoutesPage.tsx:386` | Tikslu |
| `Rasti lankytinus objektus` | `Suskaidyti marsruta dalimis` | `POIController.separateParts` `backend/Controllers/POIController.cs:165` | Tikslu |
| `Rasti lankytinus objektus` | `Pateikti marsruto duomenis` | `POIController.sendRouteData` `POIController.cs:97` | Tikslu |
| `Rasti lankytinus objektus` | `Pateikti artimus objektus` | `POIController.sendRoutePlaces` `POIController.cs:172` | Google Maps pakeista OSM |
| `Rasti lankytinus objektus` | `Patikrinti objektu duomenis` | `checkPOI` `POIController.cs:219`, `filterPOI` `POIController.cs:187` | Tikslu |
| `Rasti lankytinus objektus` | `Isrinkti ivertintus objektus` | `filterPOI` `POIController.cs:187` | Tikslu |
| `Rasti lankytinus objektus` | `Isrinkti 5 geriausiai ivertintus objektus` | `.Take(5)` `POIController.cs:106` | Tikslu |
| `Rasti lankytinus objektus` | `Ismesti klaidos pranesima` | `BadRequest`/`StatusCode` `POIController.cs:113` | Tikslu |
| `Rasti lankytinus objektus` | `Issaugoti isrinktus artimus objektus` | `savePOI` `POIController.cs:200` | Tikslu |
| `Ivertinti keliones trukme` | `Suskaidyti kelione dalimis` | `RouteController.separateParts` `RouteController.cs:409` | Tikslu |
| `Ivertinti keliones trukme` | `Pateikti keliones duomenis` | `RouteController.sendRoute` `RouteController.cs:533` | Tikslu |
| `Ivertinti keliones trukme` | `Pateikti kiekvienos dienos marsruto trukme` | `getRouteTime` `RouteController.cs:752` | Viena bendra marsruto trukme, ne dienomis |
| `Ivertinti keliones trukme` | `Issaugoti keliones trukme` | `saveRouteTime` `RouteController.cs:757` | Tikslu |
| `Pasirinkti marsruta` | `Atsidaryti marsruto pasirinkimo langa` | Trips route select `TripsPage.tsx:211` | UI dalis |
| `Pasirinkti marsruta` | `Pateikti marsruto pasirinkimo langa` | `TripsPage` route select render `TripsPage.tsx:214` | Tikslu |
| `Pasirinkti marsruta` | `Sukurti marsruta` | `RoutesPage.openRouteCreate` `RoutesPage.tsx:187` | Tikslu |
| `Pasirinkti marsruta` | `Issirinkti marsruta` | Route select `TripsPage.tsx:214` | Tikslu |
| `Pasirinkti marsruta` | `Pateikti pasirinkta marsruta` | `selectedRoute` card `TripsPage.tsx:226` | Tikslu |
| `Perziureti marsrutus` | `Pasirinkti marsrutu perziura` | `/routes` nav `frontend/src/App.tsx:26` | Tikslu |
| `Perziureti marsrutus` | `Atidaryti marsrutu perziuros langa` | `RoutesPage` list `RoutesPage.tsx:209` | Tikslu |
| `Perziureti marsrutus` | `Pasirinkti marsruta` | View button `RoutesPage.tsx:234` | Tikslu |
| `Perziureti marsrutus` | `Pateikti marsruto perziuros langa` | RouteView modalas `RoutesPage.tsx:319` | Tikslu |
| `Perziureti marsrutus` | `Redaguoti marsruta` | Edit button `RoutesPage.tsx:235` | Tikslu |
| `Redaguoti marsruta` | `Pasirinkti marsruto redagavima` | `RoutesPage.editRoute` `RoutesPage.tsx:157` | Tikslu |
| `Redaguoti marsruta` | `Pateikti marsruto redagavimo langa` | Edit/Create modalas `RoutesPage.tsx:246` | Tikslu |
| `Redaguoti marsruta` | `Atlikti pakeitimus su turimais variantais` | selected route points `RoutesPage.tsx:170` | Tikslu |
| `Redaguoti marsruta` | `Rasti lankytinus objektus` | `RoutesPage.findObjects` `RoutesPage.tsx:46` | Tikslu |
| `Redaguoti marsruta` | `Pateikti pakitusi marsruta` | Preview modalas `RoutesPage.tsx:355` | Tikslu |
| `Redaguoti marsruta` | `Patikrinti duomenis` | `RouteController.checkRoute` `RouteController.cs:401` | Tikslu |
| `Redaguoti marsruta` | `Pateikti marsruto duomenis su pakeitimais` | `previewCalculatedRoute` `RoutesPage.tsx:93` | Tikslu |
| `Redaguoti marsruta` | `Sudaryti polyline` | `createPolyLine` `RouteController.cs:691` | Tikslu |
| `Redaguoti marsruta` | `Pateikti marsruta su pakeitimais` | `setPreviewRoute` `RoutesPage.tsx:100` | Tikslu |
| `Redaguoti marsruta` | `Patvirtinti marsruta` | `confirmAndSaveRoute` `RoutesPage.tsx:111` | Tikslu |
| `Redaguoti marsruta` | `Issaugoti marsruta` | PUT `/Route/{id}` `RoutesPage.tsx:122` | Tikslu |
| `Redaguoti marsruta` | `Pateikti issaugojimo pranesima` | Success modalas `RoutesPage.tsx:386` | Tikslu |
| `Valdyti keliones` | `Pasirinkti kelioniu valdymo langa` | `/trips` nav `App.tsx:25` | Tikslu |
| `Valdyti keliones` | `Pateikti kelioniu valdymo langa` | `TripsPage` render `TripsPage.tsx:198` | Tikslu |
| `Valdyti keliones` | `Perziureti kelione` | `openTripView` `TripsPage.tsx:137` | Tikslu |
| `Valdyti keliones` | `Pateikti keliones perziuros langa` | TripView modalas `TripsPage.tsx:326` | Tikslu |
| `Valdyti keliones` | `Pasirinkti redaguoti kelione` | `openTripEdit` `TripsPage.tsx:143` | Tikslu |
| `Valdyti keliones` | `Atidaryti keliones redagavimo langa` | TripEdit modalas `TripsPage.tsx:355` | Tikslu |
| `Valdyti keliones` | `Pateikti keliones pakitimus` | TripEdit form submit `TripsPage.tsx:149` | Tikslu |
| `Valdyti keliones` | `Patikrinti keliones duomenis` | `TripController.checkTrip` `TripController.cs:226` | Tikslu |
| `Valdyti keliones` | `Issaugoti keliones pakitimus` | `TripController.updateTrip` `TripController.cs:120` | Tikslu |
| `Valdyti keliones` | `Trinti kelione` | `requestDeleteTrip` `TripsPage.tsx:160` | Tikslu |
| `Valdyti keliones` | `Pateikti trinimo patvirtinimo modala` | Delete confirmation modal `TripsPage.tsx:395` | Tikslu |
| `Valdyti keliones` | `Patvirtinti trinima` | `confirmDeleteTrip` `TripsPage.tsx:166` | Tikslu |
| `Valdyti keliones` | `Istrinti kelione` | `TripController.deleteTripData` `TripController.cs:139` | Tikslu |
| `Valdyti keliones` | `Pasirinkti keliones kurima` | Trip setup form `TripsPage.tsx:211` | Tikslu |
| `Valdyti keliones` | `Pateikti keliones kurimo langa` | `openTripCreatePage` `TripController.cs:98`, UI `TripsPage.tsx:211` | Tikslu |
| `Valdyti keliones` | `Pasirinkti marsruta` | Route select `TripsPage.tsx:214` | Tikslu |
| `Valdyti keliones` | `Pasirinkti apgyvendinima is saraso` | Hotels tab `TripsPage.tsx:291` | Tikslu |
| `Valdyti keliones` | `Pasirinkti skrydi is saraso` | Flights tab `TripsPage.tsx:292` | Tikslu |
| `Valdyti keliones` | `Pasirinkti automobili is saraso` | Cars tab `TripsPage.tsx:293` | Tikslu |
| `Valdyti keliones` | `Pateikti keliones duomenis` | `createTripFromSelection` `TripsPage.tsx:100` | Tikslu |
| `Valdyti keliones` | `Issaugoti keliones duomenis` | `TripController.saveTripInformation` `TripController.cs:105` | Tikslu |
| `Valdyti keliones` | `Ivertinti keliones trukme` | Route travel time jau saugoma per `RouteController.saveRouteTime` `RouteController.cs:757` | Tikslu marsruto lygyje |
| `Apmoketi kelione` | `Pasirinkti keliones apmokejimo langa` | `/payments` nav `App.tsx:26`, `openTripPayment` `PaymentsPage.tsx:16` | Tikslu |
| `Apmoketi kelione` | `Pateikti keliones apmokejimo langa` | `PaymentsPage` render `PaymentsPage.tsx:102` | Tikslu |
| `Apmoketi kelione` | `Pasirinkti apmokama kelione` | Trip select `PaymentsPage.tsx:135` | Tikslu |
| `Apmoketi kelione` | `Pateikti apmokamos keliones rezervaciju duomenis` | `getReservations` `PaymentsPage.tsx:34` | Tikslu |
| `Apmoketi kelione` | `Pasirinkti rezervacijos apmokejima` | Payment request forma `PaymentsPage.tsx:135` | Tikslu |
| `Apmoketi kelione` | `Surasti rezervacijos apmokejimo informacija` | `PaymentController.getTripReservations` `PaymentController.cs:36` | Tikslu |
| `Apmoketi kelione` | `Pateikti apmokejimui skirta langa` | `requestPaymentPage` `PaymentController.cs:129`, preview `PaymentsPage.tsx:154` | Tikslu |
| `Apmoketi kelione` | `Pateikti mokejimo duomenis` | `saveReservationData` `PaymentsPage.tsx:46` ir `processPayment` `PaymentsPage.tsx:79` | Tikslu |
| `Apmoketi kelione` | `Perteikti duomenis bankininkystes aplinkai` | `PaymentController.processPayment` `PaymentController.cs:222` | Lokalus/banko adapteris |
| `Apmoketi kelione` | `Pateikti mokejimo busena` | `PaymentProcessResult` `PaymentController.cs:250` | Tikslu |
| `Apmoketi kelione` | `Issaugoti apmokejimo busena` | `payment.updatePaymentData` `PaymentController.cs:151`, `SaveChangesAsync` `PaymentController.cs:158` | Tikslu |
| `Apmoketi kelione` | `Perteikti mokejimo busenos pranesima` | `Ok(...)` `PaymentController.cs:160`, UI message `PaymentsPage.tsx:84` | Tikslu |
