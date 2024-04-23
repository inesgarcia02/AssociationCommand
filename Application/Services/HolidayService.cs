using Application.DTO;
using Domain.IRepository;
using Domain.Model;

namespace Application.Services
{
    public class HolidayService
    {

        private readonly IAssociationRepository _associationRepository;

        public HolidayService(IAssociationRepository associationRepository)
        {
            _associationRepository = associationRepository;
        }

        public async Task<HolidayAmqpDTO> Validations(HolidayAmqpDTO holidayAmqpDTO)
        {
            IEnumerable<AssociationDTO> associationsFiltradasDTO = await GetByColabIdInPeriod(holidayAmqpDTO._colabId, holidayAmqpDTO.StartDate, holidayAmqpDTO.EndDate);
            if (associationsFiltradasDTO != null)
            {
                foreach (AssociationDTO association in associationsFiltradasDTO)
                {
                    // Verifica se há sobreposição entre as datas de associação e as datas de férias
                    if (association.StartDate < holidayAmqpDTO.EndDate && association.EndDate > holidayAmqpDTO.StartDate)
                    {
                        // Calcula a duração da sobreposição em dias
                        DateOnly overlapStart = association.StartDate > holidayAmqpDTO.StartDate ? association.StartDate : holidayAmqpDTO.StartDate;
                        DateOnly overlapEnd = association.EndDate < holidayAmqpDTO.EndDate ? association.EndDate : holidayAmqpDTO.EndDate;
                        int overlapDays = overlapEnd.Day - overlapStart.Day;

                        if (overlapDays <= 2)
                        {

                        }
                        else if (association.Fundamental.Equals(true))
                        {

                        }
                    }
                }
            }
            return null;
        }


        public async Task<IEnumerable<AssociationDTO>> GetByColabIdInPeriod(long colabId, DateOnly startDate, DateOnly endDate)
        {
            IEnumerable<Association> associations = await _associationRepository.GetAssociationsByColabIdInPeriodAsync(colabId, startDate, endDate);
            if (associations.Count() > 0)
            {
                // DateOnly startDateFiltered = (associations.Min(a => a.StartDate) < startDate) ? startDate : associations.Min(a => a.StartDate);
                // DateOnly endDateFiltered = (associations.Max(a => a.EndDate) > endDate) ? endDate : associations.Max(a => a.EndDate);
                IEnumerable<AssociationDTO> associationDTO = AssociationDTO.ToDTO(associations);
                return associationDTO;

            }
            return null;
        }
    }
}