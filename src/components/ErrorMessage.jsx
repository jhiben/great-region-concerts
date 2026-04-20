import React from "react";

function ErrorMessage({ message, onRetry }) {
  return (
    <div className="error-message">
      <p className="error-text">{message}</p>
      <button className="retry-button" onClick={onRetry}>
        Retry
      </button>
    </div>
  );
}

export default ErrorMessage;
