public class PostgresUsers : BaseEntity
{
    public string name { get; set; }
}

public class PostgresProducts : BaseEntity
{
    public string name { get; set; }
    public int user_id { get; set; }
    public string user_name { get; set; }
}

public class UpdatePostgresUserModel
{
    public string name { get; set; }
}


