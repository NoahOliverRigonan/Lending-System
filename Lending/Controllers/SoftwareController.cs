﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lending.Controllers
{
    public class SoftwareController : UserController
    {
        // GET: Software
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Forbidden()
        {
            return View();
        }

        [Authorize]
        public ActionResult NotFound()
        {
            return View();
        }

        [Authorize]
        public ActionResult ApplicantList()
        {
            return View();
        }

        [Authorize]
        public ActionResult ApplicantDetail(Int32? id)
        {
            if(id == null) {
                return RedirectToAction("NotFound", "Software");
            }
            else
            {
                return View();
            }
        }
    }
}