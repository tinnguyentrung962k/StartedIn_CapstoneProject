using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Project
{
    public class ProjectFilterDTO
    {
        public string? ProjectName { get; set;}
        public int? TargetFrom { get; set; }
        public int? TargetTo { get; set;}
        public int? RaisedFrom { get; set; }
        public int? RaisedTo { get; set; }
        public decimal? AvailableShareFrom { get; set; }
        public decimal? AvailableShareTo { get; set; }
    }
}
