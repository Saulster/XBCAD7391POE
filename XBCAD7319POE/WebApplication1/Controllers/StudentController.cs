using Amazon.S3.Model;
using Backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
/// <summary>
/// This is the student api controller class.. where all the magic happens
/// Authors: Sven Masche, Allana Morris
/// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        //global declaration
        private DBController _dbController;

        /// <summary>
        ///  General constructor that sets up the global db controller class
        /// </summary>
        public StudentController()
        {
            _dbController = new DBController();
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// api call to get a total count of students, only used for testing in swagger
        /// Author: Sven Masche
        /// </summary>
        [HttpGet("count")]
        public IActionResult GetStudentCount()
        {
            var count = _dbController.getCount();
            return Ok(count);  //return count as string
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Api call to get all the students and return as a list
        /// Author: Sven Masche
        /// </summary>
        [HttpGet("all")]
        public IActionResult GetAllStudents()
        {
            var students = _dbController.GetAllStudentStatuses();
            return Ok(students);  // Return list of students
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Api call to check the survey completion of a single student, based on input id
        /// Author: Sven Masche
        /// </summary>
        [HttpGet("singleCheck/{studentId}")]
        public async Task<IActionResult> checkSingleStud(string studentId)
        {
            surveyMonkey surve = new surveyMonkey();

            bool check = surve.checkStudent(studentId).Result;
            return await UpdateStudentStatus(studentId, check);
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Api call to run a check on a file input to read csv contents and return list of student ID's
        /// Author: Allana Morris
        /// </summary>
        [HttpPost("BulkCount")]
        public async Task<IActionResult> CountBulkStud([FromForm] IFormFile bulkFile)
        {
            //validating input
            if (bulkFile == null || bulkFile.Length == 0)
            {
                Console.WriteLine("No file uploaded or file is empty.");
                return BadRequest(new { code = "FILE_MISSING_OR_EMPTY", message = "No file uploaded or file is empty." });
            }

            var fileExtension = Path.GetExtension(bulkFile.FileName);
            if (fileExtension.ToLower() != ".csv")
            {
                Console.WriteLine("Only CSV files are allowed.");
                return BadRequest(new { code = "INVALID_FILE_TYPE", message = "Only CSV files are allowed." });
            }

            //setting variables
            var studentCheckID = new List<string>();
            var counter = 0;
            // Get the current year
            string currentYear = DateTime.Now.Year.ToString();

            // Construct the regex pattern with the current year
            string pattern = @"^[a-f0-9]{32}-" + currentYear + "$";

            // Create the regex object
            Regex regex = new Regex(pattern);

            try
            {
                // Read the file content and process each line.
                using (var stream = new StreamReader(bulkFile.OpenReadStream()))
                {
                    var content = await stream.ReadToEndAsync(); // Read the content asynchronously
                    var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries); // Handle both LF and CRLF line endings



                    foreach (var line in lines)
                    {
                        string studentId = line.Trim(); // Trim whitespace
                        if (string.IsNullOrWhiteSpace(studentId)) continue; // Skip empty lines

                        if (regex.IsMatch(studentId))
                        {
                            studentCheckID.Add(studentId);
                            counter++;
                        }
                    }
                    if (counter == 0)
                    {
                        return BadRequest(new { code = "NO_VALID_IDS", message = "File does not contain valid StudentIDs." });
                    }

                    Console.Write(counter);
                    return Ok(new { counter, studentCheckID });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { code = "PROCESSING_ERROR", message = $"An error occurred while processing the file: {ex.Message}" });
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Api call to perform a bulk student check with a csv file, and student id list
        /// Author: Sven Masche
        /// Edited By: Allana Morris
        /// </summary>
        [HttpPost("bulkCheck")]
        public async Task<IActionResult> checkBulkStud([FromForm] IFormFile bulkFile, [FromForm] List<string> StudentIDList)
        {
            //results variales
            var studentSurveyResults = new List<Result>();
            try
            {
                var IDList = new List<string>();
                char[] charactersToRemove = new char[] { '\\', '\"', '[', ']' };

                IDList = StudentIDList[0]
                     .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) // Split by comma and remove empty entries
                     .Select(part =>
                     charactersToRemove.Aggregate(part, (current, c) => current.Replace(c.ToString(), ""))) // Remove each character
                     .ToList();

                List<string> failedUpdates = new List<string>();
                foreach (var line in IDList)
                {
                    string studentId = line.Trim(); // Trim whitespace
                    if (string.IsNullOrWhiteSpace(studentId)) continue; // Skip empty lines

                    surveyMonkey surve = new surveyMonkey();
                    bool check = await surve.checkStudent(studentId); // Await the async call
                    var result = await UpdateStudentStatus(studentId, check) as ObjectResult;

                    // Collect failed updates for reporting.
                    if (result?.StatusCode == 404) // Not found
                    {
                        failedUpdates.Add($"Student with ID {studentId} does not exist.");
                    }
                    else if (result?.StatusCode == 400) // Bad request
                    {
                        failedUpdates.Add($"Failed to update student with ID {studentId}.");
                    }
                    else
                    {
                        // Create a new instance for each result
                        var res = new Result
                        {
                            StudentId = studentId,
                            Completed = check ? "Yes" : "No"
                        };

                        studentSurveyResults.Add(res);
                    }
                }

                // Upload the file after processing all student IDs
                BucketController bucket = new BucketController();
                var uploadResponse = bucket.UploadCSV(bulkFile);

                // Return success message along with any failed updates.
                return Ok(new
                {
                    message = "Bulk processing completed.",
                    uploadResponse,
                    failedUpdates,
                    studentSurveyResults
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred while processing the file: {ex.Message}" });
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Api call to update a given student completion status in the database
        /// Author: Sven Masche
        /// </summary>
        private async Task<IActionResult> UpdateStudentStatus(string studentId, bool check)
        {
            //Initialize the student object with common properties.
            Student student = new Student
            {
                student_id = studentId.ToString(),
                date = DateTime.Now,
                status = check ? "Yes" : "No"
            };

            //check if student even exists
            bool studentExists = _dbController.DoesStudentExist(studentId);
            if (!studentExists)
            { 
                //if not we return a not found, which should hopefully never be the case
                return NotFound(new { message = "Student does not exist." });
            }

            //if the student has been checked before, get the complete date
            if (check)
            {
                surveyMonkey monkey = new surveyMonkey();
                student.completeDate = await monkey.GetCompletionDate(studentId); // Await the async method
            }

            try
            {
                // Update the student status in the database.
                bool result = _dbController.insertStudentSurveyCompletion(student);

                // Create a response message based on the `check` value.
                string message = check ? "Student completed the survey." : "Student did not complete survey.";

                // Return the appropriate response.
                if (result)
                {
                    return Ok(new { message });
                }
                else
                {
                    return BadRequest(new { message = "Failed to update student status." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error updating student status: {ex.Message}");

                return StatusCode(500, new { message = "An error occurred while updating the student status." });
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Api call to retrieve all bulk check csv's from the bucket
        /// Author: Sven Masche
        /// </summary>
        [HttpGet("bulkHistory")]
        public async Task<IActionResult> getCSV()
        {
            BucketController bucket = new BucketController();

            //get the list of csv files
            List<S3Object> csvFiles = await bucket.ListCSVFiles();

            //map to a DTO with relevant fields
            var fileList = csvFiles.Select(file => new
            {
                FileName = file.Key,
                LastModified = file.LastModified.ToString("yyyy-MM-dd"),
                Size = file.Size
            }).ToList();

            // return the list as json
            return Ok(fileList);
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// This api call facilitates the procedures to download a csv file given just the name, 
        /// this is why names are unique in the bucket
        /// Author: Sven Masche
        /// </summary>
        [HttpGet("bulkDownload")]
        public async Task<IActionResult> downloadCSV([FromQuery] string fileName)
        {
            //Setting bucket controller
            BucketController bucket = new BucketController();

            //null check
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            try
            {
                //download the csv file
                string filePath = await bucket.DownloadCSV(fileName);

                //read the file and return it as a file stream
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                //return the file as a downloadable response
                return File(fileStream, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error downloading file: {ex.Message}");
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Api calls that holds the procedure needed to create and download a csv file based on a list of results
        /// Author: Sven Masche
        /// </summary>
        [HttpPost("resultDownload")]
        public async Task<IActionResult> DownloadResult([FromBody] List<Result> results)
        {
            try
            {
                //create the csv content
                var csvContent = new StringBuilder();
                csvContent.AppendLine("Student ID,Survey Completed");

                //add results to content
                foreach (var result in results)
                {
                    csvContent.AppendLine($"{result.StudentId},{result.Completed}");
                }

                //convert csv content to byte array
                var bytes = Encoding.UTF8.GetBytes(csvContent.ToString());

                // return csv file as a downloadable response
                return File(bytes, "text/csv", "SurveyResults.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating CSV file: {ex.Message}");
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Api call to retrieve all report related data and send to frontend
        /// Author: Sven Masche
        /// </summary>
        [HttpGet("report")]
        public async Task<IActionResult> getReportInfo(string Month, int year)
        {
            // validate Month string
            if (string.IsNullOrEmpty(Month) || !DateTime.TryParseExact(Month, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return BadRequest("Invalid month. Please provide a full month name (e.g., 'March').");
            }

            // Validate year (checking if its a reasonable time)
            if (year < 1900 || year > DateTime.Now.Year)
            {
                return BadRequest("Invalid year. Please provide a year between 1900 and the current year.");
            }

            // Convert validated month to a month number
            int monthNumber = parsedDate.Month;

            //proceed with retrieving report info
            DBController db = new DBController();
            var result = db.getReportInfo(monthNumber, year);

            return Ok(result);
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------


