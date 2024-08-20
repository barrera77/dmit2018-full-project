using eBikeSystem.BLL.Purchasing;
using eBikeSystem.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eBikeSystem
{
    public static class eBikeBackEndExtensions
    {

        public static void eBikeBackEndDependencies(this IServiceCollection services,
            Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<eBikeContext>(options);

            services.AddTransient<PurchasingServices>((ServiceProvider) =>
            {
                var context = ServiceProvider.GetService<eBikeContext>();
                return new PurchasingServices(context!);
            });
        }           


    }
}
