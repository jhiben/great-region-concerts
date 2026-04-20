import React, { useState, useMemo, useRef, useEffect } from "react";
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
  const todayRef = useRef(null);

  // Scroll to today's date (or nearest future) on mount
  useEffect(() => {
    if (todayRef.current) {
      todayRef.current.scrollIntoView({ behavior: "smooth", block: "start" });
    }
  }, []);

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  // Find index of first group >= today
  const todayIndex = useMemo(() => {
    return concertDates.findIndex((g) => new Date(g.date) >= today);
  }, [concertDates, today]);

  const lowerFilter = filter.toLowerCase().trim();

  const filteredDates = useMemo(() => {
    if (!lowerFilter) return concertDates;
    return concertDates
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
  }, [concertDates, lowerFilter]);

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
        <p className="empty-message">No concerts match "{filter}"</p>
      ) : (
        filteredDates.map((dates, idx) => {
          // Find original index for todayRef
          const origIdx = concertDates.indexOf(dates);
          return (
            <div
              key={dates.date}
              className="concert-date-group"
              ref={!lowerFilter && origIdx === todayIndex ? todayRef : null}
            >
              <h2 className="date-heading">{formatDate(dates.date)}</h2>
              <ul className="concert-items">
                {dates.concerts.map((concert) => (
                  <ConcertItem key={`${concert.band}-${concert.venue}`} concert={concert} />
                ))}
              </ul>
            </div>
          );
        })
      )}
    </div>
  );
}

export default ConcertList;
