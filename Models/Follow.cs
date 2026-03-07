using System.ComponentModel.DataAnnotations.Schema;

namespace MUMbackend.Models
{
    public class Follow
    {
        [Column("FollowerId", TypeName = "bigint")]
        public int FollowerId { get; set; }
        [Column("FollowingId", TypeName = "bigint")]
        public int FollowingId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User Follower { get; set; }
        public User Following { get; set; }

    }

}
