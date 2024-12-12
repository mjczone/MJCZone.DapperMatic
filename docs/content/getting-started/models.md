# Models

- [DmCheckConstraint](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmCheckConstraint) ([src](src/MJCZone.DapperMatic/Models/DmCheckConstraint.cs))
- [DmColumn](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmColumn) ([src](src/MJCZone.DapperMatic/Models/DmColumn.cs))
- [DmColumnOrder](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmColumnOrder) ([src](src/MJCZone.DapperMatic/Models/DmColumnOrder.cs))
- [DmConstraint](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmConstraint) ([src](src/MJCZone.DapperMatic/Models/DmConstraint.cs))
- [DmConstraintType](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmConstraintType) ([src](src/MJCZone.DapperMatic/Models/DmConstraintType.cs))
- [DmDefaultConstraint](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmDefaultConstraint) ([src](src/MJCZone.DapperMatic/Models/DmDefaultConstraint.cs))
- [DmForeignKeyAction](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmForeignKeyAction) ([src](src/MJCZone.DapperMatic/Models/DmForeignKeyAction.cs))
- [DmForeignKeyConstraint](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmForeignKeyConstraint) ([src](src/MJCZone.DapperMatic/Models/DmForeignKeyConstraint.cs))
- [DmIndex](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmIndex) ([src](src/MJCZone.DapperMatic/Models/DmIndex.cs))
- [DmOrderedColumn](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmOrderedColumn) ([src](src/MJCZone.DapperMatic/Models/DmOrderedColumn.cs))
- [DmPrimaryKeyConstraint](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmPrimaryKeyConstraint) ([src](src/MJCZone.DapperMatic/Models/DmPrimaryKeyConstraint.cs))
- [DmTable](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmTable) ([src](src/MJCZone.DapperMatic/Models/DmTable.cs))
- [DmUniqueConstraint](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmUniqueConstraint) ([src](src/MJCZone.DapperMatic/Models/DmUniqueConstraint.cs))
- [DmView](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmView) ([src](src/MJCZone.DapperMatic/Models/DmView.cs))

## Model related factory methods

- [DmTableFactory](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmTableFactory) ([src](src/MJCZone.DapperMatic/Models/DmTableFactory.cs))

```csharp
DmTable table = DmTableFactory.GetTable(typeof(app_employees))
```

- [DmViewFactory](#/packages/MJCZone.DapperMatic/ns/MJCZone.DapperMatic.Models/t/DmViewFactory) ([src](src/MJCZone.DapperMatic/Models/DmViewFactory.cs))

```csharp
DmView view = DmViewFactory.GetView(typeof(vw_onboarded_employees))
```
