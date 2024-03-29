import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";

const EditTaskForm = () => {
  const {taskId} = useParams(); //Grab the task ID from the URL
  const navigate = useNavigate(); 
  const [formData, setFormData] = useState({
    month: "",
    contact: "",
    taskName: "",
    status: "",
    email: "",
    phone: "",
    notes: "",
    eventID: "",
  });
  
  useEffect(() => {
    // Fetch task data from the server
    const fetchTaskData = async () => {
      try {
        const response = await fetch(`http://localhost:5000/Task/gettask/${taskId}`);
        if (!response.ok) throw new Error('Could not fetch task data');
        const data = await response.json();
        setFormData(data); // Assuming the API returns the exact task object format
      } catch (error) {
        console.error('Fetch error:', error);
      }
    };

    fetchTaskData();
  }, [taskId]);

  // useEffect(() => {
  //   const fetchTaskData = async (taskId) => {
  //     try {
  //       const response = await fetch(
  //         `http://localhost:5000/Task/showById/${taskId}`,
  //         {
  //           method: "GET",
  //           headers: {
  //             "Content-Type": "application/json",
  //           },
  //         }
  //       );

  //       if (!response.ok) {
  //         throw new Error("Network response was not ok");
  //       }
  //       const taskData = await response.json();
  //       setFormData(taskData);
  //     } catch (error) {
  //       console.error("Error fetching event data:", error);
  //     }
  //   };
  //   fetchTaskData(taskId); // Pass taskId as an argument here
  // }, [taskId]);

  // useEffect(() => {
  //   var currentUrl = window.location.pathname;
  //   var parts = currentUrl.split("/");
  //   var uuid = parts[parts.length - 1];
  //   setFormData((prevState) => ({
  //     ...prevState,
  //     taskID: uuid,
  //   }));
  // }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prevState) => ({ ...prevState, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    // onSubmit(formData);
    try {
      const response = await fetch("http://localhost:5000/Task/edit", {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(formData),
      });

      if (!response.ok) {
        throw new Error("Network response was not ok");
      }

      alert("Task updated successfully");
      
      // setTimeout(function () {
      //   window.location.href = `/eventlist/`;
      // }, 1000);

      navigate(`/task/${formData.eventID}`);
  

      console.log("Data updated successfully");
    } catch (error) {
      console.error("Error:", error);
    }
  };
     
  return (
    <>
      <form onSubmit={handleSubmit} className="edit-task-form">
        <label>
          Month
          <input
            type="text"
            name="month"
            value={formData.month}
            onChange={handleChange}
          />
        </label>
        <label>
          Contact
          <input
            type="text"
            name="contact"
            value={formData.contact}
            onChange={handleChange}
          />
        </label>
        <label>
           TaskName
          <input
            type="text"
            name="taskName"
            //placeholder="Edit event time here..."
            value={formData.taskName}
            onChange={handleChange}
          />
        </label>
        <label>
        Status
        <select
          name="status"
          value={formData.status}
          onChange={handleChange}
        >
          <option value="">Select status</option>
          <option value="Not Started">Not Started</option>
          <option value="Pending">Pending</option>
          <option value="In Progress">In Progress</option>
          <option value="Completed">Completed</option>
          
        </select>
      </label>

        <label>
          Email
          <input
            type="text"
            name="email"
            //placeholder="Edit event venue here..."
            value={formData.email}
            onChange={handleChange}
          />
        </label>
        <label>
          Phone
          <input
            type="text"
            name="phone"
            //placeholder="Edit event city here..."
            value={formData.phone}
            onChange={handleChange}
          />
        </label>
        <label>
          Notes
          <input
            type="text"
            name="notes"
            //placeholder="Edit contact information here..."
            value={formData.notes}
            onChange={handleChange}
          />
        </label>
        <div className="form-buttons">
        <button  className="EditTask" type="submit">Submit
        </button>
        </div>   
      </form>
    </>
  );
};

// Default props for the form in case you need default values
// EditTaskForm.defaultProps = {
//   initialData: {
//     month: "",
//     contact: "",
//     taskName: "",
//     status: "",
//     email: "",
//     phone: "",
//     notes: "",
//     eventID: "",
//   },
//   onSubmit: () => {}, // Dummy function for example
// };

export default EditTaskForm;
