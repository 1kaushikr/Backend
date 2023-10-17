using DomainLogic.Repository;
using DomainLogic.Model;
using Newtonsoft.Json.Linq;
using System.Text;

namespace DomainLogic.Supervisor
{
    public class ApplicantService : IApplicantService
    {
        private readonly IDataService _dataService;
        private readonly HttpClient _pythonClientModel;
        private readonly HttpClient _pythonClientApi;
        public ApplicantService(IDataService dataService)
        {
            _dataService = dataService;
            _pythonClientModel = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(3600)
            };
            _pythonClientApi = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(3600)
            };
        }

        /// <summary>
        /// Gets all the resume present in database and then return them.
        /// </summary>
        /// <returns>List of Application (model of resume interact with client)</returns>
        public List<Application> Get()
        {
            List<Resume> resumes;
            try
            {
                resumes = _dataService.Get();
                List<Application> applications = new List<Application>();
                foreach (Resume resume in resumes)
                {
                    applications.Add(ResumeApplication(resume));
                }
                return applications;
            }
            catch (Exception ex) 
            {
                return null;
            }
        }

        /// <summary>
        /// First convert the application (model of resume interact with client) to resume. Then post it to database.
        /// </summary>
        /// <param name="application">Application that is to be processed</param>
        public bool Post(Application application)
        {
            if (application == null)
                return false;
            try
            {
                Resume resume = new Resume();
                resume.FirstName = application.FirstName;
                resume.LastName = application.LastName;
                resume.Dob = application.Dob;
                resume.PhoneList = application.PhoneList;
                resume.EmailList = application.EmailList;
                resume.ProList = application.ProList;
                resume.ExpList = application.ExpList;
                resume.EduList = application.EduList;
                resume.Skill = application.Skill;
                if(!_dataService.PostResume(resume))
                {
                    return false;
                }
                if(!_dataService.PostConvergeResume(ConvergeTheResume(resume)))
                {
                    return false;
                }
            }
            catch (Exception ex) 
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Takes in the Id. Retrieve the Resume of that Id from the database. Convert it to client side model of resume (application) and return it.
        /// </summary>
        /// <param name="id">Id of which resume is required</param>
        /// <returns>Application of given Id</returns>
        public Application Get(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length != 24)
            {
                return null;
            }
            try
            {
                Resume resume = _dataService.Get(id);
                return ResumeApplication(resume);
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Takes the user query and send it to a python api to get a reformed question. Then send this reformed question to
        /// another python api to get the Ids of resume who passes the question test. Then fetches Resumes of previous Ids
        /// and convert them to client side format of resume (application) and then return them.  
        /// </summary>
        /// <param name="query">Object that contain query given by user as its one and only property</param>
        /// <returns>List of selected applications</returns>
        public async Task<List<Application>> Query(QueryClass query)
        {
            if (query == null)
                return null;
            try
            {
                StringContent query_ = new StringContent(query.query, Encoding.UTF8, "text/plain");
                Constant constant = new Constant();
                HttpResponseMessage response = await _pythonClientModel.PostAsync(constant.PythonModelAddress + "/query", query_);
                string reformedQuestion = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode != true)
                {
                    return null;
                }
                StringContent reformedQuestion_ = new StringContent(reformedQuestion, Encoding.UTF8, "text/plain");
                response = await _pythonClientApi.PostAsync(constant.PythonApiAddress + "/query", reformedQuestion_);
                string ids = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode != true)
                {
                    return null;
                }
                JObject jsonIds = JObject.Parse(ids);
                List<Resume> resumes = _dataService.FetchResume(jsonIds);
                List<Application> applications = new List<Application>();
                foreach (Resume resume in resumes)
                {
                    applications.Add(ResumeApplication(resume));
                }
                return applications;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves all the resume present in database and then converts them to converge-resume format and
        /// insert them in other collection of same database.
        /// </summary>
        public bool Converter()
        {
            try
            {
                List<Resume> list;
                list = _dataService.Get();
                foreach (Resume resume in list)
                {
                    if (!_dataService.PostConvergeResume(ConvergeTheResume(resume)))
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex) 
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Convert resume to client side format of resume  (application)
        /// </summary>
        /// <param name="resume">Resume to be converted</param>
        /// <returns>Converted Application</returns>
        public Application ResumeApplication(Resume resume)
        {
            if (resume == null)
                return null;
            Application application = new Application();
            application._id = resume._id;
            application.FirstName = resume.FirstName;
            application.LastName = resume.LastName;
            application.Dob = resume.Dob;
            application.PhoneList = resume.PhoneList;
            application.EmailList = resume.EmailList;
            application.EduList = resume.EduList;
            application.ExpList = resume.ExpList;
            application.ProList = resume.ProList;
            application.Skill = resume.Skill;
            return application;
        }

        /// <summary>
        /// Converge the resume to the loose fomat means reumse with no annotation means a paragraph like resume not a point wise resume. 
        /// </summary>
        /// <param name="resume">Resume to be converged</param>
        /// <returns>Converged Resume Object</returns>
        public ConvergeResume ConvergeTheResume(Resume resume)
        {
            if (resume == null)
                return null;
            string exp = "";
            if (resume.ExpList != null)
            {
                for (int i = 0; i < resume.ExpList.Count; i++)
                {
                    string v0, v1, v2, v3, v4;
                    if (resume.ExpList[i].EndDate.Length != 0)
                    {
                        v0 = "I have worked";
                        v1 = resume.ExpList[i].Role.Length > 0 ? "I have worked as " + resume.ExpList[i].Role : v0;
                        v2 = resume.ExpList[i].Org.Length > 0 ? " in " + resume.ExpList[i].Org : "";
                        v3 = resume.ExpList[i].Respon.Length > 0 ? ", there my responibilities were " + resume.ExpList[i].Respon : "";
                    }
                    else
                    {
                        v0 = "I am working";
                        v1 = resume.ExpList[i].Role.Length > 0 ? "I am working as " + resume.ExpList[i].Role : v0;
                        v2 = resume.ExpList[i].Org.Length > 0 ? " in " + resume.ExpList[i].Org : "";
                        v3 = resume.ExpList[i].Respon.Length > 0 ? ", there my responibilities are " + resume.ExpList[i].Respon : "";
                    }
                    v4 = v1 + v2 + v3;
                    exp = v4.Length > v0.Length ? exp + v4 + ". " : exp;
                }
            }
            string edu = "";
            if (resume.EduList != null)
            {
                for (int i = 0; i < resume.EduList.Count; i++)
                {
                    string v0, v1, v2, v3, v4;
                    if (resume.EduList[i].EndDate.Length != 0)
                    {
                        v0 = "I have studied";
                        v1 = resume.EduList[i].Deg.Length > 0 ? "I have completed my " + resume.EduList[i].Deg + " degree" : v0;
                        v2 = resume.EduList[i].Org.Length > 0 ? " from " + resume.EduList[i].Org : "";
                        v3 = resume.EduList[i].Major.Length > 0 ? " majoring in " + resume.EduList[i].Major : "";
                    }
                    else
                    {
                        v0 = "I am studying";
                        v1 = resume.EduList[i].Deg.Length > 0 ? "I am pursuing " + resume.EduList[i].Deg + " degree" : v0;
                        v2 = resume.EduList[i].Org.Length > 0 ? " from " + resume.EduList[i].Org : "";
                        v3 = resume.EduList[i].Major.Length > 0 ? " majoring in " + resume.EduList[i].Major : "";
                    }
                    v4 = v1 + v2 + v3;
                    edu = v4.Length > v0.Length ? edu + v4 + ". " : edu;
                }
            }
            string pro = "";
            if (resume.ProList != null)
            {
                for (int i = 0; i < resume.ProList.Count; i++)
                {
                    string v0, v1, v2, v3;
                    if (resume.ProList[i].EndDate.Length != 0)
                    {
                        v0 = "I have worked on a project,";
                        v1 = resume.ProList[i].Name.Length > 0 ? "I have worked on a project named as " + resume.ProList[i].Name : v0;
                        v2 = resume.ProList[i].Respon.Length > 0 ? " In the project, my Responibilities were " + resume.ProList[i].Respon : "";
                    }
                    else
                    {
                        v0 = "I am working on a project,";
                        v1 = resume.ProList[i].Name.Length > 0 ? "I am working on a project named as " + resume.ProList[i].Name : v0;
                        v2 = resume.ProList[i].Respon.Length > 0 ? " In the project, my Responibilities are " + resume.ProList[i].Respon : "";
                    }
                    v3 = v1 + v2;
                    pro = v3.Length > v0.Length ? pro + v3 + ". " : pro;
                }
            }
            ConvergeResume convergeResume = new ConvergeResume();
            convergeResume._id = resume._id;
            convergeResume.Resume = edu + exp + pro;
            if (resume.Skill.Count > 0)
            {
                string skill = "I am skilled in " + resume.Skill[0];
                for (int i = 1; i < resume.Skill.Count; i++)
                {
                    skill = skill + ", " + resume.Skill[i];
                }
                convergeResume.Resume += skill;
            }
            return convergeResume;
        }
    }
}
