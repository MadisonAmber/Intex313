﻿using CsvHelper;
using CsvHelper.Configuration;
using Intex313.Models;
using Intex313.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
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

        //Instance of an Inference Session
        private InferenceSession _session { get; set; }
        public HomeController(AccidentDbContext temp, InferenceSession session)
        {
            context = temp;
            _session = session;
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
        public IActionResult AccidentList(Accident filter, int pageNum, string searchInput = "", string searchInputField = "") 
        {
            return getAccidentsByFilter(pageNum, filter, searchInput, searchInputField);
        }

        [HttpPost]
        [Authorize]
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

        [Authorize]
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

        [Authorize]
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
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int accidentid)
        {
            Accident a = context.Accidents
                .Single(a => a.Crash_ID == accidentid);

            return View(a);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(Accident accident)
        {
            context.Update(accident);
            context.SaveChanges();

            return RedirectToAction("AccidentList");
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Add()
        {
            Accident a = new Accident();

            return View("Edit", a);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int accidentid)
        {
            Accident ac = context.Accidents.Single(x => x.Crash_ID == accidentid);
            return View("Confirmation", ac);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(Accident a)
        {
            context.Accidents.Remove(a);
            context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public IActionResult AccidentSummary(int accidentid)
        {
            List<string> accidentTypes = new List<string>();
            List<int> numOfAccidents = new List<int>();
            

            Accident a = context.Accidents.FirstOrDefault(x => x.Crash_ID == accidentid);

            if(a.Work_Zone_Related == true)
            {
                accidentTypes.Add("Work Zone Related");
                numOfAccidents.Add(context.Accidents.Where(x => x.Work_Zone_Related == true).Count());
            }
            if(a.Pedestrian_Involved == true)
            {
                accidentTypes.Add("Pedestrian Involved");
                numOfAccidents.Add(context.Accidents.Where(x => x.Pedestrian_Involved == true).Count());
            }
            if (a.Bicyclist_Involved == true)
            {
                accidentTypes.Add("Bicyclist Involved");
                numOfAccidents.Add(context.Accidents.Where(x => x.Bicyclist_Involved == true).Count());
            }
            if (a.Motorcycle_Involved == true)
            {
                accidentTypes.Add("Motorcycle Involved");
                numOfAccidents.Add(context.Accidents.Where(x => x.Motorcycle_Involved == true).Count());
            }
            if (a.Improper_Restraint == true)
            {
                accidentTypes.Add("Improper Restraint");
                numOfAccidents.Add(context.Accidents.Where(x => x.Improper_Restraint == true).Count());
            }
            if (a.Unrestrained == true)
            {
                accidentTypes.Add("Unrestrained");
                numOfAccidents.Add(context.Accidents.Where(x => x.Unrestrained == true).Count());
            }
            if (a.DUI == true)
            {
                accidentTypes.Add("DUI (Driving Under the Influence)");
                numOfAccidents.Add(context.Accidents.Where(x => x.DUI == true).Count());
            }
            if (a.Intersection_Related == true)
            {
                accidentTypes.Add("Intersection Related");
                numOfAccidents.Add(context.Accidents.Where(x => x.Intersection_Related == true).Count());
            }
            if (a.Wild_Animal_Related == true)
            {
                accidentTypes.Add("Wild Animal Related");
                numOfAccidents.Add(context.Accidents.Where(x => x.Wild_Animal_Related == true).Count());
            }
            if (a.Domestic_Animal_Related == true)
            {
                accidentTypes.Add("Domestic Animal Related");
                numOfAccidents.Add(context.Accidents.Where(x => x.Domestic_Animal_Related == true).Count());
            }
            if (a.Overturn_Rollover == true)
            {
                accidentTypes.Add("Overturn Rollover");
                numOfAccidents.Add(context.Accidents.Where(x => x.Overturn_Rollover == true).Count());
            }
            if (a.Commercial_Motor_Veh_Involved == true)
            {
                accidentTypes.Add("Commercial Motor Vehicle Involved");
                numOfAccidents.Add(context.Accidents.Where(x => x.Commercial_Motor_Veh_Involved == true).Count());
            }
            if (a.Teenage_Driver_Involved == true)
            {
                accidentTypes.Add("Teenage Driver Involved");
                numOfAccidents.Add(context.Accidents.Where(x => x.Teenage_Driver_Involved == true).Count());
            }
            if (a.Older_Driver_Involved == true)
            {
                accidentTypes.Add("Older Driver Involved");
                numOfAccidents.Add(context.Accidents.Where(x => x.Older_Driver_Involved == true).Count());
            }
            if (a.Night_Dark_Condition == true)
            {
                accidentTypes.Add("Nighttime/Dark Outside");
                numOfAccidents.Add(context.Accidents.Where(x => x.Night_Dark_Condition == true).Count());
            }
            if (a.Single_Vehicle == true)
            {
                accidentTypes.Add("Single Vehicle");
                numOfAccidents.Add(context.Accidents.Where(x => x.Single_Vehicle == true).Count());
            }
            if (a.Distracted_Driving == true)
            {
                accidentTypes.Add("Distracted Driving");
                numOfAccidents.Add(context.Accidents.Where(x => x.Distracted_Driving == true).Count());
            }
            if (a.Drowsy_Driving == true)
            {
                accidentTypes.Add("Drowsy Driving");
                numOfAccidents.Add(context.Accidents.Where(x => x.Drowsy_Driving == true).Count());
            }
            if (a.Roadway_Departure == true)
            {
                accidentTypes.Add("Departed from the Roadway");
                numOfAccidents.Add(context.Accidents.Where(x => x.Roadway_Departure == true).Count());
            }

            ViewBag.AccidentCounts = numOfAccidents;

            ViewBag.AccidentTypes = accidentTypes;
            return View(a);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Predictor()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SeverityPrediction(FormData data)
        {
            var result = _session.Run(new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", data.AsTensor())
            });

            Tensor<string> score = result.First().AsTensor<string>();

            var prediction = new Prediction { PredictedValue = score.First() };
            result.Dispose();

            //Determine what resources to display
            Array array;  //Create empty array
            List<Resource> links = new List<Resource>();  //Create a list of url links

            //Add initial link that will always appear
            links.Add(new Resource
            {
                Link = "https://www.mostinside.com/5-ways-to-make-roads-safer-all/#:~:text=%205%20Ways%20to%20Make%20Roads%20Safer%20All,they%20truly%20provide%20you%20with%20a...%20More%20",
                Tag = "General Road Safety Practices"
            });

            //Add records based off of conditions being met
            if (data.Work_Zone == 1)
            {
                links.Add(new Resource 
                {
                    Link = "https://blog.creativesafetysupply.com/getting-zone-keep-road-work-zones-safe-accident-free/", 
                    Tag = "Work Zone Safety Tips"
                });
            }
            if (data.Pedestrian == 1)
            {
                links.Add(new Resource
                {
                    Link = "https://gizmodo.com/5-ideas-to-make-roads-safer-for-cars-and-pedestrians-1533163537",
                    Tag = "Keeping Pedestrians Safe When Vehicles are Present"
                });
            }
            if (data.Motorcycle == 1)
            {
                links.Add(new Resource
                {
                    Link = "http://www.fema-online.eu/website/index.php/2018/01/05/how-can-we-make-our-roads-safer-for-motorcyclists/#:~:text=%20How%20can%20we%20make%20our%20roads%20safer,we%20try%20to%20find%20and%20provide...%20More%20",
                    Tag = "Ways to Make Roads Safer for Motorcycles"
                });
            }
            if (data.Intersection == 1)
            {
                links.Add(new Resource
                {
                    Link = "https://www.trafficsafetystore.com/blog/engineering-tips-make-city-intersections-safer/",
                    Tag = "Making Intersections Safer for Vehicles and Pedestrians"
                });
            }
            if (data.Teenage_Driver == 1)
            {
                links.Add(new Resource
                {
                    Link = "https://youth.gov/youth-topics/ways-promote-safe-driving-youth#:~:text=To%20address%20the%20complexity%20of%20factors%20around%20teen,and%20increasing%20the%20focus%20on%20parental%20responsibility%20%282010%29.",
                    Tag = "How Cities and Communities can Help Teenagers Drive Safer"
                });
            }
            if (data.Older_Driver == 1)
            {
                links.Add(new Resource
                {
                    Link = "https://www.cdc.gov/injury/features/older-driver-safety/index.html",
                    Tag = "Tips for Older Drivers to Keep Roads Safe"
                });
            }
            if (data.Night_Condition == 1)
            {
                links.Add(new Resource
                {
                    Link = "https://www.trafficsafetystore.com/blog/7-roadway-engineering-design-strategies-to-make-roads-safer-for-drivers/",
                    Tag = "Making Roads Safer for Night-Time Driving"
                });
            }
            if (data.Roadway_Departure == 1)
            {
                links.Add(new Resource
                {
                    Link = "https://ttap.utk.edu/techtransfer/issues/33/2019summer.pdf",
                    Tag = "Engineering Tips to Create Safer Roadway Departure Zones"
                });
            }

            array = links.ToArray();  //Add links to array
            ViewBag.Links = array;  //Assign the array to a ViewBag

            return View("SeverityPrediction", prediction);
        }
    }
}
