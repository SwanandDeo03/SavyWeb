using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SavyWeb.Classes;

namespace SavyWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SavyDBController(IConfiguration configuration) : ControllerBase
    {
        private readonly string _connectionString = configuration.GetConnectionString("MySqlConn")
                ?? throw new InvalidOperationException("Connection string not found");

        [HttpPost]
        [Route("SavyDB")]
        public IActionResult SavyClass([FromBody] SavyDBClass data)
        {
            if (data == null)
                return BadRequest("No data provided.");
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"INSERT INTO assetimagedata 
                            (asset_img_name, asset_img_tag, aasset_img_url, deleted, rowcode, created_by, updated_by, ts_createt, ts_update, ts_delete)
                            VALUES (@name, @tag, @url, @deleted, @rowcode, @created, @updated, @createdAt, @updatedAt, @deletedAt)";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", data.asset_img_name);
                cmd.Parameters.AddWithValue("@tag", data.asset_img_tag);
                cmd.Parameters.AddWithValue("@url", data.asset_img_url);
                cmd.Parameters.AddWithValue("@deleted", data.deleted);
                cmd.Parameters.AddWithValue("@rowcode", data.rowcode);
                cmd.Parameters.AddWithValue("@created", data.created_by);
                cmd.Parameters.AddWithValue("@updated", data.updated_by);
                cmd.Parameters.AddWithValue("@createdAt", data.ts_createt);
                cmd.Parameters.AddWithValue("@updatedAt", data.ts_update);
                cmd.Parameters.AddWithValue("@deletedAt", data.ts_delete);

                

                return Ok("Inserted successfully");
            }
        }
    }
}
