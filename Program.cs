using ebooks_dotnet7_api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("ebooks"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var ebooks = app.MapGroup("api/ebook");

// TODO: Add more routes
ebooks.MapPost("/", CreateEBookAsync);
ebooks.MapGet("/", GetBooks);
ebooks.MapPut("/{id}", UpdateBook);
ebooks.MapPut("/{id}/change-availability", ChangeAvailability);
ebooks.MapPut("/{id}/increment-stock", IncrementStock);
ebooks.MapPost("/purchase", PurchaseBook);
ebooks.MapDelete("/{id}", DeleteBook);


app.Run();

// TODO: Add more methods
static async Task<IResult> CreateEBookAsync(DataContext db, [FromBody] AddEbookDto book)
{
    var ebook = await db.EBooks.FirstOrDefaultAsync(b => b.Author == book.Author && b.Title == book.Title);
    Console.WriteLine(book.Author);
    Console.WriteLine(book.Title);
    Console.WriteLine(ebook);

    if(ebook is not null)
    {
        return TypedResults.BadRequest("Ya existe un libro");
    }
    var addbook = new EBook
    {
        Title = book.Title,
        Author = book.Author,
        Genre = book.Genre,
        Format = book.Format,
        Price = book.Price,
        IsAvailable = true,
        Stock = 0
    };
    await db.EBooks.AddAsync(addbook);
    await db.SaveChangesAsync();

    return TypedResults.Ok("Se ha añadido exitosamente el libro");

}

static async Task<IResult> GetBooks(DataContext db, [FromQuery] string? genre, [FromQuery] string? author, [FromQuery] string? format)
{
    if(genre is null)
    {
        if(author is null)
        {
            if(format is null)
            {
                return TypedResults.Ok(await db.EBooks.ToArrayAsync());
            } else {
                return TypedResults.Ok(await db.EBooks.Where(b => b.Format == format).ToListAsync());
            }
        } else {
            if(format is null)
            {
                return TypedResults.Ok(await db.EBooks.Where(b => b.Author == author).ToListAsync());
            } else {
                return TypedResults.Ok(await db.EBooks.Where(b => b.Author == author && b.Format == format).ToListAsync());
            }
        }
    } else {
        if(author is null)
        {
            if(format is null)
            {
                return TypedResults.Ok(await db.EBooks.Where(b => b.Genre == genre).ToListAsync());
            } else {
                return TypedResults.Ok(await db.EBooks.Where(b => b.Genre == genre && b.Format == format).ToListAsync());
            }
        } else {
            if(format is null)
            {
                return TypedResults.Ok(await db.EBooks.Where(b => b.Genre == genre && b.Author == author).ToListAsync());
            } else {
                return TypedResults.Ok(await db.EBooks.Where(b => b.Genre == genre && b.Author == author && b.Format == format).ToListAsync());
            }
        }
    }
}


static async Task<IResult> UpdateBook (DataContext db,[FromQuery] int? id, [FromBody] AddEbookDto ebook)
{
    var book = await db.EBooks.FindAsync(id);
    if(book is null)
    {
        return TypedResults.BadRequest("La id no existe");
    }
    if(ebook.Author != null && ebook.Author != "")
    {
        book.Author = ebook.Author;
    }
    if(ebook.Format != null && ebook.Format != "")
    {
        book.Format = ebook.Format;
    }
    if(ebook.Genre != null && ebook.Genre != "")
    {
        book.Genre = ebook.Genre;
    }
    if(ebook.Price > 0)
    {
        book.Price = ebook.Price;
    }
    if(ebook.Title != null && ebook.Title != "")
    {
        book.Title = ebook.Title;
    }
    await db.SaveChangesAsync();

    return TypedResults.NoContent();

}

static async Task<IResult> ChangeAvailability(DataContext db, int id)
{
    var book = await db.EBooks.FindAsync(id);
    if(book is null)
    {
        return TypedResults.BadRequest("La id no existe");
    }
    book.IsAvailable = !book.IsAvailable;
    await db.SaveChangesAsync();
    
    return TypedResults.NoContent();
}

static async Task<IResult> IncrementStock(DataContext db, int id, [FromBody] UpdateStockDto stock)
{
    var book = await db.EBooks.FindAsync(id);
    if(book is null)
    {
        return TypedResults.NotFound();
    }
    book.Stock += stock.Stock;
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}

static async Task<IResult> PurchaseBook(DataContext db, [FromBody] PurchaseBookDto bookDto)
{
    var book = await db.EBooks.FindAsync(bookDto.Id);
        if(book is null)
    {
        return TypedResults.NotFound();
    }
    if(book.Stock < bookDto.Quantity)
    {
        return TypedResults.BadRequest("No hay stock");
    }
    if(book.Price * bookDto.Quantity != bookDto.Total)
    {
        return TypedResults.BadRequest("Error en el total");
    }
    book.Stock -= bookDto.Quantity;
    await db.SaveChangesAsync();

    return TypedResults.Ok("Se ha realizado :DDDd");
}

static async Task<IResult> DeleteBook(DataContext db, int id)
{
    var book = await db.EBooks.FindAsync(id);
    if(book is null)
    {
        return TypedResults.NotFound();
    }

    db.EBooks.Remove(book);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}