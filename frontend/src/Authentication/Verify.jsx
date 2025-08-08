import React, { useEffect, useState } from "react";

const VerifyPage = () => {
  const [status, setStatus] = useState("Folyamatban...");
  const [error, setError] = useState(null);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const token = params.get("token");

    if (!token) {
      setStatus(null);
      setError("Nincs token megadva a queryben.");
      return;
    }

    fetch("/api/register/verify", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ token }),
    })
      .then(async (res) => {
        if (res.ok) {
          const data = await res.json();
          if (data.status === 1) {
            setStatus("Sikeres aktiválás!");
            setError(null);
          } else {
            setStatus(null);
            setError("Ismeretlen válasz érkezett.");
          }
        } else {
          const err = await res.json();
          if (res.status === 401 && err.error === 8) {
            setError("Érvénytelen token.");
          } else if (res.status === 409 && err.error === 7) {
            setError("A token verzió nem megfelelő (Conflict).");
          } else {
            setError("Hiba történt: " + JSON.stringify(err));
          }
          setStatus(null);
        }
      })
      .catch(() => {
        setError("Nem sikerült csatlakozni a szerverhez.");
        setStatus(null);
      });
  }, []);

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 p-4">
      <div className="bg-white p-8 rounded shadow-md max-w-md w-full text-center">
        {status && <p className="text-green-600 text-lg font-semibold">{status}</p>}
        {error && <p className="text-red-600 text-lg font-semibold">{error}</p>}
      </div>
    </div>
  );
};

export default VerifyPage;
