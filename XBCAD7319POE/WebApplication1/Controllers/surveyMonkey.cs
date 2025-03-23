using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
/// <summary>
/// This class sustains all survey monkey api related operations.
/// Author: Sven Masche
/// </summary>
    public class surveyMonkey
    {
        //global variable for api ID
        string surveyId;

        /// <summary>
        /// General constructor that retrieves the API ID from the environemnt variables
        /// Author: Sven Masche
        /// </summary>
        public surveyMonkey()
        {
            surveyId = Environment.GetEnvironmentVariable("SURVEY_ID");

            if (string.IsNullOrEmpty(surveyId))
            {
                throw new InvalidOperationException("Survey ID environment variable is not set.");
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Method that retrieves all student survey responses and checks for response from input student ID.
        /// Author: Sven Masche
        /// </summary>
        public async Task<bool> checkStudent(string inputId)
        {
            //call method to get the complete list of student responses
            List<JsonElement> responses = await GetSurveyResponsesAsync(surveyId);
            if (responses == null) return false; // Handle null responses

            foreach (var response in responses)
            {
                // Check if "custom_variables" exists and contains the "uuid" property
                if (response.TryGetProperty("custom_variables", out JsonElement customVariables) &&
                    customVariables.TryGetProperty("uuid", out JsonElement uuidElement))
                {
                    // Retrieve the uuid value
                    string uuid = uuidElement.GetString();
                    if (uuid.Equals(inputId))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        ///  Method returns the date a given student has completed thier survey
        ///  Author: Sven Masche
        /// </summary>
        public async Task<DateTime> GetCompletionDate(string inputId)
        {
            List<JsonElement> responses = await GetSurveyResponsesAsync(surveyId);
            if (responses == null) return DateTime.MinValue; // Return a default date if null

            foreach (var response in responses)
            {
                // Check if "custom_variables" exists and contains the "uuid" property
                if (response.TryGetProperty("custom_variables", out JsonElement customVariables) &&
                    customVariables.TryGetProperty("uuid", out JsonElement uuidElement))
                {
                    // Retrieve the uuid value
                    string uuid = uuidElement.GetString();
                    if (uuid.Equals(inputId))
                    {
                        // Check for completion date in completedAtElement
                        if (response.TryGetProperty("date_created", out JsonElement completedAtElement))
                        {
                            // Parse the completion date and return
                            if (completedAtElement.TryGetDateTime(out DateTime completionDate))
                            {
                                return completionDate;
                            }
                        }
                        break; // Exit after finding the matching UUID
                    }
                }
            }
            return DateTime.MinValue; // Return a default date if not found
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Method that retrieves a complete list of all student survey responses.
        /// Author: Sven Masche
        /// </summary>
        private static async Task<List<JsonElement>> GetSurveyResponsesAsync(string surveyId)
        {
            using (HttpClient client = new HttpClient())
            {
                string accessToken = Environment.GetEnvironmentVariable("SURVEY_ACCESS"); ; 
                string baseUrl = "https://api.surveymonkey.com/v3/";
                client.BaseAddress = new Uri(baseUrl);

                // Use the access token for authorization
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                List<JsonElement> responsesList = new List<JsonElement>();
                int page = 1;
                int pageSize = 100; //page size for pagination
                bool hasMore = true;

                while (hasMore)
                {
                    //endpoint for getting responses for the specific survey with pagination
                    string endpoint = $"surveys/{surveyId}/responses/bulk?page={page}&page_size={pageSize}";

                    //Make the get request
                    HttpResponseMessage response = await client.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        //parse the JSON response content into a JsonDocument
                        var responseContent = await response.Content.ReadAsStreamAsync();
                        using (JsonDocument document = await JsonDocument.ParseAsync(responseContent))
                        {
                            //navigatng to the data property where all the info is at
                            if (document.RootElement.TryGetProperty("data", out JsonElement data))
                            {
                                //clone each json object in the data array and add it to the list
                                foreach (JsonElement responseItem in data.EnumerateArray())
                                {
                                    responsesList.Add(responseItem.Clone());
                                }

                                //check if there are more pages
                                if (document.RootElement.TryGetProperty("total", out JsonElement totalElement) &&
                                    document.RootElement.TryGetProperty("per_page", out JsonElement perPageElement))
                                {
                                    int total = totalElement.GetInt32();
                                    int perPage = perPageElement.GetInt32();
                                    hasMore = (page * perPage) < total;
                                }
                                else
                                {
                                    hasMore = false;//no more pages
                                }
                            }
                            else
                            {
                                hasMore = false; //no data property
                            }
                        }
                    }
                    else
                    {
                        // Log the error and return null
                        Console.WriteLine($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                        return null;
                    }

                    page++; // Move to the next page
                }

                return responsesList;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------
