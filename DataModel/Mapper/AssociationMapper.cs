namespace DataModel.Mapper;

using DataModel.Model;
using Domain.Model;
using Domain.Factory;

public class AssociationMapper
{
    private IAssociationFactory _associationFactory;

    public AssociationMapper(IAssociationFactory associationFactory)
    {
        _associationFactory = associationFactory;
    }

    public Association ToDomain(AssociationDataModel associationDM)
    {
        Association associationDomain = _associationFactory.NewAssociation(associationDM.AssociationId ,associationDM.ColaboratorId.Id, associationDM.Project.Id, 
                                                    associationDM.StartDate,associationDM.EndDate, associationDM.Fundamental);
        associationDomain.Id = associationDM.Id;
        return associationDomain;
    }

    public IEnumerable<Association> ToDomain(IEnumerable<AssociationDataModel> associacoesDM)
    {
        List<Association> associationsDomain = new List<Association>();

        foreach (AssociationDataModel associationDM in associacoesDM)
        {
            Association associationDomain = ToDomain(associationDM);

            associationsDomain.Add(associationDomain);
        }
        return associationsDomain.AsEnumerable();
    }

    public AssociationDataModel ToDataModel(Association association, ProjectDataModel project, ColaboratorsIdDataModel colaborator)
    {
        AssociationDataModel associationDataModel = new AssociationDataModel(association, project, colaborator);

        return associationDataModel;
    }
}