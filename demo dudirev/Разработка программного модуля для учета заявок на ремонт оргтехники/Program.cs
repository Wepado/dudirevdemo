using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;


List<Order> repo = [
    new Order(1, new(2020,12,1),"eskovator", "sfx222","аа","Дудырев Д.В", "79014374771", "новая заявка"),
    ];

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();
app.UseCors(o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
string message = "";


app.MapGet("/orders", (int param = 0) =>
{
    string buffer = message;
    message = "";
    if (param != 0)
        return new { repo = repo.FindAll(x => x.Number == param), message = buffer };
    return new { repo, message = buffer };
});


app.MapGet("create", ([AsParameters] Order dto) => repo.Add(dto));

app.MapGet("update", ([AsParameters] OrderUpdateDTO dto) =>
{
    var o = repo.Find(x => x.Number == dto.Number);
    if (o == null)
        return;
    if (dto.Status != o.Status && dto.Status != "")
    {
        o.Status = dto.Status;
        message += $"Статус заявки номер {o.Number} изменён\n";
        if(o.Status == "готова к выдаче")
        {
            message += $"Заявка номер {o.Number} готова к выдаче";
            o.EndDate = DateOnly.FromDateTime(DateTime.Now);
        }
    }
    if (dto.ProblemDescription != "")
        o.ProblemDescription = dto.ProblemDescription;
    if (dto.Master != "")
        o.Master = dto.Master;
    if (dto.Comment != "")
        o.Comments.Add(dto.Comment);
});

int complete_count() => repo.FindAll(x => x.Status == "готова к выдаче").Count;

Dictionary<string, int> get_problem_type_stat() =>
    repo.GroupBy(x => x.ProblemDescription).Select(x => (x.Key, x.Count()))
    .ToDictionary(k => k.Key, v => v.Item2);

double get_average_time_to_complete() =>
    complete_count() == 0 ? 0 : repo.FindAll(x => x.Status == "готова к выдаче")
    .Select(x => x.EndDate.Value.DayNumber - x.StartDate.DayNumber)
    .Sum() / complete_count();

app.MapGet("/statistics", () => new
{
    complete_count = complete_count(),
    problem_type_stat = get_problem_type_stat(),
    average_time_to_complete = get_average_time_to_complete()

});

app.Run();



class Order (int number, DateOnly startDate, string texnick, string model, string problemDescription, string fioClient, string phoneNumber, string status)
{
    public int Number { get; set; } = number;
    public DateOnly StartDate { get; set; } = startDate;
    public DateOnly? EndDate { get; set; } = null;
    public string Texnick { get; set; } = texnick;
    public string Model { get; set; } = model;
    public string ProblemDescription { get; set; } = problemDescription;
    public string FioClient { get; set;} = fioClient;
    public string PhoneNumber { get; set; } = phoneNumber;
    public string Status { get; set; } = status;
    public string Master { get; set; } = "Не назначен";
    public List<string>? Comments { get; set; } = [];

}

record class OrderUpdateDTO
(int Number, string? 
Status = "", string? 
ProblemDescription = "", string? 
Master = "", string? 
Comment = "");