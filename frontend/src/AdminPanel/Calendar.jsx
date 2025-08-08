import React, { useEffect, useState } from "react";
import axios from "axios";

const API_URL = process.env.REACT_APP_API_URL;

const daysOfWeek = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

function getDaysInMonth(year, month) {
  const date = new Date(year, month, 1);
  const days = [];
  while (date.getMonth() === month) {
    days.push(new Date(date));
    date.setDate(date.getDate() + 1);
  }
  return days;
}

const CalendarPage = () => {
  const [selectedDate, setSelectedDate] = useState(new Date());
  const [events, setEvents] = useState([]);
  const [selectedDay, setSelectedDay] = useState(null);

  const [editingEvent, setEditingEvent] = useState(null);

  const [eventTitle, setEventTitle] = useState("");
  const [eventDate, setEventDate] = useState("");
  const [eventNotificationDate, setEventNotificationDate] = useState("");

  const token = localStorage.getItem("token");
  const authHeader = { headers: { Authorization: `Bearer ${token}` } };

  const fetchEvents = async () => {
    try {
      const from = `${selectedDate.getFullYear()}-${String(
        selectedDate.getMonth() + 1
      ).padStart(2, "0")}-01`;
      const res = await axios.get(`/api/calendar/get?From=${from}`, authHeader);

      const mappedEvents = res.data.map((e, i) => ({
        Id: e.ID,
        Title: e.TITLE || "",
        Descriptions: e.descriptions || "",
        Date: e.date,
        Notification_date: e.Notification_date || null,
      }));
      setEvents(mappedEvents);
    } catch (e) {
      console.error("Error fetching events", e);
    }
  };

  useEffect(() => {
    fetchEvents();
    setSelectedDay(null);
    setEditingEvent(null);
    setEventTitle("");
    setEventDate("");
    setEventNotificationDate("");
  }, [selectedDate]);

  const days = getDaysInMonth(selectedDate.getFullYear(), selectedDate.getMonth());

  const eventsForDay = (day) => {
    const dayStr = day.toISOString().slice(0, 10);
    return events.filter((ev) => ev.Date.slice(0, 10) === dayStr);
  };

  const handleAddEvent = async () => {
    if (!eventTitle.trim() || !eventDate) {
      alert("Please provide a title and date!");
      return;
    }
    try {
      await axios.post(
        `/api/calendar/event/create`,
        {
          Date: new Date(eventDate).toISOString(),
          Notification_date: eventNotificationDate ? new Date(eventNotificationDate).toISOString() : null,
          Title: eventTitle.trim(),
          Descriptions: "",
        },
        authHeader
      );
      resetForm();
      fetchEvents();
    } catch (e) {
      console.error("Error creating event", e);
    }
  };

  const startEditing = (ev) => {
    setEditingEvent(ev);
    setEventTitle(ev.Title);
    setEventDate(ev.Date.slice(0, 16));
    setEventNotificationDate(ev.Notification_date ? ev.Notification_date.slice(0, 16) : "");
    setSelectedDay(new Date(ev.Date));
  };

  const saveEdit = async () => {
    if (!eventTitle.trim() || !eventDate || !editingEvent) {
      alert("Please provide a title and date!");
      return;
    }
    try {
      await axios.post(
        `/api/calendar/event/edit`,
        {
          Id: editingEvent.Id,
          Date: new Date(eventDate).toISOString(),
          Notification_date: eventNotificationDate ? new Date(eventNotificationDate).toISOString() : null,
          Title: eventTitle.trim(),
          Descriptions: editingEvent.Descriptions || "",
        },
        authHeader
      );
      resetForm();
      fetchEvents();
    } catch (e) {
      console.error("Error editing event", e);
    }
  };

  const resetForm = () => {
    setEditingEvent(null);
    setEventTitle("");
    setEventDate("");
    setEventNotificationDate("");
    setSelectedDay(null);
  };

  const deleteEvent = async (id) => {
    if (!window.confirm("Are you sure you want to delete this event?")) return;

    try {
      await axios.post(`/api/calendar/event/delete`, { Id: id }, authHeader);
      if (editingEvent?.Id === id) {
        resetForm();
      }
      fetchEvents();
    } catch (e) {
      console.error("Error deleting event", e);
    }
  };

  const firstDayOfWeek = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), 1).getDay();

  // Month navigation handlers
  const goToPreviousMonth = () => {
    setSelectedDate(new Date(selectedDate.getFullYear(), selectedDate.getMonth() - 1, 1));
    resetForm();
  };

  const goToNextMonth = () => {
    setSelectedDate(new Date(selectedDate.getFullYear(), selectedDate.getMonth() + 1, 1));
    resetForm();
  };

  return (
    <div className="max-w-4xl mx-auto p-6 bg-gray-50 min-h-screen">
      <h1 className="text-4xl font-bold mb-6 text-center">Wall Calendar</h1>

      <div className="flex justify-center items-center mb-6 space-x-4">
        <button
          onClick={goToPreviousMonth}
          className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
        >
          Previous
        </button>
        <input
          type="month"
          className="border border-gray-300 rounded px-3 py-2"
          value={`${selectedDate.getFullYear()}-${String(selectedDate.getMonth() + 1).padStart(2, "0")}`}
          onChange={(e) => {
            const [year, month] = e.target.value.split("-");
            setSelectedDate(new Date(year, month - 1, 1));
            resetForm();
          }}
        />
        <button
          onClick={goToNextMonth}
          className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
        >
          Next
        </button>
      </div>

      <div className="grid grid-cols-7 gap-1 text-center font-semibold text-gray-700 mb-1">
        {daysOfWeek.map((day) => (
          <div key={day} className="p-2 border-b border-gray-300">
            {day}
          </div>
        ))}
      </div>

      <div
        className="grid grid-cols-7 gap-1 border border-gray-300 rounded bg-white"
        style={{ minHeight: "400px" }}
      >
        {[...Array(firstDayOfWeek).keys()].map((i) => (
          <div key={"empty-" + i} className="border border-gray-200 bg-gray-100" />
        ))}

        {days.map((day) => {
          const dayEvents = eventsForDay(day);
          const isSelected = selectedDay && day.toDateString() === selectedDay.toDateString();

          return (
            <div
              key={day.toISOString()}
              className={`border border-gray-300 p-2 cursor-pointer flex flex-col justify-start h-28 ${
                isSelected ? "bg-blue-100" : "hover:bg-blue-50"
              }`}
              onClick={() => {
                setSelectedDay(day);
                setEditingEvent(null);
                setEventTitle("");
                setEventDate("");
                setEventNotificationDate("");
              }}
            >
              <div className="text-right font-semibold">{day.getDate()}</div>

              <div className="mt-1 flex flex-col space-y-1 overflow-y-auto flex-grow">
                {dayEvents.map((ev) => (
                  <div
                    key={ev.Id}
                    className="bg-blue-400 text-white rounded px-1 text-xs truncate flex justify-between items-center"
                    title={ev.Title}
                  >
                    <span>{ev.Title}</span>
                    <div className="space-x-1">
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          startEditing(ev);
                          setSelectedDay(day);
                        }}
                        className="hover:text-yellow-300"
                        title="Edit"
                      >
                        ✏️
                      </button>
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          deleteEvent(ev.Id);
                        }}
                        className="hover:text-red-500"
                        title="Delete"
                      >
                        🗑️
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          );
        })}
      </div>

      <div className="mt-6 max-w-md mx-auto bg-white p-4 rounded shadow">
        <h2 className="text-xl font-semibold mb-4">
          {selectedDay
            ? `Events for: ${selectedDay.toLocaleDateString()}`
            : "Click a day to view or add events"}
        </h2>

        {selectedDay && (
          <>
            <ul className="mb-4 max-h-40 overflow-y-auto border border-gray-300 rounded p-2">
              {eventsForDay(selectedDay).length === 0 && <li>No events for this day.</li>}
              {eventsForDay(selectedDay).map((ev) => (
                <li
                  key={ev.Id}
                  className={`border-b border-gray-200 py-1 truncate ${
                    editingEvent?.Id === ev.Id ? "font-bold" : ""
                  }`}
                >
                  {ev.Title}
                </li>
              ))}
            </ul>

            <div className="flex flex-col space-y-3">
              <input
                type="text"
                className="border border-gray-300 rounded px-3 py-2"
                placeholder="Event title"
                value={eventTitle}
                onChange={(e) => setEventTitle(e.target.value)}
              />

              <label className="flex flex-col text-sm">
                Date and time:
                <input
                  type="datetime-local"
                  className="border border-gray-300 rounded px-3 py-2 mt-1"
                  value={eventDate}
                  onChange={(e) => setEventDate(e.target.value)}
                />
              </label>

              <label className="flex flex-col text-sm">
                Notification date (optional):
                <input
                  type="datetime-local"
                  className="border border-gray-300 rounded px-3 py-2 mt-1"
                  value={eventNotificationDate}
                  onChange={(e) => setEventNotificationDate(e.target.value)}
                />
              </label>

              <div className="flex space-x-2">
                {editingEvent ? (
                  <>
                    <button
                      onClick={saveEdit}
                      className="bg-yellow-500 text-white px-4 py-2 rounded hover:bg-yellow-600"
                    >
                      Save
                    </button>
                    <button
                      onClick={resetForm}
                      className="bg-gray-300 px-4 py-2 rounded hover:bg-gray-400"
                    >
                      Cancel
                    </button>
                  </>
                ) : (
                  <button
                    onClick={handleAddEvent}
                    className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
                  >
                    Add
                  </button>
                )}
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default CalendarPage;
