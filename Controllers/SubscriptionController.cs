using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proiect_medical.Data;
using Proiect_medical.Models;

namespace Proiect_medical.Controllers
{
    [Authorize(Roles = "Patient")]
    public class SubscriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Subscriptions
        public async Task<IActionResult> Index()
        {
            var subscriptions = await _context.Subscriptions.ToListAsync();
            return View(subscriptions);
        }

        // GET: Subscriptions/Subscribe/5
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Subscribe(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return Forbid();
            }

            
            patient.SubscriptionId = subscription.Id;
            _context.Update(patient);
            await _context.SaveChangesAsync();

           
            TempData["SuccessMessage"] = "Te-ai abonat cu succes la planul " + subscription.Name + "!";

            return RedirectToAction(nameof(Index));
        }

    }
}
