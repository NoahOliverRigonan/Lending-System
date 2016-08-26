﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace Lending.ApiControllers
{
    public class ApiLoanApplicationLinesController : ApiController
    {
        // data
        private Data.LendingDataContext db = new Data.LendingDataContext();

        // loan Application Lines list
        [Authorize]
        [HttpGet]
        [Route("api/loanApplicationLines/listByLoanId/{loanId}")]
        public List<Models.TrnLoanApplicationLines> listLoanApplicationLinesByLoanId(String loanId)
        {
            var loanApplicationLines = from d in db.trnLoanApplicationLines.OrderByDescending(d => d.Id)
                                       where d.LoanId == Convert.ToInt32(loanId)
                                       select new Models.TrnLoanApplicationLines
                                       {
                                           Id = d.Id,
                                           LoanId = d.LoanId,
                                           Particulars = d.Particulars,
                                           Amount = d.Amount
                                       };

            return loanApplicationLines.ToList();
        }

        // add loan Application Lines
        [Authorize]
        [HttpPost]
        [Route("api/loanApplicationLines/add")]
        public HttpResponseMessage addLoanApplicationLines(Models.TrnLoanApplicationLines loanApplicationLine)
        {
            try
            {
                var loanApplications = from d in db.trnLoanApplications where d.Id == loanApplicationLine.LoanId select d;
                if (loanApplications.Any())
                {
                    if (!loanApplications.FirstOrDefault().IsLocked)
                    {
                        Data.trnLoanApplicationLine newLoanApplicationLine = new Data.trnLoanApplicationLine();
                        newLoanApplicationLine.LoanId = loanApplicationLine.LoanId;
                        newLoanApplicationLine.Particulars = loanApplicationLine.Particulars;
                        newLoanApplicationLine.Amount = loanApplicationLine.Amount;
                        db.trnLoanApplicationLines.InsertOnSubmit(newLoanApplicationLine);
                        db.SubmitChanges();

                        var userId = (from d in db.mstUsers where d.AspUserId == User.Identity.GetUserId() select d.Id).SingleOrDefault();

                        Decimal totalLoanAmount = 0;
                        var loanApplicationLinesForTotalLoanAmount = from d in db.trnLoanApplicationLines where d.LoanId == loanApplicationLine.LoanId select d;
                        if (loanApplicationLinesForTotalLoanAmount.Any())
                        {
                            totalLoanAmount = loanApplicationLinesForTotalLoanAmount.Sum(d => d.Amount);
                        }

                        var updateLoanAmount = loanApplications.FirstOrDefault();
                        updateLoanAmount.LoanAmount = totalLoanAmount;
                        updateLoanAmount.UpdatedByUserId = userId;
                        updateLoanAmount.UpdatedDateTime = DateTime.Now;
                        db.SubmitChanges();

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // update loan Application Lines
        [Authorize]
        [HttpPut]
        [Route("api/loanApplicationLines/update/{id}")]
        public HttpResponseMessage updateLoanApplicationLines(String id, Models.TrnLoanApplicationLines loanApplicationLine)
        {
            try
            {
                var loanApplications = from d in db.trnLoanApplications where d.Id == loanApplicationLine.LoanId select d;
                if (loanApplications.Any())
                {
                    if (!loanApplications.FirstOrDefault().IsLocked)
                    {
                        var loanApplicationLines = from d in db.trnLoanApplicationLines where d.Id == Convert.ToInt32(id) select d;
                        if (loanApplicationLines.Any())
                        {
                            var updateLoanApplicationLine = loanApplicationLines.FirstOrDefault();
                            updateLoanApplicationLine.LoanId = loanApplicationLine.LoanId;
                            updateLoanApplicationLine.Particulars = loanApplicationLine.Particulars;
                            updateLoanApplicationLine.Amount = loanApplicationLine.Amount;
                            db.SubmitChanges();

                            var userId = (from d in db.mstUsers where d.AspUserId == User.Identity.GetUserId() select d.Id).SingleOrDefault();

                            Decimal totalLoanAmount = 0;
                            var loanApplicationLinesForTotalLoanAmount = from d in db.trnLoanApplicationLines where d.LoanId == loanApplicationLine.LoanId select d;
                            if (loanApplicationLinesForTotalLoanAmount.Any())
                            {
                                totalLoanAmount = loanApplicationLinesForTotalLoanAmount.Sum(d => d.Amount);
                            }

                            var updateLoanAmount = loanApplications.FirstOrDefault();
                            updateLoanAmount.LoanAmount = totalLoanAmount;
                            updateLoanAmount.UpdatedByUserId = userId;
                            updateLoanAmount.UpdatedDateTime = DateTime.Now;
                            db.SubmitChanges();

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound);
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // delete loan Application Lines
        [Authorize]
        [HttpDelete]
        [Route("api/loanApplicationLines/delete/{id}/{loanId}")]
        public HttpResponseMessage deleteLoanApplicationLines(String id, String loanId)
        {
            try
            {
                var loanApplications = from d in db.trnLoanApplications where d.Id == Convert.ToInt32(loanId) select d;
                if (loanApplications.Any())
                {
                    if (!loanApplications.FirstOrDefault().IsLocked)
                    {
                        var loanApplicationLines = from d in db.trnLoanApplicationLines where d.Id == Convert.ToInt32(id) select d;
                        if (loanApplicationLines.Any())
                        {
                            db.trnLoanApplicationLines.DeleteOnSubmit(loanApplicationLines.First());
                            db.SubmitChanges();

                            Decimal totalLoanAmount = 0;
                            var loanApplicationLinesForTotalLoanAmount = from d in db.trnLoanApplicationLines where d.LoanId == Convert.ToInt32(loanId) select d;
                            if (loanApplicationLinesForTotalLoanAmount.Any())
                            {
                                totalLoanAmount = loanApplicationLinesForTotalLoanAmount.Sum(d => d.Amount);
                            }

                            var userId = (from d in db.mstUsers where d.AspUserId == User.Identity.GetUserId() select d.Id).SingleOrDefault();

                            var updateLoanAmount = loanApplications.FirstOrDefault();
                            updateLoanAmount.LoanAmount = totalLoanAmount;
                            updateLoanAmount.UpdatedByUserId = userId;
                            updateLoanAmount.UpdatedDateTime = DateTime.Now;
                            db.SubmitChanges();

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound);
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}
