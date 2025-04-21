public class PostgresService
{
    private readonly PostgresContext _postgresContext;

    public PostgresService(PostgresContext postgresContext)
    {
        _postgresContext = postgresContext;
    }

    public IEnumerable<PostgresUsers> GetAllPostgresUsers()
    {
        return _postgresContext.users;
    }

    public IEnumerable<PostgresProducts> GetAllPostgresProducts()
    {
        return _postgresContext.products;
    }

    public void DeletePostgresUser(int id)
    {
        var user = getPostgresUser(id);
        _postgresContext.users.Remove(user);
        _postgresContext.SaveChanges();
    }

    public void UpdatePostgresUser(int id, UpdatePostgresUserModel model)
    {
        var user = getPostgresUser(id);

        user.name = model.name;

        _postgresContext.users.Update(user);

        _postgresContext.SaveChanges();
    }

    private PostgresUsers getPostgresUser(int id)
    {
        var user = _postgresContext.users.Find(id);

        if (user == null) throw new System.Collections.Generic.KeyNotFoundException("User not found");

        return user;

    }
}
