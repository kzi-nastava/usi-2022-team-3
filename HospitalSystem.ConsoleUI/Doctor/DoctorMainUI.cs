using MongoDB.Bson;
using HospitalSystem.Core.Utils;
using HospitalSystem.Core;

namespace HospitalSystem.ConsoleUI;

public class DoctorMainUI : UserUI
{
    Doctor Doctor;
    public DoctorMainUI(Hospital hospital, User user) : base(hospital, user) 
    {
        Doctor = _hospital.DoctorService.GetById((ObjectId)_user.Person.Id);
    }

     public override void Start()
    {
        bool quit = false;
        while (!quit)
        {
            Console.WriteLine("\nChoose an option below:\n\n1. View appointments for a specific day\n2. View timetable\n3. Create checkup\n4. Manage medication requests\n5. Request days off\n6. Quit");
            Console.Write("\n>>");
            var option = ReadSanitizedLine().Trim();
            switch (option)
            {
                case "1":
                {
                    ShowCheckupsByDay();
                    break;
                }
                case "2":
                {
                    new DoctorCheckupsUI(_hospital, _user).Start();
                    break;
                }
                case "3":
                {
                    CreateCheckup();
                    break;
                }
                case "4":
                {
                    new MedicationRequestsUI(_hospital, _user).Start();
                    break;
                }
                case "5":
                {
                    RequestDaysOff();
                    break;
                }
                case "6":
                {
                    quit = true;
                    break;
                }
            }
        }
    }

    public void CreateCheckup()
    {
        Console.WriteLine("Creating new Checkup appointment...");
        Console.Write("\nEnter date >>");
        string? date = Console.ReadLine();
        Console.Write("\nEnter time >>");
        string? time = Console.ReadLine();
        DateTime dateTime = DateTime.Parse(date + " " + time);
        Console.Write("\nEnter patient name >>");
        string? name = Console.ReadLine();
        Console.Write("\nEnter patient surname >>");
        string? surname = Console.ReadLine();
        if (_hospital.AppointmentService.UpsertCheckup(_user, dateTime, name, surname) == true)
        {
            Console.WriteLine("\nCheckup successfully added");
        }
        else
        {
            Console.WriteLine("Doctor is not available at that time");
        }
    }

    public void ShowCheckupsByDay()
    {
        Console.Write("\nEnter date (dd.mm.yyyy) >> ");
        var date = Console.ReadLine();
        List<Checkup> checkups = _hospital.AppointmentService.GetCheckupsByDay(Convert.ToDateTime(date));
        PrintCheckups(checkups);
    }

    public void PrintCheckups(List<Checkup> checkups)
    {
        Console.WriteLine(String.Format("{0,5} {1,24} {2,25}", "Nr.", "Date & Time", "Patient"));
        int i = 1;
        foreach (Checkup checkup in checkups)
        {
            Patient patient = _hospital.PatientService.GetPatientById((ObjectId)checkup.Patient.Id);
            Console.WriteLine(string.Concat(Enumerable.Repeat("-", 60)));
            Console.WriteLine(String.Format("{0,5} {1,24} {2,25}", i, checkup.DateRange, patient));
            i++;
        }
    }

    public void RequestDaysOff()
    {
        Console.Write("\nEnter desired range for off days\nStarting date >>");
        string? start = Console.ReadLine();
        Console.Write("\nEnding date >>");
        string? end = Console.ReadLine();
        var startDate = DateTime.TryParse(start, out DateTime newStartDate);
        var endDate = DateTime.TryParse(end, out DateTime newEndDate);
        if (startDate == true && endDate== true && newStartDate > DateTime.Now)
        {
            try
            {
            DateRange daysOff = new DateRange(newStartDate, newEndDate);
            Console.Write("\nEnter reason for request >> ");
            string reason = ReadSanitizedLine();
            if (RequestIsUrgent())
            {            
                if (daysOff.EachDay().Count() <= 5)
                {
                    _hospital.DaysOffRequestService.Approve(new DaysOffRequest(Doctor, reason, daysOff));
                    Console.WriteLine("Request succesfully sent and approved.");
                }
                else
                    Console.WriteLine("Urgent requests may only be less than 5 days.");
            }
            else
            {
                if (_hospital.DaysOffRequestService.DaysOffAllowed(daysOff))
                {
                    _hospital.DaysOffRequestService.Send(new DaysOffRequest(Doctor, reason, daysOff));
                    Console.WriteLine("Request succesfully sent.");
                }
                else
                    Console.WriteLine("You cannot have these days off, you have scheduled appointments");
            }
            }
            catch (ArgumentException) 
            {
                Console.WriteLine("End date cannot be before starting date.");
            };
        }
        else
            Console.WriteLine("Invalid date, cannot be before today.");
    }

    public bool RequestIsUrgent()
    {
        while (true)
        {
            Console.Write("\nIs this request urgent? [y/n]");
            string urgent = ReadSanitizedLine().ToLower();
            switch (urgent)
            {
                case "y":
                {
                    return true;
                }
                case "n":
                {
                    return false;
                }
                default:
                {
                    Console.WriteLine("Please enter either y[yes] or n[no].");
                    break;
                }
            }
        } 
    }
}