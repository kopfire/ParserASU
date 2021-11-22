using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ParserASU.Helpers.JSON;

namespace ParserASU.Models
{
    public class TimeTables
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Speciality { get; set; }

        [Display(Name = "Группа")]
        public string Group { get; set; }

        [Display(Name = "Недели")]
        public List<Week> Weeks { get; set; }

        [Display(Name = "Студенты")]
        public List<long> Students { get; set; }
    }
}
