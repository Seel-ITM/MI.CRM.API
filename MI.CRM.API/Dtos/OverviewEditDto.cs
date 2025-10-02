using System.Text.Json;

namespace MI.CRM.API.Dtos
{
    public class OverviewEditDto
    {
        public int ProjectId { get; set; }
        public string Field {  get; set; }
        public JsonElement Value { get; set; }
    }
}
