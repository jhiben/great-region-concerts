import React, { useState, useMemo } from "react";

function CalendarView({ concertDates }) {
  const today = new Date();
  const [currentMonth, setCurrentMonth] = useState(today.getMonth());
  const [currentYear, setCurrentYear] = useState(today.getFullYear());

  // Build a map of date string → concerts array
  const concertsByDate = useMemo(() => {
    const map = {};
    for (const group of concertDates) {
      const d = new Date(group.date);
      const key = `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
      map[key] = group.concerts;
    }
    return map;
  }, [concertDates]);

  const [selectedDate, setSelectedDate] = useState(null);

  const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
  const firstDayOfWeek = new Date(currentYear, currentMonth, 1).getDay();
  // Adjust so Monday = 0
  const startOffset = (firstDayOfWeek + 6) % 7;

  const monthNames = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December",
  ];

  const dayNames = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

  function prevMonth() {
    if (currentMonth === 0) {
      setCurrentMonth(11);
      setCurrentYear(currentYear - 1);
    } else {
      setCurrentMonth(currentMonth - 1);
    }
    setSelectedDate(null);
  }

  function nextMonth() {
    if (currentMonth === 11) {
      setCurrentMonth(0);
      setCurrentYear(currentYear + 1);
    } else {
      setCurrentMonth(currentMonth + 1);
    }
    setSelectedDate(null);
  }

  function isToday(day) {
    return (
      day === today.getDate() &&
      currentMonth === today.getMonth() &&
      currentYear === today.getFullYear()
    );
  }

  function getConcertsForDay(day) {
    const key = `${currentYear}-${currentMonth}-${day}`;
    return concertsByDate[key] || [];
  }

  function handleDayClick(day) {
    const concerts = getConcertsForDay(day);
    if (concerts.length > 0) {
      const key = `${currentYear}-${currentMonth}-${day}`;
      setSelectedDate(selectedDate === key ? null : key);
    }
  }

  const selectedConcerts = selectedDate ? (concertsByDate[selectedDate] || []) : [];
  const selectedDateObj = selectedDate
    ? new Date(
        parseInt(selectedDate.split("-")[0]),
        parseInt(selectedDate.split("-")[1]),
        parseInt(selectedDate.split("-")[2])
      )
    : null;

  // Build grid cells
  const cells = [];
  for (let i = 0; i < startOffset; i++) {
    cells.push(<div key={`empty-${i}`} className="cal-cell cal-empty" />);
  }
  for (let day = 1; day <= daysInMonth; day++) {
    const concerts = getConcertsForDay(day);
    const hasConcerts = concerts.length > 0;
    const key = `${currentYear}-${currentMonth}-${day}`;
    const isSelected = selectedDate === key;

    cells.push(
      <div
        key={day}
        className={[
          "cal-cell",
          hasConcerts ? "cal-has-events" : "",
          isToday(day) ? "cal-today" : "",
          isSelected ? "cal-selected" : "",
        ]
          .filter(Boolean)
          .join(" ")}
        onClick={() => handleDayClick(day)}
      >
        <span className="cal-day-number">{day}</span>
        {hasConcerts && (
          <div className="cal-dots">
            {concerts.slice(0, 3).map((_, i) => (
              <span key={i} className="cal-dot" />
            ))}
            {concerts.length > 3 && <span className="cal-dot-more">+{concerts.length - 3}</span>}
          </div>
        )}
      </div>
    );
  }

  return (
    <div className="calendar-view">
      <div className="cal-header">
        <button className="cal-nav-btn" onClick={prevMonth} aria-label="Previous month">
          ‹
        </button>
        <h2 className="cal-title">
          {monthNames[currentMonth]} {currentYear}
        </h2>
        <button className="cal-nav-btn" onClick={nextMonth} aria-label="Next month">
          ›
        </button>
      </div>

      <div className="cal-grid">
        {dayNames.map((d) => (
          <div key={d} className="cal-cell cal-day-name">
            {d}
          </div>
        ))}
        {cells}
      </div>

      {selectedDate && selectedConcerts.length > 0 && (
        <div className="cal-detail-panel">
          <h3 className="cal-detail-date">
            {selectedDateObj.toLocaleDateString(undefined, {
              weekday: "long",
              year: "numeric",
              month: "long",
              day: "numeric",
            })}
          </h3>
          <ul className="cal-detail-list">
            {selectedConcerts.map((c) => {
              const inner = (
                <>
                  <span className="cal-detail-band">{c.band}</span>
                  <span className="cal-detail-venue">{c.venue}</span>
                  {c.genres && c.genres.length > 0 && (
                    <div className="cal-detail-genres">
                      {c.genres.map((g) => (
                        <span key={g} className="genre-badge">{g}</span>
                      ))}
                    </div>
                  )}
                </>
              );
              return c.url ? (
                <li key={`${c.band}-${c.venue}`}>
                  <a className="cal-detail-item cal-detail-link" href={c.url} target="_blank" rel="noopener noreferrer">
                    {inner}
                  </a>
                </li>
              ) : (
                <li key={`${c.band}-${c.venue}`} className="cal-detail-item">
                  {inner}
                </li>
              );
            })}
          </ul>
        </div>
      )}
    </div>
  );
}

export default CalendarView;
