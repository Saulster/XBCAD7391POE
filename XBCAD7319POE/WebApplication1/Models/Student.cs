namespace WebApplication1.Models
{
/// <summary>
/// This class holds the student object
/// Author: Sven Masche
/// </summary>
    public class Student
    {
        public string student_id { get; set; }
        public string status { get; set; }
        public DateTime date { get; set; }

        public DateTime? completeDate { get; set; }

        public Student() { }

        public Student(string sID, string sStatus, DateTime sDate, DateTime? CompleteDate)
        {
            student_id = sID;
            status = sStatus;
            date = sDate;
            completeDate = CompleteDate;
        }
    
    }
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------
