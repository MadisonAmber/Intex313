using CsvHelper;
using CsvHelper.Configuration;
using Intex313.Models;
using Intex313.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        public IActionResult DownloadCsv(string filter)
        {
            Accident f = new Accident();
            string fileName = "accident_data_with";
            if (filter != "")
            {
                fileName = fileName + "_filter";
                try
                {
                    f = JsonConvert.DeserializeObject<Accident>(filter);
                }
                catch (Exception e)
                {

                }

            } else
            {
                fileName = fileName + "out_filter";
            }

            string filterString = BuildQueryFilter(f);
            List<Accident> accidents = context.Accidents
                .FromSqlRaw(filterString)
                .OrderBy(x => x.Crash_Date_Time)
                .ToList();

            var cc = new CsvConfiguration(new System.Globalization.CultureInfo("en-US"));
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(stream: ms, encoding: new UTF8Encoding(true)))
                {
                    using (var cw = new CsvWriter(sw, cc))
                    {
                        cw.WriteRecords(accidents);
                    }// The stream gets flushed here.
                    return File(ms.ToArray(), "text/csv", $"{fileName}_{DateTime.UtcNow.Ticks}.csv");
                }
            }

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
        public IActionResult AccidentList(Accident filter, int pageNum, string searchInput = "", string searchInputField = "") 
        {
            return getAccidentsByFilter(pageNum, filter, searchInput, searchInputField);
        }

        [HttpPost]
        public IActionResult Search(IFormCollection collection)
        {
            string filter = collection["Filter"];
            Accident f = new Accident();
            if (filter != "")
            {
                try
                {
                    f = JsonConvert.DeserializeObject<Accident>(filter);
                }
                catch (Exception e)
                {

                }

            }

            string searchInput = collection["InputValue"];
            string searchInputField = collection["InputValueField"];

            ViewBag.SearchInput = searchInput;
            ViewBag.SearchInputField = searchInputField;

            return getAccidentsByFilter(1, f, searchInput, searchInputField);
        }

        private IActionResult getAccidentsByFilter(int pageNum, Accident filter, string searchInput = "", string searchInputField = "")
        {
            int pageSize = 10;
            int numPaginationButtons = 10;

            string queryFilter = BuildQueryFilter(filter, searchInput, searchInputField);

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

        private string BuildQueryFilter(Accident filter, string InputValue = "", string InputValueField = "")
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

                            filterString = $"{filterString}\"{fieldName}\" = true";

                            numFilterParams = numFilterParams + 1;
                        }
                        break;
                    case "String":
                        string propFromModel = Convert.ToString(property?.GetValue(filter));
                        if(propFromModel != null & propFromModel != "" & (InputValueField == null || InputValueField == ""))
                        {
                            InputValueField = property.Name;
                            InputValue = propFromModel;
                        }
                        if(InputValueField == property.Name)
                        {
                            if (numFilterParams > 0)
                            {
                                filterString = filterString + " AND ";
                            }

                            filterString = $"{filterString}LOWER(\"{property.Name}\") LIKE LOWER('%{InputValue}%')";
                            numFilterParams = numFilterParams + 1;

                            ViewBag.SearchInput = InputValue;
                            ViewBag.SearchInputField = property.Name;
                            
                            filter.City = property.Name == "City" ? InputValue : "";
                            filter.Main_Road_Name = property.Name == "Main_Road_Name" ? InputValue : "";
                            filter.County_Name = property.Name == "County_Name" ? InputValue : "";
                            filter.Route = property.Name == "Route" ? InputValue : "";
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
        [HttpPost]
        public IActionResult Edit(Accident accident)
        {
            context.Update(accident);
            context.SaveChanges();

            return RedirectToAction("AccidentList");
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
        [HttpGet]
        public IActionResult Delete(int accidentid)
        {
            Accident ac = context.Accidents.Single(x => x.Crash_ID == accidentid);
            return View("Confirmation", ac);
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
            List<String> accidentTypes = new List<String>();

            Accident a = context.Accidents.FirstOrDefault(x => x.Crash_ID == accidentid);

            if(a.Work_Zone_Related == true)
            {
                accidentTypes.Add("Work Zone Related");
            }
            if(a.Pedestrian_Involved == true)
            {
                accidentTypes.Add("Pedestrian Involved");
            }
            if (a.Bicyclist_Involved == true)
            {
                accidentTypes.Add("Bicyclist Involved");
            }
            if (a.Motorcycle_Involved == true)
            {
                accidentTypes.Add("Motorcycle Involved");
            }
            if (a.Improper_Restraint == true)
            {
                accidentTypes.Add("Improper Restraint");
            }
            if (a.Unrestrained == true)
            {
                accidentTypes.Add("Unrestrained");
            }
            if (a.DUI == true)
            {
                accidentTypes.Add("DUI (Driving Under the Influence)");
            }
            if (a.Intersection_Related == true)
            {
                accidentTypes.Add("Intersection Related");
            }
            if (a.Wild_Animal_Related == true)
            {
                accidentTypes.Add("Wild Animal Related");
            }
            if (a.Domestic_Animal_Related == true)
            {
                accidentTypes.Add("Domestic Animal Related");
            }
            if (a.Overturn_Rollover == true)
            {
                accidentTypes.Add("Overturn Rollover");
            }
            if (a.Commercial_Motor_Veh_Involved == true)
            {
                accidentTypes.Add("Commercial Motor Vehicle Involved");
            }
            if (a.Teenage_Driver_Involved == true)
            {
                accidentTypes.Add("Teenage Driver Involved");
            }
            if (a.Older_Driver_Involved == true)
            {
                accidentTypes.Add("Older Driver Involved");
            }
            if (a.Night_Dark_Condition == true)
            {
                accidentTypes.Add("Nighttime/Dark Outside");
            }
            if (a.Single_Vehicle == true)
            {
                accidentTypes.Add("Single Vehicle");
            }
            if (a.Distracted_Driving == true)
            {
                accidentTypes.Add("Distracted Driving");
            }
            if (a.Drowsy_Driving == true)
            {
                accidentTypes.Add("Drowsy Driving");
            }
            if (a.Roadway_Departure == true)
            {
                accidentTypes.Add("Departed from the Roadway");
            }

            ViewBag.AccidentTypes = accidentTypes;
            return View(a);
        }

        [HttpGet]
        public IActionResult Predictor()
        {
            return View();
        }

    }
}
