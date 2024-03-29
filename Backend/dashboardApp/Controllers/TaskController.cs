using MySql.Data.MySqlClient;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

// All endpoints must start with "/Task"
[ApiController]
[Route("[controller]")]
public class TaskController : ControllerBase
{
    private string awsRdsEndpoint = "reactblogdatabase.cf8sld5urrxi.ap-southeast-2.rds.amazonaws.com";
    private string awsRdsDatabase = "dawndatabase";
    private string awsRdsUsername = "admin";
    private string awsRdsPassword = "willawilla";
    // private string awsRdsEndpoint = "dawndatabase.chsgqwkqwl9s.ap-southeast-2.rds.amazonaws.com";
    // private string awsRdsDatabase = "dawndatabase";
    // private string awsRdsUsername = "admin";
    // private string awsRdsPassword = "Thoughthisbemadnessyetthereismethodinit1!";

    public List<Task> ReadTaskFile()
    {
        List<Task> tasks = new List<Task>();
        string filepath = "NSSPTaskList.xlsx";

        using (ExcelPackage excelFile = new ExcelPackage(new FileInfo(filepath)))
        {
            // Open the worksheet in the excel file (there is only 1)
            ExcelWorksheet worksheet = excelFile.Workbook.Worksheets[0];
            // Get row count for worksheet
            int rows = worksheet.Dimension.Rows;

            // Start at row 2 (ignore headers)
            for (int row = 2; row <= rows; row++)
            {
                // Get cell value at current row and column [?]
                Guid guid = Guid.NewGuid();
                string taskID = guid.ToString();
                string month = worksheet.Cells[row, 1].GetValue<string>();
                string contact = worksheet.Cells[row, 2].GetValue<string>();
                string taskName = worksheet.Cells[row, 3].GetValue<string>();
                //    string email = worksheet.Cells[row, 4].GetValue<string>();
                //    string phone = worksheet.Cells[row, 5].GetValue<string>();
                //    string notes = worksheet.Cells[row, 6].GetValue<string>();

                Task t = new Task(month, contact, taskName, "Not started", " ", " ", " ", taskID, " ");
                tasks.Add(t);
            }
        }
        return tasks;
    }

    public void SaveTaskstoDatabase(List<Task> tasks)
    {
        string connectionString = $"Server={awsRdsEndpoint};Database={awsRdsDatabase};User Id={awsRdsUsername};Password={awsRdsPassword}";
        using MySqlConnection connection = new MySqlConnection(connectionString);
        connection.Open();

        foreach (Task t in tasks)
        {
            string insertQuery = "INSERT INTO tasklist (month, contact, taskName, status, email, phone, notes, taskID, eventID) VALUES (@month, @contact, @taskName, @status, @email, @phone, @notes, @taskID, @eventID)";
            using MySqlCommand command = new MySqlCommand(insertQuery, connection);

            command.Parameters.AddWithValue("@month", t.month);
            command.Parameters.AddWithValue("@contact", t.contact);
            command.Parameters.AddWithValue("@taskName", t.taskName);
            command.Parameters.AddWithValue("@status", t.status);
            command.Parameters.AddWithValue("@email", t.email);
            command.Parameters.AddWithValue("@phone", t.phone);
            command.Parameters.AddWithValue("@notes", t.notes);
            command.Parameters.AddWithValue("@taskID", t.taskID);
            // HARD CODE EVENT ID
            command.Parameters.AddWithValue("@eventID", "5c2904ba-564c-411c-9845-571f4a719866");
            command.ExecuteNonQuery();
        }
    }

