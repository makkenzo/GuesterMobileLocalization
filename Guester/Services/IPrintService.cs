using Guester.Models;

namespace Guester.Services
{
    public interface IPrintService
    {
        Task PrintHtml(string htmlContent, string jobNam);
    }

}
