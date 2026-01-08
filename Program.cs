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

app.MapPost("/api/bet", (List<BetRequest> request) => 
{
    var context = GenerateBetContext();

    var responses = new List<BetResponse>();
    foreach(var bet in request)
    {
        var payment = CalculateBetPayout(bet, context);

        if(bet.BetType == BetType.COLUMN && (bet.Column < 1 || bet.Column > 3))
        {
           responses.Add(new BetResponse(
                IsWin: false,
                PayoutAmount: 0,
                WinningNumber: -1,
                Message: "Invalid column. Must be 1, 2 or 3."
            ));
            
            continue;
        }

        if(bet.BetAmount <= 0)
        {
            responses.Add(new BetResponse(
                IsWin: false,
                PayoutAmount: 0,
                WinningNumber: -1,
                Message: "Bet amount must be greater than zero."
            ));
            continue;
        }

        //Lógica de verde
        if(bet.BetType == BetType.GREEN && context.WinningNumber == 0)
        {
            responses.Add(new BetResponse(
                IsWin: true,
                PayoutAmount: bet.BetAmount * 35,
                WinningNumber: context.WinningNumber,
                Message: "Bet placed sucessfully."
            ));

            continue;
        }
        
        if(context.WinningNumber == 0)
        {
            responses.Add(new BetResponse(
                IsWin: false,
                PayoutAmount: 0,
                WinningNumber: context.WinningNumber,
                Message: "Bet placed sucessfully."
            ));
            continue;
        }

        responses.Add(new BetResponse(
            IsWin: payment > 0,
            PayoutAmount: payment,
            WinningNumber: context.WinningNumber,
            Message: $"Result: {context.WinningNumber} | Win: {payment > 0}"
        ));
    }

    return responses;
});

static decimal CalculateBetPayout(BetRequest request, RouletteContext context)
{
    decimal payment = 0m;

    switch(request.BetType)
    {
        case BetType.EVEN:
            if(context.isEven)
            {
                payment = request.BetAmount * 2;
            }
            break;
        case BetType.ODD:
            if(!context.isEven)
            {
                payment = request.BetAmount * 2;
            }
            break;
        case BetType.RED:
            if(context.isRed)
            {
                payment = request.BetAmount * 2;
            }
            break;
        case BetType.BLACK:
            if(!context.isRed)
            {
                payment = request.BetAmount * 2;
            }
            break;
        case BetType.COLUMN:
            if(request.Column == context.WinningColumn)
            {
                payment = request.BetAmount * 3;
            }
            break;
    }

    return payment;
}

static RouletteContext GenerateBetContext()
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
    var winningColumn = (winningNumber % 3 == 0 ? 3 : winningNumber % 3);

    return new RouletteContext
    (
        WinningNumber: winningNumber,
        isEven: isEven,
        isRed: isRed,
        WinningColumn: winningColumn
    );
};

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
};

enum BetType
{
    ODD, //0
    EVEN,
    BLACK,
    RED,
    GREEN,
    COLUMN //5
};

record BetRequest(
    BetType BetType,
    decimal BetAmount,
    int? Column
);

record RouletteContext(
    int WinningNumber,
    bool isEven,
    bool isRed,
    int WinningColumn
);

record BetResponse(
    bool IsWin,
    decimal PayoutAmount,
    int WinningNumber,
    string Message
);