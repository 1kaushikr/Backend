using Newtonsoft.Json.Linq;


namespace DomainLogic.Repository
{
    public interface IDataService
    {
        List<Resume> FetchResume(JObject jsonIds);
        List<Resume> Get();
        Resume Get(string id);
        bool PostResume(Resume person);
        bool PostConvergeResume(ConvergeResume person);
    }
}