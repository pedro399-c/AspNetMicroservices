using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Newtonsoft.Json;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache _cache;

        public BasketRepository(IDistributedCache cache)
        {
            this._cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task DeleteBasket(string username)
        {
            await _cache.RemoveAsync(username);
        }

        public async Task<ShoppingCart> GetBasket(string username)
        {
            var basket = await this._cache.GetStringAsync(username);

            if (string.IsNullOrEmpty(basket))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<ShoppingCart>(basket);
        }

        public async Task<ShoppingCart> UpdateBasket(ShoppingCart basket)
        {
            await this._cache.SetStringAsync(basket.Username, JsonConvert.SerializeObject(basket));

            return await GetBasket(basket.Username);
        }
    }
}
