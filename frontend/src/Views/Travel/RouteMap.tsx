import { useEffect, useMemo } from 'react';
import L from 'leaflet';
import { MapContainer, Marker, Polyline, Popup, TileLayer, useMap } from 'react-leaflet';
import type { LatLngBoundsExpression, LatLngExpression } from 'leaflet';
import type { PointOfInterest, TravelRoute } from '../../types';

const cityCoordinates: Record<string, LatLngExpression> = {
    kaunas: [54.8985, 23.9036],
    vilnius: [54.6872, 25.2797],
    trakai: [54.6378, 24.9343],
    klaipeda: [55.7033, 21.1443]
};

const readNumber = (value: string) => Number.parseFloat(value.replace(',', '.'));
const cityPoint = (city: string): LatLngExpression => cityCoordinates[city.trim().toLowerCase()] ?? cityCoordinates.vilnius;

const markerIcon = (kind: 'city-start' | 'city-end' | 'poi' | 'selected', label: string) => L.divIcon({
    className: `osm-marker ${kind}`,
    html: `<span>${label}</span>`,
    iconSize: [34, 34],
    iconAnchor: [17, 17],
    popupAnchor: [0, -18]
});

const parseRouteLine = (route: TravelRoute | null): LatLngExpression[] => {
    if (!route?.polyline) return [];

    try {
        const geoJson = JSON.parse(route.polyline) as { coordinates?: [number, number][] };
        return geoJson.coordinates?.map(([longitude, latitude]) => [latitude, longitude] as LatLngExpression) ?? [];
    } catch {
        return [];
    }
};

const FitMapBounds = ({ points }: { points: LatLngExpression[] }) => {
    const map = useMap();

    useEffect(() => {
        if (points.length < 2) return;
        map.fitBounds(points as LatLngBoundsExpression, { padding: [34, 34] });
    }, [map, points]);

    return null;
};

export const RouteMap = ({
    startCity,
    endCity,
    objects,
    selectedNames,
    createdRoute,
    onToggleObject
}: {
    startCity: string;
    endCity: string;
    objects: PointOfInterest[];
    selectedNames: string[];
    createdRoute: TravelRoute | null;
    onToggleObject: (name: string) => void;
}) => {
    const start = cityPoint(startCity);
    const end = cityPoint(endCity);
    const pins = useMemo(() => objects
        .map(object => ({
            object,
            position: [readNumber(object.latitude), readNumber(object.longitude)] as LatLngExpression,
            selected: selectedNames.includes(object.name)
        }))
        .filter(pin => Number.isFinite((pin.position as number[])[0]) && Number.isFinite((pin.position as number[])[1])), [objects, selectedNames]);

    const selectedPins = selectedNames
        .map(name => pins.find(pin => pin.object.name === name))
        .filter((pin): pin is NonNullable<typeof pin> => Boolean(pin));

    const routeLine = parseRouteLine(createdRoute);
    const previewLine = [start, ...selectedPins.map(pin => pin.position), end];
    const visibleLine = routeLine.length > 1 ? routeLine : previewLine;
    const fitPoints = [...previewLine, ...pins.slice(0, 20).map(pin => pin.position)];

    return (
        <section className="osm-route-shell" aria-label="OSM route map">
            <div className="route-map-topbar">
                <div>
                    <span className="eyebrow">OSM route editor</span>
                    <h2>{startCity} to {endCity}</h2>
                </div>
                <div className="map-stat-row">
                    <span>{objects.length} road POIs</span>
                    <strong>{createdRoute ? `${createdRoute.length} km` : `${selectedPins.length} selected`}</strong>
                    <span>{createdRoute?.travelTime || 'Recalculate to update'}</span>
                </div>
            </div>

            <div className="osm-map-layout">
                <MapContainer className="osm-map" center={start} zoom={9} scrollWheelZoom>
                    <TileLayer
                        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                    />
                    <FitMapBounds points={fitPoints} />
                    <Polyline positions={visibleLine} pathOptions={{ color: '#2563eb', weight: 6, opacity: 0.88 }} />
                    <Marker position={start} icon={markerIcon('city-start', 'S')}>
                        <Popup>{startCity}</Popup>
                    </Marker>
                    <Marker position={end} icon={markerIcon('city-end', 'F')}>
                        <Popup>{endCity}</Popup>
                    </Marker>
                    {pins.map(pin => {
                        const order = selectedNames.indexOf(pin.object.name) + 1;
                        return (
                            <Marker
                                key={`${pin.object.name}-${pin.object.latitude}-${pin.object.longitude}`}
                                position={pin.position}
                                icon={markerIcon(pin.selected ? 'selected' : 'poi', pin.selected ? String(order) : '')}
                                eventHandlers={{ click: () => onToggleObject(pin.object.name) }}
                            >
                                <Popup>
                                    <strong>{pin.object.name}</strong>
                                    <br />
                                    {pin.object.type}
                                    <br />
                                    {pin.selected ? 'Selected for route' : 'Click marker to add to route'}
                                </Popup>
                            </Marker>
                        );
                    })}
                </MapContainer>

                <aside className="map-stop-list osm-stop-list">
                    <span className="eyebrow">Route order</span>
                    <strong>{selectedPins.length ? 'Selected points' : 'Select POIs on the map'}</strong>
                    <div>
                        {selectedPins.map((pin, index) => (
                            <button key={pin.object.name} type="button" onClick={() => onToggleObject(pin.object.name)}>
                                <b>{index + 1}</b>
                                <span>{pin.object.name}</span>
                            </button>
                        ))}
                        {selectedPins.length === 0 && <p>Click road POIs to force the route through those points.</p>}
                    </div>
                </aside>
            </div>
        </section>
    );
};
