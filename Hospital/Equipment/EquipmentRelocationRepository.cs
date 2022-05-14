using MongoDB.Driver;

namespace Hospital;

public class EquipmentRelocationRepository
{
    private MongoClient _dbClient;
    private EquipmentBatchRepository _equipmentRepo;  // TODO: extract to service!!

    public EquipmentRelocationRepository(MongoClient dbClient, EquipmentBatchRepository equipmentRepo)
    {
        _dbClient = dbClient;
        _equipmentRepo = equipmentRepo;
    }

    private IMongoCollection<EquipmentRelocation> GetMongoCollection()
    {
        return _dbClient.GetDatabase("hospital").GetCollection<EquipmentRelocation>("relocations");
    }

    public IQueryable<EquipmentRelocation> GetAll()
    {
        return GetMongoCollection().AsQueryable();
    }

    public void Add(EquipmentRelocation relocation)
    // todo: load these on start in scheduler
    {
        GetMongoCollection().InsertOne(relocation);
    }

    // NOTE: expects existing!!
    public void Replace(EquipmentRelocation replacing)
    {
        GetMongoCollection().ReplaceOne(relocation => relocation.Id == replacing.Id, replacing);
    }

    public void Schedule(EquipmentRelocation relocation)
    {
        Scheduler.Schedule(relocation.EndTime, () => 
        {
            MoveEquipment(relocation);
        });
    }

    private void MoveEquipment(EquipmentRelocation relocation)
    {
        var removing = new EquipmentBatch(relocation.FromRoomLocation, relocation.Name, relocation.Count, relocation.Type);
        var adding = new EquipmentBatch(relocation.ToRoomLocation, relocation.Name, relocation.Count, relocation.Type);
        _equipmentRepo.Remove(removing);
        _equipmentRepo.Add(adding);
        relocation.IsDone = true;
        Replace(relocation);
    }

    public void MoveAll(string fromLocation, string toLocation)
    {
        foreach (var batch in _equipmentRepo.GetAllIn(fromLocation))
        {
            _equipmentRepo.Remove(batch);
            batch.RoomLocation = toLocation;
            _equipmentRepo.Add(batch);
        }
    }

    // TODO: move this and some others to service
    public void ScheduleAll()
    {
        foreach (var relocation in GetAll())
        {
            if (!relocation.IsDone)
            {
                Schedule(relocation);
            }
        }
    }
}