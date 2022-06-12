using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace HospitalSystem.Core.Surveys;

[System.Serializable]
public class InvalidSurveyException : System.Exception
{
    public InvalidSurveyException() { }
    public InvalidSurveyException(string message) : base(message) { }
    public InvalidSurveyException(string message, System.Exception inner) : base(message, inner) { }
    protected InvalidSurveyException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

[BsonKnownTypes(typeof(DoctorSurvey), typeof(HospitalSurvey))]
public abstract class Survey
{
    [BsonId]
    public ObjectId Id { get; }
    public string Title { get; set; }
    public List<string> Questions { get; set; }
    public List<string> RatingQuestions { get; set; }

    public Survey(List<string> questions, List<string> ratingQuestions, string title)
    {
        Id = ObjectId.GenerateNewId();
        Questions = questions;
        RatingQuestions = ratingQuestions;
        Title = title;
    }

    public abstract void AddAnswer(SurveyAnswer answer);

    public abstract bool WasAnsweredBy(Person person);
}