    [HttpGet("printtasks/{Id}")]
    public List<Task> ReadTasksFromDatabase(string Id)
    {
        List<Task> tasks = new List<Task>();

        string connectionString = $"Server={awsRdsEndpoint};Database={awsRdsDatabase};User Id={awsRdsUsername};Password={awsRdsPassword}";
        using MySqlConnection connection = new MySqlConnection(connectionString);
        connection.Open();

        string query = "SELECT * FROM tasklist WHERE eventID = @id";

        using (MySqlCommand command = new MySqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@id", Id);

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Task t = new Task(
                        reader.GetString("month"),
                        reader.GetString("contact"),
                        reader.GetString("taskName"),
                        reader.GetString("status"),
                        reader.GetString("email"),
                        reader.GetString("phone"),
                        reader.GetString("notes"),
                        reader.GetString("taskID"),
                        reader.GetString("eventID")
                    );
                    tasks.Add(t);
                }
            }
        }
        return tasks;
    }

    [HttpGet("gettask/{taskId}")]
    public ActionResult<Task> GetTaskById(string taskId)
    {
        try
        {
            Task task = null;
            string connectionString = $"Server={awsRdsEndpoint};Database={awsRdsDatabase};User Id={awsRdsUsername};Password={awsRdsPassword}";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM tasklist WHERE taskID = @taskId";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@taskId", taskId);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            task = new Task(
                                reader.GetString("month"),
                                reader.GetString("contact"),
                                reader.GetString("taskName"),
                                reader.GetString("status"),
                                reader.GetString("email"),
                                reader.GetString("phone"),
                                reader.GetString("notes"),
                                reader.GetString("taskID"),
                                reader.GetString("eventID")
                            );
                        }
                    }
                }
            }

            if (task != null)
            {
                return Ok(task);
            }
            else
            {
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }



    // Use endpoint "/Task/returntasks" to test this function ("it works")
    [HttpGet("returntasks")]
    public ActionResult<IEnumerable<Task>> callSaveMethods()
    {
        try
        {
            SaveTaskstoDatabase(ReadTaskFile());

            return Ok("IT WORKS... (TASKS)");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("edit")]
    public IActionResult EditTask([FromBody] Task taskData)
    {
        try
        {
            string connectionString = $"Server={awsRdsEndpoint};Database={awsRdsDatabase};User Id={awsRdsUsername};Password={awsRdsPassword}";
            using MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            string updateQuery = "UPDATE tasklist SET month = @month, contact = @contact, taskName = @taskName, status = @status, email = @email, phone = @phone, notes = @notes WHERE taskID = @taskID";
            using MySqlCommand command = new MySqlCommand(updateQuery, connection);

            command.Parameters.AddWithValue("@month", taskData.month);
            command.Parameters.AddWithValue("@contact", taskData.contact);
            command.Parameters.AddWithValue("@taskName", taskData.taskName);
            command.Parameters.AddWithValue("@status", taskData.status);
            command.Parameters.AddWithValue("@email", taskData.email);
            command.Parameters.AddWithValue("@phone", taskData.phone);
            command.Parameters.AddWithValue("@notes", taskData.notes);
            command.Parameters.AddWithValue("@taskID", taskData.taskID);

            command.ExecuteNonQuery();

            return Ok("Task data updated successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("deletetask/{id}")]
    public IActionResult DeleteTask(string id)
    {
        try
        {
            string connectionString = $"Server={awsRdsEndpoint};Database={awsRdsDatabase};User Id={awsRdsUsername};Password={awsRdsPassword}";
            using MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            string taskId = id;

            string deleteQuery = "DELETE FROM tasklist WHERE taskID = @Id";
            using MySqlCommand command = new MySqlCommand(deleteQuery, connection);

            command.Parameters.AddWithValue("@Id", taskId);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                return Ok("Data deleted successfully");
            }
            else
            {
                return NotFound("No matching data found for deletion");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("addtask")]
    public IActionResult AddTaskToDatabase([FromBody] Task taskdata)
    {
        try
        {
            string connectionString = $"Server={awsRdsEndpoint};Database={awsRdsDatabase};User Id={awsRdsUsername};Password={awsRdsPassword}";

            using MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            string insertQuery = "INSERT INTO tasklist (month, contact, taskName, status, email, phone, notes, taskID, eventID) " +
                                 "VALUES (@month, @contact, @taskName, @status, @email, @phone, @notes, @taskID, @eventID)";

            using MySqlCommand command = new MySqlCommand(insertQuery, connection);

            command.Parameters.AddWithValue("@month", taskdata.month);
            command.Parameters.AddWithValue("@contact", taskdata.contact);
            command.Parameters.AddWithValue("@taskName", taskdata.taskName);
            command.Parameters.AddWithValue("@status", taskdata.status);
            command.Parameters.AddWithValue("@email", taskdata.email);
            command.Parameters.AddWithValue("@phone", taskdata.phone);
            command.Parameters.AddWithValue("@notes", taskdata.notes);

            // Generate new unique taskID
            Guid addguid = Guid.NewGuid();
            string newTaskID = addguid.ToString();
            command.Parameters.AddWithValue("@taskID", newTaskID);

            command.Parameters.AddWithValue("@eventID", taskdata.eventID);
            command.ExecuteNonQuery();

            return Ok("new Task added successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    public record Task(string month, string contact, string taskName, string status, string email, string phone, string notes, string taskID, string eventID)
    {
    }
}