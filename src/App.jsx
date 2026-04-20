import React, { useState, useEffect, useCallback } from "react";
import ConcertList from "./components/ConcertList";
import CalendarView from "./components/CalendarView";
import LoadingSpinner from "./components/LoadingSpinner";
import ErrorMessage from "./components/ErrorMessage";

function App() {
  const [concerts, setConcerts] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [view, setView] = useState("calendar");

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
      <header className="app-header">
        <h1 className="app-title">🎵 Great Region Concerts</h1>
        <div className="view-toggle">
          <button
            className={`toggle-btn ${view === "calendar" ? "active" : ""}`}
            onClick={() => setView("calendar")}
          >
            📅 Calendar
          </button>
          <button
            className={`toggle-btn ${view === "list" ? "active" : ""}`}
            onClick={() => setView("list")}
          >
            📋 List
          </button>
        </div>
      </header>
      {view === "calendar" ? (
        <CalendarView concertDates={concerts} />
      ) : (
        <ConcertList concertDates={concerts} />
      )}
    </div>
  );
}

export default App;
