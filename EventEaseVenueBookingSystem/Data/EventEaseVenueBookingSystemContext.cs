using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EventEaseVenueBookingSystem.Models;

namespace EventEaseVenueBookingSystem.Data
{
    public class EventEaseVenueBookingSystemContext : DbContext
    {
        public EventEaseVenueBookingSystemContext (DbContextOptions<EventEaseVenueBookingSystemContext> options)
            : base(options)
        {
        }

        public DbSet<EventEaseVenueBookingSystem.Models.Event> Event { get; set; } = default!;
        public DbSet<EventEaseVenueBookingSystem.Models.Venue> Venue { get; set; } = default!;
        public DbSet<EventEaseVenueBookingSystem.Models.Booking> Booking { get; set; } = default!;
    }
}
