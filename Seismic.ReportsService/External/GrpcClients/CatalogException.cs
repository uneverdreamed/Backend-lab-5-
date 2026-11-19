namespace Seismic.ReportsService.External.GrpcClients;

public class CatalogUnavailableException : Exception // каталог недоступен
{
    public CatalogUnavailableException(string message) : base(message) { }
}
public class CatalogTimeoutException : Exception // каталог не ответил за время
{
    public CatalogTimeoutException(string message) : base(message) { }
}
public class CatalogValidationException : Exception // каталог отклонил запрос как некорректный
{
    public CatalogValidationException(string message) : base(message) { }
}
public class CatalogFailureException : Exception // каталог ответил внутренней ошибкой (Internal и прочие сбои)
{
    public CatalogFailureException(string message) : base(message) { }
}
