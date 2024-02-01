import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { EventRow } from "./EventRow";

const EventTable = () => {
  const [events, setEvents] = useState([]);
  const [deletedId, setDeletedId] = useState(null);

  const handleAddEventClick = () => {
    console.log("Add Event button clicked");
    window.location.href = "http://localhost:3000/add";
  };

  const fetchEvents = () => {
    fetch("http://localhost:5000/Event/printevents")
      .then((response) => response.json())
      .then((data) => {
        setEvents(data);
      })
      .catch((error) => {
        console.error("Error fetching events:", error);
      });
  };
  // add delete function
  const onDelete = async (id) => {
    try {
      const response = await fetch(`http://localhost:5000/Event/delete/${id}`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
      });

      if (!response.ok) {
        throw new Error("Network response was not ok");
      }
      console.log("Event deleted successfully");
      setDeletedId(id);
    } catch (error) {
      console.error("Error:", error);
    }
  };

  useEffect(() => {
    fetchEvents();
  }, [deletedId]); // Fetch events again when an event is deleted

  return (
    <div className="parent-container">
      <div className="EventTable">
        <h2>Event List</h2>
        {/* <Link id="link-container" to="/add"> */}
        <button className="AddEvent" onClick={handleAddEventClick}>
          Add Event
        </button>
        {/* </Link> */}
        <table className="EventTableMainTable">
          <thead>
            <tr>
              <th>Id</th>
              <th>Start Date</th>
              <th>End Date</th>
              <th>Time</th>
              <th>EventName</th>
              <th>Venue</th>
              <th>City</th>
              <th>Contact</th>
              <th>Notes</th>
              <th>Edit & Delete</th>
            </tr>
          </thead>
          <tbody>
            {events.map((event) => (
              <EventRow key={event.id} event={event} onDelete={onDelete} />
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default EventTable;
