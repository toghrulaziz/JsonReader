using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonReader.Models
{
    public class Car
    {
        public Guid? Id { get; set; } = Guid.NewGuid();
        public string? Vendor { get; set; }
        public string? Model { get; set; }
        public string? ImagePath { get; set; }
        public int? Year { get; set; }
    }
}
