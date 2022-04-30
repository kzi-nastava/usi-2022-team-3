using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Hospital
{
    public class Room
    {
        // todo: naming
        public enum RoomType
        {
            REST,
            OPERATION,
            EXAMINATION,
            OTHER,
            STOCK
        }

        [BsonId]
        public ObjectId Id {get; set;}

        [BsonRepresentation(BsonType.String)]
        public RoomType Type {get; set;}
        public string Location {get; set;}
        public string Name {get; set;}

        public Room(string location, string name, RoomType type)
        {
            Id = ObjectId.GenerateNewId();
            Location = location;
            Name = name;
            Type = type;
        }
    }
}