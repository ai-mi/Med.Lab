using System.Collections.Generic;
using System.Threading.Tasks;

namespace Med.Labs.Application.Queries;

public interface IQueryRunner
{
	Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);
}