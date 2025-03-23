namespace WebApplication1.Models
{
/// <summary>
/// This class containst the results object
/// Author Sven Masche
/// </summary>
    public class Result
    {
        private string studentId;
        private string completed;

        public Result(string studentId, string completed)
        {
            this.studentId = studentId;
            this.completed = completed;
        }

        //secondary constructor to allow declaration of empty results
        public Result(){}

        public string StudentId { get => studentId; set => studentId = value; }
        public string Completed { get => completed; set => completed = value; }
    }
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

}
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------
