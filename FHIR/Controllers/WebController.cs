using FHIR.Data;
using FHIR.Models;
using FHIR.Models.Dto;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Security.AccessControl;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using Hl7.FhirPath.Sprache;
using Patient = FHIR.Models.Patient;
using System.Collections.Concurrent;

namespace FHIR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebController : ControllerBase
    {
        private readonly AppDbContext _db;
        public WebController(AppDbContext db) {
            _db = db;
        }
        [HttpGet]
        public ActionResult<IEnumerable<PatientDto>> Get(string resourcetype)
        {
            string r = resourcetype;
            if (r != Hl7.Fhir.Model.ResourceType.Patient.ToString() && r != Hl7.Fhir.Model.ResourceType.Practitioner.ToString() && r != Hl7.Fhir.Model.ResourceType.DiagnosticReport.ToString())
            {
                return BadRequest("Give me correct responsetype");
            }
       
            var json = _db.patients.Where(u => u.Resource_Type == r).ToList();
            //Specially for list of dto by using LINQ----------
           /*  var dto = json.Select(s => new PatientDto()
             {
                 Id = s.Id,
                 RawResource = s.RawResource,
                 Resource_Type = s.Resource_Type,
             });
             return Ok(dto);*/
            dynamic dto = new List<dynamic>();
            foreach (var item in json)
            {
                dynamic obj = new 
                {
                    //   NId=item.Id,
                    //   NRawResource = item.RawResource,
                     
                      Awesome = ("Id: " + item.Id  + "RawResource:  " + item.RawResource)
                };
                //  dto.Add("Id: " + item.Id + "RawResource:  " + item.RawResource);
                dto.Add(obj);
            } 
            return Ok(dto);//succesfully run Awesome

            //  return Ok(json);
            //below code specially for list dto--------------------
            /* var dto = new List<PatientDto>();
             foreach(var item in json)
             {
                 PatientDto obj = new PatientDto()
                 {
                     Id = item.Id,
                     RawResource = item.RawResource,
                     Resource_Type = item.Resource_Type,
                 };
                 dto.Add(obj);
             }
             return Ok(dto);*/
        }
        [HttpGet("{id:Guid}")]//web url=/id it looks like{id}
        // public ActionResult<PatientDto> Get(Guid id)
        public ActionResult Get(Guid id)
        {
            var patientv = _db.patients.Where(u => u.Id == id).FirstOrDefault();//2
            if (patientv == null)
            {
                return NotFound("Give me correct Id");
            }
            var abc = patientv.Id;
            var abc1= patientv.RawResource;
            //var obj2 = obj + obj1;this can also we can *
            var abc2= "Id: " + abc + "\nRawResource: " + abc1;//with brackets also works var abc2=("Id: " + abc + "\nRawResource: " + abc1);
            // return Ok("Id: " + obj  +  "\nRawResource: " + obj1);
            return Ok(abc2); //Awesome this works succesfully//we can this for *
           // return Ok(patientv);
          //specially for single dto---------------------
           /* return new PatientDto
            {
                Id = patientv.Id,
                RawResource = patientv.RawResource,
                Resource_Type = patientv.Resource_Type,
            };*/
        }
        //Done successfully store data by hardcoded values
        //-----------------------------DiagnosticReport--------------------------------------
        [HttpPost]
        public ActionResult<PatientDto> Create([FromBody] PatientDto patientdto)
        {
            Hl7.Fhir.Model.DiagnosticReport diagnosticreport = new DiagnosticReport();
            diagnosticreport.Id = Guid.NewGuid().ToString();
            Hl7.Fhir.Model.Identifier identifier = new Identifier();
            identifier.Use = Identifier.IdentifierUse.Usual;
            Hl7.Fhir.Model.CodeableConcept codeableConcept = new CodeableConcept();
            Hl7.Fhir.Model.Coding coding = new Coding();
            coding.Code = "PRN";
            coding.Display = "Passport Number";
            codeableConcept.Coding = new List<Coding> { coding };
            codeableConcept.Text = "PRN34535G34";
            identifier.Type = codeableConcept;
            diagnosticreport.Identifier = new List<Identifier> { identifier };
            Hl7.Fhir.Model.ResourceReference resourceReference = new ResourceReference();
            resourceReference.Reference = "Patient/034AB16";
            resourceReference.Type = "Patient";
            diagnosticreport.BasedOn = new List<ResourceReference> { resourceReference };
            diagnosticreport.Status = DiagnosticReport.DiagnosticReportStatus.Final;
            Hl7.Fhir.Model.CodeableConcept codeableConcept1 = new CodeableConcept();
            Hl7.Fhir.Model.Coding coding1 = new Coding();
            coding1.Code = "AU";
            coding1.Display = "Audiology";
            codeableConcept1.Coding =new List<Coding> {coding1};
            codeableConcept1.Text = "Health";
            diagnosticreport.Category = new List<CodeableConcept> { codeableConcept1 };
            Hl7.Fhir.Model.CodeableConcept codeableConcept2 = new CodeableConcept();
            Hl7.Fhir.Model.Coding coding2 = new Coding();
            coding2.Code = "7";
            coding2.Display = "Acyclovir [Susceptibility]";
            codeableConcept2.Coding =new List<Coding> { coding2};
            codeableConcept2.Text = "34234DFFWEF";
            diagnosticreport.Code = codeableConcept2;
            Hl7.Fhir.Model.ResourceReference resourceReference1 = new ResourceReference();
            resourceReference1.Reference = "Patient/034AB16";
            resourceReference1.Type = "Patient";
            diagnosticreport.Subject = resourceReference1;
            Hl7.Fhir.Serialization.FhirJsonSerializer fhirJsonSerializer = new FhirJsonSerializer();
            string json=fhirJsonSerializer.SerializeToString(diagnosticreport);
            if (patientdto == null)
            {
                return BadRequest(patientdto);
            }
            Patient model = new()
            {
                Id = Guid.Parse(diagnosticreport.Id),
                RawResource = json,
                Resource_Type = Hl7.Fhir.Model.ResourceType.DiagnosticReport.ToString(),
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
            };
            _db.patients.Add(model);
            _db.SaveChanges();
            return Ok(model);

        }
        //-----------------------------Practitioner------------------------------------
      /*  [HttpPost]
        public ActionResult<PatientDto> Create([FromBody] PatientDto patientdto)
        {
            Hl7.Fhir.Model.Practitioner practitioner = new Hl7.Fhir.Model.Practitioner();
            practitioner.Id=Guid.NewGuid().ToString();
            Hl7.Fhir.Model.Identifier identifier=new Identifier();
            identifier.Use = Identifier.IdentifierUse.Official;
            Hl7.Fhir.Model.CodeableConcept codeableConcept = new CodeableConcept();
            Hl7.Fhir.Model.Coding coding= new Coding();
            coding.System = "http://snomed.info/sct";
            coding.Code = "Dl";
            coding.Display = "Driving License Number";
            codeableConcept.Coding = new List<Coding> {coding};
            codeableConcept.Text = "DL3435G545";
            identifier.Type=codeableConcept;
            identifier.System = "gjggkjerg.com";
            identifier.Value = "gfggfgg";
            practitioner.Identifier=new List<Identifier> { identifier};
            practitioner.Active = true;
            Hl7.Fhir.Model.HumanName humanName = new Hl7.Fhir.Model.HumanName();
            humanName.Text = "David Smith";
            humanName.Family = "Warner";
            List<string> givenNames = new List<string>();
            givenNames.Add("Warner");
            givenNames.Add("Smith");
            humanName.Given= givenNames;
            practitioner.Name = new List<Hl7.Fhir.Model.HumanName> { humanName };
            Hl7.Fhir.Model.ContactPoint contactPoint=new ContactPoint();
            contactPoint.System = ContactPoint.ContactPointSystem.Phone;
            contactPoint.Value = "55556666";
            contactPoint.Use = ContactPoint.ContactPointUse.Home;
            practitioner.Telecom = new List<ContactPoint> { contactPoint };
            practitioner.Gender = AdministrativeGender.Male;
            practitioner.BirthDate=Convert.ToDateTime("1998-03-03").ToString("yyyy-MM-dd");
            Hl7.Fhir.Model.Address address=new Address();
            address.Use = Address.AddressUse.Work;
            address.Type = Address.AddressType.Both;
            address.Text = "Street 1503";
            address.City = "New Jersey";
            address.District = "New York";
            address.State = "California";
            address.Country = "USA";
            practitioner.Address = new List<Address> { address };
            Hl7.Fhir.Serialization.FhirJsonSerializer fhirJsonSerializer= new FhirJsonSerializer();
            string json = fhirJsonSerializer.SerializeToString(practitioner);
            if (patientdto == null)
            {
                return BadRequest(patientdto);
            }
            Patient model = new()
            {
                Id = Guid.Parse(practitioner.Id),
                RawResource = json,
                Resource_Type = Hl7.Fhir.Model.ResourceType.Practitioner.ToString(),
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
            };
            _db.patients.Add(model);
            _db.SaveChanges();
            return Ok(model);
        }*/
        //-----------------For Patient----------------------------------------------------
        /*  [HttpPost]
          public ActionResult<PatientDto> Create([FromBody]PatientDto patientdto) 
          {

              Hl7.Fhir.Model.Patient patient=new Hl7.Fhir.Model.Patient();
              patient.Id = Guid.NewGuid().ToString();
              Hl7.Fhir.Model.HumanName humanName = new Hl7.Fhir.Model.HumanName();
              humanName.Text = "Royal World";
              humanName.Family = "World";
              List<string> givenNames=new List<string>();
              givenNames.Add("Royalty");
              givenNames.Add("Royal");
             givenNames.Add("Roy");
              humanName.Given= givenNames;
              patient.Name=new List<Hl7.Fhir.Model.HumanName> { humanName };
             Hl7.Fhir.Model.ContactPoint tele=new Hl7.Fhir.Model.ContactPoint();
             tele.System = ContactPoint.ContactPointSystem.Email;
             tele.Value = "royal@world.com";
             tele.Use = ContactPoint.ContactPointUse.Work;
             patient.Telecom= new List<ContactPoint> { tele }; //new List<Hl7.Fhir.Model.ContactPoint> { tele };//this is prefered way
             Hl7.Fhir.Model.Address address=new Hl7.Fhir.Model.Address();
             address.Use=Address.AddressUse.Work;
             address.Type = Address.AddressType.Both;
             address.Text = "Street 1503";
             address.City = "New Jersey";
             address.District = "New York";
             address.State = "California";
             address.Country = "USA";
             patient.Address=new List<Address> { address }; //new List<Hl7.Fhir.Model.Address> { address };//this is prefered way
             patient.Gender = Hl7.Fhir.Model.AdministrativeGender.Female;
              patient.BirthDate = Convert.ToDateTime("1998-03-03").ToString("yyyy-MM-dd");
              Hl7.Fhir.Serialization.FhirJsonSerializer fhirJsonSerializer = new FhirJsonSerializer();
             // Hl7.Fhir.Serialization.FhirJsonParser fhirJsonParser = new Hl7.Fhir.Serialization.FhirJsonParser();
              string json = fhirJsonSerializer.SerializeToString(patient);

             // patient = fhirJsonParser.Parse<Patient>(json);//for deserialized
             // patientdto.RawResource = json;
              if (patientdto == null)
              {
                  return BadRequest(patientdto);
              }
             Patient model = new()
             {
                 //RawResource = patientdto.RawResource
                 Id = Guid.Parse(patient.Id),
                 RawResource = json,
                 Resource_Type = Hl7.Fhir.Model.ResourceType.Patient.ToString(),
                  Created=DateTime.Now,
                  LastUpdated= DateTime.Now,
              };
              _db.patients.Add(model);
              _db.SaveChanges();
              return Ok(patientdto);
            // return Ok(json);
            }    */
        //---------------------------------------Created by json input values---------------------------------
        // [HttpPost("{resourcetype:string}")]
        //  [HttpPost]

        //public ActionResult<PatientDto> Create([FromBody] JsonPatchDocument<Hl7.Fhir.Model.Patient> jsondata)
        //public ActionResult<PatientDto> Create([FromBody] Hl7.Fhir.Model.Patient jsondata)
        /*public ActionResult<PatientDto> Create([FromBody] string json)
        {*/
        //Hl7.Fhir.Model.Patient patient = new Hl7.Fhir.Model.Patient();
        //var pt = json.ToString();
        // patient = JsonSerializer.Deserialize<Hl7.Fhir.Model.Patient>(pt);
        //Hl7.Fhir.Serialization.FhirJsonSerializer fhirJsonSerializer = new FhirJsonSerializer();
        //string jsond = fhirJsonSerializer.SerializeToString(patient);

        //Patientp model = new()
        //{
        //    //RawResource = patchDTO.RawResource
        //    RawResource = jsond
        //};
        //_db.patients.Add(model);
        //_db.SaveChanges();
        //return Ok();
        //  public ActionResult<PatientDto> Create([FromBody] Hl7.Fhir.Model.Patient js)

        //[Route("patient")] url as /patient?resourcetype=resourcetype value

        // [Route("patient/{resourcetype}")]//url as /pateint/resourcetype value
        //url as /resourcetype value
        // [HttpPost("patient/{resourcetype}")]
        [HttpPost("{resourcetype}")]
        public IActionResult Create(string resourcetype,[FromBody] dynamic resource)
        {
            string resourceToPass = Convert.ToString(resource);
            if (resourcetype == Hl7.Fhir.Model.ResourceType.Patient.ToString())
            {
              //  string resourceToPass = Convert.ToString(resource);
                FhirJsonParser fhirJsonParser = new FhirJsonParser();
                fhirJsonParser.Settings.AcceptUnknownMembers = true;
                //Hl7.Fhir.Model.ResourceType.Patient.ToString();//given by mam
                //var jss=js;
                //Hl7.Fhir.Model.Patient pat =JsonConvert.DeserializeObject<Hl7.Fhir.Model.Patient>(resourceToPass);
                Hl7.Fhir.Model.Patient patient = new Hl7.Fhir.Model.Patient();
                patient = fhirJsonParser.Parse<Hl7.Fhir.Model.Patient>(resourceToPass);
                //  Hl7.Fhir.Model.Patient pat = new Hl7.Fhir.Model.Patient();
                patient.Id = Guid.NewGuid().ToString();
                patient.Meta = new Hl7.Fhir.Model.Meta();
                patient.Meta.LastUpdated = DateTime.Now;
                // patient.Meta.LastUpdated = DateTimeOffset.UtcNow;// pat.Text = js.Text;//  pat.Gender = js.Gender;//   pat.BirthDate = js.BirthDate;
                Hl7.Fhir.Serialization.FhirJsonSerializer fhirJsonSerializer = new FhirJsonSerializer();
                string jsonnew = fhirJsonSerializer.SerializeToString(patient);
                Patient model = new()
                {
                    //RawResource = patchDTO.RawResource
                    Id = Guid.Parse(patient.Id),
                    RawResource = jsonnew,
                    Resource_Type = resourcetype,
                    Created = DateTime.Now,
                    LastUpdated = DateTime.Now,
                };
                _db.patients.Add(model);//3
                _db.SaveChanges();
                return Ok(jsonnew);
            }
            if(resourcetype == Hl7.Fhir.Model.ResourceType.Practitioner.ToString())
            {
                FhirJsonParser fhirJsonParser = new FhirJsonParser();
                fhirJsonParser.Settings.AcceptUnknownMembers = true;
                Hl7.Fhir.Model.Practitioner practitioner = new Hl7.Fhir.Model.Practitioner();
                practitioner = fhirJsonParser.Parse<Hl7.Fhir.Model.Practitioner>(resourceToPass);
                practitioner.Id = Guid.NewGuid().ToString();
                practitioner.Meta = new Hl7.Fhir.Model.Meta();
                practitioner.Meta.LastUpdated = DateTime.Now;
                Hl7.Fhir.Serialization.FhirJsonSerializer fhirJsonSerializer = new FhirJsonSerializer();
                string jsonnew = fhirJsonSerializer.SerializeToString(practitioner);
                Patient model = new()
                {
                    //RawResource = patchDTO.RawResource
                    Id = Guid.Parse(practitioner.Id),
                    RawResource = jsonnew,
                    Resource_Type = resourcetype,
                    Created = DateTime.Now,
                    LastUpdated = DateTime.Now,
                };
                _db.patients.Add(model);//3
                _db.SaveChanges();
                return Ok(jsonnew);
            }
            if (resourcetype == Hl7.Fhir.Model.ResourceType.DiagnosticReport.ToString())
            {
                FhirJsonParser fhirJsonParser = new FhirJsonParser();
                fhirJsonParser.Settings.AcceptUnknownMembers = true;
                Hl7.Fhir.Model.DiagnosticReport diagnosticreport = new Hl7.Fhir.Model.DiagnosticReport();
                diagnosticreport = fhirJsonParser.Parse<Hl7.Fhir.Model.DiagnosticReport>(resourceToPass);
                diagnosticreport.Id = Guid.NewGuid().ToString();
                diagnosticreport.Meta = new Hl7.Fhir.Model.Meta();
                diagnosticreport.Meta.LastUpdated = DateTime.Now;
                Hl7.Fhir.Serialization.FhirJsonSerializer fhirJsonSerializer = new FhirJsonSerializer();
                string jsonnew = fhirJsonSerializer.SerializeToString(diagnosticreport);
                Patient model = new()
                {
                    //RawResource = patchDTO.RawResource
                    Id = Guid.Parse(diagnosticreport.Id),
                    RawResource = jsonnew,
                    Resource_Type = resourcetype,
                    Created = DateTime.Now,
                    LastUpdated = DateTime.Now,
                };
                _db.patients.Add(model);//3
                _db.SaveChanges();
                return Ok(jsonnew);
            }
                return BadRequest("Give Correct resourcetype");
            //output for these {"resourceType":"Patient"}
        }
        //--------------------------------------------DELETE-------------------------------------------------
         [HttpDelete("{id:Guid}")]//web url =/id?id=5 it looks like /id
       // [HttpDelete("{id:string}")]//web url=/id it looks like{id}
       // public IActionResult Delete(Guid id)
       public string Delete(Guid id)
        {
            var patient = _db.patients.Where(u => u.Id == id).FirstOrDefault();//4
            if(patient == null)
            {
                return "Give me correct Id to delete data";
            }
            _db.patients.Remove(patient);//5
            _db.SaveChanges();
            return "Delete Succesfully";

        }
        //-----------------------------------UPDATE----------------------------------------------------------
        [HttpPut("{id:Guid}")]//web url =/id?id=5 it looks like /id
       // [HttpPut("{id:string}")]//web url=/id it looks like{id}
        public IActionResult Update(Guid id,string resourcetype,[FromBody] dynamic resource)
        {
            var patientv = _db.patients.Where(u => u.Id == id).FirstOrDefault();
            if(patientv == null || (resourcetype !=Hl7.Fhir.Model.ResourceType.Patient.ToString() && resourcetype!=Hl7.Fhir.Model.ResourceType.Practitioner.ToString() && resourcetype != Hl7.Fhir.Model.ResourceType.DiagnosticReport.ToString())) 
            {//logic of above line is awesome just try to understand
                return NotFound("Give me Correct Id & resourcetype to update record.");
            }
            string resourceToPass = Convert.ToString(resource);
            FhirJsonParser fhirJsonParser = new FhirJsonParser();
            fhirJsonParser.Settings.AcceptUnknownMembers = true;
            Hl7.Fhir.Serialization.FhirJsonSerializer fhirJsonSerializer = new FhirJsonSerializer();
            if (resourcetype == Hl7.Fhir.Model.ResourceType.Patient.ToString())
            {
                Hl7.Fhir.Model.Patient patient = new Hl7.Fhir.Model.Patient();
                patient = fhirJsonParser.Parse<Hl7.Fhir.Model.Patient>(resourceToPass);
                //  patient.Id=id.ToString();
                patient.Meta = new Hl7.Fhir.Model.Meta();
                patient.Meta.LastUpdated = DateTime.Now;              
                string jsonn = fhirJsonSerializer.SerializeToString(patient);
                //   var patientv = _db.patients.Where(u => u.Id == id).FirstOrDefault();
                patientv.RawResource = jsonn;
                patientv.Resource_Type = resourcetype;
                patientv.LastUpdated = DateTime.Now;
                _db.SaveChanges();
                var updated = _db.patients.Where(u => u.Id == id).FirstOrDefault();
                return Ok(updated);
            }
            if (resourcetype == Hl7.Fhir.Model.ResourceType.Practitioner.ToString())
            {
                Hl7.Fhir.Model.Practitioner practitioner = new Hl7.Fhir.Model.Practitioner();
                practitioner = fhirJsonParser.Parse<Hl7.Fhir.Model.Practitioner>(resourceToPass);
                practitioner.Meta = new Hl7.Fhir.Model.Meta();
                practitioner.Meta.LastUpdated = DateTime.Now;
                string jsonnew = fhirJsonSerializer.SerializeToString(practitioner);
                patientv.RawResource = jsonnew;
                patientv.Resource_Type = resourcetype;
                patientv.LastUpdated = DateTime.Now;
                _db.SaveChanges();
                var updatedpatient = _db.patients.Where(u => u.Id == id).FirstOrDefault();
                return Ok(updatedpatient);
            }
            if (resourcetype == Hl7.Fhir.Model.ResourceType.DiagnosticReport.ToString())
            {
                Hl7.Fhir.Model.DiagnosticReport diagnosticreport = new Hl7.Fhir.Model.DiagnosticReport();
                diagnosticreport = fhirJsonParser.Parse<Hl7.Fhir.Model.DiagnosticReport>(resourceToPass);
                diagnosticreport.Meta = new Hl7.Fhir.Model.Meta();
                diagnosticreport.Meta.LastUpdated = DateTime.Now;
                string jsonn = fhirJsonSerializer.SerializeToString(diagnosticreport);
                patientv.RawResource = jsonn;
                patientv.Resource_Type = resourcetype;
                patientv.LastUpdated = DateTime.Now;
                _db.SaveChanges();
                var updated = _db.patients.Where(u => u.Id == id).FirstOrDefault();
                return Ok(updated);
            }
            return BadRequest("Something went wrong");
        }
    }
}
