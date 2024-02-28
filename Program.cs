// imports all of the names of HoneyRaesAPI.Models
using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;

// importing the fully-qualified namespace of program data models; prior to the "using" directive, above
// List<HoneyRaesAPI.Models.Customer> customers = new List<HoneyRaesAPI.Models.Customer> {};
// List<HoneyRaesAPI.Models.Employee> employees = new List<HoneyRaesAPI.Models.Employee> {};
// List<HoneyRaesAPI.Models.ServiceTickets> serviceTickets = new List<HoneyRaesAPI.Models.ServiceTickets> {};


List<Customer> customers = new List<Customer>
{ 
    new Customer () {Id = 1, Name = "Robert", Address = "123 Street St"},
    new Customer () {Id = 2, Name = "Marley", Address = "321 Street St"},
    new Customer () {Id = 3, Name = "Richard", Address = "456 Street St"}
};
List<Employee> employees = new List<Employee>
{
    new Employee () {Id = 1, Name = "George", Specialty = "777 Street St"},
    new Employee () {Id = 2, Name = "Ronald", Specialty = "888 Street St"},
};
List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket () {Id = 1, CustomerId = 1, EmployeeId = 1, Description = "dirty laundry", Emergency = true, DateCompleted = new DateTime(2023, 8, 10)},
    new ServiceTicket () {Id = 2, CustomerId = 2, Description = "cat litter", Emergency = true},
    new ServiceTicket () {Id = 3, CustomerId = 3, EmployeeId = 1, Description = "need to get rid of a dead body", Emergency = false, DateCompleted = new DateTime(2023, 7, 29)},
    new ServiceTicket () {Id = 4, CustomerId = 1, EmployeeId = 2, Description = "worrying and foreboding machinations", Emergency = false},
    new ServiceTicket () {Id = 5, CustomerId = 2, Description = "drive me to school", Emergency = true},
};

//! All of the code is setting up the application to be ready to serve as a web API
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// endpoints

// GET - retrieves all service tickets & their properties
app.MapGet("/servicetickets", () =>
{
    return serviceTickets.Select(t => new ServiceTicketDTO
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        EmployeeId = t.EmployeeId,
        Description = t.Description,
        Emergency = t.Emergency,
        DateCompleted = t.DateCompleted
    });
});

// GET - retrieves service ticket by {id} - route parameter
app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    
    Employee employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);

    Customer customer = customers.FirstOrDefault(e => e.Id == serviceTicket.CustomerId);
    
    return Results.Ok(new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        EmployeeId = serviceTicket.EmployeeId,
        Employee = employee == null ? null : new EmployeeDTO
        {
            Id = employee.Id,
            Name = employee.Name,
            Specialty = employee.Specialty
        },
        Customer = customer == null ? null : new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency,
        DateCompleted = serviceTicket.DateCompleted
    });
});

// GET - retrieves all customers
app.MapGet("/customers", () =>
{
    return customers.Select(c => new CustomerDTO
    {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address
    });
});

// GET - retrieves customer by id
app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }

    List<ServiceTicket> tickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

// GET - retrieves all employees
app.MapGet("/employees", () =>
{
    return employees.Select(e => new EmployeeDTO
    {
        Id = e.Id,
        Name = e.Name,
        Specialty = e.Specialty
    });
});

// GET - retrieves employee by id
app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    List<ServiceTicket> tickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(new EmployeeDTO
    {
        Id = employee.Id,
        Name = employee.Name,
        Specialty = employee.Specialty,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

// Creates a service ticket -- POST
app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // Get the customer data to check that the customerid for the service ticket is valid
    Customer customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    // if the client did not provide a valid customer id, this is a bad request
    if (customer == null)
    {
        return Results.BadRequest();
    }

    // creates a new id (SQL will do this for us like JSON server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);

    // Created returns a 201 status code with a link in the headers to where the new resource can be accessed
    return Results.Created($"/servicetickets/{serviceTicket.Id}", new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency
    });
});

// Deletes a service ticket
app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (serviceTicket == null)
    {
        return Results.NotFound();
    }

    serviceTickets.RemoveAt(id - 1);
    return Results.NoContent();
});

//! This starts the app, and should always be at the bottom of this file.
app.Run();