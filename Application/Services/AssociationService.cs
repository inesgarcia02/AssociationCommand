namespace Application.Services;

using Domain.Model;
using Application.DTO;

using Domain.IRepository;
using Gateway;
using RabbitMQ.Client.Events;

public class AssociationService
{
    private AssociationCreatedAmqpGateway _associationCreatedAmqpGateway;
    private AssociationPendentAmqpGateway _associationPendentAmqpGateway;
    private readonly IAssociationRepository _associationRepository;
    private readonly IColaboratorsIdRepository _colaboratorsRepository;
    private readonly IProjectRepository _projectRepository;

    public AssociationService(IAssociationRepository associationRepository, IColaboratorsIdRepository colaboratorsRepository, IProjectRepository projectRepository, AssociationCreatedAmqpGateway associationCreatedAmqpGateway, AssociationPendentAmqpGateway associationPendentAmqpGateway)
    {
        _associationRepository = associationRepository;
        _colaboratorsRepository = colaboratorsRepository;
        _projectRepository = projectRepository;
        _associationCreatedAmqpGateway = associationCreatedAmqpGateway;
        _associationPendentAmqpGateway = associationPendentAmqpGateway;
    }

    public async Task<IEnumerable<AssociationDTO>> GetAll()
    {
        IEnumerable<Association> association = await _associationRepository.GetAssociationsAsync();

        IEnumerable<AssociationDTO> associationsDTO = AssociationDTO.ToDTO(association);

        return associationsDTO;
    }

    public async Task<AssociationDTO> GetById(long id)
    {
        Association association = await _associationRepository.GetAssociationsByIdAsync(id);

        if (association != null)
        {
            AssociationDTO associationDTO = AssociationDTO.ToDTO(association);

            return associationDTO;
        }
        return null;
    }
    public async Task<AssociationDTO> Add(AssociationDTO associationDTO, List<string> errorMessages)
    {
        try
        {
            // Tente adicionar a associação ao banco de dados
            Association association = AssociationDTO.ToDomain(associationDTO);
            // Verifica se a associação já existe com base nos detalhes da associação sendo adicionada
            bool exists = await VerifyAssociation(associationDTO, errorMessages);

            if (!exists)
            {
                return null;
            }
            Association associationSaved = await _associationRepository.Add(association);

            // Se a adição for bem-sucedida, retorne a associação
            AssociationDTO assoDTO = AssociationDTO.ToDTO(associationSaved);
            return assoDTO;
        }
        catch (ArgumentException ex)
        {
            // Se ocorrer um erro ao adicionar a associação, adicione a mensagem de erro
            errorMessages.Add(ex.Message);
            return null;
        }
    }

    public async Task<AssociationDTO> PublishPending(AssociationDTO associationDTO, List<string> errorMessages)
    {
        bool verify = await VerifyAssociation(associationDTO, errorMessages);

        if (verify)
        {
            // Obter o último associationId do banco de dados
            long lastAssociationId = await _associationRepository.GetLastAssociationId();

            // Incrementar o associationId
            associationDTO.AssociationId = lastAssociationId + 1;

            string message = AssociationAmqpDTO.Serialize(associationDTO);
            _associationPendentAmqpGateway.Publish(message);

            return associationDTO;
        }

        return null;

    }

    private async Task<bool> VerifyAssociation(AssociationDTO associationDTO, List<string> errorMessages)
    {

        Association association = AssociationDTO.ToDomain(associationDTO);
        // Verifica se a associação já existe com base nos detalhes fornecidos
        bool aExists = await _associationRepository.AssociationExists(association);
        if (aExists)
        {
            errorMessages.Add("Association already exists.");
            return false;
        }

        // Verifica se o colaborador existe
        bool colabExists = await _colaboratorsRepository.ColaboratorExists(associationDTO.ColaboratorId);
        if (!colabExists)
        {
            errorMessages.Add("Colaborator doesn't exist.");
            return false;
        }

        // Verifica se o projeto existe
        bool projectExists = await _projectRepository.ProjectExists(associationDTO.ProjectId);
        if (!projectExists)
        {
            errorMessages.Add("Project doesn't exist.");
            return false;
        }

        // Verifica se as datas da associação correspondem ao projeto
        if (!await CheckDates(associationDTO))
        {
            errorMessages.Add("Association dates don't match with project.");
            return false;
        }
        if (await CheckDatesAssociation(associationDTO))
        {
            Console.WriteLine("Colaborator already has an association in this period.");
            errorMessages.Add("Colaborator already has an association in this period.");
            return false;
        }

        // Se todas as verificações passarem, retorna true
        return true;
    }

    private async Task<bool> CheckDates(AssociationDTO associationDTO)
    {
        Project p = await _projectRepository.GetProjectsByIdAsync(associationDTO.ProjectId);

        DateOnly startProject = p.StartDate;
        DateOnly? endProject = p.EndDate;

        DateOnly startAssociation = associationDTO.StartDate;
        DateOnly endAssociation = associationDTO.EndDate;

        if (endProject != null)
        {
            if (startAssociation >= startProject && endAssociation <= endProject)
            {
                return true;
            }
        }
        else if (startAssociation >= startProject)
        {
            return true;
        }


        return false;
    }

    private async Task<bool> CheckDatesAssociation(AssociationDTO associationDTO)
    {
        IEnumerable<Association> associations = await _associationRepository.GetAssociationsByColabIdInPeriodAsync(associationDTO.ColaboratorId, associationDTO.StartDate, associationDTO.EndDate);

        foreach (var association in associations)
        {
            if (((associationDTO.StartDate >= association.StartDate &&
                  associationDTO.StartDate <= association.EndDate) ||
                 (associationDTO.EndDate >= association.StartDate && associationDTO.EndDate <= association.EndDate))
                && association.ProjectId == associationDTO.ProjectId)
            {
                return true;
            }
        }

        return false;
    }
}
