using Grpc.Core;
using System.Threading.Tasks;
using TestRunner;

namespace TestRunner
{
    public class ProductServiceImpl : ProductService.ProductServiceBase
    {
        public override Task<ProductResponse> GetProduct(ProductRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ProductResponse
            {
                Id = request.Id,
                Name = "Test Product"
            });
        }
    }
}
