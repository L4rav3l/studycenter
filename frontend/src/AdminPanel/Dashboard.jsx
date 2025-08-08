import React, { useEffect, useState } from "react";

const Dashboard = () => {
  const [username, setUsername] = useState("User");
  const [weather, setWeather] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [coords, setCoords] = useState(null);

  // Get username from localStorage
  useEffect(() => {
    const storedName = localStorage.getItem("username");
    if (storedName) setUsername(storedName);
  }, []);

  // Get user location
  useEffect(() => {
    if (!navigator.geolocation) {
      setError("Geolocation is not supported by your browser");
      setLoading(false);
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        setCoords({
          lat: position.coords.latitude.toFixed(4),
          lon: position.coords.longitude.toFixed(4),
        });
      },
      () => {
        setError("Permission denied to access location");
        setLoading(false);
      }
    );
  }, []);

  // Fetch weather from wttr.in using coords
  useEffect(() => {
    if (!coords) return;

    const fetchWeather = async () => {
      setLoading(true);
      setError(null);
      try {
        // wttr.in accepts location as lat,lon or city name
        const loc = `${coords.lat},${coords.lon}`;
        const res = await fetch(`https://wttr.in/${loc}?format=j1`);
        if (!res.ok) throw new Error("Failed to fetch weather data");
        const data = await res.json();

        setWeather({
          temp: data.current_condition[0].temp_C,
          desc: data.current_condition[0].weatherDesc[0].value,
          city: data.nearest_area[0].areaName[0].value,
        });
      } catch (e) {
        setError(e.message);
      } finally {
        setLoading(false);
      }
    };

    fetchWeather();
  }, [coords]);

  return (
    <div className="min-h-screen bg-gradient-to-r from-blue-200 via-white to-green-200 flex flex-col items-center justify-center p-8 font-sans">
      <h1 className="text-4xl font-bold mb-6 text-gray-800">Welcome, {username}!</h1>

      <div className="bg-white rounded-xl shadow-lg p-6 flex flex-col items-center max-w-sm w-full">
        {loading ? (
          <p className="text-gray-600">Loading weather...</p>
        ) : error ? (
          <p className="text-red-500">{error}</p>
        ) : weather ? (
          <>
            <p className="text-6xl mb-2">{weather.temp}°C</p>
            <p className="capitalize text-gray-600 text-xl">{weather.desc}</p>
            <p className="text-gray-500 text-sm mt-1">{weather.city}</p>
          </>
        ) : (
          <p className="text-gray-600">Weather data not available</p>
        )}
      </div>
    </div>
  );
};

export default Dashboard;
