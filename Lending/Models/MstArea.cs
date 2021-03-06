﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Lending.Models
{
    public class MstArea
    {
        [Key]
        public Int32 Id { get; set; }
        public String AreaNumber { get; set; }
        public String Area { get; set; }
        public String Description { get; set; }
        public Int32 SupervisorStaffId { get; set; }
        public String SupervisorStaff { get; set; }
        public String Collector { get; set; }
        public Boolean IsLocked { get; set; }
        public Int32 CreatedByUserId { get; set; }
        public String CreatedByUser { get; set; }
        public String CreatedDateTime { get; set; }
        public Int32 UpdatedByUserId { get; set; }
        public String UpdatedByUser { get; set; }
        public String UpdatedDateTime { get; set; }
    }
}