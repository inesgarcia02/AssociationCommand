using Domain.Model;
using Domain.IRepository;
using DataModel.Mapper;
using Microsoft.EntityFrameworkCore;
using DataModel.Model;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DataModel.Repository;

public class AssociationRepository : GenericRepository<Association>, IAssociationRepository
{

    AssociationMapper _associationMapper;

    public AssociationRepository(AbsanteeContext context, AssociationMapper mapper) : base(context!)
    {
        _associationMapper = mapper;
    }

    public async Task<IEnumerable<Association>> GetAssociationsAsync()
    {
        try
        {
            IEnumerable<AssociationDataModel> associationsDataModel = await _context.Set<AssociationDataModel>()
                    .ToListAsync();

            IEnumerable<Association> associations = _associationMapper.ToDomain(associationsDataModel);

            return associations;
        }
        catch
        {
            throw;
        }
    }

    public async Task<Association> GetAssociationsByIdAsync(long id)
    {
        try
        {
            AssociationDataModel associationDataModel = await _context.Set<AssociationDataModel>()
                .FirstAsync(a => a.Id == id);

            Association association = _associationMapper.ToDomain(associationDataModel);

            return association;
        }
        catch
        {
            return null;
        }
    }

    public async Task<Association> Add(Association association)
    {
        try
        {
            ProjectDataModel projectDataModel = await _context.Set<ProjectDataModel>()
                .FirstAsync(p => p.Id == association.ProjectId);

            ColaboratorsIdDataModel colaboratorDataModel = await _context.Set<ColaboratorsIdDataModel>()
                .FirstAsync(c => c.Id == association.ColaboratorId);

            AssociationDataModel associationDataModel = _associationMapper.ToDataModel(association, projectDataModel, colaboratorDataModel);

            EntityEntry<AssociationDataModel> associationDataModelEntityEntry = _context.Set<AssociationDataModel>().Add(associationDataModel);

            await _context.SaveChangesAsync();


            AssociationDataModel associationDataModelSaved = associationDataModelEntityEntry.Entity;

            Association associationSaved = _associationMapper.ToDomain(associationDataModelSaved);

            return associationSaved;
        }
        catch
        {
            throw;
        }
    }

    public async Task<IEnumerable<Association>> GetAssociationsByColabIdInPeriodAsync(long colabId, DateOnly startDate, DateOnly endDate)
    {
        IEnumerable<AssociationDataModel> associationDataModel = await _context.Set<AssociationDataModel>()
            .Where(a => a.ColaboratorId.Id == colabId && a.EndDate > startDate && a.StartDate < endDate)
            .Include(a => a.ColaboratorId)
            .Include(p => p.Project)
            .ToListAsync();

        IEnumerable<Association> associations = _associationMapper.ToDomain(associationDataModel);
        return associations;
    }

    public async Task<long> GetLastAssociationId()
    {
        // Buscar o último associationId na base de dados
        long lastAssociationId = await _context.Set<AssociationDataModel>()
            .OrderByDescending(a => a.AssociationId)
            .Select(a => a.AssociationId)
            .FirstOrDefaultAsync();

        return lastAssociationId;
    }

    public async Task<bool> AssociationExists(Association association)
    {
        return await _context.Set<AssociationDataModel>()
            .AnyAsync(a =>
            a.AssociationId == association.AssociationId &&
                a.ColaboratorId.Id == association.ColaboratorId &&
                a.Project.Id == association.ProjectId &&
                a.StartDate == association.StartDate &&
                a.EndDate == association.EndDate);
    }
}