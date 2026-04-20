import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import App from '../App';

const mockConcerts = [
  {
    date: '2025-05-21T19:00:00+00:00',
    concerts: [
      { band: 'Test Band', genres: ['Rock', 'Metal'], venue: 'Rockhal' },
      { band: 'Another Band', genres: ['Jazz'], venue: 'Atelier' },
    ],
  },
];

function mockFetchSuccess(data) {
  globalThis.fetch = vi.fn().mockResolvedValue({
    ok: true,
    json: () => Promise.resolve(data),
  });
}

function mockFetchError(message = 'Network error') {
  globalThis.fetch = vi.fn().mockRejectedValue(new Error(message));
}

describe('App', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('shows loading indicator on initial render', () => {
    // fetch never resolves so we stay in loading state
    globalThis.fetch = vi.fn().mockReturnValue(new Promise(() => {}));
    render(<App />);
    expect(screen.getByText(/loading concerts/i)).toBeInTheDocument();
  });

  it('renders concert data after successful fetch', async () => {
    mockFetchSuccess(mockConcerts);
    render(<App />);

    expect(await screen.findByText('Test Band')).toBeInTheDocument();
    expect(screen.getByText('Another Band')).toBeInTheDocument();
    expect(screen.getByText('Rockhal')).toBeInTheDocument();
    expect(screen.getByText('Atelier')).toBeInTheDocument();
    expect(screen.getByText('Rock')).toBeInTheDocument();
    expect(screen.getByText('Jazz')).toBeInTheDocument();
  });

  it('shows error message when fetch fails', async () => {
    mockFetchError('Network error');
    render(<App />);

    expect(await screen.findByText('Network error')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument();
  });

  it('retries fetch when retry button is clicked', async () => {
    mockFetchError('Network error');
    render(<App />);

    const retryButton = await screen.findByRole('button', { name: /retry/i });
    expect(globalThis.fetch).toHaveBeenCalledTimes(1);

    // Now make fetch succeed on retry
    mockFetchSuccess(mockConcerts);
    await userEvent.click(retryButton);

    expect(await screen.findByText('Test Band')).toBeInTheDocument();
    expect(globalThis.fetch).toHaveBeenCalledTimes(1); // new mock, called once
  });

  it('shows empty state when fetch returns no concerts', async () => {
    mockFetchSuccess([]);
    render(<App />);

    await waitFor(() => {
      expect(screen.queryByText(/loading concerts/i)).not.toBeInTheDocument();
    });
    expect(screen.getByText(/no concerts found/i)).toBeInTheDocument();
  });
});
