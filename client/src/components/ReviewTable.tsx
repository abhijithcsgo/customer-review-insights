import { useMemo, useState } from 'react';
import type { Review } from '../types/review';
import { SentimentBadge } from './SentimentBadge';

interface ReviewTableProps {
  reviews: Review[];
  isLoading: boolean;
  error: string | null;
  onRetry: () => void;
}

function formatTimestamp(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date);
}

function truncate(text: string, max = 90): string {
  if (text.length <= max) {
    return text;
  }

  return `${text.slice(0, max).trimEnd()}…`;
}

export function ReviewTable({ reviews, isLoading, error, onRetry }: ReviewTableProps) {
  const [search, setSearch] = useState('');
  const [selectedReview, setSelectedReview] = useState<Review | null>(null);

  const filtered = useMemo(() => {
    const query = search.trim().toLowerCase();
    if (!query) {
      return reviews;
    }

    return reviews.filter((review) => {
      const haystack = [
        review.reviewText,
        review.category,
        review.sentiment,
        review.summary,
      ]
        .join(' ')
        .toLowerCase();

      return haystack.includes(query);
    });
  }, [reviews, search]);

  return (
    <section className="panel history-panel" aria-labelledby="history-heading">
      <div className="panel-heading history-heading-row">
        <div>
          <h2 id="history-heading">Review history</h2>
          <p>Newest reviews appear first. Search across text, sentiment, category, and summary.</p>
        </div>
        <div className="search-field">
          <label htmlFor="review-search" className="sr-only">
            Search reviews
          </label>
          <input
            id="review-search"
            type="search"
            placeholder="Search reviews…"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
          />
        </div>
      </div>

      <div className="history-body">
        {isLoading && (
          <div className="state-block" role="status">
            <p>Loading reviews…</p>
          </div>
        )}

        {!isLoading && error && (
          <div className="state-block error-state" role="alert">
            <p>{error}</p>
            <button type="button" className="secondary-button" onClick={onRetry}>
              Try again
            </button>
          </div>
        )}

        {!isLoading && !error && filtered.length === 0 && (
          <div className="state-block" role="status">
            <p>
              {reviews.length === 0
                ? 'No reviews yet. Submit the first customer review to get started.'
                : 'No reviews match your search.'}
            </p>
          </div>
        )}

        {!isLoading && !error && filtered.length > 0 && (
          <div className="table-wrap">
            <table className="reviews-table">
              <thead>
                <tr>
                  <th scope="col">Review</th>
                  <th scope="col">Sentiment</th>
                  <th scope="col">Category</th>
                  <th scope="col">AI Summary</th>
                  <th scope="col">Submitted At</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((review) => (
                  <tr key={review.id}>
                    <td>
                      <button
                        type="button"
                        className="link-button review-preview"
                        onClick={() => setSelectedReview(review)}
                        aria-label="View full review"
                      >
                        {truncate(review.reviewText)}
                      </button>
                    </td>
                    <td>
                      <SentimentBadge sentiment={review.sentiment} />
                    </td>
                    <td>{review.category}</td>
                    <td>{review.summary}</td>
                    <td>
                      <time dateTime={review.createdAt}>{formatTimestamp(review.createdAt)}</time>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {selectedReview && (
        <div
          className="modal-backdrop"
          role="presentation"
          onClick={() => setSelectedReview(null)}
        >
          <div
            className="modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="review-dialog-title"
            onClick={(event) => event.stopPropagation()}
          >
            <div className="modal-header">
              <h3 id="review-dialog-title">Full review</h3>
              <button
                type="button"
                className="icon-button"
                aria-label="Close review details"
                onClick={() => setSelectedReview(null)}
              >
                Close
              </button>
            </div>
            <div className="modal-meta">
              <SentimentBadge sentiment={selectedReview.sentiment} />
              <span>{selectedReview.category}</span>
              <time dateTime={selectedReview.createdAt}>
                {formatTimestamp(selectedReview.createdAt)}
              </time>
            </div>
            <p className="modal-summary">
              <strong>AI summary:</strong> {selectedReview.summary}
            </p>
            <p className="modal-body">{selectedReview.reviewText}</p>
          </div>
        </div>
      )}
    </section>
  );
}
