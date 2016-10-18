﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Lending.ApiControllers
{
    public class ApiLoanLogHistoryController : ApiController
    {
        // data
        private Data.LendingDataContext db = new Data.LendingDataContext();

        // loan log history list
        [Authorize]
        [HttpGet]
        [Route("api/loanLogHistory/listByApplicantIdAndByLoanId/{applicantId}/{loanId}")]
        public List<Models.TrnLoanLogHistory> listLoanLogHistoryByApplicantIdAndByLoanId(String applicantId, String loanId)
        {
            var loanLogHistories = from d in db.trnLoanLogHistories
                                   where d.trnLoanApplication.ApplicantId == Convert.ToInt32(applicantId)
                                   && d.LoanId == Convert.ToInt32(loanId)
                                   select new Models.TrnLoanLogHistory
                                   {
                                       Id = d.Id,
                                       LoanId = d.LoanId,
                                       DayNumber = d.DayNumber,
                                       CollectionDate = d.CollectionDate.ToShortDateString(),
                                       NetAmount = d.NetAmount,
                                       CollectibleAmount = d.CollectibleAmount,
                                       PenaltyAmount = d.PenaltyAmount,
                                       PaidAmount = d.PaidAmount,
                                       PreviousBalanceAmount = d.PreviousBalanceAmount,
                                       CurrentBalanceAmount = d.CurrentBalanceAmount,
                                       IsCleared = d.IsCleared,
                                       IsPenalty = d.IsPenalty,
                                       IsOverdue = d.IsOverdue,
                                       IsFullyPaid = d.IsFullyPaid,
                                       IsAction = d.IsAction
                                   };

            return loanLogHistories.ToList();
        }

        // loan log history by collectible date and by area
        [Authorize]
        [HttpGet]
        [Route("api/loanLogHistory/listByCollectionDateAndByAreaId/{collectionDate}/{areaId}")]
        public List<Models.TrnLoanLogHistory> listLoanLogHistoryByCollectionDateAndByAreaId(String collectionDate, String areaId)
        {
            var loanLogHistories = from d in db.trnLoanLogHistories
                                   where d.CollectionDate == Convert.ToDateTime(collectionDate)
                                   && d.trnLoanApplication.mstApplicant.AreaId == Convert.ToInt32(areaId)
                                   select new Models.TrnLoanLogHistory
                                   {
                                       Id = d.Id,
                                       LoanId = d.LoanId,
                                       LoanNumber = d.trnLoanApplication.LoanNumber,
                                       Applicant = d.trnLoanApplication.mstApplicant.ApplicantLastName + ", " + d.trnLoanApplication.mstApplicant.ApplicantFirstName + " " + d.trnLoanApplication.mstApplicant.ApplicantMiddleName,
                                       Area = d.trnLoanApplication.mstApplicant.mstArea.Area,
                                       DayNumber = d.DayNumber,
                                       CollectionDate = d.CollectionDate.ToShortDateString(),
                                       NetAmount = d.NetAmount,
                                       CollectibleAmount = d.CollectibleAmount,
                                       PenaltyAmount = d.PenaltyAmount,
                                       PaidAmount = d.PaidAmount,
                                       PreviousBalanceAmount = d.PreviousBalanceAmount,
                                       CurrentBalanceAmount = d.CurrentBalanceAmount,
                                       IsCleared = d.IsCleared,
                                       IsPenalty = d.IsPenalty,
                                       IsOverdue = d.IsOverdue,
                                       IsFullyPaid = d.IsFullyPaid,
                                       IsAction = d.IsAction
                                   };

            return loanLogHistories.ToList();
        }

        // clear loan log history
        [Authorize]
        [HttpPut]
        [Route("api/loanLogHistory/isCleared/updateByIdAndByLoanId/{id}/{loanId}")]
        public HttpResponseMessage updateIsClearedLoanLogHistory(String id, String loanId)
        {
            try
            {
                var loanApplications = from d in db.trnLoanApplications where d.Id == Convert.ToInt32(loanId) select d;
                if (loanApplications.Any())
                {
                    if (loanApplications.FirstOrDefault().IsLocked)
                    {
                        var loanLogHistories = from d in db.trnLoanLogHistories where d.Id == Convert.ToInt32(id) select d;
                        if (loanLogHistories.Any())
                        {
                            if (loanLogHistories.FirstOrDefault().CurrentBalanceAmount > 0)
                            {
                                if (!loanLogHistories.FirstOrDefault().IsCleared)
                                {
                                    if (loanLogHistories.FirstOrDefault().IsAction)
                                    {
                                        if (loanLogHistories.FirstOrDefault().CollectionDate <= DateTime.Today)
                                        {
                                            Data.trnCollectionLogHistory newCollectionLogHistory = new Data.trnCollectionLogHistory();
                                            newCollectionLogHistory.LoanId = Convert.ToInt32(loanId);
                                            newCollectionLogHistory.CollectionDate = loanLogHistories.FirstOrDefault().CollectionDate;
                                            newCollectionLogHistory.CollectibleAmount = loanLogHistories.FirstOrDefault().CollectibleAmount;
                                            newCollectionLogHistory.PaidAmount = loanLogHistories.FirstOrDefault().CurrentBalanceAmount;
                                            newCollectionLogHistory.IsCleared = true;
                                            newCollectionLogHistory.IsPenalty = loanLogHistories.FirstOrDefault().IsPenalty;
                                            newCollectionLogHistory.IsOverdue = loanLogHistories.FirstOrDefault().IsOverdue;
                                            newCollectionLogHistory.IsFullyPaid = loanLogHistories.FirstOrDefault().IsFullyPaid;
                                            newCollectionLogHistory.CollectorId = loanApplications.FirstOrDefault().CollectorId;
                                            newCollectionLogHistory.AccountId = (from d in db.mstAccounts where d.AccountTransactionTypeId == 2 select d.Id).FirstOrDefault();
                                            db.trnCollectionLogHistories.InsertOnSubmit(newCollectionLogHistory);
                                            db.SubmitChanges();

                                            var updateLoanLogHistory = loanLogHistories.FirstOrDefault();
                                            updateLoanLogHistory.PaidAmount = loanLogHistories.FirstOrDefault().CollectibleAmount + loanLogHistories.FirstOrDefault().PreviousBalanceAmount;
                                            updateLoanLogHistory.CurrentBalanceAmount = 0;
                                            updateLoanLogHistory.IsPenalty = false;
                                            updateLoanLogHistory.IsCleared = true;
                                            db.SubmitChanges();

                                            var loanLogHistoryByCollectionDatePrevious = from d in db.trnLoanLogHistories where d.LoanId == Convert.ToInt32(loanId) && d.CollectionDate == loanLogHistories.FirstOrDefault().CollectionDate.Date.AddDays(-1) select d;
                                            if (loanLogHistoryByCollectionDatePrevious.Any())
                                            {
                                                var updateLoanLogHistoryByCollectionDatePrevious = loanLogHistoryByCollectionDatePrevious.FirstOrDefault();
                                                updateLoanLogHistoryByCollectionDatePrevious.IsAction = false;
                                                db.SubmitChanges();

                                                var loanLogHistoryByCollectionDate = from d in db.trnLoanLogHistories where d.LoanId == Convert.ToInt32(loanId) && d.CollectionDate == loanLogHistories.FirstOrDefault().CollectionDate.Date.AddDays(1) select d;
                                                if (loanLogHistoryByCollectionDate.Any())
                                                {
                                                    var updateLoanLogHistoryByCollectionDate = loanLogHistoryByCollectionDate.FirstOrDefault();
                                                    updateLoanLogHistoryByCollectionDate.PreviousBalanceAmount = 0;
                                                    updateLoanLogHistoryByCollectionDate.CurrentBalanceAmount = loanLogHistoryByCollectionDate.FirstOrDefault().CollectibleAmount;
                                                    updateLoanLogHistoryByCollectionDate.IsAction = true;
                                                    db.SubmitChanges();
                                                }
                                            }
                                            else
                                            {
                                                var loanLogHistoryByCollectionDate = from d in db.trnLoanLogHistories where d.LoanId == Convert.ToInt32(loanId) && d.CollectionDate == loanLogHistories.FirstOrDefault().CollectionDate.Date.AddDays(1) select d;
                                                if (loanLogHistoryByCollectionDate.Any())
                                                {
                                                    var updateLoanLogHistoryByCollectionDate = loanLogHistoryByCollectionDate.FirstOrDefault();
                                                    updateLoanLogHistoryByCollectionDate.PreviousBalanceAmount = 0;
                                                    updateLoanLogHistoryByCollectionDate.CurrentBalanceAmount = loanLogHistoryByCollectionDate.FirstOrDefault().CollectibleAmount;
                                                    updateLoanLogHistoryByCollectionDate.IsAction = true;
                                                    db.SubmitChanges();
                                                }
                                            }

                                            return Request.CreateResponse(HttpStatusCode.OK);
                                        }
                                        else
                                        {
                                            return Request.CreateResponse(HttpStatusCode.BadRequest);
                                        }
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest);
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

        // absent loan log history
        [Authorize]
        [HttpPut]
        [Route("api/loanLogHistory/isPenalty/updateByIdAndByLoanId/{id}/{loanId}")]
        public HttpResponseMessage updateIsPenaltyLoanLogHistory(String id, String loanId)
        {
            try
            {
                var loanApplications = from d in db.trnLoanApplications where d.Id == Convert.ToInt32(loanId) select d;
                if (loanApplications.Any())
                {
                    if (loanApplications.FirstOrDefault().IsLocked)
                    {
                        var loanLogHistories = from d in db.trnLoanLogHistories where d.Id == Convert.ToInt32(id) select d;
                        if (loanLogHistories.Any())
                        {
                            if (!loanLogHistories.FirstOrDefault().IsPenalty)
                            {
                                if (loanLogHistories.FirstOrDefault().IsAction)
                                {
                                    if (loanLogHistories.FirstOrDefault().CollectionDate <= DateTime.Today)
                                    {
                                        Data.trnCollectionLogHistory newCollectionLogHistory = new Data.trnCollectionLogHistory();
                                        newCollectionLogHistory.LoanId = Convert.ToInt32(loanId);
                                        newCollectionLogHistory.CollectionDate = loanLogHistories.FirstOrDefault().CollectionDate;
                                        newCollectionLogHistory.CollectibleAmount = loanLogHistories.FirstOrDefault().CollectibleAmount;
                                        newCollectionLogHistory.PaidAmount = 0;
                                        newCollectionLogHistory.IsCleared = false;
                                        newCollectionLogHistory.IsPenalty = true;
                                        newCollectionLogHistory.IsOverdue = loanLogHistories.FirstOrDefault().IsOverdue;
                                        newCollectionLogHistory.IsFullyPaid = loanLogHistories.FirstOrDefault().IsFullyPaid;
                                        newCollectionLogHistory.CollectorId = loanApplications.FirstOrDefault().CollectorId;
                                        newCollectionLogHistory.AccountId = (from d in db.mstAccounts where d.AccountTransactionTypeId == 2 select d.Id).FirstOrDefault();
                                        db.trnCollectionLogHistories.InsertOnSubmit(newCollectionLogHistory);
                                        db.SubmitChanges();

                                        var day1 = from d in db.trnLoanLogHistories where d.CollectionDate == loanLogHistories.FirstOrDefault().CollectionDate.Date.AddDays(-2) select d;
                                        var day2 = from d in db.trnLoanLogHistories where d.CollectionDate == loanLogHistories.FirstOrDefault().CollectionDate.Date.AddDays(-1) select d;

                                        var penaltyValue = 10;
                                        if (day1.Any() && day2.Any())
                                        {
                                            if (day1.FirstOrDefault().IsPenalty && day2.FirstOrDefault().IsPenalty)
                                            {
                                                penaltyValue = 20;
                                            }
                                        }

                                        var updateLoanLogHistory = loanLogHistories.FirstOrDefault();
                                        updateLoanLogHistory.PenaltyAmount = penaltyValue;
                                        updateLoanLogHistory.PaidAmount = 0;
                                        updateLoanLogHistory.CurrentBalanceAmount = loanLogHistories.FirstOrDefault().CollectibleAmount + penaltyValue + loanLogHistories.FirstOrDefault().PreviousBalanceAmount;
                                        updateLoanLogHistory.IsPenalty = true;
                                        updateLoanLogHistory.IsCleared = false;
                                        db.SubmitChanges();

                                        var loanLogHistoryByCollectionDatePrevious = from d in db.trnLoanLogHistories where d.LoanId == Convert.ToInt32(loanId) && d.CollectionDate == loanLogHistories.FirstOrDefault().CollectionDate.Date.AddDays(-1) select d;
                                        if (loanLogHistoryByCollectionDatePrevious.Any())
                                        {
                                            var updateLoanLogHistoryByCollectionDatePrevious = loanLogHistoryByCollectionDatePrevious.FirstOrDefault();
                                            updateLoanLogHistoryByCollectionDatePrevious.IsAction = false;
                                            db.SubmitChanges();

                                            var loanLogHistoryByCollectionDate = from d in db.trnLoanLogHistories where d.LoanId == Convert.ToInt32(loanId) && d.CollectionDate == loanLogHistories.FirstOrDefault().CollectionDate.Date.AddDays(1) select d;
                                            if (loanLogHistoryByCollectionDate.Any())
                                            {
                                                var updateLoanLogHistoryByCollectionDate = loanLogHistoryByCollectionDate.FirstOrDefault();
                                                updateLoanLogHistoryByCollectionDate.PreviousBalanceAmount = loanLogHistories.FirstOrDefault().CollectibleAmount + loanLogHistories.FirstOrDefault().PenaltyAmount + loanLogHistories.FirstOrDefault().PreviousBalanceAmount;
                                                updateLoanLogHistoryByCollectionDate.CurrentBalanceAmount = loanLogHistoryByCollectionDate.FirstOrDefault().CollectibleAmount + loanLogHistories.FirstOrDefault().CollectibleAmount + loanLogHistories.FirstOrDefault().PenaltyAmount + loanLogHistories.FirstOrDefault().PreviousBalanceAmount;
                                                updateLoanLogHistoryByCollectionDate.IsAction = true;
                                                db.SubmitChanges();
                                            }
                                        }
                                        else
                                        {
                                            var loanLogHistoryByCollectionDate = from d in db.trnLoanLogHistories where d.LoanId == Convert.ToInt32(loanId) && d.CollectionDate == loanLogHistories.FirstOrDefault().CollectionDate.Date.AddDays(1) select d;
                                            if (loanLogHistoryByCollectionDate.Any())
                                            {
                                                var updateLoanLogHistoryByCollectionDate = loanLogHistoryByCollectionDate.FirstOrDefault();
                                                updateLoanLogHistoryByCollectionDate.PreviousBalanceAmount = loanLogHistories.FirstOrDefault().CollectibleAmount + loanLogHistories.FirstOrDefault().PenaltyAmount + loanLogHistories.FirstOrDefault().PreviousBalanceAmount;
                                                updateLoanLogHistoryByCollectionDate.CurrentBalanceAmount = loanLogHistoryByCollectionDate.FirstOrDefault().CollectibleAmount + loanLogHistories.FirstOrDefault().CollectibleAmount + loanLogHistories.FirstOrDefault().PenaltyAmount + loanLogHistories.FirstOrDefault().PreviousBalanceAmount;
                                                updateLoanLogHistoryByCollectionDate.IsAction = true;
                                                db.SubmitChanges();
                                            }
                                        }

                                        return Request.CreateResponse(HttpStatusCode.OK);
                                    }
                                    else
                                    {
                                        return Request.CreateResponse(HttpStatusCode.BadRequest);
                                    }
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.BadRequest);
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

        // loan log history get penalty value
        [Authorize]
        [HttpGet]
        [Route("api/loanLogHistory/getPenaltyValue/{collectionDate}")]
        public Decimal getLoanLogHistoryPenaltyValue(String collectionDate)
        {
            var day1 = from d in db.trnLoanLogHistories where d.CollectionDate == Convert.ToDateTime(collectionDate).Date.AddDays(-2) select d;
            var day2 = from d in db.trnLoanLogHistories where d.CollectionDate == Convert.ToDateTime(collectionDate).Date.AddDays(-1) select d;

            var penaltyValue = 10;
            if (day1.Any() && day2.Any())
            {
                if (day1.FirstOrDefault().IsPenalty && day2.FirstOrDefault().IsPenalty)
                {
                    penaltyValue = 20;
                }
            }

            return penaltyValue;
        }
    }
}