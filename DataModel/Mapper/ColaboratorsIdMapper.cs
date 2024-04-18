using DataModel.Model;
using Domain.Model;

namespace DataModel.Mapper
{
    public class ColaboratorsIdMapper
    {

        public ColaboratorsIdMapper()
        {
        }

        public Colaborator ToDomain(ColaboratorsIdDataModel colaboratorsIdDM)
        {
            Colaborator colaborator = new Colaborator(colaboratorsIdDM.Id);

            return colaborator;
        }

        public IEnumerable<Colaborator> ToDomain(IEnumerable<ColaboratorsIdDataModel> colaboratorsIdDataModel)
        {

            List<Colaborator> colaboratorsIdDomain = new List<Colaborator>();

            foreach(ColaboratorsIdDataModel colaboratorIdDomain in colaboratorsIdDataModel)
            {
                Colaborator id = ToDomain(colaboratorIdDomain);

                colaboratorsIdDomain.Add(id);
            }

            return colaboratorsIdDomain.AsEnumerable();
        }

        public ColaboratorsIdDataModel ToDataModel(long colaboratorId)
        {
            ColaboratorsIdDataModel colaboratorsIdDataModel = new ColaboratorsIdDataModel(colaboratorId);

            return colaboratorsIdDataModel;
        }
    }
}