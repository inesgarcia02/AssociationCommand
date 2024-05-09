using System.Text.Json;
using Newtonsoft.Json;

namespace Application.DTO
{
    public class AssociationAmqpDTO
    {
        public long Id { get; set; }
        public long ColaboratorId { get; set; }
        public long ProjectId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool Fundamental {get;set;}
        public string Status{get;set;}


        public AssociationAmqpDTO() { }

        public AssociationAmqpDTO(long id, long colabId, long projectId, DateOnly startDate, DateOnly endDate, bool fundamental, string status)
        {
            Id = id;
            ColaboratorId = colabId;
            ProjectId = projectId;
            StartDate = startDate;
            EndDate = endDate;
            Fundamental = fundamental;
            Status = status;
        }

        static public string Serialize(AssociationDTO associationDTO)
        {
            var jsonMessage = JsonConvert.SerializeObject(associationDTO);
            return jsonMessage;
        }

        static public AssociationAmqpDTO Deserialize(string jsonMessage)
        {
            var associationAmqpDTO = JsonConvert.DeserializeObject<AssociationAmqpDTO>(jsonMessage);
            return associationAmqpDTO!;
        }

        static public AssociationDTO ToDTO(AssociationAmqpDTO assoAmqpDTO){
            AssociationDTO associationDTO = new AssociationDTO(assoAmqpDTO.Id, assoAmqpDTO.ColaboratorId, assoAmqpDTO.ProjectId, assoAmqpDTO.StartDate, assoAmqpDTO.EndDate, assoAmqpDTO.Fundamental);

            return associationDTO;
        }
    }
}