using ProductRecommendations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineShop.Services
{
    public interface IRecommendationService
    {
        List<ScoreTest> GetProductRecommendations(string userId);
    }
}
