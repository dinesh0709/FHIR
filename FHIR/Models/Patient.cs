using System;
using System.Collections.Generic;

namespace FHIR.Models
{
    public partial class Patient
    {
        public Guid Id { get; set; }
        public string RawResource { get; set; } 
        public string Resource_Type { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
