using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML;
using ProductRecommendations;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using OnlineShop.Data;
using System.Data;
using System.Linq;

namespace OnlineShop.Services
{
    public class RecommendationService: IRecommendationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IDbConnection _configuration;
        public static string dataLocation = "./Data"; 

        public RecommendationService(ApplicationDbContext db, IDbConnection configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public List<ScoreTest> GetProductRecommendations(string userId)
        {
            var trainingDataPath = $"{dataLocation}/ProductRatings.csv";

            var context = new MLContext();
            var reader = context.Data.TextReader(new TextLoader.Arguments()
            {
                Separator = ",",
                HasHeader = true,
                Column = new[]
                {
            new TextLoader.Column("Label", DataKind.R4, 0),
            new TextLoader.Column("user", DataKind.R4, 1),
            new TextLoader.Column("productid", DataKind.R4, 2),
        }
            });

            IDataView data = reader.Read(trainingDataPath);

            var (trainData, testData) = context.BinaryClassification.TrainTestSplit(data, testFraction: 0.2);

            var pipeline = context.Transforms.Categorical.MapValueToKey("user", "userIdEncoded")
                .Append(context.Transforms.Categorical.MapValueToKey("productid", "productidEncoded"))
                .Append(new MatrixFactorizationTrainer(context, "Label", "userIdEncoded", "productidEncoded",
                    advancedSettings: s => { s.NumIterations = 20; s.K = 100; }));

            Console.WriteLine("Training recommender" + Environment.NewLine);
            var model = pipeline.Fit(trainData);

            var productData = LoadProductData();
            int productid = 0;
            var score = new List<ScoreTest>();
            foreach (var item in productData)
            {
                productid = item.ProductId;

                var predictionFunc = model.MakePredictionFunction<ProductRating, ProductRatingPrediction>(context);
                var userssId = userId.Split('-');
                float useeerid;
                if (userssId.Length > 1)
                {
                    var userqqId = Guid.Parse(userId);
                      useeerid = (float)userqqId.GetHashCode() / int.MaxValue;
                }
                else
                {
                    useeerid = float.Parse(userId.ToString());
                }

                  
                var productPrediction = predictionFunc.Predict(new ProductRating
                {
                    user = useeerid,
                    productid = productid
                });
                score.Add(new ScoreTest
                {
                    Score = productPrediction.Score,
                    ProductId = productid,
                    Name = item.Name,
                    Id = productid,
                    ProductTypeId = item.ProductTypeId,
                    Price = decimal.Parse(item.Price),
                    Image = item.Image,
                    SecondaryImage1 = item.SecondaryImage1,
                    SecondaryImage2 = item.SecondaryImage2,
                    SecondaryImage3 = item.SecondaryImage3
                    ,
                    Description = item.Description,
                    ProductColor = item.ProductColor,
                    IsAvailable = item.IsAvailable
                });
            }

            var datacheckforlist = score.AsQueryable()
                .OrderByDescending(x => x.Score)
                .Take(10)
                .Distinct()
                .ToList();

            return datacheckforlist;
        }

        private static IList<Product> LoadProductData()
        {
            var result = new List<Product>();
            var reader = $"{dataLocation}/productdetailsonlineshopproject.csv";
            //var reader = $"{dataLocation}/ProductDetails.csv";

            var isHeader = true;
            var line = String.Empty;

            using (var streamReader = new StreamReader(reader))
            {
                while (!streamReader.EndOfStream)
                {
                    if (isHeader)
                    {
                        line = streamReader.ReadLine();
                        isHeader = false;
                    }

                    line = streamReader.ReadLine();
                    var data = line.Split(',');

                    var product = new Product
                    {
                        ProductId = int.Parse(data[0].ToString()),
                        Name = data[1].ToString(),
                        Price = data[2].ToString(),
                        //Price = decimal.Parse(data[2], CultureInfo.InvariantCulture),
                        Image = data[3].ToString(),
                        SecondaryImage1 = data[9].ToString(),
                        SecondaryImage2 = data[10].ToString(),
                        SecondaryImage3 = data[11].ToString(),
                    };

                    // Make sure to include the following using statement at the top of your file:
                    // using System.Globalization;


                    result.Add(product);
                }
            }

            return result;
        }



    }
}
