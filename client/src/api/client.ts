import type { ApiError, CreateReviewRequest, Review } from '../types/review';

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '')
  ?? 'http://localhost:5056';

async function parseError(response: Response): Promise<string> {
  try {
    const data = (await response.json()) as ApiError;
    if (data.validationErrors) {
      const messages = Object.values(data.validationErrors).flat();
      if (messages.length > 0) {
        return messages.join(' ');
      }
    }

    if (data.detail) {
      return data.detail;
    }

    if (data.error) {
      return data.error;
    }
  } catch {
    // Fall through to status-based message.
  }

  return `Request failed with status ${response.status}.`;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response;

  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      headers: {
        Accept: 'application/json',
        ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
        ...init?.headers,
      },
      ...init,
    });
  } catch {
    throw new Error('Unable to reach the Customer Review Insights API. Confirm the backend is running.');
  }

  if (!response.ok) {
    throw new Error(await parseError(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export const api = {
  getReviews: () => request<Review[]>('/api/reviews'),
  getReview: (id: number) => request<Review>(`/api/reviews/${id}`),
  createReview: (payload: CreateReviewRequest) =>
    request<Review>('/api/reviews', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),
};

export { API_BASE_URL };
