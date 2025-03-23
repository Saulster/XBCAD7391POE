namespace WebApplication1.Controllers
{
    using Npgsql;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using WebApplication1.Models;


/// <summary>
/// This class holds all the code related to the rds postges database.
/// Author: Sven Masche
/// </summary>
    public class DBController
    {
       //global declarations
        NpgsqlConnection con;
        NpgsqlCommand cmd;

        /// <summary>
        /// Constructor sets global declarations to environment variables
        /// Author: Sven Masche
        /// </summary>
        public DBController() // Setting up connection string
        {
            string endpoint = Environment.GetEnvironmentVariable("DB_ENDPOINT");
            string username = Environment.GetEnvironmentVariable("DB_USERNAME");
            string pass = Environment.GetEnvironmentVariable("DB_PASSWORD");
            string dbName = Environment.GetEnvironmentVariable("DB_NAME");

            //checking incase env variables are null
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(dbName))
            {
                throw new InvalidOperationException("Database environment variables are not set.");
            }

            // setting Connection string
            con = new NpgsqlConnection($"Host={endpoint};Port=5432;Username={username};Password={pass};Database={dbName}");
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Simple method that just gets the count of all students in the student table.
        /// Only really used for testing purposes.
        /// Author: Sven Masche
        /// </summary>
        public string getCount()
        {
            string? count = "0";
            try
            {
                con.Open();

                //creating sql query command
                cmd = new NpgsqlCommand("SELECT COUNT(*) AS student_count FROM studentTbl;", con);

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    count = reader["student_count"].ToString();
                }

                //close the reader and connection
                reader.Close();
                con.Close();
                if(count != null) return count;
                else return "0";
            }
            catch (Exception ex)
            {
                con.Close();
                Console.WriteLine(ex.Message);
                return "oh bananas";
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Method that gets all the latest survey checks.
        /// Author: Sven Masche
        /// </summary>
        public List<Student> GetAllStudentStatuses()
        {
            //creating the list
            List<Student> listOfStudents = new List<Student>();

            con.Open();

            //sql quesry
            cmd = new NpgsqlCommand(@"
            SELECT s.studentid, sc.datechecked AS lastchecked, sc.surveystatus AS status
            FROM studentTbl s
            JOIN 
            (
            SELECT studentid, surveystatus, datechecked
            FROM surveycompletion
            WHERE (studentid, datechecked) 
                IN (
                SELECT studentid, MAX(datechecked) 
                FROM surveycompletion 
                GROUP BY studentid
            )
        ) sc ON s.studentid = sc.studentid ORDER BY sc.datechecked DESC;", con);

            NpgsqlDataReader reader = cmd.ExecuteReader();

            //read the response and pour into the list object
            while (reader.Read())
            {
                Student studentInfo = new Student
                {
                    student_id = reader["studentid"].ToString(),
                    date = Convert.ToDateTime(reader["lastchecked"]),
                    status = reader["status"].ToString()
                };

                listOfStudents.Add(studentInfo);
            }

            // Close the reader and connection
            reader.Close();
            con.Close();

            return listOfStudents;
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Simple method just checks if the inputted student id is in the database, returns true if true i guess.
        /// Author: Sven Masche
        /// </summary>
        public bool DoesStudentExist(string studentId)
        {
            //Check if the studentId is not null or empty
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentException("Student ID cannot be null or empty.", nameof(studentId));
            }

            bool studentExists = false;

            con.Open();

            //Creating query
            cmd = new NpgsqlCommand("SELECT COUNT(*) FROM studentTbl WHERE studentid = @studentId;", con);
            cmd.Parameters.AddWithValue("@studentId", studentId);

            // Execute the command and check the result, if count is more than 0 it means a student has returned and is valid.
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            studentExists = count > 0;

            // Close the connection
            con.Close();

            return studentExists;
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Code inserts a survey completion check to the survey completion table.
        /// Author: Sven Masche
        /// </summary>
        public bool insertStudentSurveyCompletion(Student student)
        {
            try
            {
                con.Open();

                //the query itself
                string query = "INSERT INTO surveycompletion (studentid, surveystatus, " +
                    "datechecked, completiondate) VALUES (@id, @status, @checkDate, @completionDate);";

                using (var cmd = new NpgsqlCommand(query, con))
                {
                    //adding parameters
                    cmd.Parameters.AddWithValue("@id", student.student_id);
                    cmd.Parameters.AddWithValue("@checkDate", student.date);
                    cmd.Parameters.AddWithValue("@status", student.status);

                    //handle nullable completion date
                    if (student.completeDate.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@completionDate", student.completeDate.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@completionDate", DBNull.Value); //explicitly set as NULL in the database
                    }

                    //execute command
                    int rowsAffected = cmd.ExecuteNonQuery();

                    //affected rows mean success
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting student survey completion: {ex.Message}");
                return false;
            }
            finally
            {
                //ensure the connection is always closed
                con.Close();
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Method gets user from the database using unique username
        /// Author: Sven Masche
        /// </summary>
        public Models.User getUser(string username)
        {
            //setting the logged user to null, just to be safe i guess
            Models.User currentUser = null;

            try
            {
                con.Open();

                //getting the user info as well as thier associated role
                cmd = new NpgsqlCommand(
                    "SELECT e.username, e.password, e.profile_image, e.name, r.rolename " +
                    "FROM employeetbl e " +
                    "JOIN employeeRoletbl er ON e.username = er.username " +
                    "JOIN roletbl r ON er.roleid = r.roleid " +
                    "WHERE e.username = @username;", con);

                //adding params
                cmd.Parameters.AddWithValue("username", username);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string role = reader["rolename"].ToString();

                        // Check if the role is admin
                        if (role != "Admin")
                        {
                            Console.WriteLine("Error: User does not have admin privileges.");
                            return null; //return null if not an admin
                        }

                        //populate user details if admin role is confirmed
                        currentUser = new Models.User
                        {
                            uName = reader["username"].ToString(),
                            password = reader["password"].ToString(),
                            profile_image = reader["profile_image"] as byte[],
                            realName = reader["name"].ToString()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user: {ex.Message}");
            }
            finally
            {
                con.Close(); // Ensure connection is closed in all scenarios
            }

            return currentUser;
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// This huuuge method gets all the info of the students created in a month and year as well as other stats being:
        /// most popular institutes, number of students created in the month, number of completed surveys of students that month 
        /// Author: Sven Masche
        /// </summary>
        public Report getReportInfo(int month, int year)
        {
            List<@Models.Student> listOfStudents = new List<@Models.Student>();
            int numComplete = 0; //total of completed surveys
            int numCreatedCompleted = 0; // total of surveys completed for each created user
            List<string> popularInstitutes = new List<string>(); //list of popular insitutes
            Dictionary<string, int> instituteCounts = new Dictionary<string, int>(); //dictionary for institutes

            con.Open();

            //this is where the fun begins

            // step 1 Get students created in the specified month and year along with their latest survey completion status
            using (var cmd = new NpgsqlCommand(
                @"SELECT s.studentid, s.creationdate, sc.surveystatus, sc.completiondate 
          FROM studenttbl s 
          LEFT JOIN (
              SELECT studentid, surveystatus, completiondate,
                     ROW_NUMBER() OVER (PARTITION BY studentid ORDER BY completiondate DESC) AS rn
              FROM surveycompletion
          ) sc ON s.studentid = sc.studentid AND sc.rn = 1 
          WHERE EXTRACT(MONTH FROM s.creationdate) = @month 
          AND EXTRACT(YEAR FROM s.creationdate) = @year;", con))
            {
                //params
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@year", year);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var student = new @Models.Student
                        {
                            student_id = reader.GetString(reader.GetOrdinal("studentid")),
                            date = reader.GetDateTime(reader.GetOrdinal("creationdate")),
                            status = reader.IsDBNull(reader.GetOrdinal("surveystatus")) ? null : reader.GetString(reader.GetOrdinal("surveystatus")),
                            completeDate = reader.IsDBNull(reader.GetOrdinal("completiondate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("completiondate"))
                        };

                        listOfStudents.Add(student);
                    }
                }
            }

            //setting the number of created students that month
            int numCreated = listOfStudents.Count;

            // Step 2: Count all survey completions from surveycompletion table (regardless of status)
            using (var cmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM surveycompletion " +
                "WHERE EXTRACT(MONTH FROM completiondate) = @month " +
                "AND EXTRACT(YEAR FROM completiondate) = @year;", con))
            {
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@year", year);
                //setting the total of checks this month
                numComplete = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Step 3: Count how many of the registered applicants completed their surveys with linq
            numCreatedCompleted = listOfStudents.Count(s => s.status == "Yes");


            // Now regarding the institutes
            // First query to get the maximum count of students for any single institute
            int maxCount = 0;
            using (var cmd = new NpgsqlCommand(
                "SELECT MAX(institute_count) FROM (" +
                "SELECT COUNT(si.instid) AS institute_count " +
                "FROM studentinstitutetbl si " +
                "JOIN studenttbl s ON si.studentid = s.studentid " +
                "WHERE EXTRACT(MONTH FROM s.creationdate) = @month " +
                "AND EXTRACT(YEAR FROM s.creationdate) = @year " +
                "GROUP BY si.instid) AS counts;", con))
            {
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@year", year);
                maxCount = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Second query to retrieve all institutes with the maxCount of students
            using (var cmd = new NpgsqlCommand(
                "SELECT it.name FROM studentinstitutetbl si " +
                "JOIN studenttbl s ON si.studentid = s.studentid " +
                "JOIN INSTITUTETBL it ON si.instid = it.instid " +
                "WHERE EXTRACT(MONTH FROM s.creationdate) = @month " +
                "AND EXTRACT(YEAR FROM s.creationdate) = @year " +
                "GROUP BY it.name " +
                "HAVING COUNT(si.instid) = @maxCount;", con))
            {
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@year", year);
                cmd.Parameters.AddWithValue("@maxCount", maxCount);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        popularInstitutes.Add(reader["name"].ToString());
                    }
                }
            }

            // Step 5: Retrieve a list of all institutes and count the number of students for each (for the pie chart)
            using (var cmd = new NpgsqlCommand(
                "SELECT it.name, COUNT(si.studentid) AS student_count " +
                "FROM studentinstitutetbl si " +
                "JOIN studenttbl s ON si.studentid = s.studentid " +
                "JOIN INSTITUTETBL it ON si.instid = it.instid " +
                "WHERE EXTRACT(MONTH FROM s.creationdate) = @month " +
                "AND EXTRACT(YEAR FROM s.creationdate) = @year " +
                "GROUP BY it.name;", con))
            {
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@year", year);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string instituteName = reader.GetString(reader.GetOrdinal("name"));
                        int count = reader.GetInt32(reader.GetOrdinal("student_count"));
                        instituteCounts[instituteName] = count; // Store institute name and count
                    }
                }
            }

            // Prepare the report object
            Report rep = new Report(listOfStudents, numCreated, numComplete, popularInstitutes, numCreatedCompleted, instituteCounts);

            con.Close();

            return rep;
        }
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------

