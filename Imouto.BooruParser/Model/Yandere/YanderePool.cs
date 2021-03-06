﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using Imouto.BooruParser.Model.Base;
using Microsoft.Extensions.Logging;

namespace Imouto.BooruParser.Model.Yandere
{
    public static class YanderePool
    {
        private static readonly ILogger Logger = LoggerAccessor.GetLogger(nameof(YanderePool));

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

            var poolRootDivNodes =
                docNode.SelectNodes(@"//*[@id='post-view']/div[@class='status-notice']")?
                       .Where(x => x.Attributes["id"]?.Value.Substring(0, 4) == "pool");

            if (poolRootDivNodes == null)
            {
                return resultCollection;
            }

            foreach (var divNode in poolRootDivNodes)
            {
                try
                {
                    var id = int.Parse(divNode.Attributes["id"].Value.Substring(4));
                    string name = null;
                    var aNodes = divNode.SelectNodes("div/p/a");
                    var poolNode = aNodes.LastOrDefault(x => x.Attributes["href"].Value.Substring(0, 5) == "/pool");
                    if (poolNode != null)
                    {
                        name = poolNode.InnerHtml;
                    }

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
