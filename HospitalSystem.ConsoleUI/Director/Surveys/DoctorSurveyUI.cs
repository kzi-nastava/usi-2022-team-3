using HospitalSystem.Core;
using HospitalSystem.Core.Surveys;

namespace HospitalSystem.ConsoleUI.Director.Surveys;

public class DoctorSurveyUI : SurveyUI
{
    private List<DoctorSurvey> _loadedSurveys;

    public DoctorSurveyUI(Hospital hospital) : base(hospital)
    {
        _loadedSurveys = new();
    }

    private void RefreshSurveys()
    {
        _loadedSurveys = _hospital.SurveyService.GetAllDoctor().ToList();
    }

    public override void Start()
    {
        while (true)
        {
            System.Console.Clear();
            System.Console.WriteLine("--- AVAILABLE SURVEYS ---");
            RefreshSurveys();
            DisplaySurveys(_loadedSurveys.Cast<Survey>().ToList());
            System.Console.WriteLine(@"
            INPUT OPTION:
                [view|v] View survey
                [quit|q] Quit to main menu
                [exit|x] Exit program
            ");
            System.Console.Write(">> ");
            var choice = ReadSanitizedLine();
            try
            {
                if (choice == "v" || choice == "view")
                {
                    DisplaySurvey();
                }
                else if (choice == "q" || choice == "quit")
                {
                    throw new QuitToMainMenuException("From StartManageMedicationRequests.");
                }
                else if (choice == "x" || choice == "exit")
                {
                    System.Environment.Exit(0);
                }
                else
                {
                    System.Console.WriteLine("Invalid input - please read the available commands.");
                }
            }
            catch (InvalidInputException e)
            {
                System.Console.WriteLine(e.Message);
            }
            System.Console.Write("Input anything to continue >> ");
            ReadSanitizedLine();
        }
    }

    private void DisplaySurvey()
    {
        System.Console.Write("Input survey number >> ");
        var survey = _loadedSurveys[ReadInt(0, _loadedSurveys.Count-1)];
        System.Console.Clear();
        var allDrResponses = _hospital.SurveyService.GetDoctorsWithResponsesFor(survey);
        DisplayDoctors(allDrResponses);
        System.Console.Write("Input doctor number >> ");
        var drResponses = allDrResponses[ReadInt(0, allDrResponses.Count - 1)];
        System.Console.Clear();
        System.Console.WriteLine("Showing survey: " + survey.Title);
        System.Console.WriteLine("For doctor: " + drResponses.Item1.ToString());
        DisplayAggregatedRatings(survey.AggregateRatingsFor(drResponses.Item1));
        DisplayResponses(survey.Questions, drResponses.Item2);
    }

    private void DisplayDoctors(IList<(Doctor, List<SurveyResponse>)> drResponses)
    {
        System.Console.WriteLine("No. | Doctor");
        for (int i = 0; i < drResponses.Count; i++)
        {
            System.Console.WriteLine(i + " | " + drResponses[i].Item1.ToString());
        }
    }
}