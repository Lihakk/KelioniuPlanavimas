import { useEffect, useState } from 'react';
import api from '../../api/api';
import type { RecommendationPreferences, Review, TripRecommendation } from '../../types';

export const ReviewsPage = () => {
    const [reviews, setReviews] = useState<Review[]>([]);
    const [recommendations, setRecommendations] = useState<TripRecommendation[]>([]);
    const [tripElementType, setTripElementType] = useState('Trip');
    const [tripElementId, setTripElementId] = useState(1);
    const [reviewText, setReviewText] = useState('');
    const [rating, setRating] = useState(5);
    const [preferences, setPreferences] = useState<RecommendationPreferences>({
        travelType: 'City',
        budget: 500,
        weatherPreference: 'clear'
    });
    const [message, setMessage] = useState('');

    const requestTripElementReviews = () => api.get<Review[]>('/Review').then(res => setReviews(res.data));
    const requestRecommendations = () => api.get<TripRecommendation[]>('/Review/recommendations').then(res => setRecommendations(res.data));
    const personalizeRecommendations = async () => {
        try {
            const response = await api.post<TripRecommendation[]>('/Review/recommendations', preferences);
            setRecommendations(response.data);
            setMessage('Personalized recommendations updated.');
        } catch (error) {
            console.error(error);
            setMessage('Could not personalize recommendations.');
        }
    };

    const saveReviewData = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await api.post('/Review', {
                tripElementType,
                tripElementId,
                reviewText,
                rating,
                date: new Date().toISOString()
            });
            setReviewText('');
            setMessage('Review saved.');
            await requestTripElementReviews();
            await requestRecommendations();
        } catch (error) {
            console.error(error);
            setMessage('Review can be saved only when the trip rules allow it.');
        }
    };

    useEffect(() => {
        requestTripElementReviews();
        requestRecommendations();
    }, []);

    return (
        <div className="workspace">
            <header className="page-head">
                <span className="eyebrow">Review diagrams P8-P9 and K1-K2</span>
                <h1>Reviews and recommendations</h1>
            </header>

            <div className="flow-grid">
                <section className="panel">
                    <div className="section-title">
                        <h2>Recommendations</h2>
                        <button className="btn btn-outline" onClick={requestRecommendations}>Refresh</button>
                    </div>
                    <div className="data-list dense">
                        {recommendations.map(item => (
                            <article className="data-card" key={item.tripId}>
                                <strong>{item.name}</strong>
                                <span>Score: {item.score}</span>
                                <span>{item.reason}</span>
                            </article>
                        ))}
                    </div>
                </section>

                <section className="panel">
                    <h2>Personalization</h2>
                    <label>Travel type</label>
                    <input value={preferences.travelType} onChange={e => setPreferences({ ...preferences, travelType: e.target.value })} />
                    <label>Budget</label>
                    <input type="number" value={preferences.budget} onChange={e => setPreferences({ ...preferences, budget: Number(e.target.value) })} />
                    <label>Weather preference</label>
                    <input value={preferences.weatherPreference} onChange={e => setPreferences({ ...preferences, weatherPreference: e.target.value })} />
                    <button className="btn btn-primary" onClick={personalizeRecommendations}>Personalize</button>
                    {message && <p className="status-line">{message}</p>}
                </section>
            </div>

            <div className="flow-grid">
                <section className="panel">
                    <h2>Trip element reviews</h2>
                <div className="data-list">
                    {reviews.map(review => (
                        <article className="data-card" key={review.id}>
                            <strong>{review.tripElementType} #{review.tripElementId}</strong>
                            <span>{review.rating}/5</span>
                            <p>{review.reviewText}</p>
                        </article>
                    ))}
                </div>
            </section>

            <section className="panel">
                <h3>Leave review</h3>
                <form onSubmit={saveReviewData}>
                    <label>Element type</label>
                    <input value={tripElementType} onChange={e => setTripElementType(e.target.value)} />
                    <label>Element id</label>
                    <input type="number" value={tripElementId} onChange={e => setTripElementId(Number(e.target.value))} />
                    <label>Review</label>
                    <textarea value={reviewText} onChange={e => setReviewText(e.target.value)} required />
                    <label>Rating</label>
                    <input type="number" min="1" max="5" value={rating} onChange={e => setRating(Number(e.target.value))} />
                    <button className="btn btn-primary" type="submit">Save review</button>
                </form>
            </section>
            </div>
        </div>
    );
};
