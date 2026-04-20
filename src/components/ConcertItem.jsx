import React from "react";

function ConcertItem({ concert }) {
  const content = (
    <>
      <h3 className="concert-band">{concert.band}</h3>
      {concert.genres && concert.genres.length > 0 && (
        <div className="concert-genres">
          {concert.genres.map((genre) => (
            <span key={genre} className="genre-badge">
              {genre}
            </span>
          ))}
        </div>
      )}
      <p className="concert-venue">{concert.venue}</p>
    </>
  );

  if (concert.url) {
    return (
      <li>
        <a className="concert-card concert-link" href={concert.url} target="_blank" rel="noopener noreferrer">
          {content}
        </a>
      </li>
    );
  }

  return <li className="concert-card">{content}</li>;
}

export default ConcertItem;
