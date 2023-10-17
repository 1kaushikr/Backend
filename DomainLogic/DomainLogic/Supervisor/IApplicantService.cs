using DomainLogic.Model;

namespace DomainLogic.Supervisor
{
    public interface IApplicantService
    {
        List<Application> Get();
        Application Get(string id);
        bool Post(Application person);
        Task<List<Application>> Query(QueryClass query);
        bool Converter();
    }
}