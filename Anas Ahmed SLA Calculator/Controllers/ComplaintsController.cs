using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data_Access;

namespace Anas_Ahmed_SLA_Calculator.Controllers
{
    public class ComplaintsController : Controller
    {
        private SLA_DBEntities db = new SLA_DBEntities();
        // GET: Complaints
        public ActionResult Index()
        {
            return View(db.Complaints.ToList());
        }

        public ActionResult Create() //Have only taken priority from user. Complaint will be regitered with current time
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ComplaintType,ComplaintTime,ComplaintId")] Complaint complaint)
        {
            if (ModelState.IsValid)
            {
                complaint.ComplaintTime = DateTime.Now;
                complaint.ComplaintDeadline = Deadline(complaint); //Will calculate the SLAExpiration for given complaint pirority and time
                db.Complaints.Add(complaint);
                try
                {
                    db.SaveChanges();
                }
                catch(Exception e)
                {

                }
                return RedirectToAction("Index");
            }

            return View(complaint);
        }

        public DateTime Deadline(Complaint complaint)
        {
            DateTime SLAExpiration = complaint.ComplaintTime; //this variablr will hold expiration date and will be returened later
            if(complaint.ComplaintTime.DayOfWeek.ToString() == "Saturday") //if complaint is registered on saturday
            {
                int hour = complaint.ComplaintTime.Hour;
                int min1 = complaint.ComplaintTime.Minute;
                int sec1 = complaint.ComplaintTime.Second;
                int msec1 = complaint.ComplaintTime.Millisecond;
                complaint.ComplaintTime.AddHours(-hour);
                complaint.ComplaintTime.AddMilliseconds(msec1);
                complaint.ComplaintTime.AddMinutes(min1);
                complaint.ComplaintTime.AddSeconds(sec1);
                //above code is to make sure thar all working hours are utilized after the holidays


                complaint.ComplaintTime.AddDays(2);
            }
            if(complaint.ComplaintTime.DayOfWeek.ToString() == "Sunday") //if complaint is registered on sunday
            {
                int hour = complaint.ComplaintTime.Hour;
                int min1 = complaint.ComplaintTime.Minute;
                int sec1 = complaint.ComplaintTime.Second;
                int msec1 = complaint.ComplaintTime.Millisecond;
                complaint.ComplaintTime.AddHours(-hour);
                complaint.ComplaintTime.AddMilliseconds(msec1);
                complaint.ComplaintTime.AddMinutes(min1);
                complaint.ComplaintTime.AddSeconds(sec1);
                complaint.ComplaintTime.AddDays(1);
            }
            int hours; //this variable will hold the number of hours depending to priority
            if (complaint.ComplaintType == "High")
            {
                hours = 4;
                if(complaint.ComplaintTime.DayOfWeek.ToString()=="Friday"&&complaint.ComplaintTime.Hour>=20) //this code is to make sure that if request is made late on friday and has to be finished on next woring day
                {
                    complaint.ComplaintDeadline = complaint.ComplaintDeadline.AddDays(2);
                }
            }
            else if (complaint.ComplaintType == "Medium")
            {
                hours = 10;
                if (complaint.ComplaintTime.DayOfWeek.ToString() == "Friday" && complaint.ComplaintTime.Hour >= 14)
                {
                    complaint.ComplaintDeadline = complaint.ComplaintDeadline.AddDays(2);
                }
            }
            else
            {
                hours = 24;
                if (complaint.ComplaintTime.DayOfWeek.ToString() == "Friday")
                {
                    complaint.ComplaintDeadline = complaint.ComplaintDeadline.AddDays(2);
                }
            }
            var data = db.Cloasures.Where(t => t.CloasureHour >= SLAExpiration).ToList(); //cloasure data in database
            var dates = data.Select(t => t.CloasureHour).ToList();
            var min = SLAExpiration.Minute;
            var sec = SLAExpiration.Second;
            var msec = SLAExpiration.Millisecond;
            double RemainingMin = 59 - min;
            double RemainingSec = 60 - sec;
            SLAExpiration = SLAExpiration.AddMinutes(RemainingMin);
            SLAExpiration = SLAExpiration.AddSeconds(RemainingSec);
            SLAExpiration = SLAExpiration.AddMilliseconds(-msec);
            //above code is to make sure to start the checking from the next hour of complaint registraton


            double value = 1;
            for (int i = 1; i <= hours; i++) //to find enough hours for complaint
            {
                for (int j = 0; j < dates.Count; j++) //to check every cloasure from complaint time
                {
                    if (DateTime.Equals(SLAExpiration.Hour, dates[j].Hour) && DateTime.Equals(SLAExpiration.Date, dates[j].Date))
                    {
                        i--; //if it is close hour the another hour is included in Expiration
                        break;
                    }
                }
                SLAExpiration = SLAExpiration.AddHours(value);
            }
            SLAExpiration = SLAExpiration.AddMinutes(-RemainingMin);
            SLAExpiration = SLAExpiration.AddSeconds(-RemainingSec);
            SLAExpiration = SLAExpiration.AddMilliseconds(msec);
            //the above code is to restore the mins and sec that were added earlier


            return SLAExpiration;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}