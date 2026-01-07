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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/api/bet", (BetRequest request) => 
{
    var winningNumber = Random.Shared.Next(0, 37); 

    //Lógica de par/ímpar
    var isEven = winningNumber % 2 == 0;

    //Lógica de cores
    int[] redNumbers = new int[]
    {
        1,3,5,7,9,12,14,16,18,19,21,23,25,27,30,32,34,36
    };

    var isRed = false;

    if(redNumbers.Contains(winningNumber))
    {
        isRed = true;
    }

    //lógica de coluna
    var winningColumn = 3 - winningNumber % 3;

    var payment = 0m;

    //Lógica de verde
    if(request.BetType == BetType.GREEN && winningNumber == 0)
    {
        return new BetResponse(
            IsWin: true,
            PayoutAmount: request.BetAmount * 35,
            WinningNumber: winningNumber,
            Message: "Bet placed sucessfully."
        );
    } 
    
    if(winningNumber == 0)
    {
        return new BetResponse(
            IsWin: false,
            PayoutAmount: 0,
            WinningNumber: winningNumber,
            Message: "Bet placed sucessfully."
        );
    }

    if(request.BetType == BetType.EVEN && isEven || request.BetType == BetType.ODD && !isEven)
    {
        payment = request.BetAmount * 2;
    }

    if(request.BetType == BetType.RED && isRed || request.BetType == BetType.BLACK && !isRed)
    {
        payment = request.BetAmount * 2;
    }

    if(request.BetType == BetType.COLUMN && request.Column == winningColumn)
    {
        payment = request.BetAmount * 3;
    }

    return new BetResponse(
        IsWin: payment > 0,
        PayoutAmount: (decimal)payment,
        WinningNumber: winningNumber,
        Message: "isRed: " + isRed + " isEven: " + isEven + " winningColumn: " + winningColumn
    );
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

enum BetType
{
    ODD, //0
    EVEN,
    BLACK,
    RED,
    GREEN,
    COLUMN //5
}

record BetRequest(
    BetType BetType,
    decimal BetAmount,
    int? Column
);

record BetResponse(
    bool IsWin,
    decimal PayoutAmount,
    int WinningNumber,
    string Message
);