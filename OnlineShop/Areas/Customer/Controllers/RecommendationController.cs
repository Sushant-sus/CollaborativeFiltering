using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML;
using OnlineShop.Data;
using OnlineShop.Helpers;
using OnlineShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data;
using Dapper;
using System.Threading.Tasks;
using ProductRecommendations;
using System.Globalization;
using System.Security.Claims;
using OnlineShop.Services;

namespace OnlineShop.Areas.Customer.Controllers
{
    //[Route("cart")]
    [Area("Customer")]
    public class RecommendationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IDbConnection _configuration;
        private readonly IRecommendationService _recommendationService;
        public static string dataLocation = "./Data";
        public static int bookPredictionId = 34941133;

        public RecommendationController(ApplicationDbContext db, IDbConnection configuration,IRecommendationService recommendationService)
        {
            _db = db;
           _configuration = configuration;
           _recommendationService = recommendationService;
        }

        [Route("recommendation")]
        public async Task<IActionResult> Index()
        
        {
            string usexr2Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recommendation = _recommendationService.GetProductRecommendations(usexr2Id);
            //var trainingDataPath = $"{dataLocation}/ratings.csv";
            var trainingDataPath = $"{dataLocation}/productsdatasetratingonline.csv";

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

            var prediction = model.Transform(testData);
            var metrics = context.Regression.Evaluate(prediction);

            Console.WriteLine($"Model metrics: RMS - {metrics.Rms} R^2 - {metrics.RSquared}" + Environment.NewLine);

            


        var productData = LoadProductData();
            int productid = 0;
            var score= new List<ScoreTest>();
            foreach(var item in productData)
            {
                productid = item.ProductId;

            var predictionFunc = model.MakePredictionFunction<ProductRating, ProductRatingPrediction>(context);
                 
                string usexrId = User.FindFirstValue(ClaimTypes.NameIdentifier);


                var productPrediction = predictionFunc.Predict(new ProductRating
            {
                user = float.Parse(usexrId), 
                productid = productid
            });
                score.Add(new ScoreTest {
                    Score = productPrediction.Score,
                    ProductId = productid,
                    Name = item.Name,
                    //Id = item.Id,
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

            //var datacheckforlist= score.AsQueryable().OrderByDescending(x => x.Score).Max(10);
            var datacheckforlist = score.AsQueryable()
       .OrderByDescending(x => x.Score)
       .Take(10)
       .Distinct()
       .ToList();

          
            return View(datacheckforlist);
        }

        private static IList<Product> LoadProductData()
        {
            var result = new List<Product>(); 
            var reader = $"{dataLocation}/productdetailsonlineshopproject.csv";

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
