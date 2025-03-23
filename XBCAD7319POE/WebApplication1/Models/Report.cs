using Npgsql.PostgresTypes;
using WebApplication1.Models;

namespace WebApplication1.Models;
/// <summary>
/// This class contains the report object
/// Author: Sven Masche
/// </summary>
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------

public class Report
{
    private List<@Models.Student> students;
    private int numCreated;
    private int numComplete;
    private List<string> popularInstitutes;
    private int numCreatedCompleted;
    private Dictionary<string, int> instituteCounts;

    public Report(List<Student> students, int numCreated, int numComplete, List<string> popularInstitutes, int numCreatedCompleted, Dictionary<string, int> instituteCounts)
    {
        Students = students;
        NumCreated = numCreated;
        NumComplete = numComplete;
        PopularInstitutes = popularInstitutes;
        NumCreatedCompleted = numCreatedCompleted;
        InstituteCounts = instituteCounts;
    }
    public List<Student> Students { get => students; set => students = value; }
    public int NumCreated { get => numCreated; set => numCreated = value; }
    public int NumComplete { get => numComplete; set => numComplete = value; }
    public List<string> PopularInstitutes { get => popularInstitutes; set => popularInstitutes = value; }
    public int NumCreatedCompleted { get => numCreatedCompleted; set => numCreatedCompleted = value; }
    public Dictionary<string, int> InstituteCounts { get => instituteCounts; set => instituteCounts = value; }

}
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------

