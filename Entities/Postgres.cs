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

public class PostgresBooks : BaseEntity
{
    public string name { get; set; }
    public int price { get; set; }
    public string description { get; set; }
}

public class UpdatePostgresUserModel
{
    public string name { get; set; }
}


