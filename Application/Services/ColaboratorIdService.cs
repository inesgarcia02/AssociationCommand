
using Application.DTO;
using Domain.IRepository;
using Domain.Model;

namespace Application.Services
{
    public class ColaboratorIdService
    {

        private readonly IColaboratorsIdRepository _colaboratorsIdRepository;
        
        public ColaboratorIdService(IColaboratorsIdRepository colaboratorsIdRepository) {
            _colaboratorsIdRepository = colaboratorsIdRepository;
        }

        public async Task<ColaboratorDTO> Add(long colabId)
        {
            Colaborator colaboratorSaved = await _colaboratorsIdRepository.Add(colabId);

            ColaboratorDTO colabDTO = ColaboratorDTO.ToDTO(colaboratorSaved);

            return colabDTO;
        }
    }
}