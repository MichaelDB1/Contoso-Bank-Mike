using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;

namespace Contoso_Bank_Mike
{
    public class StocksForMikesBank
    {
        public static async Task<string> GetStock(string theStock)
        {
            string strRet = string.Empty;
            double? dblTheStock = await StocksForMikesBank.GetStockPriceAsync(theStock);

            if (null == dblTheStock)   // might be a company name rather than a stock ticker name
            {
                string strTicker = await GetStockTickerName(theStock);
                if (string.Empty != strTicker)
                {
                    dblTheStock = await StocksForMikesBank.GetStockPriceAsync(strTicker);
                    theStock = strTicker;
                }
            }

            // return our reply to the user
            if (null == dblTheStock)
            {
                strRet = string.Format("Stock {0} doesn't appear to be valid", theStock.ToUpper());
            }
            else
            {
                strRet = string.Format("Stock: {0}, Value: {1}", theStock.ToUpper(), dblTheStock);
            }

            return strRet;
        }

        private static async Task<double?> GetStockPriceAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return null;

            string url = $"http://finance.yahoo.com/d/quotes.csv?s={symbol}&f=sl1";
            string csv;
            using (WebClient client = new WebClient())
            {
                csv = await client.DownloadStringTaskAsync(url).ConfigureAwait(false);
            }
            string line = csv.Split('\n')[0];
            string price = line.Split(',')[1];

            double result;
            if (double.TryParse(price, out result))
                return result;

            return null;
        }

        private static async Task<string> GetStockTickerName(string strCompanyName)
        {
            string strRet = string.Empty;
            string url = $"http://d.yimg.com/autoc.finance.yahoo.com/autoc?query={strCompanyName}&region=1&lang=en&callback=YAHOO.Finance.SymbolSuggest.ssCallback";
            string sJson = string.Empty;
            using (WebClient client = new WebClient())
            {
                sJson = await client.DownloadStringTaskAsync(url).ConfigureAwait(false);
            }

            sJson = StripJsonString(sJson);
            YhooCompanyLookup lookup = null;
            try
            {
                lookup = JsonConvert.DeserializeObject<YhooCompanyLookup>(sJson);
            }
            catch (Exception e)
            {

            }

            if (null != lookup)
            {
                foreach (lResult r in lookup.ResultSet.Result)
                {
                    if (r.exch == "NAS")
                    {
                        strRet = r.symbol;
                        break;
                    }
                }
            }

            return strRet;
        }

        // String retrurned from Yahoo Company name lookup contains more than raw JSON
        // strip off the front/back to get to raw JSON
        private static string StripJsonString(string sJson)
        {
            int iPos = sJson.IndexOf('(');
            if (-1 != iPos)
            {
                sJson = sJson.Substring(iPos + 1);
            }

            iPos = sJson.LastIndexOf(')');
            if (-1 != iPos)
            {
                sJson = sJson.Substring(0, iPos);
            }

            return sJson;
        }
    }

    public class lResult
    {
        public string symbol { get; set; }
        public string name { get; set; }
        public string exch { get; set; }
        public string type { get; set; }
        public string exchDisp { get; set; }
        public string typeDisp { get; set; }
    }

    public class ResultSet
    {
        public string Query { get; set; }
        public lResult[] Result { get; set; }
    }

    public class YhooCompanyLookup
    {
        public ResultSet ResultSet { get; set; }
    }


}
