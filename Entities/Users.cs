using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Entities
{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("FirstName")]
        public string FirstName { get; set; }

        [BsonElement("LastName")]
        public string LastName { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("PasswordHash")]
        public byte[] PasswordHash { get; set; }

        [BsonElement("PasswordSalt")]
        public byte[] PasswordSalt { get; set; }

        [BsonElement("VerificationToken")]
        public string VerificationToken { get; set; }

        [BsonElement("VerifiedAt")]
        public DateTime? VerifiedAt { get; set; }

        [BsonElement("PasswordResetToken")]
        public string PasswordResetToken { get; set; }

        [BsonElement("PasswordResetTokenExpires")]
        public DateTime? PasswordResetTokenExpires { get; set; }

        [BsonElement("LastChangePassword")]
        public DateTime? LastChangePassword { get; set; }
        [BsonElement("Status")]
        public string Status { get; set; }
    }
}
