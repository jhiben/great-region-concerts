import React, { useState, useEffect, useCallback } from "react";
import ConcertList from "./components/ConcertList";
import LoadingSpinner from "./components/LoadingSpinner";
import ErrorMessage from "./components/ErrorMessage";

function App() {
  const [concerts, setConcerts] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchConcerts = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await fetch("/api/concerts");
      if (!response.ok) {
        throw new Error(`Failed to load concerts (${response.status})`);
      }
      const data = await response.json();
      setConcerts(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchConcerts();
  }, [fetchConcerts]);

  if (isLoading) {
    return (
      <div className="container">
        <LoadingSpinner />
      </div>
    );
  }

  if (error) {
    return (
      <div className="container">
        <ErrorMessage message={error} onRetry={fetchConcerts} />
      </div>
    );
  }

  if (concerts.length === 0) {
    return (
      <div className="container">
        <p className="empty-message">No concerts found.</p>
      </div>
    );
  }

  return (
    <div className="container">
      <ConcertList concertDates={concerts} />
    </div>
  );
}

export default App;
