using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Npgsql;
using SavyWeb.Classes;


namespace SavyWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SavyDBController : ControllerBase
    {
        private readonly string _mysqlConnString;
        private readonly string _postgresConnString;
        private DateTime? parsedDateCreate;
        private DateTime? parsedDateUpdate;
        private DateTime? parsedDateDelete;

        public SavyDBController(IConfiguration configuration)
        {
            _mysqlConnString = configuration.GetConnectionString("MySqlConn")
                ?? throw new InvalidOperationException("MySQL connection string not found");
            _postgresConnString = configuration.GetConnectionString("PostgresConnection")
                ?? throw new InvalidOperationException("PostgreSQL connection string not found");
        }

        [HttpPost]
        [Route("SavyDBMySQL")]
        public IActionResult SavyClass([FromBody] SavyDBClass data)
        {
            if (data == null)
                return BadRequest("No data provided.");

            using var connection = new MySqlConnection(_mysqlConnString);
            connection.Open();
            var query = @"INSERT INTO assetimagedata 
                        (asset_img_name, asset_img_tag, asset_img_url, deleted, rowcode, created_by, updated_by, ts_create, ts_update, ts_delete)
                        VALUES (@name, @tag, @url, @deleted, @rowcode, @created, @updated, @createdAt, @updatedAt, @deletedAt)";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@name", data.asset_img_name);
            cmd.Parameters.AddWithValue("@tag", data.asset_img_tag);
            cmd.Parameters.AddWithValue("@url", data.asset_img_url);
            cmd.Parameters.AddWithValue("@deleted", data.deleted);
            cmd.Parameters.AddWithValue("@rowcode", data.rowcode);
            cmd.Parameters.AddWithValue("@created", data.created_by);
            cmd.Parameters.AddWithValue("@updated", data.updated_by);
            cmd.Parameters.AddWithValue("@createdAt", data.ts_create);
            cmd.Parameters.AddWithValue("@updatedAt", data.ts_update);
            cmd.Parameters.AddWithValue("@deletedAt", data.ts_delete);

            try
            {
                cmd.ExecuteNonQuery();
                return Ok("Data inserted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetAllMySql")]
        public IActionResult GetAllMySql()
        {
            var dataList = new List<SavyDBClass>(); // Simplified collection initialization

            using var connection = new MySqlConnection(_mysqlConnString);
            try
            {
                connection.Open();
                string query = "SELECT * FROM assetimagedata";
                using MySqlCommand cmd = new(query, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DateTime? tsCreate = null;
                    DateTime? tsUpdate = null;
                    DateTime? tsDelete = null;

                    if (reader["ts_create"] != DBNull.Value)
                    {
                        if (reader["ts_create"] is DateTime dt)
                            tsCreate = dt;
                        else
                            DateTime.TryParse(reader["ts_create"].ToString(), out DateTime parsedDateCreate);
                        tsCreate = parsedDateCreate;
                    }

                    if (reader["ts_update"] != DBNull.Value)
                    {
                        if (reader["ts_update"] is DateTime dt)
                            tsUpdate = dt;
                        else
                            DateTime.TryParse(reader["ts_update"].ToString(), out DateTime parsedDateUpdate);
                        tsUpdate = parsedDateUpdate;
                    }

                    if (reader["ts_delete"] != DBNull.Value)
                    {
                        if (reader["ts_delete"] is DateTime dt)
                            tsDelete = dt;
                        else
                            DateTime.TryParse(reader["ts_delete"].ToString(), out DateTime parsedDateDelete);
                        tsDelete = parsedDateDelete;
                    }

                    dataList.Add(new SavyDBClass
                    {
                        asset_img_name = reader["asset_img_name"]?.ToString() ?? string.Empty,
                        asset_img_tag = reader["asset_img_tag"]?.ToString() ?? string.Empty,
                        asset_img_url = reader["asset_img_url"]?.ToString() ?? string.Empty,
                        rowcode = reader["rowcode"]?.ToString() ?? string.Empty,
                        created_by = reader["created_by"]?.ToString() ?? string.Empty,
                        updated_by = reader["updated_by"]?.ToString() ?? string.Empty,
                        ts_create = tsCreate,
                        ts_update = tsUpdate,
                        ts_delete = tsDelete,
                    });
                }

                return Ok(dataList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
        }


        [HttpPost]
        [Route("SavyDBPostgres")]
        public IActionResult SavyClassPostgres([FromBody] SavyDBClass data)
        {
            if (data == null)
                return BadRequest("No data provided.");

            using var connection = new NpgsqlConnection(_postgresConnString);
            connection.Open();

            var query = @"INSERT INTO assetimagedata 
                (asset_img_name, asset_img_tag, asset_img_url, deleted, rowcode, created_by, updated_by, ts_create, ts_update, ts_delete)
                VALUES (@name, @tag, @url, @deleted, @rowcode, @created, @updated, @createdAt, @updatedAt, @deletedAt)";

            using var cmd = new NpgsqlCommand(query, connection);

            // Required field
            //cmd.Parameters.AddWithValue("@id", data.asset_img_id); // Removed '.Value' as 'asset_img_id' is already an int

            // asset_img_name is now optional
            cmd.Parameters.AddWithValue("@name", data.asset_img_name ?? (object)DBNull.Value);

            // Optional fields
            cmd.Parameters.AddWithValue("@tag", data.asset_img_tag ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@url", data.asset_img_url ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@deleted", data.deleted.HasValue ? (data.deleted.Value ? 1 : 0) : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@rowcode", data.rowcode ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@created", data.created_by ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@updated", data.updated_by ?? (object)DBNull.Value);

            // Convert DateTime? to Unix timestamps
            long? tsCreate = data.ts_create.HasValue
                ? new DateTimeOffset(data.ts_create.Value).ToUnixTimeSeconds()
                : null;

            long? tsUpdate = data.ts_update.HasValue
                ? new DateTimeOffset(data.ts_update.Value).ToUnixTimeSeconds()
                : null;

            long? tsDelete = data.ts_delete.HasValue
                ? new DateTimeOffset(data.ts_delete.Value).ToUnixTimeSeconds()
                : null;

            cmd.Parameters.AddWithValue("@createdAt", tsCreate.HasValue ? tsCreate.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@updatedAt", tsUpdate.HasValue ? tsUpdate.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@deletedAt", tsDelete.HasValue ? tsDelete.Value : (object)DBNull.Value);

            try
            {
                cmd.ExecuteNonQuery();
                return Ok("Data inserted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        [HttpGet]
        [Route("GetAllPostgres")]
        public IActionResult GetAllPostgres()
        {
            var dataList = new List<SavyDBClass>(); // Simplified collection initialization

            using var connection = new NpgsqlConnection(_postgresConnString);
            try
            {
                connection.Open();
                string query = "SELECT * FROM assetimagedata";
                using var cmd = new NpgsqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DateTime? tsCreate = null;
                    DateTime? tsUpdate = null;
                    DateTime? tsDelete = null;

                    if (reader["ts_create"] != DBNull.Value)
                        tsCreate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(reader["ts_create"])).DateTime;

                    if (reader["ts_update"] != DBNull.Value)
                        tsUpdate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(reader["ts_update"])).DateTime;

                    if (reader["ts_delete"] != DBNull.Value)
                        tsDelete = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(reader["ts_delete"])).DateTime;

                    dataList.Add(new SavyDBClass
                    {
                        asset_img_name = reader["asset_img_name"]?.ToString() ?? string.Empty,
                        asset_img_tag = reader["asset_img_tag"]?.ToString() ?? string.Empty,
                        asset_img_url = reader["asset_img_url"]?.ToString() ?? string.Empty,
                        rowcode = reader["rowcode"]?.ToString() ?? string.Empty,
                        created_by = reader["created_by"]?.ToString() ?? string.Empty,
                        updated_by = reader["updated_by"]?.ToString() ?? string.Empty,
                        ts_create = tsCreate,
                        ts_update = tsUpdate,
                        ts_delete = tsDelete,
                    });
                }

                return Ok(dataList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex.Message + ex.StackTrace);
            }
        }


        // Fix for CS1061: 'DateTime' does not contain a definition for 'HasValue'
        // The issue arises because 'DateTime' is a non-nullable value type and does not have a 'HasValue' property.
        // To fix this, the type of 'ts_create', 'ts_update', and 'ts_delete' in SavyDBClass should be changed to 'DateTime?' (nullable DateTime).


        [HttpPost]
        [Route("MigrateMySqlToPostgres")]
        public IActionResult MigrateMySqlToPostgres()
        {
            List<SavyDBClass> dataList = new();

            // Step 1: Read from MySQL
            using (var mysqlConnection = new MySqlConnection(_mysqlConnString))
            {
                try
                {
                    mysqlConnection.Open();
                    string query = "SELECT * FROM assetimagedata";
                    using var cmd = new MySqlCommand(query, mysqlConnection);
                    using var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        DateTime? tsCreate = null, tsUpdate = null, tsDelete = null;

                        if (reader["ts_create"] != DBNull.Value && DateTime.TryParse(reader["ts_create"].ToString(), out var parsedCreate))
                            tsCreate = parsedCreate;

                        if (reader["ts_update"] != DBNull.Value && DateTime.TryParse(reader["ts_update"].ToString(), out var parsedUpdate))
                            tsUpdate = parsedUpdate;

                        if (reader["ts_delete"] != DBNull.Value && DateTime.TryParse(reader["ts_delete"].ToString(), out var parsedDelete))
                            tsDelete = parsedDelete;

                        dataList.Add(new SavyDBClass
                        {
                            asset_img_name = reader["asset_img_name"]?.ToString() ?? string.Empty,
                            asset_img_tag = reader["asset_img_tag"]?.ToString() ?? string.Empty,
                            asset_img_url = reader["asset_img_url"]?.ToString() ?? string.Empty,
                            rowcode = reader["rowcode"]?.ToString() ?? string.Empty,
                            created_by = reader["created_by"]?.ToString() ?? string.Empty,
                            updated_by = reader["updated_by"]?.ToString() ?? string.Empty,
                            ts_create = tsCreate,
                            ts_update = tsUpdate,
                            ts_delete = tsDelete
                        });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error while reading from MySQL: {ex.Message}");
                }
            }

            if (dataList.Count == 0)
            {
                return NotFound("No data found in MySQL to migrate.");
            }
            int mysqlcount = dataList.Count;
            int pgsqlcount = 0;
            // Step 2: Write to PostgreSQL
            using (var postgresConnection = new NpgsqlConnection(_postgresConnString))
            {
                try
                {
                    postgresConnection.Open();

                    foreach (var data in dataList)
                    {
                        var insertQuery = @"
                    INSERT INTO assetimagedata 
                    (asset_img_name, asset_img_tag, asset_img_url, deleted, rowcode, created_by, updated_by, ts_create, ts_update, ts_delete)
                    VALUES (@name, @tag, @url, @deleted, @rowcode, @created, @updated, @createdAt, @updatedAt, @deletedAt)";

                        using var insertCmd = new NpgsqlCommand(insertQuery, postgresConnection);

                        insertCmd.Parameters.AddWithValue("@name", data.asset_img_name ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@tag", data.asset_img_tag ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@url", data.asset_img_url ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@deleted", data.deleted.HasValue ? (data.deleted.Value ? 1 : 0) : (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@rowcode", data.rowcode ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@created", data.created_by ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@updated", data.updated_by ?? (object)DBNull.Value);

                        // ✅ Convert to Unix timestamp only if date is valid
                        long? tsCreate = (data.ts_create.HasValue && data.ts_create.Value.Year >= 1970)
                            ? new DateTimeOffset(data.ts_create.Value).ToUnixTimeSeconds()
                            : null;

                        long? tsUpdate = (data.ts_update.HasValue && data.ts_update.Value.Year >= 1970)
                            ? new DateTimeOffset(data.ts_update.Value).ToUnixTimeSeconds()
                            : null;

                        long? tsDelete = (data.ts_delete.HasValue && data.ts_delete.Value.Year >= 1970)
                            ? new DateTimeOffset(data.ts_delete.Value).ToUnixTimeSeconds()
                            : null;

                        insertCmd.Parameters.AddWithValue("@createdAt", tsCreate ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@updatedAt", tsUpdate ?? (object)DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@deletedAt", tsDelete ?? (object)DBNull.Value);

                        insertCmd.ExecuteNonQuery();
                        pgsqlcount++;
                    }

                    return Ok(new
                    {
                        message = $"{dataList.Count} records migrated successfully from MySQL to PostgreSQL.",
                        mysqlCount = mysqlcount,
                        pgsqlCount = pgsqlcount
                    });

                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error while writing to PostgreSQL: {ex.Message}");
                }
            }
        }
    }
}

