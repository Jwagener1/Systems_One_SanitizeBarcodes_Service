using System.Threading;
using System.Threading.Tasks;

namespace Systems_One_SanitizeBarcodes_Service;

public interface IDbQueryExecutor
{
    Task<int> ExecuteQueryFileAsync(string filePath, CancellationToken cancellationToken);
}
