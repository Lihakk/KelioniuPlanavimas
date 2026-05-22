import { useEffect, useState } from 'react';
import api from '../../api/api';
import type { RecommendationPreferences, TripRecommendation, TravelType, WeatherPreferenceOption } from '../../types';

export const RecommendationsPage = () => {
    const [recommendations, setRecommendations] = useState<TripRecommendation[]>([]);
    const TRAVEL_TYPES: TravelType[] = [
        'Beach / Sea',
        'Mountains / Hiking',
        'City / Culture',
        'Nature / Wildlife',
        'Skiing / Winter sports',
        'Cruise'
    ];

    const WEATHER_PREFERENCES: WeatherPreferenceOption[] = [
        'Hot and sunny',
        'Warm with some cloud',
        'Mild / Spring-like',
        'Cold / Winter atmosphere',
        'No preference'
    ];

    const [preferences, setPreferences] = useState<RecommendationPreferences>({
        travelType: 'City / Culture',
        budget: 500,
        weatherPreference: 'No preference'
    });
    const [isPersonalized, setIsPersonalized] = useState(false);
    const [message, setMessage] = useState('');
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        requestRecommendations();
    }, []);

    return (
        <div className="workspace">
            <header className="page-head">
                <span className="eyebrow">Review recommendations and personalize</span>
                <h1>Trip Recommendations</h1>
            </header>

            <div className="flow-grid">
                <section className="panel">
                    <div className="section-title">
                        <h2>Recommendations</h2>
                        <button 
                            className="btn btn-outline" 
                            onClick={requestRecommendations}
                            disabled={isLoading}
                        >
                            Refresh
                        </button>
                    </div>
                    <div className="data-list dense">
                        {recommendations.length === 0 ? (
                            <p>No recommendations available</p>
                        ) : (
                            recommendations.map(item => (
                                <article className="data-card" key={item.tripId}>
                                    <strong>{item.name}</strong>
                                    <span>Score: {item.score}</span>
                                    <span className="recommendation-reason">{item.reason}</span>
                                </article>
                            ))
                        )}
                    </div>
                </section>

                <section className="panel">
                    <div className="section-title">
                        <h2>{isPersonalized ? 'Personalized' : 'General'} Preferences</h2>
                        <button 
                            className={`btn ${isPersonalized ? 'btn-primary' : 'btn-outline'}`}
                            onClick={() => setIsPersonalized(!isPersonalized)}
                        >
                            {isPersonalized ? 'Reset' : 'Customize'}
                        </button>
                    </div>

                    {isPersonalized && (
                        <form onSubmit={personalizeRecommendations} className="form-grid">
                            <div className="form-group">
                                <label htmlFor="travel-type">Travel Type</label>
                                <select
                                    id="travel-type"
                                    value={preferences.travelType}
                                    onChange={(e) => setPreferences({...preferences, travelType: e.target.value as TravelType})}
                                >
                                    {TRAVEL_TYPES.map(opt => (
                                        <option key={opt} value={opt}>{opt}</option>
                                    ))}
                                </select>
                            </div>

                            <div className="form-group">
                                <label htmlFor="budget">Budget ($)</label>
                                <input
                                    id="budget"
                                    type="number"
                                    min="0"
                                    value={preferences.budget}
                                    onChange={(e) => setPreferences({...preferences, budget: parseInt(e.target.value)})}
                                    placeholder="Enter budget"
                                />
                            </div>

                            <div className="form-group">
                                <label htmlFor="weather">Weather Preference</label>
                                <select
                                    id="weather"
                                    value={preferences.weatherPreference}
                                    onChange={(e) => setPreferences({...preferences, weatherPreference: e.target.value as WeatherPreferenceOption})}
                                >
                                    {WEATHER_PREFERENCES.map(opt => (
                                        <option key={opt} value={opt}>{opt}</option>
                                    ))}
                                </select>
                            </div>

                            <button type="submit" className="btn btn-primary" disabled={isLoading}>
                                {isLoading ? 'Processing...' : 'Apply Preferences'}
                            </button>
                        </form>
                    )}

                    {message && <p className="message">{message}</p>}
                </section>
            </div>
        </div>
    );

    // ============== HELPER FUNCTIONS ==============

    function requestRecommendations(): void {
        setIsLoading(true);
        setMessage('');
        api.get<TripRecommendation[]>('/Review/recommendations')
            .then(res => {
                setRecommendations(res.data);
                setIsPersonalized(false);
                setMessage('Recommendations loaded.');
            })
            .catch(error => {
                console.error(error);
                setMessage('Could not load recommendations.');
            })
            .finally(() => setIsLoading(false));
    }

    function personalizeRecommendations(e: React.FormEvent): void {
        e.preventDefault();
        setIsLoading(true);
        setMessage('');

        api.post<TripRecommendation[]>('/Review/recommendations', {
            travelType: preferences.travelType,
            budget: preferences.budget,
            weatherPreference: preferences.weatherPreference
        })
            .then(response => {
                setRecommendations(response.data);
                setMessage('Personalized recommendations updated.');
            })
            .catch(error => {
                console.error(error);
                setMessage('Could not personalize recommendations.');
            })
            .finally(() => setIsLoading(false));
    }
};
