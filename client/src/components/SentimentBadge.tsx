import type { Sentiment } from '../types/review';

interface SentimentBadgeProps {
  sentiment: Sentiment;
}

export function SentimentBadge({ sentiment }: SentimentBadgeProps) {
  const tone = sentiment.toLowerCase();
  return <span className={`sentiment-badge sentiment-${tone}`}>{sentiment}</span>;
}
