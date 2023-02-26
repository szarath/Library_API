using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Library_API.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime DateReserved { get; set; }
        public DateTime? DateBorrowed { get; set; }
        public virtual Book? Book { get; set; }
        public virtual Member? Member { get; set; }


    }
}
