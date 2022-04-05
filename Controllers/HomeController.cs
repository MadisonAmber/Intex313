using Intex313.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Intex313.Controllers
{
    public class HomeController : Controller
    {
        private AccidentDbContext context{ get; set; }
        public HomeController(AccidentDbContext temp)
        {
            context = temp;
        }
        public IActionResult Index(int accidentid)
        {
            List<Accident> accidents = new List<Accident>();
            /*if (accidentid == 0)
            {
                ViewBag.Heading = "All Accidents";
                accidents = context.Accidents
                    .Include(a => a.Accident)
                    .ToList();
            }
            else
            {
                accidents = context.Accidents
                    .Where(a => a.Crash_ID == accidentid)
                    .Include(a => a.Accident)
                    .ToList();
                if (accidents.Count > 0)
                {
                    ViewBag.Heading = "Accident" + accidents[0].Crash_Date_Time;
                }
                else
                {
                    ViewBag.Heading = "No Accidents Found On This Date.";
                }
            }*/

            /*ViewBag.SelectedAccident = accidentid;

            List<Accident> accidents = context.Accidents.ToList();
            ViewBag.Accidents = accidents;*/
            return View(accidents);
        
        }
            

        [HttpGet]
        public IActionResult Edit(int accidentid)
        {
            Accident a = context.Accidents
                .Single(a => a.Crash_ID == accidentid);

            List<Accident> accidents = context.Accidents.ToList();
            ViewBag.Teams = accidents;

            return View(a);
        }

        [HttpGet]
        public IActionResult Add()
        {
            List<Accident> accidents = context.Accidents.ToList();
            ViewBag.Teams = accidents;

            Accident a = new Accident();

            return View("Edit", a);
        }

        [HttpPost]
        public IActionResult Add(Accident a)
        {
            if (ModelState.IsValid)
            {
                if (a.Crash_ID == 0)
                {
                    Accident lastAccident = context.Accidents.ToList().LastOrDefault();
                    a.Crash_ID = lastAccident.Crash_ID + 1;
                    context.Add(a);
                }
                else
                {
                    context.Update(a);
                }

                context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                List<Accident> accidents = context.Accidents.ToList();
                ViewBag.Accidents = accidents;
                return View("Edit", a);
            }
        }

        [HttpPost]
        public IActionResult Delete(Accident a)
        {
            context.Accidents.Remove(a);
            context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
