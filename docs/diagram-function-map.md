# Diagram To Code Mapping

This table links each sequence diagram area to the implemented frontend screen, API endpoint, controller action and important helper functions.

| Diagram / flow | Diagram message or responsibility | Frontend code | API / controller action | Main helper functions or model methods |
|---|---|---|---|---|
| P1 Sukurti marsruta | Open route creation, enter start/end cities | `frontend/src/Views/Travel/RoutesPage.tsx` modal `RouteCreate` | `POST /api/Route/openRouteCreation`, `POST /api/Route/preview` -> `RouteController.previewRoute` | `checkRoute`, `sendCities`, `checkCities` |
| P1 Sukurti marsruta | Find POIs between cities from map data | `RoutesPage.findObjects`, `RouteMap.tsx` | `POST /api/Route/roadPOI` -> `RouteController.getRoadPOI` | `getOSMData`, `cleanRoadData`, `createCorridorPOI`, `getFallbackPOI` |
| P1 Sukurti marsruta | Select POIs that route must pass | `RouteMap` marker click, `RoutesPage.toggleObject` | Route payload `routePoints` in `POST /api/Route` | `selectObjects`, `selectedPOI`, `createRouteWithPOI` |
| P1 Sukurti marsruta | Read OSM roads and manually build graph | `RoutesPage.previewCalculatedRoute` | `POST /api/Route/preview` -> `RouteController.previewRoute` | `readOSMFile`, `buildRoadGraphQuery`, `separateRoadsAndCrossroads`, `calculateSegmentLengths`, `findNearestGraphNode`, `dijkstraDistances` |
| P1 Sukurti marsruta | Calculate route length matrix and order | `RoutesPage.previewCalculatedRoute` | `RouteController.previewRoute` | `createLengthMatrix`, `createGraph`, `calculateHeuristic`, `createInitialRoute`, `shuffleObjectOrder`, `selectRoad` |
| P1 Sukurti marsruta | Create route geometry and preview result | `RouteMap` polyline display in preview modal | `RouteController.previewRoute` | `createPolyLine`, `dijkstraPath`, `calculatePathDistance`, `createFallbackPolyLine`, `saveRouteTime`, `saveRouteData` |
| P1 Sukurti marsruta | Accept route and save confirmed preview | `RoutesPage.confirmAndSaveRoute` success modal | `POST /api/Route` -> `RouteController.saveRoute` | `hasCalculatedRouteData`, `cleanConfirmedRouteData`, EF `Routes.Add`, `SaveChangesAsync` |
| P2 Redaguoti marsruta | Load specific saved route for editing | `RoutesPage.editRoute` | `GET /api/Route/{id}` -> `getRoute/getSpecificRoute` | `getSpecificRoute`, route points included with `Include(RoutePoints)` |
| P2 Redaguoti marsruta | Add/remove POIs and recalculate edited route | `RoutesPage.toggleObject`, `RoutesPage.saveRoute` | `PUT /api/Route/{id}` -> `sendRouteData` | `sendCities`, `createRouteWithPOI`, `SelectFastestRoad`, `saveRouteData` |
| P3 Perziureti marsrutus | Show saved route list | `RoutesPage.getRoutes` | `GET /api/Route`, `GET /api/Route/getAllRoutes` | `getRoutes`, `getAllRoutes` |
| P7 Rasti lankytinus objektus | Separate route parts and request POIs | `RoutesPage.findObjects` | `POST /api/Route/roadPOI`, also `POST /api/POI/sendRouteData` for city POIs | `buildOverpassQuery`, `readOverpassPOI`, `cleanRoadData` |
| P11 Pasirinkti marsruta | Select route for trip | `TripsPage` route `<select>` | `GET /api/Route` | `getAllRoutes` |
| P13 Ivertinti keliones trukme | Save travel time after route calculation | `RoutesPage.saveRoute`, `RouteMap` stats | `GET /api/Route/getTime/{id}`, `POST/PUT /api/Route` | `evaluateRouteData`, `saveRouteTime` |
| P4 Sukurti kelione | Create trip from selected route | `TripsPage.createTripFromSelection` | `POST /api/Trip` -> `TripController.saveTripInformation` | `checkTrip`, `Trip.checkTrip` |
| P5 Redaguoti kelione | Update trip fields/status | Existing API support | `PUT /api/Trip/{id}` -> `TripController.updateTrip` | `checkTrip` |
| P6 Perziureti keliones | Show saved trips | `TripsPage.getTrips` | `GET /api/Trip`, `GET /api/Trip/getTripsData` | `getTrips`, `getTripsData` |
| P12 Pasirinkti apgyvendinima | Request hotels by selected route location | `TripsPage.requestTravelOffers`, Hotels tab | `POST /api/Trip/accommodation/list` | `requestAccommodationListFromExternalActor`, `requestProviderOffers`, `createFallbackOffers` |
| P12 Pasirinkti apgyvendinima | Assign selected hotel to trip | `OfferColumn` Select button | `POST /api/Trip/{tripId}/accommodation` | `assignAccommodationToTrip`, `selectAccommodation`, `saveSelectedAccommodation`, `createReservation` |
| P14 Pasirinkti skrydi | Request flights by route cities | `TripsPage.requestTravelOffers`, Flights tab | `POST /api/Trip/flight/list` | `requestFlightListFromExternalActor`, `createFallbackFlightOffers` |
| P14 Pasirinkti skrydi | Assign selected flight to trip | `OfferColumn` Select button | `POST /api/Trip/{tripId}/flight` | `assignFlightToTrip`, `selectFlight`, `saveSelectedFlight`, `createReservation` |
| P15 Pasirinkti automobili | Request cars by destination | `TripsPage.requestTravelOffers`, Cars tab | `POST /api/Trip/car/list` | `requestCarListFromExternalActor`, `createFallbackOffers` |
| P15 Pasirinkti automobili | Assign selected car to trip | `OfferColumn` Select button | `POST /api/Trip/{tripId}/car` | `assignCarToTrip`, `selectCar`, `saveSelectedCar`, `createReservation` |
| P8 Palikti atsiliepima | Open review form and submit review | `frontend/src/Views/Travel/ReviewsPage.tsx` | `POST /api/Review` -> `ReviewController.saveReviewData` | `checkTripStatus`, `Review.getTripStatus` |
| P9 Perziureti atsiliepimus | Request and sort reviews | `ReviewsPage.requestTripElementReviews` | `GET /api/Review` -> `requestTripElementReviews` | EF query filters by type/element id, sorted by date |
| K1 Perziureti rekomendacijas | Generate recommendations from trips/reviews | `ReviewsPage.requestRecommendations` | `GET /api/Review/recommendations` | `generateRecommendations`, `calculateScoreWeights`, `calculateRatingScore` |
| K2 Suasmeninti rekomendacijas | Validate preferences and reevaluate | `ReviewsPage.personalizeRecommendations` | `POST /api/Review/recommendations` -> `ReevaluateRecommendations` | `validatePreferences`, `analyzePreferences`, `evaluateAlternatives`, `calculateWeatherScore`, `calculateBudgetScore`, `calculateLocationScore`, `calculateDateScore` |
| P10 Valdyti keliones reikmenu sarasa | Create supply list from selected trip | `frontend/src/Views/Travel/SupplyListPage.tsx` | `POST /api/SupplyList/trip/{tripId}` | `requestNewSupplyList`, `determineTripConditions`, `analyzeTripParameters`, `createSupplyListByConditions` |
| P10 Valdyti keliones reikmenu sarasa | Reset or update saved supply list | `SupplyListPage.resetCurrentSupplyList` | `POST /api/SupplyList/{id}/resetCurrentSupplyList`, `PUT /api/SupplyList/{id}` | `updateSupplyList`, `calculateBaseClothingQuantities`, `adjustFinalQuantities` |
| K3 Registruotis | Register user | `frontend/src/Views/Travel/AccountsPage.tsx` | `POST /api/Account/register` | `AccountController.register`, validation in request/model logic |
| K4 Prisijungti | Login user | `AccountsPage.submitAccount` login mode | `POST /api/Account/login` | `AccountController.login` |
| K5 Atsijungti | Logout user | `AccountsPage.logout` | `POST /api/Account/logout` | `AccountController.logout` |
| K6 Perziureti profili | Load profile | `AccountsPage` profile panel | `GET /api/Account/profile/{id}` | `AccountController.openProfileView` equivalent action |
| K7 Redaguoti profili | Update profile fields | `AccountsPage.saveProfile` | `PUT /api/Account/profile/{id}` | `AccountController.updateUser` equivalent action |
| A1 Administruoti paskyras | List and filter/admin accounts | `AccountsPage.loadAccounts` | `GET /api/Account/administrateAccounts` | `AccountController.administrateAccounts` |
| A1 Administruoti paskyras | Block/unblock user | `AccountsPage.changeStatus` | `POST /api/Account/{id}/block`, `POST /api/Account/{id}/unblock` | `AccountController.blockUser`, `AccountController.unblockUser` |
| M1 Apmoketi kelione | Create reservation and payment request | `frontend/src/Views/Travel/PaymentsPage.tsx` | `POST /api/Payment/reservation`, `POST /api/Payment` | `saveReservationData`, `savePayment`, `Payment.savePaymentData` |
| M1 Apmoketi kelione | Process payment | `PaymentsPage.processPayment` | `POST /api/Payment/{id}/processPayment` | `requestPayment`, `processPayment`, `Payment.updatePaymentData` |
| M2 Atsaukti mokejima | Request refund and update payment/reservation | `PaymentsPage.requestRefund` | `POST /api/Payment/{id}/requestRefund` | `requestRefund`, `processPayment`, `updatePaymentData`, reservation status update |
| M3 Perziureti mokejimu istorija | Load payment history and details | `PaymentsPage.getPayments` | `GET /api/Payment`, `GET /api/Payment/getPayments`, `GET /api/Payment/{id}` | `getPaymentData`, `getPayments`, `getPayment` |
| POI CRUD / admin support | Create, view, edit, delete POIs | `frontend/src/Views/Admin/POI/*` | `GET/POST/PUT/DELETE /api/POI` | `getAllPOI`, `createNewPOI`, `editPOIData`, `deleteSelectedPOI`, `PointOfInterest.checkPOI` |
