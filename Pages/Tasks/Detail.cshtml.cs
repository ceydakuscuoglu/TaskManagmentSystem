using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

public class TaskDetailModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TaskDetailModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public TaskDto? Task { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Request.Scheme + "://" + Request.Host);
        var task = await client.GetFromJsonAsync<TaskDto>($"/api/Task/detail/{id}");
        if (task == null) return RedirectToPage("/Index");
        Task = task;
        return Page();
    }

    public class TaskDto
    {
        public Guid ID { get; set; }
        public string? Task_Title { get; set; }
        public string? Description { get; set; }
        public string? TaskStatus { get; set; }
        public string? Department { get; set; }
        public string? AssignedTo { get; set; }
        public string? CreatedBy { get; set; }
    }
}


