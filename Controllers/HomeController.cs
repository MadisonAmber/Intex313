using Intex313.Models;
using Intex313.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
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
                    Tag = "Work Zone Saftey Tips"
                });
            }
            if (data.Pedestrian == 1)
            {
                links.Add(new Resource
                {
                    Link = "https://gizmodo.com/5-ideas-to-make-roads-safer-for-cars-and-pedestrians-1533163537",
                    Tag = "Keeping pedestrians safe when vehicles are present"
                });
            }
            if (data.Motorcycle == 1)
            {
                links.Add(new Resource
                {
                    Link = "http://www.fema-online.eu/website/index.php/2018/01/05/how-can-we-make-our-roads-safer-for-motorcyclists/#:~:text=%20How%20can%20we%20make%20our%20roads%20safer,we%20try%20to%20find%20and%20provide...%20More%20",
                    Tag = "Ways to make roads safer for motocycles"
                });
            }
            if (data.Intersection == 1)
            {
                links.Add(new Resource
                {
                    Link = "https://www.trafficsafetystore.com/blog/engineering-tips-make-city-intersections-safer/",
                    Tag = "Making intersections safer for vehicles and pedestrians"
                });
            }
            if (data.Teenage_Driver == 1)
            {
                links.Add(new Resource
                {
                    Link = "https://youth.gov/youth-topics/ways-promote-safe-driving-youth#:~:text=To%20address%20the%20complexity%20of%20factors%20around%20teen,and%20increasing%20the%20focus%20on%20parental%20responsibility%20%282010%29.",
                    Tag = "How cities and communities can help teenagers drive safer"
                });
            }
            if (data.Older_Driver == 1)
            {
                links.Add(new Resource
                {
                    Link = "https://www.cdc.gov/injury/features/older-driver-safety/index.html",
                    Tag = "Tips for older drivers to keep roads safe"
                });
            }
            if (data.Night_Condition == 1)
            {
                links.Add(new Resource
                {
                    Link = "https://www.trafficsafetystore.com/blog/7-roadway-engineering-design-strategies-to-make-roads-safer-for-drivers/",
                    Tag = "Making roads safer for night-time driving"
                });
            }
            if (data.Roadway_Departure == 1)
            {
                links.Add(new Resource
                {
                    Link = "https://ttap.utk.edu/techtransfer/issues/33/2019summer.pdf",
                    Tag = "Engineering tips to create safer roadway departure zones"
                });
            }

            array = links.ToArray();  //Add links to array
            ViewBag.Links = array;  //Assign the array to a ViewBag

            return View("SeverityPrediction", prediction);
        }
    }
}
