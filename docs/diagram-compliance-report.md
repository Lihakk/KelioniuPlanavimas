# Diagram Compliance Report

This file maps the supplied activity and sequence diagrams to the current implementation. "Exact enough" means the same user/system/API responsibility exists with matching navigation and close function names. The main intentional difference is that map/time/route work uses OSM, Overpass and the local OSM graph builder instead of Google Maps, because the project implementation requirement is OSM-based routing.

| Diagram pair | Status | Implemented by | Notes |
|---|---|---|---|
| Sukurti marsruta | Done | `RoutesPage.openRouteCreate`, `RoutesPage.previewCalculatedRoute`, `RoutesPage.confirmAndSaveRoute`, `RouteController.previewRoute`, `RouteController.readOSMFile`, `RouteController.separateRoadsAndCrossroads`, `RouteController.dijkstraPath` | Matches create window, city validation, nearby POI search, selected POIs, manual OSM graph, route preview, accept/save, success message. Google Maps actor is replaced by OSM/Overpass. |
| Rasti lankytinus objektus | Done | `RouteController.getRoadPOI`, `POIController.sendRouteData`, `POIController.separateParts`, `POIController.sendRoutePlaces`, `POIController.filterPOI`, `POIController.savePOI` | The route-based POI controller path now follows the sequence names and stores filtered POIs when called. The route editor still uses `Route/roadPOI` for many road POIs because it needs more than 5 selectable objects. |
| Ivertinti keliones trukme | Done | `RouteController.separateParts`, `RouteController.sendRoute`, `RouteController.getRouteTime`, `RouteController.saveRouteTime` | Same logical sequence as the diagram. Duration comes from the manually built OSM graph or fallback route matrix, not Google Maps. |
| Pasirinkti marsruta | Done | `TripsPage.getRoutes`, `RouteController.openRoutes`, `RouteController.getAllRoutes`, `RouteController.openRouteView`, `RouteController.getSpecificRoute` | Route selection is inside `TripsPage` rather than a separate physical `RouteSelect.tsx` file, but the boundary/action is present in the UI. |
| Perziureti marsrutus | Done | `RoutesPage.getRoutes`, `RoutesPage.viewSelectedRoute`, `RoutesPage.editRoute`, `RouteController.openRoutes`, `RouteController.openRouteView`, `RouteController.openRouteEdit` | Added RouteView modal and explicit View/Edit actions from the route list. |
| Redaguoti marsruta | Done | `RoutesPage.editRoute`, `RouteController.openRouteEdit`, `RouteController.getRoutePOI`, `RouteController.sendRouteData`, `RouteController.previewRoute` | The edit flow loads saved route data, loads route POIs, lets the user change selected objects, recalculates, previews and confirms. GoogleMapsBoundary is represented by OSM/Overpass graph logic. |
| Valdyti keliones | Mostly done | `TripsPage.openTripView`, `TripsPage.openTripEdit`, `TripsPage.requestDeleteTrip`, `TripsPage.createTripFromSelection`, `TripController.openTripList`, `TripController.openTripPage`, `TripController.openTripEditPage`, `TripController.deleteConfirmation` | Added view, edit and delete confirmation modals. The reservation-management block is represented by the existing service selection/reservation calls, not a separate reservation management screen. |
| Apmoketi kelione | Done | `PaymentsPage.openTripPayment`, `PaymentsPage.processPayment`, `PaymentController.openTripPayments`, `PaymentController.getTripReservations`, `PaymentController.requestPaymentPage`, `PaymentController.processPayment`, `PaymentController.savePayment` | Added payment-page request before processing. Banking actor is implemented as local banking adapter unless `ExternalApis:BankingUrl` is configured. |

## Main Differences

| Difference | Why |
|---|---|
| Google Maps in diagrams is implemented as OSM/Overpass/Nominatim/Leaflet. | The project requirement and previous implementation work require OSM data and manual OSM graph routing. |
| Some UML boundaries are React states/modals, not separate files. | The frontend is a React SPA. `RouteCreate`, `RouteView`, `TripEdit`, and payment page are UI states in page components. |
| Some external provider APIs are local/fallback adapters. | Booking, Skyscanner, DiscoverCars and Banking URLs are configurable but empty in local config, so local adapter data is returned. |
| Route POI search has two paths. | `Route/roadPOI` returns many POIs for map selection; `POI/sendRouteData` follows the diagram's "pick up to 5 nearby POIs" responsibility. |

## Verification

| Check | Result |
|---|---|
| Backend build | `dotnet build backend\backend.csproj` passes with 0 warnings and 0 errors. |
| Frontend build | `npm.cmd run build` passes. |

