import { useCallback, useEffect, useState } from 'react';
import { api } from './api/client';
import { Header } from './components/Header';
import { ReviewSubmitPanel } from './components/ReviewSubmitPanel';
import { ReviewTable } from './components/ReviewTable';
import type { Review } from './types/review';

function App() {
  const [reviews, setReviews] = useState<Review[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const loadReviews = useCallback(async () => {
    setIsLoading(true);
    setLoadError(null);

    try {
      const data = await api.getReviews();
      setReviews(data);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to load reviews.';
      setLoadError(message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadReviews();
  }, [loadReviews]);

  async function handleSubmit(reviewText: string) {
    setIsSubmitting(true);
    try {
      const created = await api.createReview({ reviewText });
      setReviews((current) => [created, ...current.filter((item) => item.id !== created.id)]);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="app-shell">
      <div className="app-frame">
        <Header />
        <div className="content-grid">
          <ReviewSubmitPanel onSubmit={handleSubmit} isSubmitting={isSubmitting} />
          <ReviewTable
            reviews={reviews}
            isLoading={isLoading}
            error={loadError}
            onRetry={() => {
              void loadReviews();
            }}
          />
        </div>
      </div>
    </div>
  );
}

export default App;
