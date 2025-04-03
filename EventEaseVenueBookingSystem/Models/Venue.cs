using System.ComponentModel.DataAnnotations;

namespace EventEaseVenueBookingSystem.Models
{
    public class Venue
    {
        [Key] public int VenueId { get; set; }

        [Display(Name = "Venue Name")]
        public required string VenueName { get; set; }

        public string? Location { get; set; }

        public int Capacity { get; set; }



    }

}


