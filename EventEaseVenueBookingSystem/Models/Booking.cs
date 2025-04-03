using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventEaseVenueBookingSystem.Models
{
    public class Booking
    {

        [Key] public int BookingId { get; set; }


        public int EventId { get; set; }
        [ForeignKey("EventId")]

        [ValidateNever]
        public Event Event { get; set; }



        public int VenueId { get; set; }
        [ForeignKey("VenueId")]
        [ValidateNever]
        public Venue Venue { get; set; }



        [DataType(DataType.Date)]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; }

    }
}

