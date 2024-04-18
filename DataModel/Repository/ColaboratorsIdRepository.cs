namespace DataModel.Repository;

using Microsoft.EntityFrameworkCore;

using DataModel.Model;
using DataModel.Mapper;

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Domain.IRepository;
using Domain.Model;

public class ColaboratorsIdRepository : GenericRepository<ColaboratorsIdRepository>, IColaboratorsIdRepository
{    
    ColaboratorsIdMapper _colaboratorsIdMapper;
    public ColaboratorsIdRepository(AbsanteeContext context, ColaboratorsIdMapper mapper) : base(context!)
    {
        _colaboratorsIdMapper = mapper;
    }

    public async Task<IEnumerable<Colaborator>> GetColaboratorsIdAsync()
    {
        try {
            IEnumerable<ColaboratorsIdDataModel> colaboratorsIdDataModel = await _context.Set<ColaboratorsIdDataModel>()
                    .ToListAsync();

            IEnumerable<Colaborator> colaboratorsId = _colaboratorsIdMapper.ToDomain(colaboratorsIdDataModel);

            return colaboratorsId;
        }
        catch
        {
            throw;
        }
    }


    public async Task<Colaborator> GetColaboratorByIdAsync(long id)
    {
        try
        {
            ColaboratorsIdDataModel colaboratorDataModel = await _context.Set<ColaboratorsIdDataModel>()
                .FirstAsync(c => c.Id == id);

            Colaborator colaborator = _colaboratorsIdMapper.ToDomain(colaboratorDataModel);

            return colaborator;
        }
        catch
        {
            return null;
        }
    }

    public async Task<Colaborator> Add(long Id)
    {
        try {
            ColaboratorsIdDataModel colaboratorsIdDataModel = _colaboratorsIdMapper.ToDataModel(Id);

            EntityEntry<ColaboratorsIdDataModel> colaboratorIdDataModelEntityEntry = _context.Set<ColaboratorsIdDataModel>().Add(colaboratorsIdDataModel);
            
            await _context.SaveChangesAsync();

            ColaboratorsIdDataModel colaboratorIdDataModelSaved = colaboratorIdDataModelEntityEntry.Entity;

            Colaborator colaboratorIdSaved = _colaboratorsIdMapper.ToDomain(colaboratorIdDataModelSaved);

            return colaboratorIdSaved;    
        }
        catch
        {
            throw;
        }
    }

    public async Task<bool> ColaboratorExists(long colabId)
    {
        return await _context.Set<ColaboratorsIdDataModel>().AnyAsync(c => c.Id == colabId);
    }
}