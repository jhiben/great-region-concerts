import React, { useState, useEffect } from "react";

function displayDate(dateString) {
  const date = new Date(dateString);

  return `${date.getDate()}/${
    date.getMonth() + 1
  }/${date.getFullYear()} at ${date.getHours()}:${date
    .getMinutes()
    .toFixed(2)}`;
}

function App() {
  const [concerts, setConcerts] = useState([]);

  useEffect(() => {
    (async function () {
      const dates = await (await fetch(`/api/concerts`)).json();
      setConcerts(dates);
    })();
  }, []);

  if (concerts.length === 0) {
    return <div>Loading...</div>;
  }

  return concerts.map((dates) => {
    return (
      <div key={dates.date}>
        <h2>{displayDate(dates.date)}</h2>
        <ul>
          {dates.concerts.map((concert) => {
            return (
              <li key={concert.band}>
                <h3>{concert.band}</h3>
                <p>{concert.genres?.join(", ")}</p>
                <p>{concert.venue}</p>
              </li>
            );
          })}
        </ul>
      </div>
    );
  });
}

export default App;
