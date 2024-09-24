var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/employees", () => new Employee().GetAllEmployees());
app.MapGet("/employees/search", (string q) => Employee.Search(q));
app.MapGet("/employees/{id:int}", (int id) => new Employee().GetOneEmployee(id));
app.MapPost("/employees", (Employee emp) => Employee.CreateOneEmployee(emp));

app.Run();


class Employee
{
    public int Id { get; set; }
    public String? FullName { get; set; }
    public Decimal Salary { get; set; }

    private static List<Employee> Employees = new List<Employee>()
    {
        new Employee() { Id = 1, FullName = "Ahmet Güneş", Salary=90_000},
        new Employee() { Id = 2, FullName = "Sıla Bulut", Salary=70_000},
        new Employee() { Id = 3, FullName = "Can Tan", Salary=95_000},
    };

    public List<Employee> GetAllEmployees() => Employees;
    public Employee? GetOneEmployee(int id) => Employees
                                                .SingleOrDefault(e => e.Id.Equals(id));

    public static Employee CreateOneEmployee(Employee employee)
    {
        Employees.Add(employee);
        return employee;
    }

    public static List<Employee>? Search(string q)
    {
        return Employees
            .Where(e => e.FullName != null &&
                   e.FullName.ToLower().Contains(q.ToLower()))
            .ToList();
    }

}