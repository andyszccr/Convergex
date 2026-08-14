namespace Convergex.Application.DTOs.Currencies;

public static class CurrencyCatalog
{
    public static IReadOnlyList<CurrencySuggestionDto> Common { get; } =
    [
        new("USD", "Dólar estadounidense", "$"),
        new("CRC", "Colón costarricense", "₡"),
        new("EUR", "Euro", "€"),
        new("MXN", "Peso mexicano", "$"),
        new("GBP", "Libra esterlina", "£"),
        new("JPY", "Yen japonés", "¥"),
        new("CAD", "Dólar canadiense", "$"),
        new("AUD", "Dólar australiano", "$"),
        new("BRL", "Real brasileño", "R$"),
        new("CHF", "Franco suizo", "Fr"),
        new("CNY", "Yuan chino", "¥"),
        new("ARS", "Peso argentino", "$"),
        new("CLP", "Peso chileno", "$"),
        new("COP", "Peso colombiano", "$"),
        new("PEN", "Sol peruano", "S/"),
        new("GTQ", "Quetzal guatemalteco", "Q"),
        new("HNL", "Lempira hondureña", "L"),
        new("NIO", "Córdoba nicaragüense", "C$"),
        new("PAB", "Balboa panameña", "B/."),
        new("DOP", "Peso dominicano", "$"),
        new("KRW", "Won surcoreano", "₩"),
        new("INR", "Rupia india", "₹"),
        new("SEK", "Corona sueca", "kr"),
        new("NOK", "Corona noruega", "kr"),
        new("DKK", "Corona danesa", "kr"),
        new("NZD", "Dólar neozelandés", "$"),
        new("SGD", "Dólar de Singapur", "$"),
        new("HKD", "Dólar de Hong Kong", "$"),
        new("TRY", "Lira turca", "₺"),
        new("ZAR", "Rand sudafricano", "R")
    ];
}

public record CurrencySuggestionDto(string Code, string Name, string Symbol);
