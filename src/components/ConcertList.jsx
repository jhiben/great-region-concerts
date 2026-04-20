import React from "react";
import ConcertItem from "./ConcertItem";

function displayDate(dateString) {
  const date = new Date(dateString);
  return date.toLocaleString();
}

function ConcertList({ concertDates }) {
  return (
    <div className="concert-list">
      {concertDates.map((dates) => (
        <div key={dates.date} className="concert-date-group">
          <h2 className="date-heading">{displayDate(dates.date)}</h2>
          <ul className="concert-items">
            {dates.concerts.map((concert) => (
              <ConcertItem key={concert.band} concert={concert} />
            ))}
          </ul>
        </div>
      ))}
    </div>
  );
}

export default ConcertList;
