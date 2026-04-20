import React, { useState, useMemo } from "react";
import ConcertItem from "./ConcertItem";

function formatDate(dateString) {
  const d = new Date(dateString);
  const day = String(d.getDate()).padStart(2, "0");
  const month = String(d.getMonth() + 1).padStart(2, "0");
  const year = d.getFullYear();
  return `${day}/${month}/${year}`;
}

function ConcertList({ concertDates }) {
  const [filter, setFilter] = useState("");

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const lowerFilter = filter.toLowerCase().trim();

  const filteredDates = useMemo(() => {
    // Start from today — exclude past dates
    const upcoming = concertDates.filter((g) => new Date(g.date) >= today);

    if (!lowerFilter) return upcoming;
    return upcoming
      .map((group) => {
        const matchingConcerts = group.concerts.filter((c) => {
          const text = [
            c.band,
            c.venue,
            ...(c.genres || []),
            formatDate(group.date),
          ]
            .join(" ")
            .toLowerCase();
          return text.includes(lowerFilter);
        });
        if (matchingConcerts.length === 0) return null;
        return { ...group, concerts: matchingConcerts };
      })
      .filter(Boolean);
  }, [concertDates, lowerFilter, today]);

  return (
    <div className="concert-list">
      <div className="filter-bar">
        <input
          type="text"
          className="filter-input"
          placeholder="Search bands, venues, genres…"
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
        />
        {filter && (
          <button className="filter-clear" onClick={() => setFilter("")} aria-label="Clear filter">
            ✕
          </button>
        )}
      </div>
      {filteredDates.length === 0 ? (
        <p className="empty-message">
          {lowerFilter ? `No concerts match "${filter}"` : "No upcoming concerts found."}
        </p>
      ) : (
        filteredDates.map((dates) => (
          <div key={dates.date} className="concert-date-group">
            <h2 className="date-heading">{formatDate(dates.date)}</h2>
            <ul className="concert-items">
              {dates.concerts.map((concert) => (
                <ConcertItem key={`${concert.band}-${concert.venue}`} concert={concert} />
              ))}
            </ul>
          </div>
        ))
      )}
    </div>
  );
}

export default ConcertList;
