using MovieRecommendationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace MovieRecommendationSystem.Infrastructure
{
    public class RecommendationSystem<T>
    {    
        private IEnumerable<T> _itemRatings;
        private Dictionary<(int X, int Y), double> _itemSimilarityMatrix;
        private Func<T, int> _itemIdFunc, _userIdFunc;
        private Func<T, double> _ratingFunc;
        private Func<int, string> _itemNameFunc;

        public RecommendationSystem(Func<T, int> itemIdFunc, Func<T, int> userIdFunc, Func<T, double> ratingFunc, Func<int, string> itemNameFunc = null)
        {
            _itemIdFunc = itemIdFunc;
            _userIdFunc = userIdFunc;
            _ratingFunc = ratingFunc;
            _itemNameFunc = itemNameFunc;
        }

        public void LoadModel(IEnumerable<T> itemRatings)
        {
            _itemRatings = itemRatings;
            _itemSimilarityMatrix = new Dictionary<(int X, int Y), double>();
            var distinctItem = itemRatings.Select(_itemIdFunc).Distinct().ToList();
            int currentProgress = 0, totalCount = distinctItem.Count();
            Console.WriteLine($"Building model based off of {totalCount} items...");
            Parallel.ForEach(distinctItem, firstItemId =>
            {
                
                var itemName = _itemNameFunc == null ? firstItemId.ToString() : _itemNameFunc(firstItemId);
                Interlocked.Increment(ref currentProgress);

                //Console.WriteLine($" [{currentProgress}/{totalCount}] Finished work on item \"{itemName}\" in {elapsed.TotalMinutes} minute(s).");
                
                Console.WriteLine($" [{currentProgress}/{totalCount}]  Finished work on item \"{itemName}\"");
            });
            

        }
        public double PredictUserRating(int userId, int itemId)
        {
            if (_itemRatings == null || _itemSimilarityMatrix == null)
                throw new Exception("There is no data to generate recommendations from.");
            var existingRating = _itemRatings.FirstOrDefault(x => _itemIdFunc(x) == itemId && _userIdFunc(x) == userId);
            if (existingRating != null)
                return _ratingFunc(existingRating);

            var firstItemRatings = _itemRatings.Where(x => _itemIdFunc(x) == itemId);
            var firstItemAverageRating = firstItemRatings.Sum(_ratingFunc) / firstItemRatings.Count();
            
            var ratings = _itemRatings.Where(x => _userIdFunc(x) == userId);
            var distinctRatedItems = ratings.Select(_itemIdFunc).Distinct();

            IEnumerable<(double WeightedRating, double AbsoluteSimilarity)> itemData = distinctRatedItems.Select(secondItemId =>
            {
                var secondItemRatings = _itemRatings.Where(x => _itemIdFunc(x) == secondItemId);
                var secondItemAverageRating = secondItemRatings.Sum(_ratingFunc) / secondItemRatings.Count();
                return !_itemSimilarityMatrix.TryGetValue((itemId, secondItemId), out var similarity) ? (0, 0) :
                    ((_ratingFunc(ratings.First(x => _itemIdFunc(x) == secondItemId)) - secondItemAverageRating) * similarity, Math.Abs(similarity));

            });
            return firstItemAverageRating + itemData.Sum(x => x.WeightedRating) / itemData.Sum(x => x.AbsoluteSimilarity);
        }
        
    }
}
