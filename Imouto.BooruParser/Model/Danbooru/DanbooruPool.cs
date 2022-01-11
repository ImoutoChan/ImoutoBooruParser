using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;
using Microsoft.Extensions.Logging;

namespace Imouto.BooruParser.Model.Danbooru
{
    public static class DanbooruPool
    {
        private static readonly ILogger Logger = LoggerAccessor.GetLogger(nameof(DanbooruPool));

        private static readonly List<Pool> Pools = new List<Pool>();

        private static Pool CreateOrGetPool(int id, string name)
        {
            var pool = Pools.FirstOrDefault(x => x.Id == id);

            if (pool == null)
            {
                var newPool = new Pool(id, name);
                Pools.Add(newPool);
                return newPool;
            }
            return pool;
        }

        public static List<Pool> GetPools(HtmlNode docNode)
        {
            var resultCollection = new List<Pool>();

            var poolRootLiNodes = docNode.SelectNodes(@"//span[@class='pool-name']/a");

            if (poolRootLiNodes == null)
            {
                return resultCollection;
            }

            foreach (var liNode in poolRootLiNodes)
            {
                try
                {
                    var id = int.Parse(liNode.Attributes["href"].Value.Split('/').Last());
                    var name = liNode.InnerText.Substring(6);

                    resultCollection.Add(CreateOrGetPool(id, name));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in parsing pool:\n" + ex.Message);
                    Logger.LogError(ex, "Error in parsing pool");
                }
            }

            return resultCollection;
        }
    }
}
