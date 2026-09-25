using Rino.Model.NewContext.Entities;

namespace Rino.Dtos.Mapping
{
    public static class PrintEditionMap
    {
        public static PrintEdition Map(this PrintEditionDto model)
        {
            return new PrintEdition
            {
                Id = model.Id,
                Edition = model.Edition,
                NodeId = model.NodeId,
                Structure = model.Structure,
                Identifier = model.Identifier,
                MigrationDate = model.MigrationDate,
                CreationDate = model.CreationDate,
                LastModificationDate = model.LastModificationDate,
                IsPublished = model.IsPublished
            };
        }

        public static PrintEditionDto Map(this PrintEdition model)
        {
            return new PrintEditionDto
            {
                Id = model.Id,
                Edition = model.Edition,
                NodeId = model.NodeId,
                Structure = model.Structure,
                Identifier = model.Identifier,
                MigrationDate = model.MigrationDate,
                CreationDate = model.CreationDate,
                LastModificationDate = model.LastModificationDate,
                IsPublished = model.IsPublished
            };
        }


        public static PrintEdition Map(this PrintEdition entity, PrintEditionDto model)
        {
            entity.Id = model.Id;
            entity.Edition = model.Edition;
            entity.NodeId = model.NodeId;
            entity.Structure = model.Structure;
            entity.Identifier = model.Identifier;
            entity.MigrationDate = model.MigrationDate;
            entity.CreationDate = model.CreationDate;
            entity.LastModificationDate = model.LastModificationDate;
            entity.IsPublished = model.IsPublished;

            return entity;
        }
    }
}
