using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proiect_medical.Data;
using Proiect_medical.Models;

namespace Proiect_medical.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Messages
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Messages.Include(m => m.ReceiverDoctor).Include(m => m.ReceiverPatient).Include(m => m.SenderDoctor).Include(m => m.SenderPatient);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Messages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.ReceiverDoctor)
                .Include(m => m.ReceiverPatient)
                .Include(m => m.SenderDoctor)
                .Include(m => m.SenderPatient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // GET: Messages/Create
        public IActionResult Create()
        {
            ViewData["ReceiverDoctorId"] = new SelectList(_context.Doctors, "Id", "Id");
            ViewData["ReceiverPatientId"] = new SelectList(_context.Patients, "Id", "Id");
            ViewData["SenderDoctorId"] = new SelectList(_context.Doctors, "Id", "Id");
            ViewData["SenderPatientId"] = new SelectList(_context.Patients, "Id", "Id");
            return View();
        }

        // POST: Messages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content,SentAt,SenderDoctorId,SenderPatientId,ReceiverDoctorId,ReceiverPatientId")] Message message)
        {
            if (ModelState.IsValid)
            {
                _context.Add(message);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReceiverDoctorId"] = new SelectList(_context.Doctors, "Id", "Id", message.ReceiverDoctorId);
            ViewData["ReceiverPatientId"] = new SelectList(_context.Patients, "Id", "Id", message.ReceiverPatientId);
            ViewData["SenderDoctorId"] = new SelectList(_context.Doctors, "Id", "Id", message.SenderDoctorId);
            ViewData["SenderPatientId"] = new SelectList(_context.Patients, "Id", "Id", message.SenderPatientId);
            return View(message);
        }

        // GET: Messages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            ViewData["ReceiverDoctorId"] = new SelectList(_context.Doctors, "Id", "Id", message.ReceiverDoctorId);
            ViewData["ReceiverPatientId"] = new SelectList(_context.Patients, "Id", "Id", message.ReceiverPatientId);
            ViewData["SenderDoctorId"] = new SelectList(_context.Doctors, "Id", "Id", message.SenderDoctorId);
            ViewData["SenderPatientId"] = new SelectList(_context.Patients, "Id", "Id", message.SenderPatientId);
            return View(message);
        }

        // POST: Messages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,SentAt,SenderDoctorId,SenderPatientId,ReceiverDoctorId,ReceiverPatientId")] Message message)
        {
            if (id != message.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(message);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MessageExists(message.Id))
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
            ViewData["ReceiverDoctorId"] = new SelectList(_context.Doctors, "Id", "Id", message.ReceiverDoctorId);
            ViewData["ReceiverPatientId"] = new SelectList(_context.Patients, "Id", "Id", message.ReceiverPatientId);
            ViewData["SenderDoctorId"] = new SelectList(_context.Doctors, "Id", "Id", message.SenderDoctorId);
            ViewData["SenderPatientId"] = new SelectList(_context.Patients, "Id", "Id", message.SenderPatientId);
            return View(message);
        }

        // GET: Messages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.ReceiverDoctor)
                .Include(m => m.ReceiverPatient)
                .Include(m => m.SenderDoctor)
                .Include(m => m.SenderPatient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }
    }
}
