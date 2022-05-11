using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Hospital;

public class EquipmentRelocation
{
    [BsonId]
    public ObjectId Id { get; set; }
    public MongoDBRef FromRoom { get; set; }
    public MongoDBRef ToRoom { get; set; }
    public string Name { get; set; }
    public EquipmentType Type { get; set; }
    public int Count { get; set; }
    public DateTime WhenDone { get; set; }
    public bool IsDone { get; set; }

    public EquipmentRelocation(string name, int count, EquipmentType type, DateTime whenDone, ObjectId fromRoomId, ObjectId toRoomId)
    {
        Id = ObjectId.GenerateNewId();
        FromRoom = new MongoDBRef("rooms", fromRoomId);
        ToRoom = new MongoDBRef("rooms", toRoomId);
        Name = name;
        Count = count;
        WhenDone = whenDone;
        Type = type;
        IsDone = false;
    }
}