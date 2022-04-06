using Intex313.Models;
using Intex313.Models.ViewModels;
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
            
            return View(accidents);
        
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TermsConditions()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccidentList(int pageNum = 1)
        {
            int pageSize = 10;
            int numPaginationButtons = 10;

            // This will need to be slightly updated to allow for filters
            IEnumerable<Accident> accidents = context.Accidents
                .OrderBy(x => x.Crash_Date_Time)
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize);

            PageInfo pi = new PageInfo
            {
                TotalNumItems = context.Accidents.Count(),
                ItemsPerPage = pageSize,
                CurrentPage = pageNum,
                NumButtons = numPaginationButtons   
            };

            ViewBag.PageInfo = pi;

            return View(accidents);
        }
            

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var a = context.Accidents
                .Single(a => a.Crash_ID == id);

            return View(a);
        }

        [HttpPost]
        public IActionResult Edit()
        {

            if (ModelState.IsValid)
            {
                context.SaveChanges();
                return RedirectToAction("AccidentList");
            }
            else
            {
                return View("Edit", a);
            }
        }

        [HttpGet]
        public IActionResult Add()
        {
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

        [HttpGet]
        public IActionResult AccidentSummary(int accidentid)
        {
            Accident a = context.Accidents.FirstOrDefault(x => x.Crash_ID == accidentid);

            return View(a);
        }

        [HttpGet]
        public IActionResult Predictor()
        {
            return View();
        }

    }
}
