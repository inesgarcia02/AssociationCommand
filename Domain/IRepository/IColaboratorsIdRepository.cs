using Domain.Model;

namespace Domain.IRepository
{
    public interface IColaboratorsIdRepository
    {
        Task<IEnumerable<Colaborator>> GetColaboratorsIdAsync();
        Task<Colaborator> GetColaboratorByIdAsync(long id);
        Task<Colaborator> Add(long id);
        Task<bool> ColaboratorExists(long colabId);
    }
}