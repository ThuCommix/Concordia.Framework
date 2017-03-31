﻿using System;
using System.Collections.Generic;
using ThuCommix.EntityFramework.Entities;

namespace ThuCommix.EntityFramework.Metadata
{
    public interface IEntityMetadataResolver
    {
        IEnumerable<EntityMetadata> EntityMetadata { get; }

        Type GetEntityType(EntityMetadata entityMetadata);

        EntityMetadata GetEntityMetadata(Entity entity);

        EntityMetadata GetEntityMetadata(Type entityType);
    }
}
