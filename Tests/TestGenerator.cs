using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Hospital;
// TODO: rename namespace Hospital to something else...

public static class TestGenerator
{
    // THIS WILL DELETE YOUR EXISTING DATABASE!!
    public static void Generate(Hospital.Hospital hospital)
    {
        // TODO: split to functions
        var dbClient = new MongoClient("mongodb://root:root@localhost:27017");  // TODO: unhardcode
        dbClient.DropDatabase("hospital");
        System.Console.WriteLine("DROPPED EXISTING DATABASE HOSPITAL");

        GenerateUsers(hospital);
        GenerateCheckupsAndOperations(hospital);
        GenerateRoomsAndEquipments(hospital);
        GenerateCheckupChangeRequests(hospital);

        System.Console.WriteLine("GENERATED TESTS IN DB");

        WriteDatabaseToFile(dbClient);

        System.Console.WriteLine("WROTE TESTS TO FILE");
    }

    private static void GenerateCheckupChangeRequests(Hospital.Hospital hospital)
    {
        for (int i = 0; i < 20; i++)
        {
            Doctor doctor = hospital.DoctorRepo.GetDoctorByFullName("name1","surname1");
            List<Checkup> checkups = hospital.AppointmentRepo.GetCheckupsByDoctor(doctor.Id);

            if (i % 2 == 0)
            {   RequestState state = RequestState.PENDING;
                if (i % 4 == 0)
                {
                    state = RequestState.APPROVED;
                }
                Checkup alteredCheckup = checkups[i];
                DateTime newDateAndTime =  new DateTime (2077,10,10);
                alteredCheckup.StartTime = newDateAndTime;
                CheckupChangeRequest request = new CheckupChangeRequest(alteredCheckup,CRUDOperation.UPDATE,state);
                hospital.CheckupChangeRequestRepo.AddOrUpdate(request);
            } else if (i % 2 == 1) 
            {
                RequestState state = RequestState.PENDING;
                if (i % 3 == 0)
                {
                    state = RequestState.DENIED;
                }
                CheckupChangeRequest request = new CheckupChangeRequest(checkups[i],CRUDOperation.DELETE,state);
                hospital.CheckupChangeRequestRepo.AddOrUpdate(request);
            }    
        }
    }

    private static void GenerateRoomsAndEquipments(Hospital.Hospital hospital)
    {
        for (int i = 0; i < 10; i++)
        {
            if (i % 3 == 0)
            {   
                var newRoom = new Room("90" + i, "NA" + i, RoomType.STOCK);
                hospital.RoomRepo.Add(newRoom);
                for (int j = 0; j < 4; j++)
                {
                    var newEquipmentBatch = new EquipmentBatch(newRoom.Location, "scalpel", 3, EquipmentType.OPERATION);
                    hospital.EquipmentRepo.Add(newEquipmentBatch);
                }
            } 
            else if (i % 3 == 1)
            {
                var newRoom = new Room("10" + i, "NA" + i, RoomType.OPERATION);
                hospital.RoomRepo.Add(newRoom);
            } 
            else
            {
                var newRoom = new Room("55" + i, "NA" + i, RoomType.CHECKUP);
                hospital.RoomRepo.Add(newRoom);
            }
        }
    }

    private static void GenerateCheckupsAndOperations(Hospital.Hospital hospital)
    {
        DateTime dateTime = new DateTime(2022, 5, 11, 4, 15, 0);
        for (int i = 0; i < 100; i++)
        {
            Doctor doctor = hospital.DoctorRepo.GetDoctorByFullName("name1","surname1");
            Patient patient = hospital.PatientRepo.GetPatientByFullName("name2","surname2");
            dateTime = dateTime.AddHours(1);

            if (i % 2 == 0)
            {   
                Checkup check = new Checkup(dateTime, new MongoDBRef("patients",patient.Id), new MongoDBRef("doctors", doctor.Id), "anamneza");
                hospital.AppointmentRepo.AddOrUpdateCheckup(check);
            } else if (i % 2 == 1) 
            {
                Operation op = new Operation(dateTime, new MongoDBRef("patients",patient.Id), new MongoDBRef("doctors", doctor.Id), "report", new TimeSpan(1,15,0));
                hospital.AppointmentRepo.AddOrUpdateOperation(op);
            }    
        }
    }

    private static void GenerateUsers(Hospital.Hospital hospital)
    {
        int doctorSpecialtynumber = 0;
        for (int i = 0; i < 100; i++)
        {
            User user;
            if (i % 4 == 0)
            {
                Director director = new Director("name" + i, "surname" + i);
                user = new User("a" + i, "a" + i, director, Role.DIRECTOR);
                hospital.DirectorRepo.AddOrUpdateDirector(director);
                hospital.UserRepo.AddOrUpdateUser(user);
            }
            else if (i % 4 == 1)
            {
                Doctor doctor;
                int namesCount = Enum.GetNames(typeof(Specialty)).Length;
                Specialty doctorsSpecialty = (Specialty)(doctorSpecialtynumber%namesCount);
                doctorSpecialtynumber++; 
                doctor = new Doctor("name" + i,"surname" + i, doctorsSpecialty);
                user = new User("a" + i, "a" + i, doctor, Role.DOCTOR);
                hospital.DoctorRepo.AddOrUpdateDoctor(doctor);
                hospital.UserRepo.AddOrUpdateUser(user);
            }
            else if (i % 4 == 2) 
            {
                Patient patient;
                patient = new Patient("name" + i, "surname" + i, new MedicalRecord());
                hospital.PatientRepo.AddOrUpdatePatient(patient);
                user = new User("a" + i, "a" + i, patient, Role.PATIENT);
                hospital.UserRepo.AddOrUpdateUser(user);                
            }  
            else
            {
                Secretary secretary = new Secretary("name" + i, "surname" + i);
                user = new User("a" + i, "a" + i, secretary, Role.SECRETARY);
                hospital.SecretaryRepo.AddOrUpdateSecretary(secretary);
                hospital.UserRepo.AddOrUpdateUser(user);
            }
        }
    }

    private static void WriteDatabaseToFile(MongoClient dbClient)
    {
        Dictionary<String, List<Object>> allCollections = new();
        foreach (string collectionName in dbClient.GetDatabase("hospital").ListCollectionNames().ToEnumerable())
        {
            var collection = dbClient.GetDatabase("hospital").GetCollection<BsonDocument>(collectionName).AsQueryable();
            allCollections[collectionName] = new();
            allCollections[collectionName].AddRange(collection);
        }
        File.WriteAllText("db/hospital.json", 
            allCollections.ToJson(
                new JsonWriterSettings {Indent = true}
            )
        );
    }
}