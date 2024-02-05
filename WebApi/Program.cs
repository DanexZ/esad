using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceReference1;
using System.ServiceModel;
using System.Xml.Linq;
using System.Net.NetworkInformation;
using WebApi;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/kancelarie-komornicze", async context => {
    
    var requestBody = await context.Request.ReadFromJsonAsync<EntitiesInput>();

    if (requestBody == null) {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Błędne dane wejściowe.");
        return;
    }


    EpuServiceClient client = new EpuServiceClient();

    var wynik = await client.ListaKancelariiKomoniczychAsync(
        requestBody.userName,
        requestBody.password,
        requestBody.apiKey,
        requestBody.numerStrony,
        requestBody.wielkoscStrony
    );


    context.Response.StatusCode = 200;
    await context.Response.WriteAsJsonAsync(wynik, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
});

app.MapPost("/moje-sprawy", async context =>
{

    var requestBody = await context.Request.ReadFromJsonAsync<EntitiesInput>();

    if (requestBody == null)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Błędne dane wejściowe.");
        return;
    }


    EpuServiceClient client = new EpuServiceClient();

    var wynik = client.MojeSprawyAsync(
        requestBody.userName,
        requestBody.password,
        requestBody.apiKey,
        requestBody.numerStrony,
        requestBody.wielkoscStrony,
        DateTime.Now.AddDays(-60),
        DateTime.Now,
        1,
        ""
    );

    Console.WriteLine(DateTime.Now.AddDays(-60));


    context.Response.StatusCode = 200;
    await context.Response.WriteAsJsonAsync(wynik, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
});

app.MapPost("/pozwy-zloz", async context =>
{
    try {

        var requestBody = await context.Request.ReadFromJsonAsync<CreateLawsuitsInput>();

        if (requestBody == null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Błędne dane wejściowe.");
            return;
        }


        Console.WriteLine("====================================");
        Console.WriteLine("====================================");

        EpuServiceClient client = new EpuServiceClient();
           
        var wynik = await client.ZlozPozwyAsync(
            requestBody.userName,
            requestBody.password,
            requestBody.apiKey,
            requestBody.listaPozwowXML
        );

        Console.WriteLine(wynik.oznaczeniePaczki);

        context.Response.StatusCode = 200;
        await context.Response.WriteAsJsonAsync(wynik, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });


    } catch (FaultException<ZlozPozwyOutputData> ex) {

        Console.WriteLine("Wiadomosc: " + ex.Message);
        Console.WriteLine("Kod wiadomosci: " + ex.Detail.kod);
        Console.WriteLine("Informacja: " + ex.Detail.informacja);
        Console.WriteLine("Opis : " + ex.Detail.opis);

        if (ex.Detail.kod == KodOdpowiedzi.ValidationError)
        {
            foreach (ZlozPozwyOutputElement element in
           ex.Detail.wynikiWalidacji)
            {
                Console.WriteLine("Kod walidacji: " +
               element.kodWalidacji);
                Console.WriteLine("Kod walidacji pozwu: " +
               element.kodWalidacjiPozwu);
                Console.WriteLine("Liczba porzadkowa pozwu: " + element.liczbaPorzadkowa);
                Console.WriteLine("Opis walidacji: " +
               element.opisWalidacji);
            }
        }
        Console.ReadKey();

        var errorResponse = new
        {
            Message = ex.Message,
            ErrorCode = ex.Detail.kod,
            Information = ex.Detail.informacja,
            Description = ex.Detail.opis,
            ValidationErrors = ex.Detail.kod == KodOdpowiedzi.ValidationError
                ? ex.Detail.wynikiWalidacji.Select(element => new
                {
                    ValidationCode = element.kodWalidacji,
                    LawsuitValidationCode = element.kodWalidacjiPozwu,
                    OrderNumber = element.liczbaPorzadkowa,
                    ValidationDescription = element.opisWalidacji
                })
                : null
        };

        context.Response.StatusCode = 500; // lub inny kod statusu błędu
        await context.Response.WriteAsJsonAsync(errorResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
    catch (Exception ex)
    {
        var errorResponse = new
        {
            Message = ex.Message
        };

        context.Response.StatusCode = 500; // lub inny kod statusu błędu
        await context.Response.WriteAsJsonAsync(errorResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
           
    


});

app.Run();


