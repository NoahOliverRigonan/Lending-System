﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Lending.ApiControllers
{
    public class ApiResidenceTypeController : ApiController
    {
        // data
        private Data.LendingDataContext db = new Data.LendingDataContext();

        // residence type list
        [Authorize]
        [HttpGet]
        [Route("api/residenceType/list")]
        public List<Models.SysResidenceType> listApplicantResidenceType()
        {
            var residenceTypes = from d in db.sysResidenceTypes
                                 select new Models.SysResidenceType
                                 {
                                     Id = d.Id,
                                     ResidenceType = d.ResidenceType
                                 };

            return residenceTypes.ToList();
        }
    }
}
