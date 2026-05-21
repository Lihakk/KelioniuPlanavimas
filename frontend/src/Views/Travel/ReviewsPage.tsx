import { useEffect, useState } from 'react';
import api from '../../api/api';
import type { Review, TripRecommendation } from '../../types';

export const ReviewsPage = () => {
    const [reviews, setReviews] = useState<Review[]>([]);
    const [recommendations, setRecommendations] = useState<TripRecommendation[]>([]);
    const [tripElementType, setTripElementType] = useState('Trip');
    const [tripElementId, setTripElementId] = useState(1);
    const [reviewText, setReviewText] = useState('');
    const [rating, setRating] = useState(5);

    const requestTripElementReviews = () => api.get<Review[]>('/Review').then(res => setReviews(res.data));
    const requestRecommendations = () => api.get<TripRecommendation[]>('/Review/recommendations').then(res => setRecommendations(res.data));

    const saveReviewData = async (e: React.FormEvent) => {
        e.preventDefault();
        await api.post('/Review', {
            tripElementType,
            tripElementId,
            reviewText,
            rating,
            date: new Date().toISOString()
        });
        setReviewText('');
        await requestTripElementReviews();
        await requestRecommendations();
    };

    useEffect(() => {
        requestTripElementReviews();
        requestRecommendations();
    }, []);

    return (
        <div className="container page-grid">
            <section>
                <h2>Reviews and recommendations</h2>
                <div className="data-list">
                    {recommendations.map(item => (
                        <article className="data-card" key={item.tripId}>
                            <strong>{item.name}</strong>
                            <span>Score: {item.score}</span>
                            <span>{item.reason}</span>
                        </article>
                    ))}
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
    );
};
