iayos.DapperCRUD - helpers for Dapper
========================================
Features
--------
iayos.DapperCRUD is massively inspired by and extends upon [Dapper.SimpleCRUD](https://github.com/ericdc1/Dapper.SimpleCRUD/blob/master/Dapper.SimpleCRUD/SimpleCRUD.cs).

During a recent project where I was forced to use Dapper, I found myself writing a bunch of helper methods to pull out entities but would often refactor a property and get caught out
by the SimpleCRUD style of using anoymous objects for query parameters (which are not type safe and would crash when not detected during db entity property renames).

Additionally, I wanted to often 

- GetEntitiesWhereColumnEquals
- GetEntitiesWhereColumnIn
- GetEntitiesWhereColumnLike

and

- GetColumnXWhereColumnYEquals
- GetColumnXWhereColumnYIn
- GetColumnXWhereColumnYLike


# History:

Had to take the full SimpleCRUD because the GetTableName private method was causing me issues... may investigate further in future.

Example Usage:


```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}
      
var agesOfInterest = new List<int> { 18, 20, 22 };
var user = connection.GetWhereIn<User>(x => x.Age, agesOfInterest);   
```
Results in executing this SQL 
```sql
Select Id, Name, Age from [User] where Id in (18, 20, 22) 
```
