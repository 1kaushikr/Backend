using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using DomainLogic.Repository;
using Nest;
using System.Collections.Generic;

namespace DataLayer
{
    public class DataService : IDataService
    {
        private readonly IMongoCollection<Resume> _collectionResume;
        private readonly IMongoCollection<ConvergeResume> _collectionConvergeResume;

        public DataService(IMongoClient mongoClient)
        {
            IMongoDatabase dataBase = mongoClient.GetDatabase("Resumes");
            _collectionResume = dataBase.GetCollection<Resume>("Resume");
            _collectionConvergeResume = dataBase.GetCollection<ConvergeResume>("ConvergeResume");
        }

        /// <summary>
        /// Gets all the resume present in the Database.
        /// </summary>
        /// <returns>List of Resumes</returns>
        public List<Resume> Get()
        {
            try
            {
                return _collectionResume.Find(_ => true).ToList();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Post the resume to Database
        /// </summary>
        /// <param name="resume">Resume to be Posted</param>
        public bool PostResume(Resume resume)
        {
            if(resume == null) return false;
            try
            {
                _collectionResume.InsertOne(resume);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Post Converged Resume to another collection of same Database as Resume. 
        /// </summary>
        /// <param name="resume">Converged Resume to be posted</param>
        public bool PostConvergeResume(ConvergeResume resume)
        {
            if (resume == null) return false;
            try
            {
                _collectionConvergeResume.InsertOne(resume);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves resume of particular Id from Database and return it.
        /// </summary>
        /// <param name="id">Id of which resume is required</param>
        /// <returns>Retrieved resume</returns>
        public Resume Get(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length != 24)
            {
                return null;
            }
            try
            {
                return _collectionResume.Find(x => x._id == id).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the list of resumes of Ids given 
        /// </summary>
        /// <param name="jsonIds">Ids of which resums are required.</param>
        /// <returns>List of esumes</returns>
        public List<Resume> FetchResume(JObject jsonIds)
        {
            if(jsonIds == null) return null;
            try
            {
                List<Resume> selectedResumes = new List<Resume>();
                JArray idsArray = jsonIds["ids"] as JArray;
                HashSet<string> ids = new HashSet<string>(idsArray.Values<string>());
                List<Resume> resumes = Get();
                foreach (var resume in resumes)
                {
                    if (ids.Contains(resume._id))
                    {
                        selectedResumes.Add(resume);
                    }
                }
                return selectedResumes;
            }
            catch (Exception ex) 
            {
                return null;
            }
        }
    }
}
