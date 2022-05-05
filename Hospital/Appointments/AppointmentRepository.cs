using MongoDB.Driver;
using MongoDB.Bson;

namespace Hospital
{
    public class AppointmentRepository
    {
        private MongoClient _dbClient;
        public AppointmentRepository(MongoClient _dbClient)
        {
            this._dbClient = _dbClient;
        }

        public IMongoCollection<Checkup> GetCheckups()
        {
            return _dbClient.GetDatabase("hospital").GetCollection<Checkup>("checkups");
        }

        public IMongoCollection<Operation> GetOperations()
        {
            return _dbClient.GetDatabase("hospital").GetCollection<Operation>("operations");
        }

        public void AddOrUpdateCheckup(DateTime timeAndDate, MongoDBRef patient, MongoDBRef doctor, TimeSpan duration, string anamnesis)
        {
            var newCheckup = new Checkup(timeAndDate, patient, doctor, anamnesis);
            var checkups = GetCheckups();
            checkups.ReplaceOne(checkup => checkup.Id == newCheckup.Id, newCheckup, new ReplaceOptions {IsUpsert = true});
        }

        public void AddOrUpdateCheckup(Checkup newCheckup)
        {
            var checkups = GetCheckups();
            checkups.ReplaceOne(checkup => checkup.Id == newCheckup.Id, newCheckup, new ReplaceOptions {IsUpsert = true});
        }

        public void AddOrUpdateOperation(DateTime timeAndDate, MongoDBRef patient, MongoDBRef doctor, TimeSpan duration, string report)
        {
            var newOperation = new Operation(timeAndDate, patient, doctor, report);
            var operations = GetOperations();
            operations.ReplaceOne(operation => operation.Id == newOperation.Id, newOperation, new ReplaceOptions {IsUpsert = true});
        }

        public void AddOrUpdateOperation(Operation newOperation)
        {
            var operations = GetOperations();
            operations.ReplaceOne(operation => operation.Id == newOperation.Id, newOperation, new ReplaceOptions {IsUpsert = true});
        }

        public List<Checkup> GetCheckupsByDoctor(ObjectId id)
        {
            var checkups = GetCheckups();
            List<Checkup> doctorsCheckups = checkups.Find(appointment => appointment.Doctor.Id == id).ToList();
            return doctorsCheckups;
        }

        public List<Checkup> GetCheckupsByPatient(ObjectId id)
        {
            var checkups = GetCheckups();
            List<Checkup> patientCheckups = checkups.Find(appointment => appointment.Patient.Id == id).ToList();
            return patientCheckups;
        }

        public List<Operation> GetOperationsByPatient(ObjectId id)
        {
            var operations = GetOperations();
            List<Operation> patientOperations = operations.Find(appointment => appointment.Doctor.Id == id).ToList();
            return patientOperations;
        }
        
        public List<Operation> GetOperationsByDoctor(ObjectId id)
        {
            var operations = GetOperations();
            List<Operation> doctorsOperations = operations.Find(appointment => appointment.Doctor.Id == id).ToList();
            return doctorsOperations;
        }

        public List<Checkup> GetCheckupsByDay(DateTime date)
        {
            var checkups = GetCheckups();
            List<Checkup> checkupsByDay = checkups.Find(appointment => appointment.TimeAndDate > date && appointment.TimeAndDate < date.AddDays(1)).ToList();
            return checkupsByDay;
        }


        public Checkup GetCheckupById(ObjectId id)
        {
            var checkups = GetCheckups();
            Checkup checkup = checkups.Find(appointment => appointment.Doctor.Id == id).FirstOrDefault();
            return checkup;
        }

        public void DeleteCheckup(Checkup checkup)
        {
         var checkups = GetCheckups();
         var filter = Builders<Checkup>.Filter.Eq(deletedCheckup => deletedCheckup.Id, checkup.Id);
         checkups.DeleteOne(filter);
        }

        public bool IsDoctorBusy(DateTime date, Doctor doctor)
        {
            List<Checkup> checkups = GetCheckupsByDoctor(doctor.Id);
            foreach (Checkup checkup in checkups)
            {
                //Console.WriteLine(checkup.TimeAndDate.ToString(), checkup.TimeAndDate.Add(checkup.Duration).ToString());
                if (checkup.TimeAndDate <= date && checkup.TimeAndDate.Add(checkup.Duration) > date)
                {
                    return true;
                } 
            }
            return false;
        }
    }
}