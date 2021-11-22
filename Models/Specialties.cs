﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ParserASU.Models
{
    public class Specialties
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Facylty { get; set; }

        [Display(Name = "Специальность")]
        public string Name { get; set; }
    }
}
