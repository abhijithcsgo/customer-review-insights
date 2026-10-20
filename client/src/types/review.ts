export type Sentiment = 'Positive' | 'Neutral' | 'Negative';

export type Category =
  | 'Pricing'
  | 'Bug'
  | 'Usability'
  | 'Feature Request'
  | 'Other';

export interface Review {
  id: number;
  reviewText: string;
  sentiment: Sentiment;
  category: Category;
  summary: string;
  createdAt: string;
}

export interface ApiError {
  error: string;
  detail?: string;
  validationErrors?: Record<string, string[]>;
}

export interface CreateReviewRequest {
  reviewText: string;
}

export const MAX_REVIEW_LENGTH = 5000;
