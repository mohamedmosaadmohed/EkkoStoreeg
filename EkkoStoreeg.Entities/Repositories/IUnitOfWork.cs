
namespace EkkoSoreeg.Entities.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICatagoryRepository Catagory { get; }
        IProductRepository Product { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IOrderDetailsRepository OrderDetails { get; }
		IApplicationUserRepository ApplicationUser { get; }
        IColorRepository Color { get; }
        ISizeRepository Size { get; }
		IRegionRepository Region { get; }
		ICityRepository City { get; }
		int Complete();
    }
}
