using DomainLogic.Supervisor;
using DomainLogic.Model;
using Microsoft.AspNetCore.Mvc;



namespace API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ApplicantController : ControllerBase
    {
        private readonly IApplicantService _applicantService;

        public ApplicantController(IApplicantService applicantService)
        {
            _applicantService = applicantService;
        }


        /// <summary>
        /// Gets the list of all the resumes present in the database. 
        /// </summary>
        /// <returns>List of all the resumes</returns>
        [HttpGet]
        public ActionResult<List<Application>> Get()
        {
            var response = _applicantService.Get();
            if (response == null)
            {
                return BadRequest();
            }
            return _applicantService.Get();
        }


        /// <summary>
        /// Save the information of resume as selected by client to the database.
        /// </summary>
        /// <param name="application">Application model that has been selected by the client</param>
        [HttpPost]
        public ActionResult Post(Application application) 
        {
            if(application == null)
            {
                return BadRequest();
            }
            try
            {
                if(!_applicantService.Post(application))
                {
                    return BadRequest();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Gets the resume of Id coming from client.
        /// </summary>
        /// <param name="id">Id coming from client</param>
        /// <returns>Application model filled with the information from resume whose Id is given in the parameter</returns>
        [HttpGet]
        public ActionResult<Application> Retrieve([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length!=24)
            {
                return BadRequest();
            }

            var response = _applicantService.Get(id);
            if (response == null)
            {
                return BadRequest();
            }
            return response;

        }

        /// <summary>
        /// Retrieves the resumes relevant to the query given by the client.
        /// </summary>
        /// <param name="query">Object that contain query given by the client as its one and only property</param>
        /// <returns>List of resumes</returns>
        [HttpPost]
        public async Task<ActionResult<List<Application>>> Query(QueryClass query)
        {
            if (query == null)
            {
                return BadRequest();
            }
            var response = await _applicantService.Query(query);
            if (response == null)
            {
                return BadRequest();
            }
            return response;
        }

        /// <summary>
        /// Convert all the resumes present in database to converged format.
        /// </summary>
        [HttpGet]
        public ActionResult Convert()
        {
            try
            {
                if(!_applicantService.Converter())
                {  return BadRequest(); }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}