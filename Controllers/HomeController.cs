using Intex313.Models;
using Intex313.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        [HttpGet]
        public IActionResult AccidentList(int pageNum = 1, string filter = "")
        {

            Accident f = new Accident();
            if(filter != "")
            {
                try
                {
                    f = JsonConvert.DeserializeObject<Accident>(filter);
                } catch(Exception e)
                {

                }
                
            }
            return getAccidentsByFilter(pageNum, f);
        }

        [HttpPost]
        public IActionResult AccidentList(Accident filter, int pageNum)
        {
            return getAccidentsByFilter(pageNum, filter);
        }

        private IActionResult getAccidentsByFilter(int pageNum, Accident filter)
        {
            int pageSize = 10;
            int numPaginationButtons = 10;

            string queryFilter = BuildQueryFilter(filter);

            // This will need to be slightly updated to allow for filters
            IEnumerable<Accident> accidents = context.Accidents
                .FromSqlRaw(queryFilter)
                .OrderBy(x => x.Crash_Date_Time)
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize);

            PageInfo pi = new PageInfo
            {
                TotalNumItems = context.Accidents.FromSqlRaw(queryFilter).Count(),
                ItemsPerPage = pageSize,
                CurrentPage = pageNum,
                NumButtons = numPaginationButtons
            };

            ViewBag.PageInfo = pi;
            ViewBag.Filter = filter;

            return View("AccidentList", accidents);
        }

        private string BuildQueryFilter(Accident filter)
        {
            string filterString = "SELECT * FROM public.\"Accidents\" WHERE ";
            int numFilterParams = 0;

            foreach(var property in filter.GetType().GetProperties())
            {
                switch(property.PropertyType.Name.ToString())
                {
                    case "Boolean":
                        bool boolValue = false;
                        try
                        {
                            boolValue = Convert.ToBoolean(property.GetValue(filter));
                            
                        } catch (Exception e)
                        {
                            boolValue = false;
                        }
                        
                        if(boolValue == true)
                        {
                            string fieldName = property.Name;
                            if(numFilterParams > 0)
                            {
                                filterString = filterString + " AND ";
                            } 

                            filterString = filterString + " \"" + fieldName + "\" = true";

                            numFilterParams = numFilterParams + 1;
                        }
                        break;
                    default:
                        break;
                }
            }

            if(numFilterParams == 0)
            {
                filterString = "SELECT * FROM public.\"Accidents\"";
            }

            return filterString;
        }
            

        [HttpGet]
        public IActionResult Edit(int accidentid)
        {
            Accident a = context.Accidents
                .Single(a => a.Crash_ID == accidentid);

            return View(a);
        }

        [HttpGet]
        public IActionResult Add()
        {
            List<Accident> accidents = context.Accidents.ToList();

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
