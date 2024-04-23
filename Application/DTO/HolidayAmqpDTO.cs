using Newtonsoft.Json;

namespace Application.DTO
{
    public class HolidayAmqpDTO
    {
        public long Id { get; set; }
        public long _colabId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public HolidayAmqpDTO()
        {
        }

        public HolidayAmqpDTO(long colabId, long id, DateOnly startDate, DateOnly endDate)
        {
            Id = id;
            _colabId = colabId;
            StartDate = startDate;
            EndDate = endDate;
        }

        static public HolidayAmqpDTO Deserialize(string jsonMessage)
        {
            var holidayAmqpDTO = JsonConvert.DeserializeObject<HolidayAmqpDTO>(jsonMessage);
            return holidayAmqpDTO!;
        }
    }
}