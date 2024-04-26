using Application.DTO;
using Domain.IRepository;
using Domain.Model;
using Gateway;

namespace Application.Services
{
    public class HolidayService
    {

        private readonly IAssociationRepository _associationRepository;
        private HolidayVerificationAmqpGateway _holidayAmqpGateway;

        public HolidayService(IAssociationRepository associationRepository, HolidayVerificationAmqpGateway holidayVerificationAmqpGateway)
        {
            _associationRepository = associationRepository;
            _holidayAmqpGateway = holidayVerificationAmqpGateway;
        }

        public async Task<HolidayAmqpDTO> Validations(HolidayAmqpDTO holidayAmqpDTO)
        {
            
            IEnumerable<AssociationDTO> associationsFiltradasDTO = await GetByColabIdInPeriod(holidayAmqpDTO._colabId, holidayAmqpDTO._holidayPeriod.StartDate, holidayAmqpDTO._holidayPeriod.EndDate);
            if (associationsFiltradasDTO != null)
            {
                foreach (AssociationDTO association in associationsFiltradasDTO)
                {
                    int overlapDays = CalculateOverlap(association.StartDate, association.EndDate, holidayAmqpDTO._holidayPeriod.StartDate, holidayAmqpDTO._holidayPeriod.EndDate);

                    if (overlapDays <= 2)
                    {
                        string stringholidayAmqpDTO = HolidayAmqpDTO.Serialize(holidayAmqpDTO);
                        _holidayAmqpGateway.Publish("Ok " + stringholidayAmqpDTO);
                    }
                    else if (!association.Fundamental)
                    {
                        string stringholidayAmqpDTO = HolidayAmqpDTO.Serialize(holidayAmqpDTO);
                        _holidayAmqpGateway.Publish("Holiday Pendent " + stringholidayAmqpDTO);
                    }
                    else
                    {
                        string stringholidayAmqpDTO = HolidayAmqpDTO.Serialize(holidayAmqpDTO);
                        _holidayAmqpGateway.Publish("Not Ok " + stringholidayAmqpDTO);
                    }
                }
            }
            else{
                string stringholidayAmqpDTO = HolidayAmqpDTO.Serialize(holidayAmqpDTO);
                _holidayAmqpGateway.Publish("Ok " + stringholidayAmqpDTO);
                Console.WriteLine($" [x] Sent {stringholidayAmqpDTO}");
            }
            return null;
        }

        private int CalculateOverlap(DateOnly associationStartDate, DateOnly associationEndDate, DateOnly holidayStartDate, DateOnly holidayEndDate)
        {
            // Verifica se há sobreposição entre as datas de associação e as datas de férias
            if (associationStartDate < holidayEndDate && associationEndDate > holidayStartDate)
            {
                // Calcula a duração da sobreposição em dias
                DateOnly overlapStart = associationStartDate > holidayStartDate ? associationStartDate : holidayStartDate;
                DateOnly overlapEnd = associationEndDate < holidayEndDate ? associationEndDate : holidayEndDate;
                int overlapDays = overlapEnd.Day - overlapStart.Day;
                return overlapDays;
            }
            return 0; // Não há sobreposição
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