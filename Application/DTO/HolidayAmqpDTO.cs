using Newtonsoft.Json;
 
namespace Application.DTO
{
    public class HolidayAmqpDTO
    {
       public long Id { get; set; }
        public long _colabId{ get; set; }
 
        public string _status{get;set;}
 
        public HolidayPeriodDTO _holidayPeriod { get; set; }
        public HolidayAmqpDTO()
        {
        }
 
        public HolidayAmqpDTO(long colabId,long id,HolidayPeriodDTO holidayPeriod, string status)
        {
            Id = id;
            _colabId = colabId;
            _holidayPeriod = holidayPeriod;
            _status = status;
        }
 
        static public string Serialize(HolidayAmqpDTO holidayAmqpDTO)
        {
            var jsonMessage = JsonConvert.SerializeObject(holidayAmqpDTO);
            return jsonMessage;
        }
 
        static public HolidayAmqpDTO Deserialize(string jsonMessage)
        {
            var holidayAmqpDTO = JsonConvert.DeserializeObject<HolidayAmqpDTO>(jsonMessage);
            return holidayAmqpDTO!;
        }
    }
}