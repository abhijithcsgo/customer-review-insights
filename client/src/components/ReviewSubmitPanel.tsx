import { useState } from 'react';
import type { FormEvent } from 'react';
import { MAX_REVIEW_LENGTH } from '../types/review';

interface ReviewSubmitPanelProps {
  onSubmit: (reviewText: string) => Promise<void>;
  isSubmitting: boolean;
}

export function ReviewSubmitPanel({ onSubmit, isSubmitting }: ReviewSubmitPanelProps) {
  const [reviewText, setReviewText] = useState('');
  const [validationError, setValidationError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const trimmedLength = reviewText.trim().length;
  const isEmpty = trimmedLength === 0;
  const isTooLong = reviewText.length > MAX_REVIEW_LENGTH;
  const canSubmit = !isEmpty && !isTooLong && !isSubmitting;

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSuccessMessage(null);
    setSubmitError(null);

    if (isEmpty) {
      setValidationError('Please enter a review before submitting.');
      return;
    }

    if (isTooLong) {
      setValidationError(`Review must be at most ${MAX_REVIEW_LENGTH.toLocaleString()} characters.`);
      return;
    }

    setValidationError(null);

    try {
      await onSubmit(reviewText.trim());
      setReviewText('');
      setSuccessMessage('Review analyzed and saved successfully.');
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to submit review.';
      setSubmitError(message);
    }
  }

  return (
    <section className="panel submit-panel" aria-labelledby="submit-heading">
      <div className="panel-heading">
        <h2 id="submit-heading">Submit a review</h2>
        <p>Paste customer feedback to analyze sentiment, category, and a short summary.</p>
      </div>

      <form onSubmit={handleSubmit} noValidate>
        <label htmlFor="review-text" className="field-label">
          Customer review
        </label>
        <textarea
          id="review-text"
          name="reviewText"
          rows={6}
          value={reviewText}
          onChange={(event) => {
            setReviewText(event.target.value);
            setValidationError(null);
            setSubmitError(null);
            setSuccessMessage(null);
          }}
          placeholder="Example: The checkout flow is confusing and the price feels too high for what we get."
          maxLength={MAX_REVIEW_LENGTH + 100}
          disabled={isSubmitting}
          aria-invalid={Boolean(validationError) || isTooLong}
          aria-describedby="review-help review-counter"
        />

        <div className="textarea-meta">
          <p id="review-help" className="help-text">
            Maximum {MAX_REVIEW_LENGTH.toLocaleString()} characters.
          </p>
          <p
            id="review-counter"
            className={`char-counter ${isTooLong ? 'over-limit' : ''}`}
            aria-live="polite"
          >
            {reviewText.length.toLocaleString()} / {MAX_REVIEW_LENGTH.toLocaleString()}
          </p>
        </div>

        {validationError && (
          <p className="form-message error" role="alert">
            {validationError}
          </p>
        )}
        {submitError && (
          <p className="form-message error" role="alert">
            {submitError}
          </p>
        )}
        {successMessage && (
          <p className="form-message success" role="status">
            {successMessage}
          </p>
        )}

        <button type="submit" className="primary-button" disabled={!canSubmit}>
          {isSubmitting ? 'Analyzing…' : 'Submit Review'}
        </button>
      </form>
    </section>
  );
}
