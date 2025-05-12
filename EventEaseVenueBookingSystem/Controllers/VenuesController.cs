using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEaseVenueBookingSystem.Data;
using EventEaseVenueBookingSystem.Models;
using System.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEaseVenueBookingSystem.Controllers
{
    public class VenuesController : Controller
    {
        private readonly EventEaseVenueBookingSystemContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "catoos";

        public VenuesController(EventEaseVenueBookingSystemContext context, IConfiguration config)
        {
            _context = context;
            _blobServiceClient = new BlobServiceClient(config["BlobStorage:ConnectionString"]);
            _containerName = config["BlobStorage:ContainerName"];
        }

        // GET: Venues
        public async Task<IActionResult> Index(string searchString)
        {

            if (_context.Venue == null)
            {
                return Problem("Entity set 'EventEaseTestAppContext.'  is null.");
            }

            var venues = from m in _context.Venue
                         select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                venues = venues.Where(s => s.VenueName!.ToUpper().Contains(searchString.ToUpper()));
            }


            return View(await venues.ToListAsync());
        }


        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {

            return View();
        }

        // POST: Venues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("imageUrl,VenueName,Location,Capacity")] IFormFile image, Venue venue)
        {

            if (image == null && image.Length > 0) return BadRequest("Image file is required.");

            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blob = container.GetBlobClient(image.FileName);

            using var stream = image.OpenReadStream();
            await blob.UploadAsync(stream, overwrite: true);


            venue.imageUrl = blob.Uri.ToString();
            _context.Add(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }







        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, [Bind("imageUrl,VenueId,VenueName,Location,Capacity")] Venue venue, IFormFile image)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (image != null && image.Length > 0)
                    {
                        var container = _blobServiceClient.GetBlobContainerClient(_containerName);

                        // Delete the old image blob if it exists
                        if (!string.IsNullOrEmpty(venue.imageUrl))
                        {
                            var blobUri = new Uri(venue.imageUrl);
                            var blobName = WebUtility.UrlDecode(blobUri.Segments.Last());
                            var blobToDelete = container.GetBlobClient(blobName);
                            await blobToDelete.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                        }

                        // Upload the new image blob
                        var blob = container.GetBlobClient(image.FileName);
                        using var stream = image.OpenReadStream();
                        await blob.UploadAsync(stream, overwrite: true);

                        venue.imageUrl = blob.Uri.ToString();
                    }


                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(venue);
        }
        //if (image == null && image.Length > 0) return BadRequest("Image file is required.");

        //var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        //var blob = container.GetBlobClient(image.FileName);

        //// Delete the old image blob if it exists
        //var blobUri = new Uri(venue.imageUrl);
        //var blobName = WebUtility.UrlDecode(blobUri.Segments.Last());
        //var blobToDelete = container.GetBlobClient(blobName);
        //await blobToDelete.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);


        //// Upload the new image blob
        // using var stream = image.OpenReadStream();
        //await blob.UploadAsync(stream, overwrite: true);


        //venue.imageUrl = blob.Uri.ToString();
        //_context.Update(venue);
        //await _context.SaveChangesAsync();
        //return RedirectToAction(nameof(Index));




        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            var container = _blobServiceClient.GetBlobContainerClient(_containerName);

            var blobUri = new Uri(venue.imageUrl);
            var blobName = WebUtility.UrlDecode(blobUri.Segments.Last());
            var blob = container.GetBlobClient(blobName);

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool isBookingExists = await _context.Booking.AnyAsync(b => b.VenueId == id);


            // Check if there are any bookings associated with the venue
            if (isBookingExists)
            {
                var @venue = await _context.Venue.FindAsync(id);
                ModelState.AddModelError("", "Cannot delete venue as it has associated bookings.");
                return View(@venue);
            }


            var venueToDelete = await _context.Venue.FindAsync(id);

            var container = _blobServiceClient.GetBlobContainerClient(_containerName);

            var blobUri = new Uri(venueToDelete.imageUrl);
            var blobName = WebUtility.UrlDecode(blobUri.Segments.Last());
            var blob = container.GetBlobClient(blobName);

            _context.Venue.Remove(venueToDelete);
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueId == id);
        }
    }
}
