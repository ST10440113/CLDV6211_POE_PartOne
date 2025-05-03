using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEaseVenueBookingSystem.Data;
using EventEaseVenueBookingSystem.Models;

namespace EventEaseVenueBookingSystem.Controllers
{
    public class BookingsController : Controller
    {
        private readonly EventEaseVenueBookingSystemContext _context;

        public BookingsController(EventEaseVenueBookingSystemContext context)
        {
            _context = context;
        }

        public void PopulateEventList()
        {
            IEnumerable<SelectListItem> eventList = _context.Event.Select(e => new SelectListItem
            {
                Text = e.EventName,
                Value = e.EventId.ToString(),

            });
            ViewBag.EventList = eventList;

        }


        public void PopulateVenueList()
        {
            IEnumerable<SelectListItem> venueList = _context.Venue.Select(e => new SelectListItem
            {
                Text = e.VenueName,
                Value = e.VenueId.ToString(),

            });
            ViewBag.VenueList = venueList;

        }

        // GET: Bookings
        public async Task<IActionResult> Index(string searchString)
        {
            var bookingList = _context.Booking.ToList();
            foreach (var booking in bookingList)
            {
                booking.Event = _context.Event.FirstOrDefault(e => e.EventId == booking.EventId);
                booking.Venue = _context.Venue.FirstOrDefault(v => v.VenueId == booking.VenueId);
            }
            if (_context.Booking == null)
            {
                return Problem("Entity set 'EventEaseTestAppContext.'  is null.");
            }

            var bookings = from m in _context.Booking
                           select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(s => s.Event.EventName!.ToUpper().Contains(searchString.ToUpper()));
            }


            return View(await bookings.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            PopulateEventList();
            PopulateVenueList();
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            var @event = await _context.Event.FindAsync(booking.EventId);
            var @venue = await _context.Venue.FindAsync(booking.VenueId);


            bool checkForDuplicate = await _context.Booking.AnyAsync(i => i.Event.EventDate.Equals(@event.EventDate) &&
            i.Venue.VenueName.Equals(@venue.VenueName));


            if (checkForDuplicate == true)
            {
                ModelState.AddModelError(nameof(Event.EventId), "There is an existing booking with the same venue and date of event.");

                PopulateEventList();
                PopulateVenueList();

            }
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventId", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueId", booking.VenueId);



            return View(booking);
        }
        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            PopulateEventList();
            PopulateVenueList();
            var booking = await _context.Booking.FindAsync(id);

            
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventId", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueId", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingId == id);
        }
    }
}
